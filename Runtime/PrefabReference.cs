using UnityEngine;
using System;
using System.Collections.Generic;

namespace UISystem
{
    /// <summary>
    ///Prefabricated reference
    /// </summary>
    public class PrefabReference : MonoBehaviour
    {
        /// <summary>
        ///Properties
        /// </summary>
        [Serializable]
        private class Property
        {
            /// <summary>
            ///Name
            /// </summary>
            public string Name = null;

            /// <summary>
            ///Value
            /// </summary>
            public UnityEngine.Object Value = null;

#if UNITY_EDITOR
            /// <summary>
            ///Type Name
            /// </summary>
            public string TypeName = null;

            /// <summary>
            ///Description (generate comments)
            /// </summary>
            public string Desc = null;
#endif
        }

        /// <summary>
        ///Attribute List
        /// </summary>
        [SerializeField]
        private Property[] mProperties;


        /// <summary>
        ///Attribute dictionary
        /// </summary>
        private Dictionary<string, Property> mCachedProperties;

        /// <summary>
        ///Initialize dictionary
        /// </summary>
        private void InitDictionary()
        {
            //Construction cache
            mCachedProperties = new Dictionary<string, Property>();
            for (int i = 0; i < mProperties.Length; ++i)
            {
                var p = mProperties[i];

#if UNITY_EDITOR
                //Check for duplication
                if (mCachedProperties.ContainsKey(p.Name))
                {
                    // Log.Error("Prefab property {0} repeat", p.Name);
                    continue;
                }
#endif
                mCachedProperties.Add(p.Name, p);
            }
        }

        /// <summary>
        ///Get Properties
        /// </summary>
        /// <param name="t"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public UnityEngine.Object TryGetProperty(Type t, string name)
        {
            return GetPropertyInternal(t, name, false);
        }

        /// <summary>
        ///Get Properties
        /// </summary>
        /// <param name="t"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public UnityEngine.Object GetProperty(Type t, string name)
        {
            return GetPropertyInternal(t, name, true);
        }

        /// <summary>
        ///Get Properties
        /// </summary>
        /// <param name="name"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetProperty<T>(string name) where T : UnityEngine.Object
        {
            var obj = GetPropertyInternal(typeof(T), name, true);
            return obj as T;
        }

        /// <summary>
        ///Get Properties
        /// </summary>
        /// <param name="t"></param>
        /// <param name="name"></param>
        /// <param name="logError"></param>
        /// <returns></returns>
        private UnityEngine.Object GetPropertyInternal(Type t, string name, bool logError)
        {
            //initialization
            if (null == mCachedProperties)
            {
                InitDictionary();
            }

            //Can't find
            if (!mCachedProperties.TryGetValue(name, out Property p))
            {
                if (logError)
                {
                    // Log.Error("Prefab {0} property {1} not exist", gameObject.name, name);
                }
                return null;
            }

#if UNITY_EDITOR
            //Inspection type
            if (p.Value.GetType() != t)
            {
                // Log.Error("Prefab {0} property {1} type {2} not match {3}", gameObject.name, name, p.Value.GetType(), t);
                return null;
            }
#endif
            return p.Value;
        }

        /// <summary>
        ///Traversal attribute
        /// </summary>
        /// <param name="action"></param>
        public void ForEachProperties(Action<string, UnityEngine.Object> action)
        {
            for (int i = 0; i < mProperties.Length; ++i)
            {
                var p = mProperties[i];
                action(p.Name, p.Value);
            }
        }
    }
}