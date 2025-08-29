using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UISystem;

public class UIPanel_TestGridView : MonoBehaviour
{
    public class TestData
    {
        public int id;
        public string str;
    }

    [SerializeField]
    private UIGridView m_UIGridView;

    [SerializeField]
    private RectTransform m_CellRTTemplate;
    
    [SerializeField]
    private Button m_BtnRefresh;

    private List<TestData> m_DataList;

    void Awake()
    {
        m_BtnRefresh.onClick.AddListener(() => {
            StartShow();
        });
    }

    void Start()
    {
        //创建数据列表
        m_DataList = new List<TestData>();
        for (int i = 0; i < 100; i++) 
        {
            m_DataList.Add(new TestData() 
            { 
                id = i,
                str = "hello " + i 
            }); 
        }

        StartShow();
    }



    void StartShow()
    {
        m_UIGridView.Init(m_CellRTTemplate, OnCreateCell, OnShowCell);
        var count = 1000;//Random.Range(0, m_DataList.Count);
        Debug.LogWarning(count);
        m_UIGridView.StartShow(count, false);
    }

    private RectTransform OnCreateCell(int index)
    {
        var game = GameObject.Instantiate<GameObject>(m_CellRTTemplate.gameObject);
        game.SetActive(true);
        RectTransform cellRT = game.GetComponent<RectTransform>();
        return cellRT;
    }

    private void OnShowCell(int index)
    {
        RectTransform cellRT = m_UIGridView.GetCellRT(index);
    }
}
