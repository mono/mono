//
// System.Drawing.StandardPrintController.cs
//
// Author:
//   Dennis Hayes (dennish@Raytek.com)
//   Herve Poussineau (hpoussineau@fr.st)
//
// (C) 2002 Ximian, Inc
//

using System;

namespace System.Drawing.Printing
{
	public class StandardPrintController : PrintController
	{
		private int page;
		private Image image;
		
		public StandardPrintController()
		{
		}

		[MonoTODO("StandardPrintController.OnEndPage")]
		public override void OnEndPage(PrintDocument document, PrintPageEventArgs e)
		{
			//TODO: print current page
			// - image to print is this.image
			// - page settings are in e.PageSettings
			// - printer settings are in document.PrinterSettings
			// - don't forget to use document.OriginAtMargins (only if .NET 1.1)
			
			// actually, "print" == "save to a file"
			try
			{
				string fileName = document.DocumentName + " " + page.ToString("D4") + ".jpg";
				Console.WriteLine("StandardPrintController: Print page \"{0}\"", fileName);
				image.Save(fileName, System.Drawing.Imaging.ImageFormat.Jpeg);
			}
			catch (Exception) {}
			
			if (e.Graphics != null)
				e.Graphics.Dispose();
		}

		[MonoTODO("StandardPrintController.OnStartPrint")]
		public override void OnStartPrint(PrintDocument document, PrintEventArgs e){
			page = 0;
		}

		[MonoTODO("StandardPrintController.OnEndPrint")]
		public override void OnEndPrint(PrintDocument document, PrintEventArgs e){
			return;
		}

		[MonoTODO("StandardPrintController.OnStartPage")]
		public override Graphics OnStartPage(PrintDocument document, PrintPageEventArgs e)
		{
			//FIXME: I'm not sure of what I'm doing
			// I don't know what size to give to image
			// and why I have to clear it
			page++;
			// returns a new (empty) graphics
			image = new Bitmap(e.MarginBounds.Width, e.MarginBounds.Height);
			Graphics g = Graphics.FromImage(image);
			g.Clear(Color.White);
			return g;
		}
	}
}
