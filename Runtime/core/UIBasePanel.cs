
using System;

using UnityEngine;

namespace UISystem
{
    public abstract class UIBasePanel : IUIForm, IPrefabRef, IUIPanelPool
    {
        #region 私有字段 Private Fields

        protected long _id;
        protected string _uiPath;
        protected bool _isInitialized;
        protected Canvas uiCanvas;
        public int sortingLayer { get; internal set; } = 0;
        private float _curTime;
        #endregion
        /// <summary>
        /// UI唯一标识
        /// </summary>
        public long Id => _id;

        /// <summary>
        /// ui加载的路径
        /// </summary>
        public string uiPath => _uiPath;

        /// <summary>
        /// 是否已初始化
        /// </summary>
        public bool IsInitialized => _isInitialized;

        /// <summary>
        /// UI层级
        /// </summary>
        public virtual E_UILayer UILayer { get; }

        /// <summary>
        /// 池缓存时间  默认5秒
        /// </summary>
        protected virtual float cacheTime => 5;

        /// <summary>
        /// 父面板
        /// </summary>
        public UIBasePanel Parent { get;protected set; } 

        /// <summary>
        /// 显示实体
        /// </summary>
        public GameObject owner { get; protected set; }

        /// <summary>
        /// 获取视图引用类型 - 用于类型识别和管理
        /// 抽象方法，强制子类提供具体的类型标识
        /// </summary>
        /// <returns>视图引用类型枚举</returns>
        public abstract E_UIViewType GetViewType();

        /// <summary>
        /// 是否是全屏遮挡UI
        /// 涉及到堆栈遮挡 非全屏或半透明的面板应该设置False
        /// </summary>
        public abstract bool IsFullScreen { get; }

        /// <summary>
        /// 绑定显示层和逻辑层 - 这是显示层和逻辑层解耦的关键
        /// </summary>
        /// <param name="prefabRef">显示层引用</param>
        /// <param name="prefabReference">Prefab引用组件</param>
        public abstract void Bind(PrefabReference prefabReference);

        /// <summary>
        /// 检测间隔时间
        /// </summary>
        /// <param name="intervalTime"></param>
        /// <returns></returns>
        public bool CheckCacheTime(float intervalTime)
        {
            _curTime -= intervalTime;
            if (_curTime <= 0)
            {
                _curTime = cacheTime;
                return true;
            }
            return false;
        }

        public virtual void OnAwake()
        {
            _isInitialized = true;
        }

        public virtual void OnClose()
        {

        }

        public void SetRoot(Transform root,bool asFirstSibling = false, bool asFull = true)
        {
            if(root != null && owner != null)
            {
                owner.transform.SetParent(root, false);
                if(asFirstSibling)
                {
                    owner.transform.SetAsFirstSibling();
                }
                // 获取RectTransform并重新设置锚点
                if (asFull && owner.TryGetComponent<RectTransform>(out var rectTransform))
                {
                    // 设置全屏锚点
                    rectTransform.anchorMin = Vector2.zero;        // 左下角 (0,0)
                    rectTransform.anchorMax = Vector2.one;         // 右上角 (1,1)
                    rectTransform.sizeDelta = Vector2.zero;        // 清零偏移
                    rectTransform.anchoredPosition = Vector2.zero; // 清零位置偏移
                }
                owner.transform.localPosition = Vector3.zero;
                owner.transform.localScale = Vector3.one;
            }
        }

        //刷新canvas layer层
        public void RefreshSort()
        {
            if (uiCanvas == null)
                uiCanvas = owner?.GetComponent<Canvas>();
            if(uiCanvas != null)
            {
                uiCanvas.sortingOrder = sortingLayer;
            }
        }

        public void SetLayerSort(int sortLayer)
        {
            sortingLayer = sortLayer;
            RefreshSort();
        }

        public virtual void OnDispose()
        {
            if (owner != null)
                GameObject.Destroy(owner);

            Parent = null;
            _isInitialized = false;
            //告诉GC这个对象不需要调用finalizer，可以更快回收
            GC.SuppressFinalize(this);
        }

        public virtual void OnEscape()
        {

        }

        public virtual void OnHide()
        {
            if(owner != null)
                owner.SetActive(false);
        }

        public virtual void OnPause()
        {
            OnHide();
        }

        public virtual void OnReconnect()
        {

        }

        public virtual void OnResume()
        {
            OnShow();
        }

        public virtual void OnShow()
        {
            _curTime = cacheTime;
            if (owner != null)
                owner.SetActive(true);
        }

        public virtual void SetData(System.Object data)
        {

        }
    }

}

