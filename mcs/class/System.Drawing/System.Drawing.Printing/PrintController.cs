//
// System.Drawing.PrintController.cs
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
	/// Summary description for PrintController.
	/// </summary>
	public abstract class PrintController
	{
		public virtual void OnEndPage(PrintDocument document, PrintPageEventArgs e){
		}
		public virtual void OnStartPrint(PrintDocument document, PrintEventArgs e){
		}
		public virtual void OnEndPrint(PrintDocument document, PrintEventArgs e){
		}
		public virtual Graphics OnStartPage(PrintDocument document, PrintPageEventArgs e){
			throw new NotImplementedException();
		}
	}
}
