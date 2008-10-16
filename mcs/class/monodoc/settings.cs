using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Monodoc {

	public class SettingsHandler {
		static string settingsFile;
		static XmlSerializer settingsSerializer = new XmlSerializer (typeof (Settings));
		public static Settings Settings;
		
		static SettingsHandler ()
		{
			string rootDir = Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData);
			Path = System.IO.Path.Combine (rootDir, "monodoc");
			settingsFile = System.IO.Path.Combine (Path, "settings.xml");
			if (File.Exists (settingsFile)) {
				try {
					using (Stream s = File.OpenRead (settingsFile)) {
						Settings = (Settings) settingsSerializer.Deserialize (s);
					}
				} catch {
					Settings = new Settings ();
				}
			} else
				Settings = new Settings ();

			if (Settings.preferred_font_family.Length == 0)
				Settings.preferred_font_family = "Sans";
			if (Settings.preferred_font_size <= 0)
				Settings.preferred_font_size = 100;
		}
		
		public static void CheckUpgrade ()
		{
			// no new version
			if (Settings.LastSeenVersion == RootTree.MonodocVersion)
				return;
			
			// new install
			if (! File.Exists (settingsFile)) {
				Settings.LastSeenVersion = RootTree.MonodocVersion;
				Save ();
				return;
			}
		}

		public static void Save ()
		{
			EnsureSettingsDirectory ();
			using (FileStream fs = File.Create (settingsFile)){
				settingsSerializer.Serialize (fs, Settings);
			}
		}
		
		// these can be used for other types of settings too
		public static string Path;
		public static void EnsureSettingsDirectory ()
		{
			DirectoryInfo d = new DirectoryInfo (Path);
			if (!d.Exists)
				d.Create ();
		}
		
	}
		
	public class Settings {
		// public to allow serialization
		public bool EnableEditing = true;

		// Last serial number commited
		public int SerialNumber = 0;

		public bool ShowInheritedMembers = false;
		public bool ShowComments = false;

		public string Email;
		public string Key;
			
		public int LastSeenVersion = -1;

		public static bool RunningGUI = false;

		// fonts for rendering
		public string preferred_font_family = "Sans";
		public double preferred_font_size = 100;

	}
}

