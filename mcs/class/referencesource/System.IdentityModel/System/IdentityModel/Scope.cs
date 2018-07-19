//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens;

    /// <summary>
    /// Defines one class which contains all the relying party related information.
    /// This class is not thread safe.
    /// </summary>
    public class Scope
    {
        string _appliesToAddress;
        string _replyToAddress;
        EncryptingCredentials _encryptingCredentials;
        SigningCredentials _signingCredentials;
        bool _symmetricKeyEncryptionRequired = true;
        bool _tokenEncryptionRequired = true;
        Dictionary<string, object> _properties = new Dictionary<string, object>(); // for any custom data

        /// <summary>
        /// Initializes an instance of <see cref="Scope"/>
        /// </summary>
        public Scope()
            : this(null, null, null)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="Scope"/>
        /// </summary>
        /// <param name="appliesToAddress">The appliesTo address of the relying party.</param>
        public Scope(string appliesToAddress)
            : this(appliesToAddress, null, null)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="Scope"/>
        /// </summary>
        /// <param name="appliesToAddress">The appliesTo address of the relying party.</param>
        /// <param name="signingCredentials">The signing credentials for the relying party.</param>
        public Scope(string appliesToAddress, SigningCredentials signingCredentials)
            : this(appliesToAddress, signingCredentials, null)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="Scope"/>
        /// </summary>
        /// <param name="appliesToAddress">The appliesTo address of the relying party.</param>
        /// <param name="encryptingCredentials"> The encrypting credentials for the relying party.</param>
        public Scope(string appliesToAddress, EncryptingCredentials encryptingCredentials)
            : this(appliesToAddress, null, encryptingCredentials)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="Scope"/>
        /// </summary>
        /// <param name="appliesToAddress">The appliesTo address of the relying party.</param>
        /// <param name="signingCredentials">The signing credentials for the relying party.</param>
        /// <param name="encryptingCredentials"> The encrypting credentials for the relying party.</param>
        public Scope(string appliesToAddress, SigningCredentials signingCredentials, EncryptingCredentials encryptingCredentials)
        {
            _appliesToAddress = appliesToAddress;
            _signingCredentials = signingCredentials;
            _encryptingCredentials = encryptingCredentials;
        }

        /// <summary>
        /// Gets or sets the appliesTo address of the relying party.
        /// </summary>
        public virtual string AppliesToAddress
        {
            get
            {
                return _appliesToAddress;
            }
            set
            {
                _appliesToAddress = value;
            }
        }

        /// <summary>
        /// Gets or sets the encrypting credentials.
        /// </summary>
        public virtual EncryptingCredentials EncryptingCredentials
        {
            get
            {
                return _encryptingCredentials;
            }
            set
            {
                _encryptingCredentials = value;
            }
        }

        /// <summary>
        /// Gets or sets the replyTo address of the relying party.
        /// </summary>
        public virtual string ReplyToAddress
        {
            get
            {
                return _replyToAddress;
            }
            set
            {
                _replyToAddress = value;
            }
        }

        /// <summary>
        /// Gets or sets the SigningCredentials for the relying party.
        /// </summary>
        public virtual SigningCredentials SigningCredentials
        {
            get
            {
                return _signingCredentials;
            }
            set
            {
                _signingCredentials = value;
            }
        }


        /// <summary>
        /// Gets or sets the property which determines if issued symmetric keys must
        /// be encrypted by <see cref="Scope.EncryptingCredentials"/>.
        /// </summary>
        public virtual bool SymmetricKeyEncryptionRequired
        {
            get
            {
                return _symmetricKeyEncryptionRequired;
            }
            set
            {
                _symmetricKeyEncryptionRequired = value;
            }
        }

        /// <summary>
        /// Gets or sets the property which determines if issued security tokens must
        /// be encrypted by <see cref="Scope.EncryptingCredentials"/>.
        /// </summary>
        public virtual bool TokenEncryptionRequired
        {
            get
            {
                return _tokenEncryptionRequired;
            }
            set
            {
                _tokenEncryptionRequired = value;
            }
        }

        /// <summary>
        /// Gets the properties bag to extend the object.
        /// </summary>
        public virtual Dictionary<string, object> Properties
        {
            get
            {
                return _properties;
            }
        }
    }
}
