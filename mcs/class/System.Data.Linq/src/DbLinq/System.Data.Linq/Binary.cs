//
// Binary.cs
//
// Author:
//   Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2008 Novell, Inc.
//

//
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
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Linq;

namespace System.Data.Linq
{
    [SerializableAttribute]
#if WCF_ENABLED
	[DataContractAttribute]
#endif
    public sealed class Binary : IEquatable<Binary>
    {
        byte[] data;

        public Binary(byte[] value)
        {
            data = value;
        }

        public static bool operator ==(Binary binary1, Binary binary2)
        {
            bool isNull= binary1 as object==null;
            if (isNull)
                return binary2 as object == null;
            else
                return binary1.Equals(binary2);
        }

        public static bool operator !=(Binary binary1, Binary binary2)
        {
            bool isNull = binary1 as object == null;
            if (isNull)
                return binary2 as object != null;
            else
                return !binary1.data.Equals(binary2.data);
        }

        public static implicit operator Binary(byte[] value)
        {
            return new Binary(value);
        }

        public int Length
        {
            get { return data.Length; }
        }

        public bool Equals(Binary other)
        {
            if (other == null)
                return false;

            if (this.Length != other.Length)
                return false;

            for (int i = 0; i < data.Length; i++)
                if (this.data[i] != other.data[i])
                    return false;

            return true;
        }

        public override bool Equals(object obj)
        {
            Binary other = obj as Binary;
            return other != null && Equals(other);
        }

        public override int GetHashCode()
        {
            return data.GetHashCode();
        }

        public byte[] ToArray()
        {
            if (data != null)
                return data.ToArray();
            else
                return null;
        }
    }
}
