using System;
using System.Text;
using SharpCompress.Common.Zip.Headers;
using SharpCompress.Compressor.Deflate;

namespace SharpCompress.Common.Zip
{
    internal class PkwareTraditionalEncryptionData
    {
        private static readonly CRC32 crc32 = new CRC32();
        private readonly UInt32[] _Keys = {0x12345678, 0x23456789, 0x34567890};

        private PkwareTraditionalEncryptionData(string password)
        {
            Initialize(password);
        }

        private byte MagicByte
        {
            get
            {
                ushort t = (ushort) ((ushort) (_Keys[2] & 0xFFFF) | 2);
                return (byte) ((t*(t ^ 1)) >> 8);
            }
        }

        public static PkwareTraditionalEncryptionData ForRead(string password, ZipFileEntry header,
                                                              byte[] encryptionHeader)
        {
            var encryptor = new PkwareTraditionalEncryptionData(password);
            byte[] plainTextHeader = encryptor.Decrypt(encryptionHeader, encryptionHeader.Length);
            if (plainTextHeader[11] != (byte) ((header.Crc >> 24) & 0xff))
            {
                if (!FlagUtility.HasFlag(header.Flags, HeaderFlags.UsePostDataDescriptor))
                {
                    throw new CryptographicException("The password did not match.");
                }
                if (plainTextHeader[11] != (byte) ((header.LastModifiedTime >> 8) & 0xff))
                {
                    throw new CryptographicException("The password did not match.");
                }
            }
            return encryptor;
        }


        public byte[] Decrypt(byte[] cipherText, int length)
        {
            if (length > cipherText.Length)
                throw new ArgumentOutOfRangeException("length",
                                                      "Bad length during Decryption: the length parameter must be smaller than or equal to the size of the destination array.");

            var plainText = new byte[length];
            for (int i = 0; i < length; i++)
            {
                var C = (byte) (cipherText[i] ^ MagicByte);
                UpdateKeys(C);
                plainText[i] = C;
            }
            return plainText;
        }

        public byte[] Encrypt(byte[] plainText, int length)
        {
            if (plainText == null)
                throw new ArgumentNullException("plaintext");

            if (length > plainText.Length)
                throw new ArgumentOutOfRangeException("length",
                                                      "Bad length during Encryption: The length parameter must be smaller than or equal to the size of the destination array.");

            var cipherText = new byte[length];
            for (int i = 0; i < length; i++)
            {
                byte C = plainText[i];
                cipherText[i] = (byte) (plainText[i] ^ MagicByte);
                UpdateKeys(C);
            }
            return cipherText;
        }

        private void Initialize(string password)
        {
            byte[] p = StringToByteArray(password);
            for (int i = 0; i < password.Length; i++)
                UpdateKeys(p[i]);
        }

        internal static byte[] StringToByteArray(string value, Encoding encoding)
        {
            byte[] a = encoding.GetBytes(value);
            return a;
        }

        internal static byte[] StringToByteArray(string value)
        {
            return StringToByteArray(value, ArchiveEncoding.Password);
        }

        private void UpdateKeys(byte byteValue)
        {
            _Keys[0] = (UInt32) crc32.ComputeCrc32((int) _Keys[0], byteValue);
            _Keys[1] = _Keys[1] + (byte) _Keys[0];
            _Keys[1] = _Keys[1]*0x08088405 + 1;
            _Keys[2] = (UInt32) crc32.ComputeCrc32((int) _Keys[2], (byte) (_Keys[1] >> 24));
        }
    }
}