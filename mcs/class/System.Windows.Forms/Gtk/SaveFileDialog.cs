//
// System.Windows.Forms.SaveFileDialog.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc
//

using System.ComponentModel;
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
		// pregunta si queremos crear un archivo nuevo.
		[MonoTODO]
		public bool CreatePrompt {
			get {return createPrompt;}
			set {createPrompt = value;}
		}
		// pregunta si queremos sobreescribir.
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
		public override string ToString(){
			return String.Format (
				"System.Windows.Forms.SaveFileDialog: Title: {0}, FileName: {1}",
				this.Title,
				this.FileName);
		}
		
		[MonoTODO]
		internal override Gtk.Dialog CreateDialog (){
			return new Gtk.FileSelection (String.Empty);
		}
		
		
	 }
}
