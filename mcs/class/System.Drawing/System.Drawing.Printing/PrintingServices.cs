//
// Copyright (C) 2005 Novell, Inc. http://www.novell.com
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Author:
//
//	Jordi Mas i Hernandez, jordimash@gmail.com
//

using System.Runtime.InteropServices;
using System.Collections;
using System.Drawing.Printing;
using System.ComponentModel;
using System.Drawing.Imaging;

namespace System.Drawing.Printing
{
	internal abstract class PrintingServices
	{
		// Properties
		internal abstract PrinterSettings.StringCollection InstalledPrinters { get; }
		internal abstract string DefaultPrinter { get; }
				

		// Methods
		internal abstract bool StartDoc (GraphicsPrinter gr, string doc_name, string output_file);
		internal abstract IntPtr CreateGraphicsContext (PrinterSettings settings);
		internal abstract bool StartPage (GraphicsPrinter gr);
		internal abstract bool EndPage (GraphicsPrinter gr);
		internal abstract bool EndDoc (GraphicsPrinter gr);
		
		internal abstract void LoadPrinterSettings (string printer, PrinterSettings settings);
		internal abstract void LoadPrinterResolutions (string printer, PrinterSettings settings);
		internal abstract void LoadPrinterPaperSizes (string printer, PrinterSettings settings);
		
		internal void LoadDefaultResolutions (PrinterSettings.PrinterResolutionCollection col)
		{
			col.Add (new PrinterResolution ((int) PrinterResolutionKind.High, -1, PrinterResolutionKind.High));
			col.Add (new PrinterResolution ((int) PrinterResolutionKind.Medium, -1, PrinterResolutionKind.Medium));
			col.Add (new PrinterResolution ((int) PrinterResolutionKind.Low, -1, PrinterResolutionKind.Low));
			col.Add (new PrinterResolution ((int) PrinterResolutionKind.Draft, -1, PrinterResolutionKind.Draft));
		}
	}

	internal class SysPrn
	{
		static PrintingServices service;

		static SysPrn ()
		{
			int platform = (int) Environment.OSVersion.Platform;
			
			if (platform == 4 || platform == 128) {
				service = new  PrintingServicesUnix ();
			} else {
				service = new PrintingServicesWin32 ();
			}
		}

		static internal PrintingServices Service {
			get { return service; }
		}
	}
	
	internal class GraphicsPrinter
	{
		private	Graphics graphics;
		private IntPtr	hDC;
		 
		internal GraphicsPrinter (Graphics gr, IntPtr dc)
		{
			graphics = gr;
			hDC = dc;
		}
						
		internal Graphics Graphics { 
			get { return graphics; }
			set { graphics = value; }
		}
		internal IntPtr Hdc { get { return hDC; }}
	}
}


