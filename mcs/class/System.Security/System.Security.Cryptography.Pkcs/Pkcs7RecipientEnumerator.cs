//
// Pkcs7RecipientEnumerator.cs - System.Security.Cryptography.Pkcs.Pkcs7RecipientEnumerator
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

	public class Pkcs7RecipientEnumerator : IEnumerator {

		private IEnumerator enumerator;

		// constructors

		internal Pkcs7RecipientEnumerator (IEnumerable enumerable) 
		{
			enumerator = enumerable.GetEnumerator ();
		}

		// properties

		public Pkcs7Recipient Current {
			get { return (Pkcs7Recipient) enumerator.Current; }
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