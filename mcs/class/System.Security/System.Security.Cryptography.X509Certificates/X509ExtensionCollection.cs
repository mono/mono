//
// X509ExtensionCollection.cs - System.Security.Cryptography.X509ExtensionCollection
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

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

#if NET_2_0

using System;
using System.Collections;

namespace System.Security.Cryptography.X509Certificates {

	// Note: Match the definition of framework version 1.2.3400.0 on http://longhorn.msdn.microsoft.com

	public sealed class X509ExtensionCollection : ICollection, IEnumerable {

		// properties

		public int Count {
			get { return 0; }
		}

		public bool IsSynchronized {
			get { return false; }
		}

		public object SyncRoot {
			get { return null; }
		}

		public X509Extension this [int index] {
			get { return null; }
		}

		public X509Extension this [string oid] {
			get { return null; }
		}

		// methods

		public void CopyTo (X509Extension[] array, int index) 
		{
			if (array == null)
				throw new ArgumentNullException ("array");
			if (index < 0)
				throw new ArgumentException ("negative index");
			if (index > array.Length)
				throw new ArgumentOutOfRangeException ("index > array.Length");
		}

		void ICollection.CopyTo (Array array, int index)
		{
		}

		public X509ExtensionEnumerator GetEnumerator () 
		{
			return new X509ExtensionEnumerator (this);
		}

		IEnumerator IEnumerable.GetEnumerator () 
		{
			return new X509ExtensionEnumerator (this);
		}
	}
}

#endif