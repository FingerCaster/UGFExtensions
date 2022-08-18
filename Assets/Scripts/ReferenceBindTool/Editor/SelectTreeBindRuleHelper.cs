#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace ReferenceBindTool
{
    public class SelectTreeBindRuleHelper : IAutoBindRuleHelper
    {
        private SelectComponentTreePopWindow m_PopWindow;

        public SelectTreeBindRuleHelper()
        {
            m_PopWindow = new SelectComponentTreePopWindow();
        }
        public void GetBindData(Transform target, List<string> filedNames, List<Component> components)
        {
            throw new NotImplementedException();
        }

        public void AddBindComponents(ReferenceBindComponent referenceBindComponent)
        {
            m_PopWindow.Show(referenceBindComponent);
        }
    }
}
#endif