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

#if GENERICS
namespace System
{
	[CLSCompliant(false)]
	public struct Nullable<T>
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
	}
}
#endif
