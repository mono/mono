/**
* Namespace: System.Web.UI.WebControls
* Class:     RepeaterCommandEventArgs
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
	public sealed class RepeaterCommandEventArgs: CommandEventArgs
	{
		private RepeaterItem rItem;
		private object       cmdSrc;

		public RepeaterCommandEventArgs(RepeaterItem item, object commandSource, CommandEventArgs originalArgs): base(originalArgs)
		{
			rItem = item;
			cmdSrc = commandSource;
		}

		public object CommandSource
		{
			get
			{
				return cmdSrc;
			}
		}
		
		public RepaterItem Item
		{
			get
			{
				return rItem;
			}
		}
	}
}
