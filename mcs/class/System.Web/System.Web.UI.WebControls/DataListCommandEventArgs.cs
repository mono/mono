/**
* Namespace: System.Web.UI.WebControls
* Class:     DataListCommandEventArgs
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
	public sealed class DataListCommandEventArgs: CommandEventArgs
	{
		private DataListItem dlItem;
		private object       cmdSrc;
		
		public DataListCommandEventArgs(DataListItem item, object commandSource, CommandEventArgs originalArgs): base(originalArgs)
		{
			dlItem = item;
			cmdSrc = commandSource;
		}
		
		public object CommandSource
		{
			get
			{
				return cmdSrc;
			}
		}
		
		public DataListItem Item
		{
			get
			{
				return dlItem;
			}
		}
	}
}
