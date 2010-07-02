//
// System.Web.UI.WebControls.TableRow.cs
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

namespace System.Web.UI.WebControls {

	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[DefaultProperty ("Cells")]
	[ParseChildren (true, "Cells")]
	[ToolboxItem ("")]
#if NET_2_0
	[Bindable (false)]
	[Designer ("System.Web.UI.Design.WebControls.PreviewControlDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
#endif
	public class TableRow : WebControl
	{
		TableCellCollection cells;
#if NET_2_0
		bool tableRowSectionSet;

		internal TableRowCollection Container { get; set; }
#endif
		
		public TableRow ()
			: base (HtmlTextWriterTag.Tr)
		{
			AutoID = false;
		}

#if NET_2_0
		internal bool TableRowSectionSet {
			get { return tableRowSectionSet; }
		}
#endif
		
		[MergableProperty (false)]
		[PersistenceMode (PersistenceMode.InnerDefaultProperty)]
		[WebSysDescription ("")]
		[WebCategory("Layout")]
		public virtual TableCellCollection Cells {
			get {
				if (cells == null)
					cells = new TableCellCollection (this);
				return cells;
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

		TableItemStyle TableItemStyle {
			get { return (ControlStyle as TableItemStyle); }
		}
#if NET_4_0
		public override bool SupportsDisabledAttribute {
			get { return RenderingCompatibilityLessThan40; }
		}
#endif
		protected override ControlCollection CreateControlCollection ()
		{
			return new CellControlCollection (this);
		}

		protected override Style CreateControlStyle ()
		{
			return new TableItemStyle (ViewState);
		}
#if NET_2_0
		[DefaultValue (TableRowSection.TableBody)]
		public virtual TableRowSection TableSection {
			get {
				object o = ViewState ["TableSection"];
				return (o == null) ? TableRowSection.TableBody : (TableRowSection) o;
			}
			set {
				if ((value < TableRowSection.TableHeader) || (value > TableRowSection.TableFooter))
					throw new ArgumentOutOfRangeException ("TableSection");
				ViewState ["TableSection"] = (int) value;
				tableRowSectionSet = true;
				TableRowCollection container = Container;
				if (container != null)
					container.RowTableSectionSet ();
			}
		}
#endif
		// inner class
		protected class CellControlCollection : ControlCollection {

			internal CellControlCollection (TableRow owner)
				: base (owner)
			{
			}


			public override void Add (Control child)
			{
				if (child == null)
					throw new NullReferenceException ("null");
				if (!(child is TableCell))
					throw new ArgumentException ("child", Locale.GetText ("Must be an TableCell instance."));

				base.Add (child);
			}

			public override void AddAt (int index, Control child)
			{
				if (child == null)
					throw new NullReferenceException ("null");
				if (!(child is TableCell))
					throw new ArgumentException ("child", Locale.GetText ("Must be an TableCell instance."));

				base.AddAt (index, child);
			}
		}
	}
}
