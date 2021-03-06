﻿//---------------------------------------------------------------------
// <copyright file="Utilities.cs">
//      Copyright 2013 (c) Valeriy Provalov.
// </copyright>
//--------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;

namespace PassIE.KeePassHttp
{
    public static class Utilities
    {
        public static string Encode64(byte[] b)
        {
            return System.Convert.ToBase64String(b);
        }

        public static byte[] Decode64(string s)
        {
            return System.Convert.FromBase64String(s);
        }

        // from http://msdn.microsoft.com/en-us/library/dd378457.aspx
        private static readonly Guid FOLDERID_LocalAppDataLow = new Guid("A520A1A4-1780-4FF6-BD18-167343C5AF16");

        private static string GetKnownFolderPath(Guid knownFolderId)
        {
            IntPtr pszPath = IntPtr.Zero;
            try
            {
                int hr = SHGetKnownFolderPath(knownFolderId, 0, IntPtr.Zero, out pszPath);
                if (hr >= 0)
                    return Marshal.PtrToStringAuto(pszPath);
                throw Marshal.GetExceptionForHR(hr);
            }
            finally
            {
                if (pszPath != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(pszPath);
            }
        }

        [DllImport("shell32.dll")]
        static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken, out IntPtr pszPath);

        public static string GetLocalAppDataLowPath()
        {
            return GetKnownFolderPath(FOLDERID_LocalAppDataLow);
        }
    }
}
