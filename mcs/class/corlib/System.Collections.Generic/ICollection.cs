// -*- Mode: csharp; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
//
// System.Collections.Generic.ICollection
//
// Author:
//    Martin Baulig (martin@ximian.com)
//
// (C) 2003 Novell, Inc.
//

#if NET_1_2
using System;
using System.Runtime.InteropServices;

namespace System.Collections.Generic
{
	[CLSCompliant(false)]
	[ComVisible(false)]
	public interface ICollection<T> : IEnumerable<T>
	{
		void CopyTo (T[] array, int arrayIndex);

		int Count {
			get;
		}
	}
}
#endif
