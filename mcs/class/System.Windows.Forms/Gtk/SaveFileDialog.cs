//
// System.Windows.Forms.SaveFileDialog.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc
//
using System.IO;

namespace System.Windows.Forms {


	
	// <summary>
	// </summary>

    public sealed class SaveFileDialog : FileDialog {
	
		private bool createPrompt;
		private bool overwritePrompt;

		//
		//  --- Constructor
		//
		[MonoTODO]
		public SaveFileDialog(){
			Reset();				
		}

		//
		//  --- Public Properties
		//
		[MonoTODO]
		public bool CreatePrompt {
			get {return createPrompt;}
			set {createPrompt = value;}
		}
		[MonoTODO]
		public bool OverwritePrompt {
			get {return overwritePrompt;}
			set {overwritePrompt = value;}
		}

		//
		//  --- Public Methods
		//

		[MonoTODO]
		public Stream OpenFile(){
			return File.OpenWrite(FileName);
		}
		[MonoTODO]
		public override void Reset(){
			base.Reset();
			CheckFileExists = false;
		}
		
		[MonoTODO]
		internal override Gtk.Dialog CreateDialog (){
			Gtk.FileSelection s = new Gtk.FileSelection ("FileSelection - Save");
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
