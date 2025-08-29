using System;
using UnityEngine;
using UnityEngine.U2D;

namespace UISystem
{
    /// <summary>
    ///Resource path auxiliary class
    /// </summary>
    public static class ResHelper
    {

        /// <summary>
        ///Resource Type Name
        /// </summary>
        private static string[] sResTypeName;

        /// <summary>
        ///Resource Type Filter Name
        /// </summary>
        private static string[] sResTypeFilterName;

        /// <summary>
        ///Asset type corresponding to resource type
        /// </summary>
        private static Type[] sResTypeAssetType;


        /// <summary>
        ///Get StreamingAssets path
        /// </summary>
        /// <returns></returns>
        internal static string GetStreamingAssetsPath()
        {
             return Application.streamingAssetsPath;
        }


        /// <summary>
        ///Get resource type name
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static string GetResTypeName(ResType t)
        {
            if (null == sResTypeName)
            {
                sResTypeName = new string[(int)ResType.Num];
                sResTypeName[(int)ResType.Scene] = "scene";
                sResTypeName[(int)ResType.Prefab] = "prefab";
                sResTypeName[(int)ResType.Shader] = "shader";
                sResTypeName[(int)ResType.Model] = "model";
                sResTypeName[(int)ResType.Material] = "mat";
                sResTypeName[(int)ResType.Texture] = "tex";
                sResTypeName[(int)ResType.Sprite] = "sp";
                sResTypeName[(int)ResType.SpriteAtlas] = "atlas";
                sResTypeName[(int)ResType.AudioClip] = "audio";
                sResTypeName[(int)ResType.AnimationClip] = "anim";
                sResTypeName[(int)ResType.AnimatorController] = "anim_c";
                sResTypeName[(int)ResType.Font] = "fnt";
                sResTypeName[(int)ResType.TextAsset] = "txt";
                sResTypeName[(int)ResType.ScriptableObject] = "sco";
            }
            var n = (int)t;
            if (n < 0 || n >= sResTypeName.Length)
            {
                // Log.Error("Invalid ResType {0}", t);
                return null;
            }
            return sResTypeName[n];
        }

        /// <summary>
        ///Get resource type filter name
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static string GetResTypeFilterName(ResType t)
        {
            if (null == sResTypeFilterName)
            {
                sResTypeFilterName = new string[(int)ResType.Num];
                sResTypeFilterName[(int)ResType.Scene] = "Scene";
                sResTypeFilterName[(int)ResType.Prefab] = "Prefab";
                sResTypeFilterName[(int)ResType.Shader] = "Shader";
                sResTypeFilterName[(int)ResType.Model] = "Model";
                sResTypeFilterName[(int)ResType.Material] = "Material";
                sResTypeFilterName[(int)ResType.Texture] = "Texture";
                sResTypeFilterName[(int)ResType.Sprite] = "Sprite";
                sResTypeFilterName[(int)ResType.SpriteAtlas] = "SpriteAtlas";
                sResTypeFilterName[(int)ResType.AudioClip] = "AudioClip";
                sResTypeFilterName[(int)ResType.AnimationClip] = "AnimationClip";
                sResTypeFilterName[(int)ResType.AnimatorController] = "AnimatorController";
                sResTypeFilterName[(int)ResType.Font] = "Font";
                sResTypeFilterName[(int)ResType.TextAsset] = "TextAsset";
                sResTypeFilterName[(int)ResType.ScriptableObject] = "ScriptableObject";
            }
            var n = (int)t;
            if (n < 0 || n >= sResTypeFilterName.Length)
            {
                // Log.Error("Invalid ResType {0}", t);
                return null;
            }
            return sResTypeFilterName[n];
        }

        /// <summary>
        ///Get resource name
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static string GetResName(string assetName, ResType t)
        {
            var resTypeName = GetResTypeName(t);
            if (string.IsNullOrEmpty(resTypeName))
            {
                return assetName;
            }
            return $"{assetName}.{resTypeName}";
        }

        /// <summary>
        ///Get Resource Type
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Type GetAssetTypeByResType(ResType t)
        {
            if (null == sResTypeAssetType)
            {
                sResTypeAssetType = new Type[(int)ResType.Num];
                sResTypeAssetType[(int)ResType.Scene] = null;
                sResTypeAssetType[(int)ResType.Prefab] = typeof(GameObject);
                sResTypeAssetType[(int)ResType.Shader] = typeof(Shader);
                sResTypeAssetType[(int)ResType.Model] = typeof(GameObject);
                sResTypeAssetType[(int)ResType.Material] = typeof(Material);
                sResTypeAssetType[(int)ResType.Texture] = typeof(Texture);
                sResTypeAssetType[(int)ResType.Sprite] = typeof(Sprite);
                sResTypeAssetType[(int)ResType.SpriteAtlas] = typeof(SpriteAtlas);
                sResTypeAssetType[(int)ResType.AudioClip] = typeof(AudioClip);
                sResTypeAssetType[(int)ResType.AnimationClip] = typeof(AnimationClip);
                sResTypeAssetType[(int)ResType.AnimatorController] = typeof(Animator);
                sResTypeAssetType[(int)ResType.Font] = typeof(Font);
                sResTypeAssetType[(int)ResType.TextAsset] = typeof(TextAsset);
                sResTypeAssetType[(int)ResType.ScriptableObject] = typeof(ScriptableObject);
            }
            var n = (int)t;
            if (n < 0 || n >= sResTypeAssetType.Length)
            {
                // Log.Error("Invalid ResType {0}", t);
                return null;
            }
            return sResTypeAssetType[n];
        }

    }
}