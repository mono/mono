//
// AlgorithmIdentifier.cs - System.Security.Cryptography.Pkcs.AlgorithmIdentifier
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;

namespace System.Security.Cryptography.Pkcs {

	public sealed class AlgorithmIdentifier {

		private Oid _oid;
		private int _length;
		private byte[] _params;

		// constructors

		public AlgorithmIdentifier () : this (new Oid ("1.2.840.113549.3.7", "3des")) {}

		public AlgorithmIdentifier (Oid algorithm) : this (algorithm, 0) {}

		public AlgorithmIdentifier (Oid algorithm, int keyLength)
		{
// FIXME: compatibility with fx 1.2.3400.0
//			if (algorithm == null)
//				throw new ArgumentNullException ("algorithm");

			_oid = algorithm;
			_length = keyLength;
			_params = new byte [0];
		}

		// properties

		public int KeyLength { 
			get { return _length; }
			set { _length = value; }
		}

		public Oid Oid {
			get { return _oid; }
			set { _oid = value; }
		} 

		public byte[] Parameters { 
			get { return _params; }
			set { _params = value; }
		} 
	}
}
