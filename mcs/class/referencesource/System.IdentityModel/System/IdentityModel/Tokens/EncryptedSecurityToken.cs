using System;
using System.Collections.Generic;
using System.Text;
using System.IdentityModel.Tokens;
using System.Collections.ObjectModel;

namespace System.IdentityModel.Tokens
{
    /// <summary>
    /// A pseudo-token which handles encryption for a token which
    /// does not natively support it.
    /// </summary>
    /// <remarks>
    /// For example, a SamlSecurityToken has no notion of how to encrypt
    /// itself, so to issue an encrypted SAML11 assertion, wrap a 
    /// SamlSecurityToken with an EncryptedSecurityToken and provide 
    /// appropriate EncryptingCredentials.
    /// </remarks>
    public class EncryptedSecurityToken : SecurityToken
    {
        EncryptingCredentials _encryptingCredentials;
        SecurityToken _realToken;

        /// <summary>
        /// Creates an instance of EncryptedSecurityToken.
        /// </summary>
        /// <param name="token">The <see cref="SecurityToken"/> to encrypt.</param>
        /// <param name="encryptingCredentials">The <see cref="EncryptingCredentials"/> to use for encryption.</param>
        public EncryptedSecurityToken(SecurityToken token, EncryptingCredentials encryptingCredentials)
        {
            if (null == token)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }
            if (null == encryptingCredentials)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("encryptingCredentials");
            }

            _encryptingCredentials = encryptingCredentials;
            _realToken = token;
        }

        /// <summary>
        /// Inherited from <see cref="SecurityToken"/>.
        /// </summary>
        public override bool CanCreateKeyIdentifierClause<T>()
        {
            return _realToken.CanCreateKeyIdentifierClause<T>();
        }

        /// <summary>
        /// Inherited from <see cref="SecurityToken"/>.
        /// </summary>
        public override T CreateKeyIdentifierClause<T>()
        {
            return _realToken.CreateKeyIdentifierClause<T>();
        }

        /// <summary>
        /// Gets the <see cref="EncryptingCredentials"/> to use for encryption.
        /// </summary>
        public EncryptingCredentials EncryptingCredentials
        {
            get { return _encryptingCredentials; }
        }

        /// <summary>
        /// Gets a unique identifier of the security token.
        /// </summary>
        public override string Id
        {
            get { return _realToken.Id; }
        }

        /// <summary>
        /// Inherited from <see cref="SecurityToken"/>.
        /// </summary>
        public override bool MatchesKeyIdentifierClause(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            return _realToken.MatchesKeyIdentifierClause(keyIdentifierClause);
        }

        /// <summary>
        /// Inherited from <see cref="SecurityToken"/>.
        /// </summary>
        public override SecurityKey ResolveKeyIdentifierClause(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            return _realToken.ResolveKeyIdentifierClause(keyIdentifierClause);
        }

        /// <summary>
        /// Inherited from <see cref="SecurityToken"/>.
        /// </summary>
        public override ReadOnlyCollection<SecurityKey> SecurityKeys
        {
            get { return _realToken.SecurityKeys; }
        }

        /// <summary>
        /// Gets the encrypted <see cref="SecurityToken"/>.
        /// </summary>
        public SecurityToken Token
        {
            get { return _realToken; }
        }

        /// <summary>
        /// Gets the first instant in time at which this security token is valid.
        /// </summary>
        public override DateTime ValidFrom
        {
            get { return _realToken.ValidFrom; }
        }

        /// <summary>
        /// Gets the last instant in time at which this security token is valid.
        /// </summary>
        public override DateTime ValidTo
        {
            get { return _realToken.ValidTo; }
        }
    }
}
