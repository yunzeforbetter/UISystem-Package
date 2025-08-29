using System.Collections.Generic;

namespace UISystem
{
    public class DynamicAtlasManager
    {
        private static DynamicAtlasManager _instance;
        public static DynamicAtlasManager Instance
        {
            get
            {
                if(_instance == null)
                    _instance = new DynamicAtlasManager();
                return _instance;
            }
        }

        private Dictionary<DynamicAtlasGroup, DynamicAtlas> m_DynamicAtlas = new Dictionary<DynamicAtlasGroup, DynamicAtlas>();
        
        /// <summary>
        /// 对象池;
        /// </summary>
        private Stack<GetImageData> mGetImageDataStack = new Stack<GetImageData>();

        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="group"></param>
        /// <param name="topFirst"></param>
        /// <returns></returns>
        public DynamicAtlas GetDynamicAtlas(DynamicAtlasGroup group)
        {
            DynamicAtlas data;
            if (m_DynamicAtlas.ContainsKey(group))
            {
                data = m_DynamicAtlas[group];
            }
            else
            {
                data = new DynamicAtlas(group);
                m_DynamicAtlas[group] = data;
            }
            return data;
        }

        public GetImageData AllocateGetImageData()
        {
            if (mGetImageDataStack.Count > 0)
            {
                GetImageData popData = mGetImageDataStack.Pop();
                return popData;
            }
            GetImageData data = new GetImageData();
            return data;
        }

        public void ReleaseGetImageData(GetImageData data)
        {
            data.Recycle();
            mGetImageDataStack.Push(data);
        }
    }
}

