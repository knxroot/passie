//---------------------------------------------------------------------
// <copyright file="Settings.cs">
//      Copyright 2013 (c) Valeriy Provalov.
// </copyright>
//--------------------------------------------------------------------

using PassIE.KeePassHttp;
using System;
using System.IO;

namespace PassIE
{
    public class Settings
    {
        private string keePassId;
        private byte[] keePassKey;

        public string KeePassId
        {
            get
            {
                return this.keePassId;
            }
        }

        public byte[] KeePassKey
        {
            get
            {
                return this.keePassKey;
            }
        }

        public string KeePassHost { get { return "localhost"; }}
        public int KeePassPort { get { return 19455;  } }

        public void SetKeePassSettings(string id, byte[] key)
        {
            this.keePassId = id;
            this.keePassKey = key;
            this.TrySave();
        }

        public Settings()
        {
            this.Load();
        }

        private string settingsFilePath;
        private string SettingsFilePath
        {
            get
            {
                if (settingsFilePath == null)
                {
                    this.settingsFilePath = string.Format(@"{0}\PassIE\keepasshttp.key",
                        Utilities.GetLocalAppDataLowPath());
                }

                return this.settingsFilePath;
            }
        }

        private void Load()
        {
            string[] lines = null;

            if (File.Exists(this.SettingsFilePath))
            {
                lines = File.ReadAllLines(this.SettingsFilePath);
            }

            if (lines != null && lines.Length == 2)
            {
                this.keePassId = lines[0];
                this.keePassKey = Utilities.Decode64(lines[1]);
            }
        }

        private void Save()
        {
            var lines = new string[2];
            lines[0] = this.keePassId;
            lines[1] = Utilities.Encode64(this.keePassKey);

            string path = Path.GetDirectoryName(this.SettingsFilePath);
            if (path != null)
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                File.WriteAllLines(this.SettingsFilePath, lines);
            }
        }

        private bool TrySave()
        {
            if (this.keePassId != null && this.keePassKey != null)
            {
                try
                {
                    this.Save();
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception caught: {0}", ex.Message);
                }
            }

            return false;
        }
    }
}
