//
// System.Configuration.ConfigurationSection.cs
//
// Authors:
//	Duncan Mak (duncan@ximian.com)
//	Lluis Sanchez Gual (lluis@novell.com)
//	Martin Baulig <martin.baulig@xamarin.com>
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
// Copyright (c) 2012 Xamarin Inc. (http://www.xamarin.com)
//

using System.Collections;
using System.Xml;
using System.IO;
using System.Security.Cryptography.Xml;
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
					sectionInformation = new SectionInformation (this);
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
					SetRawXmlAndDeserialize (RawXml, Configuration.FilePath);
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
			elem.Configuration = Configuration;
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
			
			if (SectionHandler == null)
				DeserializeElement (reader, false);
		}
		
		[MonoInternalNote ("find the proper location for the decryption stuff")]
		private void DeserializeSection (XmlReader reader)
		{
			try
			{
				DoDeserializeSection (reader);

				// Overwrite deserialized section if it has `configSource` attribute:
				var hasConfigSourceProperty = !String.IsNullOrEmpty (SectionInformation.ConfigSource);
				if (hasConfigSourceProperty) {
					var baseDirOfIncludeFile = Path.GetFullPath (Path.GetDirectoryName (SectionInformation.ConfigFilePath ?? string.Empty));
					DeserializeConfigSource (baseDirOfIncludeFile);
				}
			}
			catch (ConfigurationErrorsException ex)
			{
				throw new ConfigurationErrorsException(String.Format("Error deserializing configuration section {0}: {1}", this.SectionInformation.Name, ex.Message));
			}
		}

		private void DeserializeConfigSource (string basePath)
		{
			string config_source = SectionInformation.ConfigSource;

			if (String.IsNullOrEmpty (config_source))
				return;

			if (Path.IsPathRooted (config_source))
				throw new ConfigurationErrorsException ("The configSource attribute must be a relative physical path.");
			
			if (HasLocalModifications ())
				throw new ConfigurationErrorsException ("A section using 'configSource' may contain no other attributes or elements.");
			
			string path = Path.Combine (basePath, config_source);
			if (!File.Exists (path)) {
				SetRawXmlAndDeserialize (null, String.Empty);
				throw new ConfigurationErrorsException (string.Format ("Unable to open configSource file '{0}'.", path));
			}
			
			var rawXml = File.ReadAllText (path);
			SetRawXmlAndDeserialize (rawXml, path);
		}

		protected override void DeserializeRawXml (XmlReader rawXmlReader)
		{
			DeserializeSection (rawXmlReader);
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

			/*
			 * FIXME: LAMESPEC
			 * 
			 * Cache the current values of 'parentElement' and 'saveMode' for later use in
			 * ConfigurationElement.SerializeToXmlElement().
			 * 
			 */
			elem.PrepareSave (parentElement, saveMode);
			bool hasValues = elem.HasValues (parentElement, saveMode);

			string ret;			
			using (StringWriter sw = new StringWriter ()) {
				using (XmlTextWriter tw = new XmlTextWriter (sw)) {
					tw.Formatting = Formatting.Indented;
					if (hasValues) {
						elem.SerializeToXmlElement (tw, name);
					}
					else if ((saveMode == ConfigurationSaveMode.Modified) && elem.IsModified ()) {
						// MS emits an empty section element.
						tw.WriteStartElement (name);
						tw.WriteEndElement ();
					}
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

