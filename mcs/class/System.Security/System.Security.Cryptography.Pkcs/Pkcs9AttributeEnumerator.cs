//
// Pkcs9AttributeEnumerator.cs - System.Security.Cryptography.Pkcs.Pkcs9AttributeEnumerator
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

	public class Pkcs9AttributeEnumerator : IEnumerator {

		private IEnumerator enumerator;

		// constructors

		internal Pkcs9AttributeEnumerator (IEnumerable enumerable) 
		{
			enumerator = enumerable.GetEnumerator ();
		}

		// properties

		public Pkcs9Attribute Current {
			get { return (Pkcs9Attribute) enumerator.Current; }
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