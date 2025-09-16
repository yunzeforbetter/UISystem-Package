using TMPro;

using UnityEditor;
using UnityEngine;

namespace UISystem.Editor
{
    /// <summary>
    /// UI组件创建扩展
    /// </summary>
    public class UICreateComponentEx
    {
        [MenuItem("GameObject/UI/Ex/UIButtonEx", false, 1)]
        public static void CreateButtonEx()
        {
            GameObject but = new GameObject("UIButtonEx");
            if (Selection.activeGameObject != null)
            {
                but.transform.SetParent(Selection.activeGameObject.transform);
            }
            but.transform.localScale = Vector3.one;
            but.transform.localPosition = Vector3.zero;
            but.AddComponent<UIAtlasImage>();
            UIButtonEx uiButtonEx = but.AddComponent<UIButtonEx>();

            GameObject textGo = new GameObject("Text");
            RectTransform rectTransform = textGo.GetComponent<RectTransform>();
            rectTransform.SetParent(but.transform, false);
            // 设置全屏
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;

            TextMeshProUGUI text = textGo.AddComponent<TextMeshProUGUI>();
            text.color = Color.black;
            text.alignment = TextAlignmentOptions.Midline;
            but.layer = LayerMask.NameToLayer("UI");
            textGo.layer = LayerMask.NameToLayer("UI");
        }

        [MenuItem("GameObject/UI/Ex/UIAtlasImage", false, 2)]
        public static void CreateUIAtlasImage()
        {
            GameObject go = new GameObject("UIAtlasImage");
            if (Selection.activeGameObject != null)
            {
                go.transform.SetParent(Selection.activeGameObject.transform);
            }
            go.transform.localScale = Vector3.one;
            go.transform.localPosition = Vector3.zero;
            UIAtlasImage uiAtlas = go.AddComponent<UIAtlasImage>();
            go.layer = LayerMask.NameToLayer("UI");
        }

        [MenuItem("GameObject/UI/Ex/DynamicRawImage", false, 3)]
        public static void CreateDynamicRawImage()
        {
            GameObject go = new GameObject("DynamicRawImage");
            if (Selection.activeGameObject != null)
            {
                go.transform.SetParent(Selection.activeGameObject.transform);
            }
            go.transform.localScale = Vector3.one;
            go.transform.localPosition = Vector3.zero;
            DynamicRawImage dynamicRaw = go.AddComponent<DynamicRawImage>();
            go.layer = LayerMask.NameToLayer("UI");
        }

        [MenuItem("GameObject/UI/Ex/UIListView", false, 4)]
        public static void CreateListView()
        {
            var templatePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Packages/com.framework.uisystem/Editor/TemplatePrefab/UIListView.prefab");
            if (templatePrefab == null)
            {
                Debug.LogError($"加载失败 请检查 Packages/com.framework.uisystem/Editor/TemplatePrefab/UIListView.prefab 是否存在");
                return;
            }
            var prefab = GameObject.Instantiate(templatePrefab, Selection.activeGameObject?.transform);
            prefab.transform.localScale = Vector3.one;
            prefab.transform.localPosition = Vector3.zero;
        }

        [MenuItem("GameObject/UI/Ex/UIGridView", false, 5)]
        public static void CreateGridView()
        {
            var templatePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Packages/com.framework.uisystem/Editor/TemplatePrefab/UIGridView.prefab");
            if(templatePrefab == null)
            {
                Debug.LogError($"加载失败 请检查 Packages/com.framework.uisystem/Editor/TemplatePrefab/UIGridView.prefab 是否存在");
                return;
            }
            var prefab = GameObject.Instantiate(templatePrefab, Selection.activeGameObject?.transform);
            prefab.transform.localScale = Vector3.one;
            prefab.transform.localPosition = Vector3.zero;
        }

    }

}