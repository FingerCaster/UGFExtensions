using System;
using System.IO;
using System.Text.RegularExpressions;
using GameFramework;
using UnityEditor;
using UnityEngine;

namespace UGFExtensions.Build.Editor
{
    [CustomEditor(typeof(VersionInfoData))]
    public class VersionInfoDataEditor : UnityEditor.Editor
    {
        private SerializedProperty m_ForceUpdateGame;
        private SerializedProperty m_LatestGameVersion;
        private SerializedProperty m_InternalGameVersion;
        private SerializedProperty m_UpdatePrefixUri;
        private SerializedProperty m_VersionListLength;
        private SerializedProperty m_InternalResourceVersion;
        private SerializedProperty m_VersionListHashCode;
        private SerializedProperty m_VersionListCompressedLength;
        private SerializedProperty m_VersionListCompressedHashCode;
        private SerializedProperty m_Environment;
        private SerializedProperty m_IsGenerateToFullPath;
        private SerializedProperty m_OutPath;
        private SerializedProperty m_IsShowCanNotChangeProperty;
        private string m_LastUpdatePrefixUri;
        private int m_LastEnvironment;

        private void OnEnable()
        {
            m_Environment = serializedObject.FindProperty("m_Environment");
            m_LastEnvironment = m_Environment.enumValueIndex;
            m_ForceUpdateGame = serializedObject.FindProperty("m_ForceUpdateGame");
            m_LatestGameVersion = serializedObject.FindProperty("m_LatestGameVersion");

            if (string.IsNullOrEmpty(m_LatestGameVersion.stringValue))
            {
                m_LatestGameVersion.stringValue = Application.version;
            }

            m_InternalGameVersion = serializedObject.FindProperty("m_InternalGameVersion");
            m_InternalGameVersion.intValue = EditorPrefs.GetInt($"{m_Environment.enumNames[m_Environment.enumValueIndex]}InternalGameVersion", 0);

            m_UpdatePrefixUri = serializedObject.FindProperty("m_UpdatePrefixUri");
            m_UpdatePrefixUri.stringValue = EditorPrefs.GetString($"{m_Environment.enumNames[m_Environment.enumValueIndex]}UpdatePrefixUri", string.Empty);
            m_LastUpdatePrefixUri = m_UpdatePrefixUri.stringValue;
            m_VersionListLength = serializedObject.FindProperty("m_VersionListLength");
            m_InternalResourceVersion = serializedObject.FindProperty("m_InternalResourceVersion");
            m_VersionListHashCode = serializedObject.FindProperty("m_VersionListHashCode");
            m_VersionListCompressedLength = serializedObject.FindProperty("m_VersionListCompressedLength");
            m_VersionListCompressedHashCode = serializedObject.FindProperty("m_VersionListCompressedHashCode");
            m_IsGenerateToFullPath = serializedObject.FindProperty("m_IsGenerateToFullPath");
            m_OutPath = serializedObject.FindProperty("m_OutPath");
            m_IsShowCanNotChangeProperty = serializedObject.FindProperty("m_IsShowCanNotChangeProperty");
            
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(m_Environment);
            if (m_LastEnvironment != m_Environment.enumValueIndex )
            {
                m_LastEnvironment = m_Environment.enumValueIndex;

                m_InternalGameVersion.intValue = EditorPrefs.GetInt($"{m_Environment.enumNames[m_Environment.enumValueIndex]}InternalGameVersion", 0);
                m_UpdatePrefixUri.stringValue = EditorPrefs.GetString($"{m_Environment.enumNames[m_Environment.enumValueIndex]}UpdatePrefixUri", string.Empty);
                m_LastUpdatePrefixUri = m_UpdatePrefixUri.stringValue;
            }
            EditorGUILayout.PropertyField(m_ForceUpdateGame);
            EditorGUILayout.PropertyField(m_LatestGameVersion);
            EditorGUILayout.PropertyField(m_InternalGameVersion);
            if (m_LastUpdatePrefixUri != m_UpdatePrefixUri.stringValue)
            {
                EditorPrefs.SetString($"{m_Environment.enumNames[m_Environment.enumValueIndex]}UpdatePrefixUri",m_UpdatePrefixUri.stringValue);
                m_LastUpdatePrefixUri = m_UpdatePrefixUri.stringValue;
            }
            EditorGUILayout.PropertyField(m_UpdatePrefixUri);
            bool isValidUri = Utility.Uri.CheckUri(m_UpdatePrefixUri.stringValue);
            if (!isValidUri)
            {
                EditorGUILayout.HelpBox("UpdatePrefixUri is Not Valid!", MessageType.Error);
            }
            EditorGUILayout.PropertyField(m_IsShowCanNotChangeProperty);
            if (m_IsShowCanNotChangeProperty.boolValue)
            {
                EditorGUILayout.PropertyField(m_InternalResourceVersion);
                EditorGUILayout.PropertyField(m_VersionListLength);
                EditorGUILayout.PropertyField(m_VersionListHashCode);
                EditorGUILayout.PropertyField(m_VersionListCompressedLength);
                EditorGUILayout.PropertyField(m_VersionListCompressedHashCode);
            }
          
            EditorGUILayout.PropertyField(m_IsGenerateToFullPath);
            if (!m_IsGenerateToFullPath.boolValue)
            {
                EditorGUILayout.Space();
                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("文件生成地址:", m_OutPath.stringValue);
                if (GUILayout.Button("选择路径"))
                {
                    m_OutPath.stringValue = EditorUtility.SaveFilePanel("选择生成地址", String.Empty, "version", "txt");
                }
                EditorGUILayout.EndHorizontal();
                if (string.IsNullOrEmpty(m_OutPath.stringValue))
                {
                    EditorGUILayout.HelpBox("OutPath is Not Valid!", MessageType.Error);
                }
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                if (GUILayout.Button("生成Txt")&& isValidUri)
                {
                    int internalGameVersion =   EditorPrefs.GetInt($"{m_Environment.enumNames[m_Environment.enumValueIndex]}InternalGameVersion", 0);
                    if (m_InternalGameVersion.intValue!= internalGameVersion)
                    {
                        EditorPrefs.SetInt($"{m_Environment.enumNames[m_Environment.enumValueIndex]}InternalGameVersion", m_InternalGameVersion.intValue);
                    }
                    VersionInfoData data = target as VersionInfoData;
                    if (data == null)
                    {
                        return;
                    }
                    File.WriteAllText(m_OutPath.stringValue,data.ToVersionInfoJson());
                    EditorUtility.RevealInFinder(m_OutPath.stringValue);
                }
            }
            
            serializedObject.ApplyModifiedProperties();

        }

        
    }
}