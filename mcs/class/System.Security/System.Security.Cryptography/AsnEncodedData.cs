//
// Oid.cs - System.Security.Cryptography.Oid
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

#if NET_1_2

using System;
using System.Text;

namespace System.Security.Cryptography {

	// Note: Match the definition of framework version 1.2.3400.0 on http://longhorn.msdn.microsoft.com

	public sealed class AsnEncodedData {

		private Oid _oid;
		private byte[] _raw;

		// constructors
	
		public AsnEncodedData (string oid, byte[] rawData)
			: this (new Oid (oid), rawData) {}

		public AsnEncodedData (Oid oid, byte[] rawData)
		{
// FIXME: compatibility with fx 1.2.3400.0
			if (oid == null)
				throw new NullReferenceException ();
//				throw new ArgumentNullException ("oid");
			if (rawData == null)
				throw new NullReferenceException ();
//				throw new ArgumentNullException ("rawData");

			_oid = oid;
			_raw = rawData;
		}

		public AsnEncodedData (AsnEncodedData asnEncodedData)
		{
// FIXME: compatibility with fx 1.2.3400.0
//			if (asnEncodedData == null)
//				throw new ArgumentNullException ("asnEncodedData");

			_oid = new Oid (asnEncodedData._oid);
			_raw = asnEncodedData._raw;
		}

		// properties

		public byte[] RawData { 
			get { return _raw; }
		}

		// methods

		public string Format (bool multiLine) 
		{
			StringBuilder sb = new StringBuilder ();
			for (int i=0; i < _raw.Length; i++) {
				sb.Append (_raw [i].ToString ("x2"));
				if (i != _raw.Length - 1)
					sb.Append (" ");
			}
			return sb.ToString ();
		}
	}
}

#endif