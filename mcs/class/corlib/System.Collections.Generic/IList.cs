// -*- Mode: csharp; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
//
// System.Collections.Generic.IList
//
// Author:
//    Martin Baulig (martin@ximian.com)
//
// (C) 2003 Novell, Inc.
//

#if GENERICS && NET_1_2
using System;
using System.Runtime.InteropServices;

namespace System.Collections.Generic
{
	[CLSCompliant(false)]
	[ComVisible(false)]
	public interface IList<T> : ICollection<T>
	{
		int Add (T item);

		void Clear ();

		bool Contains (T item);

		int IndexOf (T item);

		void Insert (int index, T item);

		void Remove (T item);

		void RemoveAt (int index);

		bool IsFixedSize {
			get;
		}

		bool IsReadOnly {
			get;
		}

		T this [int switchName] {
			get; set;
		}
	}
}
#endif