//
// System.Windows.Forms.ToolBarButton.cs
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
// Copyright (C) 2004 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Ravindra (rkumar@novell.com)
//
// TODO:
//     - DropDownMenu
//     - Adding a button to two toolbars
//
// $Revision: 1.9 $
// $Modtime: $
// $Log: ToolBarButton.cs,v $
// Revision 1.9  2004/10/14 11:10:47  ravindra
// 	- Added an internal member 'inside' to handle mouse move
// 	with mouse pressed ie mouse drag event.
// 	- Changed 'Pressed' property to return true only when
// 	'inside' and 'pressed' are both true.
// 	- Some coding style love.
//
// Revision 1.8  2004/10/05 08:42:20  ravindra
// Added an internal member dd_pressed to handle clicks on DropDown arrow.
//
// Revision 1.7  2004/09/09 11:23:05  ravindra
// Changes in ToolBarButton need to make it's parent redraw.
//
// Revision 1.6  2004/08/27 22:12:56  ravindra
// Added TypeConverter attribute.
//
// Revision 1.5  2004/08/25 20:04:40  ravindra
// Added the missing divider code and grip for ToolBar Control.
//
// Revision 1.4  2004/08/22 00:03:20  ravindra
// Fixed toolbar control signatures.
//
// Revision 1.3  2004/08/21 01:52:08  ravindra
// Improvments in mouse event handling in the ToolBar control.
//
// Revision 1.2  2004/08/17 02:00:54  ravindra
// Added attributes.
//
// Revision 1.1  2004/08/15 23:13:15  ravindra
// First Implementation of ToolBar control.
//
//

// NOT COMPLETE

using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Imaging;

namespace System.Windows.Forms
{
	[DefaultProperty ("Text")]
	[Designer ("System.Windows.Forms.Design.ToolBarButtonDesigner, " + Consts.AssemblySystem_Design, typeof (IDesigner))]
	[DesignTimeVisible (false)]
	[ToolboxItem (false)]
	public class ToolBarButton : Component
	{
		#region instance variable
		//private ContextMenu menu; //NotImplemented
		private bool enabled = true;
		private int image_index = -1;
		private ToolBar parent;
		private bool partial_push = false;
		private bool pushed = false;
		private ToolBarButtonStyle style = ToolBarButtonStyle.PushButton;
		private object tag;
		private string text = "";
		private string tooltip = "";
		private bool visible = true;
		private Point location = new Point (ThemeEngine.Current.ToolBarGripWidth,
						    ThemeEngine.Current.ToolBarGripWidth);
		internal bool dd_pressed = false; // to check for a mouse down on dropdown rect
		internal bool hilight = false;    // to hilight buttons in flat style
		internal bool inside = false;     // to handle the mouse move event with mouse pressed
		internal bool wrapper = false;    // to mark a wrapping button
		internal bool pressed = false;    // this is to check for mouse down on a button
		#endregion

		#region constructors
		public ToolBarButton () { }

		public ToolBarButton (string text)
		{
			this.text = text;
		}
		#endregion

		#region internal properties
		internal bool Hilight {
			get { return hilight; }
			set {
				if (! pushed)
					hilight = value;
				else
					hilight = false;	
			}
		}

		internal Point Location {
			get { return location; }
			set { location = value; }
		}

		internal bool Pressed {
			get {
				if (pressed && inside)
					return true;
				else
					return false;
			}
			set { pressed = value; }
		}

		internal bool Wrapper {
			get { return wrapper; }
			set { wrapper = value; }
		}
		#endregion internal properties

		#region properties
		/*
		[DefaultValue (null)]
		[TypeConverter (typeof (ReferenceConverter))]
		public Menu DropDownMenu {
			get { return menu; }

			set {
				if (value is ContextMenu)
					menu = value;
				else
					throw new ArgumentException ("DropDownMenu must be of type ContextMenu.");
			}
		}
		*/

		[DefaultValue (true)]
		[Localizable (true)]
		public bool Enabled {
			get { return enabled; }
			set {
				if (value == enabled)
					return;

				enabled = value;
				if (parent != null)
					parent.Redraw (false);
			}
		}

		[DefaultValue (-1)]
		[Editor ("System.Windows.Forms.Design.ImageIndexEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		[Localizable (true)]
		[TypeConverter (typeof (ImageIndexConverter))]
		public int ImageIndex {
			get { return image_index; }
			set {
				if (value < -1)
					throw new ArgumentException ("ImageIndex value must be above or equal to -1.");

				if (value == image_index)
					return;

				image_index = value;
				if (parent != null)
					parent.Redraw (true);
			}
		}

		[Browsable (false)]
		public ToolBar Parent {
			get { return parent; }
		}

		[DefaultValue (false)]
		public bool PartialPush {
			get { return partial_push; }
			set {
				if (value == partial_push)
					return;

				partial_push = value;
				if (parent != null)
					parent.Redraw (false);
			}
		}

		[DefaultValue (false)]
		public bool Pushed {
			get { return pushed; }
			set {
				if (value == pushed)
					return;

				pushed = value;
				if (pushed)
					hilight = false;
				if (parent != null)
					parent.Redraw (false);
			}
		}

		public Rectangle Rectangle {
			get {
				if (parent == null)
					return Rectangle.Empty;
				else if (visible && parent.Visible)
					return parent.GetChildBounds (this);
				else
					return Rectangle.Empty;
			}
		}

		[DefaultValue (ToolBarButtonStyle.PushButton)]
		[RefreshProperties (RefreshProperties.Repaint)]
		public ToolBarButtonStyle Style {
			get { return style; }
			set {
				if (value == style)
					return;

				style = value;
				if (parent != null)
					parent.Redraw (true);
			}
		}

		[Bindable (true)]
		[DefaultValue (null)]
		[Localizable (false)]
		[TypeConverter (typeof (StringConverter))]
		public object Tag {
			get { return tag; }
			set { tag = value; }
		}

		[DefaultValue (null)]
		[Localizable (true)]
		public string Text {
			get { return text; }
			set {
				if (value == text)
					return;

				text = value;
				if (parent != null)
					parent.Redraw (true);
			}
		}

		[DefaultValue (null)]
		[Localizable (true)]
		public string ToolTipText {
			get { return tooltip; }
			set { tooltip = value; }
		}

		[DefaultValue (true)]
		[Localizable (true)]
		public bool Visible {
			get { return visible; }
			set {
				if (value == visible)
					return;

				visible = value;
				if (parent != null)
					parent.Redraw (true);
			}
		}
		#endregion

		#region internal methods
		internal void SetParent (ToolBar parent)
		{
			this.parent = parent;
		}

		internal void Dump ()
		{
			Console.WriteLine ("TBButton: style: " + this.Style);
			Console.WriteLine ("TBButton: wrapper: " + this.Wrapper);
			Console.WriteLine ("TBButton: hilight: " + this.Hilight);
			Console.WriteLine ("TBButton: loc: " + this.Location);
			Console.WriteLine ("TBButton: rect: " + this.Rectangle);
			Console.WriteLine ("TBButton: txt: " + this.Text);
			Console.WriteLine ("TBButton: visible " + this.Visible);
			Console.WriteLine ("TBButton: enabled: " + this.Enabled);
			Console.WriteLine ("TBButton: image index: " + this.ImageIndex);
			Console.WriteLine ("TBButton: pushed: " + this.Pushed);
			Console.WriteLine ("TBButton: partial push: " + this.PartialPush);
		}
		#endregion

		#region methods
		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
		}

		public override string ToString ()
		{
			return string.Format ("ToolBarButton: {0}, Style: {1}", text, style);
		}
		#endregion
	}
}
