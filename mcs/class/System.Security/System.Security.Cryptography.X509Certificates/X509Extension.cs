//
// X509Extension.cs - System.Security.Cryptography.X509Extension
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

#if NET_1_2

using System;

namespace System.Security.Cryptography.X509Certificates {

	// Note: Match the definition of framework version 1.2.3400.0 on http://longhorn.msdn.microsoft.com

	public class X509Extension {

		private bool _critical;
		private AsnEncodedData _extn;
		private Oid _oid;

		// constructors

		protected X509Extension () {}

		// properties

		public bool Critical {
			get { return _critical; }
		}

		public AsnEncodedData EncodedExtension {
			get  { return _extn; }
		}

		public Oid Oid {
			get { return _oid; }
		}

		// methods

		public virtual void CopyFrom (X509Extension extension) 
		{
			if (extension == null)
				throw new ArgumentNullException ("extension");

			_critical = extension._critical;
			_extn = extension._extn;
			_oid = extension._oid;
		}
	}
}

#endif