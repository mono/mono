//
// System.Windows.Forms.OpenFileDialog.cs
//
// Author:
//   stubbed out by Paul Osman (paul.osman@sympatico.ca)
//	Dennis Hayes (dennish@raytek.com)
//	Aleksey Ryabchuk ( ryabchuk@yahoo.com )
// (C) 2002 Ximian, Inc
//

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

		internal override void initOpenFileName ( ref OPENFILENAME opf )
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
