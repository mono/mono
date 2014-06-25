using System;
using System.IO;

namespace SharpCompress.Common.Zip
{
    internal enum CryptoMode
    {
        Encrypt,
        Decrypt
    }


    internal class PkwareTraditionalCryptoStream : Stream
    {
        private readonly PkwareTraditionalEncryptionData encryptor;
        private readonly CryptoMode mode;
        private readonly Stream stream;
        private bool isDisposed;

        public PkwareTraditionalCryptoStream(Stream stream, PkwareTraditionalEncryptionData encryptor, CryptoMode mode)
        {
            this.encryptor = encryptor;
            this.stream = stream;
            this.mode = mode;
        }


        public override bool CanRead
        {
            get { return (mode == CryptoMode.Decrypt); }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return (mode == CryptoMode.Encrypt); }
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

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (mode == CryptoMode.Encrypt)
                throw new NotSupportedException("This stream does not encrypt via Read()");

            if (buffer == null)
                throw new ArgumentNullException("buffer");

            byte[] temp = new byte[count];
            int readBytes = stream.Read(temp, 0, count);
            byte[] decrypted = encryptor.Decrypt(temp, readBytes);
            Buffer.BlockCopy(decrypted, 0, buffer, offset, readBytes);
            return readBytes;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (mode == CryptoMode.Decrypt)
                throw new NotSupportedException("This stream does not Decrypt via Write()");

            if (count == 0)
            {
                return;
            }

            byte[] plaintext = null;
            if (offset != 0)
            {
                plaintext = new byte[count];
                Buffer.BlockCopy(buffer, offset, plaintext, 0, count);
            }
            else
            {
                plaintext = buffer;
            }

            byte[] encrypted = encryptor.Encrypt(plaintext, count);
            stream.Write(encrypted, 0, encrypted.Length);
        }

        public override void Flush()
        {
            //throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        protected override void Dispose(bool disposing)
        {
            if (isDisposed)
            {
                return;
            }
            isDisposed = true;
            base.Dispose(disposing);
            stream.Dispose();
        }
    }
}