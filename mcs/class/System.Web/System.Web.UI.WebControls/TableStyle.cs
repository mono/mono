//
// System.Web.UI.WebControls.TableStyle.cs
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
using System.Globalization;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public class TableStyle : Style
	{
		private static int IMAGE_URL = (0x01 << 16);
		private static int CELL_PADD = (0x01 << 17);
		private static int CELL_SPAC = (0x01 << 18);
		private static int GRID_LINE = (0x01 << 19);
		private static int HOR_ALIGN = (0x01 << 20);

		public TableStyle(): base()
		{
		}

		public TableStyle(StateBag bag): base(bag)
		{
		}

		[DefaultValue (""), Bindable (true), WebCategory ("Appearance")]
		[WebSysDescription ("An Url specifying the background image for the table.")]
		public virtual string BackImageUrl
		{
			get
			{
				if(IsSet(IMAGE_URL))
					return (string)(ViewState["BackImageUrl"]);
				return String.Empty;
			}
			set
			{
				if(value == null)
					throw new ArgumentNullException("value");
				ViewState["BackImageUrl"] = value;
				Set(IMAGE_URL);
			}
		}

		[DefaultValue (-1), Bindable (true), WebCategory ("Appearance")]
		[WebSysDescription ("The space left around the borders within a cell.")]
		public virtual int CellPadding
		{
			get
			{
				if(IsSet(CELL_PADD))
					return (int)(ViewState["CellPadding"]);
				return -1;
			}
			set
			{
				if(value < -1)
					throw new ArgumentOutOfRangeException("value", "CellPadding value has to be -1 for 'not set' or a value >= 0");
				ViewState["CellPadding"] = value;
				Set(CELL_PADD);
			}
		}

		[DefaultValue (-1), Bindable (true), WebCategory ("Appearance")]
		[WebSysDescription ("The space left between cells.")]
		public virtual int CellSpacing
		{
			get
			{
				if(IsSet(CELL_SPAC))
					return (int)(ViewState["CellSpacing"]);
				return -1;
			}
			set
			{
				if(value < -1)
					throw new ArgumentOutOfRangeException("value"," CellSpacing value has to be -1 for 'not set' or a value >= 0");
				ViewState["CellSpacing"] = value;
				Set(CELL_SPAC);
			}
		}

		[DefaultValue (typeof (GridLines), "None"), Bindable (true), WebCategory ("Appearance")]
		[WebSysDescription ("The type of grid that a table uses.")]
		public virtual GridLines GridLines
		{
			get
			{
				if(IsSet(GRID_LINE))
					return (GridLines)(ViewState["GridLines"]);
				return GridLines.None;
			}
			set
			{
				if(!Enum.IsDefined(typeof(GridLines), value))
					throw new ArgumentOutOfRangeException("value"," Gridlines value has to be a valid enumeration member");
				ViewState["GridLines"] = value;
				Set(GRID_LINE);
			}
		}

		[DefaultValue (typeof (HorizontalAlign), "NotSet"), Bindable (true), WebCategory ("Layout")]
		[WebSysDescription ("The horizonal alignment of the table.")]
		public virtual HorizontalAlign HorizontalAlign
		{
			get
			{
				if(IsSet(HOR_ALIGN))
					return (HorizontalAlign)(ViewState["HorizontalAlign"]);
				return HorizontalAlign.NotSet;
			}
			set
			{
				if(!Enum.IsDefined(typeof(HorizontalAlign), value))
					throw new ArgumentOutOfRangeException("value"," Gridlines value has to be a valid enumeration member");
				ViewState["HorizontalAlign"] = value;
				Set(HOR_ALIGN);
			}
		}

		public override void AddAttributesToRender(HtmlTextWriter writer, WebControl owner)
		{
			base.AddAttributesToRender(writer, owner);
			if(BackImageUrl.Length > 0)
			{
				writer.AddStyleAttribute(HtmlTextWriterStyle.BackgroundImage, "url(" + owner.ResolveUrl(BackImageUrl) + ")");
			}
			if(CellSpacing >= 0)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, CellSpacing.ToString(NumberFormatInfo.InvariantInfo));
			}
			if(CellPadding >= 0)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, CellPadding.ToString(NumberFormatInfo.InvariantInfo));
			}
			if(HorizontalAlign != HorizontalAlign.NotSet)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Align, Enum.Format(typeof(HorizontalAlign), HorizontalAlign, "G"));
			}
			string gd = "";
			switch(GridLines)
			{
				case GridLines.None:       gd = "";
				                           break;
				case GridLines.Horizontal: gd = "cols";
				                           break;
				case GridLines.Vertical:   gd = "rows";
				                           break;
				case GridLines.Both:       gd = "all";
				                           break;
			}
			writer.AddAttribute(HtmlTextWriterAttribute.Rules, gd);
		}

		public override void CopyFrom(Style s)
		{
			if(s != null && s is TableStyle && !s.IsEmpty)
			{
				base.CopyFrom(s);
				TableStyle from = (TableStyle)s;
				if(from.IsSet(HOR_ALIGN))
				{
					HorizontalAlign = from.HorizontalAlign;
				}
				if(from.IsSet(IMAGE_URL))
				{
					BackImageUrl = from.BackImageUrl;
				}
				if(from.IsSet(CELL_PADD))
				{
					CellPadding = from.CellPadding;
				}
				if(from.IsSet(CELL_SPAC))
				{
					CellSpacing = from.CellSpacing;
				}
				if(from.IsSet(GRID_LINE))
				{
					GridLines = from.GridLines;
				}
			}
		}

		public override void MergeWith(Style s)
		{
			if(s != null && s is TableStyle && !s.IsEmpty)
			{
				base.MergeWith(s);
				TableStyle with = (TableStyle)s;
				if(with.IsSet(HOR_ALIGN) && IsSet(HOR_ALIGN))
				{
					HorizontalAlign = with.HorizontalAlign;
				}
				if(with.IsSet(IMAGE_URL) && IsSet(IMAGE_URL))
				{
					BackImageUrl = with.BackImageUrl;
				}
				if(with.IsSet(CELL_PADD) && IsSet(CELL_PADD))
				{
					CellPadding = with.CellPadding;
				}
				if(with.IsSet(CELL_SPAC) && IsSet(CELL_SPAC))
				{
					CellSpacing = with.CellSpacing;
				}
				if(with.IsSet(GRID_LINE) && IsSet(GRID_LINE))
				{
					GridLines = with.GridLines;
				}
			}
		}

		public override void Reset()
		{
			if(IsSet(IMAGE_URL))
				ViewState.Remove("BackImageUrl");
			if(IsSet(HOR_ALIGN))
				ViewState.Remove("HorizontalAlign");
			if(IsSet(CELL_PADD))
				ViewState.Remove("CellPadding");
			if(IsSet(CELL_SPAC))
				ViewState.Remove("CellSpacing");
			if(IsSet(GRID_LINE))
				ViewState.Remove("GridLines");
			base.Reset();
		}
	}
}
