using System;
using System.Collections.Generic;
using UnityEngine;

namespace UISystem
{
    /// <summary>
    /// UI面板视图引用类 - 专门用于管理独立UI面板的完整生命周期
    /// 面板级UI具有独立性、层级管理、子面板管理等特性
    /// 适用于：主界面、弹窗、设置界面等独立功能模块
    /// </summary>
    /// <typeparam name="T">实现IPrefabRef接口的面板显示层引用类型</typeparam>
    public abstract class UIPanelViewRef<T> : UIViewRefBase<T> where T : IPrefabRef, new()
    {
        /// <summary>
        /// 子面板集合 - 管理当前面板下的所有子UI面板
        /// 使用Dictionary提供O(1)的查找性能，key为子面板ID
        /// </summary>
        private Dictionary<long, UIBasePanel> _children;
        
        /// <summary>
        /// 子面板访问器 - 提供只读访问，防止外部直接修改集合
        /// </summary>
        public Dictionary<long, UIBasePanel> Children => _children ??= new Dictionary<long, UIBasePanel>();

        /// <summary>
        /// 带参数构造函数 - 创建具有明确路径的面板
        /// </summary>
        /// <param name="id">面板唯一标识符</param>
        /// <param name="uiPath">面板资源路径</param>
        public UIPanelViewRef(long id, string uiPath) : base(id, uiPath) { }

        /// <summary>
        /// 创建子面板
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="uiPath"></param>
        /// <param name="data"></param>
        /// <param name="parent"></param>
        /// <param name="callBack"></param>
        public void CreateItem<TItem>(string uiPath,object data = null,Transform parent = null, Action<TItem> callBack = null) where TItem : UIBasePanel
        {
            if (parent == null) parent = this.owner?.transform;
            UIManager.Instance.DoOpen<TItem>(uiPath, data, (obj, panel) =>
            {
                if (obj == null) return;
                Children.Add(panel.Id, panel);
                obj.transform.SetParent(parent,false);
                obj.transform.localPosition = Vector3.zero;
                callBack?.Invoke(panel as TItem);
            });
        }

        /// <summary>
        /// 获取视图类型 - 明确标识为面板类型
        /// </summary>
        /// <returns>面板视图类型</returns>
        public override E_UIViewType GetViewType()
        {
            return E_UIViewType.Panel;
        }


        /// <summary>
        /// 处理返回键 - 面板级的返回逻辑
        /// 面板通常需要关闭整个界面
        /// </summary>
        public override void OnEscape()
        {
            // 默认行为是关闭当前面板
            UIManager.Instance.CloseUI(this.Id);
            base.OnEscape();
        }

        /// <summary>
        /// 面板销毁时的清理工作
        /// </summary>
        public override void OnDispose()
        {
            ClearChildren();
            base.OnDispose();
        }

        public void ClearChildren()
        {
            // 清理所有子面板
            foreach (var child in Children.Values)
            {
                child?.OnDispose();
            }
            Children.Clear();
        }
    }
}