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
		internal const  int MAX_PATH = 1024*8;

		string fileName;
		bool checkFileExists;
		bool addExtencsion;
		string defaultExt;
		int filterIndex;
		string filter;
		bool checkPathExists;
		bool dereferenceLinks;
		bool showHelp;
		string title;
		string initialDirectory;
		bool restoreDirectory;
		bool validateNames;

		internal FileDialog ( )
		{
			setDefaults ( );
		}
 
		public bool AddExtension {
			get { return addExtencsion;  }
			set { addExtencsion = value; }
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
			get { return fileName;  }
			set { fileName = value; }
		}

		[MonoTODO]
		public string[] FileNames {
			get {
				throw new NotImplementedException ();
			}
		}

		public string Filter {
			get { return filter; }
			set { 
				if ( value.Length > 0 ) {
					int sepNum = getSeparatorsCount ( value );
					if ( ( sepNum / 2 ) * 2 == sepNum ) {
						// number of separators should be odd
						throw new ArgumentException ("Parameter format is invalid");
					}
				}
				filter = value;
			}
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
			get { return title; }
			set { title = value;}
		}

		public bool ValidateNames {
			get { return validateNames;  }
			set { validateNames = value; }
		}

		public override void Reset()
		{
			setDefaults ( );
		}

		[MonoTODO]
		public override string ToString()
		{
			//FIXME:
			return base.ToString();
		}

		public event CancelEventHandler FileOk;

		[MonoTODO]
		protected  override IntPtr HookProc( IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam )
		{
			switch ( msg ) {
			case ( int ) Msg.WM_NOTIFY:
				OFNOTIFY ofnhdr = ( OFNOTIFY )Marshal.PtrToStructure ( lparam, typeof ( OFNOTIFY ) );
				
				switch ( ofnhdr.code ) {
				case ( int ) CommDlgNotifications.CDN_FILEOK:
					OPENFILENAME ofn = ( OPENFILENAME ) Marshal.PtrToStructure ( ofnhdr.lpOFN, typeof ( OPENFILENAME ) );
					string oldFileName = FileName;
					FileName = Win32.wine_get_unix_file_name(ofn.lpstrFile);
					CancelEventArgs e = new CancelEventArgs ( false );
					OnFileOk ( e );
					if ( e.Cancel ){
						Win32.SetWindowLong( hWnd, GetWindowLongFlag.DWL_MSGRESULT, 1 );
						return ( IntPtr ) 1 ;
					}
				break;
				case ( int ) CommDlgNotifications.CDN_HELP:
					OnHelpRequest ( EventArgs.Empty );
				break;
				}
			break;
			}
			return base.HookProc(hWnd, msg, wparam, lparam);
		}

		protected  void OnFileOk( CancelEventArgs e )
		{
			if ( FileOk != null )
				FileOk ( this, e );
		}

		[MonoTODO]
		protected  override bool RunDialog( IntPtr hWndOwner )
		{
			OPENFILENAME opf = new OPENFILENAME (  );
			opf.hwndOwner = hWndOwner;

			initOpenFileName ( ref opf );

			bool res = Win32.GetOpenFileName ( ref opf );
			if ( res )
				FileName = Win32.wine_get_unix_file_name(opf.lpstrFile);
			else {
				uint error = Win32.CommDlgExtendedError ( );
				if ( error != 0 ) {
					string errorMes = string.Empty;
					switch ( error ) {
					case (uint)CommDlgErrors.FNERR_BUFFERTOOSMALL:
						errorMes = "Too many files selected. Please select fewer files and try again.";
					break;
					case (uint)CommDlgErrors.FNERR_INVALIDFILENAME:
						errorMes = "A file name is invalid.";
					break;
					}
					throw new InvalidOperationException( errorMes );
				}
			}
			return res;
		}

		protected virtual void initOpenFileName ( ref OPENFILENAME opf ) 
		{
			opf.lStructSize  = (uint)Marshal.SizeOf( opf );
			char[] FileNameBuffer = new char[MAX_PATH];
			opf.lpstrFile = new string( FileNameBuffer );
			opf.nMaxFile = (uint) opf.lpstrFile.Length;
			opf.lpfnHook = new Win32.FnHookProc ( this.HookProc );
			opf.Flags = (int) ( OpenFileDlgFlags.OFN_ENABLEHOOK | OpenFileDlgFlags.OFN_EXPLORER );

			// convert filter to the proper format accepted by GetOpenFileName
			int FilterLength =  Filter.Length;
			if ( FilterLength > 0 ) {
				char[] FilterString = new char[ FilterLength + 1 ];
				Filter.CopyTo ( 0, FilterString, 0, FilterLength );
				int index = Array.IndexOf ( FilterString, '|' );
				while ( index != -1 ) {
					FilterString [ index ] = '\0';
					index = Array.IndexOf ( FilterString, '|', index );
				}
				FilterString [ FilterLength ] = '\0';
				opf.lpstrFilter = new string ( FilterString );
			}

			if ( FilterIndex >= 0 )
				opf.nFilterIndex = (uint)FilterIndex;

			opf.lpstrDefExt = DefaultExt;
			opf.lpstrTitle = Title;
			opf.lpstrInitialDir = InitialDirectory;

			if ( CheckFileExists )
				opf.Flags |= (int) ( OpenFileDlgFlags.OFN_FILEMUSTEXIST );
			if ( CheckPathExists )
				opf.Flags |= (int) ( OpenFileDlgFlags.OFN_PATHMUSTEXIST );
			if ( !DereferenceLinks )
				opf.Flags |= (int) ( OpenFileDlgFlags.OFN_NODEREFERENCELINKS );
			if ( ShowHelp )
				opf.Flags |= (int) ( OpenFileDlgFlags.OFN_SHOWHELP );
			if ( RestoreDirectory )
				opf.Flags |= (int) ( OpenFileDlgFlags.OFN_NOCHANGEDIR );
			if ( !ValidateNames )
				opf.Flags |= (int) ( OpenFileDlgFlags.OFN_NOVALIDATE );
		}

		private int getSeparatorsCount ( string filter )
		{
			int sepNum = 0;
			foreach ( char c in filter )
				if ( c == '|' ) sepNum++;
			return sepNum;
		}
		
		private void setDefaults ( ) 
		{
			fileName = string.Empty;
			checkFileExists = false;
			addExtencsion = true;
			defaultExt = string.Empty;
			filter = string.Empty;
			filterIndex = 1;
			checkPathExists = true;
			dereferenceLinks = true;
			showHelp = false;
			title = string.Empty;
			initialDirectory = string.Empty;
			restoreDirectory = false;
			validateNames = true;
		}
	 }
}
