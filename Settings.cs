//---------------------------------------------------------------------
// <copyright file="Settings.cs">
//      Copyright 2013 (c) Valeriy Provalov.
// </copyright>
//--------------------------------------------------------------------

using PassIE.KeePassHttp;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace PassIE
{
    [Serializable]
    public class Settings
    {
        public string KeePassId { get; set; }
        public byte[] KeePassKey { get; set; }
        public string KeePassHost { get; set; }
        public int KeePassPort { get; set; }
    }
}
