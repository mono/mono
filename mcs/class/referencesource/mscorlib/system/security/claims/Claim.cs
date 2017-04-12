//------------------------------------------------------------------------------
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>Microsoft</OWNER>
// 

//
// Claim.cs
//

namespace System.Security.Claims
{
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.IO;
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
        
        [NonSerialized]
        byte[] m_userSerializationData;

        Dictionary<string, string> m_properties;

        [NonSerialized]
        object m_propertyLock = new object();

        [NonSerialized]
        ClaimsIdentity m_subject;

        private enum SerializationMask
        {
            None = 0,
            NameClaimType = 1,
            RoleClaimType = 2,
            StringType = 4,
            Issuer = 8,
            OriginalIssuerEqualsIssuer = 16,
            OriginalIssuer = 32,
            HasProperties = 64,
            UserData = 128,
        }

        #region Claim Constructors

        /// <summary>
        /// Initializes an instance of <see cref="Claim"/> using a <see cref="BinaryReader"/>.
        /// Normally the <see cref="BinaryReader"/> is constructed using the bytes from <see cref="WriteTo(BinaryWriter)"/> and initialized in the same way as the <see cref="BinaryWriter"/>.
        /// </summary>
        /// <param name="reader">a <see cref="BinaryReader"/> pointing to a <see cref="Claim"/>.</param>
        /// <exception cref="ArgumentNullException">if 'reader' is null.</exception>
        public Claim(BinaryReader reader)
            : this(reader, null)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="Claim"/> using a <see cref="BinaryReader"/>.
        /// Normally the <see cref="BinaryReader"/> is constructed using the bytes from <see cref="WriteTo(BinaryWriter)"/> and initialized in the same way as the <see cref="BinaryWriter"/>.
        /// </summary>
        /// <param name="reader">a <see cref="BinaryReader"/> pointing to a <see cref="Claim"/>.</param>
        /// <param name="subject"> the value for <see cref="Claim.Subject"/>, which is the <see cref="ClaimsIdentity"/> that has these claims.</param>
        /// <exception cref="ArgumentNullException">if 'reader' is null.</exception>
        public Claim(BinaryReader reader, ClaimsIdentity subject)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            Initialize(reader, subject);
        }

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

        /// <summary>
        /// Copy constructor for <see cref="Claim"/>
        /// </summary>
        /// <param name="other">the <see cref="Claim"/> to copy.</param>
        /// <remarks><see cref="Claim.Subject"/>will be set to 'null'.</remarks>
        /// <exception cref="ArgumentNullException">if 'other' is null.</exception>
        protected Claim(Claim other)
            : this(other, (other == null ? (ClaimsIdentity)null : other.m_subject))
        {
        }

        /// <summary>
        /// Copy constructor for <see cref="Claim"/>
        /// </summary>
        /// <param name="other">the <see cref="Claim"/> to copy.</param>
        /// <param name="subject">the <see cref="ClaimsIdentity"/> to assign to <see cref="Claim.Subject"/>.</param>
        /// <remarks><see cref="Claim.Subject"/>will be set to 'subject'.</remarks>
        /// <exception cref="ArgumentNullException">if 'other' is null.</exception>
        protected Claim(Claim other, ClaimsIdentity subject)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            m_issuer = other.m_issuer;
            m_originalIssuer = other.m_originalIssuer;
            m_subject = subject;
            m_type = other.m_type;
            m_value = other.m_value;
            m_valueType = other.m_valueType;
            if (other.m_properties != null)
            {
                m_properties = new Dictionary<string, string>();
                foreach (var key in other.m_properties.Keys)
                {
                    m_properties.Add(key, other.m_properties[key]);
                }
            }

            if (other.m_userSerializationData != null)
            {
                m_userSerializationData = other.m_userSerializationData.Clone() as byte[];
            }
        }

        #endregion

        /// <summary>
        /// Contains any additional data provided by a derived type, typically set when calling <see cref="WriteTo(BinaryWriter, byte[])"/>.</param>
        /// </summary>
        protected virtual byte[] CustomSerializationData
        {
            get
            {
                return m_userSerializationData;
            }
        }

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
        /// <seealso cref="ClaimTypes"/>.
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
        /// <seealso cref="ClaimValueTypes"/>
        public string ValueType
        {
            get { return m_valueType; }
        }

        /// <summary>
        /// Creates a new instance <see cref="Claim"/> with values copied from this object.
        /// </summary>
        public virtual Claim Clone()
        {
            return Clone((ClaimsIdentity)null);
        }

        /// <summary>
        /// Creates a new instance <see cref="Claim"/> with values copied from this object.
        /// </summary>
        /// <param name="identity">the value for <see cref="Claim.Subject"/>, which is the <see cref="ClaimsIdentity"/> that has these claims.
        /// <remarks><see cref="Claim.Subject"/> will be set to 'identity'.</remarks>
        public virtual Claim Clone(ClaimsIdentity identity)
        {
            return new Claim(this, identity);
        }

        private void Initialize(BinaryReader reader, ClaimsIdentity subject)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            m_subject = subject;

            SerializationMask mask = (SerializationMask)reader.ReadInt32();
            int numPropertiesRead = 1;
            int numPropertiesToRead = reader.ReadInt32();
            m_value = reader.ReadString();

            if ((mask & SerializationMask.NameClaimType) == SerializationMask.NameClaimType)
            {
                m_type = ClaimsIdentity.DefaultNameClaimType;
            }
            else if ((mask & SerializationMask.RoleClaimType) == SerializationMask.RoleClaimType)
            {
                m_type = ClaimsIdentity.DefaultRoleClaimType;
            }
            else
            {
                m_type = reader.ReadString();
                numPropertiesRead++;
            }

            if ((mask & SerializationMask.StringType) == SerializationMask.StringType)
            {
                m_valueType = reader.ReadString();
                numPropertiesRead++;
            }
            else
            {
                m_valueType = ClaimValueTypes.String;
            }

            if ((mask & SerializationMask.Issuer) == SerializationMask.Issuer)
            {
                m_issuer = reader.ReadString();
                numPropertiesRead++;
            }
            else
            {
                m_issuer = ClaimsIdentity.DefaultIssuer;
            }

            if ((mask & SerializationMask.OriginalIssuerEqualsIssuer) == SerializationMask.OriginalIssuerEqualsIssuer)
            {
                m_originalIssuer = m_issuer;
            }
            else if ((mask & SerializationMask.OriginalIssuer) == SerializationMask.OriginalIssuer)
            {
                m_originalIssuer = reader.ReadString();
                numPropertiesRead++;
            }
            else
            {
                m_originalIssuer = ClaimsIdentity.DefaultIssuer;
            }

            if ((mask & SerializationMask.HasProperties) == SerializationMask.HasProperties)
            {
                // 
                int numProperties = reader.ReadInt32();
                for (int i = 0; i < numProperties; i++)
                {
                    Properties.Add(reader.ReadString(), reader.ReadString());
                }
            }

            if ((mask & SerializationMask.UserData) == SerializationMask.UserData)
            {
                // 
                int cb = reader.ReadInt32();
                m_userSerializationData = reader.ReadBytes(cb);
                numPropertiesRead++;
            }

            for (int i = numPropertiesRead; i < numPropertiesToRead; i++)
            {
                reader.ReadString();
            }
        }

        /// <summary>
        /// Serializes using a <see cref="BinaryWriter"/>
        /// </summary>
        /// <param name="writer">the <see cref="BinaryWriter"/> to use for data storage.</param>
        /// <exception cref="ArgumentNullException">if 'writer' is null.</exception>
        public virtual void WriteTo(BinaryWriter writer)
        {
            WriteTo(writer, null);
        }

        /// <summary>
        /// Serializes using a <see cref="BinaryWriter"/>
        /// </summary>
        /// <param name="writer">the <see cref="BinaryWriter"/> to use for data storage.</param>
        /// <param name="userData">additional data provided by derived type.</param>
        /// <exception cref="ArgumentNullException">if 'writer' is null.</exception>
        protected virtual void WriteTo(BinaryWriter writer, byte[] userData)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }

            // 


            int numberOfPropertiesWritten = 1;
            SerializationMask mask = SerializationMask.None;
            if (string.Equals(m_type, ClaimsIdentity.DefaultNameClaimType))
            {
                mask |= SerializationMask.NameClaimType;
            }
            else if (string.Equals(m_type, ClaimsIdentity.DefaultRoleClaimType))
            {
                mask |= SerializationMask.RoleClaimType;
            }
            else
            {
                numberOfPropertiesWritten++;
            }

            if (!string.Equals(m_valueType, ClaimValueTypes.String, StringComparison.Ordinal))
            {
                numberOfPropertiesWritten++;
                mask |= SerializationMask.StringType;
            }

            if (!string.Equals(m_issuer, ClaimsIdentity.DefaultIssuer, StringComparison.Ordinal))
            {
                numberOfPropertiesWritten++;
                mask |= SerializationMask.Issuer;
            }

            if (string.Equals(m_originalIssuer, m_issuer, StringComparison.Ordinal))
            {
                mask |= SerializationMask.OriginalIssuerEqualsIssuer;
            }
            else if (!string.Equals(m_originalIssuer, ClaimsIdentity.DefaultIssuer, StringComparison.Ordinal))
            {
                numberOfPropertiesWritten++;
                mask |= SerializationMask.OriginalIssuer;
            }

            if (Properties.Count > 0)
            {
                numberOfPropertiesWritten++;
                mask |= SerializationMask.HasProperties;
            }

            // 
            if (userData != null && userData.Length > 0)
            {
                numberOfPropertiesWritten++;
                mask |= SerializationMask.UserData;
            }

            writer.Write((Int32)mask);
            writer.Write((Int32)numberOfPropertiesWritten);
            writer.Write(m_value);

            if (((mask & SerializationMask.NameClaimType) != SerializationMask.NameClaimType) && ((mask & SerializationMask.RoleClaimType) != SerializationMask.RoleClaimType))
            {
                writer.Write(m_type);
            }

            if ((mask & SerializationMask.StringType) == SerializationMask.StringType)
            {
                writer.Write(m_valueType);
            }

            if ((mask & SerializationMask.Issuer) == SerializationMask.Issuer)
            {
                writer.Write(m_issuer);
            }

            if ((mask & SerializationMask.OriginalIssuer) == SerializationMask.OriginalIssuer)
            {
                writer.Write(m_originalIssuer);
            }

            if ((mask & SerializationMask.HasProperties) == SerializationMask.HasProperties)
            {
                writer.Write(Properties.Count);
                foreach (var key in Properties.Keys)
                {
                    writer.Write(key);
                    writer.Write(Properties[key]);
                }
            }

            if ((mask & SerializationMask.UserData) == SerializationMask.UserData)
            {
                writer.Write((Int32)userData.Length);
                writer.Write(userData);
            }

            writer.Flush();
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
