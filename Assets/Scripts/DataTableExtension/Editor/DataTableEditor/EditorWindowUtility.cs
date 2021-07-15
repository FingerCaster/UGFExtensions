using UnityEditor;
using UnityEngine;

namespace DataTableEditor
{
    public static class EditorWindowUtility
    {
        public static T CreateWindow<T>(string title) where T : EditorWindow
        {
            T editorWindow = ScriptableObject.CreateInstance<T>();
            if (title != null)
            {
                editorWindow.titleContent = new GUIContent(title);
            }
            return editorWindow;
        }
    }
}