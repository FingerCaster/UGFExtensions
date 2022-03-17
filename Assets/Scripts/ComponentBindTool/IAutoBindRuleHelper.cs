using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 自动绑定规则辅助器接口
/// </summary>
public interface IAutoBindRuleHelper
{
    /// <summary>
    /// 获取绑定数据
    /// </summary>
    /// <param name="target">绑定物体</param>
    /// <param name="filedNames">字段名集合</param>
    /// <param name="components">组件集合</param>
    /// <returns>返回绑定数据</returns>
    void GetBindData(Transform target,List<string> filedNames,List<Component> components);
}
