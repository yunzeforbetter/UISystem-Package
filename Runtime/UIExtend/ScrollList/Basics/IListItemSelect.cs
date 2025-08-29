
using System;

namespace UISystem
{
    /// <summary>
    /// 列表item选中接口
    /// </summary>
    interface IListItemSelect
    {
        void RegisterSelectCallBack(Action<bool> callBack);
    }

}
