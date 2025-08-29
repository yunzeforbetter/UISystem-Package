
#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.U2D;

namespace UISystem.Editor
{
     /// <summary>
    ///Editor Public Method
    /// </summary>
    public static class EditorUtility
    {
        /// <summary>
        ///Gets the path of the specification.
        /// </summary>
        ///<param name="path">The path to be normalized</param>
        ///<returns>The path of the specification</returns>
        public static string GetRegularPath(string path)
        {
            return EditorHelper.GetRegularPath(path);
        }

        /// <summary>
        ///Get relative directory
        /// </summary>
        /// <param name="file"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetRelativePath(string file, string path)
        {
            return EditorHelper.GetRelativePath(file, path);
        }

        /// <summary>
        ///Get relative path
        ///Allow/
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="path2"></param>
        /// <returns></returns>
        public static string GetRelativePathWithDot(string file, string path)
        {
            file = GetRegularPath(file);
            path = GetRegularPath(path);
            var arr1 = path.Split('/');
            var arr2 = file.Split('/');
            int cutIndex = 0;
            for (; cutIndex < arr1.Length && cutIndex < arr2.Length; ++cutIndex)
            {
                if (arr1[cutIndex] != arr2[cutIndex])
                {
                    break;
                }
            }
            var sb = new StringBuilder();
            for (int i = 0; i < arr1.Length - cutIndex; ++i)
            {
                sb.Append("../");
            }
            var subPath = string.Join("/", arr2, cutIndex, arr2.Length - cutIndex);
            sb.Append(subPath);
            return sb.ToString();
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
                // EditorLogger.Error("CreateAsset: null asset");
                return false;
            }
            var dir = EditorHelper.GetDirectoryName(path);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            return true;
        }

        /// <summary>
        ///Save JSON object to file
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool SaveJsonFile(object obj, string path)
        {
            var dir = EditorHelper.GetDirectoryName(path);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            var s = JsonUtility.ToJson(obj, true);
            File.WriteAllText(path, s);
            return true;
        }

        /// <summary>
        ///Convert string to uppercase hump
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        internal static string ToTitleCase(string str)
        {
            var sb = new StringBuilder(str.Length);
            var flag = true;

            for (int i = 0; i < str.Length; i++)
            {
                var c = str[i];
                if (char.IsLetterOrDigit(c))
                {
                    sb.Append(flag ? char.ToUpper(c) : c);
                    flag = false;
                }
                else
                {
                    flag = true;
                }
            }
            return sb.ToString();
        }

        /// <summary>
        ///Copy a single file
        /// </summary>
        /// <param name="srcPath"></param>
        /// <param name="dstPath"></param>
        /// <returns></returns>
        public static bool CopyFile(string srcPath, string dstPath)
        {
            //file does not exist
            if (!File.Exists(srcPath))
            {
                // EditorLogger.Error($"{srcPath} not exist");
                return false;
            }

            //Destination Folder
            var dir = EditorHelper.GetDirectoryName(dstPath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            //Copy
            File.Copy(srcPath, dstPath, true);
            return true;
        }

        /// <summary>
        ///Copy directory (files will be overwritten)
        /// </summary>
        /// <param name="srcPath"></param>
        /// <param name="dstDirectory"></param>
        /// <param name="ignoreMeta"></param>
        /// <param name="onCopyFile"></param>
        /// <returns></returns>
        static public bool CopyFileOrDirectory(string srcPath, string dstDirectory, bool ignoreMeta, Action<string> onCopyFile = null)
        {
            //directory does not exist
            if (!Directory.Exists(srcPath))
            {
                // EditorLogger.Error($"{srcPath} not exists!");
                return false;
            }

            //Create directory
            if (!Directory.Exists(dstDirectory))
            {
                Directory.CreateDirectory(dstDirectory);
            }

            //Get files and subdirectories under the directory
            DirectoryInfo dir = new DirectoryInfo(srcPath);
            var fileInfo = dir.GetFileSystemInfos();
            foreach (FileSystemInfo i in fileInfo)
            {
                if (i.Name.StartsWith(".", StringComparison.Ordinal))
                {
                    continue;
                }
                if (ignoreMeta && i.Extension == ".meta")
                {
                    continue;
                }
                var subDstPath = $"{dstDirectory}/{i.Name}";

                //catalogue
                if (i is DirectoryInfo)
                {
                    //Recursive call to copy subfolders
                    if (!CopyFileOrDirectory(i.FullName, subDstPath, ignoreMeta, onCopyFile))
                    {
                        return false;
                    }
                }
                else
                {
                    File.Copy(i.FullName, subDstPath, true);
                    onCopyFile?.Invoke(subDstPath);
                    // EditorLogger.Info($"Copy file {i.FullName} to {subDstPath}");
                }
            }
            return true;
        }

        /// <summary>
        ///Empty directory
        ///Do not delete hidden files
        /// </summary>
        /// <param name="dir"></param>
        public static void ClearDirectory(string dir)
        {
            if (!Directory.Exists(dir))
            {
                return;
            }
            var dirInfo = new DirectoryInfo(dir);
            var fileInfo = dirInfo.GetFileSystemInfos();
            foreach (FileSystemInfo i in fileInfo)
            {
                if (i.Name.StartsWith(".", StringComparison.Ordinal))
                {
                    continue;
                }

                //catalogue
                if (i is DirectoryInfo)
                {
                    Directory.Delete(i.FullName, true);
                }
                else
                {
                    i.Delete();
                }
            }
        }

        /// <summary>
        ///Get Resource Type
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public static ResType GetResType(string assetPath)
        {
            //load
            var asset = AssetDatabase.LoadMainAssetAtPath(assetPath);
            if (null == asset)
            {
                // Log.Error("Load asset {0} failed", assetPath);
                return ResType.None;
            }

            var t = asset.GetType();
            if (t == typeof(SceneAsset))
            {
                return ResType.Scene;
            }
            if (t == typeof(Shader))
            {
                return ResType.Shader;
            }
            if (t == typeof(Material))
            {
                return ResType.Material;
            }
            if (typeof(Texture).IsAssignableFrom(t))
            {
                var sp = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                if (null != sp)
                {
                    return ResType.Sprite;
                }
                return ResType.Texture;
            }
            if (t == typeof(Sprite))
            {
                return ResType.Sprite;
            }
            if (t == typeof(SpriteAtlas))
            {
                return ResType.SpriteAtlas;
            }
            if (t == typeof(AudioClip))
            {
                return ResType.AudioClip;
            }
            if (t == typeof(AnimationClip))
            {
                return ResType.AnimationClip;
            }
            if (t == typeof(AnimatorController) || t == typeof(AnimatorOverrideController))
            {
                return ResType.AnimatorController;
            }
            if (t == typeof(Font))
            {
                return ResType.Font;
            }
            if (t == typeof(TextAsset))
            {
                return ResType.TextAsset;
            }
            if (typeof(ScriptableObject).IsAssignableFrom(t))
            {
                return ResType.ScriptableObject;
            }

            //Prefabrication
            var prefabType = PrefabUtility.GetPrefabAssetType(asset);
            if (prefabType != PrefabAssetType.NotAPrefab)
            {
                if (prefabType == PrefabAssetType.Model)
                {
                    //Model
                    var go = asset as GameObject;
                    if (go.GetComponentInChildren<MeshFilter>() != null)
                    {
                        return ResType.Model;
                    }

                    //action
                    var modelImporter = AssetImporter.GetAtPath(assetPath) as ModelImporter;
                    if (modelImporter.importAnimation && modelImporter.importedTakeInfos.Length > 0)
                    {
                        return ResType.AnimationClip;
                    }

                    //Model
                    return ResType.Model;
                }
                return ResType.Prefab;
            }

            //unknown type
            // Log.Error("Unknown asset type of {0}", assetPath);
            return ResType.None;
        }

        /// <summary>
        ///Get resource name
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="assetRootPath"></param>
        /// <returns></returns>
        public static string GetAssetName(string assetPath, string assetRootPath)
        {
            //Resource Type
            var resType = GetResType(assetPath);
            if (resType == ResType.None)
            {
                return null;
            }

            //Relative path
            var relativePath = GetRelativePath(assetPath, assetRootPath);
            if (string.IsNullOrEmpty(relativePath))
            {
                return null;
            }

            //name
            var fileName = Path.GetFileNameWithoutExtension(relativePath);
            var resName = ResHelper.GetResName(fileName, resType);

            //route
            var dir = Path.GetDirectoryName(relativePath);
            if (string.IsNullOrEmpty(dir))
            {
                return resName;
            }
            
            return GetRegularPath($"{dir}/{resName}");
        }

        /// <summary>
        ///Get the resource name without extension
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="assetRootPath"></param>
        /// <returns></returns>
        public static string GetAssetNameWithoutExt(string assetPath, string assetRootPath)
        {
            var assetName = GetAssetName(assetPath, assetRootPath);
            if (string.IsNullOrEmpty(assetName))
            {
                return null;
            }
            return assetName.Split('.')[0];
        }

        /// <summary>
        ///Get the name of the AssetBundle
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="assetRootPath"></param>
        /// <returns></returns>
        public static string GetAssetBundleName(string assetPath, string assetRootPath)
        {
            var assetName = GetAssetName(assetPath, assetRootPath);
            if (string.IsNullOrEmpty(assetName))
            {
                return null;
            }
            return GetAssetBundleNameByResName(assetName);
        }

        /// <summary>
        ///Obtain the name of the AssetBundle through the resource name
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public static string GetAssetBundleNameByResName(string resName)
        {
            return $"{resName}.bundle";
        }

        /// <summary>
        ///Judge whether a directory is empty
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static bool IsEmptyDirectory(DirectoryInfo dir)
        {
            //file
            if (dir.GetFiles().Length > 0)
            {
                return false;
            }

            //folder
            var subdirs = dir.GetDirectories("*.*", SearchOption.TopDirectoryOnly);
            foreach (var subdir in subdirs)
            {
                if (!IsEmptyDirectory(subdir))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        ///Get Package Path
        /// </summary>
        /// <returns></returns>
        public static string GetPackagePath()
        {
            return GetRegularPath(Path.GetFullPath($"Packages/{EditorProjectSettings.Get().PackageName}"));
        }
    }
}
#endif