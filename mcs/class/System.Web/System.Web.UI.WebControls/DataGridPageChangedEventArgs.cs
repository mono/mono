/**
 * Namespace: System.Web.UI.WebControls
 * Class:     DataGridPageChangedEventArgs
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
	public sealed class DataGridPageChangedEventArgs : EventArgs
	{
		private object source;
		private int    npIndex;
		
		public DataGridPageChangedEventArgs(object commandSource, int newPageIndex)
		{
			source  = commandSource;
			npIndex = newPageIndex;
		}
		
		public object CommandSource
		{
			get
			{
				return source;
			}
		}
		
		public int NewPageIndex
		{
			get
			{
				return npIndex;
			}
		}
	}
}
