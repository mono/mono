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

