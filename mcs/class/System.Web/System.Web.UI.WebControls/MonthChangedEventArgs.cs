/**
 * Namespace: System.Web.UI.WebControls
 * Class:     MonthChangedEventArgs
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
	public sealed class MonthChangedEventArgs
	{
		private DateTime nDate;
		private DateTime pDate;

		public MonthChangedEventArgs(DateTime newDate, DateTime previousDate)
		{
			nDate = newDate;
			pDate = previousDate;
		}
		
		public DateTime NewDate
		{
			get
			{
				return nDate;
			}
		}
		
		public DateTime PreviousDate
		{
			get
			{
				return pDate;
			}
		}
	}
}
