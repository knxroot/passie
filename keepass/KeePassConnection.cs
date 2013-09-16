//---------------------------------------------------------------------
// <copyright file="KeePassConnection.cs">
//      Copyright 2013 (c) Valeriy Provalov.
// </copyright>
//--------------------------------------------------------------------

using System;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using ServiceStack.Text;

namespace PassIE.KeePassHttp
{
    public class KeePassConnection
    {
        public string Host { get; private set; }
        public int Port { get; private set; }
        public string Hash { get; private set; }
        public string Id { get; private set; }
        public byte[] Key { get; private set; }
        private readonly Aes aes;

        public KeePassConnection(string host, int port, string id, byte[] key)
        {
            this.Host = host;
            this.Port = port;
            this.Hash = null;
            this.Id = id;
            this.Key = key;
            this.aes = key != null ? new AesManaged {Key = key} : new AesManaged();
        }

        public KeePassConnection(string host = "localhost", int port = 19455) : this(host, port, null, null) {}

        private Uri GetKeePassUri()
        {
            return new Uri(string.Format("http://{0}:{1}", this.Host, this.Port));
        }
        
        private Response Send(Request request)
        {
            try
            {
                using (var webClient = new WebClient())
                {
                    webClient.Headers[HttpRequestHeader.ContentType] = "application/json";
                    string requestString = request.ToJson();
                    return webClient.UploadString(GetKeePassUri(), requestString).FromJson<Response>();
                }
            }
            catch (WebException ex)
            {
                throw new KeePassException("Error connecting to KeePassHttp", ex);
            }
        }

        public void Connect()
        {
            if (this.Hash == null)
            {
                var request = new Request {RequestType = Request.TEST_ASSOCIATE};
                Response response = this.Send(request);
                
                this.Hash = response.Hash;
            }
        }

        public void Disconnect()
        {
            aes.Dispose();
            this.Hash = null;
        }

        private void SetVerifier(Request request)
        {
            aes.GenerateIV();

            request.Id = this.Id;
            request.Nonce = Utilities.Encode64(aes.IV);

            using (var encryptor = this.aes.CreateEncryptor())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(request.Nonce);
                var buf = encryptor.TransformFinalBlock(bytes, 0, bytes.Length);
                request.Verifier = Utilities.Encode64(buf);
            }
        }

        private void GenerateKey()
        {
            var rnd = new Random();
            var key = new byte[32];
            rnd.NextBytes(key);
            this.Key = key;
            aes.Key = key;
        }

        public void Associate()
        {
            if (this.Hash != null)
            {
                var request = new Request();
                if (this.Key == null)
                {
                    this.GenerateKey();
                }
                request.Key = Utilities.Encode64(this.Key);
                this.SetVerifier(request);
                request.RequestType = Request.ASSOCIATE;
                Response response = this.Send(request);

                this.Id = response.Id;
                if (!response.Success || this.Id == null)
                {
                    throw new Exception(string.Format("Association failed: {0}", response.Error));
                }
            }
            else
            {
                throw new KeePassException("KeePass disconnected.");
            }
        }

        private Credentials DecryptEntry(ResponseEntry entry, string IV)
        {
            string userName, password;

            using (var decryptor = this.aes.CreateDecryptor(this.Key, Utilities.Decode64(IV)))
            {
                byte[] bytes = Utilities.Decode64(entry.Password);
                password = Encoding.UTF8.GetString(decryptor.TransformFinalBlock(bytes, 0, bytes.Length));
                bytes = Utilities.Decode64(entry.Login);
                userName = Encoding.UTF8.GetString(decryptor.TransformFinalBlock(bytes, 0, bytes.Length));
            }

            return new Credentials(userName, password);
        }

        public Credentials[] RetrieveCredentials(string url)
        {
            if (this.Key != null && this.Id != null)
            {
                var request = new Request();
                request.RequestType = Request.GET_LOGINS;
                this.SetVerifier(request);
                
                using (var encryptor = this.aes.CreateEncryptor())
                {
                    
                    byte[] bytes = Encoding.UTF8.GetBytes(url);
                    var buf = encryptor.TransformFinalBlock(bytes, 0, bytes.Length);

                    request.Url = Utilities.Encode64(buf);
                }

                Response response = this.Send(request);

                if (!response.Success)
                {
                    throw new KeePassException(string.Format("Error requesting credentials: {0}", response.Error));
                }
                
                return response.Entries.Select(entry => DecryptEntry(entry, response.Nonce)).ToArray();
            }
            return null;
        }
    }
}
