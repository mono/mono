/**
 * Namespace: System.Web.UI.WebControls
 * Delegate:  DayRenderEventArgs
 * 
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  100%
 * 
 * (C) Gaurav Vaish (2001)
 */

namespace System.Web.UI.WebControls
{
	public sealed class DayRenderEventArgs
	{
		private TableCell   cell;
		private CalendarDay day;
		
		public DayRenderEventArgs(TableCell cell, CalendarDay day)
		{
			this.cell = cell;
			this.day  = day;
		}
		
		public TableCell Cell
		{
			get
			{
				return cell;
			}
		}
		
		public CalendarDay Day
		{
			get
			{
				return day;
			}
		}
	}
}
