#if UNITY_EDITOR
using UnityEngine;

namespace UISystem.Editor
{
    /// <summary>
    ///Editor Debug Settings
    /// </summary>
    public class EditorDebugSettings : ScriptableObject
    {
        /// <summary>
        ///Resource Path
        /// </summary>
        private static readonly string AssetPath = "Assets/Editor/Settings/EditorDebugSettings.asset";

        /// <summary>
        ///Global Instance
        /// </summary>
        /// <returns></returns>
        private static EditorDebugSettings sInst;
        public static EditorDebugSettings Get()
        {
            if (null == sInst)
            {
                sInst = EditorHelper.LoadOrCreateAsset<EditorDebugSettings>(AssetPath);
            }
            return sInst;
        }

        /// <summary>
        ///Allow hot change
        /// </summary>
        public bool EnableHotUpdate;

        /// <summary>
        ///Local simulation test version information for hot change
        /// </summary>
        public bool UseLocalVersionData;

        /// <summary>
        ///Server address
        /// </summary>
        public string[] ServerUrls;

        // /// <summary>
        /////Application Configuration
        // /// </summary>
        // public AppConfig AppConfig;

        /// <summary>
        ///Use built resources
        /// </summary>
        public bool UseBuildRes;

        /// <summary>
        ///Use the built DLL
        /// </summary>
        public bool UseBuildDll;
    }
}
#endif