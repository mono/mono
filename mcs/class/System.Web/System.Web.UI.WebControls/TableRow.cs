//
// System.Web.UI.WebControls.TableRow.cs
//
// Authors:
//   Gaurav Vaish (gvaish@iitk.ac.in)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Gaurav Vaish (2002)
// (C) 2003 Andreas Nahr
//

using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	[DefaultProperty("Cells")]
	[ParseChildren(true, "Cells")]
	[ToolboxItem (false)]
	public class TableRow: WebControl
	{
		private TableCellCollection cells;

		public TableRow() : base (HtmlTextWriterTag.Tr)
		{
			PreventAutoID ();
		}

		[MergableProperty (false), PersistenceMode (PersistenceMode.InnerDefaultProperty)]
		[WebSysDescription ("All cells that exist in a table row.")]
		public virtual TableCellCollection Cells
		{
			get {
				if (cells == null)
					cells = new TableCellCollection (this);
				return cells;
			}
		}

		[DefaultValue (typeof (HorizontalAlign), "NotSet"), Bindable (true), WebCategory ("Layout")]
		[WebSysDescription ("The horizontal alignment for all table cells in that row.")]
		public virtual HorizontalAlign HorizontalAlign
		{
			get {
				object o = ViewState ["HorizontalAlign"];
				return (o == null) ? HorizontalAlign.NotSet : (HorizontalAlign) o;
			}

			set { ViewState ["HorizontalAlign"] = value; }
		}

		[DefaultValue (typeof (VerticalAlign), "NotSet"), Bindable (true), WebCategory ("Layout")]
		[WebSysDescription ("The verical alignment for all table cells in that row.")]
		public virtual VerticalAlign VerticalAlign
		{
			get {
				object o = ViewState ["VerticalAlign"];
				return (o == null) ? VerticalAlign.NotSet : (VerticalAlign) o;
			}

			set { ViewState ["VerticalAlign"] = value; }
		}

		protected override Style CreateControlStyle ()
		{
			return new TableItemStyle (ViewState);
		}

		protected override ControlCollection CreateControlCollection ()
		{
			return new CellControlCollection (this);
		}

		protected class CellControlCollection : ControlCollection
		{
			internal CellControlCollection (Control owner) : base (owner)
			{
			}

			public override void Add (Control child)
			{
				if (!(child is TableCell))
					throw new ArgumentException (HttpRuntime.FormatResourceString (
								     "Cannot_Have_Children_Of_Type",
								     "TableRow",
								     GetType ().Name.ToString ()));
				base.Add (child);
			}

			public override void AddAt(int index, Control child)
			{
				if (!(child is TableCell))
					throw new ArgumentException (HttpRuntime.FormatResourceString (
								     "Cannot_Have_Children_Of_Type",
								     "TableRow",
								     GetType ().Name.ToString ()));
				base.AddAt (index, child);
			}
		}
	}
}

