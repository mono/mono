//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System.Xml;

namespace System.IdentityModel.Tokens
{
    /// <summary>
    /// Defines a SecurityTokenHandler for Username Password Tokens. 
    /// </summary>
    public abstract class UserNameSecurityTokenHandler : SecurityTokenHandler
    {
        bool _retainPassword;

        /// <summary>
        /// Initializes an instance of <see cref="UserNameSecurityTokenHandler"/>
        /// </summary>
        protected UserNameSecurityTokenHandler()
        {
        }

        /// <summary>
        /// Controls if the password will be retained in the bootstrap token that is
        /// attached to the ClaimsIdentity in ValidateToken.  The default is false.
        /// </summary>
        public virtual bool RetainPassword
        {
            get
            {
                return _retainPassword;
            }
            set
            {
                _retainPassword = value;
            }
        }

        /// <summary>
        /// Checks the given XmlReader to verify that it is pointing to a Username
        /// token.
        /// </summary>
        /// <param name="reader">XmlReader pointing to SecurityToken.</param>
        /// <returns>True if the reader is pointing to a Username SecurityToken.</returns>
        /// <exception cref="ArgumentNullException">The given reader is null.</exception>
        public override bool CanReadToken(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            return reader.IsStartElement(WSSecurity10Constants.Elements.UsernameToken, WSSecurity10Constants.Namespace);
        }

        /// <summary>
        /// Returns true to indicate that the handler can write UsernameSecurityToken.
        /// </summary>
        public override bool CanWriteToken
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Get the System.Type of the SecurityToken that this handler can handle.
        /// </summary>
        public override Type TokenType
        {
            get
            {
                return typeof(UserNameSecurityToken);
            }
        }

        /// <summary>
        /// Get the TokenTypeIdentifier of the token that this handler can work with.
        /// </summary>
        public override string[] GetTokenTypeIdentifiers()
        {
            return new string[] { SecurityTokenTypes.UserName };
        }

        /// <summary>
        /// Reads the UsernameSecurityToken from the given XmlReader.
        /// </summary>
        /// <param name="reader">XmlReader pointing to the SecurityToken.</param>
        /// <returns>An instance of <see cref="UserNameSecurityToken"/>.</returns> 
        /// <exception cref="ArgumentNullException">The parameter 'reader' is null.</exception>
        /// <exception cref="XmlException">The token cannot be read.</exception>
        /// <exception cref="NotSupportedException">The Password was not in plain text format.</exception>
        /// <exception cref="InvalidOperationException">An unknown element was found in the SecurityToken or 
        /// the username was not specified.</exception>
        public override SecurityToken ReadToken(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            if (!CanReadToken(reader))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new XmlException(
                        SR.GetString(
                        SR.ID4065,
                        WSSecurity10Constants.Elements.Username,
                        WSSecurity10Constants.Namespace,
                        reader.LocalName,
                        reader.NamespaceURI)));
            }

            string id = null;
            string userName = null;
            string password = null;

            reader.MoveToContent();
            id = reader.GetAttribute(WSUtilityConstants.Attributes.IdAttribute, WSUtilityConstants.NamespaceURI);

            reader.ReadStartElement(WSSecurity10Constants.Elements.UsernameToken, WSSecurity10Constants.Namespace);
            while (reader.IsStartElement())
            {
                if (reader.IsStartElement(WSSecurity10Constants.Elements.Username, WSSecurity10Constants.Namespace))
                {
                    userName = reader.ReadElementString();
                }
                else if (reader.IsStartElement(WSSecurity10Constants.Elements.Password, WSSecurity10Constants.Namespace))
                {
                    string type = reader.GetAttribute(WSSecurity10Constants.Attributes.Type, null);
                    if (!string.IsNullOrEmpty(type) && !StringComparer.Ordinal.Equals(type, WSSecurity10Constants.UPTokenPasswordTextValue))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.ID4059, type, WSSecurity10Constants.UPTokenPasswordTextValue)));
                    }

                    password = reader.ReadElementString();
                }
                else if (reader.IsStartElement(WSSecurity10Constants.Elements.Nonce, WSSecurity10Constants.Namespace))
                {
                    // Nonce can be safely ignored
                    reader.Skip();
                }
                else if (reader.IsStartElement(WSUtilityConstants.ElementNames.Created, WSUtilityConstants.NamespaceURI))
                {
                    // wsu:Created can be safely ignored
                    reader.Skip();
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.ID4060, reader.LocalName, reader.NamespaceURI, WSSecurity10Constants.Elements.UsernameToken, WSSecurity10Constants.Namespace)));
                }
            }
            reader.ReadEndElement();

            if (string.IsNullOrEmpty(userName))
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4061));
            }

            return string.IsNullOrEmpty(id) ?
                new UserNameSecurityToken(userName, password) :
                new UserNameSecurityToken(userName, password, id);
        }


        /// <summary>
        /// Writes the given UsernameSecurityToken to the XmlWriter.
        /// </summary>
        /// <param name="writer">XmlWriter to write the token to.</param>
        /// <param name="token">SecurityToken to be written.</param>
        /// <exception cref="InvalidOperationException">The given token is not a UsernameSecurityToken.</exception>
        /// <exception cref="ArgumentNullException">The parameter 'writer' or 'token' is null.</exception>
        public override void WriteToken(XmlWriter writer, SecurityToken token)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }

            UserNameSecurityToken usernameSecurityToken = token as UserNameSecurityToken;

            if (usernameSecurityToken == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("token", SR.GetString(SR.ID0018, typeof(UserNameSecurityToken)));
            }

            // <wsse:UsernameToken
            writer.WriteStartElement(
               WSSecurity10Constants.Elements.UsernameToken,
               WSSecurity10Constants.Namespace
               );
            if (!string.IsNullOrEmpty(token.Id))
            {
                // wsu:Id="..."
                writer.WriteAttributeString(
                       WSUtilityConstants.Attributes.IdAttribute,
                       WSUtilityConstants.NamespaceURI,
                       token.Id
                       );
            }
            // <wsse:Username>...</wsse:Username>
            writer.WriteElementString(
                WSSecurity10Constants.Elements.Username,
                WSSecurity10Constants.Namespace,
                usernameSecurityToken.UserName
                );

            // <wsse:Password>...</wsse:Password>
            if (usernameSecurityToken.Password != null)
            {
                writer.WriteStartElement(
                    WSSecurity10Constants.Elements.Password,
                    WSSecurity10Constants.Namespace
                    );

                writer.WriteAttributeString(
                    WSSecurity10Constants.Attributes.Type,
                    null,
                    WSSecurity10Constants.UPTokenPasswordTextValue
                    );

                writer.WriteString(usernameSecurityToken.Password);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();

            writer.Flush();
        }
    }
}
