namespace SoundSystem
{
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;
    using UnityEngine;
    
    /// <summary>
    /// サウンドシステム専用のロギングクラス<para></para>
    /// - エディタ上のログに加え、ログファイルを残す<para></para>
    ///   エディタでのパス：Application.dataPath, "../Logs"<para></para>
    ///   ビルド版でのパス：Application.persistentDataPath<para></para>
    /// - カテゴリ名には原則として呼び出し元のスクリプト名(拡張子なし)を使用<para></para>
    /// - SoundSystem系の内部設計において1ファイル = 1クラス構成であることを前提としている<para></para>
    /// - 複数クラスを1ファイルに定義した場合、ログカテゴリが曖昧になる可能性がある
    /// (必要であればログ呼び出し時にカテゴリを明示的に指定し対処可能)
    /// </summary>
    internal static class Log
    {
        private static StreamWriter fileWriter;
        private static readonly object locker = new();
        private static bool isInitialized = false;
    
        public enum LogLevel
        {
            Info,
            Warn,
            Error
        }
    
        public static void Initialize(string fileName = "SoundLog.txt")
        {
            if(isInitialized) return;
    
            string logDirectory;
    #if UNITY_EDITOR
            logDirectory = Path.Combine(Application.dataPath, "../Logs");
    #else
            logDirectory = Application.persistentDataPath;
    #endif
            Directory.CreateDirectory(logDirectory);
            string logPath = Path.Combine(logDirectory, fileName);
    
            try
            {
                fileWriter = new(logPath, append: true);
                fileWriter.AutoFlush = true;
                isInitialized = true;
                Safe($"Initialize成功: {logPath}");
            }
            catch (Exception e)
            {
                Error($"Initialize失敗,{e.Message}");
            }
        }
    
        /// <param name="category">未入力なら呼び出し元のクラス名が入る</param>
        public static void Safe(string message, [CallerFilePath] string category = "")
        {
            category = Path.GetFileNameWithoutExtension(category);
            Output(LogLevel.Info, category, message);
        }
    
        /// <param name="category">未入力なら呼び出し元のクラス名が入る</param>
        public static void Warn(string message, [CallerFilePath] string category = "")
        {
            category = Path.GetFileNameWithoutExtension(category);
            Output(LogLevel.Warn, category, message);
        }
    
        /// <param name="category">未入力なら呼び出し元のクラス名が入る</param>
        public static void Error(string message, [CallerFilePath] string category = "")
        {
            category = Path.GetFileNameWithoutExtension(category);
            Output(LogLevel.Error, category, message);
        }
    
        private static void Output(LogLevel level, string category, string message)
        {
            if (fileWriter == null)
            {
                return;
            }
    
            string timestamp   = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string fullMessage = $"[{timestamp}] [{level}] [{category}] {message}";
    
    #if UNITY_EDITOR
            switch (level)
            {
                case LogLevel.Info:
                    Debug.Log(fullMessage);
                    break;
    
                case LogLevel.Warn:
                    Debug.LogWarning(fullMessage);
                    break;
    
                case LogLevel.Error:
                    Debug.LogError(fullMessage);
                    break;
            }
    #endif
    
            //競合防止(呼び出し側に非同期処理が多いため)
            lock (locker)
            {
                fileWriter.WriteLine(fullMessage);
            }
        }
    
        public static void Close()
        {
            fileWriter?.Flush();
            fileWriter?.Close();
            fileWriter = null;
        }
    }
}
