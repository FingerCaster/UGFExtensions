using System.Collections.Generic;
using System;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;

namespace UGFExtensions.Editor
{
    /// <summary>
    /// 双击 Unity Editor 中的日志重定向
    /// </summary>
    public class EditorLogRedirect
    {
        private static EditorLogRedirect m_Instance;

        public static EditorLogRedirect GetInst()
        {
            if (m_Instance == null)
            {
                m_Instance = new EditorLogRedirect();
            }

            return m_Instance;
        }

        //替换成你自己的封装类地址
        private const string DEBUGERFILEPATH = "Assets/GameFramework/Scripts/Runtime/Utility/DefaultLogHelper.cs";
        private int m_DebuggerFileInstanceId;
        private Type m_ConsoleWindowType = null;
        private FieldInfo m_ActiveTextInfo;
        private FieldInfo m_ConsoleWindowFileInfo;

        private EditorLogRedirect()
        {
            UnityEngine.Object debuggerFile = AssetDatabase.LoadAssetAtPath(DEBUGERFILEPATH, typeof(UnityEngine.Object));
            m_DebuggerFileInstanceId = debuggerFile.GetInstanceID();
            m_ConsoleWindowType = Type.GetType("UnityEditor.ConsoleWindow,UnityEditor");
            m_ActiveTextInfo = m_ConsoleWindowType.GetField("m_ActiveText", BindingFlags.Instance | BindingFlags.NonPublic);
            m_ConsoleWindowFileInfo = m_ConsoleWindowType.GetField("ms_ConsoleWindow", BindingFlags.Static | BindingFlags.NonPublic);
        }

        [UnityEditor.Callbacks.OnOpenAssetAttribute(-1)]
        private static bool OnOpenAsset(int instanceID, int line)
        {
            if (instanceID == EditorLogRedirect.GetInst().m_DebuggerFileInstanceId)
            {
                return EditorLogRedirect.GetInst().FindCode();
            }

            return false;
        }

        public bool FindCode()
        {
            var windowInstance = m_ConsoleWindowFileInfo.GetValue(null);
            var contentStrings = m_ActiveTextInfo.GetValue(windowInstance).ToString().Split('\n');

            var filePath = new List<string>();
            bool firstLog = false;
            foreach (var item in contentStrings)
            {
                if (firstLog == false)
                {
                    // to ignore exception log
                    firstLog = item.StartsWith("UnityEngine.Debug:Log");
                    continue;
                }

                if (item.Contains("at"))
                {
                    filePath.Add(item);
                }
            }

            if (IsGfLog(filePath[0]))
            {
                /*
                 * GF log stack is
                 *  0. DefaultLogHelper.cs
                 *  1. GameFrameworkLog.cs
                 *  2. Log.cs
                 *  3. realLogfile
                 */
                return PingAndOpen(filePath[3]);
            }

            return PingAndOpen(filePath[1]);
        }

        private bool IsGfLog(string lastLog)
        {
            return lastLog.Contains(":Log (GameFramework.GameFrameworkLogLevel,object) (at");
        }

        public bool PingAndOpen(string fileContext)
        {
            string regexRule = @"at ([\w\W]*):(\d+)\)";
            Match match = Regex.Match(fileContext, regexRule);
            if (match.Groups.Count > 1)
            {
                string path = match.Groups[1].Value;
                string line = match.Groups[2].Value;
                UnityEngine.Object codeObject = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));
                if (codeObject == null)
                {
                    return false;
                }

                EditorGUIUtility.PingObject(codeObject);
                AssetDatabase.OpenAsset(codeObject, int.Parse(line));
                return true;
            }

            return false;
        }
    }
}