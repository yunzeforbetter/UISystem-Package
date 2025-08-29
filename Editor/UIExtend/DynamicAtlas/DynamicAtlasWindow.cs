using System.Collections.Generic;
using UISystem;
using UnityEditor;
using UnityEngine;

public class DynamicAtlasWindow : EditorWindow
{
    private static DynamicAtlasWindow _DynamicAtlasWindow;
    private DynamicAtlasGroup _mGroup = DynamicAtlasGroup.Size_256;
    private float scale = 0.2f;
    private List<Color32[]> texColorsList = new List<Color32[]>();
    private List<Texture2D> texList = new List<Texture2D>();
    private Color32[] mFillColor;
    private List<Texture2D> tempTex2DList;
    private bool isShowFreeAreas = false;
    private bool isRefreshFreeAreas = true;
    private float formPosY = 62;
    
    private Vector2 scrollPos1;
    private Vector2 scrollPos2;
    
    public static void ShowWindow(DynamicAtlasGroup mGroup)
    {
        if (_DynamicAtlasWindow == null)
        {
            _DynamicAtlasWindow = GetWindow<DynamicAtlasWindow>();
        }
        _DynamicAtlasWindow.Show();
        _DynamicAtlasWindow.Init(mGroup);
        _DynamicAtlasWindow.titleContent.text = "PackingAtlas";
    }
    public void Init(DynamicAtlasGroup mGroup)
    {
        _mGroup = mGroup;
    }
    public void OnGUI()
    {
//        if (EditorApplication.isPlaying == false)
//        {
//            _DynamicAtlasWindow.Close();
//            return;
//        }
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("--------------------------------------------------------------------------------");

        if ((int)_mGroup == 0)
            _mGroup = DynamicAtlasGroup.Size_256;
        DynamicAtlas dynamicAtlas = DynamicAtlasManager.Instance.GetDynamicAtlas(_mGroup);
        EditorGUILayout.LabelField("图集尺寸：" + dynamicAtlas.atlasWidth + " x " + dynamicAtlas.atlasHeight);
        EditorGUILayout.LabelField("--------------------------------------------------------------------------------");
        EditorGUILayout.EndVertical();
        GUILayout.Space(10);
        scale = EditorGUILayout.Slider(scale, 0.2f, 1);
        EditorGUILayout.EndHorizontal();

        scrollPos1 = EditorGUILayout.BeginScrollView(scrollPos1);
        List<RenderTexture> renderTexList = dynamicAtlas.renderTexList;
        int count = renderTexList.Count;
        
        EditorGUILayout.BeginHorizontal();
        float width = dynamicAtlas.atlasWidth * scale;
        float height = dynamicAtlas.atlasHeight * scale;
        for (int i = 0; i < count; i++)
        {
            float poxX = (i + 1) * 10 + i * dynamicAtlas.atlasWidth * scale;
            if (isShowFreeAreas)
            {
                DrawFreeArea(i, dynamicAtlas);
            }
            GUI.DrawTexture(new Rect(poxX, formPosY, width, height), renderTexList[i]);
            GUILayout.Space(width + 5);
        }
        EditorGUILayout.EndHorizontal();

        if (count > 0)
        {
            GUILayout.Space(height + 2 * formPosY);
        }

        Dictionary<string, GetImageData> map = dynamicAtlas.GetGetter();
        foreach (KeyValuePair<string,GetImageData> data in map)
        {
            var d = data.Value;
            GUILayout.Label(string.Format("Key:{0} Gray:{1} refCount:{2}", d.url, d.isGray, d.refCount), "sv_label_1");
        }
        EditorGUILayout.EndScrollView();

        if (isShowFreeAreas)
        {
            isRefreshFreeAreas = false;
        }
    }

    void DrawFreeArea(int index, DynamicAtlas dynamicAtlas)
    {
        Texture2D tex2D = null;
        if (texList.Count < index + 1)
        {
            tex2D = new Texture2D((int)_mGroup, (int)_mGroup, TextureFormat.ARGB32, false, true);
            texList.Add(tex2D);
            if (mFillColor == null)
            {
                mFillColor = tex2D.GetPixels32();
                for (int i = 0; i < mFillColor.Length; ++i)
                    mFillColor[i] = Color.clear;
            }
        }
        else
        {
            tex2D = texList[index];
        }
        tex2D.SetPixels32(mFillColor);
        if (isRefreshFreeAreas)
        {
            Color32[] tmpColor;
            Dictionary<string, IntegerRectangle> freeList = dynamicAtlas.GetFreeAreas()[1].GetRectangleMap();
            foreach (IntegerRectangle item in freeList.Values)
            {
                int size = item.width * item.height;
                tmpColor = new Color32[size];
                for (int k = 0; k < size; ++k)
                {
                    tmpColor[k] = Color.green;//画边
                }
                tex2D.SetPixels32(item.x, item.y, item.width, item.height, tmpColor);
                int outLineSize = 2;
                if (item.width < outLineSize * 2 || item.height < outLineSize * 2)
                {
                    outLineSize = 0;
                }

                size -= outLineSize * 4;
                tmpColor = new Color32[size];
                for (int k = 0; k < size; ++k)
                {
                    tmpColor[k] = Color.yellow;
                }
                tex2D.SetPixels32(item.x + outLineSize, item.y + outLineSize, item.width - outLineSize * 2, item.height - outLineSize * 2, tmpColor);
                tex2D.Apply();
            }
        }

        float poxX = (index + 1) * 10 + index * dynamicAtlas.atlasWidth * scale;
        GUI.DrawTexture(new Rect(poxX, formPosY, dynamicAtlas.atlasWidth * scale, dynamicAtlas.atlasHeight * scale), tex2D);
    }
}
