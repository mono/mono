//
// System.Collections.Specialized.BitVector32.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System.Collections.Specialized {
	
	public struct BitVector32 : ValueType {
		int value;

		public struct Section {
			short maxval;
		}

		public static Section CreateSection (short maxval)
		{
			Section s = new Section;

		FIXME: Imeplemtn me
		}
		
		public static int CreateMask ()
		{
			return 1;
		}

		public static int CreateMask (int prev)
		{
			return prev << 1;
		}
		
		public BitVector32 (BitVector32 source)
		{
			value = source.value;
		}

		public BitVector32 (int init)
		{
			value = init;
		}

		public bool Equals (object o)
		{
			if (!(o is BitVector32))
				return false;

			return value == ((BitVector32) o).value;
		}
		
		public int Data {
			get {
				return value;
			}
		}
	}
}
