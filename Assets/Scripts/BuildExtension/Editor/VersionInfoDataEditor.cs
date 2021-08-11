using System;
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
            if (m_InternalGameVersion.intValue == 0)
            {
                m_InternalGameVersion.intValue = EditorPrefs.GetInt(
                    m_Environment.enumValueIndex == (int)VersionInfoData.EnvironmentType.Debug
                        ? "DebugInternalGameVersion"
                        : "ReleaseInternalGameVersion", 0);
            }
            
            m_UpdatePrefixUri = serializedObject.FindProperty("m_UpdatePrefixUri");
            m_VersionListLength = serializedObject.FindProperty("m_VersionListLength");
            m_InternalResourceVersion = serializedObject.FindProperty("m_InternalResourceVersion");
            m_VersionListHashCode = serializedObject.FindProperty("m_VersionListHashCode");
            m_VersionListCompressedLength = serializedObject.FindProperty("m_VersionListCompressedLength");
            m_VersionListCompressedHashCode = serializedObject.FindProperty("m_VersionListCompressedHashCode");
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(m_Environment);
            if (m_LastEnvironment != m_Environment.enumValueIndex )
            {
                m_InternalGameVersion.intValue = EditorPrefs.GetInt(
                    m_Environment.enumValueIndex == (int)VersionInfoData.EnvironmentType.Debug
                        ? "DebugInternalGameVersion"
                        : "ReleaseInternalGameVersion", 0);
                m_LastEnvironment = m_Environment.enumValueIndex;
            }
            EditorGUILayout.PropertyField(m_ForceUpdateGame);
            EditorGUILayout.PropertyField(m_LatestGameVersion);
            EditorGUILayout.PropertyField(m_InternalGameVersion);
            EditorGUILayout.PropertyField(m_UpdatePrefixUri);
            bool isValidUri = CheckUri(m_UpdatePrefixUri.stringValue);
            if (!isValidUri)
            {
                EditorGUILayout.HelpBox("UpdatePrefixUri is Not Valid!", MessageType.Error);
            }

            EditorGUILayout.PropertyField(m_InternalResourceVersion);
            EditorGUILayout.PropertyField(m_VersionListLength);
            EditorGUILayout.PropertyField(m_VersionListHashCode);
            EditorGUILayout.PropertyField(m_VersionListCompressedLength);
            EditorGUILayout.PropertyField(m_VersionListCompressedHashCode);
            serializedObject.ApplyModifiedProperties();


            if (GUILayout.Button("生成Txt")&& isValidUri)
            {
                if (m_Environment.enumValueIndex == (int)VersionInfoData.EnvironmentType.Debug)
                {
                    int internalGameVersion = EditorPrefs.GetInt("DebugInternalGameVersion", 0);
                    if (m_InternalGameVersion.intValue!= internalGameVersion)
                    {
                        EditorPrefs.SetInt("DebugInternalGameVersion", m_InternalGameVersion.intValue);
                    }
                }
                else
                {
                    int internalGameVersion = EditorPrefs.GetInt("ReleaseInternalGameVersion", 0);
                    if (m_InternalGameVersion.intValue!= internalGameVersion)
                    {
                        EditorPrefs.SetInt("ReleaseInternalGameVersion", m_InternalGameVersion.intValue);
                    }
                }
                VersionInfo versionInfo = new VersionInfo
                {
                    InternalGameVersion = m_InternalGameVersion.intValue,
                    ForceUpdateGame = m_ForceUpdateGame.boolValue,
                    LatestGameVersion = m_LatestGameVersion.stringValue,
                    UpdatePrefixUri = m_UpdatePrefixUri.stringValue,
                    InternalResourceVersion = m_InternalResourceVersion.intValue,
                    VersionListLength = m_VersionListLength.intValue,
                    VersionListHashCode = m_VersionListHashCode.intValue,
                    VersionListCompressedLength = m_VersionListCompressedLength.intValue,
                    VersionListCompressedHashCode = m_VersionListCompressedHashCode.intValue
                };

                Debug.Log(LitJson.JsonMapper.ToJson(versionInfo));
            }
        }

        private bool CheckUri(string uri)
        {
            Regex regex = new Regex(@"^[A-Za-z]+://[A-Za-z0-9-_]+\.[A-Za-z0-9-_%&?/.=]+$");
            return regex.IsMatch(uri);
        }
    }
}