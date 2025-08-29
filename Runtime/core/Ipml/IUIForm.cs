

namespace UISystem
{
    /// <summary>
    /// UI界面接口 - 定义UI生命周期和核心方法
    /// </summary>
    public interface IUIForm
    {
        /// <summary>
        /// 唯一标识 - 用于全局查找UI的ID，新手引导等功能可能会用到
        /// </summary>
        long Id { get; }

        /// <summary>
        /// ui的加载路径
        /// </summary>
        string uiPath { get; }

        /// <summary>
        /// UI所在层级
        /// </summary>
        E_UILayer UILayer { get; }

        /// <summary>
        /// 是否已初始化
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// 是否全屏
        /// </summary>
        bool IsFullScreen { get; }

        /// <summary>
        /// 初始化 - 在UI加载完成后调用，只调用一次
        /// </summary>
        void OnAwake();

        /// <summary>
        /// 设置数据 - 每次显示UI时都会调用
        /// </summary>
        /// <param name="data">传入的参数</param>
        void SetData(System.Object data);

        /// <summary>
        /// 显示UI - 在UI即将显示时调用
        /// </summary>
        void OnShow();

        /// <summary>
        /// 隐藏UI - 在UI即将隐藏时调用
        /// </summary>
        void OnHide();

        /// <summary>
        /// 关闭UI - 在UI即将关闭时调用
        /// </summary>
        void OnClose();

        /// <summary>
        /// 处理返回键/Escape键 - 按下返回键时调用
        /// </summary>
        void OnEscape();

        /// <summary>
        /// 暂停 - 当UI被其他UI覆盖时调用
        /// </summary>
        void OnPause();

        /// <summary>
        /// 恢复 - 当UI从被覆盖状态恢复时调用
        /// </summary>
        void OnResume();

        /// <summary>
        /// 断线重连时
        /// </summary>
        void OnReconnect();

    }
}

