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

		bool multiselect;
		bool showReadOnly;
		bool readOnlyChecked;

		public OpenFileDialog()
		{
			setDefaults ( );
		}

		public bool Multiselect {
			get { return multiselect; }
			set { multiselect = value;}
		}

		public bool ReadOnlyChecked {
			get { return readOnlyChecked; }
			set { readOnlyChecked = value; }
		}

		public bool ShowReadOnly {
			get { return showReadOnly; }
			set { showReadOnly = value;}
		}

		public override bool CheckFileExists {
			get { return base.CheckFileExists; }
			set { base.CheckFileExists = value;}
		}

		[MonoTODO]
		public Stream OpenFile() {
			return new FileStream ( FileName, FileMode.Open, FileAccess.Read );
		}

		public override void Reset() {
			base.Reset ( );
			setDefaults ( );
		}

		protected override void initOpenFileName ( ref OPENFILENAME opf )
		{
			base.initOpenFileName ( ref opf );

			if ( Multiselect ) opf.Flags |= ( int ) ( OpenFileDlgFlags.OFN_ALLOWMULTISELECT );
			if ( ShowReadOnly ) {
				opf.Flags &= ~( ( uint ) ( OpenFileDlgFlags.OFN_HIDEREADONLY ) );
				if ( ReadOnlyChecked )
					opf.Flags |= ( int ) ( OpenFileDlgFlags.OFN_READONLY );
			}
			else
				opf.Flags |= (int)OpenFileDlgFlags.OFN_HIDEREADONLY;

		}

		private void setDefaults ( )
		{
			CheckFileExists	= true;
			multiselect = false;
			showReadOnly = false;
			readOnlyChecked = false;
		}
	}
}
