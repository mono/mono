// AssemblyTreeImages.cs
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
