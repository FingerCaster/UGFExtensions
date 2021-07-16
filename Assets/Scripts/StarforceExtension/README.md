# StarforceExtension

[StarForce](https://github.com/EllanJiang/StarForce) 中对UGF 的扩展整合

## 说明

- Base 

  - GameEntry ---- 游戏组件集合

- DataTable

  - DRxxxx ---- 游戏中用到的配置表

- Definition

  - Constans 
    - AssetPriority ---- 资源优先级。
    - Layer----层级。
    - ProcedureData---- 游戏内流程用到的数据
    - Setting ---- 游戏内设置
  - DataStruct
    - BuildInfo ---- 存放各平台下载地址 和 资源检查地址
    - VersionInfo ---- 资源热更版本信息
  - Enum
    - QualityLevelType ---- 质量等级
    - SceneId --- 场景ID

- Entity

  - EntityData
    - EntityData ---- 实体数据
  - EntityLogic
    - EntityLogic ---- 实体逻辑

- Extensions

  - Entity
    - EntityExtension ---- 对实体组件的扩展 (显示 隐藏 附加 ...)
  - Localization
    - LocalizationExtension ----  加载本地化字典扩展
  - Sound
    - SoundExtension ---- 对声音组件的扩展（播放 暂停 ...）
  - UI
    - UIExtension ---- 对UI组件的扩展(打开界面，关闭，判断存在...)

- Helper

  - DataTable
    - DataTableHelper ---- 数据表帮助类 解析读取数据表
  - Json
    - LitJsonHelper ---- LisJson帮助类 解析json 替换为Litjson
  - Localization
    - XmlLocalizationHelper ---- xml本地化加载帮助类
  - UI
    - UguiGroupHelper ---- ugui UI组帮助类

- Litjson ----  [LitJson](https://github.com/LitJSON/litjson)

- UI

  - Base
    - UGuiForm ---- Ugui 界面基类
  - UIFormId ---- ui 界面ID 枚举

- Utility

  - AssetUtility ---- 资源加载地址工具
  - WebUtility ---- 转义Uri工具

  

