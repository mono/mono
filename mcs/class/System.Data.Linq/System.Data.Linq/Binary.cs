// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//
// Authors:
//        Antonello Provenzano  <antonello@deveel.com>
//

using System.IO;

namespace System.Data.Linq
{
    public sealed class Binary : IEquatable<Binary>
    {
        #region .ctor
        public Binary(byte[] bytes, int offset, int length)
        {
        }

        public Binary(byte[] bytes)
            : this(bytes, true)
        {
        }

        public Binary(byte[] bytes, bool copy)
        {
            if (bytes == null)
                throw new ArgumentNullException("bytes");

            if (copy)
            {
                this.bytes = new byte[bytes.Length];
                Array.Copy(bytes, this.bytes, bytes.Length);
            }
            else
            {
                this.bytes = bytes;
            }
        }
        #endregion

        #region Fields
        private byte[] bytes;

        private static Binary emptyBin;
        #endregion

        #region Properties
        public int Length
        {
            get { return bytes.Length; }
        }

        public byte this[int index]
        {
            get { return bytes[index]; }
        }

        public static Binary Empty
        {
            get
            {
                if (emptyBin == null)
                    emptyBin = new Binary(new byte[0], 0, 0);
                return emptyBin;
            }
        }
        #endregion

        #region Operators
        public static bool operator ==(Binary binary1, Binary binary2)
        {
            if (binary1 == null && binary2 == null)
                return true;
            if (binary1 == null && binary2 != null)
                return false;
            return binary1.Equals(binary2);
        }

        public static bool operator !=(Binary binary1, Binary binary2)
        {
            return !(binary1 == binary2);
        }

        public static implicit operator Binary(byte[] bytes)
        {
            return new Binary(bytes);
        }
        #endregion

        #region Public Methods

        public bool Equals(Binary other)
        {
            return Equals(other);
        }

        public override bool Equals(object obj)
        {
            //TODO:
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            //TODO:
            return base.GetHashCode();
        }

        public void CopyTo(int position, byte[] buffer, int offset, int length)
        {
            Array.Copy(this.bytes, position, buffer, offset, length);
        }

        public byte[] ToArray()
        {
            return (byte[])bytes.Clone();
        }

        public Stream ToStream()
        {
            return new MemoryStream(bytes, false);
        }

        public override string ToString()
        {
            //TODO:
            return base.ToString();
        }
        #endregion
    }
}