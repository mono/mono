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
			throw new NotImplementedException ();
		}
		[MonoTODO]
			public PrintControllerWithStatusDialog(PrintController underlyingController, string dialogTitle)
		{
			throw new NotImplementedException ();
		}

		//
		//  --- Public Methods
		//
		[MonoTODO]
		public override bool Equals(object o)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override int GetHashCode() {
			//FIXME add our proprities
			return base.GetHashCode();
		}
		[MonoTODO]
		public override void OnEndPage(PrintDocument document, PrintPageEventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public override void OnEndPrint(PrintDocument document, PrintEventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public override Graphics OnStartPage(PrintDocument document, PrintPageEventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public override void OnStartPrint(PrintDocument document, PrintEventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public override string ToString()
		{
			throw new NotImplementedException ();
		}

		//
		//  --- Destructor
		//
		[MonoTODO]
		~PrintControllerWithStatusDialog()
		{
			throw new NotImplementedException ();
		}
	 }
}
