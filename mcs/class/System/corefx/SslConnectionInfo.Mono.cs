#if SECURITY_DEP
#if MONO_SECURITY_ALIAS
extern alias MonoSecurity;
#endif

using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Authentication;

#if MONO_SECURITY_ALIAS
using MSI = MonoSecurity::Mono.Security.Interface;
#else
using MSI = Mono.Security.Interface;
#endif

namespace System.Net.Security
{
    internal partial class SslConnectionInfo
    {
        public SslConnectionInfo(MSI.CipherSuiteCode cipherSuite)
        {
            MapCipherSuite((TlsCipherSuite)cipherSuite);
        }

        public static void FillInConnectionInfo(MSI.MonoTlsConnectionInfo info)
        {
                var connectionInfo = new SslConnectionInfo(info.CipherSuiteCode);
                info.CipherAlgorithmType = (MSI.CipherAlgorithmType)connectionInfo.DataCipherAlg;
                info.CipherAlgorithmStrength = connectionInfo.DataKeySize;
                info.HashAlgorithmType = (MSI.HashAlgorithmType)connectionInfo.DataHashAlg;
                info.HashAlgorithmStrength = connectionInfo.DataHashKeySize;
                info.ExchangeAlgorithmType = (MSI.ExchangeAlgorithmType)connectionInfo.KeyExchangeAlg;
        }
    }
}
#endif
