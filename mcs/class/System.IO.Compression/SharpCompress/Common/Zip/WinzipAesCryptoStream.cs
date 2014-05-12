using System;
using System.IO;
using System.Security.Cryptography;

namespace SharpCompress.Common.Zip
{
    internal class WinzipAesCryptoStream : Stream
    {
        private const int BLOCK_SIZE_IN_BYTES = 16;
        private readonly SymmetricAlgorithm cipher;
        private readonly byte[] counter = new byte[BLOCK_SIZE_IN_BYTES];
        private readonly Stream stream;
        private readonly ICryptoTransform transform;
        private int nonce = 1;
        private byte[] counterOut = new byte[BLOCK_SIZE_IN_BYTES];
        private bool isFinalBlock;
        private long totalBytesLeftToRead;
        private bool isDisposed;

        internal WinzipAesCryptoStream(Stream stream, WinzipAesEncryptionData winzipAesEncryptionData, long length)
        {
            this.stream = stream;
            totalBytesLeftToRead = length;

            cipher = CreateCipher(winzipAesEncryptionData);

            var iv = new byte[BLOCK_SIZE_IN_BYTES];
            transform = cipher.CreateEncryptor(winzipAesEncryptionData.KeyBytes, iv);
        }

        private SymmetricAlgorithm CreateCipher(WinzipAesEncryptionData winzipAesEncryptionData)
        {
            RijndaelManaged cipher = new RijndaelManaged();
            cipher.BlockSize = BLOCK_SIZE_IN_BYTES * 8;
            cipher.KeySize = winzipAesEncryptionData.KeyBytes.Length * 8;
            cipher.Mode = CipherMode.ECB;
            cipher.Padding = PaddingMode.None;
            return cipher;
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override long Length
        {
            get { throw new NotImplementedException(); }
        }

        public override long Position
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        protected override void Dispose(bool disposing)
        {
            if (isDisposed)
            {
                return;
            }
            isDisposed = true;
            if (disposing)
            {
                //read out last 10 auth bytes
                var ten = new byte[10];
                stream.Read(ten, 0, 10);
                stream.Dispose();
            }
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (totalBytesLeftToRead == 0)
            {
                return 0;
            }
            int bytesToRead = count;
            if (count > totalBytesLeftToRead)
            {
                bytesToRead = (int)totalBytesLeftToRead;
            }
            int read = stream.Read(buffer, offset, bytesToRead);
            totalBytesLeftToRead -= read;

            ReadTransformBlocks(buffer, offset, read);

            return read;
        }

        private int ReadTransformOneBlock(byte[] buffer, int offset, int last)
        {
            if (isFinalBlock)
            {
                throw new InvalidOperationException();
            }

            int bytesRemaining = last - offset;
            int bytesToRead = (bytesRemaining > BLOCK_SIZE_IN_BYTES)
                                  ? BLOCK_SIZE_IN_BYTES
                                  : bytesRemaining;

            // update the counter
            Array.Copy(BitConverter.GetBytes(nonce++), 0, counter, 0, 4);

            // Determine if this is the final block
            if ((bytesToRead == bytesRemaining) && (totalBytesLeftToRead == 0))
            {
                counterOut = transform.TransformFinalBlock(counter,
                                                           0,
                                                           BLOCK_SIZE_IN_BYTES);
                isFinalBlock = true;
            }
            else
            {
                transform.TransformBlock(counter,
                                         0, // offset
                                         BLOCK_SIZE_IN_BYTES,
                                         counterOut,
                                         0); // offset
            }

            XorInPlace(buffer, offset, bytesToRead);
            return bytesToRead;
        }


        private void XorInPlace(byte[] buffer, int offset, int count)
        {
            for (int i = 0; i < count; i++)
            {
                buffer[offset + i] = (byte)(counterOut[i] ^ buffer[offset + i]);
            }
        }

        private void ReadTransformBlocks(byte[] buffer, int offset, int count)
        {
            int posn = offset;
            int last = count + offset;

            while (posn < buffer.Length && posn < last)
            {
                int n = ReadTransformOneBlock(buffer, posn, last);
                posn += n;
            }
        }


        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}