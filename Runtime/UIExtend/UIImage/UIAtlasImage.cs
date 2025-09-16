using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;


namespace UISystem
{
    /// <summary>
    /// 基于图集的Image控件
    /// </summary>
    public class UIAtlasImage : Image
    {
        /// <summary>
        /// 图集
        /// </summary>
        private SpriteAtlas spriteAtlas = null;

        [LabelText("自适应大小")]
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
        /// 置灰标志;
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
            // 图集未变，就直接设置精灵;
            if (spriteAtlas != null && atlasName == spriteAtlas.name)
            {
                SetSpriteInternal(spriteName);
                return;
            }

            // 加载过程种;
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