using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using ReferenceData = ReferenceBindTool.ReferenceBindComponent.ReferenceData;
namespace ReferenceBindTool.Editor
{
    public class AddBindDataWindow : PopupWindowContent
    {
        private class SelectBindObject
        {
            private Object m_BindObject;
            private bool m_IsSelect;

            public SelectBindObject(Object bindObject)
            {
                m_BindObject = bindObject;
                m_IsSelect = false;
            }

            public Object BindObject => m_BindObject;

            public bool IsSelect
            {
                get => m_IsSelect;
                set => m_IsSelect = value;
            }
        }
        
        private List<SelectBindObject> m_CanBindObjects;
        private int m_ReferenceId;

        public int ReferenceId => m_ReferenceId;

        public List<Object> GetSelectObjects()
        {
            return m_CanBindObjects.Where(_ => _.IsSelect).Select(_ => _.BindObject).ToList();
        }

        private List<Object> m_AllBindObjects;
        private Page m_Page;
        
        public override Vector2 GetWindowSize()
        {
            return new Vector2(300, 220);
        }
        private void SetBindReference(ReferenceData referenceData)
        {
            m_ReferenceId = referenceData.BindReference.GetInstanceID();
            m_AllBindObjects = new List<Object>(referenceData.BindObjects.Count);
            var excludeObjects = new List<Object>(referenceData.BindObjects.Count);
            excludeObjects.AddRange(referenceData.BindObjects.Select(_=>_.BindObject));
            m_AllBindObjects.Add(referenceData.BindReference);
            GameObject go = referenceData.BindReference as GameObject;
            if (go != null)
            {
                m_AllBindObjects.AddRange(go.GetComponentsInChildren<Component>(true).Where(_=>_.GetType()!= typeof(ReferenceBindComponent)));
            }
            m_CanBindObjects = m_AllBindObjects.Where(_ => !excludeObjects.Contains(_)).Select(_=> new SelectBindObject(_)).ToList();
            m_Page = new Page(10,m_CanBindObjects.Count);
        }
        
        public void Show(Rect rect,ReferenceData referenceData)
        {
            SetBindReference(referenceData);
            PopupWindow.Show(new Rect(rect.x-300,rect.y,rect.width,rect.height),this);
        }

        public override void OnGUI(Rect rect)
        {
            foreach (var bindObject in m_CanBindObjects)
            {
                EditorGUILayout.BeginHorizontal();
                bindObject.IsSelect = EditorGUILayout.Toggle(bindObject.IsSelect,GUILayout.Width(20));
                EditorGUILayout.LabelField(bindObject.BindObject.GetType().Name,GUILayout.Width(100));
                GUI.enabled = false;
                EditorGUILayout.ObjectField(bindObject.BindObject, typeof(Object), true);
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();

            }
            m_Page.Draw();
        }

        private bool m_IsChanged = false;

        public bool IsChanged
        {
            get => m_IsChanged;
            set => m_IsChanged = value;
        }

        public override void OnClose()
        {
            foreach (var bindObject in m_CanBindObjects)
            {
                if (!bindObject.IsSelect) continue;
                m_IsChanged = true;
                break;
            }
            base.OnClose();
        }
    }
}