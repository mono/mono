/**
 * Namespace: System.Web.UI.WebControls
 * Class:     CalendarDay
 * 
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Status:  100%
 * 
 * (C) Gaurav Vaish (2001)
 */

using System;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public class CalendarDay
	{
		private DateTime date;
		private bool     isWeekend;
		private bool     isToday;
		private bool     isSelected;
		private bool     isOtherMonth;
		private bool     isSelectable;
		private string   dayNumberText;

		public CalendarDay(DateTime date, bool isWeekend, bool isToday, bool isSelected, bool isOtherMonth, string dayNumberText)
		{
			this.date = date;
			this.isWeekend = isWeekend;
			this.isToday = isToday;
			this.isSelected = isSelected;
			this.isOtherMonth = isOtherMonth;
			this.dayNumberText = dayNumberText;
		}
		
		public DateTime Date
		{
			get
			{
				return date;
			}
		}
		
		public string DayNumberText
		{
			get
			{
				return dayNumberText;
			}
		}
		
		public bool IsOtherMonth
		{
			get
			{
				return isOtherMonth;
			}
		}
		
		public bool IsSelectable
		{
			get
			{
				return isSelectable;
			}
			set
			{
				isSelectable = value;
			}
		}
		
		public bool IsSelected
		{
			get
			{
				return isSelected;
			}
			set
			{
				isSelected = value;
			}
		}
		
		public bool IsToday
		{
			get
			{
				return isToday;
			}
			set
			{
				isToday = value;
			}
		}
		
		public bool IsWeekend
		{
			get
			{
				return isWeekend;
			}
			set
			{
				isWeekend = value;
			}
		}
	}
}
