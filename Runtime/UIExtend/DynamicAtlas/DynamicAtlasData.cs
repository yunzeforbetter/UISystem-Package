using UnityEngine;

namespace UISystem
{
    public delegate void OnCallBackMetRect(Material a, Rect b);

    public enum DynamicAtlasGroup
    {
        Size_256 = 256,
        Size_512 = 512,
        Size_1024 = 1024,
        Size_2048 = 2048
    }

    public class GetImageData
    {
        public string key;
        public string url;
        public bool isGray;

        public int texIndex = -1;
        public int refCount = 0;
        public Rect rect;
        public IntegerRectangle rectangle;//with padding

        // 加载器;
        public IAssetHandle loader;
        public OnCallBackMetRect BlitCallback;
        public OnCallBackMetRect CacheCallBack;

        public bool IsUnused()
        {
            return refCount <= 0;
        }

        public void Retain()
        {
            refCount++;
        }

        public void Release()
        {
            int count = refCount - 1;
            if (count < 0)
                Debug.LogError($"=>>>>> dynamicAtlas Release Error   refCount：{count}");

            refCount = Mathf.Max(count, 0);
        }

        public void Recycle()
        {
            if (rectangle != null)
            {
                rectangle.isUsed = false;
                rectangle = null;
            }

            isGray = false;
            texIndex = -1;
            loader = null;
        }
    }

    public class IntegerRectangle
    {
        public int x;
        public int y;
        public int width;
        public int height;
        public int right;
        public int bottom;
        public int index;    // 属于哪张大图;

        public bool isUsed = false;    // 一一对应的，所以不需要引用计数;

        public IntegerRectangle(int x = 0, int y = 0, int width = 0, int height = 0) {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            right = x + width;
            bottom = y + height;
        }

        public void Recycle()
        {
            isUsed = false;
        }

        public override string ToString()
        {
            return string.Format("x{0}_y:{1}_width:{2}_height{3}_bottom:{4}_right{5}", x, y, width, height, bottom, right);
        }
    }
}


