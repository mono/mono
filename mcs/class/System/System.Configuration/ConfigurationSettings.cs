//
// System.Configuration.ConfigurationSettings.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
//   Eric Lindvall (eric@5stops.com)
//
// (c) Christopher Podurgiel
// (c) 2002 Ximian, Inc. (http://www.ximian.com)
// (c) 2003 Novell, Inc. (http://www.novell.com)
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
			lock (typeof (ConfigurationSettings)) {
				if (config == null)
					config = DefaultConfig.GetInstance ();
			}

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

		// Invoked from System.Web
		static IConfigurationSystem ChangeConfigurationSystem (IConfigurationSystem newSystem)
		{
			if (newSystem == null)
				throw new ArgumentNullException ("newSystem");

			lock (typeof (ConfigurationSettings)) {
				IConfigurationSystem old = config;
				config = newSystem;
				return old;
			}
		}
	}

	//
	// class DefaultConfig: read configuration from machine.config file and application
	// config file if available.
	//
	class DefaultConfig : IConfigurationSystem
	{
		static DefaultConfig instance;
		ConfigurationData config;

		static DefaultConfig ()
		{
			instance = new DefaultConfig ();
		}

		private DefaultConfig ()
		{
		}

		public static DefaultConfig GetInstance ()
		{
			return instance;
		}

		public object GetConfig (string sectionName)
		{
			Init ();
			return config.GetConfig (sectionName);
		}

		public void Init ()
		{
			lock (this) {
				if (config != null)
					return;

				ConfigurationData data = new ConfigurationData ();
				if (!data.Load (GetMachineConfigPath ()))
					throw new ConfigurationException ("Cannot find " + GetMachineConfigPath ());

				string appfile = GetAppConfigPath ();
				if (appfile == null) {
					config = data;
					return;
				}

				ConfigurationData appData = new ConfigurationData (data);
				if (appData.Load (appfile))
					config = appData;
				else
					config = data;
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern private static string get_machine_config_path ();

		internal static string GetMachineConfigPath ()
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

        //
        // TODO: this should be changed to use the FileSystemWatcher
        //
        //  -eric@5stops.com 9.20.2003
        //
        class FileWatcherCache
        {
                Hashtable cacheTable;
		DateTime lastWriteTime;
                string filename;
		static TimeSpan seconds = new TimeSpan (0, 0, 2);

                public FileWatcherCache (string filename)
                {
                        cacheTable = Hashtable.Synchronized (new Hashtable ());
                        lastWriteTime = new FileInfo (filename).LastWriteTime;
                        this.filename = filename;
                }

                void CheckFileChange ()
                {
			FileInfo info = new FileInfo (filename);

			if (!info.Exists) {
				lastWriteTime = DateTime.MinValue;
				cacheTable.Clear ();
				return;
			}

			DateTime writeTime = info.LastWriteTime;
			TimeSpan ts = (info.LastWriteTime - lastWriteTime);
			if (ts >= seconds) {
				lastWriteTime = writeTime;
				cacheTable.Clear ();
			}
                }

		public object this [string key] {
			get {
				CheckFileChange ();
				return cacheTable [key];
			}

			set {
				CheckFileChange();
				cacheTable [key] = value;
			}
		}
        }

	enum AllowDefinition
	{
		Everywhere,
		MachineOnly,
		MachineToApplication
	}
	
	class SectionData
	{
		public readonly string SectionName;
		public readonly string TypeName;
		public readonly bool AllowLocation;
		public readonly AllowDefinition AllowDefinition;
		public string FileName;

		public SectionData (string sectionName, string typeName,
				    bool allowLocation, AllowDefinition allowDefinition)
		{
			SectionName = sectionName;
			TypeName = typeName;
			AllowLocation = allowLocation;
			AllowDefinition = allowDefinition;
		}
	}
	

	class ConfigurationData
	{
		ConfigurationData parent;
		Hashtable factories;
		Hashtable pending;
		string fileName;
		static object removedMark = new object ();
		static object groupMark = new object ();
                static object emptyMark = new object ();
                FileWatcherCache fileCache;

                FileWatcherCache FileCache {
                        get {
				lock (this) {
					if (fileCache != null)
						return fileCache;

					fileCache = new FileWatcherCache (fileName);
                                }

                                return fileCache;
                        }
                }

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
			this.fileName = fileName;
			if (fileName == null || !File.Exists (fileName))
				return false;

			XmlTextReader reader = null;

			try {
				FileStream fs = new FileStream (fileName, FileMode.Open, FileAccess.Read);
				reader = new XmlTextReader (fs);
				InitRead (reader);
				ReadConfigFile (reader);
			} catch (ConfigurationException) {
				throw;
			} catch (Exception e) {
				throw new ConfigurationException ("Error reading " + fileName, e);
			} finally {
				if (reader != null)
					reader.Close();
			}

			return true;
		}

		object GetHandler (string sectionName)
		{
			lock (factories) {
				object o = factories [sectionName];
				if (o == null || o == removedMark) {
					if (parent != null)
						return parent.GetHandler (sectionName);

					return null;
				}

				if (o is IConfigurationSectionHandler)
					return (IConfigurationSectionHandler) o;

				o = CreateNewHandler (sectionName, (SectionData) o);
				factories [sectionName] = o;
				return o;
			}
		}

		object CreateNewHandler (string sectionName, SectionData section)
		{
			Type t = Type.GetType (section.TypeName);
			if (t == null)
				throw new ConfigurationException ("Cannot get Type for " + section.TypeName);

			Type iconfig = typeof (IConfigurationSectionHandler);
			if (!iconfig.IsAssignableFrom (t))
				throw new ConfigurationException (sectionName + " does not implement " + iconfig);
			
			object o = Activator.CreateInstance (t, true);
			if (o == null)
				throw new ConfigurationException ("Cannot get instance for " + t);

			return o;
		}

		XmlDocument GetInnerDoc (XmlDocument doc, int i, string [] sectionPath)
		{
			if (++i >= sectionPath.Length)
				return doc;

			if (doc.DocumentElement == null)
				return null;

			XmlNode node = doc.DocumentElement.FirstChild;
			while (node != null) {
				if (node.Name == sectionPath [i]) {
					ConfigXmlDocument result = new ConfigXmlDocument ();
					result.Load (new StringReader (node.OuterXml));
					return GetInnerDoc (result, i, sectionPath);
				}
				node = node.NextSibling;
			}

			return null;
		}

		XmlDocument GetDocumentForSection (string sectionName)
		{
			ConfigXmlDocument doc = new ConfigXmlDocument ();
			if (pending == null)
				return doc;

			string [] sectionPath = sectionName.Split ('/');
			string outerxml = pending [sectionPath [0]] as string;
			if (outerxml == null)
				return doc;
			
			doc.Load (new StringReader (outerxml));
			return GetInnerDoc (doc, 0, sectionPath);
		}
		
		object GetConfigInternal (string sectionName)
		{
			object handler = GetHandler (sectionName);
			IConfigurationSectionHandler iconf = handler as IConfigurationSectionHandler;
			if (iconf == null)
				return handler;

			object parentConfig = null;
			if (parent != null)
				parentConfig = parent.GetConfig (sectionName);

			XmlDocument doc = GetDocumentForSection (sectionName);
			if (doc == null || doc.DocumentElement == null)
				return parentConfig;
			
			return iconf.Create (parentConfig, fileName, doc.DocumentElement);
		}

		public object GetConfig (string sectionName)
		{
                        object config = this.FileCache [sectionName];

                        if (config == emptyMark)
                                return null;

                        if (config != null)
                                return config;

			lock (this) {
				config = GetConfigInternal (sectionName);
				this.FileCache [sectionName] = (config == null) ? emptyMark : config;
			}

			return config;
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
			string allowLoc = null, allowDef = null;
			bool allowLocation = true;
			AllowDefinition allowDefinition = AllowDefinition.Everywhere;

			while (reader.MoveToNextAttribute ()) {
				attName = reader.Name;
				if (attName == null)
					continue;

				if (attName == "allowLocation") {
					if (allowLoc != null)
						ThrowException ("Duplicated allowLocation attribute.", reader);

					allowLoc = reader.Value;
					allowLocation = (allowLoc == "true");
					if (!allowLocation && allowLoc != "false")
						ThrowException ("Invalid attribute value", reader);

					continue;
				}

				if (attName == "allowDefinition") {
					if (allowDef != null)
						ThrowException ("Duplicated allowDefinition attribute.", reader);

					allowDef = reader.Value;
					try {
						allowDefinition = (AllowDefinition) Enum.Parse (
								   typeof (AllowDefinition), allowDef);
					} catch {
						ThrowException ("Invalid attribute value", reader);
					}

					continue;
				}

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
					if (nameValue == "location")
						ThrowException ("location is a reserved section name", reader);
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
			SectionData section = new SectionData (nameValue, typeValue, allowLocation, allowDefinition);
			section.FileName = fileName;
			factories [nameValue] = section;

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
			if (value == "location")
				ThrowException ("location is a reserved section name", reader);
			
			if (configSection != null)
				value = configSection + '/' + value;

			object o = LookForFactory (value);
			if (o != null && o != removedMark && o != groupMark)
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

		void StorePending (string name, XmlTextReader reader)
		{
			if (pending == null)
				pending = new Hashtable ();

			pending [name] = reader.ReadOuterXml ();
		}

		private void ReadConfigFile (XmlTextReader reader)
		{
			int depth = reader.Depth;
			while (!reader.EOF && reader.Depth == depth) {
				string name = reader.Name;
				if (name == "configSections") {
					if (reader.HasAttributes)
						ThrowException ("Unrecognized attribute in <configSections>.", reader);

					MoveToNextElement (reader);
					ReadSections (reader, null);
				} else if (name != null && name != "") {
					StorePending (name, reader);
					MoveToNextElement (reader);
				} else {
					MoveToNextElement (reader);
				}
			}
		}
				
		private void ThrowException (string text, XmlTextReader reader)
		{
			throw new ConfigurationException (text, fileName, reader.LineNumber);
		}
	}
}


