//
// System.Enum.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//
// TODO: Mucho left to implement.
//

using System.Globalization;

namespace System {

	public abstract class Enum : ValueType, IComparable {

		/// <summary>
		///   Compares the enum value with another enum value of the same type.
		/// </summary>
		///
		/// <remarks>
		///   
		int IComparable.CompareTo (object obj)
		{
			if (obj == null)
				return 1;

			if (obj.GetType () != GetType ())
				throw new ArgumentException (
					Locale.GetText ("Enumeration and object must be of the same type"));

			throw new NotImplementedException ();
		}
		
		public static bool IsDefined (Type enum_type, object value)
		{
			throw new NotImplementedException ();
		}

		public override string ToString ()
		{
			throw new NotImplementedException ();
		}

		public string ToString (IFormatProvider provider)
		{
			throw new NotImplementedException ();
		}

		public string ToString (String format)
		{
			throw new NotImplementedException ();
		}

		public string ToString (String format, IFormatProvider provider)
		{
			throw new NotImplementedException ();
		}
	}
}
