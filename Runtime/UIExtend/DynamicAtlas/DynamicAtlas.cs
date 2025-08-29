using System.Collections.Generic;
using UnityEngine;

namespace UISystem
{
    public class DynamicAtlas
    {
        private static Texture2D cleared_texture = new Texture2D(2, 2, TextureFormat.ARGB32, false);
        
        private float mUVXDiv, mUVYDiv;
        private List<Material> m_MaterialList = new List<Material>();
        private List<RenderTexture> m_RenderTexList = new List<RenderTexture>();
        private List<RectanglePacker> mRectangleList = new List<RectanglePacker>();

        private int mWidth = 0;
        private int mHeight = 0;
        private int mPadding = 1;

        /// <summary>
        /// key，data
        /// </summary>
        private Dictionary<string, GetImageData> _getterMap = new Dictionary<string, GetImageData>();
        private Dictionary<string, IAssetHandle> _assetReqMap = new Dictionary<string, IAssetHandle>();
        private Dictionary<string, IntegerRectangle> _rectangleMap = new Dictionary<string, IntegerRectangle>();
        
        public List<RenderTexture> renderTexList { get { return m_RenderTexList; } }
        public int atlasWidth { get { return mWidth; } }
        public int atlasHeight { get { return mHeight; } }

        private static Material mMaterial;
        private static int mBlitParamId = Shader.PropertyToID("_DrawRect");
        private static int mBlitGrayId = Shader.PropertyToID("_IsGray");

        void CreateNewAtlas()
        {
            RenderTexture renderTexture = CreateDynamicAtlasRenderTexture(mWidth, mHeight, 0, RenderTextureFormat.ARGB32);
            renderTexture.name = string.Format("DynamicAtlas {0:G} -- {1:G}", mWidth, mHeight);
            renderTexture.DiscardContents(true, true);
            Material mDefaultMaterial = new Material(Shader.Find("UI/Default"));
            mDefaultMaterial.mainTexture = renderTexture;
            m_MaterialList.Add(mDefaultMaterial);

            mMaterial.SetVector(mBlitParamId, new Vector4(0, 0, 1, 1));
            
            Graphics.Blit(cleared_texture, renderTexture, mMaterial);
            m_RenderTexList.Add(renderTexture);
            
            mRectangleList.Add(new RectanglePacker(mWidth, mHeight, mPadding));
        }

        private RenderTexture CreateDynamicAtlasRenderTexture(int width, int height, int depth, RenderTextureFormat format)
        {
            RenderTexture data = new RenderTexture(width, height, depth, format);
            data.autoGenerateMips = false;
            data.useMipMap = false;
            data.filterMode = FilterMode.Bilinear;
            data.wrapMode = TextureWrapMode.Clamp;
            data.Create();
            return data;
        }

        public DynamicAtlas(DynamicAtlasGroup group)
        {
            if (mMaterial == null)
            {
                mMaterial = new Material(Shader.Find("RenderTexture/IconRender"));
            }

            int length = (int)group;
            mWidth = length;
            mHeight = length;
            //mPadding = padding;
            mUVXDiv = 1f / mWidth;
            mUVYDiv = 1f / mHeight;
            CreateNewAtlas();
        }

        private void BlitTexture(Rect uv, int index, Texture srcTex, bool isGray)
        {
            // Rect uv = new Rect(posX * mUVXDiv, posY * mUVYDiv, srcTex.width * mUVXDiv, srcTex.height * mUVYDiv);
            RenderTexture dest = m_RenderTexList[index];
            GraphicsBlit(uv, srcTex, dest, mMaterial, isGray);
        }
        
        void GraphicsBlit(Rect rc, Texture source, RenderTexture dest, Material fxMaterial, bool isGray)
        {
            fxMaterial.SetVector(mBlitParamId, new Vector4(rc.xMin, rc.yMin, rc.xMax, rc.yMax));
            fxMaterial.SetFloat(mBlitGrayId, isGray ? 1 : 0);
    #if UNITY_EDITOR && !UNITY_EDITOR_OSX
            // MAC M芯片可能有问题
            dest.DiscardContents();
    #endif
            Graphics.Blit(source, dest, fxMaterial);
        }

        public Dictionary<string, GetImageData> GetGetter()
        {
            return _getterMap;
        }

        //bilt
        public void SetTexture(string path, bool isGray, bool isSyn, OnCallBackMetRect callback)
        {
            if (string.IsNullOrEmpty(path))
            {
                if (callback != null)
                {
                    callback(null, Rect.zero);
                }
                Debug.LogError($"loader dynamic texture fail  path is null !");
                return;
            }

            string key = isGray ? string.Concat("#", path) : path;
            
            GetImageData data = null;
            if (_getterMap.TryGetValue(key, out data))
            {
                data.Retain();
                
                if (callback == null)
                    return;

                // 正在加载中;
                if (data.loader != null && !data.loader.IsDone)
                {
                    data.BlitCallback += callback;
                    return;
                }
                
                // 设置成功;
                if (data.texIndex >= 0)
                {
                    Material material = m_MaterialList[data.texIndex];
                    callback(material, data.rect);
                    data.CacheCallBack += callback;
                    return;
                }
                else
                {
                    //进入到这里说明异常，拿着脏数据想要进行加载，应该清理掉脏数据，重新申请
                    data.Recycle();
                    _getterMap.Remove(key);
                    //if (data.loader == null || data.loader.isError())
                    //{
                    //    callback(null, Rect.zero);
                    //    data.CacheCallBack += callback;
                    //}
                    //else
                    //    data.BlitCallback += callback;

                }
                //return;
            }
            
            data = DynamicAtlasManager.Instance.AllocateGetImageData();
            _getterMap.Add(key, data);

            data.Retain();
            data.key = key;
            data.url = path;
            data.isGray = isGray;
            data.BlitCallback = callback;

            IntegerRectangle rectangle = null;
            if (_rectangleMap.TryGetValue(key, out rectangle))
            {
                rectangle.isUsed = true;
                data.rectangle = rectangle;
                data.texIndex = rectangle.index;
                data.rect = GetRect(rectangle, false);
                data.loader = GetAssetReq(path, isSyn, true);
                if (data.loader == null)
                    Debug.LogError("========loader is null==========");
                SuccessCallBack(data);

                //if (data?.loader != null && data.loader.asset == null)
                //{
                //    Debug.LogError($"DynamicAtlas ReUse texture , BUT! asset is null ================  " +
                //        $"path:{path} rectangle::{data.rectangle} isSyn:{isSyn} texIndex:{data.texIndex} rect:{data.rect} key:{key}" );
                //}

#if UNITY_EDITOR
                //Debug.Log("ReUse texture ================" + path);
#endif
            }
            else
            {
                data.loader = GetAssetReq(path, isSyn);

                if (data.loader == null)
                {
                    EmptyCallBack(data);
                    if (data.loader == null)
                        Debug.LogError("========loader is null==========");
                    return;
                }
                
                if (data.loader.IsDone && data.loader.Status != E_LoadStatus.Failed)
                {
                    //if (data?.loader != null && data.loader.asset == null)
                    //{
                    //    Debug.LogError($"DynamicAtlas Get texture , BUT! asset is null   ================  path:{path} isSyn:{isSyn} " );
                    //}
                    OnRenderTexture(data);
                }
            }
        }

        public void RecycleAssetReq(IAssetHandle ar)
        {
            if (ar == null)
                return;
            if (ar.IsUnused)
            {
                ar.Release();
                _assetReqMap.Remove(ar.Address);
            }
        }
        
        private IAssetHandle GetAssetReq(string path, bool isSyn, bool autoCreate = true)
        {
            IAssetHandle ar = null;
            if (_assetReqMap.TryGetValue(path, out ar))
            {
                //解决动态图集脏数据导致图片加载失败的问题;
                if (ar.Asset == null)
                {
                    _assetReqMap.Remove(path);
                }
                else
                {
                    return ar;
                }
            }

            if (!autoCreate)
                return null;

            ar = UIManager.Instance.resourceMgr.LoadAssetHandle<Texture2D>(path, LoadCallBack, isSyn ? E_LoadType.Sync : E_LoadType.Async);
            if (ar != null)
            {
                _assetReqMap.Add(path, ar);
            }
            return ar;
        }

        private void LoadCallBack(IAssetHandle ar)
        {
            GetImageData data = null;
            string key = ar.Address;
            if (_getterMap.TryGetValue(key, out data))
            {
                data.loader = ar;
                OnRenderTexture(data);
            }
            
            string grayKey = string.Format("#{0}", key);
            if (_getterMap.TryGetValue(grayKey, out data))
            {
                data.loader = ar;
                OnRenderTexture(data);
            }
        }

        //image用完之后销毁，同时要在atlas上同步数据
        public void RemoveImage(string path, bool isGray, OnCallBackMetRect callBack)
        {
            if (string.IsNullOrEmpty(path))
                return;
            
            string key = isGray ? string.Concat("#", path) : path;
            if (_getterMap.ContainsKey(key))
            {
                GetImageData imageData = _getterMap[key];
                imageData.BlitCallback -= callBack;
                imageData.CacheCallBack -= callBack;
                
                imageData.Release();
                if (imageData.IsUnused())
                {
                    _getterMap.Remove(key);
                 
                    RecycleAssetReq(imageData.loader);
                    DynamicAtlasManager.Instance.ReleaseGetImageData(imageData);
                }
            }

            // 下次重新分配;
//            if (_getterMap.Count == 0)
//            {
//                ResizeRectangle();
//            }
        }

#region 编辑器模式逻辑
#if UNITY_EDITOR
        private Dictionary<string, Texture> _editorTexMap = new Dictionary<string, Texture>();
        
        /// <summary>
        /// 不分配一张大图;
        /// </summary>
        /// <param name="tex"></param>
        /// <param name="key"></param>
        public void OnRenderTextureEditor(Texture tex, string key)
        {
            if(tex == null || string.IsNullOrEmpty(key))
                return;

            int index;
            IntegerRectangle useArea = GetEmptyRectangleFrom1(tex.width, tex.height, out index, key);
            if (useArea == null)
            {
                int rectangleIdx = GetHasNoUsedRectangleIdx();
                if (rectangleIdx == 0)
                {
                    Debug.Log(string.Format("Resize ======== {0}", rectangleIdx));
                    // 全部重新分配;
                    ResizeRectangleEditor(rectangleIdx);
                    useArea = GetEmptyRectangleFrom1(tex.width, tex.height, out index, key, rectangleIdx);
                }
            }
            
            if (useArea == null)
            {
                // 分配不出空间！！！
                Debug.LogError("can't get rectangle !!!!!!!!!");
                return;
            }
            
            Rect uv = GetRect(useArea, true);
            BlitTexture(uv, index, tex, false);

            if (!_editorTexMap.ContainsKey(key))
            {
                _editorTexMap.Add(key, tex);
            }
        }

        public void RemoveImageEditor(string key)
        {
            if(mRectangleList.Count == 0 || string.IsNullOrEmpty(key))
                return;

            if (_editorTexMap.ContainsKey(key))
                _editorTexMap.Remove(key);
            
            RectanglePacker rectPacker = mRectangleList[0];
            IntegerRectangle integerRect = rectPacker.getRectangleByKey(key);
            if(integerRect != null) integerRect.Recycle();
        }

        public IntegerRectangle GetIntegerRectangle(string key)
        {
            if(mRectangleList.Count == 0)
                return null;
            
            RectanglePacker rectPacker = mRectangleList[0];
            return rectPacker.getRectangleByKey(key);
        }
        
        private void ResizeRectangleEditor(int idx)
        {
            RectanglePacker packer = mRectangleList[idx];
            Dictionary<string, IntegerRectangle> map = packer.GetRectangleMap();
            foreach (KeyValuePair<string, IntegerRectangle> pair in map)
            {
                _rectangleMap.Remove(pair.Key);
            }
            
            packer.reset(mWidth, mHeight, mPadding);
            foreach (KeyValuePair<string,Texture> pair in _editorTexMap)
            {
                OnRenderTextureEditor(pair.Value, pair.Key);
            }
        }
#endif
#endregion

        private void OnRenderTexture(GetImageData imageData)
        {
            if (imageData == null || imageData.texIndex >= 0)
                return;

            // 加载失败;
            if (imageData.loader.Status == E_LoadStatus.Failed)
            {
                EmptyCallBack(imageData);
                return;
            }

            Texture2D data = imageData.loader.Asset as Texture2D;
            if(data == null)
            {
                Debug.LogError($"load texture fail   url : {imageData.url}   asset : {imageData.loader.Asset}   statu : {imageData.loader.Status}");
                return;
            }
            int index;
            IntegerRectangle useArea = GetEmptyRectangleFrom1(data.width, data.height, out index, imageData.key);
            // 没有空位了，尝试重构获取空位，最后重构都获取不到了，再考虑申请新的大图;
            if (useArea == null)
            {
                int rectangleIdx = GetHasNoUsedRectangleIdx();
                while (useArea == null && rectangleIdx >= 0)
                {
#if UNITY_EDITOR
                    Debug.Log(string.Format("Resize ======== {0}", rectangleIdx));
#endif
                    
                    // 全部重新分配;
                    ResizeRectangle(rectangleIdx);
                    useArea = GetEmptyRectangleFrom1(data.width, data.height, out index, imageData.key, rectangleIdx);

                    if (useArea == null)
                    {
                        rectangleIdx = GetHasNoUsedRectangleIdx(rectangleIdx);   
                    }
                }

                // 确实没空位了;
                if (useArea == null)
                {
                    useArea = GetEmptyRectangleFrom2(data.width, data.height, out index, imageData.key);
                }
            }

            if (useArea == null)
            {
                Debug.LogError("can't get rectangle !!!!!!!!!");
                // 分配不出空间！！！
                EmptyCallBack(imageData);
                return;
            }

            Rect uv = GetRect(useArea, true);
            BlitTexture(uv, index, data, imageData.isGray);

            imageData.texIndex = index;
            imageData.rectangle = useArea;
            imageData.rect = GetRect(useArea, false);
            useArea.index = index;
            _rectangleMap.Add(imageData.key, useArea);
            
            SuccessCallBack(imageData);
        }

        private Rect GetRect(IntegerRectangle rectangle, bool drawFlag)
        {
            if(drawFlag)
                return new Rect(rectangle.x * mUVXDiv, rectangle.y * mUVYDiv,
                rectangle.width * mUVXDiv, rectangle.height * mUVYDiv);
            else
            {
                float offset = 0.5f;
                return new Rect((rectangle.x + offset) * mUVXDiv, (rectangle.y + offset) * mUVYDiv,
                    (rectangle.width - offset * 2) * mUVXDiv, (rectangle.height - offset * 2) * mUVYDiv);
            }
        }

        private void ResizeRectangle(int idx)
        {
            RectanglePacker packer = mRectangleList[idx];
            Dictionary<string, IntegerRectangle> map = packer.GetRectangleMap();
            foreach (KeyValuePair<string, IntegerRectangle> pair in map)
            {
                _rectangleMap.Remove(pair.Key);
            }
            
            packer.reset(mWidth, mHeight, mPadding);
            
            List<GetImageData> list = new List<GetImageData>();
            list.AddRange(_getterMap.Values);
            
            foreach (GetImageData imageData in list)
            {
                if(imageData.texIndex != idx)
                    continue;
                
                if (imageData.loader == null)
                {
                    imageData.rectangle = null;
                    imageData.texIndex = -1;
                    
                    imageData.BlitCallback = imageData.CacheCallBack;
                    EmptyCallBack(imageData);
                    
                    imageData.BlitCallback = imageData.CacheCallBack;
                    imageData.CacheCallBack = null;
                    imageData.loader = GetAssetReq(imageData.url, false);
                    if (imageData.loader == null)
                        Debug.LogError("========loader is null==========");
                }
                else
                {
                    // 加载中、加载失败的不用管;
                    if (!imageData.loader.IsDone || imageData.loader.Status == E_LoadStatus.Failed)
                    {
                        continue;
                    }

                    // 成功但未申请区域的不用管;
                    if (imageData.texIndex < 0)
                    {
                        continue;
                    }

                    // 重新绘制;
                    imageData.texIndex = -1;
                    
                    imageData.BlitCallback = imageData.CacheCallBack;
                    EmptyCallBack(imageData);
                    
                    imageData.BlitCallback = imageData.CacheCallBack;
                    imageData.CacheCallBack = null;
                    
                    // 重新绑回调触发;
                    // imageData.loader.completed += LoadCallBack;
                    if (imageData.loader != null)
                    {
                        imageData.loader.AddCompleted(LoadCallBack);
                    }
                    //AssetsManager.InvokeCompleteNoRation(imageData.loader);
                }
            }
        }

        private void EmptyCallBack(GetImageData imageData)
        {
            if (imageData.BlitCallback != null)
            {
                imageData.BlitCallback.Invoke(null, Rect.zero);
                imageData.CacheCallBack = imageData.BlitCallback;
                imageData.BlitCallback = null;
            }
        }

        private void SuccessCallBack(GetImageData imageData)
        {
            if (imageData.texIndex >= 0 && imageData.BlitCallback != null)
            {
                imageData.BlitCallback.Invoke(m_MaterialList[imageData.texIndex], imageData.rect);
            }

            imageData.CacheCallBack = imageData.BlitCallback;
            imageData.BlitCallback = null;
        }

        /// <summary>
        /// 获取第一个有空引用的矩形;
        /// </summary>
        /// <returns></returns>
        private int GetHasNoUsedRectangleIdx(int startIdx = 0)
        {
            int count = mRectangleList.Count;
            for (int idx = startIdx; idx < count; idx++)
            {
                if (mRectangleList[idx].HasNoUsedRectangle())
                    return idx;
            }

            return -1;
        }

        private IntegerRectangle GetEmptyRectangleFrom1(int width, int height, out int index, string key, int rectangleIdx = -1)
        {
            index = -1;

            if (rectangleIdx >= 0)
            {
                RectanglePacker rectPacker = mRectangleList[rectangleIdx];
                
                // 分配成功;
                IntegerRectangle target = rectPacker.getRectangle(width, height, key);
                if (target != null)
                {
                    target.isUsed = true;
                    index = rectangleIdx;
                    return target;
                }

                return null;
            }
            
            int count = mRectangleList.Count;
            for (int idx = 0; idx < count; idx++)
            {
                RectanglePacker rectPacker = mRectangleList[idx];
                
                // 分配成功;
                IntegerRectangle target = rectPacker.getRectangle(width, height, key);
                if (target != null)
                {
                    target.isUsed = true;
                    index = idx;
                    return target;
                }
            }

            return null;
        }
        
        private IntegerRectangle GetEmptyRectangleFrom2(int width, int height, out int index, string key)
        {
            index = -1;
            
            CreateNewAtlas();

            int idx = mRectangleList.Count - 1;
            RectanglePacker rectPacker = mRectangleList[idx];
            IntegerRectangle target = rectPacker.getRectangle(width, height, key);
            if (target != null)
            {
                target.isUsed = true;
                index = idx;
            }
                
            return target;
        }

        public List<RectanglePacker> GetFreeAreas()
        {
            return mRectangleList;
        }
    }
}
