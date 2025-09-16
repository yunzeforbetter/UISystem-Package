
using UnityEngine.UI;
using UnityEngine;

namespace UISystem
{
    /// <summary>
    /// UI ShaderЧ��
    /// </summary>
    public static class UIShaderEffect
    {
        // �Ҷ�;
        private static Material grayMat;

        /// <summary>
        /// �����ûҲ�����
        /// </summary>
        /// <returns></returns>
        private static Material GetGrayMat()
        {
            if (grayMat == null)
            {
                Shader shader = Shader.Find("Custom/UI-Gray"); //��ȡ�û�shader  UI_Gray.shader
                if (shader == null)
                {
                    Debug.LogError($"�����û�shaderʧ��  ���Ų�");
                    return null;
                }
                Material mat = new Material(shader);
                grayMat = mat;
            }

            return grayMat;
        }

        /// <summary>
        /// ͼƬ�û�
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
        /// ͼƬ�ָ�;
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