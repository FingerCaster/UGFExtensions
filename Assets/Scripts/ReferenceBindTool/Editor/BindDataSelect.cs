using UnityEngine;

namespace ReferenceBindTool.Editor
{
    public class BindDataSelect : MonoBehaviour
    {
        [SerializeField] private Component[] m_BindComponents;

        public Component[] BindComponents
        {
            get => m_BindComponents;
        }
    }
}