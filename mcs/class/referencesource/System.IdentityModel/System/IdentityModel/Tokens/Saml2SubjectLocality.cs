//-----------------------------------------------------------------------
// <copyright file="Saml2SubjectLocality.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System;

    /// <summary>
    /// Represents the SubjectLocality element specified in [Saml2Core, 2.7.2.1].
    /// </summary>
    /// <remarks>
    /// This element is entirely advisory, since both of these fields are quite 
    /// easily "spoofed". [Saml2Core, 2.7.2.1]
    /// </remarks>
    public class Saml2SubjectLocality
    {
        private string address;
        private string dnsName;

        /// <summary>
        /// Initializes an instance of <see cref="Saml2SubjectLocality"/>.
        /// </summary>
        public Saml2SubjectLocality()
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="Saml2SubjectLocality"/> from an address and DNS name.
        /// </summary>
        /// <param name="address">A <see cref="String"/> indicating the address.</param>
        /// <param name="dnsName">A <see cref="String"/> indicating the DNS name.</param>
        public Saml2SubjectLocality(string address, string dnsName)
        {
            this.Address = address;
            this.DnsName = dnsName;
        }

        /// <summary>
        /// Gets or sets the network address of the system from which the principal identified
        /// by the subject was authenticated. [Saml2Core, 2.7.2.1]
        /// </summary>
        public string Address
        {
            get { return this.address; }
            set { this.address = XmlUtil.NormalizeEmptyString(value); }
        }

        /// <summary>
        /// Gets or sets the DNS name of the system from which the principal identified by the 
        /// subject was authenticated. [Saml2Core, 2.7.2.1]
        /// </summary>
        public string DnsName
        {
            get { return this.dnsName; }
            set { this.dnsName = XmlUtil.NormalizeEmptyString(value); }
        }
    }
}
