// AssemblyTreeImages.cs
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
using System.Windows.Forms;

namespace Mono.Doc.Gui
{
	/// <summary>
	/// Summary description for AssemblyBrowserImages.
	/// </summary>
	public class AssemblyTreeImages
	{
		private static Image[] images      = null;
		private static ImageList imageList = null;

		// this class cannot be instantiated
		private AssemblyTreeImages()
		{
		}

		static AssemblyTreeImages()
		{
			Bitmap b       = GuiResources.AssemblyTreeBitmap;
			int count      = (int) b.Width / b.Height;
			images         = new Image[count];
			Rectangle rect = new Rectangle(0, 0, b.Height, b.Height);
			imageList      = new ImageList();

			for (int j = 0; j < count; j++)
			{
				images[j] = b.Clone(rect, b.PixelFormat);
				rect.X   += b.Height;

				imageList.Images.Add(images[j]);
			}
		}
		
		// access as an ImageList for TreeView, ToolBar
		public static ImageList List  { get { return imageList; } }

		public static Image AssemblyClosedImage { get { return images[AssemblyClosed];  } }
		public static Image AssemblyOpenImage   { get { return images[AssemblyOpen];    } }
		public static Image NamespaceImage      { get { return images[Namespace];       } }
		public static Image ClassImage          { get { return images[Class];           } }
		public static Image InterfaceImage      { get { return images[Interface];       } }
		public static Image StructImage         { get { return images[Struct];          } }
		public static Image EnumImage           { get { return images[Enum];            } }
		public static Image ConstructorImage    { get { return images[Constructor];     } }
		public static Image MethodImage         { get { return images[Method];          } }
		public static Image EventImage          { get { return images[Event];           } }
		public static Image PropertyImage       { get { return images[Property];        } }
		public static Image DelegateImage       { get { return images[Delegate];        } }
		public static Image OperatorImage       { get { return images[Operator];        } }
		public static Image FieldImage          { get { return images[Field];           } }
		public static Image ShortcutsImage      { get { return images[Shortcuts];       } }

		// imageList indexes
		public static int AssemblyClosed = 0;
		public static int AssemblyOpen   = 1;
		public static int Namespace      = 2;
		public static int Class          = 3;
		public static int Interface      = 4;
		public static int Struct         = 5;
		public static int Enum           = 6;
		public static int Constructor    = 7;
		public static int Method         = 8;
		public static int Event          = 9;
		public static int Property       = 10;
		public static int Delegate       = 11;
		public static int Operator       = 12;
		public static int Field          = 13;
		public static int Shortcuts      = 14;
	}
}
