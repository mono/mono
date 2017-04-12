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
// ClaimsPrincipal.cs
//

namespace System.Security.Claims
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Threading;

    /// <summary>
    /// Concrete IPrincipal supporting multiple claims-based identities
    /// </summary>
    [Serializable]
    [ComVisible(true)]
    public class ClaimsPrincipal : IPrincipal
    {
        private enum SerializationMask
        {
            None = 0,
            HasIdentities = 1,
            UserData = 2
        }

        [NonSerialized]
        private byte[] m_userSerializationData;

        [NonSerialized]
        const string PreFix = "System.Security.ClaimsPrincipal.";
        [NonSerialized]
        const string IdentitiesKey = PreFix + "Identities";
        [NonSerialized]
        const string VersionKey = PreFix + "Version";
        [OptionalField(VersionAdded = 2)]
        string m_version = "1.0";
        [OptionalField(VersionAdded = 2)]
        string m_serializedClaimsIdentities;

        // ==== Important
        //
        // adding identities to this principal will have an effect on Authorization 
        // it is a critical as adding sids and claims to the NTToken.
        // adding claimsIdentities to this list will affect the Authorization for this Principal
        // an attempt was made to mark this as SecurityCritical, however because enumerators access it
        // we would need to extend SecuritySafeCritical to the enumerator methods AND the contstructors.
        // In the end, this requires addional [SecuritySafeCritical] attributes.  So is any additional access
        // is added to 'm_identities' then this must be carefully monitored.  This is equivalent to adding sids to the 
        // NTToken and will be used up the stack to make Authorization decisions.
        //
        
        [NonSerialized]
        List<ClaimsIdentity> m_identities = new List<ClaimsIdentity>();

        [NonSerialized]
        static Func<IEnumerable<ClaimsIdentity>, ClaimsIdentity> s_identitySelector = SelectPrimaryIdentity;

        [NonSerialized]
        static Func<ClaimsPrincipal> s_principalSelector = ClaimsPrincipalSelector;

        /// <summary>
        /// This method iterates through the collection of ClaimsIdentities and chooses an identity as the primary.
        /// The choice is made by examining all the identities in order and choosing the first on that is 
        /// a WindowsIdentity OR in the case of no WindowsIdentities, the first identity
        /// </summary>
        static ClaimsIdentity SelectPrimaryIdentity(IEnumerable<ClaimsIdentity> identities)
        {
            if (identities == null)
            {
                throw new ArgumentNullException("identities");
            }

            //
            // Loop through the identities to determine the primary identity.
            //
            ClaimsIdentity selectedClaimsIdentity = null;

            foreach (ClaimsIdentity identity in identities)
            {
                if (identity is WindowsIdentity)
                {
                    //
                    // If there is a WindowsIdentity, return that.
                    //
                    selectedClaimsIdentity = identity;
                    break;
                }
                else if (selectedClaimsIdentity == null)
                {
                    //
                    // If no primary identity has been selected yet, choose the current identity.
                    //
                    selectedClaimsIdentity = identity;
                }
            }

            return selectedClaimsIdentity;
        }

        /// <summary>
        /// Used to set a custom claims principal.
        /// </summary>
        /// <returns></returns>
        static ClaimsPrincipal SelectClaimsPrincipal()
        {
            ClaimsPrincipal claimsPrincipal = Thread.CurrentPrincipal as ClaimsPrincipal;
            if (claimsPrincipal != null)
            {
                return claimsPrincipal;
            }

            return new ClaimsPrincipal(Thread.CurrentPrincipal);
        }

        public static Func<IEnumerable<ClaimsIdentity>, ClaimsIdentity> PrimaryIdentitySelector
        {
            get
            {
                return s_identitySelector;
            }
            [SecurityCritical]
            set
            {
                s_identitySelector = value;
            }
        }

        public static Func<ClaimsPrincipal> ClaimsPrincipalSelector
        {
            get
            {
                return s_principalSelector;
            }
            [SecurityCritical]
            set
            {
                s_principalSelector = value;
            }
        }
       
        #region ClaimsPrincipal Constructors

        /// <summary>
        /// Initializes an instance of <see cref="ClaimsPrincipal"/>
        /// </summary>
        public ClaimsPrincipal()
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="Principal"/>
        /// </summary>
        /// <param name="identities">Collection of <see cref="ClaimsIdentity"/> representing the subjects in the principal. </param>
        /// 
        public ClaimsPrincipal(IEnumerable<ClaimsIdentity> identities)
        {
            if (identities == null)
            {
                throw new ArgumentNullException("identities");
            }

            Contract.EndContractBlock();

            m_identities.AddRange(identities);
        }

        /// <summary>
        /// Initializes an instance of <see cref="ClaimsPrincipal"/>
        /// </summary>
        /// <param name="identity">Collection of <see cref="ClaimsIdentity"/> representing the subjects in the principal. </param>
        /// 
        public ClaimsPrincipal(IIdentity identity)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }

            Contract.EndContractBlock();

            ClaimsIdentity ci = identity as ClaimsIdentity;
            if (ci != null)
            {
                m_identities.Add(ci);
            }
            else
            {
                m_identities.Add(new ClaimsIdentity(identity));
            }
        }

        /// <summary>
        /// Initializes an instance of <see cref="Principal"/>
        /// </summary>
        /// <param name="principal">IPrincipal whose information is copied instance</param>
        public ClaimsPrincipal(IPrincipal principal)
        {
            if (null == principal)
            {
                throw new ArgumentNullException("principal");
            }

            Contract.EndContractBlock();

            //
            // If IPrincipal is a ClaimsPrincipal add all of the identities
            // If IPrincipal is not a ClaimsPrincipal, create a new identity from IPrincipal.Identity
            //
            ClaimsPrincipal cp = principal as ClaimsPrincipal;
            if (null == cp)
            {
                m_identities.Add(new ClaimsIdentity(principal.Identity));
            }
            else
            {
                if (null != cp.Identities)
                {
                    m_identities.AddRange(cp.Identities);
                }
            }
        }

        /// <summary>
        /// Initializes an instance of <see cref="ClaimsPrincipal"/> using a <see cref="BinaryReader"/>.
        /// Normally the <see cref="BinaryReader"/> is constructed using the bytes from <see cref="WriteTo(BinaryWriter)"/> and initialized in the same way as the <see cref="BinaryWriter"/>.
        /// </summary>
        /// <param name="reader">a <see cref="BinaryReader"/> pointing to a <see cref="ClaimsPrincipal"/>.</param>
        /// <exception cref="ArgumentNullException">if 'reader' is null.</exception>
        public ClaimsPrincipal(BinaryReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            Initialize(reader);
        }


        [SecurityCritical]
        protected ClaimsPrincipal(SerializationInfo info, StreamingContext context)
        {
            if (null == info)
            {
                throw new ArgumentNullException("info");
            }

            Deserialize(info, context);
        }

        /// <summary>
        /// Contains any additional data provided by derived type, typically set when calling <see cref="WriteTo(BinaryWriter, byte[])"/>.</param>
        /// </summary>
        protected virtual byte[] CustomSerializationData
        {
            get
            {
                return m_userSerializationData;
            }
        }

        /// <summary>
        /// Creates a new instance of <see cref="ClaimsPrincipal"/> with values copied from this object.
        /// </summary>
        public virtual ClaimsPrincipal Clone()
        {
            return new ClaimsPrincipal(this);
        }

        /// <summary>
        /// Provides and extensibility point for derived types to create a custom <see cref="ClaimsIdentity"/>.
        /// </summary>
        /// <param name="reader">the <see cref="BinaryReader"/>that points at the claim.</param>
        /// <exception cref="ArgumentNullException">if 'reader' is null.</exception>
        /// <returns>a new <see cref="ClaimsIdentity"/>.</returns>
        protected virtual ClaimsIdentity CreateClaimsIdentity(BinaryReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            return new ClaimsIdentity(reader);
        }

        #endregion ClaimsPrincipal Constructors
        
        [OnSerializing()]
        [SecurityCritical]
        private void OnSerializingMethod(StreamingContext context)
        {
            if (this is ISerializable)
                return;

            m_serializedClaimsIdentities = SerializeIdentities();
        }

        [OnDeserialized()]
        [SecurityCritical]
        private void OnDeserializedMethod(StreamingContext context)
        {
            if (this is ISerializable)
                return;

            DeserializeIdentities(m_serializedClaimsIdentities);
            m_serializedClaimsIdentities = null;
        }

        [SecurityCritical]
        [SecurityPermission(SecurityAction.Assert, SerializationFormatter = true)]
        protected virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (null == info)
            {
                throw new ArgumentNullException("info");
            }

            Contract.EndContractBlock();

            info.AddValue(IdentitiesKey, SerializeIdentities());
            info.AddValue(VersionKey, m_version);

        }

        [SecurityCritical]
        [SecurityPermission(SecurityAction.Assert, SerializationFormatter = true)]
        void Deserialize(SerializationInfo info, StreamingContext context)
        {
            if (null == info)
            {
                throw new ArgumentNullException("info");
            }

            SerializationInfoEnumerator enumerator = info.GetEnumerator();
            while (enumerator.MoveNext())
            {
                switch (enumerator.Name)
                {
                    case IdentitiesKey:
                        DeserializeIdentities(info.GetString(IdentitiesKey));
                        break;

                    case VersionKey:
                        m_version = info.GetString(VersionKey);
                        break;

                    default:
                        // Ignore other fields for forward compatability.
                        break;
                }
            }
        }

        /// <summary>
        /// Deserializes a base64 string of a List of ClaimsIdentity. 
        /// The layout is a list of strings where each ClaimsIdentity occupies two entries.
        /// [ int | string.Empty, Base64(ClaimsIdentity), ...]
        /// If a non-null string is found, assume it is the handle and this indicates that a we are rehydrating a WindowsIdentity.
        /// This design was put together to solve failures when users, create a restricted appdomain {sandbox}
        /// and have WindowsIdentity as an Identity.
        /// 
        /// In PartialTrust scenarios, it is necessary to call BinaryFormatter.Deserialize(x, x, false). The false
        /// parameter controls if the serializer will perform a demand.
        /// </summary>
        /// <param name="identities"></param>
        [SecurityCritical]
        private void DeserializeIdentities(string identities)
        {
            m_identities = new List<ClaimsIdentity>();

            if (!string.IsNullOrEmpty(identities))
            {
                List<string> listOfIdentities = null;
                BinaryFormatter formatter = new BinaryFormatter();
                using (MemoryStream stream = new MemoryStream(Convert.FromBase64String(identities)))
                {
                    listOfIdentities = (List<string>)formatter.Deserialize(stream, null, false);

                    // if the first entry is not empty, we are dealing with a windowsIdentity
                    for (int i = 0; i < listOfIdentities.Count; i += 2)
                    {
                        ClaimsIdentity claimsIdentity = null;
                        using (MemoryStream claimsIdentityStream = new MemoryStream(Convert.FromBase64String(listOfIdentities[i + 1])))
                        {
                            claimsIdentity = (ClaimsIdentity)formatter.Deserialize(claimsIdentityStream, null, false);
                        }

                        // found a WindowsIdentity
                        if (!string.IsNullOrEmpty(listOfIdentities[i]))
                        {
                            Int64 handle;
                            if (Int64.TryParse(listOfIdentities[i], NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out handle))
                            {
                                claimsIdentity = new WindowsIdentity(claimsIdentity, new IntPtr(handle));
                            }
                            else
                            {
                                throw new SerializationException(Environment.GetResourceString("Serialization_CorruptedStream"));
                            }
                        }

                        m_identities.Add(claimsIdentity);
                    }
                }
            }
        }
        /// <summary>
        /// Package up all identities into a stream.
        /// Special case windows identity, since deserializing it fails in PT scenarios. Internal access has been added
        /// to the WindowsIdentity to get the value of the NTToken and to get a copy of the base ClaimsIdentity so that
        /// the WindowsIdentity can be reconstituted when deserialized.
        /// NOTE: that the type check is an exact match, if users have overridden the Windows Identity, then they are on their own.
        /// That is OK because they will have to implement ISerializable.
        /// 
        /// In PartialTrust scenarios, it is necessary to call BinaryFormatter.Serialize(x, x, x, false). The false
        /// parameter controls if the serializer will perform a demand.
        /// </summary>
        /// <returns>A base64 string of the serialized identities.</returns>
        [SecurityCritical]
        private string SerializeIdentities()
        {
            List<string> identities = new List<string>();
            BinaryFormatter formatter = new BinaryFormatter();

            foreach (ClaimsIdentity identity in m_identities)
            {
                if (identity.GetType() == typeof(WindowsIdentity))
                {
                    WindowsIdentity windowsIdentity = identity as WindowsIdentity;
                    identities.Add(windowsIdentity.GetTokenInternal().ToInt64().ToString(NumberFormatInfo.InvariantInfo));
                    using (MemoryStream claimsIdentityStream = new MemoryStream())
                    {
                        formatter.Serialize(claimsIdentityStream, windowsIdentity.CloneAsBase(), null, false);
                        identities.Add(Convert.ToBase64String(claimsIdentityStream.GetBuffer(), 0, (int)claimsIdentityStream.Length));
                    }
                }
                else
                {
                    using (MemoryStream claimsIdentityStream = new MemoryStream())
                    {
                        identities.Add("");
                        formatter.Serialize(claimsIdentityStream, identity, null, false);
                        identities.Add(Convert.ToBase64String(claimsIdentityStream.GetBuffer(), 0, (int)claimsIdentityStream.Length));
                    }
                }
            }

            using (MemoryStream ms = new MemoryStream())
            {
                formatter.Serialize(ms, identities, null, false);
                return Convert.ToBase64String(ms.GetBuffer(), 0, (int)ms.Length);
            }

        }

        // adding identities to this principal will have an effect on Authorization 
        // it is as critical as adding sids and claims to the NTToken.
        [SecurityCritical]
        public virtual void AddIdentity(ClaimsIdentity identity)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }

            Contract.EndContractBlock();

            m_identities.Add(identity);
        }

        // adding identities to this principal will have an effect on Authorization 
        // it is as critical as adding sids and claims to the NTToken. 
        //
        [SecurityCritical]
        public virtual void AddIdentities(IEnumerable<ClaimsIdentity> identities)
        {
            if (identities == null)
            {
                throw new ArgumentNullException("identities");
            }

            Contract.EndContractBlock();

            m_identities.AddRange(identities);
        }

        /// <summary>
        /// Gets the claims as <see cref="IEnumerable{Claim}"/>, associated with this <see cref="ClaimsPrincipal"/> by enumerating all <see cref="ClaimsIdentities"/>.
        /// </summary>
        public virtual IEnumerable<Claim> Claims
        {
            get
            {
                foreach (ClaimsIdentity identity in Identities)
                {
                    foreach (Claim claim in identity.Claims)
                    {
                        yield return claim;
                    }
                }
            }
        }

        /// <summary>
        /// Returns the Current Principal by calling a delegate.  Users may specify the delegate.
        /// </summary>
        public static ClaimsPrincipal Current
        {
            // just accesses the current selected principal selector, doesn't set
            get
            {
                if (s_principalSelector != null)
                {
                    return s_principalSelector();
                }
                else
                {
                    return SelectClaimsPrincipal();
                }
            }
        }

        /// <summary>
        /// Retrieves a <see cref="IEnumerable{Claim}"/> where each claim is matched by <param name="match"/>.
        /// </summary>
        /// <param name="match">The predicate that performs the matching logic.</param>
        /// <returns>A <see cref="IEnumerable{Claim}"/> of matched claims.</returns>  
        /// <remarks>Returns claims from all Identities</remarks>
        /// SafeCritical since it access m_identities
        public virtual IEnumerable<Claim> FindAll(Predicate<Claim> match)
        {
            if (match == null)
            {
                throw new ArgumentNullException("match");
            }

            Contract.EndContractBlock();

            List<Claim> claims = new List<Claim>();

            foreach (ClaimsIdentity identity in Identities)
            {
                if (identity != null)
                {
                    foreach (Claim claim in identity.FindAll(match))
                    {
                        claims.Add(claim);
                    }
                }
            }

            return claims.AsReadOnly();
        }

        /// <summary>
        /// Retrieves a <see cref="IEnumerable{Claim}"/> where each Claim.Type equals <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type of the claim to match.</param>
        /// <returns>A <see cref="IEnumerable{Claim}"/> of matched claims.</returns>   
        /// <remarks>Comparison is made using Ordinal case in-sensitive on type.<</remarks>public IEnumerable<Claim> FindAll(string claimType)
        /// SafeCritical since it access m_identities
        public virtual IEnumerable<Claim> FindAll(string type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            Contract.EndContractBlock();

            List<Claim> claims = new List<Claim>();

            foreach (ClaimsIdentity identity in Identities)
            {
                if (identity != null)
                {
                    foreach (Claim claim in identity.FindAll(type))
                    {
                        claims.Add(claim);
                    }
                }
            }

            return claims.AsReadOnly();
        }

        /// <summary>
        /// Retrieves the first <see cref="Claim"/> that is matched by <param name="match"/>.
        /// </summary>
        /// <param name="match">The predicate that performs the matching logic.</param>
        /// <returns>A <see cref="Claim"/>, null if nothing matches.</returns>
        /// <remarks>All identities are queried.</remarks>
        public virtual Claim FindFirst(Predicate<Claim> match)
        {
            if (match == null)
            {
                throw new ArgumentNullException("match");
            }

            Contract.EndContractBlock();

            Claim claim = null;

            foreach (ClaimsIdentity identity in Identities)
            {
                if (identity != null)
                {
                    claim = identity.FindFirst(match);
                    if (claim != null)
                    {
                        return claim;
                    }
                }
            }

            return claim;
        }

        /// <summary>
        /// Retrieves the first <see cref="Claim"/> where the Claim.Type equals <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type of the claim to match.</param>
        /// <returns>A <see cref="Claim"/>, null if nothing matches.</returns>
        /// <remarks>Comparison is made using Ordinal case in-sensitive, all identities are queried.</remarks>
        public virtual Claim FindFirst(string type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            Contract.EndContractBlock();

            Claim claim = null;

            for (int i = 0; i < m_identities.Count; i++)
            {
                if (m_identities[i] != null)
                {
                    claim = m_identities[i].FindFirst(type);
                    if (claim != null)
                    {
                        return claim;
                    }
                }
            }

            return claim;
        }

        /// <summary>
        /// Determines if a claim is contained within all the ClaimsIdentities in this ClaimPrincipal.
        /// </summary>
        /// <param name="match">The predicate that performs the matching logic.</param>
        /// <returns>true if a claim is found, false otherwise.</returns>
        public virtual bool HasClaim(Predicate<Claim> match)
        {
            if (match == null)
            {
                throw new ArgumentNullException("match");
            }

            Contract.EndContractBlock();

            for (int i = 0; i < m_identities.Count; i++)
            {
                if (m_identities[i] != null)
                {
                    if (m_identities[i].HasClaim(match))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Determines if a claim of claimType AND claimValue exists in any of the identities.
        /// </summary>
        /// <param name="type"> the type of the claim to match.</param>
        /// <param name="value"> the value of the claim to match.</param>
        /// <returns>true if a claim is matched, false otherwise.</returns>
        public virtual bool HasClaim(string type, string value)
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

            for (int i = 0; i < m_identities.Count; i++)
            {
                if (m_identities[i] != null)
                {
                    if (m_identities[i].HasClaim(type, value))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Collection of <see cref="ClaimsIdentity" />
        /// </summary>
        public virtual IEnumerable<ClaimsIdentity> Identities
        {
            // retruns a RO list of identites
            get { return m_identities.AsReadOnly(); }
        }

        /// <summary>
        /// Gets the identity of the current principal.
        /// </summary>
        public virtual System.Security.Principal.IIdentity Identity
        {
            get
            {
                if (s_identitySelector != null)
                {
                    return s_identitySelector(m_identities);
                }
                else
                {
                    return SelectPrimaryIdentity(m_identities);
                }
            }
        }

        /// <summary>
        /// IsInRole answers the question: does an identity this principal possesses
        /// contain a claim of type RoleClaimType where the value is '==' to the role.
        /// </summary>
        /// <param name="role">The role to check for.</param>
        /// <returns>'True' if a claim is found. Otherwise 'False'.</returns>
        /// <remarks>Each Identity has its own definition of the ClaimType that represents a role.</remarks>
        public virtual bool IsInRole(string role)
        {
            for (int i = 0; i < m_identities.Count; i++)
            {
                if (m_identities[i] != null)
                {
                    if (m_identities[i].HasClaim(m_identities[i].RoleClaimType, role))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Initializes from a <see cref="BinaryReader"/>. Normally the reader is initialized with the results from <see cref="WriteTo(BinaryWriter)"/>
        /// Normally the <see cref="BinaryReader"/> is initialized in the same way as the <see cref="BinaryWriter"/> passed to <see cref="WriteTo(BinaryWriter)"/>.
        /// </summary>
        /// <param name="reader">a <see cref="BinaryReader"/> pointing to a <see cref="ClaimsPrincipal"/>.</param>
        /// <exception cref="ArgumentNullException">if 'reader' is null.</exception>
        private void Initialize(BinaryReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            SerializationMask mask = (SerializationMask)reader.ReadInt32();
            int numPropertiesToRead = reader.ReadInt32();
            int numPropertiesRead = 0;
            if ((mask & SerializationMask.HasIdentities) == SerializationMask.HasIdentities)
            {
                numPropertiesRead++;
                int numberOfIdentities = reader.ReadInt32();
                for (int index = 0; index < numberOfIdentities; ++index)
                {                    
                    // directly add to m_identities as that is what we serialized from
                    m_identities.Add(CreateClaimsIdentity(reader));
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

            int numberOfPropertiesWritten = 0;
            var mask = SerializationMask.None;
            if (m_identities.Count > 0)
            {
                mask |= SerializationMask.HasIdentities;
                numberOfPropertiesWritten++;
            }

            if (userData != null && userData.Length > 0)
            {
                numberOfPropertiesWritten++;
                mask |= SerializationMask.UserData;
            }

            writer.Write((Int32)mask);
            writer.Write((Int32)numberOfPropertiesWritten);
            if ((mask & SerializationMask.HasIdentities) == SerializationMask.HasIdentities)
            {
                writer.Write(m_identities.Count);
                foreach (var identity in m_identities)
                {
                    identity.WriteTo(writer);
                }
            }

            if ((mask & SerializationMask.UserData) == SerializationMask.UserData)
            {
                writer.Write((Int32)userData.Length);
                writer.Write(userData);
            }

            writer.Flush();
        }
    }
}

