//
// System.Windows.Forms.PrintControllerWithStatusDialog
//
// Author:
//   stubbed out by Paul Osman (paul.osman@sympatico.ca)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc
//
using System.Drawing.Printing;
using System.Drawing;
namespace System.Windows.Forms {

	// <summary>
	// </summary>

        public class PrintControllerWithStatusDialog : PrintController {

		//
		//  --- Constructor
		//
		[MonoTODO]
			public PrintControllerWithStatusDialog(PrintController underlyingController)
		{
			
		}
		[MonoTODO]
			public PrintControllerWithStatusDialog(PrintController underlyingController, string dialogTitle)
		{
			
		}

		//
		//  --- Public Methods
		//

		[MonoTODO]
		public override void OnEndPage(PrintDocument document, PrintPageEventArgs e)
		{
			//FIXME:
			base.OnEndPage(document, e);
		}
		[MonoTODO]
		public override void OnEndPrint(PrintDocument document, PrintEventArgs e)
		{
			//FIXME:
			base.OnEndPrint(document, e);
		}
		[MonoTODO]
		public override Graphics OnStartPage(PrintDocument document, PrintPageEventArgs e)
		{
			//FIXME:
			return base.OnStartPage(document, e);
		}
		[MonoTODO]
		public override void OnStartPrint(PrintDocument document, PrintEventArgs e)
		{
			//FIXME:
			base.OnStartPrint(document, e);
		}
	 }
}
