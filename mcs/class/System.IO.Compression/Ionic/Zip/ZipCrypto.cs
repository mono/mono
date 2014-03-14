// ZipCrypto.cs
// ------------------------------------------------------------------
//
// Copyright (c) 2008, 2009, 2011 Dino Chiesa
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
// Time-stamp: <2011-July-28 06:30:59>
//
// ------------------------------------------------------------------
//
// This module provides the implementation for "traditional" Zip encryption.
//
// Created Tue Apr 15 17:39:56 2008
//
// ------------------------------------------------------------------

using System;

namespace Ionic.Zip
{
    /// <summary>
    ///   This class implements the "traditional" or "classic" PKZip encryption,
    ///   which today is considered to be weak. On the other hand it is
    ///   ubiquitous. This class is intended for use only by the DotNetZip
    ///   library.
    /// </summary>
    ///
    /// <remarks>
    ///   Most uses of the DotNetZip library will not involve direct calls into
    ///   the ZipCrypto class.  Instead, the ZipCrypto class is instantiated and
    ///   used by the ZipEntry() class when encryption or decryption on an entry
    ///   is employed.  If for some reason you really wanted to use a weak
    ///   encryption algorithm in some other application, you might use this
    ///   library.  But you would be much better off using one of the built-in
    ///   strong encryption libraries in the .NET Framework, like the AES
    ///   algorithm or SHA.
    /// </remarks>
    internal class ZipCrypto
    {
        /// <summary>
        ///   The default constructor for ZipCrypto.
        /// </summary>
        ///
        /// <remarks>
        ///   This class is intended for internal use by the library only. It's
        ///   probably not useful to you. Seriously.  Stop reading this
        ///   documentation.  It's a waste of your time.  Go do something else.
        ///   Check the football scores. Go get an ice cream with a friend.
        ///   Seriously.
        /// </remarks>
        ///
        private ZipCrypto() { }

        internal static ZipCrypto ForWrite(string password)
        {
            ZipCrypto z = new ZipCrypto();
            if (password == null)
                throw new BadPasswordException("This entry requires a password.");
            z.InitCipher(password);
            return z;
        }


        internal static ZipCrypto ForRead(string password, ZipEntry e)
        {
            System.IO.Stream s = e._archiveStream;
            e._WeakEncryptionHeader = new byte[12];
            byte[] eh = e._WeakEncryptionHeader;
            ZipCrypto z = new ZipCrypto();

            if (password == null)
                throw new BadPasswordException("This entry requires a password.");

            z.InitCipher(password);

            ZipEntry.ReadWeakEncryptionHeader(s, eh);

            // Decrypt the header.  This has a side effect of "further initializing the
            // encryption keys" in the traditional zip encryption.
            byte[] DecryptedHeader = z.DecryptMessage(eh, eh.Length);

            // CRC check
            // According to the pkzip spec, the final byte in the decrypted header
            // is the highest-order byte in the CRC. We check it here.
            if (DecryptedHeader[11] != (byte)((e._Crc32 >> 24) & 0xff))
            {
                // In the case that bit 3 of the general purpose bit flag is set to
                // indicate the presence of an 'Extended File Header' or a 'data
                // descriptor' (signature 0x08074b50), the last byte of the decrypted
                // header is sometimes compared with the high-order byte of the
                // lastmodified time, rather than the high-order byte of the CRC, to
                // verify the password.
                //
                // This is not documented in the PKWare Appnote.txt.  It was
                // discovered this by analysis of the Crypt.c source file in the
                // InfoZip library http://www.info-zip.org/pub/infozip/
                //
                // The reason for this is that the CRC for a file cannot be known
                // until the entire contents of the file have been streamed. This
                // means a tool would have to read the file content TWICE in its
                // entirety in order to perform PKZIP encryption - once to compute
                // the CRC, and again to actually encrypt.
                //
                // This is so important for performance that using the timeblob as
                // the verification should be the standard practice for DotNetZip
                // when using PKZIP encryption. This implies that bit 3 must be
                // set. The downside is that some tools still cannot cope with ZIP
                // files that use bit 3.  Therefore, DotNetZip DOES NOT force bit 3
                // when PKZIP encryption is in use, and instead, reads the stream
                // twice.
                //

                if ((e._BitField & 0x0008) != 0x0008)
                {
                    throw new BadPasswordException("The password did not match.");
                }
                else if (DecryptedHeader[11] != (byte)((e._TimeBlob >> 8) & 0xff))
                {
                    throw new BadPasswordException("The password did not match.");
                }

                // We have a good password.
            }
            else
            {
                // A-OK
            }
            return z;
        }




        /// <summary>
        /// From AppNote.txt:
        /// unsigned char decrypt_byte()
        ///     local unsigned short temp
        ///     temp :=- Key(2) | 2
        ///     decrypt_byte := (temp * (temp ^ 1)) bitshift-right 8
        /// end decrypt_byte
        /// </summary>
        private byte MagicByte
        {
            get
            {
                UInt16 t = (UInt16)((UInt16)(_Keys[2] & 0xFFFF) | 2);
                return (byte)((t * (t ^ 1)) >> 8);
            }
        }

        // Decrypting:
        // From AppNote.txt:
        // loop for i from 0 to 11
        //     C := buffer(i) ^ decrypt_byte()
        //     update_keys(C)
        //     buffer(i) := C
        // end loop


        /// <summary>
        ///   Call this method on a cipher text to render the plaintext. You must
        ///   first initialize the cipher with a call to InitCipher.
        /// </summary>
        ///
        /// <example>
        ///   <code>
        ///     var cipher = new ZipCrypto();
        ///     cipher.InitCipher(Password);
        ///     // Decrypt the header.  This has a side effect of "further initializing the
        ///     // encryption keys" in the traditional zip encryption.
        ///     byte[] DecryptedMessage = cipher.DecryptMessage(EncryptedMessage);
        ///   </code>
        /// </example>
        ///
        /// <param name="cipherText">The encrypted buffer.</param>
        /// <param name="length">
        ///   The number of bytes to encrypt.
        ///   Should be less than or equal to CipherText.Length.
        /// </param>
        ///
        /// <returns>The plaintext.</returns>
        public byte[] DecryptMessage(byte[] cipherText, int length)
        {
            if (cipherText == null)
                throw new ArgumentNullException("cipherText");

            if (length > cipherText.Length)
                throw new ArgumentOutOfRangeException("length",
                                                      "Bad length during Decryption: the length parameter must be smaller than or equal to the size of the destination array.");

            byte[] plainText = new byte[length];
            for (int i = 0; i < length; i++)
            {
                byte C = (byte)(cipherText[i] ^ MagicByte);
                UpdateKeys(C);
                plainText[i] = C;
            }
            return plainText;
        }

        /// <summary>
        ///   This is the converse of DecryptMessage.  It encrypts the plaintext
        ///   and produces a ciphertext.
        /// </summary>
        ///
        /// <param name="plainText">The plain text buffer.</param>
        ///
        /// <param name="length">
        ///   The number of bytes to encrypt.
        ///   Should be less than or equal to plainText.Length.
        /// </param>
        ///
        /// <returns>The ciphertext.</returns>
        public byte[] EncryptMessage(byte[] plainText, int length)
        {
            if (plainText == null)
                throw new ArgumentNullException("plaintext");

            if (length > plainText.Length)
                throw new ArgumentOutOfRangeException("length",
                                                      "Bad length during Encryption: The length parameter must be smaller than or equal to the size of the destination array.");

            byte[] cipherText = new byte[length];
            for (int i = 0; i < length; i++)
            {
                byte C = plainText[i];
                cipherText[i] = (byte)(plainText[i] ^ MagicByte);
                UpdateKeys(C);
            }
            return cipherText;
        }


        /// <summary>
        ///   This initializes the cipher with the given password.
        ///   See AppNote.txt for details.
        /// </summary>
        ///
        /// <param name="passphrase">
        ///   The passphrase for encrypting or decrypting with this cipher.
        /// </param>
        ///
        /// <remarks>
        /// <code>
        /// Step 1 - Initializing the encryption keys
        /// -----------------------------------------
        /// Start with these keys:
        /// Key(0) := 305419896 (0x12345678)
        /// Key(1) := 591751049 (0x23456789)
        /// Key(2) := 878082192 (0x34567890)
        ///
        /// Then, initialize the keys with a password:
        ///
        /// loop for i from 0 to length(password)-1
        ///     update_keys(password(i))
        /// end loop
        ///
        /// Where update_keys() is defined as:
        ///
        /// update_keys(char):
        ///   Key(0) := crc32(key(0),char)
        ///   Key(1) := Key(1) + (Key(0) bitwiseAND 000000ffH)
        ///   Key(1) := Key(1) * 134775813 + 1
        ///   Key(2) := crc32(key(2),key(1) rightshift 24)
        /// end update_keys
        ///
        /// Where crc32(old_crc,char) is a routine that given a CRC value and a
        /// character, returns an updated CRC value after applying the CRC-32
        /// algorithm described elsewhere in this document.
        ///
        /// </code>
        ///
        /// <para>
        ///   After the keys are initialized, then you can use the cipher to
        ///   encrypt the plaintext.
        /// </para>
        ///
        /// <para>
        ///   Essentially we encrypt the password with the keys, then discard the
        ///   ciphertext for the password. This initializes the keys for later use.
        /// </para>
        ///
        /// </remarks>
        public void InitCipher(string passphrase)
        {
            byte[] p = SharedUtilities.StringToByteArray(passphrase);
            for (int i = 0; i < passphrase.Length; i++)
                UpdateKeys(p[i]);
        }


        private void UpdateKeys(byte byteValue)
        {
            _Keys[0] = (UInt32)crc32.ComputeCrc32((int)_Keys[0], byteValue);
            _Keys[1] = _Keys[1] + (byte)_Keys[0];
            _Keys[1] = _Keys[1] * 0x08088405 + 1;
            _Keys[2] = (UInt32)crc32.ComputeCrc32((int)_Keys[2], (byte)(_Keys[1] >> 24));
        }

        ///// <summary>
        ///// The byte array representing the seed keys used.
        ///// Get this after calling InitCipher.  The 12 bytes represents
        ///// what the zip spec calls the "EncryptionHeader".
        ///// </summary>
        //public byte[] KeyHeader
        //{
        //    get
        //    {
        //        byte[] result = new byte[12];
        //        result[0] = (byte)(_Keys[0] & 0xff);
        //        result[1] = (byte)((_Keys[0] >> 8) & 0xff);
        //        result[2] = (byte)((_Keys[0] >> 16) & 0xff);
        //        result[3] = (byte)((_Keys[0] >> 24) & 0xff);
        //        result[4] = (byte)(_Keys[1] & 0xff);
        //        result[5] = (byte)((_Keys[1] >> 8) & 0xff);
        //        result[6] = (byte)((_Keys[1] >> 16) & 0xff);
        //        result[7] = (byte)((_Keys[1] >> 24) & 0xff);
        //        result[8] = (byte)(_Keys[2] & 0xff);
        //        result[9] = (byte)((_Keys[2] >> 8) & 0xff);
        //        result[10] = (byte)((_Keys[2] >> 16) & 0xff);
        //        result[11] = (byte)((_Keys[2] >> 24) & 0xff);
        //        return result;
        //    }
        //}

        // private fields for the crypto stuff:
        private UInt32[] _Keys = { 0x12345678, 0x23456789, 0x34567890 };
        private Ionic.Crc.CRC32 crc32 = new Ionic.Crc.CRC32();

    }

    internal enum CryptoMode
    {
        Encrypt,
        Decrypt
    }

    /// <summary>
    ///   A Stream for reading and concurrently decrypting data from a zip file,
    ///   or for writing and concurrently encrypting data to a zip file.
    /// </summary>
    internal class ZipCipherStream : System.IO.Stream
    {
        private ZipCrypto _cipher;
        private System.IO.Stream _s;
        private CryptoMode _mode;

        /// <summary>  The constructor. </summary>
        /// <param name="s">The underlying stream</param>
        /// <param name="mode">To either encrypt or decrypt.</param>
        /// <param name="cipher">The pre-initialized ZipCrypto object.</param>
        public ZipCipherStream(System.IO.Stream s, ZipCrypto cipher, CryptoMode mode)
            : base()
        {
            _cipher = cipher;
            _s = s;
            _mode = mode;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_mode == CryptoMode.Encrypt)
                throw new NotSupportedException("This stream does not encrypt via Read()");

            if (buffer == null)
                throw new ArgumentNullException("buffer");

            byte[] db = new byte[count];
            int n = _s.Read(db, 0, count);
            byte[] decrypted = _cipher.DecryptMessage(db, n);
            for (int i = 0; i < n; i++)
            {
                buffer[offset + i] = decrypted[i];
            }
            return n;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (_mode == CryptoMode.Decrypt)
                throw new NotSupportedException("This stream does not Decrypt via Write()");

            if (buffer == null)
                throw new ArgumentNullException("buffer");

            // workitem 7696
            if (count == 0) return;

            byte[] plaintext = null;
            if (offset != 0)
            {
                plaintext = new byte[count];
                for (int i = 0; i < count; i++)
                {
                    plaintext[i] = buffer[offset + i];
                }
            }
            else plaintext = buffer;

            byte[] encrypted = _cipher.EncryptMessage(plaintext, count);
            _s.Write(encrypted, 0, encrypted.Length);
        }


        public override bool CanRead
        {
            get { return (_mode == CryptoMode.Decrypt); }
        }
        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return (_mode == CryptoMode.Encrypt); }
        }

        public override void Flush()
        {
            //throw new NotSupportedException();
        }

        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        public override long Position
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }
        public override long Seek(long offset, System.IO.SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }
    }
}
