//
// System.Configuration.SectionGroupInfo.cs
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
	internal class SectionGroupInfo: ConfigInfo
	{
		ConfigInfoCollection sections;
		ConfigInfoCollection groups;
		static ConfigInfoCollection emptyList = new ConfigInfoCollection ();

		public SectionGroupInfo ()
		{
			TypeName = "System.Configuration.ConfigurationSectionGroup";
		}
		
		public SectionGroupInfo (string groupName, string typeName)
		{
			Name = groupName;
			TypeName = typeName;
		}
		
		public void AddChild (ConfigInfo data)
		{
			data.Parent = this;
			if (data is SectionInfo) {
				if (sections == null) sections = new ConfigInfoCollection ();
				sections [data.Name] = data;
			}
			else {
				if (groups == null) groups = new ConfigInfoCollection ();
				groups [data.Name] = data;
			}
		}
		
		public void Clear ()
		{
			if (sections != null) sections.Clear ();
			if (groups != null) groups.Clear ();
		}
		
		public bool HasChild (string name)
		{
			if (sections != null && sections [name] != null) return true;
			return (groups != null && groups [name] != null);
		}
		
		public void RemoveChild (string name)
		{
			if (sections != null)
				sections.Remove (name);
			if (groups != null)
				groups.Remove (name);
		}
		
		public SectionInfo GetChildSection (string name)
		{
			if (sections != null)
				return sections [name] as SectionInfo;
			else
				return null;
		}
		
		public SectionGroupInfo GetChildGroup (string name)
		{
			if (groups != null)
				return groups [name] as SectionGroupInfo;
			else
				return null;
		}
		
		public ConfigInfoCollection Sections
		{
			get { if (sections == null) return emptyList; else return sections; }
		}
		
		public ConfigInfoCollection Groups
		{
			get { if (groups == null) return emptyList; else return groups; }
		}
		
		public override bool HasDataContent (Configuration config)
		{
			foreach (ConfigInfoCollection col in new object[] {Sections, Groups}) {
				foreach (string key in col) {
					ConfigInfo cinfo = col [key];
					if (cinfo.HasDataContent (config))
						return true;
				}
			}
			return false;
		}
		
		public override bool HasConfigContent (Configuration cfg)
		{
			if (FileName == cfg.FileName) return true;
			foreach (ConfigInfoCollection col in new object[] {Sections, Groups}) {
				foreach (string key in col) {
					ConfigInfo cinfo = col [key];
					if (cinfo.HasConfigContent (cfg))
						return true;
				}
			}
			return false;
		}
		
#if (XML_DEP)

		public override void ReadConfig (Configuration cfg, XmlTextReader reader)
		{
			FileName = cfg.FileName;
			
			if (reader.LocalName != "configSections")
			{
				while (reader.MoveToNextAttribute ()) {
					if (reader.Name == "name")
						Name = reader.Value;
					else if (reader.Name == "type")
						TypeName = reader.Value;
					else
						ThrowException ("Unrecognized attribute", reader);
				}
				
				if (Name == null)
					ThrowException ("sectionGroup must have a 'name' attribute", reader);
	
				if (Name == "location")
					ThrowException ("location is a reserved section name", reader);
			}
			
			if (TypeName == null)
				TypeName = "System.Configuration.ConfigurationSectionGroup";
			
			if (reader.IsEmptyElement) {
				reader.Skip ();
				return;
			}
			
			reader.ReadStartElement ();
			reader.MoveToContent ();
			
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType != XmlNodeType.Element) {
					reader.Skip ();
					continue;
				}
				
				string name = reader.LocalName;
				ConfigInfo cinfo = null;
				
				if (name == "remove") {
					ReadRemoveSection (reader);
					continue;
				}

				if (name == "clear") {
					if (reader.HasAttributes)
						ThrowException ("Unrecognized attribute.", reader);

					Clear ();
					reader.Skip ();
					continue;
				}

				if (name == "section")
					cinfo = new SectionInfo ();
				else if (name == "sectionGroup")
					cinfo = new SectionGroupInfo ();
				else
					ThrowException ("Unrecognized element: " + reader.Name, reader);
					
				cinfo.ReadConfig (cfg, reader);
				ConfigInfo actInfo = Groups [cinfo.Name];
				if (actInfo == null) actInfo = Sections [cinfo.Name];
				
				if (actInfo != null) {
					if (actInfo.GetType () != cinfo.GetType ())
						ThrowException ("A section or section group named '" + cinfo.Name + "' already exists", reader);
					// Make sure that this section is saved in this configuration file:
					actInfo.FileName = cfg.FileName;
				}
				else
					AddChild (cinfo);
			}
			
			reader.ReadEndElement ();
		}
		
		public override void WriteConfig (Configuration cfg, XmlWriter writer, ConfigurationUpdateMode mode)
		{
			if (Name != null) {
				writer.WriteStartElement ("sectionGroup");
				writer.WriteAttributeString ("name", Name);
				if (TypeName != null && TypeName != "" && TypeName != "System.Configuration.ConfigurationSectionGroup")
					writer.WriteAttributeString ("type", TypeName);
			}
			else
				writer.WriteStartElement ("configSections");
			
			foreach (ConfigInfoCollection col in new object[] {Sections, Groups}) {
				foreach (string key in col) {
					ConfigInfo cinfo = col [key];
					if (cinfo.HasConfigContent (cfg))
						cinfo.WriteConfig (cfg, writer, mode);
				}
			}
			
			writer.WriteEndElement ();
		}

		private void ReadRemoveSection (XmlTextReader reader)
		{
			if (!reader.MoveToNextAttribute () || reader.Name != "name")
				ThrowException ("Unrecognized attribute.", reader);

			string removeValue = reader.Value;
			if (removeValue == null || removeValue.Length == 0)
				ThrowException ("Empty name to remove", reader);

			reader.MoveToElement ();

			if (!HasChild (removeValue))
				ThrowException ("No factory for " + removeValue, reader);

			RemoveChild (removeValue);
			reader.Skip ();
		}

		public void ReadRootData (XmlTextReader reader, Configuration config)
		{
			reader.MoveToContent ();
			ReadContent (reader, config);
		}
		
		public override void ReadData (Configuration config, XmlTextReader reader)
		{
			reader.MoveToContent ();
			reader.ReadStartElement ();
			ReadContent (reader, config);
			reader.MoveToContent ();
			reader.ReadEndElement ();
		}
		
		void ReadContent (XmlTextReader reader, Configuration config)
		{
			while (reader.NodeType != XmlNodeType.EndElement) {
				if (reader.NodeType != XmlNodeType.Element) {
					reader.Skip ();
					continue;
				}
				if (reader.LocalName == "location") {
					Configuration locConfig = new Configuration (config);
					string path = reader.GetAttribute ("path");
					ConfigurationLocation loc = new ConfigurationLocation (path, locConfig);
					config.Locations.Add (loc);
					ReadData (locConfig, reader);
				}
				
				ConfigInfo data = (sections != null) ? (ConfigInfo) sections [reader.LocalName] : (ConfigInfo) null;
				if (data == null) data = (groups != null) ? (ConfigInfo) groups [reader.LocalName] : (ConfigInfo) null;
				
				if (data != null)
					data.ReadData (config, reader);
				else
					ThrowException ("Unrecognized configuration section <" + reader.LocalName + ">", reader);
			}
		}
		
		public void WriteRootData (XmlWriter writer, Configuration config, ConfigurationUpdateMode mode)
		{
			WriteContent (writer, config, mode, false);
		}
		
		public override void WriteData (Configuration config, XmlWriter writer, ConfigurationUpdateMode mode)
		{
			writer.WriteStartElement (Name);
			WriteContent (writer, config, mode, true);
			writer.WriteEndElement ();
		}
		
		public void WriteContent (XmlWriter writer, Configuration config, ConfigurationUpdateMode mode, bool writeElem)
		{
			foreach (ConfigInfoCollection col in new object[] {Sections, Groups}) {
				foreach (string key in col) {
					ConfigInfo cinfo = col [key];
					if (cinfo.HasDataContent (config))
						cinfo.WriteData (config, writer, mode);
				}
			}
		}
#endif
	}
	
	internal class ConfigInfoCollection : NameObjectCollectionBase
	{
		public ICollection AllKeys
		{
			get { return Keys; }
		}
		
		public ConfigInfo this [string name]
		{
			get { return (ConfigInfo) BaseGet (name); }
			set { BaseSet (name, value); }
		}
	
		public ConfigInfo this [int index]
		{
			get { return (ConfigInfo) BaseGet (index); }
			set { BaseSet (index, value); }
		}
		
		public void Add (string name, ConfigInfo config)
		{
			BaseAdd (name, config);
		}
		
		public void Clear ()
		{
			BaseClear ();
		}
		
		public string GetKey (int index)
		{
			return BaseGetKey (index);
		}
		
		public void Remove (string name)
		{
			BaseRemove (name);
		}
		
		public void RemoveAt (int index)
		{
			BaseRemoveAt (index);
		}
	}
}

#endif
