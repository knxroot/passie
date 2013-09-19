//---------------------------------------------------------------------
// <copyright file="SettingsManager.cs">
//      Copyright 2013 (c) Valeriy Provalov.
// </copyright>
//--------------------------------------------------------------------

using PassIE.KeePassHttp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace PassIE
{
    public class SettingsManager
    {
        public string SettingsFilePath { get; private set; }

        public SettingsManager()
        {
            this.SettingsFilePath = string.Format(@"{0}\PassIE\keepasshttp.key", Utilities.GetLocalAppDataLowPath());
        }

        private IFormatter GetFormatter()
        {
            return new BinaryFormatter();
        }
        
        public Settings Load()
        {
            if (File.Exists(this.SettingsFilePath))
            {
                using (MemoryStream stream = new MemoryStream(ProtectedData.Unprotect(File.ReadAllBytes(this.SettingsFilePath), null, DataProtectionScope.CurrentUser))) 
                {
                    return (Settings)this.GetFormatter().Deserialize(stream);                                       
                }
            }

            return new Settings()
            {
                KeePassHost = "localhost",
                KeePassPort = 19455
            };
        }

        public void Save(Settings settings)
        {
            string path = Path.GetDirectoryName(this.SettingsFilePath);

            if (path != null)
            {
                Directory.CreateDirectory(path);
            }

            using (MemoryStream stream = new MemoryStream())
            {
                this.GetFormatter().Serialize(stream, settings);
                File.WriteAllBytes(this.SettingsFilePath, ProtectedData.Protect(stream.GetBuffer(), null, DataProtectionScope.CurrentUser));
            }
        }
    }
}
