using UnityEngine;

namespace UISystem
{

    /// <summary>
    /// UI视图引用泛型基类 - 提供显示层和逻辑层连接的通用功能
    /// 抽象出公共的View管理和绑定逻辑，提高代码复用性
    /// </summary>
    /// <typeparam name="T">实现IPrefabRef接口的显示层引用类型</typeparam>
    public abstract class UIViewRefBase<T> : UIBasePanel where T : IPrefabRef, new()
    {
        /// <summary>
        /// 显示层桥接对象 - 连接Unity GameObject和业务逻辑
        /// 使用泛型确保类型安全，避免object类型的装箱拆箱开销
        /// </summary>
        protected T View { get; private set; }

        /// <summary>
        /// 带参数构造函数 - 用于创建具有明确身份的UI视图
        /// </summary>
        /// <param name="id">UI唯一标识符</param>
        /// <param name="uiPath">UI资源路径</param>
        protected UIViewRefBase(long id, string uiPath)
        {
            _id = id;              // 设置UI唯一标识
            _uiPath = uiPath;      // 设置资源加载路径
            _isInitialized = false; // 初始化状态为未完成
        }

        /// <summary>
        /// 绑定预制体引用到视图 - 核心的显示层连接方法
        /// 此方法负责建立GameObject与业务逻辑之间的桥梁
        /// </summary>
        /// <param name="prefabReference">预制体引用对象</param>
        public override void Bind(PrefabReference prefabReference)
        {
            // 参数验证，防止空引用异常
            if (prefabReference == null)
                return;

            // 设置GameObject所有者，建立显示层关联
            owner = prefabReference.gameObject;
            
            // 延迟初始化View对象，避免不必要的内存分配
            if (View == null)
                View = new T(); // 构建显示层的关联类型实例
            
            // 将预制体引用传递给View进行具体的UI组件绑定
            View.Bind(prefabReference);
        }
    }
}
