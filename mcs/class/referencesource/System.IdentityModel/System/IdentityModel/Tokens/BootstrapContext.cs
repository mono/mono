//------------------------------------------------------------------------------
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

using System.Runtime.Serialization;
using System.Xml;
using System.IO;

namespace System.IdentityModel.Tokens
{
    /// <summary>
    /// Represents a serializable version of a token that can be attached to a <see cref="System.Security.Claims.ClaimsIdentity"/> to retain the 
    /// original token that was used to create <see cref="System.Security.Claims.ClaimsIdentity"/>
    /// </summary>
    [Serializable]
    public class BootstrapContext : ISerializable
    {
        SecurityToken _token;
        string _tokenString;
        byte[] _tokenBytes;
        SecurityTokenHandler _tokenHandler;

        const string _tokenTypeKey = "K";
        const string _tokenKey = "T";
        const char _securityTokenType = 'T';
        const char _stringTokenType = 'S';
        const char _byteTokenType = 'B';

        protected BootstrapContext(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                return;

            switch (info.GetChar(_tokenTypeKey))
            {
                case _securityTokenType:
                    {
                        SecurityTokenHandler sth = context.Context as SecurityTokenHandler;
                        if (sth != null)
                        {
                            using (XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(Convert.FromBase64String(info.GetString(_tokenKey)), XmlDictionaryReaderQuotas.Max))
                            {
                                reader.MoveToContent();
                                if (sth.CanReadToken(reader))
                                {
                                    string tokenName = reader.LocalName;
                                    string tokenNamespace = reader.NamespaceURI;
                                    SecurityToken token = sth.ReadToken(reader);

                                    if (token == null)
                                    {
                                        _tokenString = Text.Encoding.UTF8.GetString(Convert.FromBase64String(info.GetString(_tokenKey)));
                                    }
                                    else
                                    {
                                        _token = token;
                                    }
                                }
                            }
                        }
                        else
                        {
                            _tokenString = Text.Encoding.UTF8.GetString(Convert.FromBase64String(info.GetString(_tokenKey)));
                        }
                    }

                    break;

                case _stringTokenType:
                    {
                        _tokenString = info.GetString(_tokenKey);
                    }
                    break;

                case _byteTokenType:
                    {
                        _tokenBytes = (byte[])info.GetValue(_tokenKey, typeof(byte[]));
                    }
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// A SecurityToken and a SecurityTokenHandler that can serialize the token.
        /// </summary>
        /// <param name="token"><see cref="SecurityToken"/> that can be serialized. Cannot be null.</param>
        /// <param name="tokenHandler"><see cref="SecurityTokenHandler"/> that is responsible for serializing the token. Cannon be null.</param>
        /// <exception cref="ArgumentNullException"> thrown if 'token' or 'tokenHandler' is null.</exception>
        /// <remarks>The <see cref="SecurityTokenHandler"/> is used not used to deserialize the token as it cannot be assumed to exist</remarks>
        public BootstrapContext(SecurityToken token, SecurityTokenHandler tokenHandler)
        {
            if (token == null)
            {
                throw new ArgumentNullException("token");
            }

            if (tokenHandler == null)
            {
                throw new ArgumentNullException("tokenHandler");
            }

            _token = token;
            _tokenHandler = tokenHandler;
        }

        /// <summary>
        /// String that represents a SecurityToken.
        /// </summary>
        /// <param name="token">string that represents a token.  Can not be null.</param>
        /// <exception cref="ArgumentNullException"> thrown if 'token' is null.</exception>
        public BootstrapContext(string token)
        {
            if (token == null)
            {
                throw new ArgumentNullException("token");
            }

            _tokenString = token;
        }

        /// <summary>
        /// String that represents a SecurityToken.
        /// </summary>
        /// <param name="token">string that represents a token.  Can not be null.</param>
        /// <exception cref="ArgumentNullException"> thrown if 'token' is null.</exception>
        public BootstrapContext(byte[] token)
        {
            if (token == null)
            {
                throw new ArgumentNullException("token");
            }

            _tokenBytes = token;
        }

        #region ISerializable Members
        /// <summary>
        /// Called to serialize this context.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> container for storing data. Cannot be null.</param>
        /// <param name="context"><see cref="StreamingContext"/> contains the context for streaming and optionally additional user data.</param>
        /// <exception cref="ArgumentNullException"> thrown if 'info' is null.</exception>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (_tokenBytes != null)
            {
                info.AddValue(_tokenTypeKey, _byteTokenType);
                info.AddValue(_tokenKey, _tokenBytes);
            }
            else if (_tokenString != null)
            {
                info.AddValue(_tokenTypeKey, _stringTokenType);
                info.AddValue(_tokenKey, _tokenString);
            }
            else if (_token != null && _tokenHandler != null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    info.AddValue(_tokenTypeKey, _securityTokenType);
                    using (XmlDictionaryWriter writer = XmlDictionaryWriter.CreateTextWriter(ms, Text.Encoding.UTF8, false))
                    {
                        _tokenHandler.WriteToken(writer, _token);
                        writer.Flush();
                        info.AddValue(_tokenKey, Convert.ToBase64String(ms.GetBuffer(), 0, (int)ms.Length));
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Gets the string that was passed in constructor. If a different constructor was used, will be null.
        /// </summary>
        public byte[] TokenBytes
        {
            get { return _tokenBytes; }
        }

        /// <summary>
        /// Gets the string that was passed in constructor. If a different constructor was used, will be null.
        /// </summary>
        public string Token
        {
            get { return _tokenString; }
        }

        /// <summary>
        /// Gets the SecurityToken that was passed in constructor. If a different constructor was used, will be null.
        /// </summary>
        public SecurityToken SecurityToken
        {
            get { return _token; }
        }

        /// <summary>
        /// Gets the SecurityTokenHandler that was passed in constructor. If a different constructor was used, will be null.
        /// </summary>
        public SecurityTokenHandler SecurityTokenHandler
        {
            get { return _tokenHandler; }
        }
    }
}
