//
// System.Web.UI.WebControls.Table.cs
//
// Authors:
//   Gaurav Vaish (gvaish@iitk.ac.in)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Gaurav Vaish (2002)
// (C) 2003 Andreas Nahr
//

using System;
using System.Drawing;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.ComponentModel;
using System.ComponentModel.Design;

namespace System.Web.UI.WebControls
{
	[DefaultProperty("Rows")]
	[Designer ("System.Web.UI.Design.WebControls.TableDesigner, " + Consts.AssemblySystem_Design, typeof (IDesigner))]
	[ParseChildren(true, "Rows")]
	public class Table: WebControl
	{
		private TableRowCollection rows;

		private class TableRowControlCollection : ControlCollection
		{
			public TableRowControlCollection (Control owner) : base (owner)
			{
			}

			public override void Add (Control child)
			{
				if (!(child is TableRow))
					throw new ArgumentException (HttpRuntime.FormatResourceString (
									"Cannot_Have_Children_Of_Type",
									"Table",
									child.GetType ().Name.ToString ()));
				base.Add (child);
			}

			public override void AddAt(int index, Control child)
			{
				if (!(child is TableRow))
					throw new ArgumentException (HttpRuntime.FormatResourceString (
									"Cannot_Have_Children_Of_Type",
									"Table",
									child.GetType ().Name.ToString ()));
				base.AddAt (index, child);
			}
		}

		public Table () : base (HtmlTextWriterTag.Table)
		{
		}

		[DefaultValue (""), Bindable (true), WebCategory ("Appearance")]
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		[WebSysDescription ("An Url specifying the background image for the table.")]
		public virtual string BackImageUrl
		{
			get {
				if (ControlStyleCreated)
					return ((TableStyle) ControlStyle).BackImageUrl;
				return String.Empty;
			}

			set { ((TableStyle) ControlStyle).BackImageUrl = value; }
		}

		[DefaultValue (-1), Bindable (true), WebCategory ("Appearance")]
		[WebSysDescription ("The space left around the borders within a cell.")]
		public virtual int CellPadding
		{
			get {
				if (ControlStyleCreated)
					return ((TableStyle) ControlStyle).CellPadding;
				return -1;
			}

			set { ((TableStyle) ControlStyle).CellPadding = value; }
		}

		[DefaultValue (-1), Bindable (true), WebCategory ("Appearance")]
		[WebSysDescription ("The space left between cells.")]
		public virtual int CellSpacing
		{
			get {
				if (ControlStyleCreated)
					return ((TableStyle) ControlStyle).CellSpacing;
				return -1;
			}

			set { ((TableStyle) ControlStyle).CellSpacing = value; }
		}

		[DefaultValue (typeof (GridLines), "None"), Bindable (true), WebCategory ("Appearance")]
		[WebSysDescription ("The type of grid that a table uses.")]
		public virtual GridLines GridLines
		{
			get {
				if (ControlStyleCreated)
					return ((TableStyle) ControlStyle).GridLines;
				return GridLines.None;
			}

			set { ((TableStyle) ControlStyle).GridLines = value; }
		}

		[DefaultValue (typeof (HorizontalAlign), "NotSet"), Bindable (true), WebCategory ("Layout")]
		[WebSysDescription ("The horizonal alignment of the table.")]
		public virtual HorizontalAlign HorizontalAlign
		{
			get {
				if (ControlStyleCreated)
					return ((TableStyle) ControlStyle).HorizontalAlign;
				return HorizontalAlign.NotSet;
			}

			set { ((TableStyle) ControlStyle).HorizontalAlign = value; }
		}

		[MergableProperty (false), PersistenceMode (PersistenceMode.InnerDefaultProperty)]
		[WebSysDescription ("The rows of this table.")]
		public virtual TableRowCollection Rows
		{
			get {
				if (rows == null)
					rows = new TableRowCollection (this);
				return rows;
			}
		}

		protected override void AddAttributesToRender (HtmlTextWriter writer)
		{
			base.AddAttributesToRender (writer);
			if(!BorderColor.IsEmpty)
				writer.AddAttribute (HtmlTextWriterAttribute.Bordercolor,
						     ColorTranslator.ToHtml (BorderColor));

			Unit bw = BorderWidth;
			if (GridLines == GridLines.None)
				bw = Unit.Pixel (0);
			else if (bw.IsEmpty || bw.Type == UnitType.Pixel)
				bw = Unit.Pixel(1);

			writer.AddAttribute (HtmlTextWriterAttribute.Border,
					     ((int) bw.Value).ToString (NumberFormatInfo.InvariantInfo));
		}

		protected override ControlCollection CreateControlCollection ()
		{
			return new TableRowControlCollection (this);
		}

		protected override Style CreateControlStyle ()
		{
			return new TableStyle (ViewState);
		}

		protected override void RenderContents (HtmlTextWriter writer)
		{
			foreach (TableRow current in Rows)
				 current.RenderControl (writer);
		}
	}
}

