/**
 * Namespace: System.Web.UI.WebControls
 * Class:     RepeatInfo
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
	public sealed class RepeatInfo
	{
		private bool            outerTableImp;
		private int             repeatColumns;
		private RepeatDirection repeatDirection;
		private RepeatLayout    repeatLayout;

		public RepeatInfo()
		{
			outerTableImp   = false;
			repeatColumns   = 0;
			repeatDirection = RepeatDirection.Vertical;
			repeatLayout    = RepeatLayout.Table;
		}

		public bool OuterTableImplied
		{
			get
			{
				return outerTableImp;
			}
			set
			{
				outerTableImp = value;
			}
		}

		public int RepeatColumns
		{
			get
			{
				return repeatColumns;
			}
			set
			{
				repeatColumns = value;
			}
		}

		public RepeatDirection RepeatDirection
		{
			get
			{
				return repeatDirection;
			}
			set
			{
				if(!Enum.IsDefined(typeof(RepeatDirection), value))
					throw new ArgumentException();
				repeatDirection = value;
			}
		}

		public RepeatLayout RepeatLayout
		{
			get
			{
				return repeatLayout;
			}
			set
			{
				if(!Enum.IsDefined(typeof(RepeatLayout), value))
					throw new ArgumentException();
				repeatLayout = value;
			}
		}

		public void RenderRepeater(HtmlTextWriter writer, IRepeatInfoUser user, Style controlStyle, WebControl baseControl)
		{
			if(RepeatDirection == RepeatDirection.Vertical)
			{
				DoVerticalRendering(writer, user, controlStyle, baseControl);
			} else
			{
				DoHorizontalRendering(writer, user, controlStyle, baseControl);
			}
		}

		private void DoVerticalRendering(HtmlTextWriter writer, IRepeatInfoUser user, Style controlStyle, WebControl baseControl)
		{
			int total = user.RepeatedItemCount;
			int colsCount;
			int rowsCount;
			if(repeatColumns == 0 || repeatColumns==1)
			{
				colsCount = 1;
				rowsCount = total;
			} else
			{
				colsCount = repeatColumns;
				rowsCount = (total + repeatColumns - 1) / repeatColumns;
				if(rowsCount ==0 && total != 0)
				{
					rowsCount = 1;
					colsCount = total;
				}
			}
			WebControl ctrl = null;
			bool isTable = false;
			bool hasSeps = user.HasSeparators;
			if(!outerTableImp)
			{
				if(RepeatLayout == RepeatLayout.Table)
				{
					ctrl = new Table();
					isTable = true;
				} else
				{
					ctrl = new WebControl(HtmlTextWriterTag.Span);
				}
			}

			if(ctrl != null)
			{
				ctrl.ID = baseControl.ClientID;
				ctrl.CopyBaseAttributes(baseControl);
				ctrl.ApplyStyle(controlStyle);
				ctrl.RenderBeginTag(writer);
			}

			Style itemStyle;
			int colSpan = 0;
			if(user.HasHeader)
			{
				if(isTable)
				{
					writer.RenderBeginTag(HtmlTextWriterTag.Tr);
					if(colsCount != 1)
					{
						colSpan = colsCount;
						if(hasSeps)
							colSpan += colsCount;
						writer.AddAttribute(HtmlTextWriterAttribute.Colspan, colSpan.ToString(NumberFormatInfo.InvariantInfo));
					}
					itemStyle = user.GetItemStyle(ListItemType.Header, -1);
					if(itemStyle != null)
					{
						itemStyle.AddAttributesToRender(writer);
					}
					writer.RenderBeginTag(HtmlTextWriterTag.Td);
				}
				user.RenderItem(ListItemType.Header, -1, this, writer);
				if(isTable)
				{
					writer.RenderEndTag();
					writer.RenderEndTag();
				} else
				{
					if(!outerTableImp)
					{
						writer.WriteFullBeginTag("br");
					}
				}
			}

			int rowIndex = 0;
			int colIndex = 0;
			int index = 0;
			int diff = colsCount - (rowsCount*colsCount - total);
			
			while(rowIndex < rowsCount)
			{
				if(isTable)
					writer.RenderBeginTag(HtmlTextWriterTag.Tr);
				colIndex = 0;
				while(colIndex < colsCount)
				{
					if (rowIndex == rowsCount-1 && colIndex >= diff)
						break;
					
					if (colIndex < diff)
						index = rowIndex + colIndex * rowsCount;
					else
						index = rowIndex + colIndex * (rowsCount-1) + diff;

					if(index < total)
					{
						if(isTable)
						{
							itemStyle = user.GetItemStyle(ListItemType.Item, index);
							if(itemStyle != null)
							{
								itemStyle.AddAttributesToRender(writer);
							}
							writer.RenderBeginTag(HtmlTextWriterTag.Td);
						}
						user.RenderItem(ListItemType.Item, index, this, writer);
						if(isTable)
							writer.RenderEndTag();
						if(hasSeps && index != (total - 1))
						{
							if(colsCount == 1)
							{
								writer.RenderEndTag();
								writer.RenderBeginTag(HtmlTextWriterTag.Tr);
							} else
							{
								writer.WriteFullBeginTag("br");
							}
							if(isTable)
							{
								itemStyle = user.GetItemStyle(ListItemType.Separator, index);
								if(itemStyle != null)
									itemStyle.AddAttributesToRender(writer);
								writer.RenderBeginTag(HtmlTextWriterTag.Td);
							}
							user.RenderItem(ListItemType.Separator, index, this, writer);
							if(isTable)
								writer.RenderEndTag();
						}
					}
					colIndex++;
				}
				if(isTable)
					writer.RenderEndTag();
				else
					if((rowIndex != (rowsCount - 1) || user.HasFooter) && !outerTableImp)
						writer.WriteFullBeginTag("br");
				rowIndex++;
			}
			if(user.HasFooter)
			{
				if(isTable)
				{
					writer.RenderBeginTag(HtmlTextWriterTag.Tr);
					if(colsCount != 1)
					{
						writer.AddAttribute(HtmlTextWriterAttribute.Colspan, colSpan.ToString(NumberFormatInfo.InvariantInfo));
					}
					itemStyle = user.GetItemStyle(ListItemType.Footer, -1);
					if(itemStyle != null)
					{
						itemStyle.AddAttributesToRender(writer);
					}
					writer.RenderBeginTag(HtmlTextWriterTag.Td);
				}
				user.RenderItem(ListItemType.Footer, -1, this, writer);
				if(isTable)
				{
					writer.RenderEndTag();
					writer.RenderEndTag();
				}
			}
			if(ctrl != null)
			{
				ctrl.RenderEndTag(writer);
			}
		}

		private void DoHorizontalRendering (HtmlTextWriter writer,
						    IRepeatInfoUser user,
						    Style controlStyle,
						    WebControl baseControl)
		{
			/* Based on DoVerticalRendering */
			int total = user.RepeatedItemCount;
			int colsCount = 0;
			int rowsCount = 0;
			WebControl ctrl = null;
			bool isTable = true;
			bool hasSeps = user.HasSeparators;
			if (!outerTableImp){
				isTable = (RepeatLayout == RepeatLayout.Table);
				ctrl = (isTable) ? new Table () : new WebControl (HtmlTextWriterTag.Span);
				ctrl.ID = baseControl.ClientID;
				ctrl.CopyBaseAttributes (baseControl);
				ctrl.ApplyStyle (controlStyle);
				ctrl.RenderBeginTag (writer);
			}

			Style itemStyle;
			int colSpan = 0;
			if (user.HasHeader){
				if (isTable){
					writer.RenderBeginTag (HtmlTextWriterTag.Tr);
					if (colsCount != 1){
						colSpan = colsCount;
						if (hasSeps)
							colSpan += colsCount;
						writer.AddAttribute (HtmlTextWriterAttribute.Colspan,
						     colSpan.ToString (NumberFormatInfo.InvariantInfo));
					}
					itemStyle = user.GetItemStyle (ListItemType.Header, -1);
					if (itemStyle != null)
						itemStyle.AddAttributesToRender (writer);
					writer.RenderBeginTag (HtmlTextWriterTag.Td);
				}

				user.RenderItem (ListItemType.Header, -1, this, writer);
				
				if (isTable){
					writer.RenderEndTag();
					writer.RenderEndTag();
				} else if (repeatColumns < user.RepeatedItemCount)
						writer.WriteFullBeginTag ("br");
			}

			for (int index = 0; index < total; index++){
				if (isTable && index == 0)
					writer.RenderBeginTag (HtmlTextWriterTag.Tr);

				if (isTable){
					itemStyle = user.GetItemStyle (ListItemType.Item, index);
					if (itemStyle != null)
						itemStyle.AddAttributesToRender(writer);
					writer.RenderBeginTag(HtmlTextWriterTag.Td);
				}

				user.RenderItem(ListItemType.Item, index, this, writer);
				if (isTable)
					writer.RenderEndTag ();

				if (hasSeps && index != (total - 1)){
					if (isTable){
						itemStyle = user.GetItemStyle (ListItemType.Separator, index);
						if (itemStyle != null)
							itemStyle.AddAttributesToRender (writer);
						writer.RenderBeginTag (HtmlTextWriterTag.Td);
					}
					user.RenderItem (ListItemType.Separator, index, this, writer);
					if (isTable)
						writer.RenderEndTag ();
				}

				colsCount++;
				if (colsCount == repeatColumns) {
					if (isTable) {
						writer.RenderEndTag ();
						writer.RenderBeginTag (HtmlTextWriterTag.Tr);
					}
					else if (rowsCount < total)
						writer.WriteFullBeginTag ("br");
					colsCount = 0;
				}

				if (index == (total - 1)) {
					if (isTable)
						writer.RenderEndTag ();
					else if (rowsCount < total)
						writer.WriteFullBeginTag ("br");
				}
			}

			if (user.HasFooter){
				if (isTable){
					writer.RenderBeginTag (HtmlTextWriterTag.Tr);
					if (colsCount != 1)
						writer.AddAttribute (HtmlTextWriterAttribute.Colspan,
							colSpan.ToString(NumberFormatInfo.InvariantInfo));

					itemStyle = user.GetItemStyle (ListItemType.Footer, -1);
					if(itemStyle != null)
						itemStyle.AddAttributesToRender (writer);
					writer.RenderBeginTag (HtmlTextWriterTag.Td);
				}
				user.RenderItem (ListItemType.Footer, -1, this, writer);
				if (isTable){
					writer.RenderEndTag ();
					writer.RenderEndTag ();
				}
			}

			if (ctrl != null)
				ctrl.RenderEndTag(writer);
		}
	}
}
