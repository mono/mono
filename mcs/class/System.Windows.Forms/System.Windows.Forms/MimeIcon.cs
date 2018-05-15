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
// Copyright (c) 2006 Alexander Olk
//
// Authors:
//
//  Alexander Olk	alex.olk@googlemail.com
//

// use
// public static int GetIconIndexForFile( string full_filename )
// public static int GetIconIndexForMimeType( string mime_type )
// to get the image index in MimeIconEngine.SmallIcons and MimeIconEngine.LargeIcons

using System;
using System.Reflection;
using System.Drawing;
using System.Collections;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Xml;

namespace System.Windows.Forms
{
	internal enum MimeExtensionHandlerStatus
	{
		OK,
		ERROR
	}
	
	internal enum EPlatformHandler
	{
		Default,
		GNOME
	}
	
	internal class ResourceImageLoader
	{
		static Assembly assembly = typeof (ResourceImageLoader).Assembly;
		
		static internal Bitmap Get (string name)
		{
			Stream stream = assembly.GetManifestResourceStream (name);

			if (stream == null) {
				Console.WriteLine ("Failed to read {0}", name);
				return null;
			}
				
			return new Bitmap (stream);
		}
		
		static internal Icon GetIcon (string name)
		{
			Stream stream = assembly.GetManifestResourceStream (name);

			if (stream == null) {
				Console.WriteLine ("Failed to read {0}", name);
				return null;
			}

			return new Icon (stream);
		}
	}
	
	internal class MimeIconEngine
	{
		public static ImageList SmallIcons = new ImageList();
		public static ImageList LargeIcons = new ImageList();
		
		private static EPlatformHandler platform = EPlatformHandler.Default;
		
		internal static Hashtable MimeIconIndex = new Hashtable ();
		
		private static PlatformMimeIconHandler platformMimeHandler = null;
		
		private static object lock_object = new Object();
		
		static MimeIconEngine ()
		{
			SmallIcons.ColorDepth = ColorDepth.Depth32Bit;
			SmallIcons.TransparentColor = Color.Transparent;
			LargeIcons.ColorDepth = ColorDepth.Depth32Bit;
			LargeIcons.TransparentColor = Color.Transparent;
			
			string session =  Environment.GetEnvironmentVariable ("DESKTOP_SESSION");
			
			if (session != null) {
				session = session.ToUpper ();
				
				if (session == "DEFAULT") {
					string helper = Environment.GetEnvironmentVariable ("GNOME_DESKTOP_SESSION_ID");
					
					if (helper != null)
						session = "GNOME";
				}
			} else
				session = String.Empty;
			
			if (Mime.MimeAvailable && session == "GNOME") {
				SmallIcons.ImageSize = new Size (24, 24);
				LargeIcons.ImageSize = new Size (48, 48);
				
				try {
					platformMimeHandler = new GnomeHandler ();
					if (platformMimeHandler.Start () == MimeExtensionHandlerStatus.OK) {
						platform = EPlatformHandler.GNOME;
					} else {
						MimeIconEngine.LargeIcons.Images.Clear ();
						MimeIconEngine.SmallIcons.Images.Clear ();
						platformMimeHandler = new PlatformDefaultHandler ();
						platformMimeHandler.Start ();
					}
				} catch {
					MimeIconEngine.LargeIcons.Images.Clear ();
					MimeIconEngine.SmallIcons.Images.Clear ();
					platformMimeHandler = new PlatformDefaultHandler ();
					platformMimeHandler.Start ();
				}
			} else {
				SmallIcons.ImageSize = new Size (16, 16);
				LargeIcons.ImageSize = new Size (48, 48);
				
				platformMimeHandler = new PlatformDefaultHandler ();
				platformMimeHandler.Start ();
			}
		}
		
		public static int GetIconIndexForFile (string full_filename)
		{
			lock (lock_object) {
				if (platform == EPlatformHandler.Default) {
					return (int)MimeIconIndex ["unknown/unknown"];
				}
				
				string mime_type = Mime.GetMimeTypeForFile (full_filename);
				
				object oindex = GetIconIndex (mime_type);
				
				// not found, add it
				if (oindex == null) {
					int index = full_filename.IndexOf (':');
					
					if (index > 1) {
						oindex = MimeIconIndex ["unknown/unknown"];
						
					} else {
						oindex = platformMimeHandler.AddAndGetIconIndex (full_filename, mime_type);
						
						// sanity check
						if (oindex == null)
							oindex = MimeIconIndex ["unknown/unknown"];
					}
				}
				
				return (int)oindex;
			}
		}
		
		public static int GetIconIndexForMimeType (string mime_type)
		{
			lock (lock_object) {
				if (platform == EPlatformHandler.Default) {
					if (mime_type == "inode/directory") {
						return (int)MimeIconIndex ["inode/directory"];
					} else {
						return (int)MimeIconIndex ["unknown/unknown"];
					}
				}
				
				object oindex = GetIconIndex (mime_type);
				
				// not found, add it
				if (oindex == null) {
					oindex = platformMimeHandler.AddAndGetIconIndex (mime_type);
					
					// sanity check
					if (oindex == null)
						oindex = MimeIconIndex ["unknown/unknown"];
				}
				
				return (int)oindex;
			}
		}
		
		public static Image GetIconForMimeTypeAndSize (string mime_type, Size size)
		{
			lock (lock_object) {
				object oindex = GetIconIndex (mime_type);
				
				Bitmap bmp = new Bitmap (LargeIcons.Images [(int)oindex], size);
				
				return bmp;
			}
		}
		
		internal static void AddIconByImage (string mime_type, Image image)
		{
			int index = SmallIcons.Images.Add (image, Color.Transparent);
			LargeIcons.Images.Add (image, Color.Transparent);
			
			MimeIconIndex.Add (mime_type, index);
		}
		
		private static object GetIconIndex (string mime_type)
		{
			object oindex = null;
			
			if (mime_type != null) {
				// first check if mime_type is available in the mimetype/icon hashtable
				oindex = MimeIconIndex [mime_type];
				
				if (oindex == null) {
					// it is not available, check if an alias exist for mime_type
					string alias = Mime.GetMimeAlias (mime_type);
					
					if (alias != null) {
						string[] split = alias.Split (new char [] { ',' });
						
						for (int i = 0; i < split.Length; i++) {
							oindex = MimeIconIndex [split [i]];
							
							if (oindex != null)
								return oindex;
						}
					}
					
					// if oindex is still null check if mime_type is a sub class of an other mime type
					string sub_class = Mime.SubClasses [mime_type];
					
					if (sub_class != null) {
						oindex = MimeIconIndex [sub_class];
						
						if (oindex != null)
							return oindex;
					}
					
					// last check, see if we find an entry for the main mime type class
					string mime_class_main = mime_type.Substring (0, mime_type.IndexOf ('/'));
					return MimeIconIndex [mime_class_main];
				}
			}
			
			return oindex;
		}
	}
	
	internal abstract class PlatformMimeIconHandler
	{
		protected MimeExtensionHandlerStatus mimeExtensionHandlerStatus = MimeExtensionHandlerStatus.OK;
		
		public MimeExtensionHandlerStatus MimeExtensionHandlerStatus {
			get {
				return mimeExtensionHandlerStatus;
			}
		}
		
		public abstract MimeExtensionHandlerStatus Start ();
		
		public virtual object AddAndGetIconIndex (string filename, string mime_type)
		{
			return null;
		}
		
		public virtual object AddAndGetIconIndex (string mime_type)
		{
			return null;
		}
	}
	
	internal class PlatformDefaultHandler : PlatformMimeIconHandler
	{
		public override MimeExtensionHandlerStatus Start ()
		{
			MimeIconEngine.AddIconByImage ("inode/directory",  ResourceImageLoader.Get ("folder.png"));
			MimeIconEngine.AddIconByImage ("unknown/unknown",  ResourceImageLoader.Get ("text-x-generic.png"));
			MimeIconEngine.AddIconByImage ("desktop/desktop",  ResourceImageLoader.Get ("user-desktop.png"));
			MimeIconEngine.AddIconByImage ("directory/home",  ResourceImageLoader.Get ("user-home.png"));
			
			MimeIconEngine.AddIconByImage ("network/network",  ResourceImageLoader.Get ("folder-remote.png"));
			MimeIconEngine.AddIconByImage ("recently/recently",  ResourceImageLoader.Get ("document-open.png"));
			MimeIconEngine.AddIconByImage ("workplace/workplace",  ResourceImageLoader.Get ("computer.png"));
			
			return MimeExtensionHandlerStatus.OK; // return always ok
		}
	}
	
	internal class GnomeHandler : PlatformMimeIconHandler
	{
		public override MimeExtensionHandlerStatus Start ()
		{
			CreateUIIcons ();
			
			return MimeExtensionHandlerStatus.OK;
		}
		
		private void CreateUIIcons ()
		{
			AddGnomeIcon ("unknown/unknown", "gnome-fs-regular");
			AddGnomeIcon ("inode/directory", "gnome-fs-directory");
			AddGnomeIcon ("directory/home", "gnome-fs-home");
			AddGnomeIcon ("desktop/desktop", "gnome-fs-desktop");
			AddGnomeIcon ("recently/recently", "gnome-fs-directory-accept");
			AddGnomeIcon ("workplace/workplace", "gnome-fs-client");
			
			AddGnomeIcon ("network/network", "gnome-fs-network");
			AddGnomeIcon ("nfs/nfs", "gnome-fs-nfs");
			AddGnomeIcon ("smb/smb", "gnome-fs-smb");
			
			AddGnomeIcon ("harddisk/harddisk", "gnome-dev-harddisk");
			AddGnomeIcon ("cdrom/cdrom", "gnome-dev-cdrom");
			AddGnomeIcon ("removable/removable", "gnome-dev-removable");
		}
		
		private void AddGnomeIcon (string internal_mime_type, string name)
		{
			int index = -1;
			
			if (MimeIconEngine.MimeIconIndex.ContainsKey (internal_mime_type)) {
				return;
			}
			
			Image image = GnomeUtil.GetIcon (name, 48);
			
			if (image == null) {
				if (internal_mime_type == "unknown/unknown")
					image = ResourceImageLoader.Get ("text-x-generic.png");
				else
				if (internal_mime_type == "inode/directory")
					image = ResourceImageLoader.Get ("folder.png");
				else
				if (internal_mime_type == "directory/home")
					image = ResourceImageLoader.Get ("user-home.png");
				else
				if (internal_mime_type == "desktop/desktop")
					image = ResourceImageLoader.Get ("user-desktop.png");
				else
				if (internal_mime_type == "recently/recently")
					image = ResourceImageLoader.Get ("document-open.png");
				else
				if (internal_mime_type == "workplace/workplace")
					image = ResourceImageLoader.Get ("computer.png");
				else
				if (internal_mime_type == "network/network" || internal_mime_type == "nfs/nfs" || internal_mime_type == "smb/smb")
					image = ResourceImageLoader.Get ("folder-remote.png");
				else
				if (internal_mime_type == "harddisk/harddisk" || internal_mime_type == "cdrom/cdrom" || internal_mime_type == "removable/removable")
					image = ResourceImageLoader.Get ("text-x-generic.png");
			}

			if (image != null) {
				index = MimeIconEngine.SmallIcons.Images.Add (image, Color.Transparent);
				MimeIconEngine.LargeIcons.Images.Add (image, Color.Transparent);
				MimeIconEngine.MimeIconIndex.Add (internal_mime_type, index);
			}
		}
		
		public override object AddAndGetIconIndex (string filename, string mime_type)
		{
			int index = -1;
			
			Image image = GnomeUtil.GetIcon (filename, mime_type, 48);
			if (image != null) {
				index = MimeIconEngine.SmallIcons.Images.Add (image, Color.Transparent);
				MimeIconEngine.LargeIcons.Images.Add (image, Color.Transparent);
				MimeIconEngine.MimeIconIndex.Add (mime_type, index);
			}
			
			return index;
		}
		
		public override object AddAndGetIconIndex (string mime_type)
		{
			int index = -1;
			
			Image image = GnomeUtil.GetIcon (mime_type, 48);
			if (image != null) {
				index = MimeIconEngine.SmallIcons.Images.Add (image, Color.Transparent);
				MimeIconEngine.LargeIcons.Images.Add (image, Color.Transparent);
				MimeIconEngine.MimeIconIndex.Add (mime_type, index);
			}
			
			return index;
		}
	}
	
	internal class GnomeUtil
	{
		const string libgdk = "libgdk-x11-2.0";
		const string libgdk_pixbuf = "libgdk_pixbuf-2.0";
		const string libgtk = "libgtk-x11-2.0";
		const string libglib = "libglib-2.0";
		const string libgobject = "libgobject-2.0";
		const string libgnomeui = "libgnomeui-2";
		const string librsvg = "librsvg-2";
		
		[DllImport(librsvg)]
		static extern IntPtr rsvg_pixbuf_from_file_at_size (string file_name, int  width, int  height, out IntPtr error);
		
		[DllImport(libgdk_pixbuf)]
		static extern bool gdk_pixbuf_save_to_buffer (IntPtr pixbuf, out IntPtr buffer, out UIntPtr buffer_size, string type, out IntPtr error, IntPtr option_dummy);
		
		[DllImport(libglib)]
		static extern void g_free (IntPtr mem);
		
		[DllImport(libgdk)]
		static extern bool gdk_init_check (IntPtr argc, IntPtr argv);
		
		[DllImport(libgobject)]
		static extern void g_object_unref (IntPtr nativeObject);
		
		[DllImport(libgnomeui)]
		static extern string gnome_icon_lookup (IntPtr icon_theme, IntPtr thumbnail_factory, string file_uri, string custom_icon, IntPtr file_info, string mime_type, GnomeIconLookupFlags flags, IntPtr result);
		
		[DllImport(libgtk)]
		static extern IntPtr gtk_icon_theme_get_default ();
		
		[DllImport(libgtk)]
		static extern IntPtr gtk_icon_theme_load_icon (IntPtr icon_theme, string icon_name, int size, GtkIconLookupFlags flags, out IntPtr error);
		
		[DllImport(libgtk)]
		static extern bool gtk_icon_theme_has_icon (IntPtr icon_theme, string icon_name);
		
		enum GnomeIconLookupFlags
		{
			GNOME_ICON_LOOKUP_FLAGS_NONE = 0,
			GNOME_ICON_LOOKUP_FLAGS_EMBEDDING_TEXT = 1<<0,
			GNOME_ICON_LOOKUP_FLAGS_SHOW_SMALL_IMAGES_AS_THEMSELVES = 1<<1,
			GNOME_ICON_LOOKUP_FLAGS_ALLOW_SVG_AS_THEMSELVES = 1<<2
		};
		
		enum GtkIconLookupFlags
		{
			GTK_ICON_LOOKUP_NO_SVG = 1 << 0,
			GTK_ICON_LOOKUP_FORCE_SVG = 1 << 1,
			GTK_ICON_LOOKUP_USE_BUILTIN = 1 << 2
		};
		
		static bool inited = false;
		
		static IntPtr default_icon_theme = IntPtr.Zero;
		
		static void Init ()
		{
			gdk_init_check (IntPtr.Zero, IntPtr.Zero);
			
			inited = true;
			
			default_icon_theme = gtk_icon_theme_get_default ();
		}
		
		public static Image GetIcon (string file_name, string mime_type, int size)
		{
			if (!inited)
				Init ();
			
			Uri uri = new Uri (file_name);
	
			string icon;

			try {
				icon = gnome_icon_lookup (default_icon_theme, IntPtr.Zero, uri.AbsoluteUri,
							  null, IntPtr.Zero, mime_type,
							  GnomeIconLookupFlags.GNOME_ICON_LOOKUP_FLAGS_NONE, IntPtr.Zero);
			} catch {
				// If libgnomeui is not installed, in preparation for Gnome 3
				return null;
			}
			
			IntPtr error = IntPtr.Zero;
			IntPtr pixbuf = gtk_icon_theme_load_icon (default_icon_theme, icon, size,
								  GtkIconLookupFlags.GTK_ICON_LOOKUP_USE_BUILTIN, out error);
			
			if (error != IntPtr.Zero)
				return null;
			
			return GdkPixbufToImage (pixbuf);
		}
		
		public static Image GetIcon (string icon, int size)
		{
			if (!inited)
				Init ();
			
			IntPtr error = IntPtr.Zero;
			IntPtr pixbuf = gtk_icon_theme_load_icon (default_icon_theme, icon, size,
								  GtkIconLookupFlags.GTK_ICON_LOOKUP_USE_BUILTIN, out error);
			
			if (error != IntPtr.Zero)
				return null;
			
			return GdkPixbufToImage (pixbuf);
		}
		
		public static Image GdkPixbufToImage (IntPtr pixbuf)
		{
			IntPtr error = IntPtr.Zero;
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
			MemoryStream s = new MemoryStream (result);
			image = Image.FromStream (s);
			
			return image;
		}
		
		public static Image GetSVGasImage (string filename, int width, int height)
		{
			if (!inited)
				Init ();
			
			if (!File.Exists (filename))
				return null;
			IntPtr error = IntPtr.Zero;
			IntPtr pixbuf = rsvg_pixbuf_from_file_at_size (filename, width, height, out error);
			
			if (error != IntPtr.Zero)
				return null;
			
			return GdkPixbufToImage (pixbuf);
		}
		
		public static bool HasImage (string name)
		{
			if (!inited)
				Init ();
			
			return gtk_icon_theme_has_icon (default_icon_theme, name);
		}
	}
}

