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
	//
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

		[MonoTODO]
		public string InitialDirectory {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool RestoreDirectory {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public bool ShowHelp {
			get { return showHelp;  }
			set { showHelp = value; }
		}

		[MonoTODO]
		public string Title {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool ValidateNames {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
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
			//FIXME:
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
				FileName = opf.lpstrFile;
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
			opf.lpfnHook = new Win32.FnHookProc ( base.HookProc );
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

			if ( CheckFileExists )
				opf.Flags |= (int) ( OpenFileDlgFlags.OFN_FILEMUSTEXIST );
			if ( CheckPathExists )
				opf.Flags |= (int) ( OpenFileDlgFlags.OFN_PATHMUSTEXIST );
			if ( !DereferenceLinks )
				opf.Flags |= (int) ( OpenFileDlgFlags.OFN_NODEREFERENCELINKS );
			if ( ShowHelp )
				opf.Flags |= (int) ( OpenFileDlgFlags.OFN_SHOWHELP );
		}

		private int getSeparatorsCount ( string filter )
		{
			int sepNum = 0;
			int index = filter.IndexOf ( '|' );
			while ( index != -1 ) {
				sepNum ++;
				if ( index < filter.Length )
					index ++;
				index = filter.IndexOf ( '|', index );
			}
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
		}
	 }
}
