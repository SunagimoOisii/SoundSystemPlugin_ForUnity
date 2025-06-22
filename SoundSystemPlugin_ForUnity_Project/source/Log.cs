namespace SoundSystem
{
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;
    using UnityEngine;
    
    /// <summary>
    /// TEhVXep̃MONX<para></para>
    /// - GfB^̃OɉAOt@Cc<para></para>
    ///   GfB^ł̃pXFApplication.dataPath, "../Logs"<para></para>
    ///   rhłł̃pXFApplication.persistentDataPath<para></para>
    /// - JeSɂ͌ƂČĂяõXNvg(gqȂ)gp<para></para>
    /// - SoundSystemn̓݌vɂ1t@C = 1NX\ł邱ƂOƂĂ<para></para>
    /// - NX1t@Cɒ`ꍇAOJeSBɂȂ\
    /// (Kvł΃OĂяoɃJeS𖾎IɎw肵Ώ\)
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
                Safe($"Initialize: {logPath}");
            }
            catch (Exception e)
            {
                Error($"Initializes,{e.Message}");
            }
        }
    
        /// <param name="category">͂ȂĂяõNX</param>
        public static void Safe(string message, [CallerFilePath] string category = "")
        {
            category = Path.GetFileNameWithoutExtension(category);
            Output(LogLevel.Info, category, message);
        }
    
        /// <param name="category">͂ȂĂяõNX</param>
        public static void Warn(string message, [CallerFilePath] string category = "")
        {
            category = Path.GetFileNameWithoutExtension(category);
            Output(LogLevel.Warn, category, message);
        }
    
        /// <param name="category">͂ȂĂяõNX</param>
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
    
            //Ăяo(FSEManager)ɔ񓯊̂ŁA̗\h
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
