namespace UISystem
{
    /// <summary>
    /// UI视图类型枚举 - 用于明确区分不同类型的UI视图
    /// 替代模糊的object类型判断，提供类型安全的识别机制
    /// </summary>
    public enum E_UIViewType
    {
        Panel = 1,    // 面板类型 - 独立的UI界面，具有完整的生命周期管理
        Item = 2      // 项类型 - UI列表项或小组件，轻量级且可复用
    }

    public enum E_UIForType
    {
        UI_Null,
        UI_TestPanel,
    }

    public enum E_UILayer
    {
        HeadLayer = 0, //头顶气泡层
        FunctionLayer = 1000, //默认功能层
        FunctionLayerEx = 2000, //功能扩展层
        MessageLayer = 3000, //消息层
    }

}

