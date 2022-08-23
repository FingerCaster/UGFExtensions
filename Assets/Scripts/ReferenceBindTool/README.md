# ReferenceBindTool

引用绑定工具。 参考了`猫仙人`的[ComponentAutoBindTool](https://github.com/CatImmortal/ComponentAutoBindTool)。并在此基础上扩展 新增。

## 功能介绍

### 引用绑定组件

![image](https://tvax3.sinaimg.cn/large/e1b1a94bgy1h5gleo1uusj20en082ta1.jpg)

`BindAssetORPrefabRuleHelper`  

绑定资源或者预制体的规则帮助类（只可以绑定Project目录下的资源或预制体）

* `DefaultBindAssetOrPrefabRuleHelper`

  默认的绑定规则。不能绑定 **文件夹,脚本资源，“StreamingAsset” “Editor” “Resources”目录下的资源**

`BindComponentsRuleHelper` 

绑定组件的规则帮助类 （只可以绑定挂载组件物体和子物体身上符合规则的组件）

* `DefaultBindComponentsRuleHelper`

  通过在需要绑定的组件的物体上添加`BD_`前缀 绑定身上所有符合规则的组件。

  优点：不需要针对类型添加前缀。自动绑定组件。

  缺点：需要添加前缀 绑定的无用数据比较多。

* `SelectComponentBindComponentsRuleHelper`

  通过在需要绑定的组件的物体上挂载`BindDataSelect`组件选择需要绑定的组件,绑定符合规则的组件。 

  优点：不需要前缀，不会绑定无用组件。

  缺点：会增加大量无用的Mono空脚本 用来做选择。

* `TypePrefixBindRuleHelper`

  通过在需要绑定的组件的物体上添加 类型前缀方式 ，绑定符合规则的组件。

  优点：针对类型前缀的绑定 不会绑定无用的组件。

  缺点：需要添加大量前缀。

* `SelectComponentTreePopWindow`

  组件选择树。会根据挂载`ReferenceBindComponent`的物体生成一个树状编辑器 用于选择需要绑定的组件。

  ![image](https://tva3.sinaimg.cn/large/e1b1a94bgy1h5gntc947ij205j0283yd.jpg)

  优点：不需要添加前缀 不会增加无用脚本。

  缺点：需要手动选择,不能完全自动化绑定。

`CodeGeneratorRuleHelper`

代码生成规则帮助类

* `DefaultCodeGeneratorRuleHelper`

  默认代码生成规则，根据设置的`类名` `命名空间` 生成对应的 `c#` 代码。 

  **PS: 默认代码生成规则 只在挂载`ReferenceBindComponet`脚本的情况下 使用。**

  会根据绑定数据生成一个部分类。 主要代码为对应的字段 和`InitBindObjects`方法 

  需要在适当的地方调用`InitBindObjects` 初始化字段。

  ``` csharp
  namespace TestBind
  {
     public partial class TestBind
     {
         private GameObject m_GameObject_GameObject;
         private Transform m_Transform_TestBind;
         
         private void InitBindObjects(GameObject go)
         {
             m_GameObject_GameObject = bindComponent.GetBindObject<GameObject>(0);
             m_Transform_TestBind = bindComponent.GetBindObject<Transform>(1);
         }
     } 
  }
  ```

* `TransformFindCodeGeneratorRuleHelper`

  根据绑定的数据 生成`Transform.Find` 代码 。

  **PS: 生成Find代码规则 只在`NotAddComponetToolWindow`下使用**

  会根据绑定数据生成一个部分类。 主要代码为对应的字段 和对应的属性方法  其中属性方法调用时才会初始化对应字段。

  需要调用构造函数传入绑定对象。

  ```c#
  namespace TestBind
  {
     public partial class TestBind
     {
         private Transform m_Transform;
         public TestBindF(Transform transform)
         {
             m_Transform = transform;
         }
         private Transform m_Transform_TestBind;
         public Transform Transform_TestBind
         {
             get
             {
                 if(m_Transform_TestBind == null)
                 {
                     m_Transform_TestBind = m_Transform.GetComponent<Transform>();
                 }
                 return m_Transform_TestBind;
             }
         }       
     } 
  }
  ```



`绑定资源或预制体编辑器界面`  可以拖入`Project` 下面的资源物体。点击绑定按钮会根据选择的绑定规则绑定。

`SettingData 代码生成设置设置相关数据`

`SettingDataSearchable` 可以查询的代码生成数据。（因为可能有很多数据 所以增加了一个可以查询选择的编辑器）

![image](https://tvax1.sinaimg.cn/large/e1b1a94bgy1h5gp5gdpibj205w01y748.jpg)

​		`CodeGeneratorSettingData`

​		代码生成设置数据 可以设置 `代码生成目录` `命名空间`

​		`CodeGeneratorSettingConfig`

​		代码生成设置配置。 全局只需要生成一份 用于保存代码生成配置数据

​		![image](https://tva4.sinaimg.cn/large/e1b1a94bgy1h5gq148vadj20en056gm0.jpg)

​		`类名` 生成的代码的名称。可以使用默认的物体名 或者自己制定



###  不挂载脚本的组件绑定工具

![image](https://tvax1.sinaimg.cn/large/e1b1a94bgy1h5gpe60x0hj20gq07fgmt.jpg)

**基本功能和引用绑定组件一致。 不过只可以绑定对象身上的组件。**