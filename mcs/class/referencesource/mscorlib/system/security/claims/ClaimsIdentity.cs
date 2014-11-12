// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>[....]</OWNER>
// 

//
// ClaimsIdentity.cs
//

namespace System.Security.Claims
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Security.Permissions;
    using System.Security.Principal;

    /// <summary>
    /// An Identity that is represented by a set of claims.
    /// </summary>
    [Serializable]
    [ComVisible(true)]
    public class ClaimsIdentity : IIdentity
    {
        [NonSerialized]
        const string PreFix = "System.Security.ClaimsIdentity.";
        [NonSerialized]
        const string ActorKey = PreFix + "actor";        
        [NonSerialized]
        const string AuthenticationTypeKey = PreFix + "authenticationType";        
        [NonSerialized]
        const string BootstrapContextKey = PreFix + "bootstrapContext";
        [NonSerialized]
        const string ClaimsKey = PreFix + "claims";
        [NonSerialized]
        const string LabelKey = PreFix + "label";
        [NonSerialized]
        const string NameClaimTypeKey = PreFix + "nameClaimType";
        [NonSerialized]
        const string RoleClaimTypeKey = PreFix + "roleClaimType";
        [NonSerialized]
        const string VersionKey = PreFix + "version";
        [NonSerialized]
        public const string DefaultIssuer = @"LOCAL AUTHORITY";
        [NonSerialized]
        public const string DefaultNameClaimType = ClaimTypes.Name;
        [NonSerialized]
        public const string DefaultRoleClaimType = ClaimTypes.Role;
        // === Important
        //
        // adding claims to this list will affect the Authorization for this Identity 
        // originally marked this as SecurityCritical, however because enumerators access it
        // we would need to extend SecuritySafeCritical to the enumerator methods AND the constructors.
        // In the end, this requires additional [SecuritySafeCritical] attributes.  So if any additional access
        // is added to 'm_instanceClaims' then this must be carefully monitored. This is equivalent to adding sids to the 
        // NTToken and will be used up the stack to make Authorization decisions.
        //

        // these are claims that are added by using the AddClaim, AddClaims methods or passed in the constructor.
        [NonSerialized]
        List<Claim> m_instanceClaims = new List<Claim>();

        // These are claims that are external to the identity. .Net runtime attaches roles owned by principals GenericPrincpal and RolePrincipal here. 
        // They are not serialized OR remembered when cloned. Access through public method: ClaimProviders.
        [NonSerialized]
        Collection<IEnumerable<Claim>> m_externalClaims = new Collection<IEnumerable<Claim>>();

        [NonSerialized]
        string m_nameType = DefaultNameClaimType;
        
        [NonSerialized]
        string m_roleType = DefaultRoleClaimType;
        
        [OptionalField(VersionAdded=2)]
        string m_version = "1.0";

        [OptionalField(VersionAdded = 2)]
        ClaimsIdentity m_actor;

        [OptionalField(VersionAdded = 2)]
        string m_authenticationType;

        [OptionalField(VersionAdded = 2)]
        object m_bootstrapContext;

        [OptionalField(VersionAdded = 2)]
        string m_label;

        [OptionalField(VersionAdded = 2)]
        string m_serializedNameType;

        [OptionalField(VersionAdded = 2)]
        string m_serializedRoleType;

        [OptionalField(VersionAdded = 2)]
        string m_serializedClaims;
        
        #region ClaimsIdentity Constructors

        /// <summary>
        /// Initializes an instance of <see cref="ClaimsIdentity"/> with an empty claims collection.
        /// </summary>
        /// <remarks>
        /// <see cref="Identity.AuthenticationType"/> is set to null.
        /// </remarks>
        public ClaimsIdentity()
            : this((Claim[])null)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="ClaimsIdentity"/> using the name and authentication type from
        /// an <see cref="IIdentity"/> instance.
        /// </summary>
        /// <param name="identity"><see cref="IIdentity"/> to draw the name and authentication type from.</param>
        /// <exception cref="ArgumentNullException"> if <paramref name="identity"/> is null.</exception>
        public ClaimsIdentity(IIdentity identity)
            : this(identity, (IEnumerable<Claim>)null)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="Identity"/> using an enumerated collection of 
        /// <see cref="Claim"/> objects.
        /// </summary>
        /// <param name="claims">
        /// The collection of <see cref="Claim"/> objects to populate <see cref="Identity.Claims"/> with.
        /// </param>
        /// <remarks>
        /// <see cref="Identity.AuthenticationType"/> is set to null.
        /// </remarks>
        public ClaimsIdentity(IEnumerable<Claim> claims)
            : this((IIdentity) null, claims, null, null, null)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="Identity"/> with an empty <see cref="Claim"/> collection
        /// and the specified authentication type.
        /// </summary>
        /// <param name="authenticationType">The type of authentication used.</param>
        public ClaimsIdentity(string authenticationType)
            : this((IIdentity) null, (IEnumerable<Claim>)null, authenticationType, (string)null, (string)null)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="Identity"/> using an enumerated collection of 
        /// <see cref="Claim"/> objects.
        /// </summary>
        /// <param name="claims">
        /// The collection of <see cref="Claim"/> objects to populate <see cref="Identity.Claims"/> with.
        /// </param>
        /// <param name="authenticationType">The type of authentication used.</param>
        /// <remarks>
        /// <see cref="Identity.AuthenticationType"/> is set to null.
        /// </remarks>
        public ClaimsIdentity(IEnumerable<Claim> claims, string authenticationType)
            : this((IIdentity)null, claims, authenticationType, null, null)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="ClaimsIdentity"/> using the name and authentication type from
        /// an <see cref="IIdentity"/> instance.
        /// </summary>
        /// <param name="identity"><see cref="IIdentity"/> to draw the name and authentication type from.</param>
        /// <exception cref="ArgumentNullException"> if <paramref name="identity"/> is null.</exception>
        public ClaimsIdentity(IIdentity identity, IEnumerable<Claim> claims)
            : this(identity, claims, (string)null, (string)null, (string)null)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="Identity"/> with an empty <see cref="Claim"/> collection,
        /// the specified authentication type, name claim type, and role claim type.
        /// </summary>
        /// <param name="authenticationType">The type of authentication used.</param>
        /// <param name="nameType">The claim type to use for <see cref="Identity.Name"/>.</param>
        /// <param name="roleType">The claim type to use for IClaimsPrincipal.IsInRole(string).</param>
        public ClaimsIdentity(string authenticationType, string nameType, string roleType )
            : this((IIdentity) null, (IEnumerable<Claim>)null, authenticationType, nameType, roleType)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="ClaimsIdentity"/> using an enumeration of type 
        /// <see cref="Claim"/>, authentication type, name claim type, role claim type, and bootstrapContext.
        /// </summary>
        /// <param name="claims">An enumeration of type <see cref="Claim"/> to initialize this identity</param>
        /// <param name="authenticationType">The type of authentication used.</param>
        /// <param name="nameType">The claim type to identify NameClaims.</param>
        /// <param name="roleType">The claim type to identify RoleClaims.</param>
        public ClaimsIdentity(IEnumerable<Claim> claims, string authenticationType, string nameType, string roleType)
            : this((IIdentity)null, claims, authenticationType, nameType, roleType)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="ClaimsIdentity"/> using an enumeration of type 
        /// <see cref="Claim"/>, authentication type, name claim type, role claim type, and bootstrapContext.
        /// </summary>
        /// <param name="identity">The initial identity to base this identity from.</param>
        /// <param name="claims">An enumeration of type <see cref="Claim"/> to initialize this identity.</param>
        /// <param name="authenticationType">The type of authentication used.</param>
        /// <param name="nameType">The claim type to identify NameClaims.</param>
        /// <param name="roleType">The claim type to identify RoleClaims.</param>
        public ClaimsIdentity(IIdentity identity, IEnumerable<Claim> claims, string authenticationType, string nameType, string roleType)
            : this(identity, claims, authenticationType, nameType, roleType, true)
        {

        }
       
        /// <summary>
        /// This constructor was added so that the WindowsIdentity could control if the authenticationType should be checked. For WindowsIdentities this
        /// leads to a priviledged call and will fail where the caller has low priviledge.
        /// </summary>
        /// <param name="identity">The initial identity to base this identity from.</param>
        /// <param name="claims">An enumeration of type <see cref="Claim"/> to initialize this identity.</param>
        /// <param name="authenticationType">The type of authentication used.</param>
        /// <param name="nameType">The claim type to identify NameClaims.</param>
        /// <param name="roleType">The claim type to identify RoleClaims.</param>
        /// <param name="checkAuthType">This boolean flag controls if we blindly set the authenticationType, since call WindowsIdentity.AuthenticationType is a priviledged call.</param>
        internal ClaimsIdentity(IIdentity identity, IEnumerable<Claim> claims, string authenticationType, string nameType, string roleType, bool checkAuthType)
        {
            bool nameTypeSet = false;
            bool roleTypeSet = false;

            // move the authtype, nameType and roleType over from the identity ONLY if they weren't specifically set.
            if(checkAuthType && null != identity && string.IsNullOrEmpty(authenticationType)) 
            {
                // can safely ignore UnauthorizedAccessException from WindowsIdentity, 
                // LSA didn't allow the call and WindowsIdentity throws if property is never accessed, no reason to fail.
                if (identity is WindowsIdentity)
                {
                    try
                    {
                        m_authenticationType = identity.AuthenticationType;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        m_authenticationType = null;
                    }
                }
                else
                {
                    m_authenticationType = identity.AuthenticationType;
                }
            }
            else
            {
                m_authenticationType = authenticationType;
            }

            if(!string.IsNullOrEmpty(nameType))
            {
                m_nameType = nameType;
                nameTypeSet = true;
            }

            if(!string.IsNullOrEmpty(roleType))
            {
                m_roleType = roleType;
                roleTypeSet = true;
            }

            ClaimsIdentity claimsIdentity = identity as ClaimsIdentity;

            if (claimsIdentity != null)
            {
                m_label = claimsIdentity.m_label;

                // give preference to parameters
                if (!nameTypeSet)
                {
                    m_nameType = claimsIdentity.m_nameType;
                }

                if (!roleTypeSet)
                {
                    m_roleType = claimsIdentity.m_roleType;
                }

                m_bootstrapContext = claimsIdentity.m_bootstrapContext;

                if (claimsIdentity.Actor != null)
                {
                    //
                    // Check if the Actor is circular before copying. That check is done while setting
                    // the Actor property and so not really needed here. But checking just for sanity sake
                    //
                    if(!IsCircular(claimsIdentity.Actor))
                    {
                        m_actor = claimsIdentity.Actor;
                    }
                    else
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("InvalidOperationException_ActorGraphCircular"));
                    }
                }

                // We can only copy over the claims we own, it is up to the derived 
                // to copy over claims they own.
                // BUT we need to special case WindowsIdentity as it keeps its own claims.
                // In the case where we are not a windowsIdentity and the claimsIdentity is
                // we need to copy the claims

                if ((claimsIdentity is WindowsIdentity) && (!(this is WindowsIdentity)))
                    SafeAddClaims(claimsIdentity.Claims);
                else
                    SafeAddClaims(claimsIdentity.m_instanceClaims);

            }
            else
            {
                if (identity != null && !string.IsNullOrEmpty(identity.Name))
                {
                    SafeAddClaim(new Claim(m_nameType, identity.Name, ClaimValueTypes.String, DefaultIssuer, DefaultIssuer, this));
                }
            }

            if (claims != null)
            {
                SafeAddClaims(claims);
            }
        }

        /// <summary>
        /// Initializes an instance of <see cref="Identity"/> from a serialized stream created via 
        /// <see cref="ISerializable"/>.
        /// </summary>
        /// <param name="info">
        /// The <see cref="SerializationInfo"/> to read from.
        /// </param>
        /// <param name="context">The <see cref="StreamingContext"/> for serialization. Can be null.</param>
        /// <exception cref="ArgumentNullException">Thrown is the <paramref name="info"/> is null.</exception>
        [SecurityCritical]
        protected ClaimsIdentity(SerializationInfo info, StreamingContext context)
        {
            if (null == info)
            {
                throw new ArgumentNullException("info");
            }

            Deserialize(info, context, true);
        }

        /// <summary>
        /// Initializes an instance of <see cref="Identity"/> from a serialized stream created via 
        /// <see cref="ISerializable"/>.
        /// </summary>
        /// <param name="info">
        /// The <see cref="SerializationInfo"/> to read from.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown is the <paramref name="info"/> is null.</exception>
        [SecurityCritical]
        protected ClaimsIdentity(SerializationInfo info)
        {
            if (null == info)
            {
                throw new ArgumentNullException("info");
            }

            StreamingContext sc = new StreamingContext();
            Deserialize(info, sc, false);
        }

        #endregion

        /// <summary>
        /// Gets the authentication type.
        /// </summary>
        public virtual string AuthenticationType
        {
            get { return m_authenticationType; }
        }

        /// <summary>
        /// Gets a value that indicates whether the user has been authenticated.
        /// </summary>
        public virtual bool IsAuthenticated
        {
            get { return !string.IsNullOrEmpty(m_authenticationType); }
        }

        /// <summary>
        /// Gets or sets a <see cref="ClaimsIdentity"/> that was granted delegation rights.
        /// </summary>
        public ClaimsIdentity Actor
        {
            get { return m_actor; }
            set
            {
                if(value != null)
                {
                    if(IsCircular(value))
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("InvalidOperationException_ActorGraphCircular")); 
                    }
                }
                m_actor = value;
            }
        }
        
        /// <summary>
        /// Gets or sets a context that was used to create this <see cref="ClaimsIdentity"/>.
        /// </summary>
        public object BootstrapContext
        {
            get { return m_bootstrapContext; }
            
            [SecurityCritical]
            set { m_bootstrapContext = value; }
        }

        /// <summary>
        /// Gets the claims as <see cref="IEnumerable{Claim}"/>, associated with this <see cref="ClaimsIdentity"/>.
        /// </summary>       
        /// <remarks>May contain nulls.</remarks>
        public virtual IEnumerable<Claim> Claims
        {
            get
            {
                for (int i = 0; i < m_instanceClaims.Count; i++)
                {
                    yield return m_instanceClaims[i];
                }

                if (m_externalClaims != null)
                {
                    for (int j = 0; j < m_externalClaims.Count; j++)
                    {
                        if (m_externalClaims[j] != null)
                        {
                            foreach (Claim claim in m_externalClaims[j])
                            {
                                yield return claim;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Allow the association of claims with this instance of <see cref="ClaimsIdentity"/>. 
        /// The claims will not be serialized or added in Clone(). They will be included in searches, finds and returned from the call to Claims.
        /// It is recommended the creator of the claims ensures the subject of the claims reflects this <see cref="ClaimsIdentity"/>.
        /// </summary>               
        internal Collection<IEnumerable<Claim>> ExternalClaims
        {
            [FriendAccessAllowed]
            get { return m_externalClaims; }
        }

        /// <summary>
        /// Gets or sets the label for this <see cref="Identity"/>
        /// </summary>
        public string Label
        {
            get { return m_label; }
            set { m_label = value; }
        }

        /// <summary>
        /// Gets the value of the first claim that has a type of NameClaimType. If no claim is found, null is returned.
        /// </summary>        
        public virtual string Name
        {
            // just an accessor for getting the name claim
            get
            {
                Claim claim = FindFirst(m_nameType);
                if (claim != null)
                {
                    return claim.Value;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the claim type used to distinguish claims that refer to the name.
        /// </summary>
        public string NameClaimType
        {
            get { return m_nameType; }
        }

        /// <summary>
        /// Gets the claim type used to distinguish claims that refer to Roles.
        /// </summary>
        public string RoleClaimType
        {
            get { return m_roleType; }
        }

        /// <summary>
        /// Returns a new instance of <see cref="ClaimsIdentity"/> with values copied from this object.
        /// </summary>
        /// <returns>A new <see cref="Identity"/> object copied from this object</returns>
        public virtual ClaimsIdentity Clone()
        {
            ClaimsIdentity newIdentity = new ClaimsIdentity(m_instanceClaims);

            newIdentity.m_authenticationType = this.m_authenticationType;
            newIdentity.m_bootstrapContext = this.m_bootstrapContext;
            newIdentity.m_label = this.m_label;
            newIdentity.m_nameType = this.m_nameType;
            newIdentity.m_roleType = this.m_roleType;
            
            if(this.Actor != null)
            {
                // Check if the Actor is circular before copying. That check is done while setting
                // the Actor property and so not really needed here. But checking just for sanity sake
                if(!IsCircular(this.Actor))
                {
                    newIdentity.Actor = this.Actor;
                }
                else
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperationException_ActorGraphCircular"));
                }
            }

            return newIdentity;
        }

        /// <summary>
        /// Adds a single claim to this ClaimsIdentity. The claim is examined and if the subject != this, then a new claim is 
        /// created by calling claim.Clone(this).  This creates a new claim, with the correct subject.
        /// </summary>
        /// <param name="claims">Enumeration of claims to add.</param>
        /// This is SecurityCritical as we need to control who can add claims to the Identity. Futher down the pipe
        /// Authorization decisions will be made based on the claims found in this collection.
        [SecurityCritical]
        public virtual void AddClaim(Claim claim)
        {
            if (claim == null)
            {
                throw new ArgumentNullException("claim");
            }

            Contract.EndContractBlock();

            if(object.ReferenceEquals(claim.Subject, this))
            {
                m_instanceClaims.Add(claim);
            }
            else
            {
                m_instanceClaims.Add(claim.Clone(this));
            }
        }

        /// <summary>
        /// Adds a list of claims to this Claims Identity. Each claim is examined and if the subject != this, then a new claim is 
        /// created by calling claim.Clone(this).  This creates a new claim, with the correct subject.
        /// </summary>
        /// <param name="claims">Enumeration of claims to add.</param>
        /// This is SecurityCritical as we need to control who can add claims to the Identity. Futher down the pipe
        /// Authorization decisions will be made based on the claims found in this collection.        
        [SecurityCritical]
        public virtual void AddClaims(IEnumerable<Claim> claims)
        {
            if (claims == null)
            {
                throw new ArgumentNullException("claims");
            }

            Contract.EndContractBlock();

            foreach (Claim claim in claims)
            {
                if (claim == null)
                {
                    continue;
                }

                AddClaim(claim);
            }
        }

        /// <summary>
        /// Attempts to remove a claim from the identity.  It is possible that the claim cannot be removed since it is not owned
        /// by the identity.  This would be the case for role claims that are owned by the Principal.
        /// Matches by object reference.
        /// <summary/>
        [SecurityCritical]
        public virtual bool TryRemoveClaim(Claim claim)
        {
            bool removed = false;

            for (int i = 0; i < m_instanceClaims.Count; i++)
            {
                if (object.ReferenceEquals(m_instanceClaims[i], claim))
                {
                    m_instanceClaims.RemoveAt(i);
                    removed = true;
                    break;
                }
            }
            return removed;
        }

        [SecurityCritical]
        public virtual void RemoveClaim(Claim claim)
        {
            if (!TryRemoveClaim(claim))
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ClaimCannotBeRemoved", claim));
            }
        }

        /// <summary>
        /// Called from constructor, isolated for easy review
        /// This is called from the constructor, this implies that the base class has 
        /// ownership of holding onto the claims.  We can't call AddClaim as that is a virtual and the
        /// Derived class may not be constructed yet.
        /// </summary>
        /// <param name="claims"></param>
        [SecuritySafeCritical]
        void SafeAddClaims(IEnumerable<Claim> claims)
        {
            foreach (Claim claim in claims)
            {
                if (object.ReferenceEquals(claim.Subject, this))
                {
                    m_instanceClaims.Add(claim);
                }
                else
                {
                    m_instanceClaims.Add(claim.Clone(this));
                }
            }
        }

        /// <summary>
        /// Called from constructor, isolated for easy review.
        /// This is called from the constructor, this implies that the base class has 
        /// ownership of holding onto the claims.  We can't call AddClaim as that is a virtual and the
        /// Derived class may not be constructed yet.
        /// </summary>
        /// <param name="claim"></param>
        [SecuritySafeCritical]
        void SafeAddClaim(Claim claim)
        {
            if (object.ReferenceEquals(claim.Subject, this))
            {
                m_instanceClaims.Add(claim);
            }
            else
            {
                m_instanceClaims.Add(claim.Clone(this));
            }
        }

        /// <summary>
        /// Retrieves a <see cref="IEnumerable{Claim}"/> where each claim is matched by <param name="match"/>.
        /// </summary>
        /// <param name="match">The function that performs the matching logic.</param>
        /// <returns>A <see cref="IEnumerable{Claim}"/> of matched claims.</returns>   
        public virtual IEnumerable<Claim> FindAll(Predicate<Claim> match)
        {
            if (match == null)
            {
                throw new ArgumentNullException("match");
            }

            Contract.EndContractBlock();

            List<Claim> claims = new List<Claim>();

            foreach (Claim claim in Claims)
            {
                if (match(claim))
                {
                    claims.Add(claim);
                }
            }

            return claims.AsReadOnly();
        }

        /// <summary>
        /// Retrieves a <see cref="IEnumerable{Claim}"/> where each Claim.Type equals <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type of the claim to match.</param>
        /// <returns>A <see cref="IEnumerable{Claim}"/> of matched claims.</returns>   
        /// <remarks>Comparison is made using Ordinal case in-sensitive on type.<</remarks>
        public virtual IEnumerable<Claim> FindAll(string type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            Contract.EndContractBlock();

            List<Claim> claims = new List<Claim>();

            foreach (Claim claim in Claims)
            {
                if (claim != null)
                {
                    if (string.Equals(claim.Type, type, StringComparison.OrdinalIgnoreCase))
                    {
                        claims.Add(claim);
                    }
                }
            }

            return claims.AsReadOnly();
        }

        /// <summary>
        /// Determines if a claim is contained within this ClaimsIdentity.
        /// </summary>
        /// <param name="match">The function that performs the matching logic.</param>
        /// <returns>true if a claim is found, false otherwise.</returns>
        public virtual bool HasClaim(Predicate<Claim> match)
        {
            if (match == null)
            {
                throw new ArgumentNullException("match");
            }

            Contract.EndContractBlock();

            foreach (Claim claim in Claims)
            {
                if (match(claim))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines if a claim with type AND value is contained in the claims within this ClaimsIdentity.
        /// </summary>
        /// <param name="type"> the type of the claim to match.</param>
        /// <param name="value"> the value of the claim to match.</param>
        /// <returns>true if a claim is matched, false otherwise.</returns>
        /// <remarks>Does not check Issuer or OriginalIssuer.  Comparison is made using Ordinal, case sensitive on value, case in-sensitive on type.</remarks>
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

            foreach (Claim claim in Claims)
            {
                if (claim != null)
                {
                    if (claim != null
                         && string.Equals(claim.Type, type, StringComparison.OrdinalIgnoreCase)
                         && string.Equals(claim.Value, value, StringComparison.Ordinal))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Retrieves the first <see cref="Claim"/> that is matched by <param name="match"/>.
        /// </summary>
        /// <param name="match">The function that performs the matching logic.</param>
        /// <returns>A <see cref="Claim"/>, null if nothing matches.</returns>
        /// <remarks>Comparison is made using Ordinal, case in-sensitive.</remarks>
        public virtual Claim FindFirst(Predicate<Claim> match)
        {
            if (match == null)
            {
                throw new ArgumentNullException("match");
            }

            Contract.EndContractBlock();

            foreach (Claim claim in Claims)
            {
                if (match(claim))
                {
                    return claim;
                }
            }

            return null;
        }

        /// <summary>
        /// Retrieves the first <see cref="Claim"/> where Claim.Type equals <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type of the claim to match.</param>
        /// <returns>A <see cref="Claim"/>, null if nothing matches.</returns>
        /// <remarks>Comparison is made using Ordinal, case in-sensitive.</remarks>
        public virtual Claim FindFirst(string type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            Contract.EndContractBlock();

            foreach (Claim claim in Claims)
            {
                if (claim != null)
                {
                    if (string.Equals(claim.Type, type, StringComparison.OrdinalIgnoreCase))
                    {
                        return claim;
                    }
                }
            }

            return null;
        }

        [OnSerializing()]
        [SecurityCritical]
        private void OnSerializingMethod(StreamingContext context)
        {
            if (this is ISerializable)
                return;
            
            m_serializedClaims   = SerializeClaims();
            m_serializedNameType = m_nameType;
            m_serializedRoleType = m_roleType;
        }

        [OnDeserialized()]
        [SecurityCritical]
        private void OnDeserializedMethod(StreamingContext context)
        {
            if (this is ISerializable)
                return;

            if (!String.IsNullOrEmpty(m_serializedClaims))
            {
                DeserializeClaims(m_serializedClaims);
                m_serializedClaims = null;
            }

            m_nameType = string.IsNullOrEmpty(m_serializedNameType) ? DefaultNameClaimType : m_serializedNameType;
            m_roleType = string.IsNullOrEmpty(m_serializedRoleType) ? DefaultRoleClaimType : m_serializedRoleType;
        }

        [OnDeserializing()]
        private void OnDeserializingMethod(StreamingContext context)
        {
            if (this is ISerializable)
                return;

            m_instanceClaims = new List<Claim>();
            m_externalClaims = new Collection<IEnumerable<Claim>>();
        }

        /// <summary>
        /// Populates the specified <see cref="SerializationInfo"/> with the serialization data for the ClaimsIdentity
        /// </summary>
        /// <param name="info">The serialization information stream to write to. Satisfies ISerializable contract.</param>
        /// <param name="context">Context for serialization. Can be null.</param>
        /// <exception cref="ArgumentNullException">Thrown if the info parameter is null.</exception>
        [SecurityCritical]
        [SecurityPermission(SecurityAction.Assert, SerializationFormatter = true)]
        protected virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (null == info)
            {
                throw new ArgumentNullException("info");
            }
            Contract.EndContractBlock();

            BinaryFormatter formatter = new BinaryFormatter();

            info.AddValue(VersionKey, m_version);
            if (!string.IsNullOrEmpty(m_authenticationType))
            {
                info.AddValue(AuthenticationTypeKey, m_authenticationType);
            }

            info.AddValue(NameClaimTypeKey, m_nameType);
            info.AddValue(RoleClaimTypeKey, m_roleType);

            if (!string.IsNullOrEmpty(m_label))
            {
                info.AddValue(LabelKey, m_label);
            }
            
            //
            // actor
            //
            if (m_actor != null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    formatter.Serialize(ms, m_actor, null, false);
                    info.AddValue(ActorKey, Convert.ToBase64String(ms.GetBuffer(), 0, (int)ms.Length));
                }
            }

            //
            // claims
            //
            info.AddValue(ClaimsKey, SerializeClaims());

            //
            // bootstrapContext
            //
            if (m_bootstrapContext != null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    formatter.Serialize(ms, m_bootstrapContext, null, false);
                    info.AddValue(BootstrapContextKey, Convert.ToBase64String(ms.GetBuffer(), 0, (int)ms.Length));
                }
            }
        }

        [SecurityCritical]
        private void DeserializeClaims(string serializedClaims)
        {
            if (!string.IsNullOrEmpty(serializedClaims))
            {
                using (MemoryStream stream = new MemoryStream(Convert.FromBase64String(serializedClaims)))
                {
                    m_instanceClaims = (List<Claim>)(new BinaryFormatter()).Deserialize(stream, null, false);
                    for (int i = 0; i < m_instanceClaims.Count; i++)
                    {
                        m_instanceClaims[i].Subject = this;
                    }
                }
            }

            if (m_instanceClaims == null)
            {
                m_instanceClaims = new List<Claim>();
            }
        }

        [SecurityCritical]
        private string SerializeClaims()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                (new BinaryFormatter()).Serialize(ms, m_instanceClaims, null, false);
                return Convert.ToBase64String(ms.GetBuffer(), 0, (int)ms.Length);
            }
        }

        /// <summary>
        /// Checks if a circular reference exists to 'this'
        /// </summary>
        /// <param name="subject"></param>
        /// <returns></returns>
        bool IsCircular(ClaimsIdentity subject)
        {
            if(ReferenceEquals(this, subject))
            {
                return true;
            }

            ClaimsIdentity currSubject = subject;

            while(currSubject.Actor != null)
            {
                if(ReferenceEquals(this, currSubject.Actor))
                {
                    return true;
                }

                currSubject = currSubject.Actor;
            }

            return false;
        }

        // <param name="useContext"></param> The reason for this param is due to WindowsIdentity deciding to have an 
        // api that doesn't pass the context to its internal constructor.
        [SecurityCritical]
        [SecurityPermission(SecurityAction.Assert, SerializationFormatter = true)]
        private void Deserialize(SerializationInfo info, StreamingContext context, bool useContext)
        {

            if (null == info)
            {
                throw new ArgumentNullException("info");
            }

            BinaryFormatter bf;

            if (useContext)
                bf = new BinaryFormatter(null, context);
            else
                bf = new BinaryFormatter();


            SerializationInfoEnumerator enumerator = info.GetEnumerator();
            while (enumerator.MoveNext())
            {
                switch (enumerator.Name)
                {
                    case VersionKey:
                        string version = info.GetString(VersionKey);
                        break;

                    case AuthenticationTypeKey:
                        m_authenticationType = info.GetString(AuthenticationTypeKey);
                        break;

                    case NameClaimTypeKey:
                        m_nameType = info.GetString(NameClaimTypeKey);
                        break;

                    case RoleClaimTypeKey:
                        m_roleType = info.GetString(RoleClaimTypeKey);
                        break;

                    case LabelKey:
                        m_label = info.GetString(LabelKey);
                        break;

                    case ActorKey:
                        using (MemoryStream stream = new MemoryStream(Convert.FromBase64String(info.GetString(ActorKey))))
                        {
                            m_actor = (ClaimsIdentity)bf.Deserialize(stream, null, false);
                        }
                        break;

                    case ClaimsKey:
                        DeserializeClaims(info.GetString(ClaimsKey));
                        break;

                    case BootstrapContextKey:
                        using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(info.GetString(BootstrapContextKey))))
                        {
                            m_bootstrapContext = bf.Deserialize(ms, null, false);
                        }
                        break;

                    default:
                        // Ignore other fields for forward compatability.
                        break;
                }
            }
        }
    }
}
