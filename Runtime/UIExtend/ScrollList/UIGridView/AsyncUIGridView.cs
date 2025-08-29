using System;
using System.Collections.Generic;
using UnityEngine;

using static UISystem.UIScrollRect;

namespace UISystem
{
    /// <summary>
    /// 异步UIGridView组件 - 泛型版本，支持各种Panel类型
    /// </summary>
    [RequireComponent(typeof(UIGridView))]
    public class AsyncUIGridView : MonoBehaviour
    {
        [SerializeField] private UIGridView m_GridView;

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
            if (m_GridView == null)
            {
                m_GridView = GetComponent<UIGridView>();
            }
        }
#endif

        #region 初始化配置

        /// <summary>
        /// 配置GridView
        /// </summary>
        /// <typeparam name="TItem">Item Panel类型</typeparam>
        public void Configure<TItem>(string itemPath, Action<TItem, object, int> onUpdateItem) 
            where TItem : UIBasePanel
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

            // 配置UIGridView
            m_GridView.Init(OnCreateCell, OnShowCell);
            
            m_IsConfigured = true;
        }

        /// <summary>
        /// 配置GridView 
        /// </summary>
        public void Configure(string itemPath,System.Type itemPanelType,
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

            // 配置UIGridView
            m_GridView.Init(OnCreateCell, OnShowCell);
            
            m_IsConfigured = true;
        }

        #endregion

        #region 数据操作

        public void SetDataSource(System.Collections.IList dataList, bool resetPosition = true)
        {
            m_DataSource = dataList;
            RefreshGrid(resetPosition);
        }

        public void RefreshGrid(bool resetPosition = false)
        {
            int count = m_DataSource?.Count ?? 0;
            m_GridView.StartShow(count, !resetPosition);
        }

        public void RefreshItem(int index)
        {
            m_GridView.TryRefreshCellRT(index);
        }

        #endregion

        #region UIGridView回调

        /// <summary>
        /// 创建Cell回调 - 直接创建容器
        /// </summary>
        private RectTransform OnCreateCell(int index)
        {
            var container = m_ContainerManager.CreateContainer();
            return container.ContainerTransform;
        }

        /// <summary>
        /// 显示Cell回调 - 设置容器异步加载内容
        /// </summary>
        private void OnShowCell(int index)
        {
            if (!IsValidDataIndex(index)) return;

            // 获取对应的Cell
            if (!m_GridView.TryGetCellRT(index, out var cellRT)) return;

            var data = m_DataSource[index];

            // 获取或创建容器组件
            var container = m_ContainerManager.GetContainer(cellRT);
            if (container != null)
            {
                container.LoadContent(data, index, m_OnUpdateItem);
            }
        }

        #endregion

        #region 工具方法

        private bool IsValidDataIndex(int index)
        {
            return m_DataSource != null && index >= 0 && index < m_DataSource.Count;
        }

        /// <summary>
        /// 跳转到指定索引
        /// </summary>
        /// <param name="index">目标索引</param>
        /// <param name="immediately">是否立刻跳转</param>
        public void JumpTo(int index, bool immediately = false)
        {
            m_GridView.JumpTo(index, immediately);
        }

        /// <summary>
        /// 获取指定索引的Cell是否正在显示
        /// </summary>
        public bool IsCellShowing(int index)
        {
            return m_GridView.IsCellRTShowing(index);
        }

        /// <summary>
        /// 获取指定索引的Cell Transform
        /// </summary>
        public RectTransform GetCellTransform(int index)
        {
            return m_GridView.TryGetCellRT(index, out var cellRT) ? cellRT : null;
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
            
            // 清理UIGridView状态
            if (m_GridView != null)
            {
                m_GridView.Dispose();
            }
        }

        private void OnDestroy()
        {
            Clear();
        }

        #endregion
    }
}

