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

#if !NET_2_0
		[Bindable (true)]
#else
		[UrlProperty]
#endif
		[DefaultValue (""), WebCategory ("Appearance")]
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

#if NET_2_0
		[DefaultValue (""), WebCategory ("Accessibility"), Localizable (true)]
		public virtual string Caption
		{
			get {
				object o = ViewState ["Caption"];
				if(o != null) return (string)o;
				return "";
			}
			set {
				ViewState ["Caption"] = value;
			}
		}

		[DefaultValue (TableCaptionAlign.NotSet), WebCategory ("Accessibility")]
		public virtual TableCaptionAlign CaptionAlign
		{
			get {
				object o = ViewState ["CaptionAlign"];
				if(o != null) return (TableCaptionAlign) o;
				return TableCaptionAlign.NotSet;
			}
			set {
				ViewState ["CaptionAlign"] = value;
			}
		}
#endif

#if !NET_2_0
		[Bindable (true)]
#endif
		[DefaultValue (-1), WebCategory ("Appearance")]
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

#if !NET_2_0
		[Bindable (true)]
#endif
		[DefaultValue (-1), WebCategory ("Appearance")]
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

#if !NET_2_0
		[Bindable (true)]
#endif
		[DefaultValue (typeof (GridLines), "None"), WebCategory ("Appearance")]
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

#if !NET_2_0
		[Bindable (true)]
#endif
		[DefaultValue (typeof (HorizontalAlign), "NotSet"), WebCategory ("Layout")]
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
			else if (bw.IsEmpty || bw.Type != UnitType.Pixel)
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
		
#if NET_2_0
    	public override void RenderBeginTag (HtmlTextWriter writer)
		{
			base.RenderBeginTag (writer);
			if (Caption != "") {
				writer.AddAttribute ("align", CaptionAlign.ToString());
				writer.RenderBeginTag (HtmlTextWriterTag.Caption);
				writer.Write (Caption);
				writer.RenderEndTag ();
			}
		}
#endif

		protected override void RenderContents (HtmlTextWriter writer)
		{
			foreach (TableRow current in Rows)
				 current.RenderControl (writer);
		}
	}
}

