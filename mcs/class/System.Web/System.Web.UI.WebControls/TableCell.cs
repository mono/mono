/**
 * Namespace: System.Web.UI.WebControls
 * Class:     TableCell
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
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.ComponentModel;

namespace System.Web.UI.WebControls
{
	[DefaultProperty("Text")]
	[ToolboxItem(false)]
	[ControlBuilder(typeof(TableCellControlBuilder))]
	[ParseChildren(false)]
	[PersistChildren(true)]
	public class TableCell: WebControl
	{
		public TableCell () : base (HtmlTextWriterTag.Td)
		{
			PreventAutoID ();
		}

		internal TableCell (HtmlTextWriterTag tag) : base (tag)
		{
			PreventAutoID ();
		}

		public virtual int ColumnSpan
		{
			get {
				object o = ViewState ["ColumnSpan"];
				return (o == null) ? 0 : (int) o;
			}

			set { ViewState ["ColumnSpan"] = value; }
		}

		public virtual int RowSpan
		{
			get {
				object o = ViewState ["RowSpan"];
				return (o == null) ? 0 : (int) o;
			}

			set { ViewState ["RowSpan"] = value; }
		}

		public virtual string Text
		{
			get {
				object o = ViewState ["Text"];
				return (o == null) ? String.Empty : (string) o;
			}

			set { ViewState ["Text"] = value; }
		}

		public virtual HorizontalAlign HorizontalAlign
		{
			get {
				if (ControlStyleCreated)
					return ((TableItemStyle) ControlStyle).HorizontalAlign;
				return HorizontalAlign.NotSet;
			}
			set { ((TableItemStyle) ControlStyle).HorizontalAlign = value; }
		}

		public virtual VerticalAlign VerticalAlign
		{
			get {
				if (ControlStyleCreated)
					return ((TableItemStyle) ControlStyle).VerticalAlign;
				return VerticalAlign.NotSet;
			}

			set { ((TableItemStyle) ControlStyle).VerticalAlign = value; }
		}

		public virtual bool Wrap
		{
			get {
				if (ControlStyleCreated)
					return ((TableItemStyle) ControlStyle).Wrap;
				return true;
			}
			set { ((TableItemStyle) ControlStyle).Wrap = value; }
		}

		protected override void AddAttributesToRender (HtmlTextWriter writer)
		{
			base.AddAttributesToRender (writer);
			if (ColumnSpan > 0)
				writer.AddAttribute (HtmlTextWriterAttribute.Colspan,
						     ColumnSpan.ToString (NumberFormatInfo.InvariantInfo));

			if (RowSpan > 0)
				writer.AddAttribute (HtmlTextWriterAttribute.Rowspan,
						     RowSpan.ToString (NumberFormatInfo.InvariantInfo));
		}

		protected override void AddParsedSubObject (object obj)
		{
			if (HasControls ()){
				base.AddParsedSubObject (obj);
				return;
			}

			if (obj is LiteralControl){
				Text = ((LiteralControl) obj).Text;
				return;
			}

			string text = Text;
			if (text.Length > 0){
				Text = String.Empty;
				base.AddParsedSubObject (new LiteralControl (text));
			}

			base.AddParsedSubObject (obj);
		}

		protected override Style CreateControlStyle ()
		{
			return new TableItemStyle (ViewState);
		}

		protected override void RenderContents (HtmlTextWriter writer)
		{
			if (HasControls ())
				base.RenderContents (writer);
			else
				writer.Write (Text);
		}
	}
}

