/**
 * Namespace: System.Web.UI.WebControls
 * Class:     Calendar
 * 
 * Author:  Gaurav Vaish
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Status:  60%
 * 
 * (C) Gaurav Vaish (2001)
 */

using System;
using System.Collections;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public class Calendar : WebControl, IPostBackEventHandler
	{
		//
		
		private TableItemStyle          dayHeaderStyle;
		private TableItemStyle          dayStyle;
		private TableItemStyle          otherMonthDayStyle;
		private SelectedDatesCollection selectedDates;
		private ArrayList               selectedDatesList;
		private TableItemStyle          selectedDayStyle;
		private TableItemStyle          selectorStyle;
		private TableItemStyle          titleStyle;
		private TableItemStyle          todayDayStyle;
		private TableItemStyle          weekendDayStyle;

		private static readonly object DayRenderEvent        = new object();
		private static readonly object SelectionChangedEvent = new object();

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

		public TableItemStyle DayStyle
		{
			get
			{
				if(dayStyle==null)
					dayStyle = new TableItemStyle();
				return dayStyle;
			}
		}
		
		public FirstDayOfWeek FirstDayOfWeek
		{
			get
			{
				object o = ViewState["FirstDayOfWeek"];
				if(o!=null)
					return (FirstDayOfWeek)o;
				return FirstDayOfWeek.Default;
			}
			set
			{
				if(!System.Enum.IsDefined(typeof(FirstDayOfWeek), value))
					throw new ArgumentException();
				ViewState["FirstDayOfWeek"] = value;
			}
		}
		
		public string NextMonthText
		{
			get
			{
				object o = ViewState["NextMonthText"];
				if(o!=null)
					return (string)o;
				return "&gt;";
			}
			set
			{
				ViewState["NextMonthText"] = value;
			}
		}
		
		public NextPrevFormat NextPrevFormat
		{
			get
			{
				object o = ViewState["NextPrevFormat"];
				if(o!=null)
					return (NextPrevFormat)o;
				return NextPrevFormat.CustomText;
			}
			set
			{
				if(!System.Enum.IsDefined(typeof(NextPrevFormat), value))
					throw new ArgumentException();
				ViewState["NextPrevFormat"] = value;
			}
		}
		
		public TableItemStyle OtherMonthDayStyle
		{
			get
			{
				if(otherMonthDayStyle == null)
					otherMonthDayStyle = new TableItemStyle();
				return otherMonthDayStyle;
			}
		}
		
		public string PrevMonthText
		{
			get
			{
				object o = ViewState["PrevMonthText"];
				if(o!=null)
					return (string)o;
				return "&lt;";
			}
			set
			{
				ViewState["PrevMonthText"] = value;
			}
		}
		
		public DateTime SelectedDate
		{
			// TODO: Am I right here? I got confused with the "Remarks" written in the documentation
			/*
			 * Looks like I have to first do something with SelectionMode,
			 * then with SelectedDates,
			 * Update when SelectionChanged is called => Link to the function.
			 * Pretty confused at this point
			*/
			get
			{
				object o = ViewState["SelectedDate"];
				if(o!=null)
					return (DateTime)o;
				return DateTime.MinValue;
			}
			set
			{
				ViewState["SelectedDate"] = value;
			}
		}
		
		public SelectedDatesCollection SelectedDates
		{
			get
			{
				if(selectedDates==null)
				{
					if(selectedDatesList == null)
						selectedDatesList = new ArrayList();
					selectedDates = new SelectedDatesCollection(selectedDatesList);
				}
				return selectedDates;
			}
		}
		
		public TableItemStyle SelectedDayStyle
		{
			get
			{
				if(selectedDayStyle==null)
					selectedDayStyle = new TableItemStyle();
				return selectedDayStyle;
			}
		}

		public CalendarSelectionMode SelectionMode
		{
			get
			{
				object o = ViewState["SelectionMode"];
				if(o!=null)
					return (CalendarSelectionMode)o;
				return CalendarSelectionMode.Day;
			}
			set
			{
				if(!System.Enum.IsDefined(typeof(CalendarSelectionMode), value))
					throw new ArgumentException();
				ViewState["SelectionMode"] = value;
			}
		}
		
		public string SelectedMonthText
		{
			get
			{
				object o = ViewState["SelectedMonthText"];
				if(o!=null)
					return (string)o;
				return "&gt;&gt;";
			}
			set
			{
				ViewState["SelectedMonthText"] = value;
			}
		}

		public TableItemStyle SelectorStyle
		{
			get
			{
				if(selectorStyle==null)
					selectorStyle = new TableItemStyle();
				return selectorStyle;
			}
		}
		
		public string SelectedWeekText
		{
			get
			{
				object o = ViewState["SelectedWeekText"];
				if(o!=null)
					return (string)o;
				return "&gt;";
			}
			set
			{
				ViewState["SelectedWeekText"] = value;
			}
		}
		
		public bool ShowDayHeader
		{
			get
			{
				object o = ViewState["ShowDayHeader"];
				if(o!=null)
					return (bool)o;
				return true;
			}
			set
			{
				ViewState["ShowDayHeader"] = value;
			}
		}
		
		public bool ShowGridLines
		{
			get
			{
				object o = ViewState["ShowGridLines"];
				if(o!=null)
					return (bool)o;
				return false;
			}
			set
			{
				ViewState["ShowGridLines"] = value;
			}
		}
		
		public bool ShowNextPrevMonth
		{
			get
			{
				object o = ViewState["ShowNextPrevMonth"];
				if(o!=null)
					return (bool)o;
				return true;
			}
			set
			{
				ViewState["ShowNextPrevMonth"] = value;
			}
		}
		
		public bool ShowTitle
		{
			get
			{
				object o = ViewState["ShowTitle"];
				if(o!=null)
					return (bool)o;
				return true;
			}
			set
			{
				ViewState["ShowTitle"] = value;
			}
		}

		public TitleFormat TitleFormat
		{
			get
			{
				object o = ViewState["TitleFormat"];
				if(o!=null)
					return (TitleFormat)o;
				return TitleFormat.MonthYear;
			}
			set
			{
				if(!System.Enum.IsDefined(typeof(TitleFormat), value))
					throw new ArgumentException();
				ViewState["TitleFormat"] = value;
			}
		}
		
		public TableItemStyle TitleStyle
		{
			get
			{
				if(titleStyle==null)
					titleStyle = new TableItemStyle();
				return titleStyle;
			}
		}
		
		public TableItemStyle TodayDayStyle
		{
			get
			{
				if(todayDayStyle==null)
					todayDayStyle = new TableItemStyle();
				return todayDayStyle;
			}
		}
		
		public DateTime TodaysDate
		{
			get
			{
				object o = ViewState["TodaysDate"];
				if(o!=null)
					return (DateTime)o;
				return DateTime.Today;
			}
			set
			{
				ViewState["TodaysDate"] = value;
			}
		}
		
		public DateTime VisibleDate
		{
			get
			{
				object o = ViewState["VisibleDate"];
				if(o!=null)
					return (DateTime)o;
				return DateTime.MinValue;
			}
			set
			{
				ViewState["VisibleDate"] = value;
			}
		}
		
		public TableItemStyle WeekendDayStyle
		{
			get
			{
				if(weekendDayStyle == null)
					weekendDayStyle = new TableItemStyle();
				return weekendDayStyle;
			}
		}
		
		public event DayRenderEventHandler DayRender
		{
			add
			{
				Events.AddHandler(DayRenderEvent, value);
			}
			remove
			{
				Events.RemoveHandler(DayRenderEvent, value);
			}
		}
		
		public event EventHandler SelectionChanged
		{
			add
			{
				Events.AddHandler(SelectionChangedEvent, value);
			}
			remove
			{
				Events.RemoveHandler(SelectionChangedEvent, value);
			}
		}

		protected virtual void OnDayRender(TableCell cell, CalendarDay day)
		{
			if(Events!=null)
			{
				DayRenderEventHandler dreh = (DayRenderEventHandler)(Events[DayRenderEvent]);
				if(dreh!=null)
					dreh(this, new DayRenderEventArgs(cell, day));
			}
		}
		
		protected virtual void OnSelectionChanged()
		{
			if(Events!=null)
			{
				EventHandler eh = (EventHandler)(Events[SelectionChangedEvent]);
				if(eh!=null)
					eh(this, new EventArgs());
			}
		}
		
		public void RaisePostBackEvent(string eventArgument)
		{
			//TODO: THE LOST WORLD
			// Written to keep compile get going
		}
		
		protected override void Render(HtmlTextWriter writer)
		{
			//TODO: Ofcourse, I have to override this function
		}
		
		//TODO: Recheck, I am through with all the functions?
	}
}
