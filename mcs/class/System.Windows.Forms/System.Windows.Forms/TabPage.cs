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

		public class PageControlsControlCollection : ControlCollection {

			public PageControlsControlCollection ( Control owner ): base( owner ){ }

			public override void Add( Control c ) {
				if ( c is TabPage  ) {
					throw new ArgumentException();
				}
				base.Add(c);
			}
		}

		//
		//  --- Public Constructor
		//
		[MonoTODO]
		public TabPage() {
			//FIXME:
		}

		[EditorBrowsable (EditorBrowsableState.Never)]	 
		public override AnchorStyles Anchor {
			get {	return base.Anchor; }
			set {	base.Anchor = value;}	
		}

		[EditorBrowsable (EditorBrowsableState.Never)]	 
		public override DockStyle Dock {
			get {	return base.Dock; }
			set {	base.Dock = value;}
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
			get {	return base.Text; }
			set {
				base.Text = value;
				if ( Parent != null && Parent is TabControl ) {
					( ( TabControl ) Parent ).pageTextChanged ( this );
				}
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
		
		protected override ControlCollection CreateControlsInstance() {
			return new PageControlsControlCollection ( this );
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
