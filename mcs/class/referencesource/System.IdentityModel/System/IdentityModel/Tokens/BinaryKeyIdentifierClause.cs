//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System.Globalization;
    using HexBinary = System.Runtime.Remoting.Metadata.W3cXsd2001.SoapHexBinary;

    public abstract class BinaryKeyIdentifierClause : SecurityKeyIdentifierClause
    {
        readonly byte[] identificationData;

        protected BinaryKeyIdentifierClause(string clauseType, byte[] identificationData, bool cloneBuffer)
            : this(clauseType, identificationData, cloneBuffer, null, 0)
        {
        }

        protected BinaryKeyIdentifierClause(string clauseType, byte[] identificationData, bool cloneBuffer, byte[] derivationNonce, int derivationLength)
            : base(clauseType, derivationNonce, derivationLength)
        {
            if (identificationData == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("identificationData"));
            }
            if (identificationData.Length == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("identificationData", SR.GetString(SR.LengthMustBeGreaterThanZero)));
            }

            if (cloneBuffer)
            {
                this.identificationData = SecurityUtils.CloneBuffer(identificationData);
            }
            else
            {
                this.identificationData = identificationData;
            }
        }

        public byte[] GetBuffer()
        {
            return SecurityUtils.CloneBuffer(this.identificationData);
        }

        protected byte[] GetRawBuffer()
        {
            return this.identificationData;
        }

        public override bool Matches(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            BinaryKeyIdentifierClause that = keyIdentifierClause as BinaryKeyIdentifierClause;

            // PreSharp 
            #pragma warning suppress 56506
            return ReferenceEquals(this, that) || (that != null && that.Matches(this.identificationData));
        }

        public bool Matches(byte[] data)
        {
            return Matches(data, 0);
        }

        public bool Matches(byte[] data, int offset)
        {
            if (offset < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", SR.GetString(SR.ValueMustBeNonNegative)));
            }
            return SecurityUtils.MatchesBuffer(this.identificationData, 0, data, offset);
        }

        internal string ToBase64String()
        {
            return Convert.ToBase64String(this.identificationData);
        }

        internal string ToHexString()
        {
            return new HexBinary(this.identificationData).ToString();
        }
    }
}
