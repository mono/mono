/**
 * Project   : Mono
 * Namespace : System.Web.UI.MobileControls
 * Class     : ObjectListCommandEventHandler
 * Author    : Gaurav Vaish
 *
 * Copyright : 2003 with Gaurav Vaish, and with
 *             Ximian Inc
 */

using System;
using System.Web.UI.WebControls;

namespace System.Web.UI.MobileControls
{
	public class ObjectListCommandEventArgs : CommandEventArgs
	{
		private ObjectListItem item;
		private object         commandSource;

		public ObjectListCommandEventArgs(ObjectListItem item,
		                                  object source, CommandEventArgs e)
		                                  : base(e)
		{
			this.item          = item;
			this.commandSource = source;
		}

		public ObjectListCommandEventArgs(ObjectListItem item,
		                                  string commandName)
		                                  : base(commandName, item)
		{
			this.item          = item;
			this.commandSource = null;
		}

		public object CommandSource
		{
			get
			{
				return this.commandSource;
			}
		}

		public ObjectListItem ListItem
		{
			get
			{
				return this.item;
			}
		}

		protected static readonly string DefaultCommand = "Default";
	}
}
