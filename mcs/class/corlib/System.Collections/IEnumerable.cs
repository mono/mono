// -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
//
// MCS.System.Collections.IEnumerable
//
// Author:
//    Vladimir Vukicevic (vladimir@pobox.com)
//
// (C) 2001 Vladimir Vukicevic
//

using System;
using MCS.System;

namespace MCS.System.Collections {

	public interface IEnumerable {
		IEnumerator GetEnumerator();
	}
}
