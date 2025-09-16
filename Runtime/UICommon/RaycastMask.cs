using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 不规则按钮点击
/// </summary>
[RequireComponent(typeof(Image))]
public class RaycastMask : MonoBehaviour, ICanvasRaycastFilter
{

    private Image image_;
    private Sprite sprite_;

    [Tooltip("设定Sprite响应的Alpha阈值  大于等于这个值的透明度才有效")]
    [Range(0, 1.0f)]
    public float alpahThreshold = 0.5f;
    void Start()
    {
        image_ = GetComponent<Image>();
    }

    /// 重写IsRaycastLocationValid接口    
    public bool IsRaycastLocationValid(Vector2 vtor2, Camera main_Camera)
    {
        sprite_ = image_.sprite;
        var rectTransform = (RectTransform)transform;
        Vector2 localPositionPivotRelative;
        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform, vtor2, main_Camera, out localPositionPivotRelative); // 转换为以屏幕左下角为原点的坐标系
        var localPosition = new Vector2(localPositionPivotRelative.x + rectTransform.pivot.x * rectTransform.rect.width,
            localPositionPivotRelative.y + rectTransform.pivot.y * rectTransform.rect.height);
        var spriteRect = sprite_.textureRect;
        var maskRect = rectTransform.rect;
        var x = 0;
        var y = 0;        // 转换为纹理空间坐标
        switch (image_.type)
        {
            case Image.Type.Sliced:
                {
                    var border = sprite_.border;                    // x 轴裁剪
                    if (localPosition.x < border.x)
                    {
                        x = Mathf.FloorToInt(spriteRect.x + localPosition.x);
                    }
                    else if (localPosition.x > maskRect.width - border.z)
                    {
                        x = Mathf.FloorToInt(spriteRect.x + spriteRect.width - (maskRect.width - localPosition.x));
                    }
                    else
                    {
                        x = Mathf.FloorToInt(spriteRect.x + border.x +
                                             ((localPosition.x - border.x) /
                                             (maskRect.width - border.x - border.z)) *
                                             (spriteRect.width - border.x - border.z));
                    }                    // y 轴裁剪
                    if (localPosition.y < border.y)
                    {
                        y = Mathf.FloorToInt(spriteRect.y + localPosition.y);
                    }
                    else if (localPosition.y > maskRect.height - border.w)
                    {
                        y = Mathf.FloorToInt(spriteRect.y + spriteRect.height - (maskRect.height - localPosition.y));
                    }
                    else
                    {
                        y = Mathf.FloorToInt(spriteRect.y + border.y +
                                             ((localPosition.y - border.y) /
                                             (maskRect.height - border.y - border.w)) *
                                             (spriteRect.height - border.y - border.w));
                    }
                }
                break;
            case Image.Type.Simple:
            default:
                {                    // 转换为统一UV空间
                    x = Mathf.FloorToInt(spriteRect.x + spriteRect.width * localPosition.x / maskRect.width);
                    y = Mathf.FloorToInt(spriteRect.y + spriteRect.height * localPosition.y / maskRect.height);
                }
                break;
        }
        try
        {
            return sprite_.texture.GetPixel(x, y).a >= alpahThreshold;
        }
        catch (UnityException e)
        {
            Debug.LogError("请检查图片设置是不是已经勾选: Advanced/Read/Write  如果是图集请检查图集是否勾选了Advanced/Read/Write " + e.Message);
            Destroy(this);
            return false;
        }
    }

}