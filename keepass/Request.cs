//---------------------------------------------------------------------
// <copyright file="Request.cs">
//      Copyright 2013 (c) Valeriy Provalov.
// </copyright>
//--------------------------------------------------------------------

namespace PassIE.KeePassHttp
{
    public class Request
    {
        public const string GET_LOGINS = "get-logins";
        public const string GET_LOGINS_COUNT = "get-logins-count";
        public const string GET_ALL_LOGINS = "get-all-logins";
        public const string SET_LOGIN = "set-login";
        public const string ASSOCIATE = "associate";
        public const string TEST_ASSOCIATE = "test-associate";
        public const string GENERATE_PASSWORD = "generate-password";

        public string RequestType { get; set; }
        public string SortSelection { get; set; }
        public string TriggerUnlock { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Uuid { get; set; }
        public string Url { get; set; }
        public string SubmitUrl { get; set; }
        public string Key { get; set; }
        public string Id { get; set; }
        public string Verifier { get; set; }
        public string Nonce { get; set; }
        public string Realm { get; set; }
    }
}