/**
 * Namespace: System.Web.UI.WebControls
 * Class:     RepeaterItemEventArgs
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
	public sealed class RepeaterItemEventArgs : EventArgs
	{
		private RepeaterItem item;

		public RepeaterItemEventArgs(RepeaterItem item)
		{
			this.item = item;
		}
		
		public RepeaterItem Item
		{
			get
			{
				return item;
			}
		}
	}
}
