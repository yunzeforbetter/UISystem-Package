using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;


namespace UISystem
{
    /// <summary>
    /// ����ͼ����Image�ؼ�
    /// </summary>
    public class UIAtlasImage : Image
    {
        /// <summary>
        /// ͼ��
        /// </summary>
        private SpriteAtlas spriteAtlas = null;

        [LabelText("����Ӧ��С")]
        [SerializeField] private bool setNative = false;
        public bool SetNative
        {
            get { return setNative; }
            set
            {
                if (setNative == value)
                    return;

                setNative = value;
                if (setNative && sprite)
                {
                    SetNativeSize();
                }
            }
        }


        private bool isGray = false;
        /// <summary>
        /// �ûұ�־;
        /// </summary>
        public bool IsGray
        {
            get { return isGray; }
            set
            {
                if (isGray == value)
                    return;

                isGray = value;
                SetGray();
            }
        }

        private string dynamicAtlasName = string.Empty;
        private string spriteName = string.Empty;

        private bool initFlag = false;

        private IAssetHandle handle;

        protected override void Awake()
        {
            initFlag = true;
            base.Awake();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (handle == null && !string.IsNullOrEmpty(dynamicAtlasName))
            {
                LoadInternal();
            }
        }

        protected override void Start()
        {
            base.Start();
            SetGray();
            if (SetNative && sprite != null)
            {
                SetNativeSize();
            }
        }

        public void SetSprite(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                SetAtlasSpriteInternal(null, name);
                return;
            }

            string atlasName = UIAtlasSpriteMapData.GetAtlasName(name);
            SetAtlasSpriteInternal(atlasName, name);
        }
        private void SetSpriteInternal(string name)
        {
            if (spriteAtlas != null)
            {
                sprite = spriteAtlas.GetSprite(name);
                if (SetNative)
                {
                    SetNativeSize();
                }
            }
        }

        public void SetAtlasSprite(string atlasName, string spriteName)
        {
            SetAtlasSpriteInternal(atlasName, spriteName);
        }

        private void SetAtlasSpriteInternal(string atlasName, string spriteName)
        {
            // ͼ��δ�䣬��ֱ�����þ���;
            if (spriteAtlas != null && atlasName == spriteAtlas.name)
            {
                SetSpriteInternal(spriteName);
                return;
            }

            // ���ع�����;
            this.spriteName = spriteName;
            if (dynamicAtlasName != atlasName)
            {
                Clear();

                dynamicAtlasName = atlasName;
                if (string.IsNullOrEmpty(atlasName))
                {
                    sprite = null;
                }
                else if (initFlag)
                {
                    LoadInternal();
                }
            }
        }

        private void LoadInternal()
        {
            handle = UIManager.Instance.resourceMgr.LoadAssetHandle<SpriteAtlas>(dynamicAtlasName, OnSpriteAtlasLoadComplete);
        }

        private void Clear()
        {
            if (!string.IsNullOrEmpty(dynamicAtlasName))
            {
                handle?.Release();
                handle = null;
                dynamicAtlasName = null;
            }
            spriteAtlas = null;
        }

        private void OnSpriteAtlasLoadComplete(IAssetHandle resource)
        {
            if (resource == null) return;

            spriteAtlas = resource.Asset as SpriteAtlas;
            SetSpriteInternal(spriteName);
        }

        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            base.OnPopulateMesh(toFill);
            if (sprite == null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    return;
#endif
                toFill.Clear();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Clear();
        }


        private void SetGray()
        {
            if (IsGray)
            {
                UIShaderEffect.SetUIGray(this);
            }
            else
            {
                UIShaderEffect.Recovery(this);
            }
        }

    }

}