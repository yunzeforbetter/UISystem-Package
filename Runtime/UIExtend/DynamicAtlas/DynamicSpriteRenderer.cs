using UISystem;
using UnityEngine;


[RequireComponent(typeof(SpriteRenderer))]
public class DynamicSpriteRenderer : MonoBehaviour
{
    [Tooltip("可以设置默认加载图片")]
    [SerializeField] private string mCurPath;
    [SerializeField] private bool m_IsSyn = false;
    [SerializeField] private SpriteRenderer m_SpriteRender = null;
    [SerializeField] private bool AnchorBottom = false;
    
    private IAssetHandle mLoader = null;
    private IAssetHandle mLastLoader = null;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (m_SpriteRender == null)
        {
            m_SpriteRender = gameObject.GetComponent<SpriteRenderer>();
        }
    }
#endif
    
#if UNITY_EDITOR
    private bool editorCleanFlag = true;
    protected void Awake()
    {
        if (Application.isPlaying && editorCleanFlag)
        {
            if (m_SpriteRender != null)
            {
                m_SpriteRender.sprite = null;
            }
            editorCleanFlag = false;
        }
    }
#endif
    
    private void OnEnable()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            return;
#endif
        LoadCurIcon();
    }

    private void OnDisable()
    {
        ReleaseCurIcon();
    }
    
    private void OnDestroy()
    {
        Clear();
    }

    public void Clear()
    {
        ReleaseLastIcon();
        ReleaseCurIcon();
    }

    public void SetIcon(string icon, bool syn = false)
    {
        m_IsSyn = syn;
        
        // 当前加载的没有变化;
        if (icon == mCurPath && mLoader != null)
            return;

        ReleaseLastIcon();
	    
        if (mLoader != null)
        {
            if (string.IsNullOrEmpty(icon))
            {
                ReleaseCurIcon();
                mCurPath = null;
                return;
            }
		
            // 缓存住在回调成功后再移除，防止切Icon显示时闪烁;
            mLastLoader = mLoader;
        }

        mCurPath = icon;

        if (!enabled) return;

        mLoader = UIManager.Instance.resourceMgr.LoadAssetHandle<Texture2D>(mCurPath, LoadCallBack, m_IsSyn ? E_LoadType.Sync : E_LoadType.Async);
    }

    private void LoadCallBack(IAssetHandle ah)
    {
        Debug.Log("LoadCallBack ");
        if (ah == null)
            return;
        var data = ah.Asset as Texture2D;
        if (m_SpriteRender != null && data != null)
        {
            Rect spriteUV = new Rect(0, 0, data.width, data.height);
            Sprite sp = Sprite.Create(data, spriteUV, new Vector2(0.5f, AnchorBottom ? 0f : 0.5f));
            m_SpriteRender.sprite = sp;
            Debug.Log("LoadCallBack set sprite");
        }
        else
        {
            m_SpriteRender.sprite = null;
        }
//        if (setFlag && isNative)
//        {
//            SetNativeSize();
//        }
    }

    private void LoadCurIcon()
    {
        if (mLoader == null && !string.IsNullOrEmpty(mCurPath))
        {
            SetIcon(mCurPath, m_IsSyn);
        }
    }

    private void ReleaseLastIcon()
    {
        if (mLastLoader != null)
        {
            mLastLoader.Release();
            mLastLoader = null;
            m_SpriteRender.sprite = null;
        }
    }

    private void ReleaseCurIcon()
    {
        if (mLoader != null)
        {
            mLoader.Release();
            mLoader = null;
            m_SpriteRender.sprite = null;
        }
    }
}
