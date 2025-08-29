#if UNITY_EDITOR
using UnityEngine;

namespace UISystem.Editor
{
    /// <summary>
    ///Editor Project Settings
    /// </summary>
    public class EditorProjectSettings : ScriptableObject
    {
        /// <summary>
        ///Resource Path
        /// </summary>
        private static readonly string AssetPath = "Assets/Editor/Settings/EditorProjectSettings.asset";

        /// <summary>
        ///Global Instance
        /// </summary>
        /// <returns></returns>
        private static EditorProjectSettings sInst;
        public static EditorProjectSettings Get()
        {
            if (null == sInst)
            {
                sInst = EditorHelper.LoadOrCreateAsset<EditorProjectSettings>(AssetPath);
            }
            return sInst;
        }

        /// <summary>
        ///Prefabricated Reference Code Generation Directory
        /// </summary>
        public string PrefabReferencePath = "Assets/Scripts/App/Ui/Generated/PrefabReference";

        ///Game Namespace
        ///Code generation requires
        /// </summary>
        public string NativeGameNamespace = "Game";
        public string HotfixGameNamespace = "UISystem";

        /// <summary>
        ///Name of hot change project
        /// </summary>
        public string HotfixProjectName = "Hotfix";

        /// <summary>
        ///Resource root directory to be packaged
        /// </summary>
        public string ResRootPath = "Assets/res";


        /// <summary>
        ///Absolute path of hot change engineering
        /// </summary>
        // public string HotfixPorjectAbsPath { get { return $"{Application.dataPath}/../../{HotfixProjectName}"; } }
        public string HotfixPorjectAbsPath { get { return $"{ResRootPath}/../{HotfixProjectName}"; } }

        /// <summary>
        ///Hot Change Output Directory
        /// </summary>
        // public string HotfixBinPath { get { return $"{HotfixPorjectAbsPath}/Bin"; } }
        public string HotfixBinPath { get { return $"{HotfixPorjectAbsPath}/../../Temp/Bin"; } }

        /// <summary>
        ///Hotchange DLL absolute path
        /// </summary>
        // public string HotfixDllDebugPath { get { return $"{HotfixBinPath}/Debug/ils.dll"; } }
        // public string HotfixPdbDebugPath { get { return $"{HotfixBinPath}/Debug/ils.pdb"; } }
        // public string HotfixDllReleasePath { get { return $"{HotfixBinPath}/Release/ils.dll"; } }
        // public string HotfixPdbReleasePath { get { return $"{HotfixBinPath}/Release/ils.pdb"; } }
        public string HotfixDllDebugPath { get { return $"{ResRootPath}/{HotfixProjectName}.dll.bytes"; } }
        public string HotfixPdbDebugPath { get { return $"{ResRootPath}/{HotfixProjectName}.pdb.bytes"; } }
        public string HotfixDllReleasePath { get { return $"{ResRootPath}/{HotfixProjectName}.dll.bytes"; } }
        public string HotfixPdbReleasePath { get { return $"{ResRootPath}/{HotfixProjectName}.pdb.bytes"; } }

        /// <summary>
        ///Get DLL Path
        /// </summary>
        /// <param name="debug"></param>
        /// <returns></returns>
        public string GetHotfixDllPath(bool debug) { return debug ? HotfixDllDebugPath : HotfixDllReleasePath; }
        public string GetHotfixPdbPath(bool debug) { return debug ? HotfixPdbDebugPath : HotfixPdbReleasePath; }

        /// <summary>
        ///Package Name
        /// </summary>
        public string PackageName = "com.framework.common";

        public string UIProjectNameSpace = "UISystem";
    }
}
#endif