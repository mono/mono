// -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
//
// System.Collections.IDictionaryEnumerator
//
// Author:
//    Vladimir Vukicevic (vladimir@pobox.com)
//
// (C) 2001 Vladimir Vukicevic
//

using System;

namespace System.Collections {

	public interface IDictionaryEnumerator : IEnumerator {
		DictionaryEntry Entry { get; }
		object Key { get; }
		object Value { get; }
	}
}
