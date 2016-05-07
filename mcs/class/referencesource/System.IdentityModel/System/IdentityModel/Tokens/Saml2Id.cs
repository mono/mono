//-----------------------------------------------------------------------
// <copyright file="Saml2Id.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System;
    using System.Xml;

    /// <summary>
    /// Represents the identifier used for SAML assertions.
    /// </summary>
    /// <details>
    /// This identifier should be unique per [Saml2Core, 1.3.4] 
    /// and must fit the NCName xml schema definition, which is to say that
    /// it must begin with a letter or underscore. 
    /// </details>
    public class Saml2Id
    {
        private string value;

        /// <summary>
        /// Creates a new ID value based on a GUID.
        /// </summary>
        public Saml2Id()
            : this(System.IdentityModel.UniqueId.CreateRandomId())
        {
        }

        /// <summary>
        /// Creates a new ID whose value is the given string.
        /// </summary>
        /// <param name="value">The Saml2 Id.</param>
        /// <exception cref="ArgumentException">If the value is not a valid NCName.</exception>
        public Saml2Id(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
            }

            try
            {
                this.value = XmlConvert.VerifyNCName(value);
            }
            catch (XmlException e)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentException(SR.GetString(SR.ID4128), "value", e));
            }
        }

        /// <summary>
        /// Gets the identifier string.
        /// </summary>
        public string Value
        {
            get { return this.value; }
        }

        /// <summary>
        /// Compares two <see cref="Saml2Id"/> for equality.
        /// </summary>
        /// <param name="obj">Object to campare to.</param>
        /// <returns>True if this equals object.  False otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (Object.ReferenceEquals(this, obj))
            {
                return true;
            }

            Saml2Id other = obj as Saml2Id;
#pragma warning suppress 56506 // PreSharp thinks other can be null-dereffed here
            return (null != other) && StringComparer.Ordinal.Equals(this.value, other.Value);
        }

        /// <summary>
        /// Gets the hash code for the <see cref="Saml2Id"/> as an integer. 
        /// </summary>
        /// <returns>The hash code for this object.</returns>
        public override int GetHashCode()
        {
            return this.value.GetHashCode();
        }

        /// <summary>
        /// Gets the <see cref="Saml2Id"/> in text format.
        /// </summary>
        /// <returns>The string representation of this object.</returns>
        public override string ToString()
        {
            return this.value;
        }
    }
}
