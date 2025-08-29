using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UISystem;

public class UIPanel_TestListView : MonoBehaviour
{
    public class TestData
    {
        public int id;
        public string str;
    }

    [SerializeField]
    private UIListView m_UIListView;

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
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            m_UIListView.StartShow(0);
        }
    }
    void StartShow()
    {
        m_UIListView.Init(m_CellRTTemplate, OnCreateCell, OnShowCell);
        var count = m_DataList.Count;//Random.Range(0, m_DataList.Count);
        Debug.LogWarning(count);
        m_UIListView.StartShow(count, false);
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
        RectTransform cellRT = m_UIListView.GetCellRTByPosIndex(posIndex);
    }
}
