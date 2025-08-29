using System.Collections.Generic;
using UnityEngine;

namespace UISystem
{

    public interface IUIPanelPool
    {
        //检测是否到达缓存时间
        bool CheckCacheTime(float intervalTime);
        ////从缓存中拿出时
        //void OnAlloc();
        ////回收进缓存时
        //void OnRecycle();
        //彻底释放
        void OnDispose();
    }

    //UI缓存池
    public class UIPanelObjectPools : MonoBehaviour
    {
        private static UIPanelObjectPools _instance;
        public static UIPanelObjectPools Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject obj = new GameObject("UIPanelObjectPools");
                    _instance = obj.AddComponent<UIPanelObjectPools>();
                    DontDestroyOnLoad(obj);
                }
                return _instance;
            }
        }

        //最大缓存数量
        private const int MaxCount = 50;
        //缓存列表
        public readonly Dictionary<string, List<UIBasePanel>> ObjPoolMap = new Dictionary<string, List<UIBasePanel>>(MaxCount);
        private List<string> _removeLinkKey = new List<string>();
        private const float checkIntervalTime = 1;
        private float curTime;
        

        private void Update()
        {
            curTime -= Time.deltaTime;
            if (curTime <= 0)
            {
                curTime = checkIntervalTime;
                foreach (var pool in ObjPoolMap.Values)
                {
                    if (pool == null || pool.Count <= 0)
                        continue;
                    for (int i = pool.Count - 1; i >= 0; i--)
                    {
                        var item = pool[i];
                        if (item == null || item.CheckCacheTime(checkIntervalTime))
                        {
                            item?.OnDispose();
                            pool.RemoveAt(i);
                        }
                    }
                }
            }

        }

        //Push缓存物体 
        public void Push(UIBasePanel _obj, string _poolKey)
        {
            if (_obj == null) return;

            //缓存对象
            if (!ObjPoolMap.ContainsKey(_poolKey))
            {
                if (ObjPoolMap.Count >= MaxCount)
                {
                    int removeNum = ObjPoolMap.Count - MaxCount + MaxCount / 2; //超出范围 清理 超出的部分 + 总大小一半的缓存
                    foreach (var pool in ObjPoolMap)
                    {
                        if (removeNum <= 0)
                            break;
                        _removeLinkKey.Add(pool.Key);
                        removeNum--;
                    }
                    foreach (var keyId in _removeLinkKey)
                    {
                        if (!ObjPoolMap.ContainsKey(keyId))
                            continue;
                        foreach (var item in ObjPoolMap[keyId])
                        {
                            item.OnDispose();
                        }
                    }
                    _removeLinkKey.Clear();
                }
                List<UIBasePanel> entitylist = new List<UIBasePanel>();
                ObjPoolMap.Add(_poolKey, entitylist);
            }

            ObjPoolMap[_poolKey].Add(_obj);
            _obj.SetRoot(transform, false, false);
            _obj.OnHide();
            //_obj.OnRecycle();
        }

        //从缓存中取出Obj
        public UIBasePanel Pop(string _poolKey)
        {
            if (!ObjPoolMap.ContainsKey(_poolKey))
            {
                return null;
            }

            if (ObjPoolMap[_poolKey] == null)
                return null;

            //没有
            List<UIBasePanel> cachList = ObjPoolMap[_poolKey];
            if (cachList.Count == 0) return null;

            //返回第一个
            UIBasePanel obj = cachList[0];
            if (obj == null)
                return null;

            //obj.OnAlloc();
            //重置
            cachList.RemoveAt(0);
            return obj;
        }

        //从缓存中移除指定Obj
        public void Remove(string _poolKey, UIBasePanel entity)
        {
            if (entity == null)
                return;
            if (!ObjPoolMap.ContainsKey(_poolKey))
            {
                Debug.LogError($"There is no pool:{_poolKey}");
                return;
            }
            var item = ObjPoolMap[_poolKey];
            item.Remove(entity);
            entity.OnDispose();
            if (item.Count <= 0)
            {
                ObjPoolMap.Remove(_poolKey);
            }
        }

        #region 调试
        //获取缓存的总的个数
        public int GetTotalCount()
        {
            int total = 0;
            foreach (var pair in ObjPoolMap)
            {
                total += pair.Value.Count;
            }
            return total;
        }
        #endregion
    }
}