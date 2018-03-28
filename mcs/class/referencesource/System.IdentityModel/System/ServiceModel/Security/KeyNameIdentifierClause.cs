//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Security
{
    using System.Globalization;
    using System.IdentityModel;
    using System.IdentityModel.Tokens;
    using System.Runtime.CompilerServices;

    [TypeForwardedFrom("System.ServiceModel, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
    public class KeyNameIdentifierClause : SecurityKeyIdentifierClause
    {
        string keyName;

        public KeyNameIdentifierClause(string keyName)
            : base(null)
        {
            if (keyName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("keyName");
            }
            this.keyName = keyName;
        }

        public string KeyName
        {
            get { return this.keyName; }
        }

        public override bool Matches(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            KeyNameIdentifierClause that = keyIdentifierClause as KeyNameIdentifierClause;

            // PreSharp Bug: Parameter 'that' to this public method must be validated: A null-dereference can occur here.
            #pragma warning suppress 56506
            return ReferenceEquals(this, that) || (that != null && that.Matches(this.keyName));
        }

        public bool Matches(string keyName)
        {
            return this.keyName == keyName;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "KeyNameIdentifierClause(KeyName = '{0}')", this.KeyName);
        }
    }
}
