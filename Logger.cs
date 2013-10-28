//---------------------------------------------------------------------
// <copyright file="Logger.cs">
//      Copyright 2013 (c) Valeriy Provalov.
// </copyright>
//--------------------------------------------------------------------

using PassIE.KeePassHttp;
using System.IO;

namespace PassIE
{
    public enum LogPrefix
    {
        ERROR, WARNING, INFO
    };

    public class Logger
    {
        private static Logger instance = new Logger();
        private string filename;

        private Logger()
        {
            this.filename = Path.Combine(Utilities.GetLocalAppDataLowPath(), "PassIE", "passie.log");
        }

        public static Logger Instance { get { return instance;  } }

        private void LogMessage(LogPrefix prefix, string message, params object[] args)
        {
            string msg = string.Format(message, args);
            try
            {
                File.WriteAllText(this.filename, string.Format("{0}: {1}\n", prefix.ToString(), msg));
            }
            catch (IOException ex)
            {
                System.Console.Error.WriteLine("Exception caught while loggin message: {0}", ex.Message);
            }
        }

        private void LogMessage(LogPrefix prefix, string message)
        {
            File.WriteAllText(this.filename, string.Format("{0}: {1}\n", prefix.ToString(), message));
        }

        public void LogError(string message)
        {
            this.LogMessage(LogPrefix.ERROR, message);
        }

        public void LogError(string message, params object[] args)
        {
            this.LogMessage(LogPrefix.ERROR, message, args);
        }

        public void LogInfo(string message)
        {
            this.LogMessage(LogPrefix.INFO, message);
        }

        public void LogInfo(string message, params object[] args)
        {
            this.LogMessage(LogPrefix.INFO, message, args);
        }

        public void LogWarning(string message)
        {
            this.LogMessage(LogPrefix.WARNING, message);
        }

        public void LogWarning(string message, params object[] args)
        {
            this.LogMessage(LogPrefix.WARNING, message, args);
        }
    }
}
