using UnityEngine;

namespace ReferenceBindTool.Editor
{
    public class BindDataSelect : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] private Component[] m_BindComponents;

        public Component[] BindComponents
        {
            get => m_BindComponents;
        }
#endif
    }
}