//------------------------------------------------------------------------------
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>[....]</OWNER>
// 

//
// Claim.cs
//

namespace System.Security.Claims
{
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    
    /// <summary>
    /// A Claim is a statement about an entity by an Issuer.
    /// A Claim consists of a Value, a Subject and an Issuer.
    /// Additional properties, Type, ValueType, Properties and OriginalIssuer 
    /// help understand the claim when making decisions.
    /// </summary>
    [Serializable]
    public class Claim
    {
        string m_issuer;
        string m_originalIssuer;
        string m_type;
        string m_value;
        string m_valueType;
        
        Dictionary<string, string> m_properties;

        [NonSerialized]
        object m_propertyLock = new object();

        [NonSerialized]
        ClaimsIdentity m_subject;
                
        #region Claim Constructors

        /// <summary>
        /// Creates a <see cref="Claim"/> with the specified type and value.
        /// </summary>
        /// <param name="type">The claim type.</param>
        /// <param name="value">The claim value.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> or <paramref name="value"/> is null.</exception>
        /// <remarks>
        /// <see cref="Claim.Issuer"/> is set to <see cref="ClaimsIdentity.DefaultIssuer"/>,        
        /// <see cref="Claim.ValueType"/> is set to <see cref="ClaimValueTypes.String"/>, 
        /// <see cref="Claim.OriginalIssuer"/> is set to <see cref="ClaimsIdentity.DefaultIssuer"/>, and
        /// <see cref="Claim.Subject"/> is set to null.
        /// </remarks>
        /// <seealso cref="ClaimsIdentity"/>
        /// <seealso cref="ClaimTypes"/>
        /// <seealso cref="ClaimValueTypes"/>
        public Claim(string type, string value)
            : this(type, value, ClaimValueTypes.String, ClaimsIdentity.DefaultIssuer, ClaimsIdentity.DefaultIssuer, (ClaimsIdentity)null)
        {
        }

        /// <summary>
        /// Creates a <see cref="Claim"/> with the specified type, value, and value type.
        /// </summary>
        /// <param name="type">The claim type.</param>
        /// <param name="value">The claim value.</param>
        /// <param name="valueType">The claim value type.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> or <paramref name="value"/> is null.</exception>
        /// <remarks>
        /// <see cref="Claim.Issuer"/> is set to <see cref="ClaimsIdentity.DefaultIssuer"/>,
        /// <see cref="Claim.OriginalIssuer"/> is set to <see cref="ClaimsIdentity.DefaultIssuer"/>,
        /// and <see cref="Claim.Subject"/> is set to null.
        /// </remarks>
        /// <seealso cref="ClaimsIdentity"/>
        /// <seealso cref="ClaimTypes"/>        
        /// <seealso cref="ClaimValueTypes"/>
        public Claim(string type, string value, string valueType)
            : this(type, value, valueType, ClaimsIdentity.DefaultIssuer, ClaimsIdentity.DefaultIssuer, (ClaimsIdentity)null)
        {
        }

        /// <summary>
        /// Creates a <see cref="Claim"/> with the specified type, value, value type, and issuer.
        /// </summary>
        /// <param name="type">The claim type.</param>
        /// <param name="value">The claim value.</param>
        /// <param name="valueType">The claim value type. If this parameter is empty or null, then <see cref="ClaimValueTypes.String"/> is used.</param>
        /// <param name="issuer">The claim issuer. If this parameter is empty or null, then <see cref="ClaimsIdentity.DefaultIssuer"/> is used.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> or <paramref name="value"/> is null.</exception>
        /// <remarks>
        /// <see cref="Claim.OriginalIssuer"/> is set to value of the <paramref name="issuer"/> parameter,
        /// <see cref="Claim.Subject"/> is set to null.
        /// </remarks>
        /// <seealso cref="ClaimsIdentity"/>
        /// <seealso cref="ClaimTypes"/>
        /// <seealso cref="ClaimValueTypes"/>
        public Claim(string type, string value, string valueType, string issuer)
            : this(type, value, valueType, issuer, issuer, (ClaimsIdentity)null)
        {
        }

        /// <summary>
        /// Creates a <see cref="Claim"/> with the specified type, value, value type, issuer and original issuer.
        /// </summary>
        /// <param name="type">The claim type.</param>
        /// <param name="value">The claim value.</param>
        /// <param name="valueType">The claim value type. If this parameter is null, then <see cref="ClaimValueTypes.String"/> is used.</param>
        /// <param name="issuer">The claim issuer. If this parameter is empty or null, then <see cref="ClaimsIdentity.DefaultIssuer"/> is used.</param>
        /// <param name="originalIssuer">The original issuer of this claim. If this parameter is empty or null, then orignalIssuer == issuer.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> or <paramref name="value"/> is null.</exception>
        /// <remarks>
        /// <see cref="Claim.Subject"/> is set to null.
        /// </remarks>
        /// <seealso cref="ClaimsIdentity"/>
        /// <seealso cref="ClaimTypes"/>
        /// <seealso cref="ClaimValueTypes"/>
        public Claim(string type, string value, string valueType, string issuer, string originalIssuer)
            : this(type, value, valueType, issuer, originalIssuer, (ClaimsIdentity)null)
        {
        }
        
        /// <summary>
        /// Creates a <see cref="Claim"/> with the specified type, value, value type, issuer and original issuer.
        /// </summary>
        /// <param name="type">The claim type.</param>
        /// <param name="value">The claim value.</param>
        /// <param name="valueType">The claim value type. If this parameter is null, then <see cref="ClaimValueTypes.String"/> is used.</param>
        /// <param name="issuer">The claim issuer. If this parameter is empty or null, then <see cref="ClaimsIdentity.DefaultIssuer"/> is used.</param>
        /// <param name="originalIssuer">The original issuer of this claim. If this parameter is empty or null, then orignalIssuer == issuer.</param>
        /// <param name="subject">The subject that this claim describes.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> or <paramref name="value"/> is null.</exception>
        /// <seealso cref="ClaimsIdentity"/>
        /// <seealso cref="ClaimTypes"/>
        /// <seealso cref="ClaimValueTypes"/>
        public Claim(string type, string value, string valueType, string issuer, string originalIssuer, ClaimsIdentity subject)
            : this( type, value, valueType, issuer, originalIssuer, subject, null, null )
        {
        }

        /// <summary>
        /// This internal constructor was added as a performance boost when adding claims that are found in the NTToken.
        /// We need to add a property value to distinguish DeviceClaims from UserClaims.
        /// </summary>
        /// <param name="propertyKey">This allows adding a property when adding a Claim.</param>
        /// <param name="propertyValue">The value associcated with the property.</param>
        internal Claim(string type, string value, string valueType, string issuer, string originalIssuer, ClaimsIdentity subject, string propertyKey, string propertyValue)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            Contract.EndContractBlock();

            m_type = type;
            m_value = value;

            if (String.IsNullOrEmpty(valueType))
            {
                m_valueType = ClaimValueTypes.String;
            }
            else
            {
                m_valueType = valueType;
            }

            if (String.IsNullOrEmpty(issuer))
            {
                m_issuer = ClaimsIdentity.DefaultIssuer;

            }
            else
            {
                m_issuer = issuer;
            }

            if (String.IsNullOrEmpty(originalIssuer))
            {
                m_originalIssuer = m_issuer;
            }
            else
            {
                m_originalIssuer = originalIssuer;
            }

            m_subject = subject;

            if (propertyKey != null)
            {
                Properties.Add(propertyKey, propertyValue);
            }
        }

        #endregion

        /// <summary>
        /// Gets the issuer of the <see cref="Claim"/>.
        /// </summary>
        public string Issuer
        {
            get { return m_issuer; }
        }

        [OnDeserialized()]
        private void OnDeserializedMethod(StreamingContext context)
        {
            m_propertyLock = new object();
        }

        /// <summary>
        /// Gets the original issuer of the <see cref="Claim"/>.
        /// </summary>
        /// <remarks>
        /// When the <see cref="OriginalIssuer"/> differs from the <see cref="Issuer"/>, it means 
        /// that the claim was issued by the <see cref="OriginalIssuer"/> and was re-issued
        /// by the <see cref="Issuer"/>.
        /// </remarks>
        public string OriginalIssuer
        {
            get { return m_originalIssuer; }
        }

        /// <summary>        
        /// Gets the collection of Properties associated with the <see cref="Claim"/>.
        /// </summary>
        public IDictionary<string, string> Properties
        {
            get
            {
                if (m_properties == null)
                {
                    lock (m_propertyLock)
                    {
                        if (m_properties == null)
                        {
                            m_properties = new Dictionary<string, string>();
                        }
                    }
                }

                return m_properties;
            }
        }

        /// <summary>
        /// Gets the subject of the <see cref="Claim"/>.
        /// </summary>
        public ClaimsIdentity Subject
        {
            get { return m_subject; }
            internal set { m_subject = value; }
        }

        /// <summary>
        /// Gets the claim type of the <see cref="Claim"/>.
        /// </summary>
        public string Type
        {
            get { return m_type; }
        }

        /// <summary>
        /// Gets the value of the <see cref="Claim"/>.
        /// </summary>
        public string Value
        {
            get { return m_value; }
        }

        /// <summary>
        /// Gets the value type of the <see cref="Claim"/>.
        /// </summary>
        public string ValueType
        {
            get { return m_valueType; }
        }

        /// <summary>
        /// Returns a new <see cref="Claim"/> object copied from this object. The subject of the new claim object is set to null.
        /// </summary>
        /// <returns>A new <see cref="Claim"/> object copied from this object.</returns>
        /// <remarks>This is a shallow copy operation.</remarks>
        public virtual Claim Clone()
        {
            return Clone((ClaimsIdentity)null);
        }

        /// <summary>
        /// Returns a new <see cref="Claim"/> object copied from this object. The subject of the new claim object is set to identity.
        /// </summary>
        /// <param name="identity">The <see cref="ClaimsIdentity"/> that this <see cref="Claim"/> is associated with.</param>
        /// <returns>A new <see cref="Claim"/> object copied from this object.</returns>
        /// <remarks>This is a shallow copy operation.</remarks>
        public virtual Claim Clone(ClaimsIdentity identity)
        {
            Claim newClaim = new Claim(m_type, m_value, m_valueType, m_issuer, m_originalIssuer, identity);
            if (m_properties != null)
            {
                foreach (string key in m_properties.Keys)
                {
                    newClaim.Properties[key] = m_properties[key];
                }
            }

            return newClaim;
        }

        /// <summary>
        /// Returns a string representation of the <see cref="Claim"/> object.
        /// </summary>
        /// <remarks>
        /// The returned string contains the values of the <see cref="Type"/> and <see cref="Value"/> properties.
        /// </remarks>
        /// <returns>The string representation of the <see cref="Claim"/> object.</returns>
        public override string ToString()
        {
            return String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}: {1}", m_type, m_value);
        }
    }
}
