/**
 * Namespace: System.Web.UI.WebControls
 * Class:     Table
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  100%
 *
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.Drawing;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.ComponentModel;

namespace System.Web.UI.WebControls
{
	[DefaultProperty("Rows")]
	//[Designer("??")]
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

		public virtual string BackImageUrl
		{
			get {
				if (ControlStyleCreated)
					return ((TableStyle) ControlStyle).BackImageUrl;
				return String.Empty;
			}

			set { ((TableStyle) ControlStyle).BackImageUrl = value; }
		}

		public virtual int CellPadding
		{
			get {
				if (ControlStyleCreated)
					return ((TableStyle) ControlStyle).CellPadding;
				return -1;
			}

			set { ((TableStyle) ControlStyle).CellPadding = value; }
		}

		public virtual int CellSpacing
		{
			get {
				if (ControlStyleCreated)
					return ((TableStyle) ControlStyle).CellSpacing;
				return -1;
			}

			set { ((TableStyle) ControlStyle).CellSpacing = value; }
		}

		public virtual GridLines GridLines
		{
			get {
				if (ControlStyleCreated)
					return ((TableStyle) ControlStyle).GridLines;
				return GridLines.None;
			}

			set { ((TableStyle) ControlStyle).GridLines = value; }
		}

		public virtual HorizontalAlign HorizontalAlign
		{
			get {
				if (ControlStyleCreated)
					return ((TableStyle) ControlStyle).HorizontalAlign;
				return HorizontalAlign.NotSet;
			}

			set { ((TableStyle) ControlStyle).HorizontalAlign = value; }
		}

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

