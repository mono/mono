/**
 * Namespace: System.Web.UI.WebControls
 * Class:     DataListItemEventArgs
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
	public sealed class DataListItemEventArgs : EventArgs
	{
		private DataListItem item;

		public DataListItemEventArgs(DataListItem item)
		{
			this.item = item;
		}
		
		public DataListItem Item
		{
			get
			{
				return item;
			}
		}
	}
}
