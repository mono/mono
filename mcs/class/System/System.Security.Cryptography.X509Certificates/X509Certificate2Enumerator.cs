//
// System.Security.Cryptography.X509Certificates.X509Certificate2Enumerator class
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2005 Novell Inc. (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#if SECURITY_DEP || MOONLIGHT

using System.Collections;

namespace System.Security.Cryptography.X509Certificates {

	public sealed class X509Certificate2Enumerator : IEnumerator {

		private IEnumerator enumerator;

		internal X509Certificate2Enumerator (X509Certificate2Collection collection) 
		{
			enumerator = ((IEnumerable) collection).GetEnumerator ();
		}

		// properties

		public X509Certificate2 Current {
			get { return (X509Certificate2) enumerator.Current; }
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

		// IEnumerator

		object IEnumerator.Current {
			get { return enumerator.Current; }
		}

		bool IEnumerator.MoveNext ()
		{
			return enumerator.MoveNext ();
		}

		void IEnumerator.Reset ()
		{
			enumerator.Reset ();
		}
	}
}

#endif
