// -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
//
// MCS.System.Collections.IDictionaryEnumerator
//
// Author:
//    Vladimir Vukicevic (vladimir@pobox.com)
//
// (C) 2001 Vladimir Vukicevic
//

using System;
using MCS.System;

namespace MCS.System.Collections {

	public interface IDictionaryEnumerator : IEnumerator {
		DictionaryEntry Entry { get; }
		object Key { get; }
		object Value { get; }
	}
}
