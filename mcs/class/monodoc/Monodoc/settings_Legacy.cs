using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

#if LEGACY_MODE

namespace Monodoc {
	[Obsolete]
	public class SettingsHandler {
		static string settingsFile;
		static XmlSerializer settingsSerializer = new XmlSerializer (typeof (Settings));
		[Obsolete]
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

		[Obsolete]
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

		[Obsolete]
		public static void Save ()
		{
			EnsureSettingsDirectory ();
			using (FileStream fs = File.Create (settingsFile)){
				settingsSerializer.Serialize (fs, Settings);
			}
		}

		// these can be used for other types of settings to
		[Obsolete]
		public static string Path;

		[Obsolete]
		public static void EnsureSettingsDirectory ()
		{
			DirectoryInfo d = new DirectoryInfo (Path);
			if (!d.Exists)
				d.Create ();
		}
	}

	[Obsolete]
	public class Settings {
		// public to allow serialization
		[Obsolete]
		public bool EnableEditing = true;

		// Last serial number commited
		[Obsolete]
		public int SerialNumber = 0;

		[Obsolete]
		public bool ShowInheritedMembers = false;
		[Obsolete]
		public bool ShowComments = false;

		[Obsolete]
		public string Email;
		[Obsolete]
		public string Key;

		[Obsolete]
		public int LastSeenVersion = -1;

		[Obsolete]
		public static bool RunningGUI = false;

		// fonts for rendering
		[Obsolete]
		public string preferred_font_family = "Sans";
		[Obsolete]
		public double preferred_font_size = 100;
	}
}

#endif

