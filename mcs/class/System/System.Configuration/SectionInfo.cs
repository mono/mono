//
// System.Configuration.SectionInfo.cs
//
// Authors:
//	Lluis Sanchez (lluis@novell.com)
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
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Xml;
using System.IO;

namespace System.Configuration
{
	internal class SectionInfo: ConfigInfo
	{
		public bool AllowLocation = true;
		public ConfigurationAllowDefinition AllowDefinition = ConfigurationAllowDefinition.Everywhere;

		public SectionInfo ()
		{
		}
		
		public SectionInfo (string sectionName, string typeName,
				    bool allowLocation, ConfigurationAllowDefinition allowDefinition)
		{
			Name = sectionName;
			TypeName = typeName;
			AllowLocation = allowLocation;
			AllowDefinition = allowDefinition;
		}
		
		public override bool HasDataContent (Configuration config)
		{
			return config.GetSectionInstance (this, false) != null;
		}
		
		public override bool HasConfigContent (Configuration cfg)
		{
			return FileName == cfg.FileName;
		}

#if (XML_DEP)
		public override void ReadConfig (Configuration cfg, XmlTextReader reader)
		{
			string attName;
			ConfigurationAllowDefinition allowDefinition = ConfigurationAllowDefinition.Everywhere;
			FileName = cfg.FileName;

			while (reader.MoveToNextAttribute ()) {
				switch (reader.Name)
				{
					case "allowLocation":
						string allowLoc = reader.Value;
						AllowLocation = (allowLoc == "true");
						if (!AllowLocation && allowLoc != "false")
							ThrowException ("Invalid attribute value", reader);
						break;
	
					case "allowDefinition":
						string allowDef = reader.Value;
						try {
							allowDefinition = (ConfigurationAllowDefinition) Enum.Parse (
									   typeof (ConfigurationAllowDefinition), allowDef);
						} catch {
							ThrowException ("Invalid attribute value", reader);
						}
						break;
	
					case "type":
						TypeName = reader.Value;
						break;
					
					case "name":
						Name = reader.Value;
						if (Name == "location")
							ThrowException ("location is a reserved section name", reader);
						break;
						
					default:
						ThrowException ("Unrecognized attribute.", reader);
						break;
				}
			}

			if (Name == null || TypeName == null)
				ThrowException ("Required attribute missing", reader);

			reader.MoveToElement();
			reader.Skip ();
		}
		
		public override void WriteConfig (Configuration cfg, XmlWriter writer, ConfigurationUpdateMode mode)
		{
			writer.WriteStartElement ("section");
			writer.WriteAttributeString ("name", Name);
			writer.WriteAttributeString ("type", TypeName);
			if (!AllowLocation)
				writer.WriteAttributeString ("allowLocation", "false");
			if (AllowDefinition != ConfigurationAllowDefinition.Everywhere)
				writer.WriteAttributeString ("allowDefinition", AllowDefinition.ToString ());
			writer.WriteEndElement ();
		}
		
		public override void ReadData (Configuration config, XmlTextReader reader)
		{
			config.SetSectionData (this, reader.ReadOuterXml ());
		}
		
		public override void WriteData (Configuration config, XmlWriter writer, ConfigurationUpdateMode mode)
		{
			ConfigurationSection section = config.GetSectionInstance (this, false);
			if (section != null) {
				ConfigurationSection parentSection = config.Parent != null ? config.Parent.GetSectionInstance (this, false) : null;
				string xml = section.WriteXml (parentSection, config, Name, mode);
				writer.WriteRaw (xml);
			}
		}
#endif
	}
}

#endif
