/**
* Namespace: System.Web.UI.WebControls
* Class:     CommandEventArgs
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
	public class CommandEventArgs : EventArgs
	{
		private string cmdName;
		private object cmdArg;
		
		public CommandEventArgs(CommandEventArgs e)
		{
			CommandEventArgs(e.CommandName, e.CommandArgument);
		}
		
		public CommandEventArgs(string commandName, object argument)
		{
			cmdName = commandName;
			cmdArg  = argument;
		}
		
		public string CommandName
		{
			get
			{
				return cmdName;
			}
		}
		
		public object CommandArgument
		{
			get
			{
				return cmdArg;
			}
		}
	}
}
