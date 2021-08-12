# BuildExtension

对UnityGameFramework 的 打包进行扩展  version 配置生成

# 使用教程

1. 配置UGF打包需要的配置文件 及路径

   配置参考 [使用 AssetBundle 编辑工具 | Game Framework](https://gameframework.cn/uncategorized/使用-assetbundle-编辑器/)  不过由于E大文档年代久远，新版本资源系统有些许不同   `AssetBundleXXXX--> ResourceXXX `  

   [GameFrameworkConfigs](./Editor/GameFrameworkConfigs.cs) 配置UGF 资源配置文件路径

   ```csharp
   public static class GameFrameworkConfigs
   {
       [BuildSettingsConfigPath]
       public static string BuildSettingsConfig = GameFramework.Utility.Path.GetRegularPath(Path.Combine(Application.dataPath, "Res/Configs/BuildSettings.xml"));
   
       [ResourceCollectionConfigPath]
       public static string ResourceCollectionConfig = GameFramework.Utility.Path.GetRegularPath(Path.Combine(Application.dataPath, "Res/Configs/ResourceCollection.xml"));
   
       [ResourceEditorConfigPath]
       public static string ResourceEditorConfig = GameFramework.Utility.Path.GetRegularPath(Path.Combine(Application.dataPath, "Res/Configs/ResourceEditor.xml"));
   
       [ResourceBuilderConfigPath]
       public static string ResourceBuilderConfig = GameFramework.Utility.Path.GetRegularPath(Path.Combine(Application.dataPath, "Res/Configs/ResourceBuilder.xml"));
   ```

2. 设置打包事件

   Unity 菜单栏 `Game Framework/Resource Tools/Resource Builder`

   `BuildEventHandle` 设置为[UGFExtensions.Build.Editor.BuildEventhandle](./Editor/BuildEventhandle.cs) 

3. 配置VersionInfo部分参数

   1. 在`Res/Configs` 目录下右键 选择`Create/UGFExtensions/Versioninfo` 创建VersionInfo

   2. Environment   打包环境（默认为Debug, 当前只有Debug 和Release  可以自行扩展）

      需要配置UpdatePrefixUri  不同环境下 uri可能不一样 所以需要自己选择不同环境配置

   3. 两种生成version的模式 

      * `IsGenerateToFullPath`  设置为True 时 

        自动将version.txt 生成到打包界面设置的FullPath下对应平台的目录

      * `IsGenerateToFullPath`  设置为False时

        可以自行选择生成路径 点击生成按钮生成

4. 打包

   打包前如果没有配置 VersionInfo 会自动生成VersionInfo.  生成位置配置在 [BuildEventhandle](./Editor/BuildEventhandle.cs)  88 行
