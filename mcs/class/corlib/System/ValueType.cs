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

namespace System
{
	[Serializable]
	public abstract class ValueType
	{
		protected ValueType ()
		{
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern static bool InternalEquals (object o1, object o2, out object[] fields);

		// This is also used by RuntimeHelpers
		internal static bool DefaultEquals (object o1, object o2)
		{
			object[] fields;

			if (o2 == null)
				return false;

			bool res = InternalEquals (o1, o2, out fields);
			if (fields == null)
				return res;

			for (int i = 0; i < fields.Length; i += 2) {
				object meVal = fields [i];
				object youVal = fields [i + 1];
				if (meVal == null) {
					if (youVal == null)
						continue;

					return false;
				}

				if (!meVal.Equals (youVal))
					return false;
			}

			return true;
		}

		// <summary>
		//   True if this instance and o represent the same type
		//   and have the same value.
		// </summary>
		public override bool Equals (object o) {
			return DefaultEquals (this, o);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern static int InternalGetHashCode (object o, out object[] fields);

		// <summary>
		//   Gets a hashcode for this value type using the
		//   bits in the structure
		// </summary>
		public override int GetHashCode ()
		{
			object[] fields;
			int result = InternalGetHashCode (this, out fields);

			if (fields != null)
				for (int i = 0; i < fields.Length; ++i)
					if (fields [i] != null)
						result ^= fields [i].GetHashCode ();
				
			return result;
		}

		// <summary>
		//   Stringified representation of this ValueType.
		//   Must be overriden for better results, by default
		//   it just returns the Type name.
		// </summary>
		public override string ToString ()
		{
			return GetType ().FullName;
		}
	}
}
