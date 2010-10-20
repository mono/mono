//
// System.Security.Cryptography.CspParameters.cs
//
// Authors:
//	Thomas Neidhart (tome@sbox.tugraz.at)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
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

using System.Runtime.InteropServices;
using System.Security.AccessControl;

namespace System.Security.Cryptography {

	[ComVisible (true)]
	public sealed class CspParameters {

		private CspProviderFlags _Flags;
	
		public CspParameters () 
			: this (1)
		{
		}
		
		public CspParameters (int dwTypeIn) 
			: this (dwTypeIn, null) 
		{
		}
		
		public CspParameters (int dwTypeIn, string strProviderNameIn)
			: this (dwTypeIn, null, null)
		{
		}
		
		public CspParameters (int dwTypeIn, string strProviderNameIn, string strContainerNameIn)
		{
			ProviderType = dwTypeIn;
			ProviderName = strProviderNameIn;
			KeyContainerName = strContainerNameIn;
			
			// not defined in specs, only tested from M$ impl
			KeyNumber = -1;
		}

		public string KeyContainerName;
		
		public int KeyNumber;
		
		public string ProviderName;
		
		public int ProviderType;
		
		public CspProviderFlags Flags {
			get { return _Flags; }
			set { _Flags = value; }
		}

		private SecureString _password;
		private IntPtr _windowHandle;

		public CspParameters (int providerType, string providerName, string keyContainerName, 
			CryptoKeySecurity cryptoKeySecurity, IntPtr parentWindowHandle)
			: this (providerType, providerName, keyContainerName)
		{
			if (cryptoKeySecurity != null)
				CryptoKeySecurity = cryptoKeySecurity;
			_windowHandle = parentWindowHandle;
		}

		public CspParameters (int providerType, string providerName, string keyContainerName, 
			CryptoKeySecurity cryptoKeySecurity, SecureString keyPassword)
			: this (providerType, providerName, keyContainerName)
		{
			if (cryptoKeySecurity != null)
				CryptoKeySecurity = cryptoKeySecurity;
			_password = keyPassword;
		}

		internal CspParameters(CspParameters parameters)
			: this(parameters.ProviderType, parameters.ProviderName, parameters.KeyContainerName)
		{
			if (parameters.CryptoKeySecurity != null)
				CryptoKeySecurity = parameters.CryptoKeySecurity;

			_Flags = parameters.Flags;
			KeyNumber = parameters.KeyNumber;
			_password = parameters.KeyPassword;
			_windowHandle = parameters.ParentWindowHandle;
		}

		[MonoTODO ("access control isn't implemented")]
		public CryptoKeySecurity CryptoKeySecurity {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public SecureString KeyPassword {
			get { return _password; }
			set { _password = value; }
		}

		public IntPtr ParentWindowHandle {
			get { return _windowHandle; }
			set { _windowHandle = value; }
		}
	}
}
