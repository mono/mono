//
// System.Windows.Forms.FolderBrowserDialog.cs
//
// Author:
//   Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc
//

namespace System.Windows.Forms {

	// <summary>
	//
	// </summary>
	using System.Runtime.Remoting;
	using System.ComponentModel;


	// Beta specs do not specify what class to defrive from.
	// Using CommonDialog because 
	public class FolderBrowserDialog : CommonDialog  {

		string description;

		//
		//  --- Constructor
		//
		[MonoTODO]
		public FolderBrowserDialog() {
			description = "";
		}

		[MonoTODO]
		public override void Reset(){
			//
		}

		[MonoTODO]
		protected override bool RunDialog(IntPtr hWndOwner){
			throw new NotImplementedException ();
		}

		//
		//  --- Public Properties
		//

		public string Description {
			get {
				return description;
			}
			set {
				description = value;
			}
		}

		//beta docs do not have accessor.
		//protected bool DesignMode {
		//}

		//protected EventHandlerList Events {
		//}

		public Environment.SpecialFolder RootFolder {
			get {
				throw new NotImplementedException ();
			}
			set {
			//FIXME:
			}
		}

		public string SelectedPath {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}

		public bool ShowNewFolderButton {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}


		//public virtual System.ComponentModel.IContainer Container {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//}

		// FIXME: beta 1.1 says the following should be public virtual ISite Site {
		// but the compiler gives warning that it must be new.
		// Probably system.component needs to change to be beta 1.1 compliant
		public new virtual ISite Site {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}

		//Specs seem to say they need to be here, but adding them conflicts with commondialog : component.disposed/helprequest
		//public event EventHandler Disposed;
		//public event EventHandler HelpRequest;

	}
}







