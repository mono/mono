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
// Copyright (c) 2006 Alexander Olk
//
// Authors:
//
//  Alexander Olk	alex.olk@googlemail.com
//

// NOT COMPLETE - work in progress

using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;

namespace System.Windows.Forms {
	public sealed class OpenFileDialog : FileDialog {
		#region Public Constructors
		public OpenFileDialog ()
		{
			form.SuspendLayout ();
			
			form.Text = "Open";
			
			CheckFileExists = true;
			
			OpenSaveButtonText = "Open";
			SearchSaveLabel = "Look in:";
			fileDialogType = FileDialogType.OpenFileDialog;
			
			form.ResumeLayout (false);
		}
		#endregion	// Public Constructors
		
		#region Public Instance Properties
		[DefaultValue(true)]
		public override bool CheckFileExists {
			get {
				return base.CheckFileExists;
			}
			
			set {
				base.CheckFileExists = value;
			}
		}
		
		[DefaultValue(false)]
		public bool Multiselect {
			get {
				return base.BMultiSelect;
			}
			
			set {
				base.BMultiSelect = value;
			}
		}
		
		[DefaultValue(false)]
		public new bool ReadOnlyChecked {
			get {
				return base.ReadOnlyChecked;
			}
			
			set {
				base.ReadOnlyChecked = value;
			}
		}
		
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public string SafeFileName {
			get { return Path.GetFileName (FileName); }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public string[] SafeFileNames {
			get {
				string[] files = FileNames;
				
				for (int i = 0; i < files.Length; i++)
					files[i] = Path.GetFileName (files[i]);
					
				return files;
			 }
		}

		[DefaultValue(false)]
		public new bool ShowReadOnly {
			get {
				return base.ShowReadOnly;
			}
			
			set {
				base.ShowReadOnly = value;
			}
		}
		
		#endregion	// Public Instance Properties
		
		#region Public Instance Methods
		public Stream OpenFile ()
		{
			if (FileName.Length == 0)
				throw new ArgumentNullException ("OpenFile", "FileName is null");
			
			return new FileStream (FileName, FileMode.Open, FileAccess.Read);
		}
		#endregion	// Public Instance Methods
		
		public override void Reset ()
		{
			base.Reset ();
			base.BMultiSelect = false;
			base.CheckFileExists = true;
			base.ReadOnlyChecked = false;
			base.ShowReadOnly = false;
		}

		internal override string DialogTitle {
			get {
				string title = base.DialogTitle;
				if (title.Length == 0)
					title = "Open";
				return title;
			}
		}
	}
}


