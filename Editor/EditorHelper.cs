#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UISystem.Editor
{
    /// <summary>
    ///Editor auxiliary class
    /// </summary>
    public static class EditorHelper
    {
        /// <summary>
        ///Temporary list
        /// </summary>
        private static readonly List<string> sTempList = new List<string>();

        /// <summary>
        ///Gets the path of the specification.
        /// </summary>
        ///<param name="path">The path to be normalized</param>
        ///<returns>The path of the specification</returns>
        public static string GetRegularPath(string path)
        {
            if (path == null)
            {
                return null;
            }

            path = path.Replace('\\', '/');
            path = path.Replace("//", "/");

            //Remove//
            var sidx = 0;
            var idx = path.IndexOf("/..", sidx);
            while (idx > 0)
            {
                var idx0 = path.LastIndexOf('/', idx - 1, idx - sidx);
                sidx = idx + 1;
                if (idx0 >= 0)
                {
                    var folder = path.Substring(idx0 + 1, idx - idx0 - 1);
                    if (folder != "..")
                    {
                        path = path.Remove(idx0, idx - idx0 + 3);
                        sidx = 0;
                    }
                }
                idx = path.IndexOf("/..", sidx);
            }
            return path;
        }

        /// <summary>
        ///Get relative directory
        /// </summary>
        /// <param name="file"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetRelativePath(string file, string path)
        {
            var regularFile = GetRegularPath(file);
            var regularPath = GetRegularPath(path);
            if (!regularFile.StartsWith(regularPath))
            {
                // Log.Error($"Convert RelativePath {file} -> {path} failed");
                return null;
            }
            var relativePath = regularFile.Replace(regularPath, "");

            //Remove the first/
            if (relativePath.StartsWith("/"))
            {
                relativePath = relativePath.Substring(1);
            }
            return relativePath;
        }

        /// <summary>
        ///Get directory name
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetDirectoryName(string path)
        {
            //The directory name returned by Path.GetDirectoryName is
            var dir = Path.GetDirectoryName(path);
            return dir.Replace('\\', '/');
        }

        /// <summary>
        ///Get path without extension
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetPathWithoutExt(string path)
        {
            path = GetRegularPath(path);
            var ext = Path.GetExtension(path);
            if (string.IsNullOrEmpty(ext))
            {
                return path;
            }
            var idx = path.LastIndexOf(ext);
            return path.Remove(idx);
        }

        /// <summary>
        ///Create Resources
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="path"></param>
        public static bool CreateAsset(UnityEngine.Object asset, string path)
        {
            if (null == asset)
            {
                // Log.Error("CreateAsset: null asset");
                return false;
            }
            var dir = GetDirectoryName(path);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            return true;
        }

        /// <summary>
        ///Load or create resources
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        public static T LoadOrCreateAsset<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadMainAssetAtPath(path) as T;
            if (null == asset)
            {
                asset = ScriptableObject.CreateInstance<T>();
                if (!CreateAsset(asset, path))
                {
                    return null;
                }
            }
            asset = AssetDatabase.LoadMainAssetAtPath(path) as T;
            return asset;
        }

        /// <summary>
        ///Load or create Json
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        public static T LoadOrCreatJson<T>(string path) where T : new()
        {
            var fullPath = GetRegularPath($"{Application.dataPath}/../{path}");
            //establish
            if (!File.Exists(fullPath))
            {
                var obj = new T();
                var s = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
                var dir = GetDirectoryName(fullPath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                File.WriteAllText(fullPath, s);
            }
            var json = File.ReadAllText(fullPath);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>
        ///Get the name of the target platform
        /// </summary>
        /// <returns></returns>
        public static string GetBuildTargetName()
        {
            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            switch (buildTarget)
            {
                case BuildTarget.Android: return "android";
                case BuildTarget.iOS: return "ios";
                case BuildTarget.StandaloneWindows: return "win32";
                case BuildTarget.StandaloneWindows64: return "win64";
                case BuildTarget.StandaloneOSX: return "osx";
                 default: 
                     // Log.Error($"Unsupport BuildTarget {buildTarget}"); 
                     return null;
            }
        }

        /// <summary>
        ///Find Resources in Folders
        /// </summary>
        /// / ~~param name="subPath"356; "'23545;res/param name="subPath"""'...
        ///<param name="resType">Resource Type</param>
        ///<param name="searchInFolder">Search Folders</param>
        /// <returns></returns>
        private static string FindAssetInFolder(string subPath, ResType resType, string searchInFolder)
        {
            //AssetName may contain subdirectories
            subPath = GetRegularPath(subPath);
            var subDir = GetRegularPath(Path.GetDirectoryName(subPath));
            var assetName = subPath;
            var dstFolder = searchInFolder;
            if (!string.IsNullOrEmpty(subDir))
            {
                dstFolder = $"{searchInFolder}/{subDir}";
                assetName = Path.GetFileName(subPath);
            }

            //directory does not exist
            if (!Directory.Exists(dstFolder))
            {
                return null;
            }

            //search
            var filter = ResHelper.GetResTypeFilterName(resType);
            filter = $"{assetName} t:{filter}";
            var assets = AssetDatabase.FindAssets(filter, new[] { dstFolder });
            if (null == assets || assets.Length == 0)
            {
                return null;
            }

            //filter
            sTempList.Clear();
            for (int i = 0; i < assets.Length; ++i)
            {
                var path = AssetDatabase.GUIDToAssetPath(assets[i]);

                //Is a directory
                if (AssetDatabase.IsValidFolder(path))
                {
                    continue;
                }

                //File names should be consistent
                var fileName = Path.GetFileNameWithoutExtension(path);
                if (fileName != assetName)
                {
                    continue;
                }

                //Relative path must be an incoming subpath
                var relativePath = GetRelativePath(path, searchInFolder);
                if (null == relativePath)
                {
                    continue;
                }
                var relativePathWithoutExt = GetPathWithoutExt(relativePath);
                if (relativePathWithoutExt != subPath)
                {
                    continue;
                }
                sTempList.Add(path);
            }

            //empty
            if (sTempList.Count == 0)
            {
                return null;
            }

            //Resources with duplicate names
            if (sTempList.Count > 1)
            {
                // Log.Error("Asset {0} name repeat", Utility.Text.FormatArray(sTempList.ToArray()));
                return null;
            }

            return sTempList[0];
        }

        /// <summary>
        ///Find Resources
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="resType"></param>
        /// <returns></returns>
        public static string FindAsset(string assetName, ResType resType)
        {
            //Resource Directory
            var resRootPath = EditorProjectSettings.Get().ResRootPath;

            ////Search localization directory first
            // if (!string.IsNullOrEmpty(I18nManager.ResSubPath))
            // {
            //     var i18nPath = Utility.Text.Format("{0}/{1}", resRootPath, I18nManager.ResSubPath);
            //     var i18nFilePath = FindAssetInFolder(assetName, resType, i18nPath);
            //     if (!string.IsNullOrEmpty(i18nFilePath))
            //     {
            //         return i18nFilePath;
            //     }
            // }

            //Entire Resource Directory
            var path = FindAssetInFolder(assetName, resType, resRootPath);
            if (string.IsNullOrEmpty(path))
            {
                // Log.Error("Asset {0} not exist", assetName);
                return null;
            }

            return path;
        }

        /// <summary>
        ///Load Resources
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="resType"></param>
        /// <returns></returns>
        public static UnityEngine.Object LoadAsset(string assetName, ResType resType)
        {
            //Find Resources
            var path = FindAsset(assetName, resType);
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            //Resource Type
            var assetType = ResHelper.GetAssetTypeByResType(resType);
            if (null == assetType)
            {
                return null;
            }

            //load
            var asset = AssetDatabase.LoadAssetAtPath(path, assetType);

            //fail
            if (null == asset)
            {
                // Log.Error("Load asset name: {0}, type: {1} from {2} failed", assetName, resType, path);
                return null;
            }

            return asset;
        }


        /// <summary>
        ///Get Supported Languages
        /// </summary>
        /// <returns></returns>
        // public static ELanguage[] GetSupportLanguages()
        // {
        //     var i18nDir = GetRegularPath($"{EditorProjectSettings.Get().ResRootPath}/{ResDefine.I18nDirName}");
        //
        ////No localization
        //     if (!AssetDatabase.IsValidFolder(i18nDir))
        //     {
        //         return new ELanguage[0];
        //     }
        //
        ////Constructing language coding dictionary
        //     var codes = new Dictionary<string, ELanguage>();
        //     for (int i = 0; i < (int)ELanguage.Num; ++i)
        //     {
        //         var c = LanguageHelper.GetLanguageCode((ELanguage)i);
        //         if (codes.TryGetValue(c, out ELanguage lan))
        //         {
        //             Log.Error("Language {0} and {1} code {2} repeat", lan, (ELanguage)i, c);
        //             return null;
        //         }
        //         codes.Add(c, (ELanguage)i);
        //     }
        //
        ////Traverse the directory to get the language list
        //     List<ELanguage> lans = new List<ELanguage>();
        //     var dirInfo = new DirectoryInfo(i18nDir);
        //     var subDirs = dirInfo.GetDirectories("*", SearchOption.TopDirectoryOnly);
        //     for (int i = 0; i < subDirs.Length; ++i)
        //     {
        //         var dirName = subDirs[i].Name;
        //         if (!codes.TryGetValue(dirName, out ELanguage lan))
        //         {
        //             Log.Error("Language {0} not exist", subDirs[i].FullName);
        //             return null;
        //         }
        //         lans.Add(lan);
        //     }
        //
        //     return lans.ToArray();
        // }
    }
}
#endif