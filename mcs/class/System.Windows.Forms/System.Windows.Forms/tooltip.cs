//
// System.Windows.Forms.ToolTip
//
// Author:
//   stubbed out by Jackson Harper (jackson@latitudegeo.com)
//	Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc
//
using System.ComponentModel;
namespace System.Windows.Forms {

	// <summary>
	//
	// </summary>

	public sealed class ToolTip : Component, IExtenderProvider {

		//
		//  --- Public Constructors
		//
		[MonoTODO]
		public ToolTip() {
			
		}

		[MonoTODO]
		public ToolTip(IContainer cont) {
			
		}
		//
		// --- Public Properties
		//
		[MonoTODO]
		public bool Active {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}	
		}

		[MonoTODO]
		public int AutomaticDelay {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}	
		}

		[MonoTODO]
		public int AutoPopDelay{
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public int InitialDelay {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public int ReshowDelay {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public bool ShowAlways {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		// --- Public Methods
		
		[MonoTODO]
		public void RemoveAll() {
			//FIXME:
		}
		[MonoTODO]
		public void SetToolTip(Control control, string caption) {
			//FIXME:
		}
		[MonoTODO]
		public override string ToString() {
			throw new NotImplementedException ();
		}
		//
		// --- Protected Methods
		//

		[MonoTODO]
		~ToolTip() {
			
		}
		bool IExtenderProvider.CanExtend(object extendee){
			throw new NotImplementedException ();
		}

	}
}
