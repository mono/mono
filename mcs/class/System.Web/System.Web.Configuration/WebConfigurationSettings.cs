//
// System.Configuration.WebConfigurationSettings.cs
//
// Authors:
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2003,2004 Novell, Inc. (http://www.novell.com)
//

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

using System;
using System.Configuration;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Runtime.Remoting;
using System.Web.Util;
using System.Xml;

namespace System.Web.Configuration
{
	class WebConfigurationSettings
	{
		static IConfigurationSystem oldConfig;
		static WebDefaultConfig config;
		static string machineConfigPath;
		const BindingFlags privStatic = BindingFlags.NonPublic | BindingFlags.Static;
			
		private WebConfigurationSettings ()
		{
		}

		public static void Init ()
		{
			lock (typeof (WebConfigurationSettings)) {
				if (config != null)
					return;

				WebDefaultConfig settings = WebDefaultConfig.GetInstance ();
				Type t = typeof (ConfigurationSettings);
				MethodInfo changeConfig = t.GetMethod ("ChangeConfigurationSystem",
								      privStatic);

				if (changeConfig == null)
					throw new ConfigurationException ("Cannot find method CCS");

				object [] args = new object [] {settings};
				oldConfig = (IConfigurationSystem) changeConfig.Invoke (null, args);
				config = settings;
			}
		}

		public static void Init (HttpContext context)
		{
			Init ();
			config.Init (context);
		}
		
		public static object GetConfig (string sectionName)
		{
			return config.GetConfig (sectionName);
		}

		public static object GetConfig (string sectionName, HttpContext context)
		{
			return config.GetConfig (sectionName, context);
		}

		public static string MachineConfigPath {
			get {
				lock (typeof (WebConfigurationSettings)) {
					if (machineConfigPath != null)
						return machineConfigPath;

					if (config == null)
						Init ();

					Type t = oldConfig.GetType ();
					MethodInfo getMC = t.GetMethod ("GetMachineConfigPath",
									privStatic);

					if (getMC == null)
						throw new ConfigurationException ("Cannot find method GMC");

					machineConfigPath = (string) getMC.Invoke (null, null);
					return machineConfigPath;
				}
			}
		}
	}

	//
	// class WebDefaultConfig: read configuration from machine.config file and application
	// config file if available.
	//
	class WebDefaultConfig : IConfigurationSystem
	{
		static WebDefaultConfig instance;
		Hashtable fileToConfig;
		HttpContext firstContext;
		bool initCalled;

		static WebDefaultConfig ()
		{
			instance = new WebDefaultConfig ();
		}

		private WebDefaultConfig ()
		{
			fileToConfig = new Hashtable ();
		}

		public static WebDefaultConfig GetInstance ()
		{
			return instance;
		}

		public object GetConfig (string sectionName)
		{
			HttpContext current = HttpContext.Current;
			if (current == null)
				current = firstContext;
			return GetConfig (sectionName, current);
		}

		public object GetConfig (string sectionName, HttpContext context)
		{
			if (context == null)
				return null;

			ConfigurationData config = GetConfigFromFileName (context.Request.CurrentExecutionFilePath, context);
			if (config == null)
				return null;

			return config.GetConfig (sectionName, context);
		}

		ConfigurationData GetConfigFromFileName (string filepath, HttpContext context)
		{
			if (filepath == "")
				return (ConfigurationData) fileToConfig [WebConfigurationSettings.MachineConfigPath];

			string dir = UrlUtils.GetDirectory (filepath);
			if (HttpRuntime.AppDomainAppVirtualPath.Length > dir.Length)
				return  (ConfigurationData) fileToConfig [WebConfigurationSettings.MachineConfigPath];

			ConfigurationData data = (ConfigurationData) fileToConfig [dir];
			if (data != null)
				return data;

			string realpath = context.Request.MapPath (dir);
			string lower = Path.Combine (realpath, "web.config");
			bool isLower = File.Exists (lower);
			string wcfile = null;
			if (!isLower) {
				string upper = Path.Combine (realpath, "Web.config");
				bool isUpper = File.Exists (upper);
				if (isUpper)
					wcfile = upper;
			} else {
				wcfile = lower;
			}

			string tempDir = dir;
			if (tempDir == HttpRuntime.AppDomainAppVirtualPath ||
			    tempDir + "/" == HttpRuntime.AppDomainAppVirtualPath) {
				tempDir = "";
				realpath = HttpRuntime.AppDomainAppPath;
			}

			ConfigurationData parent = GetConfigFromFileName (tempDir, context);
			if (wcfile == null) {
				data = new ConfigurationData (parent, null, realpath);
				data.DirName = dir;
				fileToConfig [dir] = data;
			}

			if (data == null) {
				data = new ConfigurationData (parent, wcfile);
				data.DirName = dir;
				data.LoadFromFile (wcfile);
				fileToConfig [dir] = data;
				RemotingConfiguration.Configure (wcfile);
			}

			return data;
		}

		public void Init ()
		{
			// nothing. We need a context.
		}

		public void Init (HttpContext context)
		{
			if (initCalled)
				return;

			lock (this) {
				if (initCalled)
					return;

				firstContext = context;
				ConfigurationData data = new ConfigurationData ();
				if (!data.LoadFromFile (WebConfigurationSettings.MachineConfigPath))
					throw new ConfigurationException ("Cannot find " + WebConfigurationSettings.MachineConfigPath);

				fileToConfig [WebConfigurationSettings.MachineConfigPath] = data;
				initCalled = true;
			}
		}

		static string GetAppConfigPath ()
		{
			AppDomainSetup currentInfo = AppDomain.CurrentDomain.SetupInformation;

			string configFile = currentInfo.ConfigurationFile;
			if (configFile == null || configFile.Length == 0)
				return null;

			return configFile;

		}
	}

        class FileWatcherCache
        {
                Hashtable cacheTable;
		string path;
		string filename;
		FileSystemWatcher watcher;
		ConfigurationData data;

                public FileWatcherCache (ConfigurationData data)
                {
			this.data = data;
			cacheTable = new Hashtable ();
			this.path = Path.GetDirectoryName (data.FileName);
			this.filename = Path.GetFileName (data.FileName);
			if (!Directory.Exists (path))
				return;

			watcher = new FileSystemWatcher (this.path, this.filename);
			FileSystemEventHandler handler = new FileSystemEventHandler (SetChanged);
			watcher.Created += handler;
			watcher.Changed += handler;
			watcher.Deleted += handler;
			watcher.EnableRaisingEvents = true;
                }

		void SetChanged (object o, FileSystemEventArgs args)
		{
			lock (data) {
				cacheTable.Clear ();
				data.Reset ();
				if (args.ChangeType == WatcherChangeTypes.Created)
					RemotingConfiguration.Configure (args.FullPath);

				if (args.ChangeType != WatcherChangeTypes.Deleted)
					data.LoadFromFile (args.FullPath);
			}
		}
		
		public object this [string key] {
			get {
				lock (data)
					return cacheTable [key];
			}

			set {
				lock (data)
					cacheTable [key] = value;
			}
		}

		public void Close ()
		{
			if (watcher != null)
				watcher.EnableRaisingEvents = false;
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
		Hashtable locations;
		string fileName;
		string dirname;
		string realdir;
		static object removedMark = new object ();
		static object groupMark = new object ();
                static object emptyMark = new object ();
                FileWatcherCache fileCache;
		static char [] forbiddenPathChars = new char [] {
					';', '?', ':', '@', '&', '=', '+',
					'$', ',','\\', '*', '\"', '<', '>'
					};

		static string forbiddenStr = "';', '?', ':', '@', '&', '=', '+', '$', ',', '\\', '*', '\"', '<', '>'";

                internal FileWatcherCache FileCache {
                        get {
				lock (this) {
					if (fileCache != null)
						return fileCache;

					fileCache = new FileWatcherCache (this);
                                }

                                return fileCache;
                        }
                }

		internal string FileName {
			get { return fileName; }
		}

		internal ConfigurationData Parent {
			get { return parent; }
		}

		internal string DirName {
			get { return dirname; }
			set { dirname = value; }
		}

		internal void Reset ()
		{
			factories.Clear ();
			if (pending != null)
				pending.Clear ();

			if (locations != null)
				locations.Clear ();
		}
		
		public ConfigurationData () : this (null, null)
		{
		}

		public ConfigurationData (ConfigurationData parent, string filename)
		{
			this.parent = (parent == this) ? null : parent;
			this.fileName = filename;
			factories = new Hashtable ();
		}

		public ConfigurationData (ConfigurationData parent, string filename, string realdir)
		{
			this.parent = (parent == this) ? null : parent;
			this.realdir = realdir;
			if (filename == null) {
				this.fileName = Path.Combine (realdir, "*.config");
			} else {
				this.fileName = filename;
			}
			factories = new Hashtable ();
		}

		public bool LoadFromFile (string fileName)
		{
			this.fileName = fileName;
			if (fileName == null || !File.Exists (fileName))
				return false;

			XmlTextReader reader = null;

			try {
				FileStream fs = new FileStream (fileName, FileMode.Open, FileAccess.Read);
				reader = new XmlTextReader (fs);
				InitRead (reader);
				ReadConfig (reader, false);
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

		public void LoadFromReader (XmlTextReader reader, string fakeFileName, bool isLocation)
		{
			fileName = fakeFileName;
			MoveToNextElement (reader);
			ReadConfig (reader, isLocation);
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
			
			StringReader reader = new StringReader (outerxml);
			XmlTextReader rd = new XmlTextReader (reader);
			rd.MoveToContent ();
			doc.LoadSingleElement (fileName, rd);

			return GetInnerDoc (doc, 0, sectionPath);
		}
		
		object GetConfigInternal (string sectionName, HttpContext context, bool useLoc)
		{
			object handler = GetHandler (sectionName);
			IConfigurationSectionHandler iconf = handler as IConfigurationSectionHandler;
			if (iconf == null)
				return handler;

			object parentConfig = null;
			if (parent != null) {
				if (useLoc)
					parentConfig = parent.GetConfig (sectionName, context);
				else
					parentConfig = parent.GetConfigOptLocation (sectionName, context, false);
			}

			XmlDocument doc = GetDocumentForSection (sectionName);
			if (doc == null || doc.DocumentElement == null)
				return parentConfig;

			return iconf.Create (parentConfig, fileName, doc.DocumentElement);
		}

		public object GetConfig (string sectionName, HttpContext context)
		{
			if (locations != null && dirname != null) {
				string reduced = UrlUtils.MakeRelative (context.Request.CurrentExecutionFilePath, dirname);
				string [] parts = reduced.Split ('/');
				Location location = null;
				int length = parts.Length;

				string target = null;
				for (int i = 0; i < parts.Length; i++) {
					if (target == null)
						target = parts [i];
					else
						target = target + "/" + parts [i];

					if (locations.ContainsKey (target)) {
						location = locations [target] as Location;
					} else if (locations.ContainsKey (target + "/*")) {
						location = locations [target + "/*"] as Location;
					}
				}
				
				if (location == null) {
					location = locations ["*"] as Location;
				}

				if (location != null && location.Config != null) {
					object o = location.Config.GetConfigOptLocation (sectionName, context, false);
					if (o != null) {
						return o;
					}
				}
			}

			return GetConfigOptLocation (sectionName, context, true);
		}

		object GetConfigOptLocation (string sectionName, HttpContext context, bool useLoc)
		{
			object config = this.FileCache [sectionName];
                        if (config == emptyMark)
                                return null;

                        if (config != null)
                                return config;

			lock (this) {
				config = GetConfigInternal (sectionName, context, useLoc);
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

		internal void MoveToNextElement (XmlTextReader reader)
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

		void StoreLocation (string name, XmlTextReader reader)
		{
			string path = null;
			bool haveAllow = false;
			bool allowOverride = true;
			string att = null;

			while (reader.MoveToNextAttribute ()) {
				att = reader.Name;

				if (att == "path") {
					if (path != null)
						ThrowException ("Duplicate path attribute", reader);

					path = reader.Value;
					if (path.StartsWith ("."))
						ThrowException ("Path cannot begin with '.'", reader);

					if (path.IndexOfAny (forbiddenPathChars) != -1)
						ThrowException ("Path cannot contain " + forbiddenStr, reader);

					continue;
				}

				if (att == "allowOverride") {
					if (haveAllow)
						ThrowException ("Duplicate allowOverride attribute", reader);

					haveAllow = true;
					allowOverride = (reader.Value == "true");
					if (!allowOverride && reader.Value != "false")
						ThrowException ("allowOverride must be either true or false", reader);
					continue;
				}

				ThrowException ("Unrecognized attribute.", reader);
			}

			if (att == null)
				return; // empty location tag

			Location loc = new Location (this, path, allowOverride);
			if (locations == null)
				locations = new Hashtable ();
			else if (locations.ContainsKey (loc.Path))
				ThrowException ("Duplicated location path: " + loc.Path, reader);

			reader.MoveToElement ();
			loc.LoadFromString (reader.ReadInnerXml ());
			locations [loc.Path] = loc;
			if (!loc.AllowOverride) {
				XmlTextReader inner = loc.GetReader ();
				if (inner != null) {
					MoveToNextElement (inner);
					ReadConfig (loc.GetReader (), true);
				}
			}

			loc.XmlStr = null;
		}

		void StorePending (string name, XmlTextReader reader)
		{
			if (pending == null)
				pending = new Hashtable ();

			if (pending.ContainsKey (name))
				ThrowException ("Sections can only appear once: " + name, reader);

			pending [name] = reader.ReadOuterXml ();
		}

		void ReadConfig (XmlTextReader reader, bool isLocation)
		{
			int depth = reader.Depth;
			while (!reader.EOF && reader.Depth == depth) {
				string name = reader.Name;

				if (name == "configSections") {
					if (isLocation)
						ThrowException ("<configSections> inside <location>", reader);

					if (reader.HasAttributes)
						ThrowException ("Unrecognized attribute in <configSections>.", reader);

					MoveToNextElement (reader);
					if (reader.Depth > depth)
						ReadSections (reader, null);
				} else if (name == "location") {
					if (isLocation)
						ThrowException ("<location> inside <location>", reader);

					StoreLocation (name, reader);
					MoveToNextElement (reader);
				} else if (name != null && name != ""){
					StorePending (name, reader);
					MoveToNextElement (reader);
				} else {
					MoveToNextElement (reader);
				}
			}
		}
				
		void ThrowException (string text, XmlTextReader reader)
		{
			throw new ConfigurationException (text, fileName, reader.LineNumber);
		}
	}

	class Location
	{
		string path;
		bool allowOverride;
		ConfigurationData parent;
		ConfigurationData thisOne;
		string xmlstr;

		public Location (ConfigurationData parent, string path, bool allowOverride)
		{
			this.parent = parent;
			this.allowOverride = allowOverride;
			this.path = (path == null || path == "") ? "*" : path;
		}

		public bool AllowOverride {
			get { return (path != "*" || allowOverride); }
		}

		public string Path {
			get { return path; }
		}
		
		public string XmlStr {
			set { xmlstr = value; }
		}
		
		public void LoadFromString (string str)
		{
			if (str == null)
				throw new ArgumentNullException ("str");

			if (thisOne != null)
				throw new InvalidOperationException ();

			this.xmlstr = str.Trim ();
			if (xmlstr == "")
				return;

			XmlTextReader reader = new XmlTextReader (new StringReader (str));
			thisOne = new ConfigurationData (parent, parent.FileName);
			thisOne.LoadFromReader (reader, parent.FileName, true);
		}

		public XmlTextReader GetReader ()
		{
			if (xmlstr == "")
				return null;

			XmlTextReader reader = new XmlTextReader (new StringReader (xmlstr));
			return reader;
		}

		public ConfigurationData Config {
			get { return thisOne; }
		}
	}
}


