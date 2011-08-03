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
// Copyright (c) 2004 Novell, Inc.
//
// Authors:
//	Dennis Hayes	(dennish@raytek.com)

using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace System.Windows.Forms.Design {
	[ToolboxItem (false)]
	[ClassInterfaceAttribute (ClassInterfaceType.AutoDispatch)]
	[ComVisible (true)]
	public class ComponentEditorForm : Form {

		[MonoTODO]
		public ComponentEditorForm(object component, Type[] pageTypes){
		}

		#region Public Properties
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		new public virtual bool AutoSize {
			get { return base.AutoSize; }
			set { base.AutoSize = value; }
		}
		#endregion
		
		[MonoTODO]
		protected override void OnActivated(EventArgs e){
		}

		[MonoTODO]
		protected virtual void OnSelChangeSelector(object source, TreeViewEventArgs e){
		}

		[MonoTODO]
		public override bool PreProcessMessage(ref Message msg){
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual DialogResult ShowForm(){
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual DialogResult ShowForm(int page){
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual DialogResult ShowForm(IWin32Window owner){
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual DialogResult ShowForm(IWin32Window owner, int page){
			throw new NotImplementedException ();
		}
		[MonoTODO]
		// can't override the function in control. bug in compiler. Fixed?
		protected override void OnHelpRequested(HelpEventArgs e){
		}

		#region Public Events
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler AutoSizeChanged {
			add { base.AutoSizeChanged += value; }
			remove { base.AutoSizeChanged -= value; }
		}
		#endregion
	}
}
