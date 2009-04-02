namespace System.Web.Mvc {
    using System;
    using System.Security.Cryptography;

    internal sealed class AntiForgeryToken {

        private const int TokenLength = 128 / 8;
        private static RNGCryptoServiceProvider _prng = new RNGCryptoServiceProvider();

        private string _salt;
        private string _value;

        public AntiForgeryToken() {
        }

        // copy constructor
        public AntiForgeryToken(AntiForgeryToken token) {
            if (token == null) {
                throw new ArgumentNullException("token");
            }

            CreationDate = token.CreationDate;
            Salt = token.Salt;
            Value = token.Value;
        }

        public DateTime CreationDate {
            get;
            set;
        }

        public string Salt {
            get {
                return _salt ?? String.Empty;
            }
            set {
                _salt = value;
            }
        }

        public string Value {
            get {
                return _value ?? String.Empty;
            }
            set {
                _value = value;
            }
        }

        private static string GenerateRandomTokenString() {
            byte[] tokenBytes = new byte[TokenLength];
            _prng.GetBytes(tokenBytes);

            string token = Convert.ToBase64String(tokenBytes);
            return token;
        }

        public static AntiForgeryToken NewToken() {
            string tokenString = GenerateRandomTokenString();
            return new AntiForgeryToken() {
                CreationDate = DateTime.Now,
                Value = tokenString
            };
        }

    }
}
