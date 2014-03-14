// EncryptionAlgorithm.cs
// ------------------------------------------------------------------
//
// Copyright (c)  2009 Dino Chiesa
// All rights reserved.
//
// This code module is part of DotNetZip, a zipfile class library.
//
// ------------------------------------------------------------------
//
// This code is licensed under the Microsoft Public License. 
// See the file License.txt for the license details.
// More info on: http://dotnetzip.codeplex.com
//
// ------------------------------------------------------------------
//
// last saved (in emacs): 
// Time-stamp: <2009-October-21 17:24:45>
//
// ------------------------------------------------------------------
//
// This module defines the EncryptionAgorithm enum
//
// 
// ------------------------------------------------------------------


namespace Ionic.Zip
{
    /// <summary>
    /// An enum that provides the various encryption algorithms supported by this
    /// library.
    /// </summary>
    ///
    /// <remarks>
    ///
    /// <para>
    ///   <c>PkzipWeak</c> implies the use of Zip 2.0 encryption, which is known to be
    ///   weak and subvertible.
    /// </para>
    ///
    /// <para>
    ///   A note on interoperability: Values of <c>PkzipWeak</c> and <c>None</c> are
    ///   specified in <see
    ///   href="http://www.pkware.com/documents/casestudies/APPNOTE.TXT">PKWARE's zip
    ///   specification</see>, and are considered to be "standard".  Zip archives
    ///   produced using these options will be interoperable with many other zip tools
    ///   and libraries, including Windows Explorer.
    /// </para>
    ///
    /// <para>
    ///   Values of <c>WinZipAes128</c> and <c>WinZipAes256</c> are not part of the Zip
    ///   specification, but rather imply the use of a vendor-specific extension from
    ///   WinZip. If you want to produce interoperable Zip archives, do not use these
    ///   values.  For example, if you produce a zip archive using WinZipAes256, you
    ///   will be able to open it in Windows Explorer on Windows XP and Vista, but you
    ///   will not be able to extract entries; trying this will lead to an "unspecified
    ///   error". For this reason, some people have said that a zip archive that uses
    ///   WinZip's AES encryption is not actually a zip archive at all.  A zip archive
    ///   produced this way will be readable with the WinZip tool (Version 11 and
    ///   beyond).
    /// </para>
    ///
    /// <para>
    ///   There are other third-party tools and libraries, both commercial and
    ///   otherwise, that support WinZip's AES encryption. These will be able to read
    ///   AES-encrypted zip archives produced by DotNetZip, and conversely applications
    ///   that use DotNetZip to read zip archives will be able to read AES-encrypted
    ///   archives produced by those tools or libraries.  Consult the documentation for
    ///   those other tools and libraries to find out if WinZip's AES encryption is
    ///   supported.
    /// </para>
    ///
    /// <para>
    ///   In case you care: According to <see
    ///   href="http://www.winzip.com/aes_info.htm">the WinZip specification</see>, the
    ///   actual AES key used is derived from the <see cref="ZipEntry.Password"/> via an
    ///   algorithm that complies with <see
    ///   href="http://www.ietf.org/rfc/rfc2898.txt">RFC 2898</see>, using an iteration
    ///   count of 1000.  The algorithm is sometimes referred to as PBKDF2, which stands
    ///   for "Password Based Key Derivation Function #2".
    /// </para>
    ///
    /// <para>
    ///   A word about password strength and length: The AES encryption technology is
    ///   very good, but any system is only as secure as the weakest link.  If you want
    ///   to secure your data, be sure to use a password that is hard to guess.  To make
    ///   it harder to guess (increase its "entropy"), you should make it longer.  If
    ///   you use normal characters from an ASCII keyboard, a password of length 20 will
    ///   be strong enough that it will be impossible to guess.  For more information on
    ///   that, I'd encourage you to read <see
    ///   href="http://www.redkestrel.co.uk/Articles/RandomPasswordStrength.html">this
    ///   article.</see>
    /// </para>
    ///
    /// <para>
    ///   The WinZip AES algorithms are not supported with the version of DotNetZip that
    ///   runs on the .NET Compact Framework.  This is because .NET CF lacks the
    ///   HMACSHA1 class that is required for producing the archive.
    /// </para>
    /// </remarks>
    internal enum EncryptionAlgorithm
    {
        /// <summary>
        /// No encryption at all.
        /// </summary>
        None = 0,

        /// <summary>
        /// Traditional or Classic pkzip encryption.
        /// </summary>
        PkzipWeak,

#if AESCRYPTO
        /// <summary>
        /// WinZip AES encryption (128 key bits).
        /// </summary>
        WinZipAes128,

        /// <summary>
        /// WinZip AES encryption (256 key bits).
        /// </summary>
        WinZipAes256,
#endif

        /// <summary>
        /// An encryption algorithm that is not supported by DotNetZip.
        /// </summary>
        Unsupported = 4,


        // others... not implemented (yet?)
    }

}
