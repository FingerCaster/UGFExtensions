using UnityEditor;
using UnityEngine;

namespace UGFExtensions.Build.Editor
{
    [CustomPropertyDrawer(typeof(VersionInfoData))]
    public class VersionInfoDataDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property,
            GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            var rect = new Rect(position.x, position.y - 20, position.width, 18);
            DrawProperty(ref rect, property.FindPropertyRelative("m_ForceUpdateGame"));
            DrawProperty(ref rect, property.FindPropertyRelative("m_LatestGameVersion"));
            DrawProperty(ref rect, property.FindPropertyRelative("m_InternalGameVersion"));
            DrawProperty(ref rect, property.FindPropertyRelative("m_UpdatePrefixUri"));
            bool isValidUri = Utility.Uri.CheckUri(property.FindPropertyRelative("m_UpdatePrefixUri").stringValue);
            if (!isValidUri)
            {
                rect = new Rect(rect.x+30, rect.y + 20, rect.width, 35);
                EditorGUI.HelpBox(rect, "UpdatePrefixUri is Not Valid!", MessageType.Error);
                rect.y += 20;
                rect.x -= 30;
            }

            DrawProperty(ref rect, property.FindPropertyRelative("m_IsShowCanNotChangeProperty"));
            if (property.FindPropertyRelative("m_IsShowCanNotChangeProperty").boolValue)
            {
                DrawProperty(ref rect, property.FindPropertyRelative("m_InternalResourceVersion"));
                DrawProperty(ref rect, property.FindPropertyRelative("m_VersionListLength"));
                DrawProperty(ref rect, property.FindPropertyRelative("m_VersionListHashCode"));
                DrawProperty(ref rect, property.FindPropertyRelative("m_VersionListCompressedLength"));
                DrawProperty(ref rect, property.FindPropertyRelative("m_VersionListCompressedHashCode"));
            }
            EditorGUI.EndProperty();
        }

        private void DrawProperty(ref Rect position, SerializedProperty property)
        {
            position = new Rect(position.x, position.y + 20, position.width, 18);
            EditorGUI.PropertyField(position, property);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = 240f;
            bool isShow = property.FindPropertyRelative("m_IsShowCanNotChangeProperty").boolValue;
            if (!isShow)
            {
                height -= 100;
            }
            bool isValidUri = Utility.Uri.CheckUri(property.FindPropertyRelative("m_UpdatePrefixUri").stringValue);
            if (isValidUri)
            {
                height -= 40;
            }
            return height;
        }
    }
}