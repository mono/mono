//
// X509Extensions.cs: Handles X.509 extensions.
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Collections;

using Mono.Security;

namespace Mono.Security.X509 {
	/*
	 * Extensions  ::=  SEQUENCE SIZE (1..MAX) OF Extension
	 * 
	 * Note: 1..MAX -> There shouldn't be 0 Extensions in the ASN1 structure
	 */
#if INSIDE_CORLIB
	internal
#else
	public
#endif
	class X509Extensions : ICollection, IEnumerable {

		private ArrayList extensions;
		private bool readOnly;

		public X509Extensions ()
		{
			extensions = new ArrayList ();
		}

		public X509Extensions (ASN1 asn1) : this ()
		{
			readOnly = true;
			if (asn1 == null)
				return;
			if (asn1.Tag != 0x30)
				throw new Exception ("Invalid extensions format");
			for (int i=0; i < asn1.Count; i++) {
				X509Extension extension = new X509Extension (asn1 [i]);
				extensions.Add (extension);
			}
		}

		// ICollection
		public int Count {
			get { return extensions.Count; }
		}

		// ICollection
		public bool IsSynchronized {
			get { return extensions.IsSynchronized; }
		}

		// ICollection
		public object SyncRoot {
			get { return extensions.SyncRoot; }
		}

		// ICollection
		public void CopyTo (Array array, int index) 
		{
			extensions.CopyTo (array, index);
		}

		// IEnumerable
		public IEnumerator GetEnumerator () 
		{
			return extensions.GetEnumerator ();
		}

		public X509Extension this [int index] {
			get { return (X509Extension) extensions [index]; }
		}

		public X509Extension this [string index] {
			get { 
				for (int i=0; i < extensions.Count; i++) {
					X509Extension extension = (X509Extension) extensions [i];
					if (extension.OID == index)
						return extension;
				}
				return null; 
			}
		}

		public void Add (X509Extension extension) 
		{
			if (readOnly)
				throw new NotSupportedException ("Extensions are read only");
			extensions.Add (extension);
		}

		public byte[] GetBytes () 
		{
			if (extensions.Count < 1)
				return null;
			ASN1 sequence = new ASN1 (0x30);
			for (int i=0; i < extensions.Count; i++) {
				X509Extension x = (X509Extension) extensions [i];
				sequence.Add (x.ASN1);
			}
			return sequence.GetBytes ();
		}
	}
}
