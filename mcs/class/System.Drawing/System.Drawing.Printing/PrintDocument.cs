//
// System.Drawing.PrintDocument.cs
//
// Author:
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc
//
using System;

namespace System.Drawing.Printing {
	/// <summary>
	/// Summary description for PrintDocument.
	/// </summary>
	public class PrintDocument{// : Component {
		private PageSettings defaultpagesettings;
		private string documentname;
		private bool originAtMargins; // .NET V1.1 Beta

		//[MonoTODO]
		public PrintDocument() {
			//FIXME: do we need to init defaultpagesetting, or does pagesettings do that?
			documentname = "Document"; //offical default.
		}
		public PageSettings DefaultPageSettings{
			get{
				return defaultpagesettings;
			}
			set{
				defaultpagesettings = value;
			}
		}
		/// <summary>
		/// Name of the document, not the file!
		/// </summary>

		public string DocumentName{
			get{
				return documentname;
			}
			set{
				documentname = value;
			}
		}
		[MonoTODO]
		public PrintController PrintController{
			get{
				throw new NotImplementedException ();				
			}
			set{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public PrinterSettings PrinterSettings{
			get{
				throw new NotImplementedException ();				
			}
			set{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool OriginAtMargins{// .NET V1.1 Beta
			get{
				return originAtMargins;
			}
			set{
				originAtMargins = value;
			}
		}
		public void Print(){
			throw new NotImplementedException ();
		}
		public override string ToString(){
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void OnBeginPrint(PrintEventArgs e){
			//fire the event
		}
		[MonoTODO]
		protected virtual void OnEndPrint(PrintEventArgs e){
			//fire the event
		}
		[MonoTODO]
		protected virtual void OnPrintPage(PrintEventArgs e){
			//fire the event
		}
		[MonoTODO]
		protected virtual void OnQueryPageSettings(PrintEventArgs e){
			//fire the event
		}
		[MonoTODO]
		protected virtual void OnBeginPaint(PrintEventArgs e){
			//fire the event
		}
		public event PrintEventHandler BeginPrint;
		public event PrintEventHandler EndPrint;
		public event PrintEventHandler PrintPage;
		public event PrintEventHandler QuerypageSettings;


	}
}
