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
// Copyright (c) 2004-2005 Novell, Inc. (http://www.novell.com)
//
// Authors:
//
//  Alexander Olk	xenomorph2@onlinehome.de
//

// NOT COMPLETE - work in progress

using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;

namespace System.Windows.Forms
{
	[Designer("System.Windows.Forms.Design.OpenFileDialogDesigner, " + Consts.AssemblySystem_Design)]
	public sealed class OpenFileDialog : FileDialog
	{
		#region Public Constructors
		public OpenFileDialog( )
		{
			form.Text = "Open";
			
			form.Size =  new Size( 554, 405 ); // 384
			
			OpenSaveButtonText = "Open";
			
			SearchSaveLabelText = "Search in:";
			
			checkFileExists = true;
			
			fileDialogType = FileDialogType.OpenFileDialog;
			
			fileDialogPanel = new FileDialogPanel( this );
		}
		#endregion	// Public Constructors
		
		#region Public Instance Properties
		[DefaultValue(true)]
		public override bool CheckFileExists
		{
			get
			{
				return checkFileExists;
			}
			
			set
			{
				checkFileExists = value;
			}
		}
		
		[DefaultValue(false)]
		public new bool Multiselect
		{
			get
			{
				return multiSelect;
			}
			
			set
			{
				base.Multiselect = value;
			}
		}
		
		[DefaultValue(false)]
		public new bool ReadOnlyChecked
		{
			get
			{
				return readOnlyChecked;
			}
			
			set
			{
				base.ReadOnlyChecked = value;
			}
		}
		
		[DefaultValue(false)]
		public new bool ShowReadOnly
		{
			get
			{
				return showReadOnly;
			}
			
			set
			{
				base.ShowReadOnly = value;
			}
		}
		
		#endregion	// Public Instance Properties
		
		#region Public Instance Methods
		public Stream OpenFile( )
		{
			if ( FileName == null )
				throw new ArgumentNullException( "OpenFile", "FileName is null" );
			
			return new FileStream( FileName, FileMode.Open, FileAccess.Read );
		}
		#endregion	// Public Instance Methods
		
		public override void Reset( )
		{
			base.Reset( );
			multiSelect = false;
			checkFileExists = true;
			readOnlyChecked = false;
			ReadOnlyChecked = readOnlyChecked;
			showReadOnly = false;
			ShowReadOnly = showReadOnly;
		}
	}
}


