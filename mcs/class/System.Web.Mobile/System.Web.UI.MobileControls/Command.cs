/**
 * Project   : Mono
 * Namespace : System.Web.UI.MobileControls
 * Class     : Command
 * Author    : Gaurav Vaish
 *
 * Copyright : 2003 with Gaurav Vaish, and with
 *             Ximian Inc
 */

using System.Web.UI;

namespace System.Web.UI.MobileControls
{
	public class Command : TextControl, IPostBackEventHandler
	{
		private static readonly object ClickEvent       = new object();
		private static readonly object ItemCommandEvent = new object();

		public Command()
		{
		}

		public event EventHandler Click
		{
			add
			{
				Events.AddHandler(ClickEvent, value);
			}
			remove
			{
				Events.RemoveHandler(ClickEvent, value);
			}
		}

		public event ObjectListCommandEventHandler ItemCommand
		{
			add
			{
				Events.AddHandler(ItemCommandEvent, value);
			}
			remove
			{
				Events.RemoveHandler(ItemCommandEvent, value);
			}
		}

		void IPostBackEventHandler.RaisePostBackEvent(string eventArgument)
		{
			throw new NotImplementedException();
		}
	}
}
