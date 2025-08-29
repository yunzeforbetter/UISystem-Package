using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UISystem.Editor
{
    /// <summary>
    ///Prefabricated Reference Properties Panel
    /// </summary>
    [CustomEditor(typeof(PrefabReference))]
    public class PrefabReferenceInspector : UnityEditor.Editor
    {
        /// <summary>
        ///Type Selection List
        /// </summary>
        private struct TypeSelectList
        {
            /// <summary>
            ///Object List
            /// </summary>
            public UnityEngine.Object[] Objects;

            /// <summary>
            ///Type List
            /// </summary>
            public Type[] Types;

            /// <summary>
            ///Name List
            /// </summary>
            public string[] Names;
        }

        /// <summary>
        ///Edit Target
        /// </summary>
        private PrefabReference mTarget;

        /// <summary>
        ///Attribute List
        /// </summary>
        SerializedProperty mPropProperties;

        /// <summary>
        /// OnEnable
        /// </summary>
        private void OnEnable()
        {
            mTarget = target as PrefabReference;
            mPropProperties = serializedObject?.FindProperty("mProperties");
        }

        /// <summary>
        ///Whether to include the specified object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool ContainsProperty(UnityEngine.Object obj)
        {
            for (int i = 0; i < mPropProperties.arraySize; ++i)
            {
                var p = mPropProperties.GetArrayElementAtIndex(i);
                var pValue = p.FindPropertyRelative("Value");
                if (pValue.objectReferenceValue == obj)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///Add Attribute
        /// </summary>
        /// <param name="name"></param>
        /// <param name="val"></param>
        private void AddProperty(string name, UnityEngine.Object val)
        {
            //Don't add yourself
            if (val is PrefabReference)
            {
                return;
            }

            //Do not add repeatedly
            if (ContainsProperty(val))
            {
                return;
            }

            //add to
            int index = mPropProperties.arraySize;
            mPropProperties.InsertArrayElementAtIndex(index);

            //initialization
            var p = mPropProperties.GetArrayElementAtIndex(index);
            p.FindPropertyRelative("Name").stringValue = name;
            p.FindPropertyRelative("Value").objectReferenceValue = val;
            p.FindPropertyRelative("TypeName").stringValue = val.GetType().Name;
        }

        /// <summary>
        ///Temporary list
        /// </summary>
        private static readonly List<Type> sTempTypeList = new List<Type>();
        private static readonly List<string> sTempStrList = new List<string>();
        private static readonly List<UnityEngine.Object> sTempObjectList = new List<UnityEngine.Object>();

        /// <summary>
        ///Generate Type Selection List
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static TypeSelectList BuildSelectTypeList(UnityEngine.Object obj)
        {
            // GameObject
            GameObject go = obj as GameObject;
            if (null == go)
            {
                var c = obj as Component;
                if (null == c)
                {
                    // Log.Error("Invalid PropertyType {0}({1})", obj.name, obj.GetType());
                    return default;
                }
                go = (obj as Component).gameObject;
            }

            //Join yourself
            sTempObjectList.Clear();
            sTempObjectList.Add(go);

            //Traversal component
            var components = go.GetComponents<Component>();
            for (int i = 0; i < components.Length; ++i)
            {
                var c = components[i];
                if (null == c)
                {
                    continue;
                }
                if (c is PrefabReference)
                {
                    continue;
                }
                sTempObjectList.Add(c);
            }

            //Generate a list of types and names
            sTempTypeList.Clear();
            sTempStrList.Clear();
            for (int i = 0, ci = sTempObjectList.Count; i < ci; ++i)
            {
                var c = sTempObjectList[i];
                sTempTypeList.Add(c.GetType());
                sTempStrList.Add(c.GetType().Name);
            }

            //return
            return new TypeSelectList
            {
                Objects = sTempObjectList.ToArray(),
                Types = sTempTypeList.ToArray(),
                Names = sTempStrList.ToArray()
            };
        }

        /// <summary>
        /// OnInspectorGUI
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            if (GetCurrentScript() is {} script)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.ObjectField("Script", script, script.GetType(), false);
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.Space();
            }

            for (int i = 0; i < mPropProperties.arraySize; ++i)
            {
                GUILayout.BeginHorizontal();
                var p = mPropProperties.GetArrayElementAtIndex(i);

                //Member Properties
                var pName = p.FindPropertyRelative("Name");
                var pValue = p.FindPropertyRelative("Value");
                var pTypeName = p.FindPropertyRelative("TypeName");
                var pDesc = p.FindPropertyRelative("Desc");
                pName.stringValue = EditorGUILayout.TextField(pName.stringValue, GUILayout.Width(150));
                pValue.objectReferenceValue = EditorGUILayout.ObjectField(pValue.objectReferenceValue, typeof(UnityEngine.Object), true);

                //Type selection
                if (pValue.objectReferenceValue != null)
                {
                    var list = BuildSelectTypeList(pValue.objectReferenceValue);
                    if (null != list.Names)
                    {
                        var index = Array.IndexOf(list.Names, pTypeName.stringValue);
                        if (index < 0)
                        {
                            index = 0;
                        }
                        index = EditorGUILayout.Popup(index, list.Names);
                        pTypeName.stringValue = list.Names[index];
                        pValue.objectReferenceValue = list.Objects[index];
                    }
                }
                pDesc.stringValue = EditorGUILayout.TextField(pDesc.stringValue, GUILayout.Width(100));

                //delete
                if (GUILayout.Button("-"))
                {
                    mPropProperties.DeleteArrayElementAtIndex(i--);
                }
                GUILayout.EndHorizontal();
            }

            //Drag area
            GUILayout.BeginVertical();
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Drag Here");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            GUILayout.EndVertical();

            //Drag
            var evtType = UnityEngine.Event.current.type;
            if (evtType == EventType.DragUpdated || evtType == EventType.DragPerform)
            {
                // Show a copy icon on the drag
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (evtType == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    foreach (var o in DragAndDrop.objectReferences)
                    {
                        if (!(o is GameObject) && !(o is Component))
                        {
                            continue;
                        }
                        AddProperty(o.name, o);
                    }
                }
                UnityEngine.Event.current.Use();
            }

            //Generate Code
            if (GUILayout.Button("Generate Reference Class"))
            {
                GenPrefabReferenceClass();
            }

            //Apply Changes
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        ///Check prefabrication information
        /// </summary>
        /// <returns></returns>
        private bool CheckPrefabInfo()
        {
            var setObjects = new HashSet<UnityEngine.Object>();
            var setNames = new HashSet<string>();
            var prefabName = mTarget.gameObject.name;
            for (int i = 0; i < mPropProperties.arraySize; ++i)
            {
                var p = mPropProperties.GetArrayElementAtIndex(i);
                var name = p.FindPropertyRelative("Name").stringValue;
                var value = p.FindPropertyRelative("Value").objectReferenceValue;
                var typeName = p.FindPropertyRelative("TypeName").stringValue;
                var desc = p.FindPropertyRelative("Desc").stringValue;

                //Object duplication
                if (setObjects.Contains(value))
                {
                    Debug.LogError($"<{prefabName}> property <{name}> value repeat");
                    return false;
                }

                //Duplicate name
                if (setNames.Contains(name))
                {
                    Debug.LogError($"<{prefabName}> property <{name}> name repeat");
                    return false;
                }
                setObjects.Add(value);
                setNames.Add(name);
            }
            return true;
        }

        /// <summary>
        ///Generate Prefabricated Reference Class
        /// </summary>
        private bool GenPrefabReferenceClass()
        {
            //inspect
            if (!CheckPrefabInfo())
            {
                return false;
            }
            var prefabName = EditorUtility.ToTitleCase(mTarget.gameObject.name);
            var className = $"{prefabName}Ref";

            //class
            var cls = new CCClass
            {
                ClassName = className,
                DeclarationFormat = "public class {0} : IPrefabRef"
            };

            // PrefabReference
            var cRef = mTarget.GetType().Name;
            var pRef = new CCClassProperty
            {
                Declaration = $"public {cRef} Prefab {{ get; private set; }}",
                Desc = cRef
            };
            cls.AddProperty(pRef);

            //Binding function
            var method = new CCClassMethod
            {
                Declaration = $"public void Bind(PrefabReference rc)",
                Desc = "Bind GameObject"
            };
            cls.AddMethod(method);
            method.AppendBodyLine($"Prefab = rc;");

            //traversal attributes 
            for (int i = 0; i < mPropProperties.arraySize; ++i)
            {
                //Member Properties
                var p = mPropProperties.GetArrayElementAtIndex(i);
                var name = p.FindPropertyRelative("Name").stringValue;
                var typeName = p.FindPropertyRelative("TypeName").stringValue;
                var desc = p.FindPropertyRelative("Desc").stringValue;

                //The first name is empty
                if (string.IsNullOrEmpty(name))
                {
                    // Log.Error("Null property name in prefab {0}", mTarget.gameObject.name);
                    continue;
                }

                //Type is empty
                if (string.IsNullOrEmpty(typeName))
                {
                    // Log.Error("Null type name of property {0} in prefab {1}", name, mTarget.gameObject.name);
                    continue;
                }

                //statement
                var tcname = EditorUtility.ToTitleCase(name);
                var cp = new CCClassProperty
                {
                    Declaration = $"public {typeName} {tcname} {{ get; private set; }}",
                    Desc = string.IsNullOrEmpty(desc) ? tcname : desc
                };
                cls.AddProperty(cp);

                //initialization
                method.AppendBodyLine($"{tcname} = rc.GetProperty(typeof({typeName}), \"{name}\") as {typeName};");
            }

            //write file
            var file = new CCFile
            {
                Name = cls.ClassName,
                Author = "Code Generator",
                NameSpace = EditorProjectSettings.Get().HotfixGameNamespace,
                Path = EditorProjectSettings.Get().PrefabReferencePath
            };
            file.AddUsingNameSpace("UnityEngine");
            file.AddUsingNameSpace("UnityEngine.UI");
            file.AddUsingNameSpace("TMPro");
            //file.AddUsingNameSpace("Kyub.EmojiSearch.UI");
            file.AddUsingNameSpace("UISystem");
            file.AddClass(cls);
            file.WriteToFile();

            //Save it
            serializedObject.ApplyModifiedProperties();
            UnityEditor.EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
            return true;
        }

        private MonoScript GetCurrentScript()
        {
            var paths = new[]
            {
                "Assets/Scripts/HotUpdate/Application/Module/",
                "Assets/Scripts/HotUpdate/UI/"
            };
            paths = AssetDatabase.FindAssets($"t:{nameof(MonoScript)} {mTarget.name}", paths);
            paths = Array.ConvertAll(paths, AssetDatabase.GUIDToAssetPath);
            if (paths.FirstOrDefault(path => !path.EndsWith("Ref.cs")) is { } scriptPath)
            {
                return AssetDatabase.LoadAssetAtPath<MonoScript>(scriptPath);
            }
            return null;
        }
    }
}