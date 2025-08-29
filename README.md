# UISystem-Package - 高性能UI系统

[![Unity Version](https://img.shields.io/badge/Unity-2022.3%2B-blue.svg)](https://unity3d.com/get-unity/download)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

## 🚀 项目简介

该框架集成了现代UI开发所需的核心功能，包括智能堆栈管理、虚拟化无限滚动、运行时动态图集优化和实时模糊背景效果。

## ✨ 核心特性

### 🔄 智能UI堆栈管理
- **多层级管理**：支持多个UI层级，自动处理渲染顺序
- **生命周期控制**：完整的面板生命周期管理（Show/Hide/Pause/Resume）
- **对象池优化**：内置对象池系统，减少GC压力，提升性能
- **类型安全**：强类型UI管理，避免运行时错误

```csharp
// 打开UI面板
UIManager.Instance.OpenPanel<MainMenuPanel>("UI/MainMenu", data);

// 关闭UI面板
UIManager.Instance.CloseUI("UI/MainMenu");
```

### 📜 虚拟化无限列表
- **高性能渲染**：虚拟化技术，支持数万条数据流畅滚动
- **循环滚动**：支持无限循环滚动，完美用户体验
- **动态刷新**：智能数据更新，仅刷新可见区域元素
- **灵活布局**：支持垂直/水平/网格多种布局模式

```csharp
// 初始化列表
listView.Configure<T>(path, OnCreateCell, OnShowCell);
listView.SetDataSource(dataCount);

// 动态刷新单个元素
listView.TryRefreshCellRT(dataIndex);
```

### 🎨 运行时动态图集
- **智能纹理合并**：运行时自动合并小纹理，减少Draw Call
- **内存优化**：矩形打包算法，最大化纹理空间利用率
- **异步加载**：支持异步纹理加载，不阻塞主线程
- **自动回收**：智能内存管理，自动回收unused纹理

```csharp
// 设置动态纹理
DynamicRawImage.SetIcon(texturePath);

```

### 🌫️ 实时模糊背景
- **多模式支持**：兼容Screen Space Overlay/Camera模式
- **实时截图**：高效的背景截图技术
- **高斯模糊**：可配置的模糊强度和效果
- **性能优化**：智能缓存，避免重复计算

```csharp
// 启用模糊背景
blurController.BlurBg();

```

## 🛠️ 编辑器工具集

### 🔧 强大的编辑器扩展
- **代码生成器**：自动生成UI绑定代码，提高开发效率
- **组件创建助手**：快速创建标准UI组件
- **预制体模板**：内置ListView/GridView预制体模板
- **可视化调试**：丰富的Inspector面板，直观的参数调整

### 📊 性能分析工具
- **动态图集预览**：实时查看图集使用情况
- **内存使用统计**：监控UI系统内存占用
- **性能分析器**：识别性能瓶颈

## 📦 快速开始

### 安装方式

#### Package Manager安装（推荐）
1. 打开Unity Editor
2. 进入 `Window > Package Manager`
3. 点击左上角的 `+` 按钮
4. 选择 `Add package from git URL`
5. 输入：`https://github.com/yunzeforbetter/UISystem-Package.git`

或者在项目的 `Packages/manifest.json` 文件中添加：
```json
{
  "dependencies": {
    "com.framework.uisystem": "https://github.com/yunzeforbetter/UISystem-Package.git"
  }
}
```

#### Unity Package导入
2. 在Unity中选择 `Assets > Import Package > Custom Package`
3. 选择下载的 `.unitypackage` 文件导入

### 基础配置

#### 1. 设置UI根节点
```csharp
// 在场景中创建Canvas，并设置为UI根节点
public class UIInitializer : MonoBehaviour
{
    [SerializeField] private Canvas uiCanvas;
    [SerializeField] private IResourceManager resourceManager;
    
    void Start()
    {
        // 初始化UI管理器
        UIManager.Instance.SetUIRoot(uiCanvas);
        UIManager.Instance.SetResourcesMgr(resourceManager);
    }
}
```

#### 2. 创建UI面板类
```csharp
public class MainMenuPanel : UIPanelViewRef<MainMenuPanel>
{
    // 定义UI层级
    public override E_UILayer UILayer => E_UILayer.Normal;
    
    // 是否全屏显示
    public override bool IsFullScreen => true;
    
     public override void OnAwake()
    {
        base.OnAwake();
    }
	public override void SetData(object data)
    {
        base.SetData(data);
        Debug.Log($" SetData  {data}");
    }
    // 显示时调用
    public override void OnShow()
    {
        base.OnShow();
        Debug.Log("主菜单面板显示");
    }
    
    // 关闭时调用
    public override void OnClose()
    {
        base.OnClose();
        Debug.Log("主菜单面板关闭");
    }
    
}
```

## 📁 项目结构

```
com.framework.uisystem/
├── Runtime/                    # 运行时核心代码
│   ├── core/                  # 核心UI管理系统
│   │   ├── UIManager.cs       # UI管理器主类
│   │   ├── UIBasePanel.cs     # UI面板基类
│   │   └── UIPanelPool.cs     # UI对象池管理
│   ├── UIExtend/              # 扩展功能模块
│   │   ├── ScrollList/        # 虚拟化滚动列表
│   │   │   ├── UIListView.cs  # 列表视图组件
│   │   │   └── UIGridView.cs  # 网格视图组件
│   │   ├── DynamicAtlas/      # 动态图集系统
│   │   │   ├── DynamicAtlas.cs # 动态图集核心
│   │   │   └── DynamicAtlasManager.cs # 图集管理器
│   │   └── BlurBackground/    # 模糊背景效果
│   │       └── BlurController.cs # 模糊控制器
│   ├── PrefabReference.cs     # 预制体引用组件
│   └── UIConst.cs            # UI常量定义
├── Editor/                    # 编辑器工具
│   ├── Tools/                 # 开发工具集
│   │   ├── CodeGen/          # 代码生成工具
│   │   └── UIPrefabGen.cs    # UI预制体生成工具
│   └── UIExtend/              # UI扩展编辑器
│       ├── ScrollList/        # 滚动列表编辑器
│       └── DynamicAtlas/      # 动态图集编辑器
└── package.json               # Package配置文件
```

## 🎯 适用场景

- **手机游戏**：移动端UI优化，适配各种分辨率
- **PC游戏**：复杂UI界面，支持高分辨率显示  
- **应用软件**：企业级应用UI开发
- **VR/AR应用**：3D空间UI交互

## 🔧 系统要求

- **Unity版本**：2022.3 LTS 或更高版本
- **平台支持**：Windows、macOS、Linux、iOS、Android
- **渲染管线**：支持Built-in、URP、HDRP
- **.NET版本**：.NET Standard 2.1

## 📈 性能优势

| 特性 | 传统方案 | 本框架 | 性能提升 |
|------|----------|---------|----------|
| 大量UI元素 | 卡顿明显 | 流畅运行 | **10x+** |
| 纹理加载 | 频繁IO操作 | 动态合并 | **5x+** |
| 内存占用 | 持续增长 | 智能回收 | **-60%** |
| Draw Call | 数量较多 | 大幅减少 | **-80%** |

## 📚 文档和示例

### API文档
- [核心API参考](docs/api/core.md)
- [列表组件API](docs/api/scrolllist.md)
- [动态图集API](docs/api/dynamicatlas.md)
- [模糊背景API](docs/api/blur.md)

### 示例项目
- [基础UI演示](examples/BasicUI/)
- [无限列表示例](examples/InfiniteList/)
- [动态图集演示](examples/DynamicAtlas/)
- [模糊效果示例](examples/BlurEffect/)

## 🐛 常见问题

### Q: 如何自定义UI层级？
A: 在项目的 `UIEnumType.cs` 文件中修改 `E_UILayer` 枚举，添加您需要的层级。

### Q: 列表性能优化建议？
A: 
1. 合理设置Cell大小，避免过小或过大
2. 在OnShowCell中只更新必要的UI元素
3. 使用对象池管理复杂的Cell内容

### Q: 动态图集不生效怎么办？
A: 
1. 检查纹理导入设置，确保Read/Write Enable开启
2. 验证纹理格式兼容性
3. 查看Console错误日志

## 🤝 贡献指南

我们欢迎社区贡献！请遵循以下步骤：

1. Fork 本项目
2. 创建功能分支 (`git checkout -b feature/develop`)
3. 提交更改 (`git commit -m 'Add some Todo'`)
4. 推送到分支 (`git push origin feature/develop`)
5. 创建 Pull Request

### 开发路线图
- [ ] 支持Addressable资源管理
- [ ] 增加UI动画系统
- [ ] 集成MVVM架构模式
- [ ] 添加更多布局组件
- [ ] 优化移动端性能
- [ ] 支持多语言本地化
- [ ] 添加UI音效系统

### 代码规范
- 使用C#命名约定
- 添加XML文档注释
- 保持代码简洁可读
- 编写单元测试

## 📄 开源协议

本项目采用 MIT 协议开源 - 查看 [LICENSE](LICENSE) 文件了解详情。

## 🙏 致谢

感谢以下贡献者和项目：
- Unity Technologies 提供强大的游戏引擎
- 社区开发者的宝贵反馈和建议
- 所有为项目贡献代码的开发者

---

⭐ **如果这个项目对您有帮助，请给我们一个Star！**

🐛 **问题反馈**：[GitHub Issues](https://github.com/yunzeforbetter/UISystem-Package/issues)

## 更新日志

### v1.0.0
- 🎉 首次发布
- ✨ 实现智能UI堆栈管理
- ✨ 添加虚拟化无限列表
- ✨ 集成运行时动态图集
- ✨ 支持实时模糊背景效果
- 🛠️ 完善编辑器工具集
- 📚 添加完整文档和示例
