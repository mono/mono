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
using System.Text;

namespace System.Drawing.Printing
{
	internal class PrintingServicesWin32 : PrintingServices
	{
		internal PrintingServicesWin32 ()
		{

		}

		internal override void LoadPrinterSettings (string printer, PrinterSettings settings)
		{
			int ret;

			settings.maximum_copies = Win32DeviceCapabilities (printer, null, DCCapabilities.DC_COPIES, IntPtr.Zero, IntPtr.Zero);

			ret = Win32DeviceCapabilities (printer, null, DCCapabilities.DC_DUPLEX, IntPtr.Zero, IntPtr.Zero);
			settings.can_duplex = (ret == 1) ? true : false;

			ret = Win32DeviceCapabilities (printer, null, DCCapabilities.DC_COLORDEVICE, IntPtr.Zero, IntPtr.Zero);
			settings.supports_color = (ret == 1) ? true : false;

			ret = Win32DeviceCapabilities (printer, null, DCCapabilities.DC_ORIENTATION, IntPtr.Zero, IntPtr.Zero);
			if (ret != -1)
				settings.landscape_angle = ret;
		}

		internal override void LoadPrinterResolutions (string printer, PrinterSettings settings)
		{
			int ret;
			IntPtr ptr, buff = IntPtr.Zero;

			settings.PrinterResolutions.Clear ();
			LoadDefaultResolutions (settings.PrinterResolutions);
			ret = Win32DeviceCapabilities (printer, null, DCCapabilities.DC_ENUMRESOLUTIONS, IntPtr.Zero, IntPtr.Zero);

			if (ret == -1)
				return;

			ptr = buff = Marshal.AllocHGlobal (ret * 2 * Marshal.SizeOf (buff));
			ret = Win32DeviceCapabilities (printer, null, DCCapabilities.DC_ENUMRESOLUTIONS, buff, IntPtr.Zero);
			int x, y;
			if (ret != -1) {
				for (int i = 0; i < ret; i++) {
					x = Marshal.ReadInt32 (ptr);
					ptr = new IntPtr (ptr.ToInt64 () + Marshal.SizeOf (x));
					y = Marshal.ReadInt32 (ptr);
					ptr = new IntPtr (ptr.ToInt64 () + Marshal.SizeOf (y));
					settings.PrinterResolutions.Add (new PrinterResolution
						(x,y, PrinterResolutionKind.Custom));
				}
			}
			Marshal.FreeHGlobal (buff);
		}

		internal override void LoadPrinterPaperSizes (string printer, PrinterSettings settings)
		{
			int items, ret;
			IntPtr ptr_names, buff_names = IntPtr.Zero;
			IntPtr ptr_sizes, buff_sizes = IntPtr.Zero;
			IntPtr ptr_sizes_enum, buff_sizes_enum = IntPtr.Zero;
			string name;

			settings.PaperSizes.Clear ();
			items = Win32DeviceCapabilities (printer, null, DCCapabilities.DC_PAPERSIZE, IntPtr.Zero, IntPtr.Zero);

			if (items == -1)
				return;

			try {
				ptr_sizes = buff_sizes = Marshal.AllocHGlobal (items * 2 * 4);
				ptr_names = buff_names = Marshal.AllocHGlobal (items * 64 * 2);
				ptr_sizes_enum = buff_sizes_enum = Marshal.AllocHGlobal (items * 2);
				ret = Win32DeviceCapabilities (printer, null, DCCapabilities.DC_PAPERSIZE, buff_sizes, IntPtr.Zero);

				if (ret == -1) {
					// the finally clause will free the unmanaged memory before returning
					return;
				}

				ret = Win32DeviceCapabilities (printer, null, DCCapabilities.DC_PAPERS, buff_sizes_enum, IntPtr.Zero);
				ret = Win32DeviceCapabilities (printer, null, DCCapabilities.DC_PAPERNAMES, buff_names, IntPtr.Zero);

				int x, y;
				PaperSize ps;
				PaperKind kind;
				for (int i = 0; i < ret; i++) {
					x = Marshal.ReadInt32 (ptr_sizes);
					ptr_sizes = new IntPtr (ptr_sizes.ToInt64 () + 4);
					y = Marshal.ReadInt32 (ptr_sizes);
					ptr_sizes = new IntPtr (ptr_sizes.ToInt64 () + 4);

					x = PrinterUnitConvert.Convert (x, PrinterUnit.TenthsOfAMillimeter,
					      PrinterUnit.Display);

					y = PrinterUnitConvert.Convert (y, PrinterUnit.TenthsOfAMillimeter,
					      PrinterUnit.Display);

					name  = Marshal.PtrToStringUni (ptr_names);
					ptr_names = new IntPtr (ptr_names.ToInt64 () + 64 * 2);

					kind = (PaperKind) Marshal.ReadInt16 (ptr_sizes_enum);
					ptr_sizes_enum = new IntPtr (ptr_sizes_enum.ToInt64 () + 2);

					ps = new PaperSize (name, x,y);
					ps.SetKind (kind);
					settings.PaperSizes.Add (ps);
				}
			}
			finally {
				if (buff_names != IntPtr.Zero)
					Marshal.FreeHGlobal (buff_names);
				if (buff_sizes != IntPtr.Zero)
					Marshal.FreeHGlobal (buff_sizes);
				if (buff_sizes_enum != IntPtr.Zero)
					Marshal.FreeHGlobal (buff_sizes_enum);
			}
		}

		internal override bool StartDoc (GraphicsPrinter gr, string doc_name, string output_file)
		{
			DOCINFO di = new DOCINFO ();
			int ret;

			di.cbSize = Marshal.SizeOf (di);
  			di.lpszDocName = Marshal.StringToHGlobalUni (doc_name);
  			di.lpszOutput = IntPtr.Zero;
  			di.lpszDatatype = IntPtr.Zero;
  			di.fwType = 0;

  			ret = Win32StartDoc (gr.Hdc, ref di);  			
			Marshal.FreeHGlobal (di.lpszDocName);
			return (ret > 0) ? true : false;
		}

		internal override bool StartPage (GraphicsPrinter gr)
		{
			int ret = Win32StartPage (gr.Hdc);			
			return (ret > 0) ? true : false;
		}

		internal override bool EndPage (GraphicsPrinter gr)
		{
			int ret = Win32EndPage (gr.Hdc);			
			return (ret > 0) ? true : false;
		}

		internal override bool EndDoc (GraphicsPrinter gr)
		{
			int ret = Win32EndDoc (gr.Hdc);
			Win32DeleteDC (gr.Hdc);			
			gr.Graphics.Dispose ();						
			return (ret > 0) ? true : false;
		}

		internal override IntPtr CreateGraphicsContext (PrinterSettings settings)
		{
			IntPtr dc = IntPtr.Zero;
			dc = Win32CreateDC (null, settings.PrinterName, null, IntPtr.Zero /* DEVMODE */);
			return dc;
		}

		// Properties
		internal override string DefaultPrinter {
			get {
            			StringBuilder name = new StringBuilder (1024);
            			int length = name.Capacity;

            			Win32GetDefaultPrinter (name, ref length);            			
				return name.ToString ();
			}
		}

		internal override PrinterSettings.StringCollection InstalledPrinters {
			get {
				PrinterSettings.StringCollection col = new PrinterSettings.StringCollection (new string[] {});
				PRINTER_INFO printer_info;
				uint cbNeeded = 0, printers = 0;
				IntPtr ptr, buff;
				string s;

				// Determine space need it
        			Win32EnumPrinters (2 /* PRINTER_ENUM_LOCAL */,
            				null, 2, IntPtr.Zero, 0, ref cbNeeded, ref printers);

            			ptr = buff = Marshal.AllocHGlobal ((int) cbNeeded);
				try {
					// Give us the printer list
					Win32EnumPrinters (2 /* PRINTER_ENUM_LOCAL */,
						null, 2, buff, (uint)cbNeeded, ref cbNeeded, ref printers);

					for (int i = 0; i < printers; i++) {
			            		printer_info = (PRINTER_INFO) Marshal.PtrToStructure (ptr, typeof (PRINTER_INFO));
	            				s  = Marshal.PtrToStringUni (printer_info.pPrinterName);
			            		col.Add (s);
			            		ptr = new IntPtr (ptr.ToInt64 () + Marshal.SizeOf (printer_info));
					}
				}
				finally {
					Marshal.FreeHGlobal (buff);
				}
				return col;
			}
		}

		//
		// DllImports
		//

		[DllImport("winspool.drv", CharSet=CharSet.Unicode, EntryPoint="DeviceCapabilities", SetLastError=true)]
		static extern int Win32DeviceCapabilities (string device, string port, DCCapabilities cap, IntPtr outputBuffer, IntPtr deviceMode);

		[DllImport("winspool.drv", CharSet=CharSet.Unicode, EntryPoint="EnumPrinters", SetLastError=true)]
		static extern int Win32EnumPrinters (int Flags, string Name, uint Level, IntPtr pPrinterEnum, uint cbBuf,
    			ref uint pcbNeeded, ref uint pcReturned);

      		[DllImport("winspool.drv", EntryPoint="GetDefaultPrinter", CharSet=CharSet.Unicode, SetLastError=true)]
      		private static extern int Win32GetDefaultPrinter (StringBuilder buffer, ref int bufferSize);

    		[DllImport("gdi32.dll", EntryPoint="CreateDC")]
		static extern IntPtr Win32CreateDC (string lpszDriver, string lpszDevice,
   			string lpszOutput, IntPtr lpInitData);

   		[DllImport("gdi32.dll", CharSet=CharSet.Unicode, EntryPoint="StartDoc")]
		static extern int Win32StartDoc (IntPtr hdc, [In] ref DOCINFO lpdi);

		[DllImport("gdi32.dll", EntryPoint="StartPage")]
		static extern int Win32StartPage (IntPtr hDC);

		[DllImport("gdi32.dll", EntryPoint="EndPage")]
		static extern int Win32EndPage (IntPtr hdc);

		[DllImport("gdi32.dll", EntryPoint="EndDoc")]
		static extern int Win32EndDoc (IntPtr hdc);

		[DllImport("gdi32.dll", EntryPoint="DeleteDC")]
  		public static extern IntPtr Win32DeleteDC (IntPtr hDc);

    		//
    		// Structs
    		//
		[StructLayout (LayoutKind.Sequential)]
		internal struct PRINTER_INFO
		{
			public IntPtr	pServerName;
			public IntPtr	pPrinterName;
			public IntPtr	pShareName;
			public IntPtr	pPortName;
			public IntPtr	pDriverName;
			public IntPtr	pComment;
			public IntPtr	pLocation;
			public IntPtr	pDevMode;
			public IntPtr	pSepFile;
			public IntPtr	pPrintProcessor;
			public IntPtr	pDatatype;
			public IntPtr	pParameters;
			public IntPtr	pSecurityDescriptor;
			public uint	Attributes;
			public uint	Priority;
			public uint	DefaultPriority;
			public uint	StartTime;
			public uint	UntilTime;
			public uint	Status;
			public uint	cJobs;
			public uint	AveragePPM;
    		}

    		[StructLayout (LayoutKind.Sequential)]
		internal struct DOCINFO
		{
  			public int     	cbSize;
  			public IntPtr	lpszDocName;
  			public IntPtr	lpszOutput;
  			public IntPtr	lpszDatatype;
  			public int	fwType;
  		}

  		// Enums
  		internal enum DCCapabilities : short
		{
			DC_FIELDS = 1,
			DC_PAPERS = 2,
			DC_PAPERSIZE = 3,
			DC_MINEXTENT = 4,
			DC_MAXEXTENT = 5,
			DC_BINS = 6,
			DC_DUPLEX = 7,
			DC_SIZE = 8,
			DC_EXTRA = 9,
			DC_VERSION = 10,
			DC_DRIVER = 11,
			DC_BINNAMES = 12,
			DC_ENUMRESOLUTIONS = 13,
			DC_FILEDEPENDENCIES = 14,
			DC_TRUETYPE = 15,
			DC_PAPERNAMES = 16,
			DC_ORIENTATION = 17,
			DC_COPIES = 18,
			DC_BINADJUST = 19,
			DC_EMF_COMPLIANT = 20,
			DC_DATATYPE_PRODUCED = 21,
			DC_COLLATE = 22,
			DC_MANUFACTURER = 23,
			DC_MODEL = 24,
			DC_PERSONALITY = 25,
			DC_PRINTRATE = 26,
			DC_PRINTRATEUNIT = 27,
			DC_PRINTERMEM = 28,
			DC_MEDIAREADY = 29,
			DC_STAPLE = 30,
			DC_PRINTRATEPPM = 31,
			DC_COLORDEVICE = 32,
			DC_NUP = 33
		}
 	}
}


