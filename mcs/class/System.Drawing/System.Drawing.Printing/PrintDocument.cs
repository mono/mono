//
// System.Drawing.PrintDocument.cs
//
// Authors:
//   Dennis Hayes (dennish@Raytek.com)
//   Herve Poussineau (hpoussineau@fr.st)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002 Ximian, Inc
//

using System;
using System.ComponentModel;

namespace System.Drawing.Printing
{
	[DefaultEvent ("PrintPage"), DefaultProperty ("DocumentName")]
	[ToolboxItemFilter ("System.Drawing.Printing", ToolboxItemFilterType.Allow)]
	[DesignerCategory ("Component")]
	public class PrintDocument : System.ComponentModel.Component
	{
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
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		[SRDescription ("The settings for the current page.")]
		public PageSettings DefaultPageSettings{
			get{
				return defaultpagesettings;
			}
			set{
				defaultpagesettings = value;
			}
		}

		// Name of the document, not the file!
		[DefaultValue ("document")]
		[SRDescription ("The name of the document.")]
		public string DocumentName{
			get{
				return documentname;
			}
			set{
				documentname = value;
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		[SRDescription ("The print controller object.")]
		public PrintController PrintController{
			get{
				return printcontroller;
			}
			set{
				printcontroller = value;
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		[SRDescription ("The current settings for the active printer.")]
		public PrinterSettings PrinterSettings{
			get{
				return printersettings;
			}
			set{
				printersettings = value;
			}
		}

#if !(NET_1_0)
		[DefaultValue (false)]
		[SRDescription ("Determines if the origin is set at the specified margins.")]
		public bool OriginAtMargins{
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
			if (QueryPageSettings != null)
				QueryPageSettings(this, e);
		}

		[SRDescription ("Raised when printing begins")]
		public event PrintEventHandler BeginPrint;

		[SRDescription ("Raised when printing ends")]
		public event PrintEventHandler EndPrint;

		[SRDescription ("Raised when printing of a new page begins")]
		public event PrintPageEventHandler PrintPage;

		[SRDescription ("Raised before printing of a new page begins")]
		public event QueryPageSettingsEventHandler QueryPageSettings;
	}
}
