using UISystem;
using UnityEngine;
using UnityEngine.UI;

public class DynamicRawImage : RawImage
{
    [Tooltip("可以设置默认加载图片")]
    [SerializeField] private string curPath;
    [SerializeField] private bool isGray = false;
    [SerializeField] private bool isSyn = false;
    [SerializeField] private bool isNative = false;
	
    private string lastIcon;
    private bool lastIsGray;
    private bool loadFlag = false;

    private bool setFlag = false;

    private bool isShow = false;
    
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
        isShow = true;
        
        base.OnEnable();
#if UNITY_EDITOR
        if (!Application.isPlaying)
            return;
#endif
        LoadCurIcon();
    }

    protected override void OnDisable()
    {
        isShow = false;
        
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
        if (!setFlag)
        {
            toFill.Clear();
        }
    }

    public void SetNative(bool native)
    {
        if (isNative == native)
            return;

        isNative = native;
        if (native == false || !setFlag)
            return;

        SetNativeSize();
    }

    public void SetGray(bool gray)
    {
        if (isGray == gray)
            return;

        SetIcon(curPath, gray, isSyn);
    }

    public void SetIcon(string icon, bool isGray = false, bool syn = false)
    {
        if (Atlas == null)
            return;

        isSyn = syn;
        
        // 当前加载的没有变化;
        if (icon == curPath && isGray == this.isGray && loadFlag)
            return;

        ReleaseLastIcon();
	    
        if (loadFlag)
        {
            if (string.IsNullOrEmpty(icon))
            {
                ReleaseCurIcon();
                curPath = null;
                this.isGray = isGray;
                return;
            }
		
            // 缓存住在回调成功后再移除，防止切Icon显示时闪烁;
            lastIcon = curPath;
            lastIsGray = this.isGray;
        }

        curPath = icon;
        this.isGray = isGray;

        if (!isShow) return;
	    
        loadFlag = true;
        Atlas.SetTexture(curPath, this.isGray, isSyn, LoadCallBack);
    }

    private void LoadCallBack(Material mater, Rect uv)
    {
        ReleaseLastIcon();

        material = mater;
        uvRect = uv;

        setFlag = mater != null;
        if (setFlag && isNative)
        {
            SetNativeSize();
        }
    }


    private DynamicAtlas dynamicAtlas;
    public DynamicAtlas Atlas
    {
        get
        {
            if (dynamicAtlas == null)
            {
                dynamicAtlas = DynamicAtlasManager.Instance.GetDynamicAtlas(GetGroup());
            }
            return dynamicAtlas;
        }
    }

    private void LoadCurIcon()
    {
        if (!loadFlag && !string.IsNullOrEmpty(curPath))
        {
            SetIcon(curPath, isGray, isSyn);
        }
    }

    private void ReleaseLastIcon()
    {
        if (string.IsNullOrEmpty(lastIcon))
            return;
		
        Atlas.RemoveImage(lastIcon, lastIsGray, LoadCallBack);
        lastIcon = null;
    }

    private void ReleaseCurIcon()
    {
        if (!loadFlag)
            return;

        Atlas.RemoveImage(curPath, isGray, LoadCallBack);
        loadFlag = false;
        setFlag = false;
    }
}
