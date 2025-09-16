
using UnityEngine.UI;
using UnityEngine;

namespace UISystem
{
    /// <summary>
    /// UI Shader–ßπ˚
    /// </summary>
    public static class UIShaderEffect
    {
        // ª“∂»;
        private static Material grayMat;

        /// <summary>
        /// ¥¥Ω®÷√ª“≤ƒ÷ «Ú
        /// </summary>
        /// <returns></returns>
        private static Material GetGrayMat()
        {
            if (grayMat == null)
            {
                Shader shader = Shader.Find("Custom/UI-Gray"); //ªÒ»°÷√ª“shader  UI_Gray.shader
                if (shader == null)
                {
                    Debug.LogError($"º”‘ÿ÷√ª“shader ß∞‹  «Î≈≈≤È");
                    return null;
                }
                Material mat = new Material(shader);
                grayMat = mat;
            }

            return grayMat;
        }

        /// <summary>
        /// Õº∆¨÷√ª“
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
        /// Õº∆¨ª÷∏¥;
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