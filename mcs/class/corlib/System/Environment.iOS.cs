// Copyright 2014 Xamarin Inc.

// note: or older hack to give the Documents (or Library) directories 

using System.IO;
using System.Runtime.InteropServices;

namespace System {

	public static partial class Environment {

		static string ns_document;
		static string ns_library;

		[DllImport ("__Internal")]
		extern static string xamarin_GetFolderPath (int folder);

		static string NSDocumentDirectory {
			get {
				if (ns_document == null) {
#if MONOTOUCH_TV
					// The "normal" NSDocumentDirectory is a read-only directory on tvOS
					// and that breaks a lot of assumptions in the runtime and the BCL
					// to avoid this we relocate the Documents directory under Caches
					ns_document = Path.Combine (NSLibraryDirectory, "Caches", "Documents");
					if (!Directory.Exists (ns_document))
						Directory.CreateDirectory (ns_document);
#else
					ns_document = xamarin_GetFolderPath (/* NSDocumentDirectory */ 9);
#endif
				}
				return ns_document;
			}
		}

		// Various user-visible documentation, support, and configuration files
		static string NSLibraryDirectory {
			get {
				if (ns_library == null)
					ns_library = xamarin_GetFolderPath (/* NSLibraryDirectory */ 5);
				return ns_library;
			}
		}

		public static string GetFolderPath (SpecialFolder folder, SpecialFolderOption option)
		{
			return UnixGetFolderPath (folder, option);
		}

		// needed by our BCL, e.g. IsolatedStorageFile.cs
		internal static string UnixGetFolderPath (SpecialFolder folder, SpecialFolderOption option)
		{
			var dir = iOSGetFolderPath (folder);
			if ((option == SpecialFolderOption.Create) && !Directory.Exists (dir))
				Directory.CreateDirectory (dir);
			return dir;
		}

		internal static string iOSGetFolderPath (SpecialFolder folder)
		{
			switch (folder) {
			case SpecialFolder.MyComputer:
			case SpecialFolder.Programs:
			case SpecialFolder.SendTo:
			case SpecialFolder.StartMenu:
			case SpecialFolder.Startup:
			case SpecialFolder.Cookies:
			case SpecialFolder.History:
			case SpecialFolder.Recent:
			case SpecialFolder.CommonProgramFiles:
			case SpecialFolder.System:
			case SpecialFolder.NetworkShortcuts:
			case SpecialFolder.CommonStartMenu:
			case SpecialFolder.CommonPrograms:
			case SpecialFolder.CommonStartup:
			case SpecialFolder.CommonDesktopDirectory:
			case SpecialFolder.PrinterShortcuts:
			case SpecialFolder.Windows:
			case SpecialFolder.SystemX86:
			case SpecialFolder.ProgramFilesX86:
			case SpecialFolder.CommonProgramFilesX86:
			case SpecialFolder.CommonDocuments:
			case SpecialFolder.CommonAdminTools:
			case SpecialFolder.AdminTools:
			case SpecialFolder.CommonMusic:
			case SpecialFolder.CommonPictures:
			case SpecialFolder.CommonVideos:
			case SpecialFolder.LocalizedResources:
			case SpecialFolder.CommonOemLinks:
			case SpecialFolder.CDBurning:
				return String.Empty;
			
			// personal == ~
			case SpecialFolder.Personal:
			case SpecialFolder.LocalApplicationData:
				return NSDocumentDirectory;

			case SpecialFolder.ApplicationData:
				// note: at first glance that looked like a good place to return NSLibraryDirectory 
				// but it would break isolated storage for existing applications
				return Path.Combine (NSDocumentDirectory, ".config");

			case SpecialFolder.Resources:
				return NSLibraryDirectory; // older (8.2 and previous) would return String.Empty

			case SpecialFolder.Desktop:
			case SpecialFolder.DesktopDirectory:
				return Path.Combine (NSDocumentDirectory, "Desktop");

			case SpecialFolder.MyMusic:
				return Path.Combine (NSDocumentDirectory, "Music");

			case SpecialFolder.MyPictures:
				return Path.Combine (NSDocumentDirectory, "Pictures");

			case SpecialFolder.Templates:
				return Path.Combine (NSDocumentDirectory, "Templates");

			case SpecialFolder.MyVideos:
				return Path.Combine (NSDocumentDirectory, "Videos");

			case SpecialFolder.CommonTemplates:
				return "/usr/share/templates";

			case SpecialFolder.Fonts:
				return Path.Combine (NSDocumentDirectory, ".fonts");

			case SpecialFolder.Favorites:
				return Path.Combine (NSLibraryDirectory, "Favorites");

			case SpecialFolder.ProgramFiles:
				return "/Applications";

			case SpecialFolder.InternetCache:
				return Path.Combine (NSLibraryDirectory, "Caches");

			case SpecialFolder.UserProfile:
				return internalGetHome ();

			case SpecialFolder.CommonApplicationData:
				return "/usr/share";

			default:
				throw new ArgumentException ("Invalid SpecialFolder");
			}
		}
	}
}