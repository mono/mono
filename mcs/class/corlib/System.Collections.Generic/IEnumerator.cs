// -*- Mode: csharp; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
//
// System.Collections.Generic.IEnumerator
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
	public interface IEnumerator<T> : IDisposable
	{
		bool MoveNext ();

		T Current {
			get;
		}
	}
}
#endif
