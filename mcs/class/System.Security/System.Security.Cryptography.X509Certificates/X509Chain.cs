//
// X509Chain.cs - System.Security.Cryptography.X509Certificates.X509Chain
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

	public class X509Chain {

		private bool _machineContext;
		private X509ChainElementCollection _elements;
		private X509ChainPolicy _policy;
		private X509ChainStatus[] _status;

		// constructors

		public X509Chain () : this (false) {}

		public X509Chain (bool useMachineContext) 
		{
			_machineContext = useMachineContext;
			_elements = new X509ChainElementCollection ();
			_policy = new X509ChainPolicy ();
		}

		// properties

		public X509ChainElementCollection ChainElements {
			get { return _elements; }
		}

		public X509ChainPolicy ChainPolicy {
			get { return _policy; }
		} 

		public X509ChainStatus[] ChainStatus {
			get { 
				if (_status == null)
					_status = new X509ChainStatus [0]; 
				return _status;
			}
		} 

		// methods

		[MonoTODO]
		public bool Build (X509CertificateEx certificate)
		{
			return false;
		}

		// static methods

		public static X509Chain Create ()
		{
			return new X509Chain ();
		}
	}
}

#endif