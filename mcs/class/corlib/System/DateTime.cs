//
// System.DateTime.cs
//
// This implementation is just to get String.cs to compile
//

using System.Globalization;
namespace System {

	public struct DateTime : IComparable {
		
		long ticks;

		public DateTime (long ticks)
		{
			this.ticks = ticks;
		}

		public int CompareTo (object v)
		{
			if (!(v is System.DateTime))
				throw new ArgumentException ("Value is not a System.DateTime");

			return (int) (ticks - ((DateTime) v).ticks);
		}

		public override bool Equals (object o)
		{
			if (!(o is System.DateTime))
				return false;

			return ((DateTime) o).ticks == ticks;
		}

		public override int GetHashCode ()
		{
			return (int) ticks;
		}

		public TypeCode GetTypeCode ()
		{
			return TypeCode.DateTime;
		}

		public static DateTime Parse (string s)
		{
			// TODO: Implement me
			return new DateTime (0);
		}

		public static DateTime Parse (string s, IFormatProvider fp)
		{
			// TODO: Implement me
			return new DateTime (0);
		}

		public static DateTime Parse (string s, NumberStyles style, IFormatProvider fp)
		{
			// TODO: Implement me
			return new DateTime (0);
		}

		public override string ToString ()
		{
			// TODO: Implement me

			return "";
		}

		public string ToString (IFormatProvider fp)
		{
			// TODO: Implement me.
			return "";
		}

		public string ToString (string format)
		{
			// TODO: Implement me.
			return "";
		}

		public string ToString (string format, IFormatProvider fp)
		{
			// TODO: Implement me.
			return "";
		}
	}
}
