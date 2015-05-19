//
// Mono.Web.Util.SettingsMappingManager
//
// Authors:
//   Marek Habersack (mhabersack@novell.com)
//
// (C) 2007 Novell, Inc
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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.Configuration;
using System.Xml;
using System.Xml.XPath;

namespace Mono.Web.Util
{
	public class SettingsMappingManager
	{
		const string settingsMapFileName = "settings.map";
		const string localSettingsMapFileName = settingsMapFileName + ".config";

		static object mapperLock = new object ();
		
		static SettingsMappingManager _instance;
		static string _mappingFile;
		Dictionary <Type, SettingsMapping> _mappers;
		static Dictionary <object, object> _mappedSections;
		static SettingsMappingPlatform _myPlatform;

		static bool _runningOnWindows;
		internal static bool IsRunningOnWindows {
                        get { return _runningOnWindows; }
                }

		public static SettingsMappingPlatform Platform {
			get { return _myPlatform; }
		}
		
		public bool HasMappings {
			get { return (_mappers != null && _mappers.Count > 0); }
		}
		
		static SettingsMappingManager ()
		{
			_mappingFile = Path.Combine (Path.GetDirectoryName (RuntimeEnvironment.SystemConfigurationFile), settingsMapFileName);
			PlatformID pid = Environment.OSVersion.Platform;
			_runningOnWindows = ((int) pid != 128 && (int) pid != 4 && (int) pid != 6);

		}

		static public void Init ()
		{
			if (_instance != null)
				return;
			
			if (Environment.GetEnvironmentVariable ("MONO_ASPNET_INHIBIT_SETTINGSMAP") != null)
				return;
				
			NameValueCollection appSettings = WebConfigurationManager.AppSettings;
			if (appSettings != null) {
				string inhibit = appSettings ["MonoAspnetInhibitSettingsMap"];
				if (String.Compare (inhibit, "true", StringComparison.OrdinalIgnoreCase) == 0)
					return;
			}

			if (IsRunningOnWindows)
				_myPlatform = SettingsMappingPlatform.Windows;
			else
				_myPlatform = SettingsMappingPlatform.Unix;
		
			SettingsMappingManager mapper = new SettingsMappingManager ();
			mapper.LoadMappings ();

			if (mapper.HasMappings) {
				_instance = mapper;
				_mappedSections = new Dictionary <object, object> ();
			}
		}
		
		void LoadMappings ()
		{
			if (File.Exists (_mappingFile))
				LoadMappings (_mappingFile);

			AppDomainSetup domain = AppDomain.CurrentDomain.SetupInformation;
			string appMappingFile = Path.Combine (domain.ApplicationBase, localSettingsMapFileName);
			if (File.Exists (appMappingFile))
				LoadMappings (appMappingFile);
		}
		
		void LoadMappings (string mappingFilePath)
		{
			XPathNavigator top;
			XPathDocument doc;
			try {
				doc = new XPathDocument (mappingFilePath);
				top = doc.CreateNavigator ();
			} catch (Exception ex) {
				throw new ApplicationException ("Error loading mapping settings", ex);
			}
			
			XPathNodeIterator iter;
			if (_mappers == null)
				_mappers = new Dictionary <Type, SettingsMapping> ();
			else {
				iter = top.Select ("//settingsMap/clear");
				if (iter.MoveNext ())
					_mappers.Clear ();
			}

			iter = top.Select ("//settingsMap/map[string-length (@sectionType) > 0 and string-length (@mapperType) > 0 and string-length (@platform) > 0]");
			SettingsMapping map;
			
			while (iter.MoveNext ()) {
				map = new SettingsMapping (iter.Current);
				if (_myPlatform != map.Platform)
					continue;
				
				if (!_mappers.ContainsKey (map.SectionType))
					_mappers.Add (map.SectionType, map);
				else
					_mappers [map.SectionType] = map;
			}      
		}
		
		public static object MapSection (object input)
		{
			if (_instance == null || input == null)
				return input;

			object mappedSection;
			if (_mappedSections.TryGetValue (input, out mappedSection))
				return mappedSection;
			
			object ret = _instance.MapSection (input, input.GetType ());
			lock (mapperLock) {
				if (ret != null && !_mappedSections.ContainsKey (ret))
					_mappedSections.Add (ret, ret);
			}
			
			return ret;
		}

		object MapSection (object input, Type type)
		{
			if (_mappers == null || _mappers.Count == 0 || !_mappers.ContainsKey (type))
				return input;
      
			SettingsMapping map;
			if (!_mappers.TryGetValue (type, out map))
				return input;
			
			if (map == null)
				return input;
      
			return map.MapSection (input, type);
		}
	}
}
