//
// X509ChainElementEnumerator.cs - System.Security.Cryptography.X509Certificates.X509ChainElementEnumerator
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

#if NET_1_2

using System;
using System.Collections;

namespace System.Security.Cryptography.X509Certificates {

	// Note: Match the definition of framework version 1.2.3400.0 on http://longhorn.msdn.microsoft.com

	public sealed class X509ChainElementEnumerator : IEnumerator {

		private IEnumerator enumerator;

		internal X509ChainElementEnumerator (IEnumerable enumerable) 
		{
			enumerator = enumerable.GetEnumerator ();
		}

		// properties

		public X509ChainElement Current {
			get { return (X509ChainElement) enumerator.Current; }
		}

		object IEnumerator.Current {
			get { return enumerator.Current; }
		}

		// methods

		public bool MoveNext ()
		{
			return enumerator.MoveNext ();
		}

		public void Reset() 
		{
			enumerator.Reset ();
		}
	}
}

#endif