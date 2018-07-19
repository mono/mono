// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

//
// X509ChainPolicy.cs
//

namespace System.Security.Cryptography.X509Certificates {
    using System.Globalization;

    public enum X509RevocationMode {
        NoCheck  = 0,
        Online   = 1,
        Offline  = 2
    }

    public enum X509RevocationFlag {
        EndCertificateOnly = 0,
        EntireChain        = 1,
        ExcludeRoot        = 2
    }

    [Flags]
    public enum X509VerificationFlags {
        NoFlag                                      = 0x00000000,
        IgnoreNotTimeValid                          = 0x00000001,
        IgnoreCtlNotTimeValid                       = 0x00000002,
        IgnoreNotTimeNested                         = 0x00000004,
        IgnoreInvalidBasicConstraints               = 0x00000008,
        AllowUnknownCertificateAuthority            = 0x00000010,
        IgnoreWrongUsage                            = 0x00000020,
        IgnoreInvalidName                           = 0x00000040,
        IgnoreInvalidPolicy                         = 0x00000080,
        IgnoreEndRevocationUnknown                  = 0x00000100,
        IgnoreCtlSignerRevocationUnknown            = 0x00000200,
        IgnoreCertificateAuthorityRevocationUnknown = 0x00000400,
        IgnoreRootRevocationUnknown                 = 0x00000800,
        AllFlags                                    = 0x00000FFF
    }

    public sealed class X509ChainPolicy {
        private OidCollection m_applicationPolicy;
        private OidCollection m_certificatePolicy;
        private X509RevocationMode m_revocationMode;
        private X509RevocationFlag m_revocationFlag;
        private DateTime m_verificationTime;
        private TimeSpan m_timeout;
        private X509Certificate2Collection m_extraStore;
        private X509VerificationFlags m_verificationFlags;

        public X509ChainPolicy () {
            Reset();
        }

        public OidCollection ApplicationPolicy {
            get {
                return m_applicationPolicy;
            }
        }

        public OidCollection CertificatePolicy {
            get {
                return m_certificatePolicy;
            }
        }

        public X509RevocationMode RevocationMode {
            get {
                return m_revocationMode;
            }
            set {
                if (value < X509RevocationMode.NoCheck || value > X509RevocationMode.Offline)
                    throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, SR.GetString(SR.Arg_EnumIllegalVal), "value"));
                m_revocationMode = value;
            }
        }

        public X509RevocationFlag RevocationFlag {
            get {
                return m_revocationFlag;
            }
            set {
                if (value < X509RevocationFlag.EndCertificateOnly || value > X509RevocationFlag.ExcludeRoot)
                    throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, SR.GetString(SR.Arg_EnumIllegalVal), "value"));
                m_revocationFlag = value;
            }
        }

        public X509VerificationFlags VerificationFlags {
            get {
                return m_verificationFlags;
            }
            set {
                if (value < X509VerificationFlags.NoFlag || value > X509VerificationFlags.AllFlags)
                    throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, SR.GetString(SR.Arg_EnumIllegalVal), "value"));
                m_verificationFlags = value;
            }
        }

        public DateTime VerificationTime {
            get {
                return m_verificationTime;
            }
            set {
                m_verificationTime = value;
            }
        }

        public TimeSpan UrlRetrievalTimeout {
            get {
                return m_timeout;
            }
            set {
                m_timeout = value;
            }
        }

        public X509Certificate2Collection ExtraStore {
            get {
                return m_extraStore;
            }
        }

        public void Reset () {
            m_applicationPolicy = new OidCollection();
            m_certificatePolicy = new OidCollection();
            m_revocationMode = X509RevocationMode.Online;
            m_revocationFlag = X509RevocationFlag.ExcludeRoot;
            m_verificationFlags = X509VerificationFlags.NoFlag;
            m_verificationTime = DateTime.Now;
            m_timeout = new TimeSpan(0, 0, 0); // default timeout
            m_extraStore = new X509Certificate2Collection();
        }
    }
}
