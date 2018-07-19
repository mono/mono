//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IdentityModel.Tokens;

namespace System.IdentityModel.Metadata
{
    /// <summary>
    /// Defines the key descriptor.
    /// </summary>
    public class KeyDescriptor
    {
        SecurityKeyIdentifier _ski;
        KeyType _use = KeyType.Unspecified;
        Collection<EncryptionMethod> _encryptionMethods = new Collection<EncryptionMethod>();

        /// <summary>
        /// Empty constructor.
        /// </summary>
        public KeyDescriptor()
            : this(null)
        {
        }

        /// <summary>
        /// Constructs with a security key identifier.
        /// </summary>
        /// <param name="ski">The <see cref="SecurityKeyIdentifier"/> for this instance.</param>
        public KeyDescriptor(SecurityKeyIdentifier ski)
        {
            _ski = ski;
        }

        /// <summary>
        /// Gets or sets the <see cref="SecurityKeyIdentifier"/>.
        /// </summary>
        public SecurityKeyIdentifier KeyInfo
        {
            get { return _ski; }
            set { _ski = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="KeyType"/>.
        /// </summary>
        public KeyType Use
        {
            get { return _use; }
            set { _use = value; }
        }

        /// <summary>
        /// Gets the collection of <see cref="EncryptionMethod"/>.
        /// </summary>
        public ICollection<EncryptionMethod> EncryptionMethods
        {
            get { return _encryptionMethods; }
        }
    }
}
