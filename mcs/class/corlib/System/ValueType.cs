//
// System.ValueType.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// (C) 2003 Novell, Inc.  http://www.novell.com
//

using System.Reflection;
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
		public override bool Equals (object o)
		{
			if (o == null)
				return false;

			bool result = InternalEquals (this, o);
			if (result)
				return result;

			Type me = GetType ();
			Type you = o.GetType ();
			if (me != you)
				return false;

			BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
					     BindingFlags.Instance;

			FieldInfo [] meFlds = me.GetFields (flags);
			FieldInfo [] youFlds = you.GetFields (flags);
			for (int i = meFlds.Length - 1; i >= 0; i--) {
				object meVal = meFlds [i].GetValue (this);
				object youVal = youFlds [i].GetValue (o);
				if (meVal == null) {
					if (youVal == null)
						continue;

					return false;
				}

				result = meVal.Equals (youVal);
				if (!result)
					return false;
			}

			return true;
		}

		// <summary>
		//   Gets a hashcode for this value type using the
		//   bits in the structure
		// </summary>
		public override int GetHashCode ()
		{
			Type me = GetType ();
			BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
					     BindingFlags.Instance;

			FieldInfo [] meFlds = me.GetFields (flags);
			if (meFlds.Length == 0)
				return base.GetHashCode ();

			int result = 0;
			foreach (object o in meFlds) {
				if (o != null)
					result ^= o.GetHashCode ();
			}

			return result;
		}

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
		internal extern static bool InternalEquals (object o1, object o2);
	}
}
