//
// System.Configuration.ConfigurationSettings.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// C) Christopher Podurgiel
// (c) 2002 Ximian, Inc. (http://www.ximian.com)
//

using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.XPath;

namespace System.Configuration
{
	public sealed class ConfigurationSettings
	{
		static IConfigurationSystem config;
			
		private ConfigurationSettings ()
		{
		}

		public static object GetConfig (string sectionName)
		{
			if (config == null)
				config = DefaultConfig.GetInstance ();

			return config.GetConfig (sectionName);
		}

		public static NameValueCollection AppSettings
		{
			get {
				object appSettings = GetConfig ("appSettings");
				if (appSettings == null)
					appSettings = new NameValueCollection ();

				return (NameValueCollection) appSettings;
			}
		}

	}

	//
	// class DefaultConfig: read configuration from machine.config file and application
	// config file if available.
	//
	class DefaultConfig : IConfigurationSystem
	{
		static string creatingInstance = "137213797382-asad";
		static string buildingData = "1797382-ladgasjkdg";
		static DefaultConfig instance;
		ConfigurationData config;

		private DefaultConfig ()
		{
		}

		public static DefaultConfig GetInstance ()
		{
			if (instance == null) {
				lock (creatingInstance) {
					if (instance == null) {
						instance = new DefaultConfig ();
						instance.Init ();
					}
					
				}
			}

			return instance;
		}

		public object GetConfig (string sectionName)
		{
			if (config == null) 
				return null;

			return config.GetConfig (sectionName);
		}

		public void Init ()
		{
			if (config == null){
				lock (buildingData) {
					if (config != null)
						return;

					ConfigurationData data = new ConfigurationData ();
					if (data.Load (GetMachineConfigPath ())) {
						ConfigurationData appData = new ConfigurationData (data);
						appData.Load (GetAppConfigPath ());
						config = appData;
					} else {
						Console.WriteLine ("** Warning **: cannot find " + GetMachineConfigPath ());
						Console.WriteLine ("Trying to load app config file...");
						data.Load (GetAppConfigPath ());
						config = data;
					}
				}
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern private static string get_machine_config_path ();

		private static string GetMachineConfigPath ()
		{
			return get_machine_config_path ();
		}

		private static string GetAppConfigPath ()
		{
			AppDomainSetup currentInfo = AppDomain.CurrentDomain.SetupInformation;

			string configFile = currentInfo.ConfigurationFile;
			if (configFile == null || configFile.Length == 0)
				return null;

			return configFile;

		}
	}

	class ConfigurationData
	{
		ConfigurationData parent;
		Hashtable factories;
		string fileName;
		object removedMark = new object ();
		object groupMark = new object ();

		public ConfigurationData () : this (null)
		{
		}

		public ConfigurationData (ConfigurationData parent)
		{
			this.parent = (parent == this) ? null : parent;
			factories = new Hashtable ();
		}

		public bool Load (string fileName)
		{
			if (fileName == null)
				return false;

			this.fileName = fileName;
			XmlTextReader reader = null;

			try {
				try {
					FileStream fs = new FileStream (fileName, FileMode.Open, FileAccess.Read);
					reader = new XmlTextReader (fs);
				} catch (Exception ex) {
					return false;
				}

				InitRead (reader);
				ReadConfigFile (reader);
			} finally {
				if (reader != null)
					reader.Close();
			}

			return true;
		}

		object GetHandler (string sectionName)
		{
			object o = factories [sectionName];
			if (o == null || o == removedMark) {
				if (parent != null)
					return parent.GetHandler (sectionName);

				return null;
			}

			if (o is IConfigurationSectionHandler)
				return (IConfigurationSectionHandler) o;

			Type t = Type.GetType ((string) o);
			if (t == null)
				throw new ConfigurationException ("Cannot get Type for " + o);

			Type iconfig = typeof (IConfigurationSectionHandler);
			if (!iconfig.IsAssignableFrom (t))
				throw new ConfigurationException (sectionName + " does not implement " + iconfig);
			
			o = Activator.CreateInstance (t, true);
			if (o == null)
				throw new ConfigurationException ("Cannot get instance for " + t);

			factories [sectionName] = o;
			
			return o;
		}

		//TODO: Should use XPath when it works properly for this.
		XmlDocument GetDocumentForSection (string sectionName)
		{
			XmlTextReader reader = null;
			try {
				FileStream fs = new FileStream (fileName, FileMode.Open, FileAccess.Read);
				reader = new XmlTextReader (fs);
			} catch {
				return null;
			}

			ConfigXmlDocument doc = new ConfigXmlDocument ();
			InitRead (reader);
			string [] sectionPath = sectionName.Split ('/');
			int i = 0;
			if (!reader.EOF) {
				if (reader.Name == "configSections")
					reader.Skip ();

				while (!reader.EOF) {
					if (reader.NodeType == XmlNodeType.Element &&
					    reader.Name == sectionPath [i]) {
						if (++i == sectionPath.Length) {
							doc.LoadSingleElement (fileName, reader);
							break;
						}
						MoveToNextElement (reader);
						continue;
					}
					reader.Skip ();
					if (reader.NodeType != XmlNodeType.Element)
						MoveToNextElement (reader);
				}
			}

			reader.Close ();
			return doc;
		}
		
		public object GetConfig (string sectionName)
		{
			object handler = GetHandler (sectionName);
			if (handler == null)
				return null;

			if (!(handler is IConfigurationSectionHandler))
				return handler;

			object parentConfig = null;
			if (parent != null)
				parentConfig = parent.GetConfig (sectionName);

			XmlDocument doc = GetDocumentForSection (sectionName);
			if (doc == null || doc.DocumentElement == null) {
				if (parentConfig == null)
					return null;

				return parentConfig;
			}
			
			return ((IConfigurationSectionHandler) handler).Create (parentConfig, null, doc.DocumentElement);
		}

		private object LookForFactory (string key)
		{
			object o = factories [key];
			if (o != null)
				return o;

			if (parent != null)
				return parent.LookForFactory (key);

			return null;
		}
		
		private void InitRead (XmlTextReader reader)
		{
			reader.MoveToContent ();
			if (reader.NodeType != XmlNodeType.Element || reader.Name != "configuration")
				ThrowException ("Configuration file does not have a valid root element", reader);

			if (reader.HasAttributes)
				ThrowException ("Unrecognized attribute in root element", reader);

			MoveToNextElement (reader);
		}

		private void MoveToNextElement (XmlTextReader reader)
		{
			while (reader.Read ()) {
				XmlNodeType ntype = reader.NodeType;
				if (ntype == XmlNodeType.Element)
					return;

				if (ntype != XmlNodeType.Whitespace &&
				    ntype != XmlNodeType.Comment &&
				    ntype != XmlNodeType.SignificantWhitespace &&
				    ntype != XmlNodeType.EndElement)
					ThrowException ("Unrecognized element", reader);
			}
		}

		private void ReadSection (XmlTextReader reader, string sectionName)
		{
			string attName;
			string nameValue = null;
			string typeValue = null;

			while (reader.MoveToNextAttribute ()) {
				attName = reader.Name;
				if (attName == null)
					continue;

				if (attName == "allowLocation" || attName == "allowDefinition")
					continue;

				if (attName == "type")  {
					if (typeValue != null)
						ThrowException ("Duplicated type attribute.", reader);
					typeValue = reader.Value;
					continue;
				}
				
				if (attName == "name")  {
					if (nameValue != null)
						ThrowException ("Duplicated name attribute.", reader);
					nameValue = reader.Value;
					continue;
				}

				ThrowException ("Unrecognized attribute.", reader);
			}

			if (nameValue == null || typeValue == null)
				ThrowException ("Required attribute missing", reader);

			if (sectionName != null)
				nameValue = sectionName + '/' + nameValue;

			reader.MoveToElement();
			object o = LookForFactory (nameValue);
			if (o != null && o != removedMark)
				ThrowException ("Already have a factory for " + nameValue, reader);

			factories [nameValue] = typeValue;
			MoveToNextElement (reader);
		}

		private void ReadRemoveSection (XmlTextReader reader, string sectionName)
		{
			if (!reader.MoveToNextAttribute () || reader.Name != "name")
				ThrowException ("Unrecognized attribute.", reader);

			string removeValue = reader.Value;
			if (removeValue == null || removeValue.Length == 0)
				ThrowException ("Empty name to remove", reader);

			reader.MoveToElement ();

			if (sectionName != null)
				removeValue = sectionName + '/' + removeValue;

			object o = LookForFactory (removeValue);
			if (o != null && o == removedMark)
				ThrowException ("No factory for " + removeValue, reader);

			factories [removeValue] = removedMark;
			MoveToNextElement (reader);
		}

		private void ReadSectionGroup (XmlTextReader reader, string configSection)
		{
			if (!reader.MoveToNextAttribute ())
				ThrowException ("sectionGroup must have a 'name' attribute.", reader);

			if (reader.Name != "name")
				ThrowException ("Unrecognized attribute.", reader);

			if (reader.MoveToNextAttribute ())
				ThrowException ("Unrecognized attribute.", reader);

			string value = reader.Value;
			if (configSection != null)
				value = configSection + '/' + value;

			object o = LookForFactory (value);
			if (o != null && o != removedMark)
				ThrowException ("Already have a factory for " + value, reader);

			factories [value] = groupMark;
			MoveToNextElement (reader);
			ReadSections (reader, value);
		}

		private void ReadSections (XmlTextReader reader, string configSection)
		{
			int depth = reader.Depth;
			while (reader.Depth == depth) {
				string name = reader.Name;
				if (name == "section") {
					ReadSection (reader, configSection);
					continue;
				} 
				
				if (name == "remove") {
					ReadRemoveSection (reader, configSection);
					continue;
				}

				if (name == "clear") {
					if (reader.HasAttributes)
						ThrowException ("Unrecognized attribute.", reader);

					factories.Clear ();
					MoveToNextElement (reader);
					continue;
				}

				if (name == "sectionGroup") {
					ReadSectionGroup (reader, configSection);
					continue;
				}
				

				ThrowException ("Unrecognized element: " + reader.Name, reader);
			}
		}

		private void ReadConfigFile (XmlTextReader reader)
		{
			int depth = reader.Depth;
			while (reader.Depth == depth) {
				string name = reader.Name;
				if (name == "configSections") {
					if (reader.HasAttributes)
						ThrowException ("Unrecognized attribute in configSections element.", reader);
					MoveToNextElement (reader);
					ReadSections (reader, null);
					return;
				}

				MoveToNextElement (reader);
			}
		}
				
		private void ThrowException (string text, XmlTextReader reader)
		{
			throw new ConfigurationException (text, fileName, reader.LineNumber);
		}
	}
}


