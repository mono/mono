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

//
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
		private bool   added;
		private int    imageIndex;

		[MonoTODO]
		public TabPage() {
			added = false;
			imageIndex = -1;
		}

		public TabPage (string textValue) : this() {
			text = textValue;
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

		public int ImageIndex {
			get {	return imageIndex; }
			set {
				if ( value < -1 )
					throw new ArgumentException(
						string.Format("'{0}' is not a valid value for 'value'. 'value' must be greater than or equal to -1.",
						value ) ) ;

				if ( imageIndex != value ) {
					imageIndex = value;

					if ( Parent != null && Parent is TabControl ) {
						( ( TabControl ) Parent ).pageImageIndexChanged ( this );
					}
				}
			}
		}
		[MonoTODO]
		public override string Text  {
			get {	return text; }
			set {
				text = value;
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
			if ( Parent != null ) {
				Rectangle rect = Parent.DisplayRectangle;			
				base.SetBoundsCore(rect.Left, rect.Top, rect.Width, rect.Height, BoundsSpecified.All);
			}
			else
				base.SetBoundsCore( x, y, width, height, specified );
		}

		internal bool isAdded
		{
			get { return added; }
			set { added = value;}
		}

		protected override void SetVisibleCore ( bool value )
		{
			TabControl parent = Parent as TabControl;
			if ( parent != null && this != parent.SelectedTab )
				value = false;

			base.SetVisibleCore ( value );
		}
	}
}
