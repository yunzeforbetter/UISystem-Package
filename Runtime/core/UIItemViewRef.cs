namespace UISystem
{
   /// <summary>
   /// ui子面板
   /// </summary>
   /// <typeparam name="T"></typeparam>
    public class UIItemViewRef<T> : UIViewRefBase<T> where T : IPrefabRef, new()
    {
        /// <summary>
        /// 带参数构造函数 - 用于创建特定的项实例
        /// </summary>
        /// <param name="id">项唯一标识符</param>
        /// <param name="uiPath">项资源路径</param>
        public UIItemViewRef(long id, string uiPath) : base(id, uiPath) { }

        public override bool IsFullScreen => false;

        /// <summary>
        /// 获取视图类型 - 明确标识为项类型
        /// </summary>
        /// <returns>项视图类型</returns>
        public override E_UIViewType GetViewType()
        {
            return E_UIViewType.Item;
        }

        /// <summary>
        /// 项销毁时的轻量级清理
        /// </summary>
        public override void OnDispose()
        {
            base.OnDispose();
        }
    }
}