/**
* Namespace: System.Web.UI.WebControls
* Class:     DataGridCommandEventArgs
*
* Author:  Gaurav Vaish
* Maintainer: gvaish@iitk.ac.in
* Implementation: yes
* Status:  100%
*
* (C) Gaurav Vaish (2001)
*/

using System;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public sealed class DataGridCommandEventArgs : CommandEventArgs
	{
		DataGridItem     dgItem;
		object           cmdSrc;
		
		public DataGridCommandEventArgs(DataGridItem item, object commandSource, CommandEventArgs originalArgs): base(originalArgs)
		{
			dgItem = item;
			cmdSrc = commandSource;
		}
		
		public object CommandSource
		{
			get
			{
				return cmdSrc;
			}
		}
		
		public DataGridItem Item
		{
			get
			{
				return dgItem;
			}
		}
	}
}
