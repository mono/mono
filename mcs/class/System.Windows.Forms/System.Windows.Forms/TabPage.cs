//
// System.Windows.Forms.TabPage
//
// Author:
//   stubbed out by Jackson Harper (jackson@latitudegeo.com)
//   Dennis Hayes (dennish@Raytek.com)
//   implemented by Aleksey Ryabchuk (ryabchuk@yahoo.com)
//
// (C) 2002 Ximian, Inc
//
using System.ComponentModel;
using System.Drawing;
namespace System.Windows.Forms {

	// <summary>
	// </summary>

	public class TabPage : Panel {

		public class TabPageControlCollection : ControlCollection {

			public TabPageControlCollection ( Control owner ): base( owner ){ }

			public override void Add( Control c ) {
				if ( c is TabPage  ) {
					throw new ArgumentException();
				}
				base.Add(c);
			}
		}

		private string toolTipText;

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
			get {	return toolTipText; }
			set {	toolTipText = value;}
		}
		
		[MonoTODO]
		public static TabPage GetTabPageOfComponent(object comp) {
			throw new NotImplementedException ();
		}

		public override string ToString() {
			return GetType().Name.ToString () + ": {" + Text + "}";
		}
		
		protected override ControlCollection CreateControlsInstance() {
			return new TabPageControlCollection ( this );
		}

		protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified) {
			Rectangle rect = Parent.DisplayRectangle;			
			base.SetBoundsCore(rect.Left, rect.Top, rect.Width, rect.Height, BoundsSpecified.All);
		}
	}
}
