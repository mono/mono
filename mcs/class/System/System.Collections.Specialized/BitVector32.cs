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
//

using System;
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
				return (((Int16) mask).GetHashCode () << 16) + 
				       ((Int16) offset).GetHashCode ();
			}
			
			public override string ToString ()
			{
				return "Section{0x" + Convert.ToString(mask, 16) + 
				       ", 0x" + Convert.ToString(offset, 16) + "}";
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
		
		public BitVector32 (BitVector32 source)
		{
			bits = source.bits;
		}

		public BitVector32 (int init)
		{
			bits = init;
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
				bits &= (~section.Mask << section.Offset);
				bits |= (value << section.Offset);
			}
		}
		
		public bool this [int mask] {
			get {
				return (bits & mask) == mask;
			}
			
			set { 
				if (value)
					bits |= mask;
				else
					bits &= ~mask;
			}
		}
		
		// Methods
		
		public static int CreateMask ()
		{
			return 1;
		}

		public static int CreateMask (int prev)
		{
			if (prev == 0)
				return 1;
			if (prev == Int32.MinValue) 
				throw new InvalidOperationException ("all bits set");
			return prev << 1;
		}

		public static Section CreateSection (short maxValue)
		{
			return CreateSection (maxValue, new Section (0, 0));
		}
		
		public static Section CreateSection (short maxValue, BitVector32.Section previous)
		{
			if (maxValue < 1)
				throw new ArgumentException ("maxValue");
			
			int bit = HighestSetBit(maxValue) + 1;
			int mask = (1 << bit) - 1;
			int offset = previous.Offset + NumberOfSetBits (previous.Mask);

			if (offset + NumberOfSetBits (mask) > 32) {
				throw new ArgumentException ("Sections cannot exceed 32 bits in total");
			}

			return new Section ((short) mask, (short) offset);
		}
		
		public override bool Equals (object o)
		{
			if (!(o is BitVector32))
				return false;

			return bits == ((BitVector32) o).bits;
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
		private static int NumberOfSetBits (int i) 
		{
			int count = 0;
			for (int bit = 0; bit < 32; bit++) {
				int mask = 1 << bit;
				if ((i & mask) != 0) 
					count++;
			}
			return count;
		}

		private static int HighestSetBit (int i) 
		{
			for (int bit = 31; bit >= 0; bit--) {
				int mask = 1 << bit;
				if ((mask & i) != 0) {
					return bit;
				}
			}

			return -1;
		}

	}
}
