//---------------------------------------------------------------------
// <copyright file="KeePassException.cs">
//      Copyright 2013 (c) Valeriy Provalov.
// </copyright>
//--------------------------------------------------------------------

using System;

namespace PassIE.KeePassHttp
{
    public class KeePassException : Exception
    {
        public KeePassException() {}
        public KeePassException(string message) : base(message) {}
        public KeePassException(string message, Exception innerException) : base(message, innerException) {}
    }
}