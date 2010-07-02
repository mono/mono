//
// System.Web.UI.WebControls.TableCell.cs
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Globalization;
using System.Security.Permissions;
using System.Text;
using System.Web.Util;

namespace System.Web.UI.WebControls {

	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[ControlBuilder (typeof (TableCellControlBuilder))]
	[DefaultProperty ("Text")]
	[ParseChildren (false)]
	[ToolboxItem ("")]
#if NET_2_0
	[Bindable (false)]
	[Designer ("System.Web.UI.Design.WebControls.PreviewControlDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
#else
	[PersistChildren (true)]
#endif
	public class TableCell : WebControl {

		public TableCell ()
			: base (HtmlTextWriterTag.Td)
		{
			AutoID = false;
		}

		// FIXME: is there a clean way to change the tag's name without using a ctor ?
		// if not then this truly limits the usefulness of inheritance
		internal TableCell (HtmlTextWriterTag tag)
			: base (tag)
		{
			AutoID = false;
		}


#if NET_2_0
		[DefaultValue (null)]
		[TypeConverter (typeof (StringArrayConverter))]
		public virtual string[] AssociatedHeaderCellID {
			get {
				object o = ViewState ["AssociatedHeaderCellID"];
				return (o == null) ? new string[0] : (string[]) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("AssociatedHeaderCellID");
				else
					ViewState ["AssociatedHeaderCellID"] = value;
			}
		}
#endif

#if ONLY_1_1
		[Bindable (true)]
#endif
		[DefaultValue (0)]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public virtual int ColumnSpan {
			get {
				object o = ViewState ["ColumnSpan"];
				return (o == null) ? 0 : (int) o;
			}
			set {
				// LAMESPEC: undocumented (but like Table.CellPadding)
				if (value < 0)
					throw new ArgumentOutOfRangeException ("< 0");
				ViewState ["ColumnSpan"] = value;
			}
		}

#if ONLY_1_1
		[Bindable (true)]
#endif
		[DefaultValue (HorizontalAlign.NotSet)]
		[WebSysDescription ("")]
		[WebCategory ("Layout")]
		public virtual HorizontalAlign HorizontalAlign {
			get {
				if (!ControlStyleCreated)
					return HorizontalAlign.NotSet; // default value
				return TableItemStyle.HorizontalAlign;
			}
			set { TableItemStyle.HorizontalAlign = value; }
		}

#if ONLY_1_1
		[Bindable (true)]
#endif
		[DefaultValue (0)]
		[WebSysDescription ("")]
		[WebCategory ("Layout")]
		public virtual int RowSpan {
			get {
				object o = ViewState ["RowSpan"];
				return (o == null) ? 0 : (int) o;
			}
			set {
				// LAMESPEC: undocumented (but like Table.CellPadding)
				if (value < 0)
					throw new ArgumentOutOfRangeException ("< 0");
				ViewState ["RowSpan"] = value;
			}
		}

#if NET_2_0
		[Localizable (true)]
		[PersistenceMode (PersistenceMode.InnerDefaultProperty)]
#else
		[Bindable (true)]
#endif
		[DefaultValue ("")]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public virtual string Text {
			get {
				object o = ViewState ["Text"];
				return (o == null) ? String.Empty : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("Text");
				else {
					ViewState ["Text"] = value;
					if (HasControls ())
						Controls.Clear ();
				}
			}
		}

#if ONLY_1_1
		[Bindable (true)]
#endif
		[DefaultValue (VerticalAlign.NotSet)]
		[WebSysDescription ("")]
		[WebCategory ("Layout")]
		public virtual VerticalAlign VerticalAlign {
			get {
				if (!ControlStyleCreated)
					return VerticalAlign.NotSet; // default value
				return TableItemStyle.VerticalAlign;
			}
			set { TableItemStyle.VerticalAlign = value; }
		}

#if ONLY_1_1
		[Bindable (true)]
#endif
		[DefaultValue (true)]
		[WebSysDescription ("")]
		[WebCategory ("Layout")]
		public virtual bool Wrap {
			get {
				if (!ControlStyleCreated)
					return true; // default value
				return TableItemStyle.Wrap;
			}
			set { TableItemStyle.Wrap = value; }
		}
#if NET_4_0
		public override bool SupportsDisabledAttribute {
			get { return RenderingCompatibilityLessThan40; }
		}
#endif
		TableItemStyle TableItemStyle {
			get { return (ControlStyle as TableItemStyle); }
		}

		protected override void AddAttributesToRender (HtmlTextWriter writer)
		{
			base.AddAttributesToRender (writer);
			if (writer == null)
				return;

			int i = ColumnSpan;
			if (i > 0)
				writer.AddAttribute (HtmlTextWriterAttribute.Colspan, i.ToString (Helpers.InvariantCulture), false);

			i = RowSpan;
			if (i > 0)
				writer.AddAttribute (HtmlTextWriterAttribute.Rowspan, i.ToString (Helpers.InvariantCulture), false);
#if NET_2_0
			string[] ahci = AssociatedHeaderCellID;
			if (ahci.Length > 1) {
				StringBuilder sb = new StringBuilder ();
				for (i = 0; i < ahci.Length - 1; i++) {
					sb.Append (ahci [i]);
					sb.Append (",");
				}
				sb.Append (ahci.Length - 1);
				writer.AddAttribute (HtmlTextWriterAttribute.Headers, sb.ToString ());
			} else if (ahci.Length == 1) {
				// most common case (without a StringBuilder)
				writer.AddAttribute (HtmlTextWriterAttribute.Headers, ahci [0]);
			}
#endif
		}

		protected override void AddParsedSubObject (object obj)
		{
			if (HasControls ()) {
				base.AddParsedSubObject (obj);
				return;
			}
			
			LiteralControl lc = (obj as LiteralControl);
			if (lc == null) {
				string s = Text;
				if (s.Length > 0) {
					Controls.Add (new LiteralControl (s));
					// remove from viewstate
					Text = null;
				}
				base.AddParsedSubObject(obj);
			} else {
				// this will clear any existing controls
				Text = lc.Text;
			}
		}

		protected override Style CreateControlStyle ()
		{
			return new TableItemStyle (ViewState);
		}

#if NET_2_0
		protected internal
#else		
		protected
#endif		
		override void RenderContents (HtmlTextWriter writer)
		{
			if (HasControls () || HasRenderMethodDelegate ())
				base.RenderContents (writer);
			else
				writer.Write (Text);
		}
	}
}

