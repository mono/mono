//
// System.Drawing.PrinterSettings.cs
//
// Author:
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc
//
using System;

namespace System.Drawing.Printing
{
	/// <summary>
	/// Summary description for PrinterSettings.
	/// </summary>
	/// 
	[Serializable]
	//[ComVisible(false)]
	public class PrinterSettings {//: IClonable
		public PrinterSettings()
		{
		}
		// SUBCLASS
		/// <summary>
		/// Summary description for PaperSourceCollection.
		/// </summary>
		public class PaperSourceCollection {
			public PaperSourceCollection() {
			}
		}
		/// <summary>
		/// Summary description for PaperSizeCollection.
		/// </summary>
		public class PaperSizeCollection {
			public PaperSizeCollection() {
			}
		}
		/// <summary>
		/// Summary description for PrinterResolutionCollection.
		/// </summary>
		public class PrinterResolutionCollection {
			public PrinterResolutionCollection() {
			}
		}
	}
}
