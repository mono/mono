//
// System.Drawing.PrintPageEventArgs.cs
//
// Author:
//   Dennis Hayes (dennish@Raytek.com)
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
		//Graphics graphics;
		bool hasmorePages;
		//Rectangle marginBounds;
		//Rectangle pageBounds;
		PageSettings pageSettings;

//		public PrintPageEventArgs(Graphics graphics, rectangle marginBounds,
//			Rectangle pageBounds, PageSettings pageSettings) {
//		}
		public bool Cancel {
			get{
				return cancel;
			}
			set{
				cancel = value;
			}
		}
//		public Graphics Graphics {
//			get{
//				return graphics;
//			}
//			set{
//				graphics = value;
//			}
//		}
		public bool HasMorePages {
			get{
				return HasMorePages;
			}
			set{
				HasMorePages = value;
			}
		}
//		public Rectangle MarginBounds {
//			get{
//                return marginBounds;
//			}
//			set{
//				marginBounds = value;
//			}
//		}
//		public Rectangle PageBounds {
//			get{
//				return pageBounds;
//			}
//			set{
//				pageBounds = value;
//			}
//		}
		public PageSettings PageSettings {
			get{
				return pageSettings;
			}
			set{
				pageSettings = value;
			}
		}
}
}
