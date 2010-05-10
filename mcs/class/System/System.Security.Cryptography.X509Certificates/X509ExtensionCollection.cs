//
// System.Security.Cryptography.X509Certificates.X509ExtensionCollection
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//	Tim Coleman (tim@timcoleman.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) Tim Coleman, 2004
// Copyright (C) 2004-2006 Novell Inc. (http://www.novell.com)
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
using Mono.Security;
using MX = Mono.Security.X509;

namespace System.Security.Cryptography.X509Certificates {

	public sealed class X509ExtensionCollection : ICollection, IEnumerable {

		private ArrayList _list;

		// constructors

		public X509ExtensionCollection ()
		{
			_list = new ArrayList ();
		}

		internal X509ExtensionCollection (MX.X509Certificate cert)
		{
			_list = new ArrayList (cert.Extensions.Count);
			if (cert.Extensions.Count == 0)
				return;

#if !MOONLIGHT
			object[] parameters = new object [2];
#endif
			foreach (MX.X509Extension ext in cert.Extensions) {
				bool critical = ext.Critical;
				string oid = ext.Oid;
				byte[] raw_data = null;
				// extension data is embedded in an octet stream (4)
				ASN1 value = ext.Value;
				if ((value.Tag == 0x04) && (value.Count > 0))
					raw_data = value [0].GetBytes ();

				X509Extension newt = null;
#if MOONLIGHT
				// non-extensible
				switch (oid) {
				case "2.5.29.14":
					newt = new X509SubjectKeyIdentifierExtension (new AsnEncodedData (oid, raw_data), critical);
					break;
				case "2.5.29.15":
					newt = new X509KeyUsageExtension (new AsnEncodedData (oid, raw_data), critical);
					break;
				case "2.5.29.19":
					newt = new X509BasicConstraintsExtension (new AsnEncodedData (oid, raw_data), critical);
					break;
				case "2.5.29.37":
					newt = new X509EnhancedKeyUsageExtension (new AsnEncodedData (oid, raw_data), critical);
					break;
				}
#else
				parameters [0] = new AsnEncodedData (oid, raw_data);
				parameters [1] = critical;
				newt = (X509Extension) CryptoConfig.CreateFromName (oid, parameters);
#endif
				if (newt == null) {
					// not registred in CryptoConfig, using default
					newt = new X509Extension (oid, raw_data, critical);
				}
				_list.Add (newt);
			}
		}

		// properties

		public int Count {
			get { return _list.Count; }
		}

		public bool IsSynchronized {
			get { return _list.IsSynchronized; }
		}

		public object SyncRoot {
			get { return this; }
		}

		public X509Extension this [int index] {
			get {
				if (index < 0)
					throw new InvalidOperationException ("index");
				return (X509Extension) _list [index];
			}
		}

		public X509Extension this [string oid] {
			get {
				if (oid == null)
					throw new ArgumentNullException ("oid");
				if ((_list.Count == 0) || (oid.Length == 0))
					return null;

				foreach (X509Extension extension in _list) {
					if (extension.Oid.Value.Equals (oid))
						return extension;
				}
				return null;
			}
		}

		// methods

		public int Add (X509Extension extension) 
		{
			if (extension == null)
				throw new ArgumentNullException ("extension");

			return _list.Add (extension);
		}

		public void CopyTo (X509Extension[] array, int index) 
		{
			if (array == null)
				throw new ArgumentNullException ("array");
			if (index < 0)
				throw new ArgumentOutOfRangeException ("negative index");
			if (index >= array.Length)
				throw new ArgumentOutOfRangeException ("index >= array.Length");

			_list.CopyTo (array, index);
		}

		void ICollection.CopyTo (Array array, int index)
		{
			if (array == null)
				throw new ArgumentNullException ("array");
			if (index < 0)
				throw new ArgumentOutOfRangeException ("negative index");
			if (index >= array.Length)
				throw new ArgumentOutOfRangeException ("index >= array.Length");

			_list.CopyTo (array, index);
		}

		public X509ExtensionEnumerator GetEnumerator () 
		{
			return new X509ExtensionEnumerator (_list);
		}

		IEnumerator IEnumerable.GetEnumerator () 
		{
			return new X509ExtensionEnumerator (_list);
		}
	}
}

#endif
