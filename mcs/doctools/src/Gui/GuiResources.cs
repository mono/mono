// GuiResources.cs
// John Barnette (jbarn@httcb.net)
// 
// Copyright (c) 2002 John Barnette
//
// This file is part of Monodoc, a multilingual API documentation tool.
//
// Monodoc is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// Monodoc is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Monodoc; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

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

		public static Bitmap ErrorExplosionBitmap
		{
			get { return (Bitmap) imageMgr.GetObject("ErrorExplosion.Bitmap"); }
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
