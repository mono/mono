//
// System.Windows.Forms.Design.ComponentEditorForm.cs
//
// Author:
//   Dennis Hayes (dennish@raytek.com)
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//
using System;
using System.Windows.Forms;
namespace System.Windows.Forms.Design {
	/// <summary>
	/// Summary description for ComponentEditorForm.
	/// </summary>
	public class ComponentEditorForm : Form {

		[MonoTODO]
		public ComponentEditorForm(object component, Type[] pageTypes){
		}

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
	}
}
