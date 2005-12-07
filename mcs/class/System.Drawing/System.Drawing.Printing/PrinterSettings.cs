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
		public PrinterSettings()
		{
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
			public int Add (PaperSize paperSize) {throw new NotImplementedException (); }	
			public void CopyTo (PaperSize[] paperSizes, int index) {throw new NotImplementedException (); }			
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
			public int Add (PrinterResolution printerResolution) {throw new NotImplementedException (); }
			public void CopyTo (PrinterResolution[] printerResolutions, int index) {throw new NotImplementedException (); }
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
      			public int Add (string value) {throw new NotImplementedException (); }
      			public void CopyTo (string[] strings, int index) {throw new NotImplementedException (); }
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

		[MonoTODO("PrinterSettings.CanDuplex")]
		public bool CanDuplex
		{
			get { throw new NotImplementedException(); }
		}

		[MonoTODO("PrinterSettings.Collate")]
		public bool Collate
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		[MonoTODO("PrinterSettings.Copies")]
		public short Copies
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
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

		[MonoTODO("PrinterSettings.FromPage")]
		public int FromPage
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		[MonoTODO("PrinterSettings.InstalledPrinters")]
		public static PrinterSettings.StringCollection InstalledPrinters
		{
			get { throw new NotImplementedException(); }
		}

		[MonoTODO("PrinterSettings.IsDefaultPrinter")]
		public bool IsDefaultPrinter
		{
			get { throw new NotImplementedException(); }
		}

		[MonoTODO("PrinterSettings.IsPlotter")]
		public bool IsPlotter
		{
			get { throw new NotImplementedException(); }
		}

		[MonoTODO("PrinterSettings.IsValid")]
		public bool IsValid
		{
			get { throw new NotImplementedException(); }
		}

		[MonoTODO("PrinterSettings.LandscapeAngle")]
		public int LandscapeAngle
		{
			get { throw new NotImplementedException(); }
		}

		[MonoTODO("PrinterSettings.MaximumCopies")]
		public int MaximumCopies
		{
			get { throw new NotImplementedException(); }
		}

		[MonoTODO("PrinterSettings.MaximumPage")]
		public int MaximumPage
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		[MonoTODO("PrinterSettings.MinimumPage")]
		public int MinimumPage
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		[MonoTODO("PrinterSettings.PaperSizes")]
		public PrinterSettings.PaperSizeCollection PaperSizes
		{
			get { throw new NotImplementedException(); }
		}

		[MonoTODO("PrinterSettings.PaperSources")]
		public PrinterSettings.PaperSourceCollection PaperSources
		{
			get { throw new NotImplementedException(); }
		}
#if NET_2_0		
		[MonoTODO("PrinterSettings.PrintFileName")]
		public string PrintFileName
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}
#endif
		[MonoTODO("PrinterSettings.PrinterName")]
		public string PrinterName
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		[MonoTODO("PrinterSettings.PrinterResolutions")]
		public PrinterSettings.PrinterResolutionCollection PrinterResolutions
		{
			get { throw new NotImplementedException(); }
		}

		[MonoTODO("PrinterSettings.PrintRange")]
		public PrintRange PrintRange
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		[MonoTODO("PrinterSettings.PrintToFile")]
		public bool PrintToFile
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		[MonoTODO("PrinterSettings.SupportsColor")]
		public bool SupportsColor
		{
			get { throw new NotImplementedException(); }
		}

		[MonoTODO("PrinterSettings.ToPage")]
		public int ToPage
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		//methods

		[MonoTODO("PrinterSettings.Clone")]
		public virtual object Clone()
		{
			throw new NotImplementedException();
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
