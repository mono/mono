// -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
//
// MCS.System.Collections.IList
//
// Author:
//    Vladimir Vukicevic (vladimir@pobox.com)
//
// (C) 2001 Vladimir Vukicevic
//

using System;
using MCS.System;

namespace MCS.System.Collections {

	public interface IList : ICollection, IEnumerable {
		// properties

		bool IsFixedSize { get; }

		bool IsReadOnly { get; }

		object this[int index] { get; set; }

		// methods

		int Add (object value);

		void Clear ();

		bool Contains (object value);

		int IndexOf (object value);

		void Insert (int index, object value);

		void Remove (object value);

		void RemoveAt (int index);
	}
}
