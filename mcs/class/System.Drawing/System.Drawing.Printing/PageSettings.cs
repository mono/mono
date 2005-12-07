//
// System.Drawing.PageSettings.cs
//
// Authors:
//   Dennis Hayes (dennish@Raytek.com)
//   Herve Poussineau (hpoussineau@fr.st)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002 Ximian, Inc
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System;
using System.Runtime.InteropServices;

namespace System.Drawing.Printing
{
#if NET_2_0
	[Serializable]
#else
	[ComVisible (false)]
#endif
	public class PageSettings : ICloneable
	{
		bool _Color;
		bool _Landscape;
		float _HardMarginX;
		float _HardMarginY;
		RectangleF _PrintableArea;
		// create a new default Margins object (is 1 inch for all margins)
		Margins _Margins = new Margins();
		PaperSize _PaperSize;
		PaperSource _PaperSource;
		PrinterResolution _PrinterResolution;
		PrinterSettings _PrinterSettings;
		
		public PageSettings() : this(new PrinterSettings())
		{
		}
		
		public PageSettings(PrinterSettings printerSettings)
		{
			PrinterSettings = printerSettings;
			
			Color = printerSettings.DefaultPageSettings.Color;
			Landscape = printerSettings.DefaultPageSettings.Landscape;
			PaperSize = printerSettings.DefaultPageSettings.PaperSize;
			PaperSource = printerSettings.DefaultPageSettings.PaperSource;
			PrinterResolution = printerSettings.DefaultPageSettings.PrinterResolution;
		}
		
		// used by PrinterSettings.DefaultPageSettings
		internal PageSettings(PrinterSettings printerSettings, bool color, bool landscape, PaperSize paperSize, PaperSource paperSource, PrinterResolution printerResolution)
		{
			PrinterSettings = printerSettings;
			
			Color = color;
			Landscape = landscape;
			PaperSize = paperSize;
			PaperSource = paperSource;
			PrinterResolution = printerResolution;
		}

		//props
		public Rectangle Bounds{
			get{
				int width = this.PaperSize.Width;
				int height = this.PaperSize.Height;
				
				width -= this.Margins.Left + this.Margins.Right;
				height -= this.Margins.Top + this.Margins.Bottom;
				
				if (this.Landscape) {
					// swap width and height
					int tmp = width;
					width = height;
					height = tmp;
				}
				return new Rectangle(0, 0, width, height);
			}
		}
		
		public bool Color{
			get{
				return _Color;
			}
			set{
				_Color = value;
			}
		}
		
		public bool Landscape {
			get{
				return _Landscape;
			}
			set{
				_Landscape = value;
			}
		}
		
		public Margins Margins{
			get{
				return _Margins;
			}
			set{
				_Margins = value;
			}
		}
		
		public PaperSize PaperSize{
			get{
				return _PaperSize;
			}
			set{
				_PaperSize = value;
			}
		}
		
		public PaperSource PaperSource{
			get{
				return _PaperSource;
			}
			set{
				_PaperSource = value;
			}
		}
		
		public PrinterResolution PrinterResolution{
			get{
				return _PrinterResolution;
			}
			set{
				_PrinterResolution = value;
			}
		}
		
		public PrinterSettings PrinterSettings{
			get{
				return _PrinterSettings;
			}
			set{
				_PrinterSettings = value;
			}
		}		
#if NET_2_0
		public float HardMarginX {
			get {
				return _HardMarginX;
			}
		}
		
		public float HardMarginY {
			get {
				return _HardMarginY;
			}
		}
		
		public RectangleF PrintableArea {
			get {
				return _PrintableArea;
			}
		}
#endif


		public object Clone(){
			return new PageSettings(this.PrinterSettings);
		}


		[MonoTODO("PageSettings.CopyToHdevmode")]
		public void CopyToHdevmode (IntPtr hdevmode){
			throw new NotImplementedException ();
		}


		[MonoTODO("PageSettings.SetHdevmode")]
		public void SetHdevmode (IntPtr hdevmode){
			throw new NotImplementedException ();
		}	

		public override string ToString(){
			string ret = "[PageSettings: Color={0}";
			ret += ", Landscape={1}";
			ret += ", Margins={2}";
			ret += ", PaperSize={3}";
			ret += ", PaperSource={4}";
			ret += ", PrinterResolution={5}";
			ret += "]";
			
			return String.Format(ret, this.Color, this.Landscape, this.Margins, this.PaperSize, this.PaperSource, this.PrinterResolution);
		}
	}
}
