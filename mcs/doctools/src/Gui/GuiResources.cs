using System;
using System.Drawing;
using System.Reflection;
using System.Resources;

namespace Mono.Doc.Gui
{
	/// <summary>
	/// Summary description for Resources.
	/// </summary>
	public class GuiResources
	{
		private static ResourceManager imageMgr = null;
		private static ResourceManager textMgr  = null;

		// this class cannot be instantiated
		private GuiResources()
		{
		}

		static GuiResources()
		{
			imageMgr = new ResourceManager("Mono.Doc.Gui.ImageResources", Assembly.GetExecutingAssembly());
			textMgr  = new ResourceManager("Mono.Doc.Gui.TextResources", Assembly.GetExecutingAssembly());
		}

		public static Bitmap AssemblyTreeBitmap
		{
			get { return (Bitmap) imageMgr.GetObject("AssemblyTree.Bitmap"); }
		}

		public static Bitmap AboutMonodocBitmap
		{
			get { return (Bitmap) imageMgr.GetObject("AboutMonodoc.Bitmap"); }
		}

		public static Icon OpenBookIcon
		{
			get	{ return (Icon) imageMgr.GetObject("OpenBook.Icon"); }
		}

		public static Icon ClosedBookIcon
		{
			get { return (Icon) imageMgr.GetObject("ClosedBook.Icon"); }
		}

		public static string GetString(string key)
		{
			return textMgr.GetString(key);
		}
	}
}
