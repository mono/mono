// -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
//
// MCS.System.Collections.IDictionary
//
// Author:
//    Vladimir Vukicevic (vladimir@pobox.com)
//
// (C) 2001 Vladimir Vukicevic
//

using System;
using MCS.System;

namespace MCS.System.Collections {

	public interface IDictionary {
		// properties

		bool IsFixedSize { get; }

		bool IsReadOnly { get; }

		object this[object key] { get; set; }

		ICollection Keys { get; }

		ICollection Values { get; }

		// methods

		void Add (object key, object value);

		void clear ();

		bool Contains (object key);

		IDictionaryEnumerator GetEnumerator ();

		void Remove (object key);
	}
}
