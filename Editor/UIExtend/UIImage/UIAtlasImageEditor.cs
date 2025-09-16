using Sirenix.OdinInspector.Editor;
using UISystem;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[CustomEditor(typeof(UIAtlasImage), true)]
public class UIAtlasImageEditor : OdinEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        UIAtlasImage atlasImage = target as UIAtlasImage;
        if (atlasImage == null) return;

        if (GUILayout.Button("SetNativeSize"))
        {
            atlasImage.SetNativeSize();
        }
    }

    public override bool HasPreviewGUI()
    {
        return true;
    }


    public override void OnPreviewGUI(Rect rect, GUIStyle background)
    {
        Image image = target as Image;
        if (image == null) return;

        Sprite sf = image.sprite;
        if (sf == null) return;
        UnityEditor.UI.SpriteDrawUtilityCopy.DrawSprite(sf, rect, image.canvasRenderer.GetColor());
    }
}