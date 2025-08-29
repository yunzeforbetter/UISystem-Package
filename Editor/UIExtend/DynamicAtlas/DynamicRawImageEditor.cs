using UnityEditor;
using UnityEditor.UI;

using UnityEngine;

[CustomEditor(typeof(DynamicRawImage))]
public class DynamicRawImageEditor : Editor
{
	public override void OnInspectorGUI()
	{
        base.OnInspectorGUI();
        
        DynamicRawImage icon = target as DynamicRawImage;
        if(icon == null)
	        return;

        if (GUILayout.Button("查看动态图集"))
        {
	        DynamicAtlasWindow.ShowWindow(icon.GetGroup());   
        }
	}
	
	private static Rect Outer(DynamicRawImage rawImage)
	{
		Rect outer = rawImage.uvRect;
		outer.xMin *= rawImage.rectTransform.rect.width;
		outer.xMax *= rawImage.rectTransform.rect.width;
		outer.yMin *= rawImage.rectTransform.rect.height;
		outer.yMax *= rawImage.rectTransform.rect.height;
		return outer;
	}
	
	public override bool HasPreviewGUI()
	{
		return true;
	}

	
	public override void OnPreviewGUI(Rect rect, GUIStyle background)
	{
		DynamicRawImage rawImage = target as DynamicRawImage;
		Texture tex = rawImage.mainTexture;

		if (tex == null)
			return;

		var outer = Outer(rawImage);
		SpriteDrawUtilityCopy.DrawSprite(tex, rect, outer, rawImage.uvRect, rawImage.canvasRenderer.GetColor());
	}
}
