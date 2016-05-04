//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.Text;
    using System.Security.Cryptography;

    public class DataProtectionSecurityStateEncoder : SecurityStateEncoder
    {
        byte[] entropy;
        bool useCurrentUserProtectionScope;

        public DataProtectionSecurityStateEncoder()
            : this(true)
        {
            // empty
        }

        public DataProtectionSecurityStateEncoder(bool useCurrentUserProtectionScope)
            : this(useCurrentUserProtectionScope, null)
        { }

        public DataProtectionSecurityStateEncoder(bool useCurrentUserProtectionScope, byte[] entropy)
        {
            this.useCurrentUserProtectionScope = useCurrentUserProtectionScope;
            if (entropy == null)
            {
                this.entropy = null;
            }
            else
            {
                this.entropy = DiagnosticUtility.Utility.AllocateByteArray(entropy.Length);
                Buffer.BlockCopy(entropy, 0, this.entropy, 0, entropy.Length);
            }
        }

        public bool UseCurrentUserProtectionScope
        {
            get
            {
                return this.useCurrentUserProtectionScope;
            }
        }

        public byte[] GetEntropy()
        {
            byte[] result = null;
            if (this.entropy != null)
            {
                result = DiagnosticUtility.Utility.AllocateByteArray(this.entropy.Length);
                Buffer.BlockCopy(this.entropy, 0, result, 0, this.entropy.Length);
            }
            return result;
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append(this.GetType().ToString());
            result.AppendFormat("{0}  UseCurrentUserProtectionScope={1}", Environment.NewLine, this.useCurrentUserProtectionScope);
            result.AppendFormat("{0}  Entropy Length={1}", Environment.NewLine, (this.entropy == null) ? 0 : this.entropy.Length);
            return result.ToString();
        }

        protected internal override byte[] DecodeSecurityState( byte[] data )
        {
            try
            {
                return ProtectedData.Unprotect(data, this.entropy, (this.useCurrentUserProtectionScope) ? DataProtectionScope.CurrentUser : DataProtectionScope.LocalMachine);
            }
            catch (CryptographicException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(SR.GetString(SR.SecurityStateEncoderDecodingFailure), exception));
            }

        }

        protected internal override byte[] EncodeSecurityState( byte[] data )
        {
            try
            {
                return ProtectedData.Protect(data, this.entropy, (this.useCurrentUserProtectionScope) ? DataProtectionScope.CurrentUser : DataProtectionScope.LocalMachine);
            }
            catch (CryptographicException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(SR.GetString(SR.SecurityStateEncoderEncodingFailure), exception));
            }
        }
    }
}
