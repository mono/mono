//
// System.Int16.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System {
	
	public struct Int16 : ValueType {
		public const short MinValue = -32768;
		public const short MaxValue =  32767;
		
		short value;

		public int CompareTo (object v)
		{
			if (!(value is System.Int16))
				throw new ArgumentException ("Value is not a System.Int16");

			return value - ((short) v);
		}

		public override bool Equals (object o)
		{
			if (!(o is System.Int16))
				return false;

			return ((short) o) == value;
		}

		public override int GetHashCode ()
		{
			return value;
		}

		public TypeCode GetTypeCode ()
		{
			return TypeCode.Int16;
		}

		public static short Parse (string s)
		{
			// TODO: Implement me
			return 0;
		}

		public static short Parse (string s, IFormatProvider)
		{
			// TODO: Implement me
			return 0;
		}

		public static short Parse (string s, NumberStyles s, fp)
		{
			// TODO: Implement me
			return 0;
		}

		public static short Parse (string s, NumberStyles s, IFormatProvider fp)
		{
			// TODO: Implement me
			return 0;
		}
	}
}
