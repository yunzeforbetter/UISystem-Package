using System.Collections.Generic;
using System;
using UnityEngine;

namespace UISystem
{
    /// <summary>
    /// 异步Item容器管理器
    /// 用于异步加载Item时需要同步返回的情况，先将容器返回
    /// </summary>
    public class AsyncItemContainerManager : IDisposable
    {
        private readonly string m_ItemPath;
        private readonly System.Type m_ItemPanelType;
        private readonly Dictionary<RectTransform, AsyncItemContainer> m_ContainerDict;
        private readonly Stack<AsyncItemContainer> m_ContainerPool;

        public AsyncItemContainerManager(string itemPath, System.Type itemPanelType)
        {
            m_ItemPath = itemPath;
            m_ItemPanelType = itemPanelType;
            m_ContainerDict = new Dictionary<RectTransform, AsyncItemContainer>();
            m_ContainerPool = new Stack<AsyncItemContainer>();
        }

        /// <summary>
        /// 创建容器
        /// </summary>
        public AsyncItemContainer CreateContainer()
        {
            AsyncItemContainer container;

            if (m_ContainerPool.Count > 0)
            {
                container = m_ContainerPool.Pop();
                container.Reset();
            }
            else
            {
                container = new AsyncItemContainer(m_ItemPath, m_ItemPanelType);
            }

            m_ContainerDict[container.ContainerTransform] = container;
            return container;
        }

        /// <summary>
        /// 获取容器
        /// </summary>
        public AsyncItemContainer GetContainer(RectTransform containerRT)
        {
            m_ContainerDict.TryGetValue(containerRT, out var container);
            return container;
        }

        /// <summary>
        /// 回收容器
        /// </summary>
        public void RecycleContainer(AsyncItemContainer container)
        {
            if (container != null)
            {
                m_ContainerDict.Remove(container.ContainerTransform);
                container.Recycle();
                m_ContainerPool.Push(container);
            }
        }

        public void Dispose()
        {
            foreach (var container in m_ContainerDict.Values)
            {
                container?.Dispose();
            }

            while (m_ContainerPool.Count > 0)
            {
                m_ContainerPool.Pop()?.Dispose();
            }

            m_ContainerDict.Clear();
            m_ContainerPool.Clear();
        }
    }

    /// <summary>
    /// 异步Item容器 - 支持运行时动态确认
    /// </summary>
    public class AsyncItemContainer : IDisposable
    {
        private readonly string m_ItemPath;
        private readonly System.Type m_ItemPanelType;
        private GameObject m_ContainerObject;
        private RectTransform m_ContainerTransform;
        private UIBasePanel m_CurrentItem;
        private bool m_IsLoading;
        private LoadRequest m_PendingRequest;
        private bool m_IsDisposed;

        public RectTransform ContainerTransform => m_ContainerTransform;
        public UIBasePanel CurrentItem => m_CurrentItem;
        public bool IsLoading => m_IsLoading;

        public AsyncItemContainer(string itemPath, System.Type itemPanelType)
        {
            m_ItemPath = itemPath;
            m_ItemPanelType = itemPanelType;
            CreateContainer();
        }

        /// <summary>
        /// 创建容器GameObject
        /// </summary>
        private void CreateContainer()
        {
            m_ContainerObject = new GameObject("ItemContainer");
            m_ContainerTransform = m_ContainerObject.AddComponent<RectTransform>();

            var containerComponent = m_ContainerObject.AddComponent<ItemContainerComponent>();
            containerComponent.SetContainer(this);
        }

        /// <summary>
        /// 加载内容
        /// </summary>
        public void LoadContent(object data, int index, Action<UIBasePanel, object, int> onUpdateCallback)
        {
            if (m_CurrentItem != null)
            {
                onUpdateCallback?.Invoke(m_CurrentItem, data, index);
                return;
            }

            if (m_IsLoading)
            {
                m_PendingRequest = new LoadRequest { Data = data, Index = index, Callback = onUpdateCallback };
                return;
            }

            StartAsyncLoad(data, index, onUpdateCallback);
        }

        /// <summary>
        /// 开始异步加载 
        /// </summary>
        private void StartAsyncLoad(object data, int index, Action<UIBasePanel, object, int> onUpdateCallback)
        {
            m_IsLoading = true;
            UIManager.Instance.DoOpen(m_ItemPanelType, m_ItemPath, data, (obj, panel) =>
            {
                m_IsLoading = false;

                if (panel != null && m_ContainerObject != null)
                {
                    // 将Item设置为容器的子对象
                    panel.owner.transform.SetParent(m_ContainerTransform, false);

                    // 设置Item布局参数
                    var itemRT = panel.owner.GetComponent<RectTransform>();
                    if (itemRT != null)
                    {
                        itemRT.anchorMin = Vector2.zero;
                        itemRT.anchorMax = Vector2.one;
                        itemRT.sizeDelta = Vector2.zero;
                        itemRT.anchoredPosition = Vector2.zero;
                    }

                    m_CurrentItem = panel;
                    // 处理待处理请求
                    if (m_PendingRequest != null)
                    {
                        onUpdateCallback?.Invoke(panel, m_PendingRequest.Data, m_PendingRequest.Index);
                        m_PendingRequest = null;
                    }
                    else
                    {
                        // 执行回调
                        onUpdateCallback?.Invoke(panel, data, index);
                    }
                }
            });

        }

        /// <summary>
        /// 重置容器
        /// </summary>
        public void Reset()
        {
            if (m_ContainerObject != null)
            {
                m_ContainerObject.SetActive(true);
            }
        }

        /// <summary>
        /// 回收容器
        /// </summary>
        public void Recycle()
        {
            if (m_IsDisposed) return; // 已释放的容器不进行回收操作

            if (m_CurrentItem != null)
            {
                UIManager.Instance.CloseUI(m_CurrentItem);
                m_CurrentItem = null;
            }

            if (m_ContainerObject != null)
            {
                m_ContainerObject.SetActive(false);
            }

            m_PendingRequest = null;
        }

        public void Dispose()
        {
            if (m_IsDisposed) return; // 防止重复释放

            m_IsDisposed = true;

            if (m_CurrentItem != null)
            {
                UIManager.Instance.CloseUI(m_CurrentItem);
                m_CurrentItem = null;
            }

            if (m_ContainerObject != null)
            {
                // 先清除ItemContainerComponent的引用，防止双重释放
                var containerComponent = m_ContainerObject.GetComponent<ItemContainerComponent>();
                if (containerComponent != null)
                {
                    containerComponent.ClearContainer();
                }

                UnityEngine.Object.Destroy(m_ContainerObject);
                m_ContainerObject = null;
                m_ContainerTransform = null;
            }

            m_PendingRequest = null;
        }

        private class LoadRequest
        {
            public object Data;
            public int Index;
            public Action<UIBasePanel, object, int> Callback;
        }
    }

    /// <summary>
    /// 容器组件
    /// </summary>
    public class ItemContainerComponent : MonoBehaviour
    {
        private AsyncItemContainer m_Container;

        public void SetContainer(AsyncItemContainer container)
        {
            m_Container = container;
        }

        public AsyncItemContainer GetContainer()
        {
            return m_Container;
        }

        /// <summary>
        /// 清除容器引用，防止双重释放
        /// </summary>
        public void ClearContainer()
        {
            m_Container = null;
        }

        private void OnDestroy()
        {
            m_Container?.Dispose();
        }
    }
}