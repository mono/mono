//
//		System.Windows.Forms.TabPage
//
//		Author:
//			Alberto Fernandez		(infjaf00@yahoo.es)
//

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;

namespace System.Windows.Forms{

	public class TabPage : Panel{
		[MonoTODO]
		public TabPage() : this (""){
		}
		[MonoTODO]
		public TabPage(string text){
			this.Text = text;
		}
		[MonoTODO]
		public override AnchorStyles Anchor {
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}
		[MonoTODO]
		public override DockStyle Dock {
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}
		[MonoTODO]
		public new bool Enabled {
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }		
		}
		[MonoTODO]
		public int ImageIndex {
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }		
		}

		//No usar
		public new int TabIndex {
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}
		public new bool TabStop {
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		[MonoTODO]
		public override string Text {
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}
		[MonoTODO]
		public string ToolTipText {
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }		
		}

		//No usar
		public new bool Visible {
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		[MonoTODO]
		protected override ControlCollection CreateControlsInstance(){
			return new TabPageControlCollection(this);
		}
		[MonoTODO]
		public static TabPage GetTabPageOfComponent(object comp){
			throw new NotImplementedException();
		}
		[MonoTODO]
		protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified){
			throw new NotImplementedException();
		}
		public override string ToString(){
			return "TabPage: {" + Text + "}";
		}

		public new event EventHandler DockChanged;
		public new event EventHandler EnabledChanged;
		public new event EventHandler TabIndexChanged;
		public new event EventHandler TabStopChanged;
		public new event EventHandler VisibleChanged;

		public class TabPageControlCollection : ControlCollection {
			public TabPageControlCollection ( Control owner ): base( owner ){ }

			public override void Add( Control c ) {
				if ( c is TabPage  ) {
					throw new ArgumentException();
				}
				base.Add(c);
			}
		}


		/*[MonoTODO]
		public class TabPage.TabPageControlCollection :  Control.ControlCollection{
			public TabPage.TabPageControlCollection( TabPage owner);
			public override void Add(Control value);
		}*/
	}
}
