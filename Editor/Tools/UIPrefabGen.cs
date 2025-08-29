using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace UISystem.Editor
{
    public class UIPrefabGen : OdinEditorWindow
    {
        public enum UIPrefabType
        {
            Panel,
            Item,
        }
        [MenuItem("Tools/UIPrefabGen")]
        private static void OpenWindwo()
        {
            var window = GetWindow<UIPrefabGen>();
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 600);
        }

        [LabelText("UIPrefabName")]
        public string UIPrefabName;
        [EnumPaging]
        public UIPrefabType UIType;
        [FolderPath(ParentFolder = ("Assets/Share/Resources/UI"))]
        public string UIPrefabPath = string.Empty;
        [FolderPath(ParentFolder = ("Assets/Scripts/App/Modules"))]
        public string UIPrefabSpriptPath = string.Empty;

        [Button("GenUIPrefab")]
        private void GenUIPrefab()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogError("Unity is Playing");
                return;
            }

            Regex regex = new Regex(@"^[A-Z][a-zA-Z0-9]+$");
            if (!regex.IsMatch(UIPrefabName))
            {
                Debug.LogError("UIPrefabName is not available ! Please rename!!!");
                return;
            }


            Regex regex_1 = new Regex(@"^$|[a-zA-Z][a-zA-Z0-9]+$");
            if (!regex_1.IsMatch(UIPrefabPath) || !regex_1.IsMatch(UIPrefabSpriptPath))
            {
                Debug.LogError("UIPrefabPath or UIPrefabSpriptPath is not available ! Please rename!!!");
                return;
            }
            bool isSucces = false;
            if (UIType == UIPrefabType.Panel)
            {
                GameObject go_panel = new GameObject(UIPrefabName);
                var rt_1 = go_panel.AddComponent<RectTransform>();
                rt_1.anchorMin = Vector2.zero;
                rt_1.anchorMax = Vector2.one;
                rt_1.sizeDelta = Vector2.zero;

                GameObject go_root = new GameObject("UIRoot");
                var rt_2 = go_root.AddComponent<RectTransform>();
                rt_2.anchorMin = new Vector2(0.5f, 0.5f);
                rt_2.anchorMax = new Vector2(0.5f, 0.5f);
                rt_2.sizeDelta = Vector2.zero;
                rt_2.SetParent(rt_1);

                //go_panel.AddComponent<UIDetail>();

                var pr = go_panel.AddComponent<PrefabReference>();
                PrefabReferenceInspector prEditor = UnityEditor.Editor.CreateEditor(pr) as PrefabReferenceInspector;
                Type t = prEditor.GetType();
                MethodInfo method = t.GetMethod("GenPrefabReferenceClass", BindingFlags.NonPublic | BindingFlags.Instance);
                method.Invoke(prEditor, null);
                PrefabUtility.SaveAsPrefabAsset(go_panel, $"Assets/{UIPrefabPath}/{UIPrefabName}.prefab");
                isSucces = GenUIPrefabPanel_Class();
                AssetDatabase.Refresh();
                DestroyImmediate(go_panel);
            }
            else if (UIType == UIPrefabType.Item)
            {
                GameObject go_panel = new GameObject(UIPrefabName);
                var rt_1 = go_panel.AddComponent<RectTransform>();
                rt_1.anchorMin = new Vector2(0.5f, 0.5f);
                rt_1.anchorMax = new Vector2(0.5f, 0.5f);
                rt_1.sizeDelta = Vector2.zero;
                var pr = go_panel.AddComponent<PrefabReference>();
                PrefabReferenceInspector prEditor = UnityEditor.Editor.CreateEditor(pr) as PrefabReferenceInspector;
                Type t = prEditor.GetType();
                MethodInfo method = t.GetMethod("GenPrefabReferenceClass", BindingFlags.NonPublic | BindingFlags.Instance);
                method.Invoke(prEditor, null);
                PrefabUtility.SaveAsPrefabAsset(go_panel, $"Assets/{UIPrefabPath}/{UIPrefabName}.prefab");
                isSucces = GenUIPrefabItem_Class();
                AssetDatabase.Refresh();
                DestroyImmediate(go_panel);
            }
        }

        private bool GenUIPrefabPanel_Class()
        {
            var prefabRefName = $"{EditorUtility.ToTitleCase(UIPrefabName)}Ref";
            var cls = new CCClass
            {
                ClassName = UIPrefabName,
                DeclarationFormat = "public class {0} :  UIPanelViewRef<{1}>",
                Parameters = prefabRefName,
            };

            var method_OnAwake = new CCClassMethod
            {
                Declaration = $"public override void OnAwake()",
                Desc = "OnAwake"
            };
            cls.AddMethod(method_OnAwake);
            method_OnAwake.AppendBodyLine($"base.OnAwake();");

            var method_SetData = new CCClassMethod
            {
                Declaration = $"public override void SetData(params object[] args)",
                Desc = "SetData"
            };
            cls.AddMethod(method_SetData);
            method_SetData.AppendBodyLine($"base.SetData(args);");

            var method_OnClose = new CCClassMethod
            {
                Declaration = $"public override void OnClose()",
                Desc = "OnClose"
            };
            cls.AddMethod(method_OnClose);
            method_OnClose.AppendBodyLine($"base.OnClose();");

            var method_OnEscape = new CCClassMethod
            {
                Declaration = $"public override void OnEscape()",
                Desc = "OnEscape"
            };
            cls.AddMethod(method_OnEscape);
            method_OnEscape.AppendBodyLine($"base.OnEscape();");

            var file = new CCFile
            {
                Name = cls.ClassName,
                Author = "Code Generator",
                NameSpace = EditorProjectSettings.Get().UIProjectNameSpace,
                Path = $"Assets/Scripts/App/Modules/{UIPrefabSpriptPath}",
                IsHaveHeadNotes = false
            };
            file.AddUsingNameSpace("UnityEngine");
            file.AddUsingNameSpace("UISystem");
            file.AddClass(cls);
            file.WriteToFile();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return true;

        }
        private bool GenUIPrefabItem_Class()
        {
            var prefabRefName = $"{EditorUtility.ToTitleCase(UIPrefabName)}Ref";
            var cls = new CCClass
            {
                ClassName = UIPrefabName,
                DeclarationFormat = "public class {0} :  UIItemViewRef<{1}>",
                Parameters = prefabRefName,
            };
            var method_OnAwake = new CCClassMethod
            {
                Declaration = $"public override void Awake()",
                Desc = "Awake"
            };
            cls.AddMethod(method_OnAwake);
            method_OnAwake.AppendBodyLine($"base.Awake();");

            var file = new CCFile
            {
                Name = cls.ClassName,
                Author = "Code Generator",
                NameSpace = EditorProjectSettings.Get().UIProjectNameSpace,
                Path = $"Assets/Scripts/App/Modules/{UIPrefabSpriptPath}",
                IsHaveHeadNotes = false
            };
            file.AddUsingNameSpace("UnityEngine");
            file.AddUsingNameSpace("UISystem");
            file.AddClass(cls);
            file.WriteToFile();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return true;
        }
    }

}