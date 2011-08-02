//
// System.Configuration.ConfigurationSection.cs
//
// Authors:
//	Duncan Mak (duncan@ximian.com)
//  Lluis Sanchez Gual (lluis@novell.com)
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
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//

#if NET_2_0
using System.Collections;
using System.Xml;
using System.IO;
#if !TARGET_JVM
using System.Security.Cryptography.Xml;
#endif
using System.Configuration.Internal;

namespace System.Configuration
{
	public abstract class ConfigurationSection : ConfigurationElement
	{
		SectionInformation sectionInformation;
		IConfigurationSectionHandler section_handler;
		string externalDataXml;
		
		protected ConfigurationSection ()
		{
		}

		internal string ExternalDataXml {
			get { return externalDataXml; }
		}
		
		internal IConfigurationSectionHandler SectionHandler {
			get { return section_handler; }
			set { section_handler = value; }
		}

		[MonoTODO]
		public SectionInformation SectionInformation {
			get {
				if (sectionInformation == null)
					sectionInformation = new SectionInformation ();
				return sectionInformation;
			}
		}
		
		private object _configContext;
		
		internal object ConfigContext {
			get {
				return _configContext;
			}
			set {
				_configContext = value;
			}
		}

		[MonoTODO ("Provide ConfigContext. Likely the culprit of bug #322493")]
		protected internal virtual object GetRuntimeObject ()
		{
			if (SectionHandler != null) {
				ConfigurationSection parentSection = sectionInformation != null ? sectionInformation.GetParentSection () : null;
				object parent = parentSection != null ? parentSection.GetRuntimeObject () : null;
				if (RawXml == null)
					return parent;
				
				try {
					// This code requires some re-thinking...
					XmlReader reader = new ConfigXmlTextReader (
						new StringReader (RawXml),
						Configuration.FilePath);

					DoDeserializeSection (reader);
					
					if (!String.IsNullOrEmpty (SectionInformation.ConfigSource)) {
						string fileDir = SectionInformation.ConfigFilePath;
						if (!String.IsNullOrEmpty (fileDir))
							fileDir = Path.GetDirectoryName (fileDir);
						else
							fileDir = String.Empty;
					
						string path = Path.Combine (fileDir, SectionInformation.ConfigSource);
						if (File.Exists (path)) {
							RawXml = File.ReadAllText (path);
							SectionInformation.SetRawXml (RawXml);
						}
					}
				} catch {
					// ignore, it can fail - we deserialize only in order to get
					// the configSource attribute
				}
				XmlDocument doc = new ConfigurationXmlDocument ();
				doc.LoadXml (RawXml);
				return SectionHandler.Create (parent, ConfigContext, doc.DocumentElement);
			}
			return this;
		}

		[MonoTODO]
		protected internal override bool IsModified ()
		{
			return base.IsModified ();
		}

		[MonoTODO]
		protected internal override void ResetModified ()
		{
			base.ResetModified ();
		}

		ConfigurationElement CreateElement (Type t)
		{
			ConfigurationElement elem = (ConfigurationElement) Activator.CreateInstance (t);
			elem.Init ();
			if (IsReadOnly ())
				elem.SetReadOnly ();
			return elem;
		}

		void DoDeserializeSection (XmlReader reader)
		{
			reader.MoveToContent ();

			string protection_provider = null;
			string config_source = null;
			string localName;
			
			while (reader.MoveToNextAttribute ()) {
				localName = reader.LocalName;
				if (localName == "configProtectionProvider")
					protection_provider = reader.Value;
				else if (localName == "configSource")
					config_source = reader.Value;
			}

			/* XXX this stuff shouldn't be here */
			{
				if (protection_provider != null) {
					ProtectedConfigurationProvider prov = ProtectedConfiguration.GetProvider (protection_provider, true);
					XmlDocument doc = new ConfigurationXmlDocument ();

					reader.MoveToElement ();

					doc.Load (new StringReader (reader.ReadInnerXml ()));

					XmlNode n = prov.Decrypt (doc);

					reader = new XmlNodeReader (n);

					SectionInformation.ProtectSection (protection_provider);

					reader.MoveToContent ();
				}
			}

			if (config_source != null)
				SectionInformation.ConfigSource = config_source;
			
			SectionInformation.SetRawXml (RawXml);
			if (SectionHandler == null)
				DeserializeElement (reader, false);
		}
		
		[MonoInternalNote ("find the proper location for the decryption stuff")]
		protected internal virtual void DeserializeSection (XmlReader reader)
		{
			DoDeserializeSection (reader);
		}

		internal void DeserializeConfigSource (string basePath)
		{
			string config_source = SectionInformation.ConfigSource;

			if (String.IsNullOrEmpty (config_source))
				return;

			if (Path.IsPathRooted (config_source))
				throw new ConfigurationException ("The configSource attribute must be a relative physical path.");
			
			if (HasLocalModifications ())
				throw new ConfigurationException ("A section using 'configSource' may contain no other attributes or elements.");
			
			string path = Path.Combine (basePath, config_source);
			if (!File.Exists (path)) {
				RawXml = null;
				SectionInformation.SetRawXml (null);
				return;
			}
			
			RawXml = File.ReadAllText (path);
			SectionInformation.SetRawXml (RawXml);
			DeserializeElement (new ConfigXmlTextReader (new StringReader (RawXml), path), false);
		}
		
		protected internal virtual string SerializeSection (ConfigurationElement parentElement, string name, ConfigurationSaveMode saveMode)
		{
			externalDataXml = null;
			ConfigurationElement elem;
			if (parentElement != null) {
				elem = (ConfigurationElement) CreateElement (GetType());
				elem.Unmerge (this, parentElement, saveMode);
			}
			else
				elem = this;
			
			string ret;			
			using (StringWriter sw = new StringWriter ()) {
				using (XmlTextWriter tw = new XmlTextWriter (sw)) {
					tw.Formatting = Formatting.Indented;
					elem.SerializeToXmlElement (tw, name);
					tw.Close ();
				}
				
				ret = sw.ToString ();
			}
			
			string config_source = SectionInformation.ConfigSource;
			
			if (String.IsNullOrEmpty (config_source))
				return ret;

			externalDataXml = ret;
			using (StringWriter sw = new StringWriter ()) {
				bool haveName = !String.IsNullOrEmpty (name);

				using (XmlTextWriter tw = new XmlTextWriter (sw)) {
					if (haveName)
						tw.WriteStartElement (name);
					tw.WriteAttributeString ("configSource", config_source);
					if (haveName)
						tw.WriteEndElement ();
				}

				return sw.ToString ();
			}
		}
	}
}
#endif
