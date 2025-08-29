using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UISystem;

public class UIPanel_TestPageView : MonoBehaviour
{
    public class TestData
    {
        public int id;
        public string str;
    }

    [SerializeField]
    private UIPageView m_UIPageView;

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
        for (int i = 0; i < 10; i++) 
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
        m_UIPageView.Init(m_CellRTTemplate, OnCreateCell, OnShowCell);
        m_UIPageView.StartShow(m_DataList.Count, false);
    }

    //posIndex 和 dataIndex 在 开启循环后，可能不同
    private RectTransform OnCreateCell(int posIndex, int dataIndex)
    {
        var game = GameObject.Instantiate<GameObject>(m_CellRTTemplate.gameObject);
        game.SetActive(true);
        RectTransform cellRT = game.GetComponent<RectTransform>();
        return cellRT;
    }

    //posIndex 和 dataIndex 在 开启循环后，可能不同
    private void OnShowCell(int posIndex, int dataIndex)
    {
        RectTransform cellRT = m_UIPageView.GetCellRTByPosIndex(posIndex);
    }
}
