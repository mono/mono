//
// System.Runtime.InteropServices.ArrayWithOffset.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Novell, Inc.  http://www.ximian.com
//
using System;

namespace System.Runtime.InteropServices {

	public struct ArrayWithOffset {
		object array;
		int offset;

		public ArrayWithOffset (object array, int offset)
		{
			this.array = array;
			this.offset = offset;
		}

		public override bool Equals (object obj)
		{
			if (obj == null)
				return false;
			if (!(obj is ArrayWithOffset))
				return false;
			ArrayWithOffset other = (ArrayWithOffset) obj;

			return (other.array == array && other.offset == offset);
		}

		public override int GetHashCode ()
		{
			return offset;
		}

		public object GetArray ()
		{
			return array;
		}				

		public object GetOffset ()
		{
			return offset;
		}				
	}
}
