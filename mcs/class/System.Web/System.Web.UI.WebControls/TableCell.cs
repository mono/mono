//
// System.Web.UI.WebControls.TableCell.cs
//
// Authors:
//   Gaurav Vaish (gvaish@iitk.ac.in)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Gaurav Vaish (2002)
// (C) 2003 Andreas Nahr
//

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

		[DefaultValue (0), Bindable (true), WebCategory ("Appearance")]
		[WebSysDescription ("The number of columns spanned by this cell.")]
		public virtual int ColumnSpan
		{
			get {
				object o = ViewState ["ColumnSpan"];
				return (o == null) ? 0 : (int) o;
			}

			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException ("value", "ColumnSpan value has to be >= 0.");
				ViewState ["ColumnSpan"] = value;
			}
		}

		[DefaultValue (0), Bindable (true), WebCategory ("Layout")]
		[WebSysDescription ("The number of rows spanned by this cell.")]
		public virtual int RowSpan
		{
			get {
				object o = ViewState ["RowSpan"];
				return (o == null) ? 0 : (int) o;
			}

			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException ("value", "RowSpan value has to be >= 0.");
				ViewState ["RowSpan"] = value;
			}
		}

		[DefaultValue (""), Bindable (true), WebCategory ("Appearance")]
		[WebSysDescription ("The text that is shown in this cell.")]
		public virtual string Text
		{
			get {
				object o = ViewState ["Text"];
				return (o == null) ? String.Empty : (string) o;
			}

			set { ViewState ["Text"] = value; }
		}

		[DefaultValue (typeof (HorizontalAlign), "NotSet"), Bindable (true), WebCategory ("Layout")]
		[WebSysDescription ("The horizontal alignment for this cell.")]
		public virtual HorizontalAlign HorizontalAlign
		{
			get {
				if (ControlStyleCreated)
					return ((TableItemStyle) ControlStyle).HorizontalAlign;
				return HorizontalAlign.NotSet;
			}
			set { ((TableItemStyle) ControlStyle).HorizontalAlign = value; }
		}

		[DefaultValue (typeof (VerticalAlign), "NotSet"), Bindable (true), WebCategory ("Layout")]
		[WebSysDescription ("The horizontal alignment for this cell.")]
		public virtual VerticalAlign VerticalAlign
		{
			get {
				if (ControlStyleCreated)
					return ((TableItemStyle) ControlStyle).VerticalAlign;
				return VerticalAlign.NotSet;
			}

			set { ((TableItemStyle) ControlStyle).VerticalAlign = value; }
		}

		[DefaultValue (true), Bindable (true), WebCategory ("Layout")]
		[WebSysDescription ("Determines if the text in the cell should be wraped at line-end.")]
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

