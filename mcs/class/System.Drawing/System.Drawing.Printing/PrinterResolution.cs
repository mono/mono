//
// System.Drawing.Printing.PrinterResolution.cs
//
// Author:
//   Dennis Hayes (dennish@Raytek.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002 Ximian, Inc
// (C) 2003 Andreas Nahr
//
using System;

namespace System.Drawing.Printing 
{

	public class PrinterResolution 
	{
		private PrinterResolutionKind kind;
		private int x;
		private int y;

		private PrinterResolution () 
		{
		}

		internal PrinterResolution (int x, int y, PrinterResolutionKind kind)
		{
			this.x = x;
			this.y = y;
			this.kind = kind;
		}

		public int X {
			get {
				return x;
			}
		}

		public int Y {
			get {
				return y;
			}
		}

		public PrinterResolutionKind Kind {
			get {
				return kind;
			}
		}
	}
}
