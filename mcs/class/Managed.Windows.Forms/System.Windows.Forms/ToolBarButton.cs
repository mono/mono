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
// Copyright (C) 2004 Novell, Inc.
//
// Authors:
//	Ravindra (rkumar@novell.com)
//
// TODO:
//	- DropDownMenu
//      - Adding a button to two toolbars
//
// $Revision: 1.2 $
// $Modtime: $
// $Log: ToolBarButton.cs,v $
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
		private int imageIndex = -1;
		private ToolBar parent;
		private bool partialPush = false;
		private bool pushed = false;
		private ToolBarButtonStyle style = ToolBarButtonStyle.PushButton;
		private object tag;
		private string text = "";
		private string toolTip = "";
		private bool visible = true;
		private Point location = new Point (0, 0);
		private bool wrapper = false;
		private bool hilight = false;
		#endregion

		#region constructors
		public ToolBarButton () { }

		public ToolBarButton (string text)
		{
			this.text = text;
		}
		#endregion

		#region internal properties
		internal Point Location {
			get { return location; }
			set { location = value; }
		}

		internal bool Wrapper {
			get { return wrapper; }
			set { wrapper = value; }
		}

		internal bool Hilight {
			get { return hilight; }
			set {
				if (! pushed)
					hilight = value;
				else
					hilight = false;	
			}
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
			}
		}

		[DefaultValue (-1)]
		[Editor ("System.Windows.Forms.Design.ImageIndexEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		[Localizable (true)]
		//[TypeConverter (typeof (ImageIndexConverter)]
		public int ImageIndex {
			get { return imageIndex; }
			set {
				if (value == imageIndex)
					return;

				imageIndex = value;
			}
		}

		[Browsable (false)]
		public ToolBar Parent {
			get { return parent; }
		}

		[DefaultValue (false)]
		public bool PartialPush {
			get { return partialPush; }
			set {
				if (value == partialPush)
					return;

				partialPush = value;
			}
		}

		[DefaultValue (false)]
		public bool Pushed {
			get { return pushed; }
			set {
				if (value == pushed)
					return;

				pushed = value;
				if (pushed) hilight = false;
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
			}
		}

		[DefaultValue (null)]
		[Localizable (true)]
		public string ToolTipText {
			get { return toolTip; }
			set { toolTip = value; }
		}

		[DefaultValue (true)]
		[Localizable (true)]
		public bool Visible {
			get { return visible; }
			set {
				if (value == visible)
					return;

				visible = value;
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
