//---------------------------------------------------------------------
// <copyright file="Response.cs">
//      Copyright 2013 (c) Valeriy Provalov.
// </copyright>
//--------------------------------------------------------------------

using System.Collections.Generic;

namespace PassIE.KeePassHttp
{
    public class Response
    {
        public string RequestType { get; set; }
        public string Error { get; set; }
        public bool Success { get; set; }
        public string Id { get; set; }
        public int Count { get; set; }
        public string Version { get; set; }
        public string Hash { get; set; }
        public List<ResponseEntry> Entries { get; set; }
        public string Nonce { get; set; }
        public string Verifier { get; set; }
    }
}