//
// Pkcs7RecipientCollection.cs - System.Security.Cryptography.Pkcs.Pkcs7RecipientCollection
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

#if NET_1_2

using System;
using System.Collections;
using System.Security.Cryptography.X509Certificates;

namespace System.Security.Cryptography.Pkcs {

	public class Pkcs7RecipientCollection : ICollection, IEnumerable {

		private ArrayList _list;

		// constructors

		public Pkcs7RecipientCollection () 
		{
			_list = new ArrayList ();
		}

		public Pkcs7RecipientCollection (Pkcs7Recipient recipient) : base () 
		{
			_list.Add (recipient);
		}

		public Pkcs7RecipientCollection (SubjectIdentifierType recipientIdentifierType, X509CertificateExCollection certificates) : base () 
		{
			foreach (X509CertificateEx x509 in certificates) {
				Pkcs7Recipient p7r = new Pkcs7Recipient (recipientIdentifierType, x509);
				_list.Add (p7r);
			}
		}

		// properties

		public int Count {
			get { return _list.Count; }
		}

		public bool IsSynchronized {
			get { return _list.IsSynchronized; }
		}

		public Pkcs7Recipient this [int index] {
			get { return (Pkcs7Recipient) _list [index]; }
		}

		public object SyncRoot {
			get { return _list.SyncRoot; }
		}

		// methods

		public int Add (Pkcs7Recipient recipient) 
		{
			return _list.Add (recipient);
		}

		public void CopyTo (Array array, int index) 
		{
			_list.CopyTo (array, index);
		}

		public void CopyTo (Pkcs7Recipient[] array, int index) 
		{
			_list.CopyTo (array, index);
		}

		public Pkcs7RecipientEnumerator GetEnumerator () 
		{
			return new Pkcs7RecipientEnumerator (_list);
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return new Pkcs7RecipientEnumerator (_list);
		}

		public void Remove (Pkcs7Recipient recipient) 
		{
			_list.Remove (recipient);
		}
	}
}

#endif