//
// System.Windows.Forms.TabPage
//
// Author:
//   stubbed out by Jackson Harper (jackson@latitudegeo.com)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc
//
using System.ComponentModel;
namespace System.Windows.Forms {

	// <summary>
	// </summary>

	public class TabPage : Panel {

		//
		//  --- Public Constructor
		//
		[MonoTODO]
		public TabPage() {
			//FIXME:
		}
		//
		//  --- Public Properties
		//
		[MonoTODO]
		public override AnchorStyles Anchor {
			get {
				//FIXME:
				return base.Anchor;
			}
			set {
				//FIXME:
				base.Anchor = value;
			}	
		}
		[MonoTODO]
		public override DockStyle Dock {
			get {
				//FIXME:
				return base.Dock;
			}
			set {
				//FIXME:
				base.Dock = value;			}
		}
		[MonoTODO]
		public int ImageIndex {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		[MonoTODO]
		public override string Text  {
			get {
				//FIXME:
				return base.Text;
			}
			set {
				//FIXME:
				base.Text = value;
			}
		}
		[MonoTODO]
		public string ToolTipText  {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		
		//  --- Public Methods
		
		[MonoTODO]
		public static TabPage GetTabPageOfComponent(object comp) {
			//FIXME:
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public override string ToString() {
			//FIXME:
			return base.ToString();
		}
		
		//  --- Protected Methods
		
		[MonoTODO]
		protected override ControlCollection CreateControlsInstance() {
			//FIXME:
			return base.CreateControlsInstance();
		}
		[MonoTODO]
		protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified) {
			//FIXME:
			base.SetBoundsCore(x,y,width,height,specified);
		}
		// FIXME  dont compile
		//[MonoTODO]
		//public class TabPageControlCollection : Control.ControlCollection {
		//	//
		//	// --- Public Methods
		//	//
		//	public override void Add(Control value) {
		//		throw new NotImplementedException ();
		//	}
		//}
	}
}
