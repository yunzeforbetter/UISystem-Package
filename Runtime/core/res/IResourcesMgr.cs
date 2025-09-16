using System;
using UnityEngine;

namespace UISystem
{

    public enum E_LoadType
    {
        //资源同步加载方式
        Sync,
        //资源异步加载方式
        Async,
    }
    public enum E_LoadStatus
    {
        None,
        Processing,
        Succeed,
        Failed
    }

    /// <summary>
    /// 资源管理器接口 - 用于UI系统的资源管理抽象
    /// 支持不同的资源管理器实现，提供统一的资源加载接口
    /// </summary>
    public interface IResourceManager
    {
        /// <summary>
        /// 实例化游戏对象句柄 - 异步加载并实例化预制体
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="data">自定义数据</param>
        /// <param name="loadType">加载类型（同步/异步）</param>
        /// <param name="position">初始位置</param>
        /// <returns>游戏对象句柄，用于管理加载状态和回调</returns>
        IGameObjectHandle InstantiateHandle(string path, object data = null, E_LoadType loadType = E_LoadType.Async, Vector3 position = default);

        /// <summary>
        /// 加载资源句柄 - 用于加载各种类型的资源
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="path">资源路径</param>
        /// <param name="completed">完成回调</param>
        /// <param name="loadType">加载类型</param>
        /// <returns>资源句柄</returns>
        IAssetHandle LoadAssetHandle<T>(string path, Action<IAssetHandle> completed = null, E_LoadType loadType = E_LoadType.Async) where T : UnityEngine.Object;

        /// <summary>
        /// 同步实例化游戏对象 - 立即返回实例化的游戏对象
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns>实例化的游戏对象，失败返回null</returns>
        GameObject InstantiateSync(string path);

        /// <summary>
        /// 异步实例化游戏对象 - 通过回调返回实例化结果
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="callback">完成回调</param>
        /// <param name="loadType">加载类型</param>
        void InstantiateAsync(string path, Action<GameObject> callback, E_LoadType loadType = E_LoadType.Async);

        /// <summary>
        /// 检查资源是否存在
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns>资源是否存在</returns>
        bool HasAsset(string path);
    }

    /// <summary>
    /// 游戏对象句柄接口 - 抽象游戏对象加载过程的控制句柄
    /// </summary>
    public interface IGameObjectHandle
    {
        /// <summary>
        /// 自定义数据 - 在加载过程中传递的额外数据
        /// </summary>
        object Data { get; set; }

        /// <summary>
        /// 资源路径 - 要加载的资源路径
        /// </summary>
        string Path { get; }

        /// <summary>
        /// 是否已调用 - 标记是否已开始加载过程
        /// </summary>
        bool IsInvoked { get; }

        /// <summary>
        /// 是否完成 - 标记加载是否已完成
        /// </summary>
        bool IsComplete { get; }

        /// <summary>
        /// 是否已取消 - 标记加载是否被取消
        /// </summary>
        bool IsCancelled { get; }

        /// <summary>
        /// 实例化的游戏对象 - 加载完成后的游戏对象
        /// </summary>
        GameObject GameObject { get; }

        /// <summary>
        /// 完成回调 - 加载完成时调用的回调函数
        /// </summary>
        Action<GameObject> Callback { get; set; }

        /// <summary>
        /// 加载类型 - 同步或异步加载
        /// </summary>
        E_LoadType LoadType { get; }

        /// <summary>
        /// 初始位置 - 实例化时的初始位置
        /// </summary>
        Vector3 Position { get; }

        /// <summary>
        /// 取消加载 - 取消当前的加载操作
        /// 注意：取消后不会触发回调，必须在回调函数调用前处理
        /// </summary>
        void Cancel();
    }

    /// <summary>
    /// 资源句柄接口 - 抽象资源加载过程的控制句柄
    /// </summary>
    public interface IAssetHandle
    {
        /// <summary>
        /// 加载的资源对象
        /// </summary>
        UnityEngine.Object Asset { get; }

        /// <summary>
        /// 是否加载完成
        /// </summary>
        bool IsDone { get; }

        /// <summary>
        /// 资源加载状态
        /// </summary>
        E_LoadStatus Status { get; }

        /// <summary>
        /// 加载进度 (0-1)
        /// </summary>
        float Progress { get; }

        /// <summary>
        /// 资源寻址
        /// </summary>
        string Address { get; }

        /// <summary>
        /// 是否可以释放了
        /// </summary>
        bool IsUnused { get; }

        /// <summary>
        /// 添加回调监听
        /// </summary>
        /// <param name="completed"></param>
        void AddCompleted(Action<IAssetHandle> completed);

        /// <summary>
        /// 移除回调监听
        /// </summary>
        /// <param name="completed"></param>
        void RemoveCompleted(Action<IAssetHandle> completed);

        /// <summary>
        /// 释放资源句柄
        /// </summary>
        void Release();


    }
}