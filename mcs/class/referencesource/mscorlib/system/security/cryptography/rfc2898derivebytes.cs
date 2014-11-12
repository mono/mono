// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>[....]</OWNER>
// 

//
// Rfc2898DeriveBytes.cs
//

// This implementation follows RFC 2898 recommendations. See http://www.ietf.org/rfc/Rfc2898.txt
// It uses HMACSHA1 as the underlying pseudorandom function.

namespace System.Security.Cryptography {
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Diagnostics.Contracts;

    [System.Runtime.InteropServices.ComVisible(true)]
    public class Rfc2898DeriveBytes : DeriveBytes
    {
        private byte[] m_buffer;
        private byte[] m_salt;
        private HMACSHA1 m_hmacsha1;  // The pseudo-random generator function used in PBKDF2

        private uint m_iterations;
        private uint m_block;
        private int m_startIndex;
        private int m_endIndex;

        private const int BlockSize = 20;

        //
        // public constructors
        //

        public Rfc2898DeriveBytes(string password, int saltSize) : this(password, saltSize, 1000) {}

        public Rfc2898DeriveBytes(string password, int saltSize, int iterations) {
            if (saltSize < 0) 
                throw new ArgumentOutOfRangeException("saltSize", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            Contract.EndContractBlock();

            byte[] salt = new byte[saltSize];
            Utils.StaticRandomNumberGenerator.GetBytes(salt);

            Salt = salt;
            IterationCount = iterations;
            m_hmacsha1 = new HMACSHA1(new UTF8Encoding(false).GetBytes(password));
            Initialize();
        }

        public Rfc2898DeriveBytes(string password, byte[] salt) : this(password, salt, 1000) {}

        public Rfc2898DeriveBytes(string password, byte[] salt, int iterations) : this (new UTF8Encoding(false).GetBytes(password), salt, iterations) {}

        public Rfc2898DeriveBytes(byte[] password, byte[] salt, int iterations) {
            Salt = salt;
            IterationCount = iterations;
            m_hmacsha1 = new HMACSHA1(password);
            Initialize();
        }

        //
        // public properties
        //

        public int IterationCount {
            get { return (int) m_iterations; }
            set {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("ArgumentOutOfRange_NeedPosNum"));
                Contract.EndContractBlock();
                m_iterations = (uint) value;
                Initialize();
            }
        }

        public byte[] Salt {
            get { return (byte[]) m_salt.Clone(); }
            set { 
                if (value == null)
                    throw new ArgumentNullException("value");
                if (value.Length < 8) 
                    throw new ArgumentException(Environment.GetResourceString("Cryptography_PasswordDerivedBytes_FewBytesSalt"));
                Contract.EndContractBlock();
                m_salt = (byte[]) value.Clone(); 
                Initialize();
            }
        }

        //
        // public methods
        //

        public override byte[] GetBytes(int cb) {
            if (cb <= 0)
                throw new ArgumentOutOfRangeException("cb", Environment.GetResourceString("ArgumentOutOfRange_NeedPosNum"));
            Contract.EndContractBlock();
            byte[] password = new byte[cb];

            int offset = 0;
            int size = m_endIndex - m_startIndex;
            if (size > 0) {
                if (cb >= size) {
                    Buffer.InternalBlockCopy(m_buffer, m_startIndex, password, 0, size);
                    m_startIndex = m_endIndex = 0;
                    offset += size;
                } else {
                    Buffer.InternalBlockCopy(m_buffer, m_startIndex, password, 0, cb);
                    m_startIndex += cb;
                    return password;
                }
            }

            Contract.Assert(m_startIndex == 0 && m_endIndex == 0, "Invalid start or end index in the internal buffer." );

            while(offset < cb) {
                byte[] T_block = Func();
                int remainder = cb - offset;
                if(remainder > BlockSize) {
                    Buffer.InternalBlockCopy(T_block, 0, password, offset, BlockSize);
                    offset += BlockSize;
                } else {
                    Buffer.InternalBlockCopy(T_block, 0, password, offset, remainder);
                    offset += remainder;
                    Buffer.InternalBlockCopy(T_block, remainder, m_buffer, m_startIndex, BlockSize - remainder);
                    m_endIndex += (BlockSize - remainder);
                    return password;
                }
            }
            return password;
        }

        public override void Reset() {
            Initialize();
        }

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);

            if (disposing) {
                if (m_hmacsha1 != null) {
                    ((IDisposable)m_hmacsha1).Dispose();
                }

                if (m_buffer != null) {
                    Array.Clear(m_buffer, 0, m_buffer.Length);
                }
                if (m_salt != null) {
                    Array.Clear(m_salt, 0, m_salt.Length);
                }
            }
        }

        private void Initialize() {
            if (m_buffer != null)
                Array.Clear(m_buffer, 0, m_buffer.Length);
            m_buffer = new byte[BlockSize];
            m_block = 1;
            m_startIndex = m_endIndex = 0;
        }

        // This function is defined as follow :
        // Func (S, i) = HMAC(S || i) | HMAC2(S || i) | ... | HMAC(iterations) (S || i) 
        // where i is the block number.
        private byte[] Func () {
            byte[] INT_block = Utils.Int(m_block);

            m_hmacsha1.TransformBlock(m_salt, 0, m_salt.Length, null, 0);
            m_hmacsha1.TransformBlock(INT_block, 0, INT_block.Length, null, 0);
            m_hmacsha1.TransformFinalBlock(EmptyArray<Byte>.Value, 0, 0);
            byte[] temp = m_hmacsha1.HashValue;
            m_hmacsha1.Initialize();

            byte[] ret = temp;
            for (int i = 2; i <= m_iterations; i++) {
                m_hmacsha1.TransformBlock(temp, 0, temp.Length, null, 0);
                m_hmacsha1.TransformFinalBlock(EmptyArray<Byte>.Value, 0, 0);
                temp = m_hmacsha1.HashValue;
                for (int j = 0; j < BlockSize; j++) {
                    ret[j] ^= temp[j];
                }
                m_hmacsha1.Initialize();
            }

            // increment the block count.
            m_block++;
            return ret;
        }
    }
}
