//
// OidEnumerator.cs - System.Security.Cryptography.OidEnumerator
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

#if NET_1_2

using System;
using System.Collections;

namespace System.Security.Cryptography {

	// Note: Match the definition of framework version 1.2.3400.0 on http://longhorn.msdn.microsoft.com

	public sealed class OidEnumerator : IEnumerator {

		private OidCollection _collection;
		private int _position;

		// note: couldn't reuse the IEnumerator from ArrayList because 
		// it doesn't throw the same exceptions
		internal OidEnumerator (OidCollection collection) 
		{
			_collection = collection;
			_position = -1;
		}

		// properties

		public Oid Current {
			get {
				if (_position < 0)
					throw new ArgumentOutOfRangeException ();
				return (Oid) _collection [_position];
			}
		}

		object IEnumerator.Current {
			get {
				if (_position < 0)
					throw new ArgumentOutOfRangeException ();
				return _collection [_position];
			}
		}

		// methods

		public bool MoveNext () 
		{
			if (++_position < _collection.Count)
				return true;
			else {
				// strangely we must always be able to return the last entry 
				_position = _collection.Count - 1;
				return false;
			}
		}

		public void Reset () 
		{
			_position = -1;
		}
	}
}

#endif