using UISystem;
using UnityEngine;
using UnityEngine.UI;

public class DynamicRawImage : RawImage
{
    //[SerializeField] private DynamicAtlasGroup m_Group = DynamicAtlasGroup.Size_2048;
    [Tooltip("可以设置默认加载图片")]
    [SerializeField] private string mCurPath;
    [SerializeField] private bool mIsGray = false;
    [SerializeField] private bool m_IsSyn = false;
    [SerializeField] private bool m_IsNative = false;
	
    private DynamicAtlas mAtlas;
    private string mLastIcon;
    private bool mLastIsGray;
    private bool mLoadFlag = false;

    private bool mSetFlag = false;

    private bool mIsShow = false;
    
#if UNITY_EDITOR
    private bool editorCleanFlag = true;
    protected override void Awake()
    {
        base.Awake();
        if (Application.isPlaying && editorCleanFlag)
        {
            texture = null;
            editorCleanFlag = false;
        }
    }
#endif

    public DynamicAtlasGroup GetGroup()
    {
        // 写死，防止不规范导致多张大图
        return DynamicAtlasGroup.Size_2048;
    }

    protected override void OnEnable()
    {
        mIsShow = true;
        
        base.OnEnable();
#if UNITY_EDITOR
        if (!Application.isPlaying)
            return;
#endif
        LoadCurIcon();
    }

    protected override void OnDisable()
    {
        mIsShow = false;
        
        base.OnDisable();
        ReleaseCurIcon();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        ReleaseLastIcon();
        ReleaseCurIcon();
    }
    
    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        base.OnPopulateMesh(toFill);
        #if UNITY_EDITOR
        if (!Application.isPlaying)
            return;
        #endif
        if (!mSetFlag)
        {
            toFill.Clear();
        }
    }

    public void SetNative(bool native)
    {
        if (m_IsNative == native)
            return;

        m_IsNative = native;
        if (native == false || !mSetFlag)
            return;

        SetNativeSize();
    }

    public void SetGray(bool gray)
    {
        if (mIsGray == gray)
            return;

        SetIcon(mCurPath, gray, m_IsSyn);
    }

    public void SetIcon(string icon, bool isGray = false, bool syn = false)
    {
        if (atlas == null)
            return;

        m_IsSyn = syn;
        
        // 当前加载的没有变化;
        if (icon == mCurPath && isGray == mIsGray && mLoadFlag)
            return;

        ReleaseLastIcon();
	    
        if (mLoadFlag)
        {
            if (string.IsNullOrEmpty(icon))
            {
                ReleaseCurIcon();
                mCurPath = null;
                mIsGray = isGray;
                return;
            }
		
            // 缓存住在回调成功后再移除，防止切Icon显示时闪烁;
            mLastIcon = mCurPath;
            mLastIsGray = mIsGray;
        }

        mCurPath = icon;
        mIsGray = isGray;

        if (!mIsShow) return;
	    
        mLoadFlag = true;
        atlas.SetTexture(mCurPath, mIsGray, m_IsSyn, LoadCallBack);
    }

    private void LoadCallBack(Material mater, Rect uv)
    {
        ReleaseLastIcon();

        material = mater;
        uvRect = uv;

        mSetFlag = mater != null;
        if (mSetFlag && m_IsNative)
        {
            SetNativeSize();
        }
    }
    
    private DynamicAtlas atlas
    {
        get
        {
            if (mAtlas == null)
            {
                mAtlas = DynamicAtlasManager.Instance.GetDynamicAtlas(GetGroup());
            }
            return mAtlas;
        }
    }

    private void LoadCurIcon()
    {
        if (!mLoadFlag && !string.IsNullOrEmpty(mCurPath))
        {
            SetIcon(mCurPath, mIsGray, m_IsSyn);
        }
    }

    private void ReleaseLastIcon()
    {
        if (string.IsNullOrEmpty(mLastIcon))
            return;
		
        atlas.RemoveImage(mLastIcon, mLastIsGray, LoadCallBack);
        mLastIcon = null;
    }

    private void ReleaseCurIcon()
    {
        if (!mLoadFlag)
            return;

        atlas.RemoveImage(mCurPath, mIsGray, LoadCallBack);
        mLoadFlag = false;
        mSetFlag = false;
    }
}
