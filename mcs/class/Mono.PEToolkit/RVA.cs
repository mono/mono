
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
/*
 * Copyright (c) 2002 Sergey Chaban <serge@wildwestsoftware.com>
 */

using System;
using System.IO;

namespace Mono.PEToolkit {

	/// <summary>
	/// Relative Virtual Address.
	/// </summary>
	public struct RVA {

		public static readonly RVA Null;

		public uint value;

		static RVA()
		{
			Null = new RVA(0);
		}


		public RVA(uint val)
		{
			value = val;
		}


		public uint Value {
			get {
				return value;
			}
			set {
				this.value = value;
			}
		}

		public void Write (BinaryWriter writer)
		{
			writer.Write (value);
		}

		public static implicit operator RVA (uint val)
		{
			return new RVA(val);
		}

		public static implicit operator uint (RVA rva)
		{
			return rva.value;
		}

		public override int GetHashCode()
		{
			return (int) value;
		}

		public override bool Equals(object o)
		{
			bool res = o is RVA;
			if (res) res = (this.value == ((RVA)o).value);
			return res;
		}

		public static bool operator == (RVA rva1, RVA rva2)
		{
			return rva1.Equals(rva2);
		}

		public static bool operator != (RVA rva1, RVA rva2)
		{
			return !rva1.Equals(rva2);
		}

		public static bool operator < (RVA rva1, RVA rva2)
		{
			return (rva1.value < rva2.value);
		}

		public static bool operator > (RVA rva1, RVA rva2) {
			return (rva1.value > rva2.value);
		}

		public static bool operator <= (RVA rva1, RVA rva2)
		{
			return (rva1.value <= rva2.value);
		}

		public static bool operator >= (RVA rva1, RVA rva2)
		{
			return (rva1.value >= rva2.value);
		}

		public static RVA operator + (RVA rva, uint x)
		{
			return new RVA (rva.value + x);
		}

		public static RVA operator - (RVA rva, uint x)
		{
			return new RVA (rva.value - x);
		}


		public override string ToString()
		{
			if (this == Null) return "NULL";
			return ("0x" + value.ToString("X"));
		}

		unsafe public static int Size {
			get {
				return sizeof (uint);
			}
		}

	}

}

