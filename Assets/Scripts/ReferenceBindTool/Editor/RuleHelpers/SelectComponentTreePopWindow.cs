using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ReferenceBindTool.Editor
{
    public class SelectComponentTreePopWindow : PopupWindowContent
    {
        private TreeViewState m_TreeViewState;
        private TreeView m_TreeView;
        private ReferenceBindComponent m_ReferenceBindComponent;  
        private Dictionary<int, bool> m_Select;


        public override Vector2 GetWindowSize()
        {
            return new Vector2(400, 600);
        }

        public void Show(ReferenceBindComponent bindComponentList)
        {
            var select = bindComponentList.BindComponents.ToDictionary(_ => _.BindObject.GetInstanceID(), _ => true);

            if (m_Select == null)
            {
                m_Select = new Dictionary<int, bool>(select);
            }
            else
            {
                m_Select.Clear();
                m_Select.AddRange(select);
            }

            if (m_ReferenceBindComponent != bindComponentList)
            {
                m_ReferenceBindComponent = bindComponentList;
                m_TreeViewState = new TreeViewState();
                m_TreeView = new ComponentTreeView(m_TreeViewState, bindComponentList.transform,m_Select);
            }
            
            m_TreeView.Reload();
            // if (m_TreeView != null)
            //     m_TreeView.SetSelection (Selection.instanceIDs);
            Rect rect = EditorGUILayout.GetControlRect();
            PopupWindow.Show(new Rect(rect.x - 400, rect.y, rect.width, rect.height), this);
        }


        public override void OnGUI(Rect rect)
        {
            DoToolbar();
            DoTreeView();
        }

        void DoTreeView()
        {
            Rect rect = GUILayoutUtility.GetRect(0, 100000, 0, 100000);
            m_TreeView.OnGUI(rect);
        }

        void DoToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        public override void OnClose()
        {
            // Debug.Log(LitJson.JsonMapper.ToJson(m_Select.ToDictionary(_=>_.Key.ToString())));
            var tempList = new List<ReferenceBindComponent.BindObjectData>(m_ReferenceBindComponent.BindComponents.Count);
            tempList.AddRange(m_ReferenceBindComponent.BindComponents);
            m_ReferenceBindComponent.BindObjects.Clear();
            m_ReferenceBindComponent.BindComponents.Clear();
            foreach (var selectItem in m_Select)
            {
                if (selectItem.Value)
                {
                    var bindData = tempList.Find(_ => _.BindObject.GetInstanceID() == selectItem.Key);
                    Component component = GetComponent(selectItem.Key);
                    string name = bindData == null ? m_ReferenceBindComponent.NameRuleHelper.GetDefaultFieldName(component) : bindData.FieldName;
                    m_ReferenceBindComponent.AddBindComponent(name, component, false);
                }
            }
            m_ReferenceBindComponent.SyncBindObjects();
            base.OnClose();
        }
        Component GetComponent (int instanceID)
        {
            return (Component)EditorUtility.InstanceIDToObject(instanceID);
        }
    }
}