/**
 * Namespace: System.Web.UI.WebControls
 * Class:     TableStyle
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
					throw new ArgumentNullException("BackImageUrl");
				ViewState["BackImageUrl"] = value;
				Set(IMAGE_URL);
			}
		}

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
					throw new ArgumentOutOfRangeException("CellPadding");
				ViewState["CellPadding"] = value;
				Set(CELL_PADD);
			}
		}

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
					throw new ArgumentOutOfRangeException("CellSpacing");
				ViewState["CellSpacing"] = value;
				Set(CELL_SPAC);
			}
		}

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
					throw new ArgumentException();
				ViewState["GridLines"] = value;
				Set(GRID_LINE);
			}
		}

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
					throw new ArgumentException();
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
