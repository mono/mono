// -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
//
// System.Collections.IEnumerable
//
// Author:
//    Vladimir Vukicevic (vladimir@pobox.com)
//
// (C) 2001 Vladimir Vukicevic
//

using System;
using System.Runtime.InteropServices;

namespace System.Collections {
	[Guid("496B0ABE-CDEE-11d3-88E8-00902754C43A")]
	public interface IEnumerable {
		[DispId(-4)]
		IEnumerator GetEnumerator();
	}
}
