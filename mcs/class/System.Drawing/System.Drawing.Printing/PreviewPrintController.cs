//
// System.Drawing.PreviewPrintControler.cs
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
	/// Summary description for PreviewPrintControler.
	/// </summary>
	public class PreviewPrintController : PrintController
	{
		private bool useantialias;
		public PreviewPrintControler()
		{
			useantialias = false;
		}
//		//[MonoTODO]
//		public override void OnEndPage(PrintDocument document, PrintPageEventArgs e){
//			throw new NotImplementedException ();
//		}
//		//[MonoTODO]
//		public override void OnStartPrint(PrintDocument document, PrintPageEventArgs e){
//			throw new NotImplementedException ();
//		}
//		//[MonoTODO]
//		public override void OnEndPrint(PrintDocument document, PrintPageEventArgs e){
//			throw new NotImplementedException ();
//		}
		//[MonoTODO]
		//public override Graphics OnStartPage(PrintDocument document, PrintPageEventArgs e){
		//	throw new NotImplementedException ();
		//}
		
		public bool UseAntiAlias {
			get{
				return useantialias;
			}
			set{
				useantialias = value;
			}
		}
		//[MonoTODO]
		public PreviewPageInfo [] GetPreviewPageInfo(){
			throw new NotImplementedException ();
		}

	}
}
