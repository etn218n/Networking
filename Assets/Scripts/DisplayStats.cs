using System;
using System.Collections.Generic;
using System.Text;
using Mirror;
using UnityEngine;

namespace Utilities
{
    public class DisplayStats : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool showLog;
        [SerializeField] private int fontSize   = 16;
        [SerializeField] private int bufferSize = 200;

        private float deltaTime;
        private string finalLog;
        private Vector2 scrollPosition;
        private Queue<string> logQueue;
        private Texture2D backgroundTexture;
        
        private static DisplayStats instance;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this.gameObject);
                return;
            }

            instance = this;
            finalLog = string.Empty;
            logQueue = new Queue<string>();
            backgroundTexture = GenerateColorTexture(1, 1, new Color(40f / 256f, 44f / 256f, 52f / 256f));
            
            Application.logMessageReceivedThreaded += HandleLog;

            DontDestroyOnLoad(this.gameObject);
        }

        private void Update()
        {
            if (Application.isBatchMode)
                return;

            if (Input.GetKeyDown(KeyCode.F1))
                showLog = !showLog;
            
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        }
        
        private void HandleLog(string log, string stacktrace, LogType logType)
        {
            var coloredLog = ColorLog(log, logType);
            var stampedLog = StampLog();
            
            logQueue.Enqueue(stampedLog + coloredLog);

            ClearOldMessages();

            finalLog = BuildGUIMessage(logQueue);
        }
        
        private void ClearOldMessages()
        {
            while (logQueue.Count > bufferSize)
                logQueue.Dequeue();
        }

        private string BuildGUIMessage(IEnumerable<string> messages)
        {
            var builder = new StringBuilder();

            foreach (var message in messages)
                builder.Append($"{message}\n");

            return builder.ToString();
        }
        
        private string ColorLog(string content, LogType logType)
        {
            switch (logType)
            {
                case LogType.Error:
                    return $"<color=#E65D61> {content}</color>";
                case LogType.Assert:
                    return $"<color=#6697B5> {content}</color>";
                case LogType.Warning:
                    return $"<color=#F9AE57> {content}</color>";
                case LogType.Log:
                    return $"<color=#ABB2BF> {content}</color>";
                case LogType.Exception:
                    return $"<color=#E65D61> {content}</color>";
                default:
                    throw new ArgumentOutOfRangeException(nameof(logType), logType, null);
            }
        }

        private string StampLog()
        {
            var timeStamp = DateTime.Now.ToString("HH:mm:ss");
            
            return $"<color=#ABB2BF><b>[{timeStamp}]:</b></color>";
        }
        
        private void OnGUI()
        {
            if (showLog)
                DrawLogs();
            
            DrawPerformanceStats();
        }
        
        private void DrawLogs()
        {
            var style = GUI.skin.textArea;
            
            style.richText = true;
            style.wordWrap = true;
            style.margin   = new RectOffset(0, 0, 0, 1);
            style.padding  = new RectOffset(7, 7, 7, 7);
            style.fontSize = fontSize;
            style.normal.background = backgroundTexture;

            var scrollBarStyle = GUI.skin.verticalScrollbar;
            scrollBarStyle.margin = new RectOffset(0, 0, 0, 0);
            scrollBarStyle.normal.background = backgroundTexture;

            scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, false, GUIStyle.none, scrollBarStyle, GUIStyle.none, GUILayout.Width(Screen.width), GUILayout.Height((float)Screen.height / 4));

            GUILayout.Label(finalLog, style);
            
            GUILayout.EndScrollView();
        }
        
        private void DrawPerformanceStats()
        {
            var style = new GUIStyle();
            
            style.richText = true;
            style.wordWrap = true;
            style.padding  = new RectOffset(7, 7, 7, 7);
            style.fontSize = fontSize;

            var fps = CalculateFPS().ToString("F0");
            var ping = Math.Round(NetworkTime.rtt * 1000);
            var ticks = GameManager.Instance.ServerTicks;

            GUILayout.BeginArea(new Rect(0, Screen.height - fontSize * 2, Screen.width, fontSize * 2));
            GUILayout.Label($"<color=#ABB2BF>FPS: {fps}       Ping: {ping} ms       Server ticks: {ticks}</color>", style);
            GUILayout.EndArea();
        }
        
        private float CalculateFPS()
        {
            return 1f / deltaTime;
        }
        
        private Texture2D GenerateColorTexture(int width, int height, Color color)
        {
            var pixelMap = new Color[width * height];
 
            for(int i = 0; i < pixelMap.Length; i++)
                pixelMap[i] = color;
 
            var texture2D = new Texture2D(width, height);
            
            texture2D.SetPixels(pixelMap);
            texture2D.Apply();
 
            return texture2D;
        }
    }
}