//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System.Globalization;

    public class LocalIdKeyIdentifierClause : SecurityKeyIdentifierClause
    {
        readonly string localId;
        readonly Type[] ownerTypes;

        public LocalIdKeyIdentifierClause(string localId)
            : this(localId, (Type[])null)
        {
        }

        public LocalIdKeyIdentifierClause(string localId, Type ownerType)
            : this(localId, ownerType == null ? (Type[])null : new Type[] { ownerType })
        {
        }

        public LocalIdKeyIdentifierClause(string localId, byte[] derivationNonce, int derivationLength, Type ownerType)
            : this(null, derivationNonce, derivationLength, ownerType == null ? (Type[])null : new Type[] { ownerType })
        {
        }

        internal LocalIdKeyIdentifierClause(string localId, Type[] ownerTypes)
            : this(localId, null, 0, ownerTypes)
        {
        }

        internal LocalIdKeyIdentifierClause(string localId, byte[] derivationNonce, int derivationLength, Type[] ownerTypes)
            : base(null, derivationNonce, derivationLength)
        {
            if (localId == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("localId");
            }
            if (localId == string.Empty)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.LocalIdCannotBeEmpty));
            }
            this.localId = localId;
            this.ownerTypes = ownerTypes;
        }

        public string LocalId
        {
            get { return this.localId; }
        }

        public Type OwnerType
        {
            get { return (this.ownerTypes == null || this.ownerTypes.Length == 0) ? null : this.ownerTypes[0]; }
        }

        public override bool Matches(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            LocalIdKeyIdentifierClause that = keyIdentifierClause as LocalIdKeyIdentifierClause;

            // PreSharp 
#pragma warning suppress 56506
            return ReferenceEquals(this, that) || (that != null && that.Matches(this.localId, this.OwnerType));
        }

        public bool Matches(string localId, Type ownerType)
        {
            if (string.IsNullOrEmpty(localId))
                return false;
            if (this.localId != localId)
                return false;
            if (this.ownerTypes == null || ownerType == null)
                return true;

            for (int i = 0; i < this.ownerTypes.Length; ++i)
            {
                if (this.ownerTypes[i] == null || this.ownerTypes[i] == ownerType)
                    return true;
            }
            return false;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "LocalIdKeyIdentifierClause(LocalId = '{0}', Owner = '{1}')", this.LocalId, this.OwnerType);
        }
    }
}
