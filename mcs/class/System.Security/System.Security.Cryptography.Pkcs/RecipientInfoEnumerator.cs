//
// RecipientInfoEnumerator.cs - System.Security.Cryptography.Pkcs.RecipientInfoEnumerator
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

#if NET_1_2

using System;
using System.Collections;

namespace System.Security.Cryptography.Pkcs {

	public class RecipientInfoEnumerator : IEnumerator {

		private IEnumerator enumerator;

		// constructors

		internal RecipientInfoEnumerator (IEnumerable enumerable)
		{
			enumerator = enumerable.GetEnumerator ();
		}

		// properties

		public RecipientInfo Current {
			get { return (RecipientInfo) enumerator.Current; }
		}

		object IEnumerator.Current {
			get { return enumerator.Current; }
		}

		// methods

		public bool MoveNext ()
		{
			return enumerator.MoveNext ();
		}

		public void Reset ()
		{
			enumerator.Reset ();
		}
	}
}

#endif