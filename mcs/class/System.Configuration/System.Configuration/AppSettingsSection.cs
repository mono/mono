//
// System.Configuration.AppSettingsSection.cs
//
// Authors:
//	Duncan Mak (duncan@ximian.com)
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
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//

using System;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Xml;
using System.IO;

namespace System.Configuration {

	public sealed class AppSettingsSection : ConfigurationSection
	{
                private static ConfigurationPropertyCollection _properties;
                private static readonly ConfigurationProperty _propFile;
                private static readonly ConfigurationProperty _propSettings;

                static AppSettingsSection ()
                {
                        _propFile = new ConfigurationProperty ("file", typeof(string), "",
							       new StringConverter(), null, ConfigurationPropertyOptions.None);
                        _propSettings = new ConfigurationProperty ("", typeof(KeyValueConfigurationCollection), null, 
								   null, null, ConfigurationPropertyOptions.IsDefaultCollection);

                        _properties     = new ConfigurationPropertyCollection ();

                        _properties.Add (_propFile);
                        _properties.Add (_propSettings);
                }

		public AppSettingsSection ()
		{
		}

		protected internal override  bool IsModified ()
		{
			return Settings.IsModified ();
		}

		[MonoInternalNote ("file path?  do we use a System.Configuration api for opening it?  do we keep it open?  do we open it writable?")]
		protected internal override void DeserializeElement (XmlReader reader, bool serializeCollectionKey)
		{
			/* need to do this so we pick up the File attribute */
			base.DeserializeElement (reader, serializeCollectionKey);

			if (File != "") {
				try {
					Stream s = System.IO.File.OpenRead (File);
					XmlReader subreader = new ConfigXmlTextReader (s, File);
					base.DeserializeElement (subreader, serializeCollectionKey);
					s.Close ();
				}
				catch {
					// nada, we just ignore a missing/unreadble file
				}
			}
		}

		protected internal override void Reset (ConfigurationElement parentSection)
		{
			AppSettingsSection psec = parentSection as AppSettingsSection;
			if (psec != null)
				Settings.Reset (psec.Settings);
		}

		[MonoTODO]
		protected internal override string SerializeSection (
			ConfigurationElement parent, string name, ConfigurationSaveMode mode)
		{
			if (File == "") {
				return base.SerializeSection (parent, name, mode);
			}
			else {
				throw new NotImplementedException ();
			}
		}

		[ConfigurationProperty ("file", DefaultValue = "")]
		public string File {
			get { return (string)base [_propFile]; }
			set { base [_propFile] = value; }
		}

		[ConfigurationProperty ("", Options = ConfigurationPropertyOptions.IsDefaultCollection)]
		public KeyValueConfigurationCollection Settings {
			get { return (KeyValueConfigurationCollection) base [_propSettings]; }
		}

		protected internal override ConfigurationPropertyCollection Properties {
			get {
				return _properties;
			}
		}

		protected internal override object GetRuntimeObject ()
		{
			KeyValueInternalCollection col = new KeyValueInternalCollection ();
				
			foreach (string key in Settings.AllKeys) {
				KeyValueConfigurationElement ele = Settings[key];
				col.Add (ele.Key, ele.Value);
			}
				
			if (!ConfigurationManager.ConfigurationSystem.SupportsUserConfig)
				col.SetReadOnly ();

			return col;
		}
	}
}
