//
// CryptographicAttribute.cs - System.Security.Cryptography.Pkcs.CryptographicAttribute
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

	public class CryptographicAttribute {

		private Oid _oid;
		private ArrayList _list;

		// constructors

		public CryptographicAttribute (Oid oid) 
		{
// FIXME: compatibility with fx 1.2.3400.0
//			if (oid == null)
//				throw new ArgumentNullException ("oid");

			_oid = oid;
			_list = new ArrayList ();
		}

		public CryptographicAttribute (Oid oid,	ArrayList values) : this (oid) 
		{
// FIXME: compatibility with fx 1.2.3400.0
			if (values == null)
				throw new NullReferenceException ();
//				throw new ArgumentNullException ("values");

			_list.AddRange (values);
		}

		public CryptographicAttribute (Oid oid,	object value) : this (oid)
		{
// FIXME: compatibility with fx 1.2.3400.0
//			if (value == null)
//				throw new ArgumentNullException ("value");

			_list.Add (value);
		}

		// properties

		public Oid Oid { 
			get { return _oid; }
		}

		public ArrayList Values { 
			get { return _list; }
		}
	}
}

#endif