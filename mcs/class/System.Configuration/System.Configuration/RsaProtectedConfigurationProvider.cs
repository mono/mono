//
// System.Configuration.RsaProtectedConfigurationProvider.cs
//
// Authors:
// 	Chris Toshok (toshok@ximian.com)
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Xml;
using System.Collections.Specialized;
using System.Security.Cryptography;

namespace System.Configuration
{
	public sealed class RsaProtectedConfigurationProvider: ProtectedConfigurationProvider
	{
		string cspProviderName;
		string keyContainerName;
		bool useMachineContainer;
		bool useOAEP;

		public RsaProtectedConfigurationProvider ()
		{
		}

		[MonoTODO]
		public override XmlNode Decrypt (XmlNode encrypted_node)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override XmlNode Encrypt (XmlNode node)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void Initialize (string name, NameValueCollection configurationValues)
		{
			string flag;

			base.Initialize (name, configurationValues);

			keyContainerName = configurationValues ["keyContainerName"];
			cspProviderName = configurationValues ["cspProviderName"];

			flag = configurationValues ["useMachineContainer"];
			if (flag != null && flag.ToLower() == "true")
				useMachineContainer = true;

			flag = configurationValues ["useOAEP"];
			if (flag != null && flag.ToLower() == "true")
				useOAEP = true;
		}

		[MonoTODO]
		public void AddKey (int keySize, bool exportable)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DeleteKey ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void ExportKey (string xmlFileName, bool includePrivateParameters)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void ImportKey (string xmlFileName, bool exportable)
		{
			throw new NotImplementedException ();
		}

		public string CspProviderName
		{
			get { return cspProviderName; }
		}

		public string KeyContainerName {
			get { return keyContainerName; }
		}

		[MonoTODO]
		public RSAParameters RsaPublicKey {
			get { throw new NotImplementedException (); }
		}

		public bool UseMachineContainer {
			get { return useMachineContainer; }
		}

		public bool UseOAEP {
			get { return useOAEP; }
		}
	}
}
#endif
