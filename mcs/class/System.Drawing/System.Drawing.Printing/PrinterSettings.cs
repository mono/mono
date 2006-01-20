//
// System.Drawing.PrinterSettings.cs
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
using System.Collections;
using System.Drawing.Printing;
using System.ComponentModel;
using System.Drawing.Imaging;

namespace System.Drawing.Printing
{
	[Serializable]
#if ! NET_2_0
	[ComVisible(false)]
#endif	
	public class PrinterSettings : ICloneable
	{
		private string printer_name;
		private string print_filename;
		private short copies;
		private int maximum_page;
		private int minimum_page; 
		private int from_page;
		private int to_page;
		private bool collate;
		private PrintRange print_range;
		internal int maximum_copies;
		internal bool can_duplex;
		internal bool supports_color;
		internal int landscape_angle;		
		internal PrinterSettings.PrinterResolutionCollection printer_resolutions;
		internal PrinterSettings.PaperSizeCollection paper_sizes;
		
		public PrinterSettings() : this (SysPrn.Service.DefaultPrinter)
		{			
		}
		
		internal PrinterSettings (string printer)
		{						
			printer_name = printer;			
			ResetToDefaults ();
			SysPrn.Service.LoadPrinterSettings (printer_name, this);			
		}
		
		private void ResetToDefaults ()
		{			
			printer_resolutions = null;
			paper_sizes = null;
			maximum_page = 9999; 	
		}

		// Public subclasses

		public class PaperSourceCollection : ICollection, IEnumerable
		{
			ArrayList _PaperSources = new ArrayList();
			
			public PaperSourceCollection(PaperSource[] array) {
				foreach (PaperSource ps in array)
					_PaperSources.Add(ps);
			}
			
			public int Count { get { return _PaperSources.Count; } }
			int ICollection.Count { get { return _PaperSources.Count; } }
			bool ICollection.IsSynchronized { get { return false; } }
			object ICollection.SyncRoot { get { return this; } }			
#if NET_2_0
			[EditorBrowsable(EditorBrowsableState.Never)]
      			public int Add (PaperSource paperSource) {throw new NotImplementedException (); }
			public void CopyTo (PaperSource[] paperSources, int index)  {throw new NotImplementedException (); }
#endif
			
			public virtual PaperSource this[int index] {
				get { return _PaperSources[index] as PaperSource; }
			}
			
			IEnumerator IEnumerable.GetEnumerator()
			{
				return _PaperSources.GetEnumerator();
			}
			
			public IEnumerator GetEnumerator()
			{
				return _PaperSources.GetEnumerator();
			}
			
			void ICollection.CopyTo(Array array, int index)
			{
				_PaperSources.CopyTo(array, index);
			}
		}

		public class PaperSizeCollection : ICollection, IEnumerable
		{
			ArrayList _PaperSizes = new ArrayList();
			
			public PaperSizeCollection(PaperSize[] array) {
				foreach (PaperSize ps in array)
					_PaperSizes.Add(ps);
			}
			
			public int Count { get { return _PaperSizes.Count; } }
			int ICollection.Count { get { return _PaperSizes.Count; } }
			bool ICollection.IsSynchronized { get { return false; } }
			object ICollection.SyncRoot { get { return this; } }			
#if NET_2_0		
			[EditorBrowsable(EditorBrowsableState.Never)]
			public int Add (PaperSize paperSize) {return _PaperSizes.Add (paperSize); }	
			public void CopyTo (PaperSize[] paperSizes, int index) {throw new NotImplementedException (); }			
#else
			internal int Add (PaperSize paperSize) {return _PaperSizes.Add (paperSize); }	
#endif
			
			public virtual PaperSize this[int index] {
				get { return _PaperSizes[index] as PaperSize; }
			}
			
			IEnumerator IEnumerable.GetEnumerator()
			{
				return _PaperSizes.GetEnumerator();
			}
			
			public IEnumerator GetEnumerator()
			{
				return _PaperSizes.GetEnumerator();
			}
			
			void ICollection.CopyTo(Array array, int index)
			{
				_PaperSizes.CopyTo(array, index);
			}
			
			internal void Clear ()
			{ 
				_PaperSizes.Clear (); 
			}
		}

		public class PrinterResolutionCollection : ICollection, IEnumerable
		{
			ArrayList _PrinterResolutions = new ArrayList();
			
			public PrinterResolutionCollection(PrinterResolution[] array) {
				foreach (PrinterResolution pr in array)
					_PrinterResolutions.Add(pr);
			}
			
			public int Count { get { return _PrinterResolutions.Count; } }
			int ICollection.Count { get { return _PrinterResolutions.Count; } }
			bool ICollection.IsSynchronized { get { return false; } }
			object ICollection.SyncRoot { get { return this; } }			
#if NET_2_0
			[EditorBrowsable(EditorBrowsableState.Never)]
			public int Add (PrinterResolution printerResolution) { return _PrinterResolutions.Add (printerResolution); }
			public void CopyTo (PrinterResolution[] printerResolutions, int index) {throw new NotImplementedException (); }
#else
			internal int Add (PrinterResolution printerResolution) { return _PrinterResolutions.Add (printerResolution); }
#endif
						
			public virtual PrinterResolution this[int index] {
				get { return _PrinterResolutions[index] as PrinterResolution; }
			}
			
			IEnumerator IEnumerable.GetEnumerator()
			{
				return _PrinterResolutions.GetEnumerator();
			}
			
			public IEnumerator GetEnumerator()
			{
				return _PrinterResolutions.GetEnumerator();
			}
			
			void ICollection.CopyTo(Array array, int index)
			{
				_PrinterResolutions.CopyTo(array, index);
			}
			
			internal void Clear ()
			{ 
				_PrinterResolutions.Clear (); 
			}
		}

		public class StringCollection : ICollection, IEnumerable
		{
			ArrayList _Strings = new ArrayList();
			
			public StringCollection(string[] array) {
				foreach (string s in array)
					_Strings.Add(s);
			}
			
			public int Count { get { return _Strings.Count; } }
			int ICollection.Count { get { return _Strings.Count; } }
			bool ICollection.IsSynchronized { get { return false; } }
			object ICollection.SyncRoot { get { return this; } }
						
			public virtual string this[int index] {
				get { return _Strings[index] as string; }
			}
#if NET_2_0
			[EditorBrowsable(EditorBrowsableState.Never)]
      			public int Add (string value) { return _Strings.Add (value); }
      			public void CopyTo (string[] strings, int index) {throw new NotImplementedException (); }      			
#else
			internal int Add (string value) { return _Strings.Add (value); }
#endif

			IEnumerator IEnumerable.GetEnumerator()
			{
				return _Strings.GetEnumerator();
			}
			
			public IEnumerator GetEnumerator()
			{
				return _Strings.GetEnumerator();
			}
			
			void ICollection.CopyTo(Array array, int index)
			{
				_Strings.CopyTo(array, index);
			}			
		}
		
		//properties
		
		public bool CanDuplex
		{
			get { return can_duplex; }
		}
		
		public bool Collate
		{
			get { return collate; }
			set { collate = value; }
		}

		public short Copies
		{
			get { return copies; }
			set { 
				if (value < 0)
					throw new ArgumentException ("The value of the Copies property is less than zero.");
				
				copies = value;
			}
		}

		[MonoTODO("PrinterSettings.DefaultPageSettings")]
		public PageSettings DefaultPageSettings
		{
			get
			{
				return new PageSettings(
					this,
					// TODO: get default color mode for this printer
					false,
					// TODO: get default orientation for this printer
					false,
					// TODO: get default paper size for this printer
					new PaperSize("A4", 827, 1169),
					// TODO: get default paper source for this printer
					new PaperSource("default", PaperSourceKind.FormSource),
					// TODO: get default resolution for this printer
					new PrinterResolution(300, 300, PrinterResolutionKind.Medium)
				);
			}
		}

		[MonoTODO("PrinterSettings.Duplex")]
		public Duplex Duplex
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}
		
		public int FromPage
		{
			get { return from_page; }
			set {
				if (value < 0)
					throw new ArgumentException ("The value of the FromPage property is less than zero");
				
				from_page = value;
			}
		}
		
		public static PrinterSettings.StringCollection InstalledPrinters
		{
			get { return SysPrn.Service.InstalledPrinters; }
		}
	
		public bool IsDefaultPrinter
		{
			get { return (printer_name == SysPrn.Service.DefaultPrinter); }
		}

		[MonoTODO("PrinterSettings.IsPlotter")]
		public bool IsPlotter
		{
			get { return false; }
		}

		[MonoTODO("PrinterSettings.IsValid")]
		public bool IsValid
		{
			get { return true; }
		}
		
		public int LandscapeAngle
		{
			get { return landscape_angle; }
		}
		
		public int MaximumCopies
		{
			get { return maximum_copies; }
		}
		
		public int MaximumPage
		{
			get { return maximum_page; }
			set {
				// This not documented but behaves like MinimumPage
				if (value < 0)
					throw new ArgumentException ("The value of the MaximumPage property is less than zero");
				
				maximum_page = value;
			}
		}
		
		public int MinimumPage
		{
			get { return minimum_page; }
			set {
				if (value < 0)
					throw new ArgumentException ("The value of the MaximumPage property is less than zero");
				
				minimum_page = value;
			}
		}
		
		public PrinterSettings.PaperSizeCollection PaperSizes
		{
			get {
				if (paper_sizes == null) {
					paper_sizes = new PrinterSettings.PaperSizeCollection (new PaperSize [] {});
					SysPrn.Service.LoadPrinterPaperSizes (printer_name, this);
				}				
				return paper_sizes;				
			}
		}

		[MonoTODO("PrinterSettings.PaperSources")]
		public PrinterSettings.PaperSourceCollection PaperSources
		{
			get { throw new NotImplementedException(); }
		}
#if NET_2_0		
		
		public string PrintFileName
		{
			get { return print_filename; }
			set { print_filename = value; }
		}
#endif		
		public string PrinterName
		{
			get { return printer_name; }
			set { 
				if (printer_name == value)
					return;
					
				printer_name = value;
				SysPrn.Service.LoadPrinterSettings (printer_name, this);
			}
		}
		
		public PrinterSettings.PrinterResolutionCollection PrinterResolutions
		{
			get {
				if (printer_resolutions == null) {
					printer_resolutions = new PrinterSettings.PrinterResolutionCollection (new PrinterResolution[] {});
					SysPrn.Service.LoadPrinterResolutions (printer_name, this);
				}
				return printer_resolutions;
			}
		}
		
		public PrintRange PrintRange
		{
			get { return print_range; }
			set { 
				if (value != PrintRange.AllPages && value != PrintRange.Selection &&
					value != PrintRange.SomePages)
					throw new InvalidEnumArgumentException ("The value of the PrintRange property is not one of the PrintRange values");
				
				print_range = value;
			}
		}

		[MonoTODO("PrinterSettings.PrintToFile")]
		public bool PrintToFile
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}
		
		public bool SupportsColor
		{
			get { return supports_color; }
		}
		
		public int ToPage
		{
			get { return to_page; }
			set {
				if (value < 0)
					throw new ArgumentException ("The value of the ToPage property is less than zero");
				
				to_page = value;
			}		
		}

		//methods		
		public virtual object Clone()
		{
			PrinterSettings ps = new PrinterSettings (printer_name);
			return ps;
		}

		[MonoTODO("PrinterSettings.CreateMeasurementGraphics")]
		public Graphics CreateMeasurementGraphics()
		{
			throw new NotImplementedException();
		}
#if NET_2_0
		[MonoTODO("PrinterSettings.CreateMeasurementGraphics")]
		public Graphics CreateMeasurementGraphics(bool honorOriginAtMargins)		
		{
			throw new NotImplementedException();
		}
		
		[MonoTODO("PrinterSettings.CreateMeasurementGraphics")]
		public Graphics CreateMeasurementGraphics(PageSettings pageSettings)		
		{
			throw new NotImplementedException();
		}
		
		[MonoTODO("PrinterSettings.CreateMeasurementGraphics")]
		public Graphics CreateMeasurementGraphics (PageSettings pageSettings, bool honorOriginAtMargins)		
		{
			throw new NotImplementedException();
		} 
#endif		

		[MonoTODO("PrinterSettings.GetHdevmode")]
		public IntPtr GetHdevmode()
		{
			throw new NotImplementedException();
		}

		[MonoTODO("PrinterSettings.GetHdevmode")]
		public IntPtr GetHdevmode(PageSettings pageSettings)
		{
			throw new NotImplementedException();
		}

		[MonoTODO("PrinterSettings.GetHdevname")]
		public IntPtr GetHdevnames()
		{
			throw new NotImplementedException();
		}
		
#if NET_2_0

		[MonoTODO("IsDirectPrintingSupported")]
		public bool IsDirectPrintingSupported (Image image)
		{
			throw new NotImplementedException();
		}
		
		[MonoTODO("IsDirectPrintingSupported")]
		public bool IsDirectPrintingSupported (ImageFormat imageFormat)
		{
			throw new NotImplementedException();
		}
#endif

		[MonoTODO("PrinterSettings.SetHdevmode")]
		public void SetHdevmode(IntPtr hdevmode)
		{
			throw new NotImplementedException();
		}

		[MonoTODO("PrinterSettings.SetHdevnames")]
		public void SetHdevnames(IntPtr hdevnames)
		{
			throw new NotImplementedException();
		}

		[MonoTODO("PrinterSettings.ToString")]
		public override string ToString()
		{
			throw new NotImplementedException();
		}		
	}
}
