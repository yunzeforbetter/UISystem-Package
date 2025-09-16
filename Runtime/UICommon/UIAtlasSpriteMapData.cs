using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.U2D;

namespace UISystem
{
    /// <summary>
    /// ͼ������ӳ��������
    /// ��¼��������Ӧ��ͼ����Ϣ
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
        /// ��ʼ��
        /// </summary>
        public void Init()
        {
            //����ӳ���
            spriteMap.Clear();
            foreach (AtlasSpriteData data in atlasSrpiteDatas)
            {
                foreach (string name in data.spriteList)
                {
                    spriteMap.Add(name, data.atlasName);
                }
            }
        }

        [Button("����")]
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

        //��������
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
        /// ��ȡ�����Ӧ��ͼ������
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
    /// ͼ����������
    /// </summary>
    [Serializable]
    public class AtlasSpriteData
    {
        public string atlasName;
        public List<string> spriteList = new();
    }

}