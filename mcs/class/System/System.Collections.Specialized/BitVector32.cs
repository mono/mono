//
// System.Collections.Specialized.BitVector32.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Lawrence Pit (loz@cable.a2000.nl)
//   Andrew Birkett (adb@tardis.ed.ac.uk)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.Text;

namespace System.Collections.Specialized {
	
	public struct BitVector32 {
		int bits;

		public struct Section {
			private short mask;
			private short offset;
			
			internal Section (short mask, short offset) {
				this.mask = mask;
				this.offset = offset;
			}
			
			public short Mask {
				get { return mask; }
			}
			
			public short Offset {
				get { return offset; }
			}
			public static bool operator == (Section a, Section b)
			{
				return a.mask == b.mask &&
				       a.offset == b.offset;
			}

			public static bool operator != (Section a, Section b)
			{
				return a.mask != b.mask ||
				       a.offset != b.offset;
			}

			public bool Equals (Section obj)
			{
				return this.mask == obj.mask &&
				       this.offset == obj.offset;
			}

			public override bool Equals (object o) 
			{
				if (! (o is Section))
					return false;

				Section section = (Section) o;
				return this.mask == section.mask &&
				       this.offset == section.offset;
			}			

			public override int GetHashCode ()
			{
				return mask << offset; 
			}
			
			public override string ToString ()
			{
				return ToString (this); 
			}

			public static string ToString (Section value)
			{
				StringBuilder b = new StringBuilder ();
				b.Append ("Section{0x");
				b.Append (Convert.ToString(value.Mask,16));
				b.Append (", 0x");
				b.Append (Convert.ToString(value.Offset,16));
				b.Append ("}");

				return b.ToString ();
			}
		}
		
		// Constructors
		
		public BitVector32 (BitVector32 value)
		{
			bits = value.bits;
		}

		public BitVector32 (int data)
		{
			bits = data;
		}
		
		// Properties
		
		public int Data {
			get { return bits; }
		}
		
		public int this [BitVector32.Section section] {
			get {
				return ((bits >> section.Offset) & section.Mask);
			}

			set {
				if (value < 0)
					throw new ArgumentException ("Section can't hold negative values");
				if (value > section.Mask)
					throw new ArgumentException ("Value too large to fit in section");
				bits &= ~(section.Mask << section.Offset);
				bits |= (value << section.Offset);
			}
		}
		
		public bool this [int bit] {
			get {
				return (bits & bit) == bit;
			}
			
			set { 
				if (value)
					bits |= bit;
				else
					bits &= ~bit;
			}
		}
		
		// Methods
		
		public static int CreateMask ()
		{
			return 1;
		}

		public static int CreateMask (int previous)
		{
			if (previous == 0)
				return 1;
			if (previous == Int32.MinValue) 
				throw new InvalidOperationException ("all bits set");
			return previous << 1;
		}

		public static Section CreateSection (short maxValue)
		{
			return CreateSection (maxValue, new Section (0, 0));
		}
		
		public static Section CreateSection (short maxValue, BitVector32.Section previous)
		{
			if (maxValue < 1)
				throw new ArgumentException ("maxValue");
			
			int bit = HighestSetBit(maxValue);
			int mask = (1 << bit) - 1;
			int offset = previous.Offset + HighestSetBit (previous.Mask);

			if (offset + bit > 32) {
				throw new ArgumentException ("Sections cannot exceed 32 bits in total");
			}

			return new Section ((short) mask, (short) offset);
		}
		
		public override bool Equals (object o)
		{
			return (o is BitVector32) && bits == ((BitVector32) o).bits;
		}

		public override int GetHashCode ()
		{
			return bits.GetHashCode ();
		}
		
		public override string ToString () 
		{
			return ToString (this);
		}
		
		public static string ToString (BitVector32 value)
		{
			StringBuilder b = new StringBuilder ();
			b.Append ("BitVector32{");
			long mask = (long) 0x80000000;
			while (mask > 0) {
				b.Append (((value.bits & mask) == 0) ? '0' : '1');
				mask >>= 1;
			}
			b.Append ('}');
			return b.ToString ();
		}

		// Private utilities
		private static int HighestSetBit (int i) 
		{
			int count = 0;
			while(i >> count != 0)
				count++;
			return count;
		}
	}
}
