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
// Copyright (c) 2005 Novell, Inc.
//
// Authors:
//	Someone
//	Jonathan Chambers (jonathan.chambers@ansys.com)
//

using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.Reflection;

namespace System.Windows.Forms
{
	[DefaultProperty("Document")]
	public sealed class PrintDialog : CommonDialog {
		PrintDocument document;
		PrinterSettings printer_settings;
		bool allow_current_page;
		bool allow_print_to_file;
		bool allow_selection;
		bool allow_some_pages;
		bool show_help;
		bool show_network;
		bool print_to_file;
		
		public PrintDialog ()
		{
			allow_print_to_file = true;
			show_network = true;
		}

		public override void Reset ()
		{
		}

#if NET_2_0
		public bool AllowCurrentPage {
			get {
				return allow_current_page;
			}

			set {
				allow_current_page = value;
			}
		}
#endif

		[DefaultValue(true)]
		public bool AllowPrintToFile {
			get {
				return allow_print_to_file;
			}

			set {
				allow_print_to_file = value;
			}
		}

		[DefaultValue(false)]
		public bool AllowSelection {
			get {
				return allow_selection;
			}

			set {
				allow_selection = value;
			}
		}

		[DefaultValue(false)]
		public bool AllowSomePages {
			get {
				return allow_some_pages;
			}

			set {
				allow_some_pages = value;
			}
		}

		[DefaultValue(null)]
		public PrintDocument Document {
			get {
				return document;
			}

			set {
				document = value;
			}
		}

		[Browsable(false)]
		[DefaultValue(null)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public PrinterSettings PrinterSettings {
			get {
				return printer_settings;
			}

			set {
				printer_settings = value;
			}
		}

		[DefaultValue(false)]
		public bool PrintToFile {
			get {
				return print_to_file;
			}

			set {
				print_to_file = value;
			}
		}

		[DefaultValue(true)]
		public bool ShowNetwork {
			get {
				return show_network;
			}

			set {
				show_network = value;
			}
		}

		[DefaultValue(false)]
		public bool ShowHelp {
			get {
				return show_help;
			}

			set {
				show_help = value;
			}
		}

		protected override bool RunDialog (IntPtr hwnd)
		{
			return true;
		}
	}
}
