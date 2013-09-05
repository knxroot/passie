//---------------------------------------------------------------------
// <copyright file="ResponseEntry.cs">
//      Copyright 2013 (c) Valeriy Provalov.
// </copyright>
//--------------------------------------------------------------------
using System.Collections.Generic;

namespace PassIE.KeePassHttp
{
    public class ResponseEntry
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public string Uuid { get; set; }
        public string Name { get; set; }
        public List<ResponseStringField> StringFields { get; set; }
    }
}