//
// System.Windows.Forms.FolderBrowserDialog.cs
//
// Author:
//   Dennis Hayes (dennish@raytek.com)
// Gtk.
//   Alberto Fernandez (infjaf00@yahoo.es)
//
// (C) 2002 Ximian, Inc
//

using System.Runtime.Remoting;
using System.ComponentModel;

namespace System.Windows.Forms {

	// <summary>
	//
	// </summary>
	


	// Beta specs do not specify what class to defrive from.
	// Using CommonDialog because 
	public class FolderBrowserDialog : CommonDialog  {
		string folder;
		string description;

		//
		//  --- Constructor
		//
		[MonoTODO]
		public FolderBrowserDialog() {
			description = String.Empty;
			folder = (Dialog as Gtk.FileSelection).Filename;
		}

		[MonoTODO]
		public override void Reset(){
			//
		}

		[MonoTODO]
		protected override bool RunDialog(IntPtr hWndOwner){
			this.Dialog.Run();
			folder = (Dialog as Gtk.FileSelection).Filename;
			return dialogRetValue;
		}

		//
		//  --- Public Properties
		//

		public string Description {
			get { return description; }
			set { description = value; }
		}

		[MonoTODO]
		public Environment.SpecialFolder RootFolder {
			get { return Environment.SpecialFolder.Desktop; }
			set {
				if (! Enum.IsDefined (typeof (Environment.SpecialFolder), value)){
					throw new InvalidEnumArgumentException();
				}
			}
		}

		public string SelectedPath {
			get { return (this.Dialog as Gtk.FileSelection).Filename; }
			set { (this.Dialog as Gtk.FileSelection).Filename = value; }
		}

		public bool ShowNewFolderButton {
			get { return (this.Dialog as Gtk.FileSelection).FileopCDir.Visible; }
			set { (this.Dialog as Gtk.FileSelection).FileopCDir.Visible = value;}
		}
		public override String ToString(){
			return "System.Windows.Forms.FolderBrowserDialog";
		}

		[MonoTODO]
		internal override Gtk.Dialog CreateDialog (){
			Gtk.FileSelection f = new Gtk.FileSelection(String.Empty);
			f.FileList.Sensitive = false;
			f.FileopDelFile.Visible = false;
			f.FileopRenFile.Visible = false;
			f.SelectionEntry.Visible = false;
			return f;
		}

		internal override void OnCancel (){
			(Dialog as Gtk.FileSelection).Filename = folder;
		}
	}
}
