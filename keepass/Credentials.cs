//---------------------------------------------------------------------
// <copyright file="Credentials.cs">
//      Copyright 2013 (c) Valeriy Provalov.
// </copyright>
//--------------------------------------------------------------------

namespace PassIE.KeePassHttp
{
    public class Credentials
    {
        public string UserName { get; private set; }
        public string Password { get; private set; }

        public Credentials(string userName, string password)
        {
            this.UserName = userName;
            this.Password = password;
        }
    }
}
