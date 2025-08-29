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
        [MenuItem("GameObject/UI/Ex/UIGridView", false, 2)]
        public static void CreateGridView()
        {
            var templatePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/3rd/uiSystem/Editor/TemplatePrefab/UIGridView.prefab");
            if(templatePrefab == null)
            {
                Debug.LogError($"加载失败 请检查 Assets/3rd/uiSystem/Editor/TemplatePrefab/UIGridView.prefab 是否存在");
                return;
            }
            var prefab = GameObject.Instantiate(templatePrefab, Selection.activeGameObject?.transform);
            prefab.transform.localScale = Vector3.one;
            prefab.transform.localPosition = Vector3.zero;
        }
        [MenuItem("GameObject/UI/Ex/UIListView", false, 1)]
        public static void CreateListView()
        {
            var templatePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/3rd/uiSystem/Editor/TemplatePrefab/UIListView.prefab");
            if (templatePrefab == null)
            {
                Debug.LogError($"加载失败 请检查 Assets/3rd/uiSystem/Editor/TemplatePrefab/UIListView.prefab 是否存在");
                return;
            }
            var prefab = GameObject.Instantiate(templatePrefab, Selection.activeGameObject?.transform);
            prefab.transform.localScale = Vector3.one;
            prefab.transform.localPosition = Vector3.zero;
        }

    }

}