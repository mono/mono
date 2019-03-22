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

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Xml;
using System.Text;
using System.IO;

namespace System.Configuration
{
	internal class SectionInfo: ConfigInfo
	{
		bool allowLocation = true;
		bool requirePermission = true;
		bool restartOnExternalChanges;
		ConfigurationAllowDefinition allowDefinition = ConfigurationAllowDefinition.Everywhere;
		ConfigurationAllowExeDefinition allowExeDefinition = ConfigurationAllowExeDefinition.MachineToApplication;

		public SectionInfo ()
		{
		}
		
		public SectionInfo (string sectionName, SectionInformation info)
		{
			Name = sectionName;
			TypeName = info.Type;
			this.allowLocation = info.AllowLocation;
			this.allowDefinition = info.AllowDefinition;
			this.allowExeDefinition = info.AllowExeDefinition;
			this.requirePermission = info.RequirePermission;
			this.restartOnExternalChanges = info.RestartOnExternalChanges;
		}
		
		public override object CreateInstance ()
		{
			object ob = base.CreateInstance ();
			ConfigurationSection sec = ob as ConfigurationSection;
			if (sec != null) {
				sec.SectionInformation.AllowLocation = allowLocation;
				sec.SectionInformation.AllowDefinition = allowDefinition;
				sec.SectionInformation.AllowExeDefinition = allowExeDefinition;
				sec.SectionInformation.RequirePermission = requirePermission;
				sec.SectionInformation.RestartOnExternalChanges = restartOnExternalChanges;
				sec.SectionInformation.SetName (Name);
			}
			return ob;
		}
		
		public override bool HasDataContent (Configuration config)
		{
			return config.GetSectionInstance (this, false) != null || config.GetSectionXml (this) != null;
		}
		
		public override bool HasConfigContent (Configuration cfg)
		{
			return StreamName == cfg.FileName;
		}

		public override void ReadConfig (Configuration cfg, string streamName, XmlReader reader)
		{
			StreamName = streamName;
			ConfigHost = cfg.ConfigHost;

			while (reader.MoveToNextAttribute ()) {
				switch (reader.Name)
				{
					case "allowLocation":
						string allowLoc = reader.Value;
						allowLocation = (allowLoc == "true");
						if (!allowLocation && allowLoc != "false")
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
	
					case "allowExeDefinition":
						string allowExeDef = reader.Value;
						try {
							allowExeDefinition = (ConfigurationAllowExeDefinition) Enum.Parse (
									   typeof (ConfigurationAllowExeDefinition), allowExeDef);
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
						
					case "requirePermission":
						string reqPerm = reader.Value;
						bool reqPermValue = (reqPerm == "true");
						if (!reqPermValue && reqPerm != "false")
							ThrowException ("Invalid attribute value", reader);
						requirePermission = reqPermValue;
						break;

					case "restartOnExternalChanges":
						string restart = reader.Value;
						bool restartValue = (restart == "true");
						if (!restartValue && restart != "false")
							ThrowException ("Invalid attribute value", reader);
						restartOnExternalChanges = restartValue;
						break;

					default:
						ThrowException (String.Format ("Unrecognized attribute: {0}", reader.Name), reader);
						break;
				}
			}

			if (Name == null || TypeName == null)
				ThrowException ("Required attribute missing", reader);

			reader.MoveToElement();
			reader.Skip ();
		}
		
		public override void WriteConfig (Configuration cfg, XmlWriter writer, ConfigurationSaveMode mode)
		{
			writer.WriteStartElement ("section");
			writer.WriteAttributeString ("name", Name);
			writer.WriteAttributeString ("type", TypeName);
			if (!allowLocation)
				writer.WriteAttributeString ("allowLocation", "false");
			if (allowDefinition != ConfigurationAllowDefinition.Everywhere)
				writer.WriteAttributeString ("allowDefinition", allowDefinition.ToString ());
			if (allowExeDefinition != ConfigurationAllowExeDefinition.MachineToApplication)
				writer.WriteAttributeString ("allowExeDefinition", allowExeDefinition.ToString ());
			if (!requirePermission)
				writer.WriteAttributeString ("requirePermission", "false");
			writer.WriteEndElement ();
		}
		
		public override void ReadData (Configuration config, XmlReader reader, bool overrideAllowed)
		{
			if (!config.HasFile && !allowLocation)
				throw new ConfigurationErrorsException ("The configuration section <" + Name + "> cannot be defined inside a <location> element.", reader); 
			if (!config.ConfigHost.IsDefinitionAllowed (config.ConfigPath, allowDefinition, allowExeDefinition)) {
				object ctx = allowExeDefinition != ConfigurationAllowExeDefinition.MachineToApplication ? (object) allowExeDefinition : (object) allowDefinition;
				throw new ConfigurationErrorsException ("The section <" + Name + "> can't be defined in this configuration file (the allowed definition context is '" + ctx + "').", reader);
			}
			if (config.GetSectionXml (this) != null)
				ThrowException ("The section <" + Name + "> is defined more than once in the same configuration file.", reader);
			config.SetSectionXml (this, reader.ReadOuterXml ());
		}
		
		public override void WriteData (Configuration config, XmlWriter writer, ConfigurationSaveMode mode)
		{
			string xml;
			
			ConfigurationSection section = config.GetSectionInstance (this, false);

			if (section != null) {
				ConfigurationSection parentSection = config.Parent != null ? config.Parent.GetSectionInstance (this, false) : null;
				xml = section.SerializeSection (parentSection, Name, mode);

				string externalDataXml = section.ExternalDataXml;
				string filePath = config.FilePath;
				
				if (!String.IsNullOrEmpty (filePath) && !String.IsNullOrEmpty (externalDataXml)) {
					string path = Path.Combine (Path.GetDirectoryName (filePath), section.SectionInformation.ConfigSource);
					using (StreamWriter sw = new StreamWriter (path)) {
						sw.Write (externalDataXml);
					}
				}
				
				if (section.SectionInformation.IsProtected) {
					StringBuilder sb = new StringBuilder ();
					sb.AppendFormat ("<{0} configProtectionProvider=\"{1}\">\n",
							 Name,
							 section.SectionInformation.ProtectionProvider.Name);
					sb.Append (config.ConfigHost.EncryptSection (xml,
										     section.SectionInformation.ProtectionProvider,
										     ProtectedConfiguration.Section));
					sb.AppendFormat ("</{0}>", Name);
					xml = sb.ToString ();
				}
			}
			else {
				xml = config.GetSectionXml (this);
			}
			
			if (!string.IsNullOrEmpty (xml)) {
				writer.WriteRaw (xml);
/*				XmlTextReader tr = new XmlTextReader (new StringReader (xml));
				writer.WriteNode (tr, true);
				tr.Close ();*/
			}
		}
		
		internal override void Merge (ConfigInfo data)
		{}

		internal override bool HasValues (Configuration config, ConfigurationSaveMode mode)
		{
			var section = config.GetSectionInstance (this, false);
			if (section == null)
				return false;

			var parent = config.Parent != null ? config.Parent.GetSectionInstance (this, false) : null;
			return section.HasValues (parent, mode);
		}

		internal override void ResetModified (Configuration config)
		{
			ConfigurationSection section = config.GetSectionInstance (this, false);
			if (section != null)
				section.ResetModified ();
		}
	}
}

