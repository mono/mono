//
// CspKeyContainerInfo.cs: Information about CSP based key containers
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

#if NET_1_2

using System;

namespace System.Security.Cryptography {

	public sealed class CspKeyContainerInfo {

		private CspParameters _params;
		internal bool _random;

		// constructors

		public CspKeyContainerInfo (CspParameters parameters) 
		{
			_params = parameters;
			_random = true; // by default we always generate a key
		}

		// properties

		// always true for Mono
		public bool Accessible {
			get { return true; }
		}
		
		// always true for Mono
		public bool Exportable {
			get { return true; }
		}
		
		// always false for Mono
		public bool HardwareDevice {
			get { return false; }
		}
		
		public string KeyContainerName { 
			get { return _params.KeyContainerName; }
		}
		
		public KeyNumber KeyNumber { 
			get { return (KeyNumber)_params.KeyNumber; }
		}
		
		// always false for Mono
		public bool MachineKeyStore {
			get { return false; }
		}
		
		// always false for Mono
		public bool Protected {
			get { return false; }
		}
		
		public string ProviderName {
			get { return _params.ProviderName; }
		}
		
		public int ProviderType { 
			get { return _params.ProviderType; }
		}
		
		// true if generated, false if imported
		public bool RandomlyGenerated {
			get { return _random; }
		}
		
		// always false for Mono
		public bool Removable {
			get { return false; }
		}
		
		public string UniqueKeyContainerName {
			get { return _params.ProviderName + "\\" + _params.KeyContainerName; }
		}
	}
}

#endif