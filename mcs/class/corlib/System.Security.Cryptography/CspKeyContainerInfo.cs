//
// CspKeyContainerInfo.cs: Information about CSP based key containers
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

using System.Runtime.InteropServices;
#if !DISABLE_SECURITY
using System.Security.AccessControl;
#endif

namespace System.Security.Cryptography {

#if !DISABLE_SECURITY
	[ComVisible (true)]
#endif
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

		#if !DISABLE_SECURITY
		// always null for Mono
		public CryptoKeySecurity CryptoKeySecurity {
			get { return null; }
		}
		#endif

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
