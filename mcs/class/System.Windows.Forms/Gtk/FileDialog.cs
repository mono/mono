//
// System.Windows.Forms.FileDialog.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//   Dennis Hayes (dennish@raytek.com)
//   Aleksey Ryabchuk (ryabchuk@yahoo.com)
// (C) 2002 Ximian, Inc
//
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace System.Windows.Forms {

	// <summary>
	// Displays a dialog window from which the user can select a file.
	// </summary>

    public abstract class FileDialog : CommonDialog {

		
		private string fileName;
		private bool addExtension;
		private bool checkFileExists;
		private bool checkPathExists;
		private string defaultExt;
		private bool dereferenceLinks;
		int filterIndex;
		string filter;
		
		
		bool showHelp;
		string initialDirectory;
		bool restoreDirectory;
		bool validateNames;

		// Don't use, ? 
		//protected static readonly object EventFileOk;
		
		protected FileDialog ( ):base(){	
			this.Reset();		
		}
 
		public bool AddExtension {
			get { return addExtension;  }
			set { addExtension = value; }
		}

		public virtual bool CheckFileExists {
			get { return checkFileExists;  }
			set { checkFileExists = value; }
		}

		public bool CheckPathExists {
			get { return checkPathExists;	}
			set { checkPathExists = value;	}
		}

		public string DefaultExt {
			get { return defaultExt;  }
			set { 
				if ( value.StartsWith ( "." ) )
					defaultExt = value.Remove( 0, 1 );
				else 
					defaultExt = value; 
			}
		}

		public bool DereferenceLinks {
			get { return dereferenceLinks;  }
			set { dereferenceLinks = value; }
		}

		public string FileName {
			get { return (Dialog as Gtk.FileSelection).Filename;}
			set { (Dialog as Gtk.FileSelection).Filename = value;}
		}

		[MonoTODO]
		public string[] FileNames {
			get {return new String[]{this.FileName};}
		}

		public string Filter {
			get { return filter; }
			set { filter = value;}
		}

		public int FilterIndex {
			get { return filterIndex;  }
			set { filterIndex = value; }
		}

		public string InitialDirectory {
			get { return initialDirectory;  }
			set { initialDirectory = value; }
		}

		public bool RestoreDirectory {
			get { return restoreDirectory;  }
			set { restoreDirectory = value; }
		}

		public bool ShowHelp {
			get { return showHelp;  }
			set { showHelp = value; }
		}

		public string Title {
			get { return Dialog.Title; }
			set { Dialog.Title = value;}
		}

		public bool ValidateNames {
			get { return validateNames;  }
			set { validateNames = value; }
		}

		public override void Reset(){
			fileName = string.Empty;
			checkFileExists = false;
			addExtension = true;
			defaultExt = string.Empty;
			filter = string.Empty;
			filterIndex = 1;
			checkPathExists = true;
			dereferenceLinks = true;
			showHelp = false;
			initialDirectory = string.Empty;
			restoreDirectory = false;
			validateNames = true;
		}

		[MonoTODO]
		public override string ToString(){
			return base.ToString();
		}

		public event CancelEventHandler FileOk;

	
		protected  void OnFileOk( CancelEventArgs e ){
			if ( FileOk != null )
				FileOk ( this, e );
		}

		[MonoTODO]
		protected  override bool RunDialog( IntPtr hWndOwner ){
			Dialog.Run();
			return dialogRetValue;
		}


	 }
}
