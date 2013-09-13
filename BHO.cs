//---------------------------------------------------------------------
// <copyright file="BHO.cs">
//      Copyright 2013 (c) Valeriy Provalov.
// </copyright>
//--------------------------------------------------------------------

using Microsoft.Win32;
using mshtml;
using PassIE.KeePassHttp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using WebBrowser = SHDocVw.WebBrowser;

namespace PassIE
{
    [ComVisible(true), Guid("CD04D1E5-6416-45E7-85F4-2A3DC324EC7F"), ClassInterface(ClassInterfaceType.None)]
    public class BHO : IObjectWithSite
    {
        public static string BHO_KEY_NAME = "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Browser Helper Objects";
        private WebBrowser webBrowser;

        private string keePassId;
        private byte[] keePassKey;

        private KeePassConnection keePassConnection;
        private KeePassConnection GetKeePassConnection()
        {
            if (keePassConnection == null)
            {
                try
                {
                    this.LoadKeePassHttpSettings();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception caught: {0}", ex.Message);
                }
            }

            return keePassConnection;
        }

        private Dictionary<string, Credentials> credentialsCache = new Dictionary<string, Credentials>();
            
        [ComRegisterFunction]
        public static void RegisterBHO(Type type)
        {
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(BHO_KEY_NAME, true);
            RegistryKey ourKey = null;

            try
            {
                if (registryKey == null)
                {
                    registryKey = Registry.LocalMachine.CreateSubKey(BHO_KEY_NAME);

                    if (registryKey == null)
                    {
                        throw new Exception(string.Format("Error opening registry key {0}", BHO_KEY_NAME));
                    }
                }

                string guid = type.GUID.ToString("B");
                ourKey = registryKey.OpenSubKey(guid);
                if (ourKey == null)
                {
                    ourKey = registryKey.CreateSubKey(guid);

                    if (ourKey == null)
                    {
                        throw new Exception(string.Format("Error creating registry subkey {0}", guid));
                    }
                }

                ourKey.SetValue("NoExplorer", 1, RegistryValueKind.DWord);
            }
            finally
            {
                if (registryKey != null)
                {
                    registryKey.Close();
                }

                if (ourKey != null)
                {
                    ourKey.Close();
                }
            }
        }

        [ComUnregisterFunction]
        public static void UnregisterBHO(Type type)
        {
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(BHO_KEY_NAME, true);

            string guid = type.GUID.ToString("B");

            if (registryKey != null)
            {
                registryKey.DeleteSubKey(guid, false);
            }
        }

        public void OnBeforeNavigate2(
            object pDisp, 
            ref object URL,
            ref object Flags, 
            ref object TargetFrameName, 
            ref object PostData, 
            ref object Headers, 
            ref bool Cancel)
        {
        }

        private void FillPassword(string url, Dictionary<IHTMLElement, IHTMLElement> credentialsFields)
        {
            if (credentialsFields.Count > 0)
            {
                KeePassConnection keePassConnection = this.GetKeePassConnection();

                if (keePassConnection != null)
                {
                    Credentials credentials = null;
                    if (!credentialsCache.TryGetValue(url, out credentials))
                    {
                        Credentials[] result = keePassConnection.RetrieveCredentials(url);    
                        if (result.Length > 0)
                        {
                            credentials = result[0];
                            credentialsCache[url] = credentials;
                        }
                    }

                    if (credentials != null)
                    {
                        foreach (var credentialsFieldPair in credentialsFields)
                        {
                            var passwordField = credentialsFieldPair.Key as IHTMLInputElement;
                            var usernameField = credentialsFieldPair.Value as IHTMLInputElement;

                            if (passwordField != null)
                            {
                                passwordField.value = credentials.Password;
                            }

                            if (usernameField != null)
                            {
                                usernameField.value = credentials.UserName;
                            }
                        }
                    }
                }
            }
        }

        public void OnDocumentComplete(object pDisp, ref object url)
        {
            var document = webBrowser.Document as HTMLDocument;

            if (document != null)
            {
                Dictionary<IHTMLElement, IHTMLElement> credentialsFields = CredentialsFinder.FindCredentials(document);

                this.FillPassword(document.url, credentialsFields);
            }
        }

        private void LoadKeePassHttpSettings()
        {
            string keePassKeyFilePath = string.Format(@"{0}\PassIE\keepasshttp.key", Utilities.GetLocalAppDataLowPath());
            string[] lines = null;

            if (File.Exists(keePassKeyFilePath))
            {
                lines = File.ReadAllLines(keePassKeyFilePath);
            }

            if (lines != null && lines.Length == 2)
            {
                this.keePassId = lines[0];
                this.keePassKey = Utilities.Decode64(lines[1]);

                this.keePassConnection = new KeePassConnection(
                    "localhost",
                    19455,
                    this.keePassId,
                    this.keePassKey);
                this.keePassConnection.Connect();
            }
            else
            {
                this.keePassConnection = new KeePassConnection();
                this.keePassConnection.Connect();

                this.keePassConnection.Associate();
                lines = new string[2];
                lines[0] = this.keePassConnection.Id;
                lines[1] = Utilities.Encode64(this.keePassConnection.Key);

                string path = Path.GetDirectoryName(keePassKeyFilePath);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                File.WriteAllLines(keePassKeyFilePath, lines);
            }
        }

        #region IObjectWithSite  Members
        public int SetSite(object site)
        {
            if (site != null)
            {
                this.webBrowser = (WebBrowser) site;
                this.webBrowser.BeforeNavigate2 += this.OnBeforeNavigate2;
                this.webBrowser.DocumentComplete += this.OnDocumentComplete;
            }
            else
            {
                this.webBrowser.BeforeNavigate2 -= this.OnBeforeNavigate2;
                this.webBrowser.DocumentComplete -= this.OnDocumentComplete;
                this.webBrowser = null;
            }
            return 0;
        }

        public int GetSite(ref Guid guid, out IntPtr ppvSite)
        {
            IntPtr iUnknownForObject = Marshal.GetIUnknownForObject(webBrowser);
            int hr = Marshal.QueryInterface(iUnknownForObject, ref guid, out ppvSite);

            Marshal.Release(iUnknownForObject);

            return hr;
        }
        #endregion
    }
}
