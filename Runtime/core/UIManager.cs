using System;
using System.Collections.Generic;
using UnityEngine;

namespace UISystem
{
    /// <summary>
    /// UI管理器
    /// </summary>
    public class UIManager
    {
        #region 单例模式 Singleton Pattern

        private static UIManager instance;
        public static UIManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new UIManager();
                }
                return instance;
            }
        }

        #endregion

        #region 私有字段 Private Fields

        // 所有UI面板字典 - 按ID索引
        private Dictionary<long, UIBasePanel> allPanelDic = new Dictionary<long, UIBasePanel>();

        // UI窗口界面字典 - 路径匹配 窗口同时仅会打开一个
        private Dictionary<string, UIBasePanel> uiPanelDic = new Dictionary<string, UIBasePanel>();

        // UI层级管理 - 每个层级维护一个UI栈
        private Dictionary<E_UILayer, Stack<UIBasePanel>> layerStackDic = new Dictionary<E_UILayer, Stack<UIBasePanel>>();

        // UI层级根节点 - 用于控制显示层级
        private Dictionary<E_UILayer, Transform> layerRootDic = new Dictionary<E_UILayer, Transform>();

        // UI根Canvas
        private Canvas uiCanvas;

        public IResourceManager resourceMgr { get; private set; } //资源加载器

        #endregion

        #region 构造函数 Constructor

        /// <summary>
        /// 私有构造函数
        /// </summary>
        private UIManager()
        {
            InitializeLayerStacks();
        }

        #endregion

        #region 初始化方法 Initialization Methods

        /// <summary>
        /// 初始化层级栈
        /// </summary>
        private void InitializeLayerStacks()
        {
            var layers = Enum.GetValues(typeof(E_UILayer));
            foreach (E_UILayer layer in layers)
            {
                layerStackDic[layer] = new Stack<UIBasePanel>();
            }
        }


        /// <summary>
        /// 设置UI根Canvas和层级
        /// </summary>
        /// <param name="canvas">UI根Canvas</param>
        public void SetUIRoot(Canvas canvas)
        {
            uiCanvas = canvas;
            InitializeLayerRoots();
        }

        /// <summary>
        /// 设置资源加载器
        /// </summary>
        /// <param name="mgr"></param>
        public void SetResourcesMgr(IResourceManager mgr)
        {
            resourceMgr = mgr;
        }

        public Canvas GetRootCanvas()
        {
            return uiCanvas;
        }

        /// <summary>
        /// 初始化层级根节点
        /// </summary>
        private void InitializeLayerRoots()
        {
            if (uiCanvas == null) return;

            var layers = Enum.GetValues(typeof(E_UILayer));
            foreach (E_UILayer layer in layers)
            {
                GameObject layerRoot = new GameObject($"Layer_{layer}");
                layerRoot.AddComponent<CanvasGroup>();
                RectTransform rectTransform = layerRoot.AddComponent<RectTransform>();
                rectTransform.SetParent(uiCanvas.transform, false);
                
                // 设置全屏
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.sizeDelta = Vector2.zero;
                rectTransform.anchoredPosition = Vector2.zero;
                layerRoot.layer = LayerMask.NameToLayer("UI");
                layerRootDic[layer] = rectTransform;
            }
        }

        #endregion

        #region UI操作方法 UI Operation Methods

       /// <summary>
       /// 打开面板
       /// </summary>
       /// <typeparam name="T"></typeparam>
       /// <param name="uiPath"></param>
       /// <param name="data"></param>
        public void OpenPanel<T>(string uiPath,System.Object data = null) where T : UIBasePanel
        {
            DoOpen<T>(uiPath, data, (owner, panel) =>
            {
                if (panel != null)
                {
                    var root = GetLayerRoot(panel.UILayer);
                    panel.SetRoot(root);
                    //打开面板的时候检测有无高斯模糊
                    owner.GetComponent<UiGaussAbs>()?.OnGaussBg(owner, panel);
                }
            });
        }

        public void DoOpen<T>(string uiPath, System.Object data = null, Action<GameObject, UIBasePanel> onCallBack = null) where T : UIBasePanel
        {
            if (string.IsNullOrEmpty(uiPath))
            {
                Debug.LogError(">> DoOpen ui fail  uiPath is null");
                return;
            }
            //如果是面板ui
            if(uiPanelDic.TryGetValue(uiPath,out var value))
            {
                //先关闭再打开
                CloseUI(value);
            }
            //优先检查缓存里面是否有可用
            UIBasePanel uiForm = UIPanelObjectPools.Instance.Pop(uiPath);
            //没有的话开始加载资源
            if (uiForm == null)
            {
                var handler = resourceMgr.InstantiateHandle(uiPath, data);
                handler.Callback = (obj) =>
                {
                    //加载失败
                    if(obj == null)
                    {
                        onCallBack?.Invoke(null,null);
                        return;
                    }     
                    // 生成UI ID
                    long uiId = GenerateUIId();
                    // 使用工厂方法创建UI实例
                    uiForm = (T)Activator.CreateInstance(typeof(T), uiId, uiPath);
                    var prefabReference = obj.GetComponent<PrefabReference>();
                    if(prefabReference == null)
                    {
                        Debug.LogError($"绑定异常  {uiPath} 没有挂载 PrefabReference 组件！！");
                    }
                    //绑定显示层
                    uiForm.Bind(prefabReference);
                    // 注册UI
                    RegisterUI(uiForm);
                    // 首次加载才会调用
                    uiForm.OnAwake();
                    uiForm.SetData(data);
                    uiForm.OnShow();
                    onCallBack?.Invoke(uiForm.owner, uiForm);
                };
            }
            else
            {
                // 注册UI
                RegisterUI(uiForm);
                uiForm.SetData(data);
                uiForm.OnShow();
                onCallBack?.Invoke(uiForm.owner, uiForm);
            }
            
        }

        public void DoOpen(Type type,string uiPath, System.Object data = null, Action<GameObject, UIBasePanel> onCallBack = null)
        {
            if (string.IsNullOrEmpty(uiPath))
            {
                Debug.LogError(">> DoOpen ui fail  uiPath is null");
                return;
            }
            //如果是面板ui
            if (uiPanelDic.TryGetValue(uiPath, out var value))
            {
                //先关闭再打开
                CloseUI(value);
            }
            //优先检查缓存里面是否有可用
            UIBasePanel uiForm = UIPanelObjectPools.Instance.Pop(uiPath);
            //没有的话开始加载资源
            if (uiForm == null)
            {
                var handler = resourceMgr.InstantiateHandle(uiPath, data);
                handler.Callback = (obj) =>
                {
                    //加载失败
                    if (obj == null)
                    {
                        onCallBack?.Invoke(null, null);
                        return;
                    }
                    // 生成UI ID
                    long uiId = GenerateUIId();
                    // 使用工厂方法创建UI实例
                    uiForm = Activator.CreateInstance(type, uiId, uiPath) as UIBasePanel;
                    if(uiForm == null)
                    {
                        onCallBack?.Invoke(null, null);
                        return;
                    }
                    //绑定显示层
                    uiForm.Bind(obj.GetComponent<PrefabReference>());
                    // 注册UI
                    RegisterUI(uiForm);
                    // 首次加载才会调用
                    uiForm.OnAwake();
                    uiForm.SetData(data);
                    uiForm.OnShow();
                    onCallBack?.Invoke(uiForm.owner, uiForm);
                };
            }
            else
            {
                // 注册UI
                RegisterUI(uiForm);
                uiForm.SetData(data);
                uiForm.OnShow();
                onCallBack?.Invoke(uiForm.owner, uiForm);
            }

        }
        /// <summary>
        /// 关闭UI
        /// </summary>
        /// <param name="uiId">UI</param>
        public void CloseUI(long uiId)
        {
            if (allPanelDic.TryGetValue(uiId, out UIBasePanel uiForm))
            {
                CloseUI(uiForm);
            }
        }

        /// <summary>
        /// 关闭UI
        /// </summary>
        /// <param name="uiType">UI类型</param>
        public void CloseUI(string uiPath)
        {
            if (uiPanelDic.TryGetValue(uiPath, out UIBasePanel uiForm))
            {
                CloseUI(uiForm);
            }
        }

        /// <summary>
        /// 关闭UI
        /// </summary>
        /// <param name="uiForm">UI实例</param>
        public void CloseUI(UIBasePanel uiForm)
        {
            if (uiForm == null) return;
            try
            {
                // 调用生命周期方法
                uiForm.OnClose();
                // 取消注册
                UnregisterUI(uiForm);
                //入池
                UIPanelObjectPools.Instance?.Push(uiForm, uiForm.uiPath);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UIManager] 关闭UI时发生错误: {ex.Message} / Error occurred while closing UI: {ex.Message}");
            }
        }

        /// <summary>
        /// 隐藏UI
        /// </summary>
        /// <param name="uiId">UI</param>
        public void HideUI(long uiId)
        {
            if (allPanelDic.TryGetValue(uiId, out UIBasePanel uiForm))
            {
                uiForm.OnHide();
            }
        }

        /// <summary>
        /// 显示UI
        /// </summary>
        /// <param name="uiId">UI</param>
        public void ShowUI(long uiId)
        {
            if (allPanelDic.TryGetValue(uiId, out UIBasePanel uiForm))
            {
                uiForm.OnShow();
            }
        }

        #endregion

        #region 辅助方法 Helper Methods

        // UI ID生成器
        private long uiIdGenerator = 0;
        /// <summary>
        /// 生成UI ID
        /// </summary>
        /// <returns>UI ID</returns>
        private long GenerateUIId()
        {
            return ++uiIdGenerator;
        }

        /// <summary>
        /// 注册UI - 改进的类型识别和管理逻辑
        /// 根据UI视图类型进行不同的注册处理，提供类型安全的管理机制
        /// </summary>
        /// <param name="uiForm">UI实例</param>
        private void RegisterUI(UIBasePanel uiForm)
        {
            // 将UI添加到全局管理字典
            allPanelDic[uiForm.Id] = uiForm;
            // 使用明确的类型检查替代模糊的类型转换
            switch (uiForm.GetViewType())
            {
                case E_UIViewType.Panel:
                    // 只有面板类型才记录路径关联，因为面板是唯一的且需要路径管理
                    uiPanelDic[uiForm.uiPath] = uiForm;
                    // 只有面板需要添加到层级堆栈管理
                    AddToLayerStack(uiForm);

                    var stack = layerStackDic[uiForm.UILayer];
                    //刷新sort layer
                    int layer = (int)uiForm.UILayer;
                    foreach (var item in stack)
                    {
                        if (item == null)
                            continue;
                        layer += 20;
                        item.SetLayerSort(layer);
                    }
                    break;
                case E_UIViewType.Item:
                    // 项类型不需要路径关联，因为它们通常是批量创建的
                    // 项类型也不需要层级堆栈管理，因为它们通常由面板管理
                    // 可以在这里添加特定的项管理逻辑，如对象池管理
                    break;
            }


        }

        /// <summary>
        /// 取消注册UI - 改进的类型识别和清理逻辑
        /// 根据UI视图类型进行不同的清理处理，确保资源正确释放
        /// </summary>
        /// <param name="uiForm">UI实例</param>
        private void UnregisterUI(UIBasePanel uiForm)
        {
            // 从全局管理字典中移除
            allPanelDic.Remove(uiForm.Id);
            // 根据UI类型进行不同的清理处理
            switch (uiForm.GetViewType())
            {
                case E_UIViewType.Panel:
                    // 只有面板类型需要清理路径关联和层级堆栈
                    uiPanelDic.Remove(uiForm.uiPath);
                    RemoveFromLayerStack(uiForm);
                    uiForm.sortingLayer = 0;
                    break;

                case E_UIViewType.Item:
                    // 项类型只需要基础清理，不涉及路径和层级管理
                    // 可以在这里添加特定的项清理逻辑，如返回对象池
                    break;
            }
        }

        /// <summary>
        /// 添加到层级栈
        /// </summary>
        /// <param name="uiForm">UI实例</param>
        private void AddToLayerStack(UIBasePanel uiForm)
        {
            var layer = uiForm.UILayer;
            var stack = layerStackDic[layer];

            // 暂停当前栈顶UI
            if (stack.Count > 0)
            {
                if(uiForm.IsFullScreen)
                    stack.Peek().OnPause();
            }

            stack.Push(uiForm);
        }

        /// <summary>
        /// 从层级栈移除
        /// </summary>
        /// <param name="uiForm">UI实例 UI instance</param>
        private void RemoveFromLayerStack(UIBasePanel uiForm)
        {
            var layer = uiForm.UILayer;
            var stack = layerStackDic[layer];

            // 如果是栈顶UI
            if (stack.Count > 0 && stack.Peek() == uiForm)
            {
                stack.Pop();

                // 恢复新的栈顶UI
                if (stack.Count > 0)
                {
                    if (uiForm.IsFullScreen)
                        stack.Peek().OnResume();
                }
            }
            else
            {
                // 如果不是栈顶，需要从栈中找到并移除
                var tempStack = new Stack<UIBasePanel>();

                while (stack.Count > 0)
                {
                    var current = stack.Pop();
                    if (current == uiForm)
                    {
                        break;
                    }
                    tempStack.Push(current);
                }

                // 恢复栈
                while (tempStack.Count > 0)
                {
                    stack.Push(tempStack.Pop());
                }
            }
        }

        #endregion

        #region 查询方法 Query Methods

        /// <summary>
        /// 获取层级根节点
        /// </summary>
        /// <param name="layer">层级 Layer</param>
        /// <returns>层级根节点</returns>
        public Transform GetLayerRoot(E_UILayer layer)
        {
            return layerRootDic.TryGetValue(layer, out Transform root) ? root : null;
        }

        /// <summary>
        /// 获取UI实例
        /// </summary>
        /// <param name="uiId">UI</param>
        /// <returns>UI实例</returns>
        public UIBasePanel GetUI(long uiId)
        {
            return allPanelDic.TryGetValue(uiId, out UIBasePanel uiForm) ? uiForm : null;
        }

        /// <summary>
        /// 获取UI实例
        /// </summary>
        /// <param name="uiType">UI类型</param>
        /// <returns>UI实例</returns>
        public UIBasePanel GetUI(string uiPath)
        {
            return uiPanelDic.TryGetValue(uiPath, out UIBasePanel uiForm) ? uiForm : null;
        }

         /// <summary>
         /// 检查UI是否已打开
         /// </summary>
         /// <param name="uiType">UI类型</param>
         /// <returns>是否已打开</returns>
         public bool IsUIOpened(string uiPath)
         {
             UIBasePanel ui = GetUI(uiPath);
             return ui!= null && ui.IsInitialized;
         }

        #endregion

        #region 清理方法 Cleanup Methods

        /// <summary>
        /// 关闭所有UI
        /// </summary>
        public void CloseAllUI()
        {
            var uiList = new List<UIBasePanel>(allPanelDic.Values);
            foreach (var ui in uiList)
            {
                CloseUI(ui);
            }
        }

        /// <summary>
        /// 释放全部UI
        /// </summary>
        public void Cleanup()
        {
            CloseAllUI();
            
            allPanelDic.Clear();
            uiPanelDic.Clear();
            
            foreach (var stack in layerStackDic.Values)
            {
                stack.Clear();
            }
            uiIdGenerator = 0;
        }

        #endregion
    }
}
