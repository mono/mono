//
// System.ValueType.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.CompilerServices;

namespace System {

	[Serializable]
	public abstract class ValueType {

		// <summary>
		//   ValueType constructor
		// </summary>
		protected ValueType ()
		{
		}

		// <summary>
		//   True if this instance and o represent the same type
		//   and have the same value.
		// </summary>
		public override bool Equals (object o) {
			return InternalEquals (this, o);
		}

		// <summary>
		//   Gets a hashcode for this value type using the
		//   bits in the structure
		// </summary>
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern override int GetHashCode ();

		// <summary>
		//   Stringified representation of this ValueType.
		//   Must be overriden for better results, by default
		//   it just returns the Type name.
		// </summary>
		public override string ToString ()
		{
			return GetType().FullName;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static bool InternalEquals (object o1, object o2);
	}
}
