//
// System.Configuration.AppSettingsSection.cs
//
// Authors:
//	Duncan Mak (duncan@ximian.com)
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

#if NET_2_0 && XML_DEP
#if XML_DEP
using System;
using System.Collections.Specialized;
using System.Xml;
using System.IO;

namespace System.Configuration {

	public sealed class AppSettingsSection : ConfigurationSection
	{
		ConfigNameValueCollection values;
		bool hasClear;
		
		public AppSettingsSection ()
		{
		}

		protected internal override  bool IsModified ()
		{
			return values != null && values.IsModified;
		}

		[MonoTODO ("Read file attribute")]
		protected internal override void ReadXml (XmlReader reader, object context)
		{
			XmlDocument doc = new XmlDocument ();
			XmlNode data = doc.ReadNode (reader);
			hasClear = ((XmlElement)data)["clear"] != null;
			values = ConfigHelper.GetNameValueCollection (values, data, "key", "value");
		}

		protected internal override void Reset (ConfigurationElement parentSection, object context)
		{
			AppSettingsSection sec = parentSection as AppSettingsSection;
			if (sec != null && sec.values != null)
				values = new ConfigNameValueCollection (sec.values);
			else
				values = null;
		}

		protected internal override void ResetModified ()
		{
			if (values != null) values.ResetModified ();
		}

		protected internal override string WriteXml (
			ConfigurationElement parent, object context, string name, ConfigurationUpdateMode mode)
		{
			AppSettingsSection sec = parent as AppSettingsSection;
			NameValueCollection parentValues = sec != null && !hasClear ? sec.Settings : null;
			
			StringWriter sw = new StringWriter ();
			XmlTextWriter writer = new XmlTextWriter (sw);
			writer.WriteStartElement ("appSettings");
			
			if (hasClear) {
				writer.WriteStartElement ("clear");
				writer.WriteEndElement ();
			}
			
			foreach (string key in values) {
				string val = values [key];
				string parentVal = parentValues != null ? parentValues [key] : null;
				if (parentVal != val) {
					writer.WriteStartElement ("add");
					writer.WriteAttributeString ("key", key);
					writer.WriteAttributeString ("value", val);
					writer.WriteEndElement ();
				}
			}
			
			if (parentValues != null) {
				foreach (string key in parentValues) {
					if (values [key] == null) {
						writer.WriteStartElement ("remove");
						writer.WriteAttributeString ("key", key);
						writer.WriteEndElement ();
					}
				}
			}
			
			writer.WriteEndElement ();
			return sw.ToString ();
		}

		[MonoTODO]
		public string File {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public NameValueCollection Settings {
			get {
				if (values == null)
					values = new ConfigNameValueCollection();
				return values;
			}
		}
	}
}
#endif
#endif
