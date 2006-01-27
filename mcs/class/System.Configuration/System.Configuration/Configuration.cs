//
// System.Configuration.Configuration.cs
//
// Authors:
//	Duncan Mak (duncan@ximian.com)
// 	Lluis Sanchez Gual (lluis@novell.com)
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
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using System.Xml;
using System.IO;
using System.Configuration.Internal;

namespace System.Configuration {

	public sealed class Configuration
	{
		Configuration parent;
		Hashtable elementData = new Hashtable ();
		string streamName;
		ConfigurationSectionGroup rootSectionGroup;
		ConfigurationLocationCollection locations;
		SectionGroupInfo rootGroup;
		IConfigSystem system;
		bool hasFile;
		string rootNamespace;
		
		string configPath;
		string locationConfigPath;
			
		internal Configuration (Configuration parent)
		{
			Init (parent.system, null, parent);
		}
		
		internal Configuration (InternalConfigurationSystem system, string locationSubPath)
		{
			hasFile = true;
			this.system = system;
			
			system.InitForConfiguration (ref locationSubPath, out configPath, out locationConfigPath);
			
			Configuration parent = null;
			
			if (locationSubPath != null) {
				parent = new Configuration (system, locationSubPath);
				if (locationConfigPath != null)
					parent = parent.FindLocationConfiguration (locationConfigPath, parent);
			}
			
			Init (system, configPath, parent);
		}
		
		internal Configuration FindLocationConfiguration (string relativePath, Configuration defaultConfiguration)
		{
			ConfigurationLocation loc = Locations.Find (relativePath);
			
			Configuration parentConfig = defaultConfiguration;
			
			if (LocationConfigPath != null) {
				Configuration parentFile = GetParentWithFile ();
				if (parentFile != null) {
					string parentRelativePath = system.Host.GetConfigPathFromLocationSubPath (LocationConfigPath, relativePath);
					parentConfig = parentFile.FindLocationConfiguration (parentRelativePath, defaultConfiguration);
				}
			}

			if (loc == null)
				return parentConfig;
			
			loc.SetParentConfiguration (parentConfig);
			return loc.OpenConfiguration ();
		}
		
		internal void Init (IConfigSystem system, string configPath, Configuration parent)
		{
			this.system = system;
			this.configPath = configPath;
			streamName = system.Host.GetStreamName (configPath);
			this.parent = parent;
			if (parent != null)
				rootGroup = parent.rootGroup;
			else {
				rootGroup = new SectionGroupInfo ();
				rootGroup.StreamName = streamName;
			}
			
			if (configPath != null)
				Load ();
		}
		
		internal Configuration Parent {
			get { return parent; }
			set { parent = value; }
		}
		
		internal Configuration GetParentWithFile ()
		{
			Configuration parentFile = Parent;
			while (parentFile != null && !parentFile.HasFile)
				parentFile = parentFile.Parent;
			return parentFile;
		}
		
		internal string FileName {
			get { return streamName; }
		}

		internal IInternalConfigHost ConfigHost {
			get { return system.Host; }
		}
		
		internal string LocationConfigPath {
			get { return locationConfigPath; }
		}

		internal string ConfigPath {
			get { return configPath; }
		}

		public AppSettingsSection AppSettings {
			get { return (AppSettingsSection) GetSection ("appSettings"); }
		}

		public ConnectionStringsSection ConnectionStrings {
			get { return (ConnectionStringsSection) GetSection ("connectionStrings"); }
		}

		public string FilePath {
			get { return streamName; }
		}

		public bool HasFile {
			get {
				return hasFile;
			}
		}

		[MonoTODO ("HostingContext")]
		ContextInformation evaluationContext;
		public ContextInformation EvaluationContext {
			get {
				if (evaluationContext == null)
					evaluationContext = new ContextInformation (this, null /* XXX */);
				return evaluationContext;
			}
		}
		
		public ConfigurationLocationCollection Locations {
			get {
				if (locations == null) locations = new ConfigurationLocationCollection ();
				return locations;
			}
		}

		public bool NamespaceDeclared {
			get { return rootNamespace != null; }
			set { rootNamespace = value ? "http://schemas.microsoft.com/.NetConfiguration/v2.0" : null; }
		}

		public ConfigurationSectionGroup RootSectionGroup {
			get {
				if (rootSectionGroup == null) {
					rootSectionGroup = new ConfigurationSectionGroup ();
					rootSectionGroup.Initialize (this, rootGroup);
				}
				return rootSectionGroup;
			}                        
		}

		public ConfigurationSectionGroupCollection SectionGroups {
			get { return RootSectionGroup.SectionGroups; }                        
		}

		public ConfigurationSectionCollection Sections {
			get { return RootSectionGroup.Sections; }
		}
		
		public ConfigurationSection GetSection (string path)
		{
			string[] parts = path.Split ('/');
			if (parts.Length == 1)
				return Sections [parts[0]];

			ConfigurationSectionGroup group = SectionGroups [parts[0]];
			for (int n=1; group != null && n<parts.Length-1; n++)
				group = group.SectionGroups [parts [n]];

			if (group != null)
				return group.Sections [parts [parts.Length - 1]];
			else
				return null;
		}
		
		public ConfigurationSectionGroup GetSectionGroup (string path)
		{
			string[] parts = path.Split ('/');
			ConfigurationSectionGroup group = SectionGroups [parts[0]];
			for (int n=1; group != null && n<parts.Length; n++)
				group = group.SectionGroups [parts [n]];
			return group;
		}
		
		internal ConfigurationSection GetSectionInstance (SectionInfo config, bool createDefaultInstance)
		{
			object data = elementData [config];
			ConfigurationSection sec = data as ConfigurationSection;
			if (sec != null || !createDefaultInstance) return sec;
			
			object secObj = config.CreateInstance () as ConfigurationSection;
			if (secObj == null)
				sec = new IgnoreSection ();
			else
				sec = (ConfigurationSection) secObj;
				
			ConfigurationSection parentSection = parent != null ? parent.GetSectionInstance (config, true) : null;
			sec.RawXml = data as string;
			sec.Reset (parentSection);
			
			if (data != null) {
				XmlTextReader r = new XmlTextReader (new StringReader (data as string));
				sec.DeserializeSection (r);
				r.Close ();
			}
			
			elementData [config] = sec;
			return sec;
		}
		
		internal ConfigurationSectionGroup GetSectionGroupInstance (SectionGroupInfo group)
		{
			ConfigurationSectionGroup gr = group.CreateInstance () as ConfigurationSectionGroup;
			if (gr != null) gr.Initialize (this, group);
			return gr;
		}
		
		internal void SetConfigurationSection (SectionInfo config, ConfigurationSection sec)
		{
			elementData [config] = sec;
		}
		
		internal void SetSectionXml (SectionInfo config, string data)
		{
			elementData [config] = data;
		}
		
		internal string GetSectionXml (SectionInfo config)
		{
			return elementData [config] as string;
		}
		
		internal void CreateSection (SectionGroupInfo group, string name, ConfigurationSection sec)
		{
			if (group.HasChild (name))
				throw new ConfigurationException ("Cannot add a ConfigurationSection. A section or section group already exists with the name '" + name + "'");
				
			if (!HasFile && !sec.SectionInformation.AllowLocation)
				throw new ConfigurationErrorsException ("The configuration section <" + name + "> cannot be defined inside a <location> element."); 

			if (!system.Host.IsDefinitionAllowed (configPath, sec.SectionInformation.AllowDefinition, sec.SectionInformation.AllowExeDefinition)) {
				object ctx = sec.SectionInformation.AllowExeDefinition != ConfigurationAllowExeDefinition.MachineToApplication ? (object) sec.SectionInformation.AllowExeDefinition : (object) sec.SectionInformation.AllowDefinition;
				throw new ConfigurationErrorsException ("The section <" + name + "> can't be defined in this configuration file (the allowed definition context is '" + ctx + "').");
			}
						
			if (sec.SectionInformation.Type == null)
				sec.SectionInformation.Type = system.Host.GetConfigTypeName (sec.GetType ());
			
			SectionInfo section = new SectionInfo (name, sec.SectionInformation.Type, sec.SectionInformation.AllowLocation, sec.SectionInformation.AllowDefinition, sec.SectionInformation.AllowExeDefinition);
			section.StreamName = streamName;
			section.ConfigHost = system.Host;
			group.AddChild (section);
			elementData [section] = sec;
		}
		
		internal void CreateSectionGroup (SectionGroupInfo parentGroup, string name, ConfigurationSectionGroup sec)
		{
			if (parentGroup.HasChild (name)) throw new ConfigurationException ("Cannot add a ConfigurationSectionGroup. A section or section group already exists with the name '" + name + "'");
			if (sec.Type == null) sec.Type = system.Host.GetConfigTypeName (sec.GetType ());
			sec.SetName (name);

			SectionGroupInfo section = new SectionGroupInfo (name, sec.Type);
			section.StreamName = streamName;
			section.ConfigHost = system.Host;
			parentGroup.AddChild (section);
			elementData [section] = sec;
		}
		
		internal void RemoveConfigInfo (ConfigInfo config)
		{
			elementData.Remove (config);
		}
		
		public void Save ()
		{
			Save (ConfigurationSaveMode.Modified, false);
		}
		
		public void Save (ConfigurationSaveMode mode)
		{
			Save (mode, false);
		}
		
		public void Save (ConfigurationSaveMode mode, bool forceUpdateAll)
		{
			object ctx = null;
			Stream stream = system.Host.OpenStreamForWrite (streamName, null, ref ctx);
			try {
				Save (stream, mode, forceUpdateAll);
				system.Host.WriteCompleted (streamName, true, ctx);
			} catch (Exception ex) {
				system.Host.WriteCompleted (streamName, false, ctx);
				throw;
			} finally {
				stream.Close ();
			}
		}
		
		public void SaveAs (string filename)
		{
			SaveAs (filename, ConfigurationSaveMode.Modified, false);
		}
		
		public void SaveAs (string filename, ConfigurationSaveMode mode)
		{
			SaveAs (filename, mode, false);
		}
		
		[MonoTODO ("Detect if file has changed")]
		public void SaveAs (string filename, ConfigurationSaveMode mode, bool forceUpdateAll)
		{
			Save (new FileStream (filename, FileMode.OpenOrCreate, FileAccess.Write), mode, forceUpdateAll);
		}

		void Save (Stream stream, ConfigurationSaveMode mode, bool forceUpdateAll)
		{
			XmlTextWriter tw = new XmlTextWriter (new StreamWriter (stream));
			tw.Formatting = Formatting.Indented;
			try {
				tw.WriteStartDocument ();
				if (rootNamespace != null)
					tw.WriteStartElement ("configuration", rootNamespace);
				else
					tw.WriteStartElement ("configuration");
				if (rootGroup.HasConfigContent (this)) {
					rootGroup.WriteConfig (this, tw, mode);
				}
				
				foreach (ConfigurationLocation loc in Locations) {
					if (loc.OpenedConfiguration == null) {
						tw.WriteRaw ("\n");
						tw.WriteRaw (loc.XmlContent);
					}
					else {
						tw.WriteStartElement ("location");
						tw.WriteAttributeString ("path", loc.Path); 
						if (!loc.AllowOverride)
							tw.WriteAttributeString ("allowOverride", "false");
						loc.OpenedConfiguration.SaveData (tw, mode, forceUpdateAll);
						tw.WriteEndElement ();
					}
				}
				
				SaveData (tw, mode, forceUpdateAll);
				tw.WriteEndElement ();
			}
			finally {
				tw.Close ();
			}
		}
		
		void SaveData (XmlTextWriter tw, ConfigurationSaveMode mode, bool forceUpdateAll)
		{
			rootGroup.WriteRootData (tw, this, mode);
		}
		
		bool Load ()
		{
			if (streamName == null || streamName == "")
				return true;

			XmlTextReader reader = null;
			Stream stream = null;
			
			try {
				stream = system.Host.OpenStreamForRead (streamName);
			} catch (Exception e) {
				return false;
			}

			try {
				reader = new XmlTextReader (stream);
				ReadConfigFile (reader, streamName);
			} finally {
				if (reader != null)
					reader.Close();
			}
			return true;
		}


		internal void ReadConfigFile (XmlTextReader reader, string fileName)
		{
			reader.MoveToContent ();

			if (reader.NodeType != XmlNodeType.Element || reader.Name != "configuration")
				ThrowException ("Configuration file does not have a valid root element", reader);

			if (reader.HasAttributes) {
				while (reader.MoveToNextAttribute ()) {
					if (reader.LocalName == "xmlns") {
						rootNamespace = reader.Value;
						continue;
					}
					ThrowException (String.Format ("Unrecognized attribute '{0}' in root element", reader.LocalName), reader);
				}
			}

			reader.MoveToElement ();

			if (reader.IsEmptyElement) {
				reader.Skip ();
				return;
			}
			
			reader.ReadStartElement ();
			reader.MoveToContent ();

			if (reader.LocalName == "configSections") {
				if (reader.HasAttributes)
					ThrowException ("Unrecognized attribute in <configSections>.", reader);
				
				rootGroup.ReadConfig (this, fileName, reader);
			}
			
			rootGroup.ReadRootData (reader, this, true);
		}
		
		internal void ReadData (XmlTextReader reader, bool allowOverride)
		{
			rootGroup.ReadRootData (reader, this, allowOverride);
		}
		

		private void ThrowException (string text, XmlTextReader reader)
		{
			throw new ConfigurationException (text, streamName, reader.LineNumber);
		}
	}
}

#endif
