using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BlurBackgroundRenderFeature : ScriptableRendererFeature
{
    public Material blurMaterial;
    public Material frostMaterial;
    
    public const bool AlwaysEnableTest = false;
    
    public static BlurBackgroundRenderFeature Instance;
    public bool EnableCapture { get; set; }
    public int CaptureFrameIndex { get; private set; }
    
    private BlurRenderPass _blurPass;
    private bool Valid => blurMaterial != null && frostMaterial != null;
    
    public override void Create()
    {
        Instance = this;
        _blurPass = new BlurRenderPass(blurMaterial, frostMaterial);
        EnableCapture = false;
    }

    protected override void Dispose(bool disposing)
    {
        _blurPass?.Dispose();
    }

    public void SetScreenShot(RenderTexture screenshot)
    {
        _blurPass.SetScreenShot(screenshot);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if ((EnableCapture || AlwaysEnableTest) && Valid)
        {
            renderer.EnqueuePass(_blurPass);
            CaptureFrameIndex = Time.frameCount;
            EnableCapture = false;
        }
    }

}

public class BlurRenderPass : ScriptableRenderPass
{
    private Material _blurMaterial;
    private Material _frostMaterial;
    
    private readonly string _profilingTag = "Blur Renderer Pass";
    private ProfilingSampler _profilingSampler = new ProfilingSampler("Blur Renderer Pass");
    
    private RTHandle[] _blurRTs = new RTHandle[4];
    private RTHandle[] _tempRTs = new RTHandle[4];

    private RTHandle _screenShot;
    private enum BlurPass
    {
        BlurH,
        BlurV,
    }

    public BlurRenderPass(Material blurMaterial, Material frostMaterial)
    {
        _blurMaterial = blurMaterial;
        _frostMaterial = frostMaterial;
        for (var i = 0; i < 4; i++)
            _blurRTs[i] = RTHandles.Alloc("_BlurTexture_" + i);
        
        renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
    }

    public void Dispose()
    {
        for (int i = 0; i < 4; i++)
            RTHandles.Release(_blurRTs[i]);

        RTHandles.Release(_screenShot);
    }

    public void SetScreenShot(RenderTexture rt)
    {
        _screenShot = RTHandles.Alloc(rt);
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        var cameraData = renderingData.cameraData;
        for(var i = 0; i < 4; i++)
        {
            var downScaling = (int)Mathf.Pow(2, i + 1);
            RenderingUtils.ReAllocateIfNeeded(ref _tempRTs[i], new RenderTextureDescriptor
            {
                width = cameraData.camera.scaledPixelWidth / downScaling,
                height = cameraData.camera.scaledPixelHeight / downScaling,
                dimension = TextureDimension.Tex2D,
                colorFormat = RenderTextureFormat.RGB111110Float,
                msaaSamples = 1,
                volumeDepth = 1
            }, FilterMode.Bilinear);
        }
    }

    public override void OnCameraCleanup(CommandBuffer cmd)
    {
        for (var i = 0; i < 4; i++)
            RTHandles.Release(_tempRTs[i]);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        var cmd = CommandBufferPool.Get(_profilingTag);
        using (new ProfilingScope(cmd, _profilingSampler))
        {
            var cameraData = renderingData.cameraData;
            ReAllocateRTIfNeeded(cameraData);

            // 1. Blur Passes
            //var source = cameraData.renderer.cameraColorTargetHandle;
            //RTHandle source = RTHandles.Alloc(BuiltinRenderTextureType.None);
            RTHandle source = _screenShot;
            // var cameraTarget = BuiltinRenderTextureType.CurrentActive;
            // RTHandleStaticHelpers.SetRTHandleStaticWrapper(cameraTarget);
            // RTHandle source = RTHandleStaticHelpers.s_RTHandleWrapper;
           
            for (var i = 0; i < 4; i++)
            {
                Blitter.BlitCameraTexture(cmd, source, _blurRTs[i]);
                Blitter.BlitCameraTexture(cmd, _blurRTs[i], _tempRTs[i], _blurMaterial, (int)BlurPass.BlurH);
                Blitter.BlitCameraTexture(cmd, _tempRTs[i], _blurRTs[i], _blurMaterial, (int) BlurPass.BlurV);
                source = _blurRTs[i];
            }
            
            // 2. Frost Pass
            for (var i = 0; i < 4; i++)
            {
                _frostMaterial.SetTexture(ShaderConstants._BlurTextures[i], _blurRTs[i]);
                Shader.SetGlobalTexture(ShaderConstants._BlurTextures[i], _blurRTs[i]);
            }

            //source.Release();

            // Blitter.BlitCameraTexture(cmd, cameraData.renderer.cameraColorTargetHandle, _outputRT, _frostMaterial, 0);
            // cmd.Blit((RenderTargetIdentifier)cameraData.renderer.cameraColorTargetHandle, _outputRT, _frostMaterial, 0);
        }
        
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
        CommandBufferPool.Release(cmd);
    }

    private void ReAllocateRTIfNeeded(in CameraData cameraData)
    {
        for(var i = 0; i < 4; i++)
        {
            var downScaling = (int)Mathf.Pow(2, i + 1);
            RenderingUtils.ReAllocateIfNeeded(ref _blurRTs[i], new RenderTextureDescriptor
            {
                width = cameraData.camera.scaledPixelWidth / downScaling,
                height = cameraData.camera.scaledPixelHeight / downScaling,
                dimension = TextureDimension.Tex2D,
                colorFormat = RenderTextureFormat.RGB111110Float,
                msaaSamples = 1,
                volumeDepth = 1
            }, FilterMode.Bilinear);
        }
    }

    static class ShaderConstants
    {
        public static readonly int _BlurSpread = Shader.PropertyToID("_BlurSpread");

        public static readonly int _FrostIntensity = Shader.PropertyToID("_FrostIntensity");
        public static readonly int[] _BlurTextures = new[]
        {
            Shader.PropertyToID("_BlurTexture_0"),
            Shader.PropertyToID("_BlurTexture_1"),
            Shader.PropertyToID("_BlurTexture_2"),
            Shader.PropertyToID("_BlurTexture_3"),
        };
    }
}
