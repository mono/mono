//
// System.Nullable
//
// Martin Baulig (martin@ximian.com)
//
// (C) 2004 Novell, Inc.
//

using System.Reflection;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

#if NET_1_2
namespace System
{
	public struct Nullable<T> : IComparable<Nullable<T>>
	{
		T value;
		bool has_value;

		public Nullable (T value)
		{
			this.value = value;
			this.has_value = true;
		}

		public bool HasValue {
			get { return has_value; }
		}

		public T Value {
			get { return value; }
		}

		public T GetValueOrDefault ()
		{
			if (has_value)
				return value;
			return default (T);
		}

		public T GetValueOrDefault (T def_value)
		{
			if (has_value)
				return value;
			else
				return def_value;
		}

		public int CompareTo (Nullable<T> other)
		{
			if (!has_value && other.has_value)
				return -1;
			else if (has_value && !other.has_value)
				return 1;
			else if (!has_value && !other.has_value)
				return 0;
			else if (value == other.value)
				return 0;
			else
				return 1;
		}
	}
}
#endif
