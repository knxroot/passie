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
using System.Reflection;
using System.Runtime.InteropServices;
using WebBrowser = SHDocVw.WebBrowser;

namespace PassIE
{
    [ComVisible(true), Guid("CD04D1E5-6416-45E7-85F4-2A3DC324EC7F"), ClassInterface(ClassInterfaceType.None)]
    public class BHO : IObjectWithSite
    {
        public static string BHO_KEY_NAME = "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Browser Helper Objects";
        private WebBrowser webBrowser;
        private Dictionary<string, Credentials> credentialsCache = new Dictionary<string, Credentials>();
        private WebBrowserEventSink eventSink;

        private SettingsManager SettingsManager = new SettingsManager();

        private KeePassConnection keePassConnection;
        private KeePassConnection GetKeePassConnection()
        {
            if (this.keePassConnection == null)
            {
                try
                {
                    Settings settings = this.SettingsManager.Load();

                    this.keePassConnection = new KeePassConnection(
                        settings.KeePassHost,
                        settings.KeePassPort,
                        settings.KeePassId,
                        settings.KeePassKey);
                    this.keePassConnection.Connect();

                    if (settings.KeePassId == null || settings.KeePassKey == null)
                    {
                        this.keePassConnection.Associate();
                        settings.KeePassId = this.keePassConnection.Id;
                        settings.KeePassKey = this.keePassConnection.Key;
                        this.SettingsManager.Save(settings);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception caught: {0}", ex.Message);
                }
            }

            return this.keePassConnection;
        }
        
        public BHO()
        { 
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(MyResolveEventHandler);
        }
        private static Assembly MyResolveEventHandler(object sender, ResolveEventArgs args)
        {
            return typeof(BHO).Assembly;
        }
                  
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

        private void TryFillCredentials()
        {
            HTMLDocument document = this.webBrowser.Document as HTMLDocument;
            if (document != null)
            {
                this.TryFillCredentials(document);
            }
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
                        if (result != null && result.Length > 0)
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

        private void TryFillCredentials(HTMLDocument document)
        {
            try
            {
                Dictionary<IHTMLElement, IHTMLElement> credentialsFields =
                    CredentialsFinder.FindCredentials(document);

                this.FillPassword(document.url, credentialsFields);
            }
            catch (KeePassException ex)
            {
                Console.WriteLine("Exception caught: {0}", ex.Message);
            }
        }

        #region IObjectWithSite  Members
        public int SetSite(object site)
        {
            try
            {
                this.webBrowser = site as WebBrowser;

                if (this.webBrowser != null)
                {
                    eventSink = new WebBrowserEventSink(this.TryFillCredentials, this.TryFillCredentials);
                    eventSink.Connect(this.webBrowser);
                }
                else
                {
                    eventSink.Disconnect(this.webBrowser);
                }
            }
            catch (COMException ex)
            {
                Console.WriteLine("Exception caught: {0}", ex.Message);
            }

            return 0;
        }

        public int GetSite(ref Guid guid, out IntPtr ppvSite)
        {
            IntPtr iUnknownForObject = Marshal.GetIUnknownForObject(this.webBrowser);
            int hr = Marshal.QueryInterface(iUnknownForObject, ref guid, out ppvSite);

            Marshal.Release(iUnknownForObject);

            return hr;
        }
        #endregion
    }
}
