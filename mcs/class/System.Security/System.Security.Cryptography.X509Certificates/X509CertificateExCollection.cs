//
// System.Security.Cryptography.X509Certificates.X509Certificate2Collection class
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//	Tim Coleman (tim@timcoleman.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) Tim Coleman, 2004
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

#if NET_2_0

using System.Collections;

namespace System.Security.Cryptography.X509Certificates {

	public sealed class X509Certificate2Collection : X509CertificateCollection {

		// constructors

		public X509Certificate2Collection ()
		{
		}

		public X509Certificate2Collection (X509Certificate2Collection certificates)
		{
			AddRange (certificates);
		}

		public X509Certificate2Collection (X509Certificate2 certificate) 
		{
			Add (certificate);
		}

		public X509Certificate2Collection (X509Certificate2[] certificates) 
		{
			AddRange (certificates);
		}

		// properties

		public new X509Certificate2 this [int index] {
			get {
				if (index < 0)
					throw new ArgumentOutOfRangeException ("negative index");
				if (index >= InnerList.Count)
					throw new ArgumentOutOfRangeException ("index >= Count");
				return (X509Certificate2) InnerList [index];
			}
			set { InnerList [index] = value; }
		}

		// methods

		public int Add (X509Certificate2 certificate)
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");

			return InnerList.Add (certificate);
		}

		// note: transactional
		public void AddRange (X509Certificate2[] certificates) 
		{
			if (certificates == null)
				throw new ArgumentNullException ("certificates");

			for (int i=0; i < certificates.Length; i++)
				InnerList.Add (certificates [i]);
		}

		// note: transactional
		public void AddRange (X509Certificate2Collection certificates) 
		{
			if (certificates == null)
				throw new ArgumentNullException ("certificates");

			InnerList.AddRange (certificates);
		}

		public bool Contains (X509Certificate2 certificate) 
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");

			foreach (X509Certificate2 c in InnerList) {
				if (certificate.Equals (c))
					return true;
			}
			return false;
		}

		public byte[] Export (X509ContentType contentType) 
		{
			return null;
		}

		public byte[] Export (X509ContentType contentType, string password) 
		{
			return null;
		}

		public X509Certificate2Collection Find (X509FindType findType, object findValue, bool validOnly) 
		{
			return null;
		}

		public new X509Certificate2Enumerator GetEnumerator () 
		{
			return null;
		}

		public void Import (byte[] rawData) 
		{
		}

		public void Import (byte[] rawData, string password, X509KeyStorageFlags keyStorageFlags)
		{
		}

		public void Import (string fileName) 
		{
		}

		public void Import (string fileName, string password, X509KeyStorageFlags keyStorageFlags) 
		{
		}

		public void Insert (int index, X509Certificate2 certificate) 
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");
			if (index < 0)
				throw new ArgumentOutOfRangeException ("negative index");
			if (index >= InnerList.Count)
				throw new ArgumentOutOfRangeException ("index >= Count");

			InnerList.Insert (index, certificate);
		}

		public void Remove (X509Certificate2 certificate) 
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");

			for (int i=0; i < InnerList.Count; i++) {
				X509Certificate2 c = (X509Certificate2) InnerList [i];
				if (certificate.Equals (c)) {
					InnerList.RemoveAt (i);
					// only first instance is removed
					return;
				}
			}
		}

		// note: transactional
		public void RemoveRange (X509Certificate2[] certificates)
		{
			if (certificates == null)
				throw new ArgumentNullException ("certificate");
		}

		// note: transactional
		public void RemoveRange (X509Certificate2Collection certificates) 
		{
			if (certificates == null)
				throw new ArgumentNullException ("certificate");
		}

		// note: UI
		public X509Certificate2Collection Select (string title, string message, X509SelectionFlag selectionFlag)
		{
			return null;
		}

		// note: UI
		public X509Certificate2Collection Select (string title, string message, X509SelectionFlag selectionFlag, IntPtr hwndParent) 
		{
			return null;
		}
	}
}

#endif
