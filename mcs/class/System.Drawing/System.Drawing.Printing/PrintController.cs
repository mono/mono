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
	public abstract class PrintController
	{
		public PrintController ()
		{
		}

		[MonoTODO]
		public virtual void OnEndPage (PrintDocument document, PrintPageEventArgs e)
		{
		}

		[MonoTODO]
		public virtual void OnStartPrint (PrintDocument document, PrintEventArgs e)
		{
		}

		[MonoTODO]
		public virtual void OnEndPrint (PrintDocument document, PrintEventArgs e)
		{
		}

		[MonoTODO]
		public virtual Graphics OnStartPage (PrintDocument document, PrintPageEventArgs e)
		{
			throw new NotImplementedException();
			return null;
		}
	}
}
