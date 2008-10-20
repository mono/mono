//
// System.Web.UI.HtmlControls.HtmlTableRow.cs
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
using System.Security.Permissions;

namespace System.Web.UI.HtmlControls {

	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[ParseChildren (true, "Cells")]	
	public class HtmlTableRow : HtmlContainerControl {

		HtmlTableCellCollection _cells;


		public HtmlTableRow ()
			: base ("tr")
		{
		}


		[DefaultValue ("")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebSysDescription("")]
		[WebCategory("Layout")]
		public string Align {
			get {
				string s = Attributes ["align"];
				return (s == null) ? String.Empty : s;
			}
			set {
				if (value == null)
					Attributes.Remove ("align");
				else
					Attributes ["align"] = value;
			}
		}

		[DefaultValue ("")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebSysDescription("")]
		[WebCategory("Appearance")]
		public string BgColor {
			get {
				string s = Attributes ["bgcolor"];
				return (s == null) ? String.Empty : s;
			}
			set {
				if (value == null)
					Attributes.Remove ("bgcolor");
				else
					Attributes ["bgcolor"] = value;
			}
		}

		[DefaultValue ("")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebSysDescription("")]
		[WebCategory("Appearance")]
		public string BorderColor {
			get {
				string s = Attributes ["bordercolor"];
				return (s == null) ? String.Empty : s;
			}
			set {
				if (value == null)
					Attributes.Remove ("bordercolor");
				else
					Attributes ["bordercolor"] = value;
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public virtual HtmlTableCellCollection Cells {
			get {
				if (_cells == null)
					_cells = new HtmlTableCellCollection (this);
				return _cells;
			}
		}

		[DefaultValue ("")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebSysDescription("")]
		[WebCategory("Layout")]
		public string Height {
			get {
				string s = Attributes ["height"];
				return (s == null) ? String.Empty : s;
			}
			set {
				if (value == null)
					Attributes.Remove ("height");
				else
					Attributes ["height"] = value;
			}
		}

		public override string InnerHtml {
			get { throw new NotSupportedException (); }
			set { throw new NotSupportedException (); }
		}

		public override string InnerText {
			get { throw new NotSupportedException (); }
			set { throw new NotSupportedException (); }
		}

		[DefaultValue ("")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebSysDescription("")]
		[WebCategory("Layout")]
		public string VAlign {
			get {
				string s = Attributes ["valign"];
				return (s == null) ? String.Empty : s;
			}
			set {
				if (value == null)
					Attributes.Remove ("valign");
				else
					Attributes ["valign"] = value;
			}
		}

		int Count {
			get { return (_cells == null) ? 0 : _cells.Count; }
		}


		protected override ControlCollection CreateControlCollection ()
		{
			return new HtmlTableCellControlCollection (this);
		}

#if NET_2_0
		protected internal
#else		
		protected
#endif		
		override void RenderChildren (HtmlTextWriter writer)
		{
			if (HasControls ()) {
				writer.Indent++;
				base.RenderChildren (writer);
				writer.Indent--;
				writer.WriteLine ();
			}
		}

		protected override void RenderEndTag (HtmlTextWriter writer)
		{
			if (Count == 0)
				writer.WriteLine ();
			writer.WriteEndTag (TagName);
			if (writer.Indent == 0)
				writer.WriteLine ();
		}


		protected class HtmlTableCellControlCollection : ControlCollection {

			internal HtmlTableCellControlCollection (HtmlTableRow owner)
				: base (owner)
			{
			}

			public override void Add (Control child)
			{
				if (child == null)
					throw new NullReferenceException ("null");
				if (!(child is HtmlTableCell))
					throw new ArgumentException ("child", Locale.GetText ("Must be an HtmlTableCell instance."));

				base.Add (child);
			}

			public override void AddAt (int index, Control child)
			{
				if (child == null)
					throw new NullReferenceException ("null");
				if (!(child is HtmlTableCell))
					throw new ArgumentException ("child", Locale.GetText ("Must be an HtmlTableCell instance."));

				base.AddAt (index, child);
			}
		}
	}
}
