//
// System.Collections.IEnumerator
//
// Author:
//    Vladimir Vukicevic (vladimir@pobox.com)
//
// (C) 2001 Vladimir Vukicevic
//

using System;

namespace System.Collections {

    public interface IEnumerator {
	object Current { get; }

	bool MoveNext ();

	void Reset ();
    }

}
