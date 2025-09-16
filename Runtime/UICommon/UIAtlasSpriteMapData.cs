using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.U2D;

namespace UISystem
{
    /// <summary>
    /// 图集精灵映射数据类
    /// 记录精灵所对应的图集信息
    /// </summary>
    [CreateAssetMenu(menuName = "UI/UIAtlasSpriteMapData")]
    public class UIAtlasSpriteMapData : SerializedScriptableObject
    {
        [SerializeField]
        public string atlasRootPath = "";

        private static Dictionary<string, string> spriteMap = new();
        [SerializeField]
        private List<AtlasSpriteData> atlasSrpiteDatas = new();

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            //构建映射表
            spriteMap.Clear();
            foreach (AtlasSpriteData data in atlasSrpiteDatas)
            {
                foreach (string name in data.spriteList)
                {
                    spriteMap.Add(name, data.atlasName);
                }
            }
        }

        [Button("生成")]
        public void InitEditor()
        {
#if UNITY_EDITOR
            var allAtlas = UnityEditor.AssetDatabase.FindAssets("t:spriteAtlas", new string[] { atlasRootPath });
            List<SpriteAtlas> spriteAtlasList = new List<SpriteAtlas>();
            foreach (string atlas in allAtlas)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(atlas);
                SpriteAtlas spriteAtlas = UnityEditor.AssetDatabase.LoadAssetAtPath<SpriteAtlas>(path);
                if (spriteAtlas == null) continue;
                spriteAtlasList.Add(spriteAtlas);
            }
            InitData(spriteAtlasList);
#endif
        }

        //生成数据
        private void InitData(List<SpriteAtlas> list)
        {
            atlasSrpiteDatas.Clear();

            List<string> spriteList = new List<string>();
            foreach (SpriteAtlas atlas in list)
            {
                int count = atlas.spriteCount;
                if (count == 0) continue;

                Sprite[] sprites = new Sprite[count];
                atlas.GetSprites(sprites);

                AtlasSpriteData spriteData = new AtlasSpriteData();
                spriteData.atlasName = atlas.name;

                foreach (Sprite sprite in sprites)
                {
                    string spriteN = sprite.name.Replace("(Clone)", "");
                    if (!spriteList.Contains(spriteN))
                    {
                        spriteList.Add(spriteN);
                        spriteData.spriteList.Add(spriteN);
                    }
                }
                atlasSrpiteDatas.Add(spriteData);
            }
        }

        /// <summary>
        /// 获取精灵对应的图集名称
        /// </summary>
        /// <param name="spriteName"></param>
        /// <returns></returns>
        public static string GetAtlasName(string spriteName)
        {
            if (spriteMap.TryGetValue(spriteName, out string atlasName))
            {
                return atlasName;
            }
            return null;
        }
    }

    /// <summary>
    /// 图集精灵数据
    /// </summary>
    [Serializable]
    public class AtlasSpriteData
    {
        public string atlasName;
        public List<string> spriteList = new();
    }

}