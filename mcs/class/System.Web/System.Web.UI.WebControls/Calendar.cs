/**
 * Namespace: System.Web.UI.WebControls
 * Class:     Calendar
 * 
 * Author:  Gaurav Vaish
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Status:  20%
 * 
 * (C) Gaurav Vaish (2001)
 */

using System;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public class Calendar : WebControl, IPostBackEventHandler
	{
		//
		
		private TableItemStyle dayHeaderStyle;
		public Calendar(): base()
		{
			//TODO: Initialization
		}
		
		public int CellPadding
		{
			get
			{
				object o = ViewState["CellPadding"];
				if(o!=null)
					return (int)o;
				return 2;
			}
			set
			{
				ViewState["CellPadding"] = value;
			}
		}

		public int CellSpacing
		{
			get
			{
				object o = ViewState["CellSpacing"];
				if(o!=null)
					return (int)o;
				return 0;
			}
			set
			{
				if(value<-1)
					throw new ArgumentOutOfRangeException();
				ViewState["CellSpacing"] = value;
			}
		}
		
		public TableItemStyle DayHeaderStyle
		{
			get
			{
				if(dayHeaderStyle==null)
					dayHeaderStyle = new TableItemStyle();
				return dayHeaderStyle;
			}
		}

		public DayNameFormat DayNameFormat
		{
			get
			{
				object o = ViewState["DayNameFormat"];
				if(o!=null)
					return (DayNameFormat)o;
				return DayNameFormat.Short;
			}
			set
			{
				if(!System.Enum.IsDefined(typeof(DayNameFormat),value))
					throw new ArgumentException();
				ViewState["DayNameFormat"] = value;
			}
		}
		
		public void RaisePostBackEvent(string eventArgument)
		{
			//TODO: THE LOST WORLD
			// Written to keep compile get going
		}
	}
}
