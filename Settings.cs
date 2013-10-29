//---------------------------------------------------------------------
// <copyright file="Settings.cs">
//      Copyright 2013 (c) Valeriy Provalov.
// </copyright>
//--------------------------------------------------------------------

using System;

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
