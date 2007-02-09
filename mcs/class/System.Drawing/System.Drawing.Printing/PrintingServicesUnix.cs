//
// Copyright (C) 2005, 2007 Novell, Inc. http://www.novell.com
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
using System.Text;
using System.IO;

namespace System.Drawing.Printing
{
	internal class PrintingServicesUnix : PrintingServices
	{
		private Hashtable doc_info = new Hashtable ();
		private bool cups_installed;
		private string printer_name;
		private bool is_printer_valid;

		internal PrintingServicesUnix ()
		{
			CheckCupsInstalled ();
		}

		private void CheckCupsInstalled ()
		{
			try {
				cupsGetDefault ();
			}
			catch (DllNotFoundException) {
				Console.WriteLine("libcups not found. To have printing support, you need cups installed");
				cups_installed = false;
				return;
			}

			cups_installed = true;
		}
		
		internal override bool IsPrinterValid(string printer, bool force)
		{
			if (!cups_installed || printer == null | printer == String.Empty)
				return false;

			if (!force && this.printer_name != null && String.Intern(this.printer_name).Equals(printer))
				return is_printer_valid;

			IntPtr ptr = cupsGetPPD (printer);
			string ppd_filename = Marshal.PtrToStringAnsi (ptr);
			is_printer_valid = ppd_filename != null;
			this.printer_name = printer; 
			return is_printer_valid;
		}

		// Methods
		internal override void LoadPrinterSettings (string printer, PrinterSettings settings)
		{
			IntPtr ptr, ppd_handle, ptr_opt, ptr_choice;
			string ppd_filename;
			PPD_FILE ppd;
			PPD_OPTION option;
			PPD_CHOICE choice;			

			if (cups_installed == false || (printer == null) || (printer == String.Empty))
				return;
		
			ptr = cupsGetPPD (printer);
			ppd_filename = Marshal.PtrToStringAnsi (ptr);
			ppd_handle = ppdOpenFile (ppd_filename);
			//Console.WriteLine ("File: {0}", ppd_filename);

			ppd = (PPD_FILE) Marshal.PtrToStructure (ppd_handle, typeof (PPD_FILE));
			settings.landscape_angle = ppd.landscape;
			settings.supports_color = (ppd.color_device == 0) ? false : true;

			// Default paper source
			ptr_opt = ppdFindOption (ppd_handle, "InputSlot"); 
			if (ptr_opt != IntPtr.Zero) {
				option = (PPD_OPTION) Marshal.PtrToStructure (ptr_opt, typeof (PPD_OPTION));
				ptr_choice = option.choices;
				for (int c = 0; c < option.num_choices; c++) {
					choice = (PPD_CHOICE) Marshal.PtrToStructure (ptr_choice, typeof (PPD_CHOICE));
					ptr_choice = new IntPtr (ptr_choice.ToInt64 () + Marshal.SizeOf (choice));
					if (choice.choice == option.defchoice) {
						foreach (PaperSource paper_source in settings.PaperSources) {
							if (paper_source.SourceName ==  choice.text) {
								settings.DefaultPageSettings.PaperSource = paper_source;
								break;
							}
						}
						break;
					}
				}
			}
			
			// Default paper size
			ptr_opt = ppdFindOption (ppd_handle, "PageSize"); 
			if (ptr_opt != IntPtr.Zero) {
				option = (PPD_OPTION) Marshal.PtrToStructure (ptr_opt, typeof (PPD_OPTION));
				ptr_choice = option.choices;
				for (int c = 0; c < option.num_choices; c++) {
					choice = (PPD_CHOICE) Marshal.PtrToStructure (ptr_choice, typeof (PPD_CHOICE));
					ptr_choice = new IntPtr (ptr_choice.ToInt64 () + Marshal.SizeOf (choice));
					if (choice.choice == option.defchoice) {
						foreach (PaperSize paper_size in settings.PaperSizes) {
							if (paper_size.PaperName == choice.text) {
								settings.DefaultPageSettings.PaperSize = paper_size;
								break;
							}
						}
						break;
					}
				}
			}
			
			ppdClose (ppd_handle);
		}

		internal override void LoadPrinterResolutions (string printer, PrinterSettings settings)
		{
			settings.PrinterResolutions.Clear ();
			LoadDefaultResolutions (settings.PrinterResolutions);
		}

		internal override void LoadPrinterPaperSizes (string printer, PrinterSettings settings)
		{
			IntPtr ptr, ppd_handle;
			string ppd_filename, real_name;
			PPD_FILE ppd;
			PPD_SIZE size;
			PaperSize ps;

			settings.PaperSizes.Clear ();

			ptr = cupsGetPPD (printer);
			ppd_filename = Marshal.PtrToStringAnsi (ptr);
			ppd_handle = ppdOpenFile (ppd_filename);

			ppd = (PPD_FILE) Marshal.PtrToStructure (ppd_handle, typeof (PPD_FILE));
			ptr = ppd.sizes;
			float w, h;
			for (int i = 0; i < ppd.num_sizes; i++) {
				size = (PPD_SIZE) Marshal.PtrToStructure (ptr, typeof (PPD_SIZE));
				real_name = GetPaperSizeName (ppd_handle, size.name);
				ptr = new IntPtr (ptr.ToInt64 () + Marshal.SizeOf (size));
				w = size.width * 100 / 72;
				h = size.length * 100 / 72;
				ps = new PaperSize (real_name, (int) w, (int) h);
				ps.SetKind (GetPaperKind ((int) w, (int) h));
				settings.PaperSizes.Add (ps);
			}

			ppdClose (ppd_handle);
		}		
		
		internal override void LoadPrinterPaperSources (string printer, PrinterSettings settings)
		{
			IntPtr ptr, ppd_handle, ptr_opt, ptr_choice;
			string ppd_filename;
			PPD_OPTION option;
			PPD_CHOICE choice;

			if (cups_installed == false || (printer == null) || (printer == String.Empty))
				return;

			ptr = cupsGetPPD (printer);
			ppd_filename = Marshal.PtrToStringAnsi (ptr);
			ppd_handle = ppdOpenFile (ppd_filename);

			ptr_opt = ppdFindOption (ppd_handle, "InputSlot"); 

			if (ptr_opt == IntPtr.Zero) {
				ppdClose (ppd_handle);
				return;	
			}
			
			option = (PPD_OPTION) Marshal.PtrToStructure (ptr_opt, typeof (PPD_OPTION));
			//Console.WriteLine (" OPTION  key:{0} def:{1} text: {2}", option.keyword, option.defchoice, option.text);

			ptr_choice = option.choices;
			for (int c = 0; c < option.num_choices; c++) {
				choice = (PPD_CHOICE) Marshal.PtrToStructure (ptr_choice, typeof (PPD_CHOICE));
				ptr_choice = new IntPtr (ptr_choice.ToInt64 () + Marshal.SizeOf (choice));
				//Console.WriteLine ("       choice:{0} - text: {1}", choice.choice, choice.text);
				settings.PaperSources.Add (new PaperSource (choice.text, PaperSourceKind.Custom));					
			}
			ppdClose (ppd_handle);
		}

		internal override bool StartPage (GraphicsPrinter gr)
		{
			return true;
		}

		internal override bool EndPage (GraphicsPrinter gr)
		{
			GdipGetPostScriptSavePage (gr.Hdc);
			return true;
		}

		string tmpfile;

		internal override bool EndDoc (GraphicsPrinter gr)
		{
			DOCINFO doc = (DOCINFO) doc_info[gr.Hdc];

			gr.Graphics.Dispose (); // Dispose object to force surface finish

			IntPtr options;
			int options_count = GetCupsOptions (doc.settings, doc.default_page_settings, out options);

			cupsPrintFile (doc.settings.PrinterName, doc.filename, doc.title, options_count, options);
			cupsFreeOptions (options_count, options);
			doc_info.Remove (gr.Hdc);
			if (tmpfile != null) {
				try { File.Delete (tmpfile); }
				catch { }
			}
			return true;
		}

		internal override bool StartDoc (GraphicsPrinter gr, string doc_name, string output_file)
		{
			DOCINFO doc = (DOCINFO) doc_info[gr.Hdc];
			doc.title = doc_name;
			return true;
		}

		// Unfortunately, PrinterSettings and PageSettings couldn't be referencing each other,
		// thus we need to pass them separately
		internal override IntPtr CreateGraphicsContext (PrinterSettings settings, PageSettings default_page_settings)
		{
			IntPtr graphics = IntPtr.Zero;
			string name;
			if (!settings.PrintToFile) {
				StringBuilder sb = new StringBuilder (1024);
				int length = sb.Capacity;
				cupsTempFile (sb, length);
				name = sb.ToString ();
				tmpfile = name;
			}
			else
				name = settings.PrintFileName;

			PaperSize psize = default_page_settings.PaperSize;
			int width, height;
			if (default_page_settings.Landscape) { // Swap in case of landscape
				width = psize.Height;
				height = psize.Width;
			} else {
				width = psize.Width;
				height = psize.Height;
			}

			GdipGetPostScriptGraphicsContext (name,
				width / 100 * 72,
				height / 100 * 72, 
				// Harcoded dpy's
				300, 300, ref graphics);

			DOCINFO doc = new DOCINFO ();
			doc.filename = name.ToString();
			doc.settings = settings;
			doc.default_page_settings = default_page_settings;
			doc_info.Add (graphics, doc);

			return graphics;
		}

		internal int GetCupsOptions (PrinterSettings printer_settings, PageSettings page_settings, out IntPtr options)
		{
			options = IntPtr.Zero;

			PaperSize size = page_settings.PaperSize;
			int width = size.Width * 72 / 100;
			int height = size.Height * 72 / 100;

			string options_string =
				"copies=" + printer_settings.Copies + " " + 
				"Collate=" + printer_settings.Collate + " " +
				"ColorModel=" + (page_settings.Color ? "Color" : "Black") + " " +
				"PageSize=" + String.Format ("Custom.{0}x{1}", width, height) + " " +
				"landscape=" + page_settings.Landscape;

			return cupsParseOptions (options_string, 0, ref options);
		}

		// Properties

		internal override PrinterSettings.StringCollection InstalledPrinters {
			get {
			  	int n_printers;
				IntPtr dests = IntPtr.Zero, ptr_printers, ptr_printer;
				int cups_dests_size = Marshal.SizeOf(typeof(CUPS_DESTS));
				string str;
				PrinterSettings.StringCollection col = new PrinterSettings.StringCollection (new string[] {});

				if (cups_installed == false)
					return col;

				n_printers = cupsGetDests (ref dests);

				ptr_printers = dests;
				for (int i = 0; i < n_printers; i++) {
					ptr_printer = (IntPtr) Marshal.ReadInt32 (ptr_printers);
					str = Marshal.PtrToStringAnsi (ptr_printer);
					Marshal.FreeHGlobal (ptr_printer);
					ptr_printers = new IntPtr (ptr_printers.ToInt64 () + cups_dests_size);
					col.Add (str);
				}
				Marshal.FreeHGlobal (dests);
				return col;
			}
		}

		internal override string DefaultPrinter {
			get {
				IntPtr str;

				if (cups_installed == false)
					return string.Empty;

				str = cupsGetDefault ();
				if (str != IntPtr.Zero)
					return Marshal.PtrToStringAnsi (str);

				return GetAlternativeDefaultPrinter();
			}
		}

		// Private functions

		/// <summary>
		/// Due to some weird cups thing, cupsGetDefault does not always return 
		/// the default printer, hence this function, that goes through each
		/// printer searching for one that has the is_default flag set. 
		/// If there is none with the flag, but there are printers available,
		/// returns the first one. See #80519, #80198
		/// </summary>
		/// <returns></returns>
		private string GetAlternativeDefaultPrinter ()
		{
			IntPtr printers = IntPtr.Zero;
			string printer_name = String.Empty;

			int printer_count = cupsGetDests (ref printers);
			try {
				int cups_dests_size = Marshal.SizeOf (typeof (CUPS_DESTS));
				IntPtr current = printers;
				for (int i = 0; i < printer_count; i++) {
					CUPS_DESTS printer = (CUPS_DESTS) Marshal.PtrToStructure (current, typeof (CUPS_DESTS));

					if ((printer.is_default != 0) || (printer_name.Length == 0)) {
						printer_name = Marshal.PtrToStringAnsi (printer.name);
						if (printer.is_default != 0)
							break;
					}
					current = new IntPtr (current.ToInt64 () + cups_dests_size);
				}
			}
			finally {
				Marshal.FreeHGlobal (printers);
			}

			return printer_name;
		}

		private string GetPaperSizeName (IntPtr ppd, string name)
		{
			string rslt = name;
			PPD_OPTION option;
			PPD_CHOICE choice;
			IntPtr ptr_opt, ptr_choice;
			
			ptr_opt = ppdFindOption (ppd, "PageSize"); 

			if (ptr_opt == IntPtr.Zero) {
				return rslt;	
			}
			
			option = (PPD_OPTION) Marshal.PtrToStructure (ptr_opt, typeof (PPD_OPTION));

			ptr_choice = option.choices;
			for (int c = 0; c < option.num_choices; c++) {
				choice = (PPD_CHOICE) Marshal.PtrToStructure (ptr_choice, typeof (PPD_CHOICE));
				ptr_choice = new IntPtr (ptr_choice.ToInt64 () + Marshal.SizeOf (choice));
				if (name.Equals (choice.choice)) {
					// Special case for custom size (cups returns NULL for it)
					if (name == "Custom" && choice.text == null)
						rslt = "Custom";
					else
						rslt = choice.text;

					break;
				}
			}
			return rslt;
		}

		private PaperKind GetPaperKind (int width, int height)
		{
			if (width == 827 && height == 1169)
				return PaperKind.A4;
			if (width == 583 && height == 827)
				return PaperKind.A5;
			if (width == 717 && height == 1012)
				return PaperKind.B5;
			if (width == 693 && height == 984)
				return PaperKind.B5Envelope;
			if (width == 638 && height == 902)
				return PaperKind.C5Envelope;
			if (width == 449 && height == 638)
				return PaperKind.C6Envelope;
			if (width == 1700 && height == 2200)
				return PaperKind.CSheet;
			if (width == 433 && height == 866)
				return PaperKind.DLEnvelope;
			if (width == 2200 && height == 3400)
				return PaperKind.DSheet;
			if (width == 3400 && height == 4400)
				return PaperKind.ESheet;
			if (width == 725 && height == 1050)
				return PaperKind.Executive;
			if (width == 850 && height == 1300)
				return PaperKind.Folio;
			if (width == 850 && height == 1200)
				return PaperKind.GermanStandardFanfold;
			if (width == 1700 && height == 1100)
				return PaperKind.Ledger;
			if (width == 850 && height == 1400)
				return PaperKind.Legal;
			if (width == 927 && height == 1500)
				return PaperKind.LegalExtra;
			if (width == 850 && height == 1100)
				return PaperKind.Letter;
			if (width == 927 && height == 1200)
				return PaperKind.LetterExtra;
			if (width == 850 && height == 1269)
				return PaperKind.LetterPlus;
			if (width == 387 && height == 750)
				return PaperKind.MonarchEnvelope;
			if (width == 387 && height == 887)
				return PaperKind.Number9Envelope;
			if (width == 413 && height == 950)
				return PaperKind.Number10Envelope;
			if (width == 450 && height == 1037)
				return PaperKind.Number11Envelope;
			if (width == 475 && height == 1100)
				return PaperKind.Number12Envelope;
			if (width == 500 && height == 1150)
				return PaperKind.Number14Envelope;
			if (width == 363 && height == 650)
				return PaperKind.PersonalEnvelope;
			if (width == 1000 && height == 1100)
				return PaperKind.Standard10x11;
			if (width == 1000 && height == 1400)
				return PaperKind.Standard10x14;
			if (width == 1100 && height == 1700)
				return PaperKind.Standard11x17;
			if (width == 1200 && height == 1100)
				return PaperKind.Standard12x11;
			if (width == 1500 && height == 1100)
				return PaperKind.Standard15x11;
			if (width == 900 && height == 1100)
				return PaperKind.Standard9x11;
			if (width == 550 && height == 850)
				return PaperKind.Statement;
			if (width == 1100 && height == 1700)
				return PaperKind.Tabloid;
			if (width == 1487 && height == 1100)
				return PaperKind.USStandardFanfold;

			return PaperKind.Custom;
		}

		internal override void GetPrintDialogInfo (string printer, ref string port, ref string type, ref string status, ref string comment)
		{			
			int printers, state = -1;
			CUPS_DESTS cups_dests;
			CUPS_OPTIONS options;
			string str;
			IntPtr dests = IntPtr.Zero, ptr_printers, ptr_printer, ptr_options;
			int cups_dests_size = Marshal.SizeOf(typeof(CUPS_DESTS));
			int options_size = Marshal.SizeOf(typeof(CUPS_OPTIONS));
			
			if (cups_installed == false)
				return;

			printers = cupsGetDests (ref dests);

			ptr_printers = dests;
			for (int i = 0; i < printers; i++) {
				cups_dests = (CUPS_DESTS) Marshal.PtrToStructure (ptr_printers, typeof (CUPS_DESTS));
				str = Marshal.PtrToStringAnsi (cups_dests.name);
				ptr_printers = new IntPtr (ptr_printers.ToInt64 () + cups_dests_size);
				if (str != printer)
					continue;

				ptr_options = cups_dests.options;
				for (int o = 0; o < cups_dests.num_options; o ++) {
					options = (CUPS_OPTIONS) Marshal.PtrToStructure (ptr_options, typeof (CUPS_OPTIONS));
					str = Marshal.PtrToStringAnsi (options.name);					
					if (str == "printer-state") {
						state = Int32.Parse (Marshal.PtrToStringAnsi (options.val));
					} else {
						if (str == "printer-info")
							comment = Marshal.PtrToStringAnsi (options.val);
					}
					ptr_options = new IntPtr (ptr_options.ToInt64 () + options_size);
				}
				
			}

			Marshal.FreeHGlobal (dests);

			if (state == 4) {
				status = "Printing";
			}
			else {
				if (state == 5) {
					status = "Stopped";
				}
				else {
					status =  "Ready";
				}
			}			
		}

		//
		// DllImports
		//

		[DllImport("libcups", CharSet=CharSet.Ansi)]
		static extern int cupsGetDests (ref IntPtr dests);

		[DllImport("libcups", CharSet=CharSet.Ansi)]
		static extern IntPtr cupsTempFile (StringBuilder sb, int len);

		[DllImport("libcups", CharSet=CharSet.Ansi)]
		static extern IntPtr cupsGetDefault ();

		[DllImport("libcups", CharSet=CharSet.Ansi)]
		static extern int cupsPrintFile (string printer, string filename, string title, int num_options, IntPtr options);

		[DllImport("libcups", CharSet=CharSet.Ansi)]
		static extern IntPtr cupsGetPPD (string printer);

		[DllImport("libcups", CharSet=CharSet.Ansi)]
		static extern IntPtr ppdOpenFile (string filename);

		[DllImport("libcups", CharSet=CharSet.Ansi)]
		static extern IntPtr ppdFindOption (IntPtr ppd_file, string keyword);

		[DllImport("libcups")]
		static extern void ppdClose (IntPtr ppd);

		[DllImport ("libcups", CharSet=CharSet.Ansi)]
		static extern int cupsParseOptions (string arg, int number_of_options, ref IntPtr options);

		[DllImport("libcups")]
		static extern void cupsFreeOptions (int number_options, IntPtr options);

		[DllImport("gdiplus.dll", CharSet=CharSet.Ansi)]
		static extern int GdipGetPostScriptGraphicsContext (string filename, int with, int height, double dpix, double dpiy, ref IntPtr graphics);

		[DllImport("gdiplus.dll")]
		static extern int GdipGetPostScriptSavePage (IntPtr graphics);


		//Struct
		public struct DOCINFO
		{
 			public PrinterSettings settings;
			public PageSettings default_page_settings;
			public string title;
			public string filename;
  		}

		public struct PPD_SIZE
		{
			public	int marked;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst=42)]
			public	string name;
			public  float width;
			public  float length;
			public	float left;
			public  float bottom;
			public	float right;
			public 	float top;
		}

		public struct PPD_GROUP
		{
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst=40)]
			public string text;
  			[MarshalAs(UnmanagedType.ByValTStr, SizeConst=42)]
			public string name;
  			public int num_options;
			public IntPtr options;
 			public int num_subgroups;
			public IntPtr subgrups;
		}

		public struct PPD_OPTION
		{
			public byte 	conflicted;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst=41)]
			public string	keyword;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst=41)]
			public string 	defchoice;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst=81)]
			public string 	text;
			public int	ui;
  			public int 	section;
  			public float	order;
  			public int	num_choices;
  			public IntPtr	choices;
		}

		public struct PPD_CHOICE
		{
			public byte 	marked;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst=41)]
			public string	choice;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst=81)]
			public string 	text;
			public IntPtr	code;
			public IntPtr	option;
		}

		public struct PPD_FILE
		{
  			public int	language_level;
			public int	color_device;
			public int	variable_sizes;
			public int	accurate_screens;
			public int	contone_only;
			public int	landscape;
			public int	model_number;
			public int	manual_copies;
			public int	throughput;
  			public int	colorspace;
  			public IntPtr 	patches;
			public int	num_emulations;
  			public IntPtr	emulations;
			public IntPtr 	jcl_begin;
			public IntPtr 	jcl_ps;
			public IntPtr 	jcl_end;
			public IntPtr 	lang_encoding;
			public IntPtr 	lang_version;
			public IntPtr 	modelname;
			public IntPtr 	ttrasterizer;
			public IntPtr 	manufacturer;
			public IntPtr 	product;
			public IntPtr 	nickname;
			public IntPtr 	shortnickname;
  			public int	num_groups;
			public IntPtr	groups;
  			public int	num_sizes;
  			public IntPtr	sizes;

			/* There is more data after this that we are not using*/
		}


		public struct CUPS_OPTIONS
		{
		  	public IntPtr name;
		  	public IntPtr val;
		}
		
		public struct CUPS_DESTS
		{
			public IntPtr	name;
			public IntPtr	instance;
			public int 	is_default;
			public int	num_options;
			public IntPtr	options;
		}
	}
}

