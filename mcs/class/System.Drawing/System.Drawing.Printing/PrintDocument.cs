//
// System.Drawing.PrintDocument.cs
//
// Author:
//   Dennis Hayes (dennish@Raytek.com)
//   Herve Poussineau (hpoussineau@fr.st)
//
// (C) 2002 Ximian, Inc
//
using System;

namespace System.Drawing.Printing {
	/// <summary>
	/// Summary description for PrintDocument.
	/// </summary>
	public class PrintDocument : System.ComponentModel.Component {
		private PageSettings defaultpagesettings;
		private PrinterSettings printersettings;
		private PrintController printcontroller;
		private string documentname;
#if !(NET_1_0)
		private bool originAtMargins = false; // .NET V1.1 Beta
#endif

		public PrintDocument() {
			documentname = "document"; //offical default.
			defaultpagesettings = new PageSettings(); // use default values of default printer
			printersettings = new PrinterSettings(); // use default values
			printcontroller = new StandardPrintController();
		}
		
// properties
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
		public PrintController PrintController{
			get{
				return printcontroller;
			}
			set{
				printcontroller = value;
			}
		}
		public PrinterSettings PrinterSettings{
			get{
				return printersettings;
			}
			set{
				printersettings = value;
			}
		}
#if !(NET_1_0)
		public bool OriginAtMargins{// .NET V1.1 Beta
			get{
				return originAtMargins;
			}
			set{
				originAtMargins = value;
			}
		}
#endif

// methods
		public void Print(){
			PrintEventArgs printArgs = new PrintEventArgs();
			this.OnBeginPrint(printArgs);
			if (printArgs.Cancel)
				return;
			PrintController.OnStartPrint(this, printArgs);
			if (printArgs.Cancel)
				return;
			
			// while there is more pages
			PrintPageEventArgs printPageArgs;
			do
			{
				PageSettings pageSettings = DefaultPageSettings.Clone() as PageSettings;
				this.OnQueryPageSettings(new QueryPageSettingsEventArgs(pageSettings));
				
				printPageArgs = new PrintPageEventArgs(
						null,
						pageSettings.Bounds,
						new Rectangle(0, 0, pageSettings.PaperSize.Width, pageSettings.PaperSize.Height),
						pageSettings);
				Graphics g = PrintController.OnStartPage(this, printPageArgs);
				// assign Graphics in printPageArgs
				printPageArgs.SetGraphics(g);
				
				if (!printPageArgs.Cancel)
					this.OnPrintPage(printPageArgs);
				
				PrintController.OnEndPage(this, printPageArgs);
				if (printPageArgs.Cancel)
					break;
			} while (printPageArgs.HasMorePages);
			
			this.OnEndPrint(printArgs);
			PrintController.OnEndPrint(this, printArgs);
		}
		public override string ToString(){
			return "[PrintDocument " + this.DocumentName + "]";
		}
		
// events
		protected virtual void OnBeginPrint(PrintEventArgs e){
			//fire the event
			if (BeginPrint != null)
				BeginPrint(this, e);
		}
		
		protected virtual void OnEndPrint(PrintEventArgs e){
			//fire the event
			if (EndPrint != null)
				EndPrint(this, e);
		}
		
		protected virtual void OnPrintPage(PrintPageEventArgs e){
			//fire the event
			if (PrintPage != null)
				PrintPage(this, e);
		}
		
		protected virtual void OnQueryPageSettings(QueryPageSettingsEventArgs e){
			//fire the event
			if (QuerypageSettings != null)
				QuerypageSettings(this, e);
		}
		
		public event PrintEventHandler BeginPrint;
		public event PrintEventHandler EndPrint;
		public event PrintPageEventHandler PrintPage;
		public event QueryPageSettingsEventHandler QuerypageSettings;
	}
}
