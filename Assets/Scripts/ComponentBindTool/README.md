# ComponentBindTool

基于`猫仙人`的项目扩展  去掉繁杂的绑定前缀  **改为根据自定义的前缀和组件列表匹配** 大体功能不变 但是修改了部分编辑器代码 

将绑定方法 和属性设置方法抽出 到[ComponentAutoBindToolExtensions](./Editor/ComponentAutoBindToolExtensions.cs) 

将代码生成部分抽出 提供工具类[ComponentAutoBindToolUtility](./Editor/ComponentAutoBindToolUtility.cs)  

可以和其他自己的UI编辑器代码生成等工具 结合使用

本扩展依赖`Common/Searchable` 扩展。

## 使用方法

    代码生成配置存在于 `AutoBindSettingConfig` 可以自己增删 修改。

    不同的规则 工程内可以混用 但是一个ComponetAutoBindTool 只能使用一种规则

1.  根据名称前缀和对应的组件列表进行绑定 Helper: `DefaultAutoBindRuleHelper`
   
   需要绑定的组件物体前添加前缀 默认为`BD_` 可以自行扩展 添加`ComponentAutoBindTool` 组件
   
   ![image](https://tva3.sinaimg.cn/large/e1b1a94bgy1h30x811tj9j20c2075ab6.jpg)
   
   **PS: 默认绑定规则的绑定列表 只有`Transform` 组件 需要自行增加**

2. 根据类型前缀进行绑定 Helper:`TypePrefixBindRuleHelper`
   
   ![image](https://tvax4.sinaimg.cn/large/e1b1a94bgy1h30x65t1vzj20bu08fjsk.jpg)
   
   在需要绑定的物体命名增加类型对应的前缀 如果当前物体又多个需要绑定的组件 用 `_` 分割不同组件即可 例如 `Trans_Btn_Img_xxxx `。
   
   前缀类型映射 保存在`TypePrefixBindRuleHelper脚本中 m_PrefixesDict` 可以自行添加

3. 手动选择需要绑定的组件 Helper: `SelectComponentBindRuleHelper`
   
   再需要绑定的物体添加`BindDataSelect` 组件。选择需要的组件 点击绑定按钮。
   
   之后返回到ComponetAutoBindTool 脚本点击自动绑定即可
   
   ![image](https://tva2.sinaimg.cn/large/e1b1a94bgy1h30xbrl02kj20bs07c0tc.jpg)![image](https://tva4.sinaimg.cn/large/e1b1a94bgy1h30xcqp48gj20bp02r74d.jpg)
   
   ![image](https://tva2.sinaimg.cn/large/e1b1a94bgy1h30xeq13p3j20bv078gmq.jpg)



  不同Helper的优缺点。

   ` DefaultAutoBindRuleHelper`

       缺点 : 需要添加前缀 绑定的无用数据比较多。 

       优点：不需要针对类型添加前缀。自动绑定组件。

    `TypePrefixBindRuleHelper`

        缺点：需要添加大量前缀。

        优点：针对类型前缀的绑定 不会绑定无用的组件。

    `SelectComponentBindRuleHelper`

         缺点：会增加大量无用的Mono空脚本。

         优点：不需要前缀，不回绑定无用组件。

## 引用

[猫仙人项目源地址](https://github.com/CatImmortal/ComponentAutoBindTool)
