//
// System.Windows.Forms.OpenFileDialog.cs
//
// Author:
//   stubbed out by Paul Osman (paul.osman@sympatico.ca)
//	Dennis Hayes (dennish@raytek.com)
//	Aleksey Ryabchuk ( ryabchuk@yahoo.com )
// (C) 2002 Ximian, Inc
//
using System.ComponentModel;
using System.IO;
using System.Runtime.Remoting;
using System;
namespace System.Windows.Forms {

	// <summary>
	// Represents a common dialog box that displays the control
	// that allows the user to open a file.
	// </summary>

	public sealed class OpenFileDialog : FileDialog {

		bool showReadOnly;
		bool readOnlyChecked;

		public OpenFileDialog(){			
			Reset();
		}

		public bool Multiselect {
			get { return (Dialog as Gtk.FileSelection).SelectMultiple; }
			set { (Dialog as Gtk.FileSelection).SelectMultiple = value;}
		}
		[MonoTODO]
		public bool ReadOnlyChecked {
			get { return readOnlyChecked; }
			set { readOnlyChecked = value; }
		}
		[MonoTODO]
		public bool ShowReadOnly {
			get { return showReadOnly; }
			set { showReadOnly = value;}
		}

		[MonoTODO]
		public Stream OpenFile() {
			return new FileStream ( FileName, FileMode.Open, FileAccess.Read );
		}

		public override void Reset() {
			base.Reset ( );
			CheckFileExists = true;
			Multiselect = false;
			ShowReadOnly = false;
			ReadOnlyChecked = false;
		}
		
		[MonoTODO]
		internal override Gtk.Dialog CreateDialog (){
			Gtk.FileSelection s = new Gtk.FileSelection ("FileSelection - Open");
			s.CancelButton.Clicked += new EventHandler (BtnCancelClicked);
			s.OkButton.Clicked += new EventHandler (BtnOkClicked);
			s.DeleteEvent += new GtkSharp.DeleteEventHandler (OnDelete);
			s.Response += new GtkSharp.ResponseHandler (OnResponse);
			return s;
		}
		[MonoTODO]
		internal void BtnCancelClicked (object sender, EventArgs args){
		}
		[MonoTODO]
		internal void BtnOkClicked (object sender, EventArgs args){
		}
	}
}
