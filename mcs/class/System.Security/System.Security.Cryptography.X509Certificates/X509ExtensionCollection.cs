//
// X509ExtensionCollection.cs - System.Security.Cryptography.X509ExtensionCollection
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