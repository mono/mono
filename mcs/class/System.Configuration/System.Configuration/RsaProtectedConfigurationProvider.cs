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

using System.Xml;
using System.IO;
using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;

namespace System.Configuration
{
	public sealed class RsaProtectedConfigurationProvider: ProtectedConfigurationProvider
	{
		string cspProviderName;
		string keyContainerName;
		bool useMachineContainer;
		bool useOAEP;

		RSACryptoServiceProvider rsa;

		RSACryptoServiceProvider GetProvider ()
		{
			if (rsa == null) {
				CspParameters c = new CspParameters ();
				c.ProviderName = cspProviderName;
				c.KeyContainerName = keyContainerName;
				if (useMachineContainer)
					c.Flags |= CspProviderFlags.UseMachineKeyStore;

				rsa = new RSACryptoServiceProvider (c);
			}

			return rsa;
		}

		public RsaProtectedConfigurationProvider ()
		{
		}

		[MonoTODO]
		public override XmlNode Decrypt (XmlNode encryptedNode)
		{
			XmlDocument doc = new ConfigurationXmlDocument ();
			
			doc.Load (new StringReader (encryptedNode.OuterXml));

			EncryptedXml ex = new EncryptedXml (doc);

			ex.AddKeyNameMapping ("Rsa Key", GetProvider ());

			ex.DecryptDocument ();
			
			return doc.DocumentElement;
		}

		[MonoTODO]
		public override XmlNode Encrypt (XmlNode node)
		{
			XmlDocument doc = new ConfigurationXmlDocument ();
			
			doc.Load (new StringReader (node.OuterXml));

			EncryptedXml ex = new EncryptedXml (doc);

			ex.AddKeyNameMapping ("Rsa Key", GetProvider ());

			EncryptedData d = ex.Encrypt (doc.DocumentElement, "Rsa Key");

			return d.GetXml();
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
			RSACryptoServiceProvider prov = GetProvider ();
			string xml = prov.ToXmlString (includePrivateParameters);

			FileStream stream = new FileStream (xmlFileName, FileMode.OpenOrCreate, FileAccess.Write);
			StreamWriter writer = new StreamWriter (stream);

			writer.Write (xml);
			writer.Close ();
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

		public RSAParameters RsaPublicKey {
			get {
				RSACryptoServiceProvider prov = GetProvider ();
				return prov.ExportParameters (false);
			}
		}

		public bool UseMachineContainer {
			get { return useMachineContainer; }
		}

		public bool UseOAEP {
			get { return useOAEP; }
		}
	}
}

