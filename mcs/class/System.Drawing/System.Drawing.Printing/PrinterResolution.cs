//
// System.Drawing.PrinterResolution.cs
//
// Author:
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc
//
using System;

namespace System.Drawing.Printing {
	/// <summary>
	/// Summary description for PrinterResolution.
	/// </summary>
	public class PrinterResolution {
		public PrinterResolutionKind kind;
		public int x;
		public int y;
		public PrinterResolution(){
			x = -1;
			y = -1;
		}
		public int X{
			get{
				return x;
			}
		}
		public int Y{
			get{
				return y;
			}
		}
//		public int Kind{
//			get{
//				return kind;
//			}
//		}

	}
}
