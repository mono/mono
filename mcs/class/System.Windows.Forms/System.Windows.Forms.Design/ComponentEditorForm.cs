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
		public ComponentEditorForm() {
			//
			// TODO: Add constructor logic here
			//
			throw new NotImplementedException ();
		}

		public ComponentEditorForm(object component, Type[] pageTypes){
			//
			// TODO: Add constructor logic here
			//
			throw new NotImplementedException ();
		}

		protected override void OnActivated(EventArgs e){
			throw new NotImplementedException ();
		}

		public override bool PreProcessMessage(ref Message msg){
			throw new NotImplementedException ();
		}

		public virtual DialogResult ShowForm(){
			throw new NotImplementedException ();
		}

		public virtual DialogResult ShowForm(int page){
			throw new NotImplementedException ();
		}

		public virtual DialogResult ShowForm(IWin32Window owner){
			throw new NotImplementedException ();
		}

		public virtual DialogResult ShowForm(IWin32Window owner, int page){
			throw new NotImplementedException ();
		}
		// can't override the function in control. bug in compiler
		//protected override void onHelpRequested(HelpEventArgs e){
		//	throw new NotImplementedException ();
		//}
	}
}
