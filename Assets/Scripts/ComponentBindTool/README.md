# ComponentBindTool

基于`猫仙人`的项目扩展  去掉繁杂的绑定前缀  **改为根据自定义的前缀和组件列表匹配** 大体功能不变 但是修改了部分编辑器代码 

将绑定方法 和属性设置方法抽出 到[ComponentAutoBindToolExtensions](./Editor/ComponentAutoBindToolExtensions.cs) 
将代码生成部分抽出 提供工具类[ComponentAutoBindToolUtility](./Editor/ComponentAutoBindToolUtility.cs)  
可以和其他自己的UI编辑器代码生成等工具 结合使用

## 使用方法
需要绑定的组件物体前添加前缀 默认为`BD_`  可以自行扩展  添加`ComponentAutoBindTool` 组件

![image](https://tva3.sinaimg.cn/large/e1b1a94bgy1h09cxq1ewbj20bv073aaw.jpg)
点击自动绑定 会自动搜索带有`BD_`前缀的子物体  并根据前缀对应的组件列表进行查找绑定

**PS: 默认绑定规则的绑定列表 只有`Transform` 组件 需要自行增加**

## 扩展
1. 在扩展基础上增加前缀 
	继承`IBindRule` 接口  并实现属性 详情可以参考默认[DefaultAuToBindRuleHelper](./DefaultAuToBindRuleHelper.cs)
2. 自定义方式扩展
	继承[IAutoBindRuleHelper](./IAutoBindRuleHelper.cs)  实现`GetBindData` 接口 自定义绑定规则

## 引用
[猫仙人项目源地址](https://github.com/CatImmortal/ComponentAutoBindTool)
