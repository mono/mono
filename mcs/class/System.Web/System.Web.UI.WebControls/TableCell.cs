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

using System;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.ComponentModel;

namespace System.Web.UI.WebControls
{
#if NET_2_0
	[Bindable (false)]
#else
	[PersistChildren(true)]
#endif
	[DefaultProperty("Text")]
	[ToolboxItem(false)]
	[ControlBuilder(typeof(TableCellControlBuilder))]
	[ParseChildren(false)]
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

#if NET_2_0
		public TableCell (string text) : this ()
		{
			Text = text;
		}
#endif

#if !NET_2_0
		[Bindable (true)]
#endif
		[DefaultValue (0), WebCategory ("Appearance")]
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

#if !NET_2_0
		[Bindable (true)]
#endif
		[DefaultValue (0), WebCategory ("Layout")]
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

#if NET_2_0
	    [LocalizableAttribute (true)]
    	[PersistenceModeAttribute (PersistenceMode.EncodedInnerDefaultProperty)]
#else
		[Bindable (true)]
#endif
		[DefaultValue (""), WebCategory ("Appearance")]
		[WebSysDescription ("The text that is shown in this cell.")]
		public virtual string Text
		{
			get {
				object o = ViewState ["Text"];
				return (o == null) ? String.Empty : (string) o;
			}

			set {
				if (HasControls ())
					Controls.Clear ();
				ViewState ["Text"] = value;
			}
		}

#if !NET_2_0
		[Bindable (true)]
#endif
		[DefaultValue (typeof (HorizontalAlign), "NotSet"), WebCategory ("Layout")]
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

#if !NET_2_0
		[Bindable (true)]
#endif
		[DefaultValue (typeof (VerticalAlign), "NotSet"), WebCategory ("Layout")]
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

#if !NET_2_0
		[Bindable (true)]
#endif
		[DefaultValue (true), WebCategory ("Layout")]
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

