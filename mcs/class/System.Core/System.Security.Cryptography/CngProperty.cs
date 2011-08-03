//
// System.Security.Cryptography.CngProperty
//
// Copyright (C) 2011 Juho Vähä-Herttua
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

namespace System.Security.Cryptography {

	// note: CNG stands for "Cryptography API: Next Generation"

	public struct CngProperty : IEquatable<CngProperty> {
		private string name;
		private byte[] val;
		private CngPropertyOptions opts;

		public CngProperty(string name, byte[] value, CngPropertyOptions options)
		{
			if (name == null)
				throw new ArgumentNullException("name");

			this.name = name;
			this.val = (value != null) ? (byte[]) value.Clone () : null;
			this.opts = options;
		}

		public string Name {
			get { return name; }
		}

		public CngPropertyOptions Options {
			get { return opts; }
		}

		public byte[] GetValue ()
		{
			if (val == null)
				return null;
			return (byte[]) val.Clone ();
		}

		public bool Equals (CngProperty other)
		{
			if (this.name != other.name || this.opts != other.opts)
				return false;
			if (this.val == null && other.val == null)
				return true;
			if (this.val == null || other.val == null)
				return false;
			if (this.val.Length != other.val.Length)
				return false;

			for (int i=0; i<val.Length; i++) {
				if (this.val[i] != other.val[i])
					return false;
			}
			return true;
		}

		public override bool Equals (object obj)
		{
			if (obj == null)
				return false;
			if (!(obj is CngProperty))
				return false;
			return Equals ((CngProperty) obj);
		}

		public override int GetHashCode ()
		{
			int ret = name.GetHashCode () ^ opts.GetHashCode ();
			if (val == null)
				return ret;

			for (int i=0; i<val.Length; i++) {
				// Handle each 4 bytes of byte array as a little-endian
				// integer and XOR it with the resulting hash code value
				ret ^= val[i] << 8*(i % 4);
			}
			return ret;
		}

		// static

		public static bool operator == (CngProperty left, CngProperty right)
		{
			return left.Equals (right);
		}

		public static bool operator != (CngProperty left, CngProperty right)
		{
			return !left.Equals (right);
		}
	}
}
