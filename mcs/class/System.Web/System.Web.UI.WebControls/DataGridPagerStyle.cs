/**
 * Namespace: System.Web.UI.WebControls
 * Class:     DataGridPagerStyle
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
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public sealed class DataGridPagerStyle : TableItemStyle
	{
		DataGrid owner;

		private static int MODE         = (0x01 << 19);
		private static int NEXT_PG_TEXT = (0x01 << 20);
		private static int PG_BTN_COUNT = (0x01 << 21);
		private static int POSITION     = (0x01 << 22);
		private static int VISIBLE      = (0x01 << 23);
		private static int PREV_PG_TEXT = (0x01 << 24);

		internal DataGridPagerStyle(DataGrid owner): base()
		{
			this.owner = owner;
		}

		internal bool IsPagerOnTop
		{
			get {
				PagerPosition p = Position;
				return (p == PagerPosition.Top || p == PagerPosition.TopAndBottom);
			}
		}
		
		internal bool IsPagerOnBottom
		{
			get {
				PagerPosition p = Position;
				return (p == PagerPosition.Bottom || p == PagerPosition.TopAndBottom);
			}
		}
		
		public PagerMode Mode
		{
			get
			{
				if(IsSet(MODE))
				{
					return (PagerMode)ViewState["Mode"];
				}
				return PagerMode.NextPrev;
			}
			set
			{
				if(!Enum.IsDefined(typeof(PagerMode), value))
				{
					throw new ArgumentOutOfRangeException("value");
				}
				ViewState["Mode"] = value;
				Set(MODE);
			}
		}

		public string NextPageText
		{
			get
			{
				if(IsSet(NEXT_PG_TEXT))
				{
					return (string)ViewState["NextPageText"];
				}
				return "&gt;";
			}
			set
			{
				ViewState["NextPageText"] = value;
				Set(NEXT_PG_TEXT);
			}
		}

		public string PrevPageText
		{
			get
			{
				if(IsSet(PREV_PG_TEXT))
				{
					return (string)ViewState["PrevPageText"];
				}
				return "&lt;";
			}
			set
			{
				ViewState["PrevPageText"] = value;
				Set(PREV_PG_TEXT);
			}
		}

		public int PageButtonCount
		{
			get
			{
				if(IsSet(PG_BTN_COUNT))
				{
					return (int)ViewState["PageButtonCount"];
				}
				return 10;
			}
			set
			{
				ViewState["PageButtonCount"] = value;
				Set(PG_BTN_COUNT);
			}
		}

		public PagerPosition Position
		{
			get
			{
				if(IsSet(POSITION))
				{
					return (PagerPosition)ViewState["Position"];
				}
				return PagerPosition.Bottom;
			}
			set
			{
				if(!Enum.IsDefined(typeof(PagerPosition), value))
				{
					throw new ArgumentException();
				}
				ViewState["Position"] = value;
				Set(POSITION);
			}
		}

		public bool Visible
		{
			get
			{
				if(IsSet(VISIBLE))
				{
					return (bool)ViewState["Visible"];
				}
				return true;
			}
			set
			{
				ViewState["Visible"] = value;
				Set(PG_BTN_COUNT);
			}
		}

		public override void CopyFrom(Style s)
		{
			if(s != null && !s.IsEmpty && s is DataGridPagerStyle)
			{
				base.CopyFrom(s);
				DataGridPagerStyle from = (DataGridPagerStyle)s;
				if(from.IsSet(MODE))
				{
					Mode = from.Mode;
				}
				if(from.IsSet(NEXT_PG_TEXT))
				{
					NextPageText = from.NextPageText;
				}
				if(from.IsSet(PG_BTN_COUNT))
				{
					PageButtonCount = from.PageButtonCount;
				}
				if(from.IsSet(POSITION))
				{
					Position = from.Position;
				}
				if(from.IsSet(VISIBLE))
				{
					Visible = from.Visible;
				}
				if(from.IsSet(PREV_PG_TEXT))
				{
					PrevPageText = from.PrevPageText;
				}
			}
		}

		public override void MergeWith(Style s)
		{
			if(s != null && !s.IsEmpty && s is DataGridPagerStyle)
			{
				base.MergeWith(s);
				DataGridPagerStyle with = (DataGridPagerStyle)s;
				if(with.IsSet(MODE) && !IsSet(MODE))
				{
					Mode = with.Mode;
				}
				if(with.IsSet(NEXT_PG_TEXT) && !IsSet(NEXT_PG_TEXT))
				{
					NextPageText = with.NextPageText;
				}
				if(with.IsSet(PG_BTN_COUNT) && !IsSet(PG_BTN_COUNT))
				{
					PageButtonCount = with.PageButtonCount;
				}
				if(with.IsSet(POSITION) && !IsSet(POSITION))
				{
					Position = with.Position;
				}
				if(with.IsSet(VISIBLE) && !IsSet(VISIBLE))
				{
					Visible = with.Visible;
				}
				if(with.IsSet(PREV_PG_TEXT) && !IsSet(PREV_PG_TEXT))
				{
					PrevPageText = with.PrevPageText;
				}
			}
		}

		public override void Reset()
		{
			if(IsSet(MODE))
			{
				ViewState.Remove("Mode");
			}
			if(IsSet(NEXT_PG_TEXT))
			{
				ViewState.Remove("NextPageText");
			}
			if(IsSet(PG_BTN_COUNT))
			{
				ViewState.Remove("PageButtonCount");
			}
			if(IsSet(POSITION))
			{
				ViewState.Remove("Position");
			}
			if(IsSet(VISIBLE))
			{
				ViewState.Remove("Visible");
			}
			if(IsSet(PREV_PG_TEXT))
			{
				ViewState.Remove("PrevPageText");
			}
			base.Reset();
		}
	}
}
