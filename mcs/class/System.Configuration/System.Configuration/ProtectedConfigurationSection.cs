//
// System.Configuration.ProtectedConfigurationSection.cs
//
// Authors:
// 	Duncan Mak (duncan@ximian.com)
//	Chris Toshok (toshok@ximian.com)
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
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
//

using System.IO;
using System.Xml;

namespace System.Configuration
{
	public sealed class ProtectedConfigurationSection: ConfigurationSection
	{
		static ConfigurationProperty defaultProviderProp;
		static ConfigurationProperty providersProp;
		static ConfigurationPropertyCollection properties;

		ProtectedConfigurationProviderCollection providers;

		static ProtectedConfigurationSection ()
		{
			defaultProviderProp = new ConfigurationProperty ("defaultProvider", typeof (string), "RsaProtectedConfigurationProvider");
			providersProp = new ConfigurationProperty ("providers", typeof (ProviderSettingsCollection), null);

			properties = new ConfigurationPropertyCollection();
			properties.Add (defaultProviderProp);
			properties.Add (providersProp);
		}

		[ConfigurationProperty ("defaultProvider", DefaultValue="RsaProtectedConfigurationProvider")]
		public string DefaultProvider {
			get { return (string)base[defaultProviderProp]; }
			set { base[defaultProviderProp] = value; }
		}

		[ConfigurationProperty ("providers")] 
		public ProviderSettingsCollection Providers {
			get { return (ProviderSettingsCollection)base[providersProp]; }
		}

		protected internal override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

		internal string EncryptSection (string clearXml, ProtectedConfigurationProvider protectionProvider)
		{
			XmlDocument doc = new ConfigurationXmlDocument ();
			doc.LoadXml (clearXml);

			XmlNode encryptedNode = protectionProvider.Encrypt (doc.DocumentElement);

			return encryptedNode.OuterXml;
		}

		internal string DecryptSection (string encryptedXml, ProtectedConfigurationProvider protectionProvider)
		{
			XmlDocument doc = new ConfigurationXmlDocument ();
			doc.InnerXml = encryptedXml;

			XmlNode decryptedNode = protectionProvider.Decrypt (doc.DocumentElement);

			return decryptedNode.OuterXml;
		}

		internal ProtectedConfigurationProviderCollection GetAllProviders ()
		{
			if (providers == null) {
				providers = new ProtectedConfigurationProviderCollection ();

				foreach (ProviderSettings ps in Providers) {
					providers.Add (InstantiateProvider (ps));
				}
			}

			return providers;
		}

		ProtectedConfigurationProvider InstantiateProvider (ProviderSettings ps)
		{
			Type t = Type.GetType (ps.Type, true);
			ProtectedConfigurationProvider prov = Activator.CreateInstance (t) as ProtectedConfigurationProvider;
			if (prov == null)
				throw new Exception ("The type specified does not extend ProtectedConfigurationProvider class.");

			prov.Initialize (ps.Name, ps.Parameters);

			return prov;
		}
	}
}

