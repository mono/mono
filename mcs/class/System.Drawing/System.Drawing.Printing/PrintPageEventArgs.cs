//
// System.Drawing.PrintPageEventArgs.cs
//
// Author:
//   Dennis Hayes (dennish@Raytek.com)
//   Herve Poussineau (hpoussineau@fr.st)
//
// (C) 2002 Ximian, Inc
//
using System;
using System.Drawing;
namespace System.Drawing.Printing {
	/// <summary>
	/// Summary description for PrintPageEventArgs.
	/// </summary>
	public class PrintPageEventArgs : EventArgs {
		bool cancel;
		Graphics graphics;
		bool hasmorePages;
		Rectangle marginBounds;
		Rectangle pageBounds;
		PageSettings pageSettings;

		public PrintPageEventArgs(Graphics graphics, Rectangle marginBounds,
			Rectangle pageBounds, PageSettings pageSettings) {
			this.graphics = graphics;
			this.marginBounds = marginBounds;
			this.pageBounds = pageBounds;
			this.pageSettings = pageSettings;
		}
		public bool Cancel {
			get{
				return cancel;
			}
			set{
				cancel = value;
			}
		}
		public Graphics Graphics {
			get{
				return graphics;
			}
		}
		public bool HasMorePages {
			get{
				return hasmorePages;
			}
			set{
				hasmorePages = value;
			}
		}
		public Rectangle MarginBounds {
			get{
                return marginBounds;
			}
		}
		public Rectangle PageBounds {
			get{
				return pageBounds;
			}
		}
		public PageSettings PageSettings {
			get{
				return pageSettings;
			}
		}
		
		// used in PrintDocument.Print()
		internal void SetGraphics(Graphics g)
		{
			graphics = g;
		}
}
}
