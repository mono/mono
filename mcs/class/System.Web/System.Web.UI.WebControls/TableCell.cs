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

namespace System.Web.UI.WebControls
{
	public class TableCell: WebControl
	{
		[MonoTODO]
		public TableCell(): base(HtmlTextWriterTag.Td)
		{
			//TODO: What's the function to prevent Control to give _auto_generated_id
		}

		[MonoTODO]
		internal TableCell(HtmlTextWriterTag tag): base(tag)
		{
			//TODO: What's the function to prevent Control to give _auto_generated_id
		}

		public virtual int ColumnSpan
		{
			get
			{
				object o = ViewState["ColumnSpan"];
				if(o != null)
					return (int)o;
				return 0;
			}
			set
			{
				ViewState["ColumnSpan"] = value;
			}
		}

		public virtual int RowSpan
		{
			get
			{
				object o = ViewState["RowSpan"];
				if(o != null)
					return (int)o;
				return 0;
			}
			set
			{
				ViewState["RowSpan"] = value;
			}
		}

		public virtual string Text
		{
			get
			{
				object o = ViewState["Text"];
				if(o != null)
					return (string)o;
				return String.Empty;
			}
			set
			{
				ViewState["Text"] = value;
			}
		}

		public virtual HorizontalAlign HorizontalAlign
		{
			get
			{
				if(ControlStyleCreated)
					return ((TableItemStyle)ControlStyle).HorizontalAlign;
				return HorizontalAlign.NotSet;
			}
			set
			{
				((TableItemStyle)ControlStyle).HorizontalAlign = value;
			}
		}

		public virtual VerticalAlign VerticalAlign
		{
			get
			{
				if(ControlStyleCreated)
					return ((TableItemStyle)ControlStyle).VerticalAlign;
				return VerticalAlign.NotSet;
			}
			set
			{
				((TableItemStyle)ControlStyle).VerticalAlign = value;
			}
		}

		public virtual bool Wrap
		{
			get
			{
				if(ControlStyleCreated)
					return ((TableItemStyle)ControlStyle).Wrap;
				return true;
			}
			set
			{
				((TableItemStyle)ControlStyle).Wrap = value;
			}
		}

		protected override void AddAttributesToRender(HtmlTextWriter writer)
		{
			AddAttributesToRender(writer);
			if(ColumnSpan > 0)
				writer.AddAttribute(HtmlTextWriterAttribute.Colspan, Colspan.ToString(NumberFormatInfo.InvariantInfo));
			if(RowSpan > 0)
				writer.AddAttribute(HtmlTextWriterAttribute.Rowspan, Rowspan.ToString(NumberFormatInfo.InvariantInfo));
		}

		protected override void AddParsedSubObject(object obj)
		{
			if(HasControls())
			{
				AddParsedSubObject(obj);
				return;
			}
			if(obj is LiteralControl)
			{
				Text = ((LiteralControl)obj).Text;
				return;
			}
			string text = Text;
			if(text.Length > 0)
			{
				Text = String.Empty;
				AddParsedSubObject(new LiteralControl(text));
			}
			AddParsedSubObject(obj);
		}

		protected override Style CreateControlStyle()
		{
			return new TableItemStyle(ViewState);
		}

		protected override void RenderContents(HtmlTextWriter writer)
		{
			if(HasControls())
			{
				RenderContents(writer);
				return;
			}
			writer.Write(Text);
		}
	}
}
