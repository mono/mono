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
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Authors:
//
//  Alexander Olk	xenomorph2@onlinehome.de
//

// short "how to" if you want to add an other platform handler, etc:
// - first add mime type names and icon names (best is without extension) to MimeIconEngine, for example:
//   MimeIconEngine.AddMimeTypeAndIconName( "inode/directory", "gnome-fs-directory" );
// - next add the icon name (the same as used in AddMimeTypeAndIconName) and the full filename, for example:
//   MimeIconEngine.AddIcon( "gnome-fs-directory", "/opt/gnome/share/icons/gnome/48x48/filesystems/gnome-fs-directory.png" );
//   AddIcon adds the icon to the image lists SmallIconList and LargeIconList
// - provide always a "unknown/unknown" 'mime type' with a default icon for unkown mime types,
//   "desktop/desktop" 'mime type' for the desktop icon, "directory/home" 'mime type for the home dir of the user and so on
//   (look at the default platform handler)
//
// use
// public static int GetIconIndexForFile( string full_filename )
// public static int GetIconIndexForMimeType( string mime_type )
// to get the image index in MimeIconEngine.SmallIcons and MimeIconEngine.LargeIcons
// use
// public static Image GetIconForMimeTypeAndSize( string mime_type, Size size )
// to get the image itself for a mime type with a specific size

using System;
using System.Drawing;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Xml;

namespace System.Windows.Forms
{
	internal enum MimeExtensionHandlerStatus
	{
		OK,
		NO_KDEGLOBALS,
		NO_GNOMECONFIG,
		NO_ICONS,
		NO_MIMELNK
	}
	
	internal enum EPlatformHandler
	{
		Default,
		KDE,
		GNOME
		// Win, Mac OSX...
	}
	
	internal class MimeIconEngine
	{
		public static ImageList SmallIcons = new ImageList();
		public static ImageList LargeIcons = new ImageList();
		
		private static EPlatformHandler platform = EPlatformHandler.Default;
		
		private static IconIndexHash MimeTypeIconIndexHash = new IconIndexHash();
		
		struct IconPath { 
			public string Fullname; 
			public IconPath (string path)
			{
				Fullname = path;
			}
		}

		struct SvgIconPath { 
			public string Fullname; 
			public SvgIconPath (string path)
			{
				Fullname = path;
			}
		}

		private class IconIndexHash {

			Hashtable hash = new Hashtable ();

			private int LoadIcon (IconPath path)
			{
				Bitmap bmp = new Bitmap (path.Fullname);
			
				int index = SmallIcons.Images.Add (bmp, Color.Transparent);
				LargeIcons.Images.Add (bmp, Color.Transparent);
				return index;
			}

			private int LoadSvgIcon (SvgIconPath path)
			{
				Image image = SVGUtil.GetSVGasImage (path.Fullname, 48, 48);
			
				int index = SmallIcons.Images.Add (image, Color.Transparent);
				LargeIcons.Images.Add (image, Color.Transparent);
				return index;
			}

			private int LoadIcon (object path)
			{
				if (path is SvgIconPath)
					return LoadSvgIcon ((SvgIconPath) path);
				else
					return LoadIcon ((IconPath) path);
			}

			public object this [object key] {
				get {
					if (hash [key] == null)
						return null;
					else if (hash [key] is int)
						return hash [key];

					hash [key] = LoadIcon (hash [key]);
					return hash [key];
				}
			}

			public void Add (string name, object path)
			{
				hash [name] = path;
			}

			public bool ContainsKey (string s)
			{
				return hash.ContainsKey (s);
			}
		}

		private static NameValueCollection IconNameMimeTypeNameValueCollection = new NameValueCollection();
		
		private static StringCollection added_icons = new StringCollection();
		
		private static object lock_object = new Object();
		
		static MimeIconEngine( )
		{
			// add some more aliases, kde for example uses other mime type names for some mime types...
			MimeGenerated.Aliases.Add( "application/x-compressed-tar", "application/x-tgz" );
			MimeGenerated.Aliases.Add( "application/x-bzip-compressed-tar", "application/x-tbz" );
			MimeGenerated.Aliases.Add( "application/zip", "application/x-zip" );
			MimeGenerated.Aliases.Add( "text/x-patch", "text/x-diff" );
			
			SmallIcons.ColorDepth = ColorDepth.Depth32Bit;
			SmallIcons.TransparentColor = Color.Transparent;
			LargeIcons.ColorDepth = ColorDepth.Depth32Bit;
			LargeIcons.TransparentColor = Color.Transparent;
			
			string session =  Environment.GetEnvironmentVariable( "DESKTOP_SESSION" );
			
			if ( session != null )
			{
				session = session.ToUpper( );
				
				if ( session == "DEFAULT" )
				{
					string helper = Environment.GetEnvironmentVariable( "KDE_FULL_SESSION" );
					
					if ( helper != null )
						session = "KDE";
					else
					{
						helper = Environment.GetEnvironmentVariable( "GNOME_DESKTOP_SESSION_ID" );
						
						if ( helper != null )
							session = "GNOME";
					}
				}
			}
			else
				session = "";
			
			//Console.WriteLine( "Desktop session is: " + session ); 
			
			PlatformMimeIconHandler platformMimeHandler = null;
			
			if ( session == "KDE" )
			{
				SmallIcons.ImageSize = new Size( 24, 24 );
				LargeIcons.ImageSize = new Size( 48, 48 );
				
				platformMimeHandler = new KdeHandler( );
				if ( platformMimeHandler.Start( ) == MimeExtensionHandlerStatus.OK )
				{
					platform = EPlatformHandler.KDE;
				}
				else // fallback to default
				{
					MimeIconEngine.LargeIcons.Images.Clear( );
					MimeIconEngine.SmallIcons.Images.Clear( );
					platformMimeHandler = new PlatformDefaultHandler( );
					platformMimeHandler.Start( );
				}
			}
			else
			if ( session == "GNOME" )
			{
				SmallIcons.ImageSize = new Size( 24, 24 );
				LargeIcons.ImageSize = new Size( 48, 48 );
				
				platformMimeHandler = new GnomeHandler( );
				if ( platformMimeHandler.Start( ) == MimeExtensionHandlerStatus.OK )
				{
					platform = EPlatformHandler.GNOME;
				}
				else // fallback to default
				{
					MimeIconEngine.LargeIcons.Images.Clear( );
					MimeIconEngine.SmallIcons.Images.Clear( );
					platformMimeHandler = new PlatformDefaultHandler( );
					platformMimeHandler.Start( );
				}
			}
			else
			{
				SmallIcons.ImageSize = new Size( 16, 16 );
				LargeIcons.ImageSize = new Size( 48, 48 );
				
				platformMimeHandler = new PlatformDefaultHandler( );
				platformMimeHandler.Start( );
			}
			
			IconNameMimeTypeNameValueCollection = null;
			added_icons = null;
		}
		
		public static int GetIconIndexForFile( string full_filename )
		{
			lock ( lock_object )
			{
				string mime_type = Mime.GetMimeTypeForFile( full_filename );
				
				if ( platform == EPlatformHandler.Default )
				{
					if ( mime_type == "inode/directory" )
					{
						return (int)MimeTypeIconIndexHash[ "inode/directory" ];
					}
					else
					{
						return (int)MimeTypeIconIndexHash[ "unknown/unknown" ];
					}
				}
				
				object oindex = GetIconIndex( mime_type );
				
				if ( oindex == null )
					oindex = MimeTypeIconIndexHash[ "unknown/unknown" ];
				
				return (int)oindex;
			}
		}
		
		public static int GetIconIndexForMimeType( string mime_type )
		{
			lock ( lock_object )
			{
				if ( platform == EPlatformHandler.Default )
				{
					if ( mime_type == "inode/directory" )
					{
						return (int)MimeTypeIconIndexHash[ "inode/directory" ];
					}
					else
					{
						return (int)MimeTypeIconIndexHash[ "unknown/unknown" ];
					}
				}
				
				object oindex = GetIconIndex( mime_type );
				
				if ( oindex == null )
					oindex = MimeTypeIconIndexHash[ "unknown/unknown" ];
				
				return (int)oindex;
			}
		}
		
		public static Image GetIconForMimeTypeAndSize( string mime_type, Size size )
		{
			lock ( lock_object )
			{
				object oindex = GetIconIndex( mime_type );
				
				if ( oindex == null )
					oindex = MimeTypeIconIndexHash[ "unknown/unknown" ];
				
				Bitmap bmp = new Bitmap( LargeIcons.Images[ (int)oindex ], size );
				
				return bmp;
			}
		}
		
		internal static void AddIcon( string name, string fullname )
		{
			if ( !CheckIfIconIsNeeded( name ) )
				return;
			
			if ( added_icons.Contains( name ) )
				return;
			
			added_icons.Add( name );
			
			AddMimeTypeIconIndexHash( name, new IconPath (fullname) );
		}
		
		internal static void AddSVGIcon( string name, string fullname )
		{
			if ( !CheckIfIconIsNeeded( name ) )
				return;
			
			if ( added_icons.Contains( name ) )
				return;
			
			added_icons.Add( name );
			
			AddMimeTypeIconIndexHash( name, new SvgIconPath (fullname) );
		}
		
		private static bool CheckIfIconIsNeeded( string name )
		{
			string mime_types = IconNameMimeTypeNameValueCollection[ name ];
			
			if ( mime_types != null )
				return true;
			
			return false;
		}
		
		internal static void AddMimeTypeIconIndexHash( string name, object path_or_index )
		{
			string mime_type = IconNameMimeTypeNameValueCollection[ name ];
			
			if ( mime_type == null )
				return;
			
			string[] split = mime_type.Split( new char[] { ',' } );
			
			foreach ( string s in split )
			{
				if ( MimeTypeIconIndexHash.ContainsKey( s ) )
					continue;
				
				MimeTypeIconIndexHash.Add( s, path_or_index );
			}
		}
		
		internal static void AddIconByImage( string name, Image image )
		{
			int index = SmallIcons.Images.Add( image, Color.Transparent );
			LargeIcons.Images.Add( image, Color.Transparent );
			
			AddMimeTypeIconIndexHash( name, index );
		}
		
		internal static void AddMimeTypeAndIconName( string mimetype, string iconname )
		{
			if ( iconname.Equals( String.Empty ) )
				return;
			
			IconNameMimeTypeNameValueCollection.Add( iconname, mimetype );
		}
		
		private static object GetIconIndex( string mime_type )
		{
			object oindex = null;
			
			if ( mime_type != null )
			{
				// first check if mime_type is available in the mimetype/icon hashtable
				oindex = MimeTypeIconIndexHash[ mime_type ];
				
				if ( oindex == null )
				{
					// it is not available, check if an alias exist for mime_type
					string alias = Mime.GetMimeAlias( mime_type );
					
					if ( alias != null )
					{
						string[] split = alias.Split( new char[] { ',' } );
						
						foreach ( string s in split )
						{
							oindex = MimeTypeIconIndexHash[ s ];
							
							if ( oindex != null )
								return oindex;
						}
					}
					
					// if oindex is still null check if mime_type is a sub class of an other mime type
					string sub_class = MimeGenerated.SubClasses[ mime_type ];
					
					if ( sub_class != null )
						return MimeTypeIconIndexHash[ sub_class ];
					
					// last check, see if we find an entry for the main mime type class
					string mime_class_main = mime_type.Substring( 0, mime_type.IndexOf( '/' ) );
					return MimeTypeIconIndexHash[ mime_class_main ];
				}
			}
			
			return oindex;
		}
	}
	
	internal abstract class PlatformMimeIconHandler
	{
		protected StringCollection mime_paths = new StringCollection();
		
		protected StringCollection icon_paths = new StringCollection();
		
		protected string icon_theme = "";
		
		protected MimeExtensionHandlerStatus mimeExtensionHandlerStatus = MimeExtensionHandlerStatus.OK;
		
		public MimeExtensionHandlerStatus MimeExtensionHandlerStatus
		{
			get {
				return mimeExtensionHandlerStatus;
			}
		}
		
		public abstract MimeExtensionHandlerStatus Start( );
		
		// check, if icon, mime, etc., directories exist
		protected virtual bool CheckPlatformDirectories( )
		{
			return true;
		}
	}
	
	internal class PlatformDefaultHandler : PlatformMimeIconHandler
	{
		public override MimeExtensionHandlerStatus Start( )
		{
			MimeIconEngine.AddMimeTypeAndIconName( "unknown/unknown", "paper" );
			MimeIconEngine.AddMimeTypeAndIconName( "inode/directory", "folder" );
			MimeIconEngine.AddMimeTypeAndIconName( "desktop/desktop", "desktop" );
			MimeIconEngine.AddMimeTypeAndIconName( "directory/home", "folder_with_paper" );
			MimeIconEngine.AddMimeTypeAndIconName( "network/network", "monitor-planet" );
			MimeIconEngine.AddMimeTypeAndIconName( "recently/recently", "last_open" );
			MimeIconEngine.AddMimeTypeAndIconName( "workplace/workplace", "monitor-computer" );
			
			MimeIconEngine.AddIconByImage( "folder",  (Image)Locale.GetResource( "folder" ) );
			MimeIconEngine.AddIconByImage( "paper",  (Image)Locale.GetResource( "paper" ) );
			MimeIconEngine.AddIconByImage( "desktop",  (Image)Locale.GetResource( "desktop" ) );
			MimeIconEngine.AddIconByImage( "folder_with_paper",  (Image)Locale.GetResource( "folder_with_paper" ) );
			MimeIconEngine.AddIconByImage( "monitor-planet",  (Image)Locale.GetResource( "monitor-planet" ) );
			MimeIconEngine.AddIconByImage( "last_open",  (Image)Locale.GetResource( "last_open" ) );
			MimeIconEngine.AddIconByImage( "monitor-computer",  (Image)Locale.GetResource( "monitor-computer" ) );
			
			return MimeExtensionHandlerStatus.OK; // return always ok
		}
	}
	
	internal class KdeHandler : PlatformMimeIconHandler
	{
		string full_kdegloabals_filename = Environment.GetFolderPath( Environment.SpecialFolder.Personal )
		+ "/"
		+ ".kde/share/config/kdeglobals";
		
		public override MimeExtensionHandlerStatus Start( )
		{
			if ( !ReadKdeglobals( ) )
				return mimeExtensionHandlerStatus;
			
			if ( !CheckPlatformDirectories( ) )
				return mimeExtensionHandlerStatus;
			
			// check if the theme is svg only
			// if true, use theme "default.kde"
			// don't know if that is available in every linux distribution
			if ( SVGOnly( ) )
				icon_theme = "default.kde";
			else
			// check if there is a /48x48 directory
			if( No48x48( ) )
				icon_theme = "default.kde";
			
			ReadMimetypes( );
			
			ReadIcons( );
			
			return mimeExtensionHandlerStatus;
		}
		
		private bool SVGOnly( )
		{
			// check only the first path in icon_paths
			if ( icon_paths.Count > 0 )
			{
				string icon_path = icon_paths[ 0 ] + icon_theme;
				string[] dirs = Directory.GetDirectories( icon_path );
				
				if ( dirs.Length == 1 && dirs[ 0 ] == "scalable" )
					return true;
			}
			
			return false;
		}
		
		private bool No48x48( )
		{
			// check only the first path in icon_paths
			if ( icon_paths.Count > 0 )
			{
				string icon_path = icon_paths[ 0 ] + icon_theme;
				string[] dirs = Directory.GetDirectories( icon_path );
				
				foreach( string path in dirs )
				{
					if ( path.EndsWith( "48x48" ) )
						return false;
				}
			}
			
			return true;
		}
		
		protected override bool CheckPlatformDirectories( )
		{
			bool icons_found = false;
			
			// default icon dirs
			if ( Directory.Exists( "/opt/kde3/share/icons/default.kde" ) )
			{
				icon_paths.Add( "/opt/kde3/share/icons" + "/" );
				icons_found = true;
			}
			else
			if ( Directory.Exists( "/usr/share/icons/default.kde" ) )
			{
				icon_paths.Add( "/usr/share/icons" + "/" );
				icons_found = true;
			}
			else
			if ( Directory.Exists( "/usr/local/share/icons/default.kde" ) )
			{
				icon_paths.Add( "/usr/local/share/icons"  + "/" );
				icons_found = true;
			}
			else
			if ( !icons_found )
			{
				mimeExtensionHandlerStatus = MimeExtensionHandlerStatus.NO_ICONS;
				return false;
			}
			
			bool mimelnk_found = false;
			
			if ( Directory.Exists( "/usr/share/mimelnk" ) )
			{
				mime_paths.Add( "/usr/share/mimelnk" + "/" );
				mimelnk_found = true;
			}
			
			if ( Directory.Exists( "/usr/local/share/mimelnk" ) )
			{
				mime_paths.Add( "/usr/local/share/mimelnk" + "/" );
				mimelnk_found = true;
			}
			
			if ( Directory.Exists( "/opt/kde3/share/mimelnk" ) )
			{
				mime_paths.Add( "/opt/kde3/share/mimelnk" + "/" );
				mimelnk_found = true;
			}
			
			if ( !mimelnk_found )
			{
				mimeExtensionHandlerStatus = MimeExtensionHandlerStatus.NO_MIMELNK;
				return false;
			}
			
			return true;
		}
		
		private void ReadIcons( )
		{
			foreach ( string icon_path_in in icon_paths )
			{
				string icon_path = icon_path_in + icon_theme + "/48x48";
				
				string[] directories = Directory.GetDirectories( icon_path );
				
				foreach ( string d in directories )
				{
					DirectoryInfo di = new DirectoryInfo( d );
					
					FileInfo[] fileinfo = di.GetFiles( );
					
					foreach ( FileInfo fi in fileinfo )
					{
						string name = Path.GetFileNameWithoutExtension( fi.Name );
						
						MimeIconEngine.AddIcon( name, fi.FullName );
					}
				}
			}
		}
		
		private void ReadMimetypes( )
		{
			MimeIconEngine.AddMimeTypeAndIconName( "unknown/unknown", "unknown" );
			MimeIconEngine.AddMimeTypeAndIconName( "desktop/desktop", "desktop" );
			MimeIconEngine.AddMimeTypeAndIconName( "directory/home", "folder_home" );
			MimeIconEngine.AddMimeTypeAndIconName( "network/network", "network" );
			MimeIconEngine.AddMimeTypeAndIconName( "recently/recently", "folder_man" );
			MimeIconEngine.AddMimeTypeAndIconName( "workplace/workplace", "system" );
			
			MimeIconEngine.AddMimeTypeAndIconName( "nfs/nfs", "nfs_mount" );
			MimeIconEngine.AddMimeTypeAndIconName( "smb/smb", "server" );
			
			MimeIconEngine.AddMimeTypeAndIconName( "harddisk/harddisk", "hdd_mount" );
			MimeIconEngine.AddMimeTypeAndIconName( "cdrom/cdrom", "cdrom_mount" );
			MimeIconEngine.AddMimeTypeAndIconName( "removable/removable", "usbpendrive_mount" );
			
			foreach ( string mime_path in mime_paths )
			{
				string[] directories = Directory.GetDirectories( mime_path );
				
				foreach ( string d in directories )
				{
					string[] files = Directory.GetFiles( d );
					
					foreach ( string f in files )
					{
					    try {
						ReadDotDesktop( f );
					    } catch {
						// Ignore errors if the file can not be read.
					    }
					}
				}
			}
		}
		
		private void ReadDotDesktop( string filename )
		{
			StreamReader sr = new StreamReader( filename );
			
			string line = sr.ReadLine( );
			
			string icon_name = "";
			
			string mime_type = "";
			
			bool have_icon = false;
			bool have_mimetype = false;
			
			while ( line != null )
			{
				line = line.Trim( );
				
				if ( line.StartsWith( "Icon" ) )
				{
					icon_name = line.Substring( line.IndexOf( '=' ) + 1 );
					icon_name = icon_name.Trim( );
					if ( have_mimetype )
						break;
					have_icon = true;
				}
				else
				if ( line.StartsWith( "MimeType" ) )
				{
					mime_type = line.Substring( line.IndexOf( '=' ) + 1 );
					mime_type = mime_type.Trim( );
					if ( have_icon )
						break;
					have_mimetype = true;
				}
				
				line = sr.ReadLine( );
			}
			
			sr.Close( );
			
			MimeIconEngine.AddMimeTypeAndIconName( mime_type, icon_name );
		}
		
		private bool ReadKdeglobals( )
		{
			if ( !File.Exists( full_kdegloabals_filename ) )
			{
				mimeExtensionHandlerStatus = MimeExtensionHandlerStatus.NO_KDEGLOBALS;
				
				return false;
			}
			
			StreamReader sr = new StreamReader( full_kdegloabals_filename );
			
			string line = sr.ReadLine( );
			
			while ( line != null )
			{
				if ( line.IndexOf( "[Icons]" ) != -1 )
				{
					line = sr.ReadLine( );
					
					if ( line != null && line.IndexOf( "Theme" ) != -1 )
					{
						line = line.Trim( );
						
						icon_theme = line.Substring( line.IndexOf( '=' ) + 1 );
						
						icon_theme = icon_theme.Trim( );
						
						break;
					}
				}
				
				line = sr.ReadLine( );
			}
			
			sr.Close( );
			
			return true;
		}
	}
	
	internal class GnomeHandler : PlatformMimeIconHandler
	{
		string full_gnome_gconf_tree = Environment.GetFolderPath( Environment.SpecialFolder.Personal )
		+ "/"
		+ ".gconf/%gconf-tree.xml";
		
		bool is_svg_icon_theme = false;
		
		string main_icon_theme_path;
		
		StringCollection inherits_path_collection = new StringCollection ();
		
		public override MimeExtensionHandlerStatus Start( )
		{
			icon_theme = String.Empty;
			
			if (!File.Exists (full_gnome_gconf_tree))
				full_gnome_gconf_tree = Environment.GetFolderPath( Environment.SpecialFolder.Personal )
					+ "/"
					+ ".gconf/desktop/gnome/interface/%gconf.xml";
			
			GetIconThemeFromGConf ();
			
			if (!GetIconPaths ())
				return MimeExtensionHandlerStatus.NO_ICONS;
			
			if (!GetMainIconThemePath ())
				return MimeExtensionHandlerStatus.NO_ICONS;
			
			GetIconThemeInherits ();
			
			CreateUIIcons ();
			
			CreateMimeTypeIcons( );
			
			inherits_path_collection = null;
			icon_paths = null;
			
			return MimeExtensionHandlerStatus.OK;
		}
		
		private bool GetIconThemeFromGConf ()
		{
			if (!File.Exists (full_gnome_gconf_tree))
				return false;
			
			try {
				bool found_icon_theme_in_xml = false;
				
				XmlTextReader xtr = new XmlTextReader (full_gnome_gconf_tree);
				
				while (xtr.Read ()) {
					if (xtr.NodeType == XmlNodeType.Element && xtr.Name.ToUpper () == "ENTRY" && xtr.GetAttribute ("name") == "icon_theme") {
						found_icon_theme_in_xml = true;
					} else
					if (xtr.NodeType == XmlNodeType.Element && xtr.Name.ToUpper () == "STRINGVALUE" && found_icon_theme_in_xml) {
						xtr.Read ();
						icon_theme = xtr.Value;
						break;
					}
				}
				xtr.Close ();
				
				if (icon_theme != String.Empty)
					return true;
				else {
					icon_theme = "gnome";
					return false;
				}
			} catch ( Exception e ) {
				return false;
			}
		}
		
		private bool GetIconPaths () 
		{
			string global_icon_path = "";
			
			if (Directory.Exists ("/opt/gnome/share/icons"))
				global_icon_path = "/opt/gnome/share/icons";
			else
			if (Directory.Exists ("/usr/share/icons"))
				global_icon_path = "/usr/share/icons";
			else
			if (Directory.Exists ("/usr/local/share/icons"))
				global_icon_path = "/usr/local/share/icons";
			
			if (global_icon_path.Length > 0)
				icon_paths.Add (global_icon_path);
			
			if (Directory.Exists (Environment.GetFolderPath (Environment.SpecialFolder.Personal) + "/.icons"))
				icon_paths.Add (Environment.GetFolderPath (Environment.SpecialFolder.Personal) + "/.icons");
			
			if (icon_paths.Count == 0)
				return false;
			
			return true;
		}
		
		private bool GetMainIconThemePath ()
		{
			foreach (string path in icon_paths) {
				if (Directory.Exists (path + "/" + icon_theme)) {
					main_icon_theme_path = path + "/" + icon_theme;
					return true;
				}
			}
			
			return false;
		}
		
		private void GetIconThemeInherits ()
		{
			inherits_path_collection.Add (main_icon_theme_path);
			GetIndexThemeInherits (main_icon_theme_path + "/" + "index.theme");
		}
		
		private void GetIndexThemeInherits (string filename)
		{
			StringCollection tmp_inherits = new StringCollection ();
			
			try {
				StreamReader sr = new StreamReader (filename);
				
				string line = sr.ReadLine ();
				
				while (line != null) {
					if (line.IndexOf ("Inherits=") != -1) {
						line = line.Trim ();
						line = line.Replace ("Inherits=", "");
						
						line = line.Trim ();
						
						string[] split = line.Split (new char [] { ',' });
						
						tmp_inherits.AddRange (split);
						break;
					}
					line = sr.ReadLine ();
				}
				
				sr.Close ();
			} catch (Exception e) {
				
			}
			
			if (tmp_inherits.Count > 0) {
				foreach (string icon_theme in tmp_inherits) {
					foreach (string path in icon_paths) {
						if (Directory.Exists (path + "/" + icon_theme)) {
							if (!inherits_path_collection.Contains (path + "/" + icon_theme))
								inherits_path_collection.Add (path + "/" + icon_theme);
							if (File.Exists (path + "/" + icon_theme + "/" + "index.theme"))
								GetIndexThemeInherits (path + "/" + icon_theme + "/" + "index.theme");
							break;
						}
					}
				}
			}
		}
		
		private void CreateUIIcons ()
		{
			string resolv_path = ResolvePath (main_icon_theme_path);
			
			// use default gnome icon theme if there isn't a "/48x48" or "/scalable" dir
			// for the current theme
			if (resolv_path == String.Empty)
				foreach (string path in icon_paths)
					if (Directory.Exists (path + "/gnome")) {
						resolv_path = path + "/gnome/48x48/";
						break;
					}
			
			string[] dirs = Directory.GetDirectories (resolv_path);
			
			Hashtable name_mime_hash = new Hashtable ();
			
			name_mime_hash ["gnome-fs-directory"] = "inode/directory";
			name_mime_hash ["gnome-fs-regular"] = "unknown/unknown";
			name_mime_hash ["gnome-fs-desktop"] = "desktop/desktop";
			name_mime_hash ["gnome-fs-home"] = "directory/home";
			name_mime_hash ["gnome-fs-network"] = "network/network";
			name_mime_hash ["gnome-fs-directory-accept"] = "recently/recently";
			name_mime_hash ["gnome-fs-client"] = "workplace/workplace";
			
			name_mime_hash ["gnome-fs-nfs"] = "nfs/nfs";
			name_mime_hash ["gnome-fs-smb"] = "smb/smb";
			
			name_mime_hash ["gnome-dev-cdrom"] = "cdrom/cdrom";
			name_mime_hash ["gnome-dev-harddisk"] = "harddisk/harddisk";
			name_mime_hash ["gnome-dev-removable"] = "removable/removable";
			
			if (!CheckAndAddUIIcons (dirs, name_mime_hash)) {
				//could be a kde icon theme, so we check kde icon names also
				name_mime_hash.Clear ();
				name_mime_hash ["folder"] = "inode/directory";
				name_mime_hash ["unknown"] = "unknown/unknown";
				name_mime_hash ["desktop"] = "desktop/desktop";
				name_mime_hash ["folder_home"] = "directory/home";
				name_mime_hash ["network"] = "network/network";
				name_mime_hash ["folder_man"] = "recently/recently";
				name_mime_hash ["system"] = "workplace/workplace";
				
				name_mime_hash ["nfs_mount"] = "nfs/nfs";
				name_mime_hash ["server"] = "smb/smb";
				
				name_mime_hash ["cdrom_mount"] = "cdrom/cdrom";
				name_mime_hash ["hdd_mount"] = "harddisk/harddisk";
				name_mime_hash ["usbpendrive_mount"] = "removable/removable";
				
				CheckAndAddUIIcons (dirs, name_mime_hash);
			}
		}
		
		private bool CheckAndAddUIIcons (string[] dirs, Hashtable name_mime_hash)
		{
			string extension = is_svg_icon_theme ? "svg" : "png";
			int counter = 0;
			
			foreach (string place in dirs) {
				foreach (DictionaryEntry entry in name_mime_hash) {
					string key = (string)entry.Key;
					if (File.Exists (place + "/" + key + "." + extension)) {
						string value = (string)entry.Value;
						MimeIconEngine.AddMimeTypeAndIconName (value, key);
						if (!is_svg_icon_theme)
							MimeIconEngine.AddIcon (key, place + "/" + key + "." + extension);
						else
							MimeIconEngine.AddSVGIcon (key, place + "/" + key + "." + extension);
						counter++;
						if (counter == name_mime_hash.Count)
							return true;
					}
				}
			}
			
			return false;
		}
		
		private void CreateMimeTypeIcons ()
		{
			foreach (string ip in inherits_path_collection) {
				string path_to_use = ResolvePath (ip);
				
				if (path_to_use == String.Empty)
					continue;
				
				if (!Directory.Exists (path_to_use + "mimetypes"))
				    continue;
				
				string[] files = Directory.GetFiles (path_to_use + "mimetypes");
				
				foreach (string file in files) {
					string extension = Path.GetExtension (file);
					
					if (!is_svg_icon_theme) {
						if (extension != ".png")
							continue;
					} else
					if (extension != ".svg")
						continue;
					
					string file_name = Path.GetFileNameWithoutExtension (file);
					
					if (!file_name.StartsWith ("gnome-mime-"))
						continue;
					
					StringBuilder mime_type = new StringBuilder (file_name.Replace ("gnome-mime-", ""));
					
					for (int i = 0; i < mime_type.Length; i++)
						if (mime_type [i] == '-') {
							mime_type [i] = '/';
							break;
						}
					
					MimeIconEngine.AddMimeTypeAndIconName (mime_type.ToString (), file_name);
					
					if (!is_svg_icon_theme)
						MimeIconEngine.AddIcon (file_name, file);
					else
						MimeIconEngine.AddSVGIcon (file_name, file);
				}
			}
		}
		
		private string ResolvePath (string path)
		{
			if (Directory.Exists (path + "/48x48")) {
				is_svg_icon_theme = false;
				return path + "/48x48/";
			}
			
			if (Directory.Exists (path + "/scalable")) {
				is_svg_icon_theme = true;
				return path + "/scalable/";
			}
			
			return String.Empty;
		}
	}
	
	internal class SVGUtil {
		[DllImport("librsvg-2.so")]
		static extern IntPtr rsvg_pixbuf_from_file_at_size (string file_name, int  width, int  height, out IntPtr error);
		
		[DllImport("libgdk-x11-2.0.so")]
		static extern bool gdk_pixbuf_save_to_buffer (IntPtr pixbuf, out IntPtr buffer, out UIntPtr buffer_size, string type, out IntPtr error, IntPtr option_dummy);
		
		[DllImport("libglib-2.0.so")]
		static extern void g_free (IntPtr mem);
		
		[DllImport("libgdk-x11-2.0.so")]
		static extern bool gdk_init_check(out int argc, string argv);
		
		[DllImport("libgobject-2.0.so")]
		static extern void g_object_unref (IntPtr nativeObject);
		
		static bool inited = false;
		
		static void Init () {
			int argc = 0;
			string argv = "";
			
			gdk_init_check (out argc, argv);
			
			inited = true;
		}
		
		public static Image GetSVGasImage (string filename, int width, int height) {
			if (!inited)
				Init ();
			
			if (!File.Exists (filename))
				return null;
			IntPtr error = IntPtr.Zero;
			IntPtr pixbuf = rsvg_pixbuf_from_file_at_size (filename, width, height, out error);
			
			if (error != IntPtr.Zero)
				return null;
			
			error = IntPtr.Zero;
			IntPtr buffer;
			UIntPtr buffer_size_as_ptr;
			string type = "png";
			
			bool saved = gdk_pixbuf_save_to_buffer (pixbuf, out buffer, out buffer_size_as_ptr, type, out error, IntPtr.Zero);
			
			if (!saved)
				return null;
			
			int buffer_size = (int) (uint) buffer_size_as_ptr;
			byte[] result = new byte [buffer_size];
			Marshal.Copy (buffer, result, 0, (int) buffer_size);
			g_free (buffer);
			g_object_unref (pixbuf);
			
			Image image = null;
			using (MemoryStream s = new MemoryStream (result))
				image = Image.FromStream (s);
			
			return image;
		}
	}
}

