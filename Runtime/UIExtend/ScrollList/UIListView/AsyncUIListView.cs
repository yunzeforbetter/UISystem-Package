using System;
using System.Collections.Generic;
using UnityEngine;

namespace UISystem
{
    /// <summary>
    /// 异步UIListView组件 - 泛型版本，支持各种Panel类型
    /// </summary>
    [RequireComponent(typeof(UIListView))]
    public class AsyncUIListView : MonoBehaviour
    {
        [SerializeField] private UIListView m_ListView;

        /// <summary>
        /// Item Panel类型
        /// </summary>
        private System.Type m_ItemPanelType;

        /// <summary>
        /// 数据源
        /// </summary>
        private System.Collections.IList m_DataSource;

        /// <summary>
        /// Item容器管理器
        /// </summary>
        private AsyncItemContainerManager m_ContainerManager;

        /// <summary>
        /// 数据更新回调
        /// </summary>
        private Action<UIBasePanel, object, int> m_OnUpdateItem;

        /// <summary>
        /// 是否已配置
        /// </summary>
        private bool m_IsConfigured;

#if UNITY_EDITOR
        void OnValidate()
        {
            if (m_ListView == null)
            {
                m_ListView = GetComponent<UIListView>();
            }
        }
#endif

        #region 初始化配置

        /// <summary>
        /// 配置ListView
        /// </summary>
        /// <typeparam name="TItem">Item Panel类型</typeparam>
        public void Configure<TItem>(string itemPath, Action<TItem, object, int> onUpdateItem) where TItem : UIBasePanel
        {
            // 防止重复初始化
            if (m_IsConfigured)
            {
                Clear(); // 先清理一次
            }

            m_ItemPanelType = typeof(TItem);
            // 包装回调函数
            m_OnUpdateItem = (panel, data, index) =>
            {
                if (panel is TItem typedPanel)
                {
                    onUpdateItem?.Invoke(typedPanel, data, index);
                }
            };

            // 初始化容器管理器
            m_ContainerManager = new AsyncItemContainerManager(itemPath, m_ItemPanelType);

            // 配置UIListView
            m_ListView.Init(OnCreateCell, OnShowCell);
            
            m_IsConfigured = true;
        }

        /// <summary>
        /// 配置ListView - 非泛型版本（兼容旧代码）
        /// </summary>
        public void Configure(string itemPath, System.Type itemPanelType,
            Action<UIBasePanel, object, int> onUpdateItem)
        {
            // 防止重复初始化
            if (m_IsConfigured)
            {
                Clear(); // 先清理一次
            }

            m_ItemPanelType = itemPanelType ?? typeof(UIBasePanel);
            m_OnUpdateItem = onUpdateItem;

            // 初始化容器管理器
            m_ContainerManager = new AsyncItemContainerManager(itemPath, m_ItemPanelType);

            // 配置UIListView
            m_ListView.Init(OnCreateCell, OnShowCell);
            
            m_IsConfigured = true;
        }

        #endregion

        #region 数据操作

        public void SetDataSource(System.Collections.IList dataList, bool resetPosition = true)
        {
            m_DataSource = dataList;
            RefreshList(resetPosition);
        }

        public void RefreshList(bool resetPosition = false)
        {
            int count = m_DataSource?.Count ?? 0;
            m_ListView.StartShow(count, !resetPosition);
        }

        public void RefreshItem(int index)
        {
            m_ListView.TryRefreshCellRT(index);
        }

        #endregion

        #region UIListView回调

        /// <summary>
        /// 创建Cell回调 - 直接创建容器
        /// </summary>
        private RectTransform OnCreateCell(int posIndex, int dataIndex)
        {
            var container = m_ContainerManager.CreateContainer();
            return container.ContainerTransform;
        }

        /// <summary>
        /// 显示Cell回调 - 设置容器异步加载内容
        /// </summary>
        private void OnShowCell(int posIndex, int dataIndex)
        {
            var cellRT = m_ListView.GetCellRTByPosIndex(posIndex);
            if (cellRT == null) return;

            var validIndex = GetValidDataIndex(dataIndex);
            if (validIndex < 0 || validIndex >= m_DataSource?.Count) return;

            var data = m_DataSource[validIndex];

            // 获取或创建容器组件
            var container = m_ContainerManager.GetContainer(cellRT);
            if (container != null)
            {
                container.LoadContent(data, validIndex, m_OnUpdateItem);
            }
        }

        #endregion

        #region 工具方法

        private int GetValidDataIndex(int dataIndex)
        {
            if (m_DataSource == null || m_DataSource.Count == 0) return -1;

            if (m_ListView.loop)
            {
                return ((dataIndex % m_DataSource.Count) + m_DataSource.Count) % m_DataSource.Count;
            }

            return dataIndex >= 0 && dataIndex < m_DataSource.Count ? dataIndex : -1;
        }

        #endregion

        #region 清理方法

        /// <summary>
        /// 清理所有状态
        /// </summary>
        public void Clear()
        {
            // 清理数据源和回调
            m_DataSource = null;
            m_OnUpdateItem = null;

            // 清理容器管理器
            m_ContainerManager?.Dispose();
            m_ContainerManager = null;
            
            // 重置配置状态
            m_ItemPanelType = null;
            m_IsConfigured = false;
            
            // 清理UIListView状态
            if (m_ListView != null)
            {
                m_ListView.Dispose(); 
            }
        }

        private void OnDestroy()
        {
            Clear();
        }

        #endregion
    }


}