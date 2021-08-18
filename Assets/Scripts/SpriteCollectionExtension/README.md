# SpriteCollectionExtension

收集Sprite  扩展Sprite 加载卸载 管理

Unity Sprite有个问题 是如果Sprite在预制体上有依赖  加载类型会此变成Texture2d  从而导致加载Sprite失败  

针对这个问题  写了这个扩展  收集需要加载的Sprite  生成一个SpriteCollection  加载Sprite时 通过SpriteCollention 获取Sprite 

**PS: 当前版本 编辑器采用Odin 实现 需要自行导入Odin 插件( odin 太好用了 强烈推荐)  未来会增加通用编辑器（基本写完了 还需要完善。）** 

# 使用方法

## SpriteCollection 使用方法

1. Project界面右键  `UGFExtensions/SpriteCollection`创建SpriteCollection
2. SpriteCollect 支持 单个Sprite  multiple类型的图片 文件夹  直接拖到`Objects` 上即可
3. `PackPreview` 可以立即添加Sprite 到 SpriteCollection的字典中。 如果不点击 会在（打包,打AB包，编辑器进入播放状态）自动进行添加  Ps： 打ab包预处理 只针对UGF  通过BuildEventHandle 的接口进行处理

## SpriteCollection 的加载 卸载

SpriteCollection 的加载 卸载 通过[SpriteCollectionComponent](./SpriteCollectionComponent.cs) 进行控制。用户无需手动管理。

`SpriteCollectionComponent ` 中 `m_AutoReleaseInterval` 用于配置自动释放时间间隔  默认60s回收一次 可以自行在检视面板调整

## Sprite 加载

加载Sprite 通过 `SpriteCollectionComponent` 的`SetSprite`方法

``` csharp
  public async void SetSprite(ISetSpriteObject setSpriteObject)
```

```csharp
  public interface ISetSpriteObject
    {
        /// <summary>
        /// 精灵名称
        /// </summary>
        string SpritePath { get;}
        /// <summary>
        /// 精灵所在收集器地址
        /// </summary>
        string CollectionPath { get;}
        /// <summary>
        /// 设置精灵
        /// </summary>
        void SetSprite(Sprite sprite);
        /// <summary>
        /// 是否可以回收
        /// </summary>
        bool IsCanRelease();
    }
```

`SetSprite` 需要提供一个 实现了`ISetSpriteObject` 接口的对象。 

`WaitSetImage` 为针对Image 进行的包装   如果需要使用其他类型 加载Sprite 可参照其 进行拓展

`SetSpriteExtensions` 脚本 提供了Image 的扩展方法`SetSprite`  

加载Sprite :

```csharp
image.SetSprite("Assets/xxxx/xxx.asset","Assets/xxxx/xxxx/xxxx.png");
```

其中参数` collectionPath` `spritePath` 均为Assets下全路径   `spritePath` 可以参照`SpriteCollection` 中字典的key   如果只是单独sprite ，key既是 sprite的路径   如果是multiple 类型图片  key是 图片路径+分割成小图的名字。

