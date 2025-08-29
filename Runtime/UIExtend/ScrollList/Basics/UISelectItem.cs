
using System;
using System.Collections.Generic;

using UnityEngine;

namespace UISystem
{
    /// <summary>
    /// UI选中
    /// </summary>
    public class UISelectItem : MonoBehaviour, IListItemSelect
    {
        public List<GameObject> selectObjs;
        public List<GameObject> unselectObjs;

        private Action<bool> onSelectCallBack;

        public void OnSelect(bool isSelect)
        {
            selectObjs?.ForEach(x => x.SetActive(isSelect));
            unselectObjs?.ForEach(x => x.SetActive(!isSelect));
            onSelectCallBack?.Invoke(isSelect);
        }

        public void RegisterSelectCallBack(Action<bool> callBack)
        {
            onSelectCallBack = callBack;
        }

        private void OnDestroy()
        {
            onSelectCallBack = null;
        }

    }
}