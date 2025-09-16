静态图集image，支持动态替换图集icon，配合Unity静态图集使用
使用前置操作
1. Create->UI->UIAtlasSpriteMapData 创建图集信息映射
2. 在AtlasRootPath处填写放置全部静态图集的根目录路径
3. 点击 【生成】并保存
4. 将该数据放置到资源管理中，运行时优先加载并调用UIAtlasSpriteMapData.Init方法完成静态信息写入即可

完成上述前置操作后即可设置UIAtlasImage.SetSprite通过精灵名称设置精灵