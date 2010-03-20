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

#if CONFIGURATION_DEP && !TARGET_JVM
extern alias PrebuiltSystem;
using NameValueCollection = PrebuiltSystem.System.Collections.Specialized.NameValueCollection;
#endif

using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
#if (XML_DEP)
using System.Xml;
using System.Xml.XPath;
#endif
#if TARGET_JVM
using vmw.common;
using vmw.@internal.io;
#endif

namespace System.Configuration
{
	public sealed class ConfigurationSettings
	{
#if !TARGET_JVM
     		static IConfigurationSystem config = DefaultConfig.GetInstance ();
#else
		static IConfigurationSystem config {
			get {
				IConfigurationSystem conf = (IConfigurationSystem) AppDomain.CurrentDomain.GetData ("ConfigurationSettings.Config");
				if (conf == null) {
					conf = DefaultConfig.GetInstance ();
					AppDomain.CurrentDomain.SetData ("ConfigurationSettings.Config", conf);
				}
				return conf;
			}
			set {
				AppDomain.CurrentDomain.SetData ("ConfigurationSettings.Config", value);
			}
		}
#endif
		static object lockobj = new object ();
		private ConfigurationSettings ()
		{
		}

#if NET_2_0
		[Obsolete ("This method is obsolete, it has been replaced by System.Configuration!System.Configuration.ConfigurationManager.GetSection")]
#endif
		public static object GetConfig (string sectionName)
		{
#if NET_2_0 && CONFIGURATION_DEP
			return ConfigurationManager.GetSection (sectionName);
#else
			return config.GetConfig (sectionName);
#endif
		}

#if NET_2_0
		[Obsolete ("This property is obsolete.  Please use System.Configuration.ConfigurationManager.AppSettings")]
#endif
		public static NameValueCollection AppSettings
		{
			get {
#if NET_2_0 && CONFIGURATION_DEP
				object appSettings = ConfigurationManager.GetSection ("appSettings");
#else
				object appSettings = GetConfig ("appSettings");
#endif
				if (appSettings == null)
					appSettings = new NameValueCollection ();
				return (NameValueCollection) appSettings;
			}
		}

		// Invoked from System.Web, disable warning
		internal static IConfigurationSystem ChangeConfigurationSystem (IConfigurationSystem newSystem)
		{
			if (newSystem == null)
				throw new ArgumentNullException ("newSystem");

			lock (lockobj) {
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
#if !TARGET_JVM
        	static readonly DefaultConfig instance = new DefaultConfig ();        
#else
		static DefaultConfig instance {
			get {
				DefaultConfig conf = (DefaultConfig) AppDomain.CurrentDomain.GetData ("DefaultConfig.instance");
				if (conf == null) {
					conf = new DefaultConfig ();
					AppDomain.CurrentDomain.SetData ("DefaultConfig.instance", conf);
				}
				return conf;
			}
			set {
				AppDomain.CurrentDomain.SetData ("DefaultConfig.instance", value);
			}
		}
#endif
		ConfigurationData config;
		
		private DefaultConfig ()
		{
		}

		public static DefaultConfig GetInstance ()
		{
			return instance;
		}

#if NET_2_0
		[Obsolete ("This method is obsolete.  Please use System.Configuration.ConfigurationManager.GetConfig")]
#endif
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
				if (data.LoadString (GetBundledMachineConfig ())) {
					// do nothing
				} else {
					if (!data.Load (GetMachineConfigPath ()))
						throw new ConfigurationException ("Cannot find " + GetMachineConfigPath ());

				}
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
#if TARGET_JVM
		internal static string GetBundledMachineConfig ()
		{
			return null;
		}
		internal static string GetMachineConfigPath ()
		{
			return System.Runtime.InteropServices.RuntimeEnvironment.SystemConfigurationFile;
		}
#else
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern private static string get_bundled_machine_config ();
		internal static string GetBundledMachineConfig ()
		{
			return get_bundled_machine_config ();
		}
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern private static string get_machine_config_path ();
		internal static string GetMachineConfigPath ()
		{
			return get_machine_config_path ();
		}
#endif
		private static string GetAppConfigPath ()
		{
			AppDomainSetup currentInfo = AppDomain.CurrentDomain.SetupInformation;

			string configFile = currentInfo.ConfigurationFile;
			if (configFile == null || configFile.Length == 0)
				return null;

			return configFile;

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
#if XML_DEP
		public string FileName;
#endif
		public readonly bool RequirePermission;

		public SectionData (string sectionName, string typeName,
			    bool allowLocation, AllowDefinition allowDefinition, bool requirePermission)
		{
			SectionName = sectionName;
			TypeName = typeName;
			AllowLocation = allowLocation;
			AllowDefinition = allowDefinition;
			RequirePermission = requirePermission;
		}
	}


	class ConfigurationData
	{
		ConfigurationData parent;
		Hashtable factories;
		static object removedMark = new object ();
		static object emptyMark = new object ();
#if (XML_DEP)
		Hashtable pending;
		string fileName;
		static object groupMark = new object ();
#endif
		Hashtable cache;

		Hashtable FileCache {
			get {
				if (cache != null)
					return cache;

				cache = new Hashtable ();
				return cache;
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

		// SECURITY-FIXME: limit this with an imperative assert for reading the specific file
		[FileIOPermission (SecurityAction.Assert, Unrestricted = true)]
		public bool Load (string fileName)
		{
#if (XML_DEP)
			this.fileName = fileName;
			if (fileName == null
#if !TARGET_JVM
				|| !File.Exists (fileName)
#endif
)
				return false;
			
			XmlTextReader reader = null;

			try {
#if !TARGET_JVM
				FileStream fs = new FileStream (fileName, FileMode.Open, FileAccess.Read);
#else
				Stream fs = (Stream) vmw.common.IOUtils.getStream (fileName);

				//patch for machine.config
				if (fs == null && fileName.EndsWith ("machine.config")) {
					fs = (Stream) IOUtils.getStreamForGHConfigs (fileName);
				}

				if (fs == null) {
					return false;
				}
#endif
				reader = new XmlTextReader (fs);
				if (InitRead (reader))
					ReadConfigFile (reader);
			} catch (ConfigurationException) {
				throw;
			} catch (Exception e) {
				throw new ConfigurationException ("Error reading " + fileName, e);
			} finally {
				if (reader != null)
					reader.Close();
			}
#endif
			return true;
		}
		
		public bool LoadString (string data)
		{
			if (data == null)
				return false;
#if (XML_DEP)
			XmlTextReader reader = null;

			try {
				TextReader tr = new StringReader (data);
				reader = new XmlTextReader (tr);
				if (InitRead (reader))
					ReadConfigFile (reader);
			} catch (ConfigurationException) {
				throw;
			} catch (Exception e) {
				throw new ConfigurationException ("Error reading " + fileName, e);
			} finally {
				if (reader != null)
					reader.Close();
			}
#endif
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

#if false
			Type iconfig = typeof (IConfigurationSectionHandler);
			if (!iconfig.IsAssignableFrom (t))
				throw new ConfigurationException (sectionName + " does not implement " + iconfig);
#endif
			
			object o = Activator.CreateInstance (t, true);
			if (o == null)
				throw new ConfigurationException ("Cannot get instance for " + t);

			return o;
		}
#if (XML_DEP)
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
#else
		object GetConfigInternal (string sectionName)
                {
                    return null;
                }
#endif
		public object GetConfig (string sectionName)
		{
			object config;
			lock (this) {
				config = this.FileCache [sectionName];
			}

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
#if (XML_DEP)
		private bool InitRead (XmlTextReader reader)
		{
			reader.MoveToContent ();
			if (reader.NodeType != XmlNodeType.Element || reader.Name != "configuration")
				ThrowException ("Configuration file does not have a valid root element", reader);

			if (reader.HasAttributes)
				ThrowException ("Unrecognized attribute in root element", reader);
			if (reader.IsEmptyElement) {
				reader.Skip ();
				return false;
			}
			reader.Read ();
			reader.MoveToContent ();
			return reader.NodeType != XmlNodeType.EndElement;
		}

		// FIXME: this approach is not always safe and likely to cause bugs.
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
			bool requirePermission = false;
			string requirePer = null;
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

				if (attName == "requirePermission") {
					if (requirePer != null)
						ThrowException ("Duplicated requirePermission attribute.", reader);
					requirePer = reader.Value;
					requirePermission = (requirePer == "true");
					if (!requirePermission && requirePer != "false")
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
			SectionData section = new SectionData (nameValue, typeValue, allowLocation,
				allowDefinition, requirePermission);
			section.FileName = fileName;
			factories [nameValue] = section;

			if (reader.IsEmptyElement)
				reader.Skip ();
			else {
				reader.Read ();
				reader.MoveToContent ();
				if (reader.NodeType != XmlNodeType.EndElement)
					// sub-section inside a section
					ReadSections (reader, nameValue);
				reader.ReadEndElement ();
			}
			reader.MoveToContent ();
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

			string value = null;
			do {
				if (reader.Name == "name") {
					if (value != null)
						ThrowException ("Duplicate 'name' attribute.", reader);
					value = reader.Value;
				}
				else
#if NET_2_0
				if (reader.Name != "type")
#endif
					ThrowException ("Unrecognized attribute.", reader);
			} while (reader.MoveToNextAttribute ());

			if (value == null)
				ThrowException ("No 'name' attribute.", reader);
			
			if (value == "location")
				ThrowException ("location is a reserved section name", reader);

			if (configSection != null)
				value = configSection + '/' + value;

			object o = LookForFactory (value);
			if (o != null && o != removedMark && o != groupMark)
				ThrowException ("Already have a factory for " + value, reader);

			factories [value] = groupMark;

			if (reader.IsEmptyElement) {
				reader.Skip ();
				reader.MoveToContent ();
			} else {
				reader.Read ();
				reader.MoveToContent ();
				if (reader.NodeType != XmlNodeType.EndElement)
					ReadSections (reader, value);
				reader.ReadEndElement ();
				reader.MoveToContent ();
			}
		}

		// It stops XmlReader consumption at where it found
		// surrounding EndElement i.e. EndElement is not consumed here
		private void ReadSections (XmlTextReader reader, string configSection)
		{
			int depth = reader.Depth;
			for (reader.MoveToContent ();
			     reader.Depth == depth;
			     reader.MoveToContent ()) {
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
			//int depth = reader.Depth;
			for (reader.MoveToContent ();
			     !reader.EOF && reader.NodeType != XmlNodeType.EndElement;
			     reader.MoveToContent ()) {
				string name = reader.Name;
				if (name == "configSections") {
					if (reader.HasAttributes)
						ThrowException ("Unrecognized attribute in <configSections>.", reader);
					if (reader.IsEmptyElement)
						reader.Skip ();
					else {
						reader.Read ();
						reader.MoveToContent ();
						if (reader.NodeType != XmlNodeType.EndElement)
							ReadSections (reader, null);
						reader.ReadEndElement ();
					}
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
#endif
	}
}


