
using UnityEngine.UI;
using UnityEngine;

namespace UISystem
{
    /// <summary>
    /// UI Shader效果
    /// </summary>
    public static class UIShaderEffect
    {
        // 灰度;
        private static Material grayMat;

        /// <summary>
        /// 创建置灰材质球
        /// </summary>
        /// <returns></returns>
        private static Material GetGrayMat()
        {
            if (grayMat == null)
            {
                Shader shader = Shader.Find("Custom/UI-Gray"); //获取置灰shader  UI_Gray.shader
                if (shader == null)
                {
                    Debug.LogError($"加载置灰shader失败  请排查");
                    return null;
                }
                Material mat = new Material(shader);
                grayMat = mat;
            }

            return grayMat;
        }

        /// <summary>
        /// 图片置灰
        /// </summary>
        /// <param name="img"></param>
        public static void SetUIGray(MaskableGraphic img)
        {
            if (img.material == GetGrayMat())
                return;
            img.material = GetGrayMat();
            img.SetMaterialDirty();
        }

        /// <summary>
        /// 图片恢复;
        /// </summary>
        /// <param name="img"></param>
        public static void Recovery(MaskableGraphic img)
        {
            if (img.material == GetGrayMat())
            {
                img.material = null;
            }
        }
    }

}