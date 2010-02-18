using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace Mono.Configuration.Crypto
{
	public class ConfigSection
	{
		static readonly char[] sectionSplitChars = { '/' };
		
		ProtectedConfigurationProvider GetProvider (string providerName, string containerName, bool useMachineStore)
		{
			if (String.IsNullOrEmpty (providerName))
				providerName = ProtectedConfiguration.DefaultProvider;
			
			ProtectedConfigurationProvider prov = ProtectedConfiguration.Providers [providerName];
			if (prov == null)
				throw new InvalidOperationException (String.Format ("Provider '{0}' is unknown.", providerName));
			
			// We need to create a new instance in order to be able to pass our own
			// parameters to the provider
			var ret = Activator.CreateInstance (prov.GetType ()) as ProtectedConfigurationProvider;
			ret.Initialize (providerName, new NameValueCollection {
					{"keyContainerName", containerName},
					{"useMachineContainer", useMachineStore.ToString ()},
				}
			);

			return ret;
		}

		string BuildXPathExpression (string configSection)
		{
			var sb = new StringBuilder ("//config:configuration");
			string[] parts = configSection.Split (sectionSplitChars);

			foreach (string s in parts)
				sb.Append ("/config:" + s);

			return sb.ToString ();
		}

		bool IsValidSection (string configFile, string configSection)
		{
			var exeMap = new ExeConfigurationFileMap ();
			exeMap.ExeConfigFilename = configFile;

			global::System.Configuration.Configuration config = ConfigurationManager.OpenMappedExeConfiguration (exeMap, ConfigurationUserLevel.None);
			if (config == null)
				return false;

			try {
				var cs = config.GetSection (configSection) as ConfigurationSection;
				if (cs == null)
					return false;
			} catch (CryptographicException) {
				// Ignore - it's ok, since we only want to know if the section is
				// valid and this exception is thrown when an attempt to decrypt a
				// section is made and the protection provider for some reason
				// didn't do the job (e.g. we encrypted the section with local store
				// key container and the provider wants to access a global store one
				// and cannot find it)
			}
			
			return true;
		}
		
		string LoadSection (string configFile, string configSection, string containerName, bool useMachineStore,
				    out XmlDocument doc, out XmlNode section, out ProtectedConfigurationProvider provider)
		{
			if (!IsValidSection (configFile, configSection))
				throw new InvalidOperationException (String.Format ("Section '{0}' is not a valid section in file '{1}'", configSection, configFile));
			
			// This should be done using Configuration, but currently the Mono
			// System.Configuration saves configuration files without preserving
			// non-significant whitespace which gives a very ugly result.
			doc = new XmlDocument ();
			doc.PreserveWhitespace = true;
			doc.Load (configFile);

			XmlNamespaceManager nsmgr = null;
			XmlElement root = doc.DocumentElement;
			string ns = null;
			if (root.HasAttributes) {
				ns = root.GetAttribute ("xmlns");
				if (!String.IsNullOrEmpty (ns)) {
					nsmgr = new XmlNamespaceManager (doc.NameTable);
					nsmgr.AddNamespace ("config", ns);
				}
			}
			
			section = doc.DocumentElement.SelectSingleNode (BuildXPathExpression (configSection), nsmgr);
			// This check is necessary even though IsValidSection returned true - it's
			// because the section might have been found in files other than the one
			// we're processing.
			if (section == null)
				throw new InvalidOperationException (String.Format ("Section '{0}' not found in config file '{1}'", configSection, configFile));

			XmlAttribute attr = section.Attributes ["configProtectionProvider"];
			string configProtectionProvider = attr == null ? null : attr.Value;
			
			provider = GetProvider (configProtectionProvider, containerName, useMachineStore);

			return ns;
		}

		void SaveWithBackup (string configFile, XmlDocument doc)
		{
			string backupFile = configFile + ".save";
			bool copied = false;
			
			try {
				File.Copy (configFile, backupFile, true);
				copied = true;
				
				using (var fs = File.Open (configFile, FileMode.Truncate, FileAccess.Write, FileShare.None)) {
					doc.Save (fs);
					fs.Flush ();
				}
			} catch {
				if (copied)
					File.Copy (backupFile, configFile);
			} finally {
				if (copied)
					File.Delete (backupFile);
			}
		}
		
		public void Encrypt (string configFile, string configSection, string containerName, bool useMachineStore)
		{
			if (String.IsNullOrEmpty (configFile))
				throw new ArgumentNullException ("configFile");

			if (String.IsNullOrEmpty (configSection))
				throw new ArgumentNullException ("configSection");

			if (String.IsNullOrEmpty (containerName))
				throw new ArgumentNullException ("containerName");
			
			XmlNode section;
			XmlDocument doc;
			ProtectedConfigurationProvider provider;

			string ns = LoadSection (configFile, configSection, containerName, useMachineStore, out doc, out section, out provider);

			XmlNode encrypted = provider.Encrypt (section);
			if (encrypted == null)
				throw new InvalidOperationException (String.Format ("Failed to encrypt '{0}' section in config file '{1}'", configSection, configFile));			

			XmlNode newSection = doc.CreateElement (section.Name, ns);
			XmlAttribute attr = doc.CreateAttribute ("configProtectionProvider");
			attr.Value =  provider.Name;
			newSection.InnerXml = encrypted.OuterXml;
			newSection.Attributes.Append (attr);
			
			section.ParentNode.ReplaceChild (newSection, section);
			SaveWithBackup (configFile, doc);
		}

		public void Decrypt (string configFile, string configSection, string containerName, bool useMachineStore)
		{
			if (String.IsNullOrEmpty (configFile))
				throw new ArgumentNullException ("configFile");

			if (String.IsNullOrEmpty (configSection))
				throw new ArgumentNullException ("configSection");

			if (String.IsNullOrEmpty (containerName))
				throw new ArgumentNullException ("containerName");
			
			XmlNode section;
			XmlDocument doc;
			ProtectedConfigurationProvider provider;

			LoadSection (configFile, configSection, containerName, useMachineStore, out doc, out section, out provider);
			
			XmlNode decrypted = provider.Decrypt (section);
			XmlNode newNode = doc.ImportNode (decrypted.FirstChild, true);
			XmlAttribute attr = newNode.Attributes ["xmlns"];
			
			if (attr != null)
				newNode.Attributes.Remove (attr);
			
			section.ParentNode.ReplaceChild (newNode, section);
			SaveWithBackup (configFile, doc);
		}
	}
}
