#define BLUR_USING_CANVASGROUP
using System;
using System.Collections;
using UISystem;
using UnityEngine;
using UnityEngine.UI;

public class BlurController : MonoBehaviour
{
    private static readonly int BlurIterateCount = 2;
    private static readonly int ShaderBlurSpreadProp = Shader.PropertyToID("_BlurSpread");

    public RawImage backGroundImage;
    [SerializeField] private bool autoValid = true;
    [SerializeField] private Material blurMaterial = null;
    [Range(0.1f, 3.0f)]
    [SerializeField] private float blurSpread = 0.5f;

    private bool _enableBlur = false;
    private bool _bgReady = false;
    private RenderTexture _blurRT0 = null;
    private RenderTexture _blurRT1 = null;

#if BLUR_USING_GAMEOBJECT
    public GameObject HideView;
#elif BLUR_USING_CANVAS
    private Canvas _panelCanvas;
#elif BLUR_USING_CANVASGROUP
    private CanvasGroup _panelCanvasGroup;
    private float _canvasGroupAlpha = 1.0f;
#endif

    private GameObject _topPanelObj = null;
    private float _topCanvasGroupAlpha = 1.0f;
    private Action<Transform> onTopPanelBlurSuccess;
    private void Awake()
    {
#if BLUR_USING_CANVAS
        _panelCanvas = GetComponent<Canvas>();
#elif BLUR_USING_CANVASGROUP
        _panelCanvasGroup = GetComponent<CanvasGroup>();
        if (_panelCanvasGroup == null)
        {
            _panelCanvasGroup = gameObject.AddComponent<CanvasGroup>();
            _canvasGroupAlpha = 1.0f;
        }
        else
            _canvasGroupAlpha = _panelCanvasGroup.alpha;
#endif
    }

    private void OnEnable()
    {
        StartCoroutine(SnapShot());
        if (!autoValid)
        {
            return;
        }
        BlurBg();
    }

    private void OnDisable()
    {
        if (backGroundImage != null)
        {
            backGroundImage.texture = null;
        }
        ReleaseRT();
    }
    private void OnDestroy()
    {
        if (backGroundImage != null)
        {
            backGroundImage.texture = null;
        }
        ReleaseRT();
    }
    private void ReleaseRT()
    {
        _bgReady = false;

        if (_blurRT0 != null)
        {
            RenderTexture.ReleaseTemporary(_blurRT0);
            _blurRT0 = null;
        }

        if (_blurRT1 != null)
        {
            RenderTexture.ReleaseTemporary(_blurRT1);
            _blurRT1 = null;
        }
        EnableHideView(true);
    }

    public void SetTopPanel(long ui_id, Action<Transform> callBack)
    {
        var panel = UIManager.Instance.GetUI(ui_id);
        if (panel != null)
        {
            _topPanelObj = panel.owner;
            var topPanelCanvasGroup = _topPanelObj?.GetComponent<CanvasGroup>();
            if (topPanelCanvasGroup != null)
            {
                _topCanvasGroupAlpha = topPanelCanvasGroup.alpha;
            }
            onTopPanelBlurSuccess = callBack;
        }
    }
    public void BlurBg()
    {
        backGroundImage.color = Color.clear;
        _enableBlur = true;
        SetupBg();
    }

    public void NoBlurBg()
    {
        _enableBlur = false;
        if (backGroundImage != null)
        {
            backGroundImage.texture = null;
        }
        ReleaseRT();
        EnableHideView(true);
    }

    private void SetupBg()
    {
        if (backGroundImage != null && backGroundImage.texture == null)
        {
            if (_bgReady)
            {
                backGroundImage.color = Color.white;
                backGroundImage.texture = _blurRT0;
            }
        }
    }
    private void Update()
    {
        if (_enableBlur)
        {
            SetupBg();
        }
    }
    void EnableHideView(bool enable)
    {
#if BLUR_USING_GAMEOBJECT
        HideView.SetActive(enable);
#elif BLUR_USING_CANVAS
        _panelCanvas.enabled = enable;
#elif BLUR_USING_CANVASGROUP
        _panelCanvasGroup.alpha = enable ? _canvasGroupAlpha : 0.0f;
#endif

        if (_topPanelObj != null)
        {
            var topPanelCanvas = _topPanelObj.GetComponent<Canvas>();
            var topPanelCanvasGroup = _topPanelObj.GetComponent<CanvasGroup>();

            if (topPanelCanvasGroup != null)
            {
                topPanelCanvasGroup.alpha = enable ? _topCanvasGroupAlpha : 0.0f;
            }
            else if (topPanelCanvas != null)
            {
                topPanelCanvas.enabled = enable;
            }

            if(enable)
            {
                onTopPanelBlurSuccess?.Invoke(_topPanelObj.transform);
                onTopPanelBlurSuccess = null;
            }
        }
    }

    private Camera FindMainCamera()
    {
        return Camera.main;
    }

    private Camera FindUICamera()
    {
        // 查找专门用于UI的相机
        var cameras = FindObjectsOfType<Camera>();
        foreach (var cam in cameras)
        {
            if (cam.cullingMask == LayerMask.GetMask("UI") || cam.name.Contains("UI"))
            {
                return cam;
            }
        }
        return null;
    }


    IEnumerator SnapShot()
    {
        //等待渲染结束
        yield return new WaitForEndOfFrame();

        EnableHideView(false);

        bool captureSuccess = false;

        if (IsScreenSpaceCameraMode())
        {
            // 首先尝试相机渲染方式（适用于Screen Space - Camera模式）
            var mainCam = FindMainCamera(); //获取渲染3d场景的主相机
            var uiCam = FindUICamera(); //获取渲染ui的相机
            if(mainCam != null || uiCam != null)
                captureSuccess = TryCameraCapture(mainCam, uiCam);
        }

        // 如果相机方式失败，使用屏幕截图方式（适用于Screen Space - Overlay模式）
        if (!captureSuccess)
        {
            // 强制渲染当前帧
            Canvas.ForceUpdateCanvases();
            // 对于屏幕截图，需要等待UI隐藏生效
            yield return new WaitForEndOfFrame();
            captureSuccess = TryScreenCapture();
        }

        if (!captureSuccess)
        {
            Debug.LogError("所有截图方式都失败了！");
        }

        EnableHideView(true);


    }
    private bool IsScreenSpaceCameraMode()
    {
        // 检查是否是Screen Space - Camera模式
        var canvas = UIManager.Instance.GetRootCanvas();
        return canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera;
    }

    private bool TryCameraCapture(Camera mainCam, Camera uiCam)
    {
        try
        {
            // Referred from https://forum.unity.com/threads/capture-overlay-ugui-camera-to-texture.1007156/
            //if (mainCam == null && UICam == null)
            //{
            //    Debug.LogError("Blur Controller can not find camera!");
            //}
            //else
            //{
            //    var width = Mathf.FloorToInt(Screen.width);
            //    var height = Mathf.FloorToInt(Screen.height);
            //    _blurRT0 = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
            //    _blurRT1 = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
            //    var rawScreenShot = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
            //    if (UICam != null)
            //    {
            //        var prevUICameraTarget = UICam.targetTexture;
            //        UICam.targetTexture = rawScreenShot;
            //        UICam.Render();
            //        UICam.targetTexture = prevUICameraTarget;
            //    }
            //    if (mainCam != null)
            //    {
            //        var prevCameraTarget = mainCam.targetTexture;
            //        mainCam.targetTexture = rawScreenShot;
            //        mainCam.Render();
            //        mainCam.targetTexture = prevCameraTarget;
            //    }

            //    var rect = new Vector4(1.0f, 1.0f, 0.0f, 0.0f);
            //    Graphics.Blit(rawScreenShot, _blurRT0, new Vector2(rect.x, rect.y), new Vector2(rect.z, rect.w));
            //    RenderTexture.ReleaseTemporary(rawScreenShot);

            //    if (blurMaterial != null)
            //    {
            //        for (var i = 0; i < BlurIterateCount; i++)
            //        {
            //            blurMaterial.SetFloat(ShaderBlurSpreadProp, (1.0f + i) * blurSpread);
            //            Graphics.Blit(_blurRT0, _blurRT1, blurMaterial, 0);
            //            Graphics.Blit(_blurRT1, _blurRT0, blurMaterial, 1);
            //        }
            //    }

            //    RenderTexture.ReleaseTemporary(_blurRT1);
            //    _blurRT1 = null;

            //    _bgReady = true;
            //    EnableHideView(true);
            //}
            var width = Mathf.FloorToInt(Screen.width);
            var height = Mathf.FloorToInt(Screen.height);
            _blurRT0 = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
            _blurRT1 = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
            var rawScreenShot = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);

            // 相机渲染逻辑
            if (uiCam != null)
            {
                var prevUICameraTarget = uiCam.targetTexture;
                uiCam.targetTexture = rawScreenShot;
                uiCam.Render();
                uiCam.targetTexture = prevUICameraTarget;
            }
            if (mainCam != null)
            {
                var prevCameraTarget = mainCam.targetTexture;
                mainCam.targetTexture = rawScreenShot;
                mainCam.Render();
                mainCam.targetTexture = prevCameraTarget;
            }
            var rect = new Vector4(1.0f, 1.0f, 0.0f, 0.0f);
            Graphics.Blit(rawScreenShot, _blurRT0, new Vector2(rect.x, rect.y), new Vector2(rect.z, rect.w));
            RenderTexture.ReleaseTemporary(rawScreenShot);

            ApplyBlurEffect();
            _bgReady = true;
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"相机截图失败: {e.Message}");
            return false;
        }
    }

    private bool TryScreenCapture()
    {
        try
        {
            //获取缓冲区的像素信息
            var screenShot = ScreenCapture.CaptureScreenshotAsTexture();
            if (screenShot != null)
            {
                var width = screenShot.width;
                var height = screenShot.height;
                _blurRT0 = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
                _blurRT1 = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);

                Graphics.Blit(screenShot, _blurRT0);
                DestroyImmediate(screenShot);

                ApplyBlurEffect();
                _bgReady = true;
                return true;
            }
            return false;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"屏幕截图失败: {e.Message}");
            return false;
        }
    }

    private void ApplyBlurEffect()
    {
        if (blurMaterial != null)
        {
            for (var i = 0; i < BlurIterateCount; i++)
            {
                blurMaterial.SetFloat(ShaderBlurSpreadProp, (1.0f + i) * blurSpread);
                Graphics.Blit(_blurRT0, _blurRT1, blurMaterial, 0);
                Graphics.Blit(_blurRT1, _blurRT0, blurMaterial, 1);
            }
        }

        RenderTexture.ReleaseTemporary(_blurRT1);
        _blurRT1 = null;
    }
}
