/*++
Copyright (c) 2003 Microsoft Corporation

Module Name:

    SslEnumProperties.cs

Abstract:

    Public enum types used in conjunction SslStream class

Author:
    Alexei Vopilov    Sept 28-2003

Revision History:

--*/
namespace System.Security.Authentication {
using System.Diagnostics.CodeAnalysis;
using System.Net;

    [Flags]
    public enum SslProtocols
    {
        None          =0,
        Ssl2          = SchProtocols.Ssl2,
        Ssl3          = SchProtocols.Ssl3,
        Tls           = SchProtocols.Tls10,
        Tls11         = SchProtocols.Tls11,
        Tls12         = SchProtocols.Tls12,
        Default       = Ssl3 | Tls
    }

    public enum ExchangeAlgorithmType
    {
        None            = 0,

        [SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Rsa", Justification="this would be a breaking change; has previously shipped")]
        RsaSign         = (Alg.ClassSignture| Alg.TypeRSA | Alg.Any),

        [SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Rsa", Justification="this would be a breaking change; has previously shipped")]
        RsaKeyX         = (Alg.ClassKeyXch  | Alg.TypeRSA | Alg.Any),

        [SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Diffie", Justification="this would be a breaking change; has previously shipped")]
        DiffieHellman  = (Alg.ClassKeyXch  | Alg.TypeDH  | Alg.NameDH_Ephem),

    }


    public enum CipherAlgorithmType
    {
        None        = 0,   //No encrytpion

        Rc2         = (Alg.ClassEncrypt | Alg.TypeBlock  | Alg.NameRC2),

        Rc4         = (Alg.ClassEncrypt | Alg.TypeStream | Alg.NameRC4),

        Des         = (Alg.ClassEncrypt | Alg.TypeBlock  | Alg.NameDES),

        TripleDes   = (Alg.ClassEncrypt | Alg.TypeBlock  | Alg.Name3DES),

        Aes         = (Alg.ClassEncrypt | Alg.TypeBlock  | Alg.NameAES),

        Aes128      = (Alg.ClassEncrypt | Alg.TypeBlock  | Alg.NameAES_128),

        Aes192      = (Alg.ClassEncrypt | Alg.TypeBlock  | Alg.NameAES_192),

        Aes256      = (Alg.ClassEncrypt | Alg.TypeBlock  | Alg.NameAES_256),

        Null        = (Alg.ClassEncrypt)  // 0-bit NULL cipher algorithm
    }

    public enum HashAlgorithmType
    {
        None        = 0,

        Md5         = (Alg.ClassHash | Alg.Any  | Alg.NameMD5),

        Sha1        = (Alg.ClassHash | Alg.Any  | Alg.NameSHA)
    }

}



