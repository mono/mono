// GuiResources.cs
// John Barnette (jbarn@httcb.net)
// 
// Copyright (c) 2002 John Barnette

// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.

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
