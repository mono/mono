//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel
{
    using System;
    using System.ComponentModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;
    using System.Net;
    using System.Net.Security;

    public sealed class MsmqTransportSecurity
    {
        MsmqAuthenticationMode msmqAuthenticationMode;
        MsmqEncryptionAlgorithm msmqEncryptionAlgorithm;
        MsmqSecureHashAlgorithm msmqHashAlgorithm;
        ProtectionLevel msmqProtectionLevel;

        public MsmqTransportSecurity()
        {
            this.msmqAuthenticationMode = MsmqDefaults.MsmqAuthenticationMode;
            this.msmqEncryptionAlgorithm = MsmqDefaults.MsmqEncryptionAlgorithm;
            this.msmqHashAlgorithm = MsmqDefaults.MsmqSecureHashAlgorithm;
            this.msmqProtectionLevel = MsmqDefaults.MsmqProtectionLevel;
        }

        public MsmqTransportSecurity(MsmqTransportSecurity other)
        {
            if (null == other)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("other");
            this.msmqAuthenticationMode = other.MsmqAuthenticationMode;
            this.msmqEncryptionAlgorithm = other.MsmqEncryptionAlgorithm;
            this.msmqHashAlgorithm = other.MsmqSecureHashAlgorithm;
            this.msmqProtectionLevel = other.MsmqProtectionLevel;
        }

        internal bool Enabled
        {
            get
            {
                return this.msmqAuthenticationMode != MsmqAuthenticationMode.None && this.msmqProtectionLevel != ProtectionLevel.None;
            }
        }

        [DefaultValue(MsmqDefaults.MsmqAuthenticationMode)]
        public MsmqAuthenticationMode MsmqAuthenticationMode
        {
            get { return this.msmqAuthenticationMode; }
            set
            {
                if (!MsmqAuthenticationModeHelper.IsDefined(value))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                this.msmqAuthenticationMode = value;
            }
        }

        [DefaultValue(MsmqDefaults.MsmqEncryptionAlgorithm)]
        public MsmqEncryptionAlgorithm MsmqEncryptionAlgorithm
        {
            get { return this.msmqEncryptionAlgorithm; }
            set
            {
                if (!MsmqEncryptionAlgorithmHelper.IsDefined(value))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                this.msmqEncryptionAlgorithm = value;
            }
        }

        [DefaultValue(MsmqDefaults.MsmqSecureHashAlgorithm)]
        public MsmqSecureHashAlgorithm MsmqSecureHashAlgorithm
        {
            get { return this.msmqHashAlgorithm; }
            set
            {
                if (!MsmqSecureHashAlgorithmHelper.IsDefined(value))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                this.msmqHashAlgorithm = value;
            }
        }

        [DefaultValue(MsmqDefaults.MsmqProtectionLevel)]
        public ProtectionLevel MsmqProtectionLevel
        {
            get { return this.msmqProtectionLevel; }
            set
            {
                if (!ProtectionLevelHelper.IsDefined(value))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                this.msmqProtectionLevel = value;
            }
        }

        internal void Disable()
        {
            this.msmqAuthenticationMode = MsmqAuthenticationMode.None;
            this.msmqProtectionLevel = ProtectionLevel.None;
        }
    }
}
