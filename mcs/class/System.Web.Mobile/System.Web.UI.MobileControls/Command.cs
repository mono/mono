/**
 * Project   : Mono
 * Namespace : System.Web.UI.MobileControls
 * Class     : Command
 * Author    : Gaurav Vaish
 *
 * Copyright : 2003 with Gaurav Vaish, and with
 *             Ximian Inc
 */

using System.Collections.Specialized;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace System.Web.UI.MobileControls
{
	public class Command : TextControl, IPostBackEventHandler,
	                       IPostBackDataHandler
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

		bool IPostBackDataHandler.LoadPostData(string key,
		                                      NameValueCollection data)
		{
			bool dataChanged;
			bool stateChanged = Adapter.LoadPostData(key, data, null, out dataChanged);
			if(stateChanged)
			{
				if(dataChanged)
					Page.RegisterRequiresRaiseEvent(this);
			} else
			{
				if(data[key] != null)
					Page.RegisterRequiresRaiseEvent(this);
			}
			return false;
		}

		void IPostBackDataHandler.RaisePostDataChangedEvent()
		{
		}

		void IPostBackEventHandler.RaisePostBackEvent(string eventArgument)
		{
			if(CausesValidation)
				MobilePage.Validate();
			Form.CurrentPage = 1;
			OnClick(EventArgs.Empty);
			OnItemCommand(new CommandEventArgs(CommandName, CommandArgument));
		}

		public bool CausesValidation
		{
			get
			{
				object o = ViewState["CausesValidation"];
				if(o != null)
					return (bool)o;
				return true;
			}
			set
			{
				ViewState["CausesValidation"] = value;
			}
		}

		public string CommandArgument
		{
			get
			{
				object o = ViewState["CommandArgument"];
				if(o != null)
					return (string)o;
				return String.Empty;
			}
			set
			{
				ViewState["CommandArgument"] = value;
			}
		}

		public string CommandName
		{
			get
			{
				object o = ViewState["CommandName"];
				if(o != null)
					return (string)o;
				return String.Empty;
			}
			set
			{
				ViewState["CommandName"] = value;
			}
		}

		public CommandFormat Format
		{
			get
			{
				object o = ViewState["Format"];
				if(o != null)
					return (CommandFormat)o;
				return CommandFormat.Button;
			}
			set
			{
				//if(!System.Enum.IsDefined(typeof(CommandFormat), value)
				//	throw new ArgumentException("Illegal value");
				ViewState["Format"] = value;
			}
		}

		public string ImageUrl
		{
			get
			{
				object o = ViewState["ImageUrl"];
				if(o != null)
					return (string)o;
				return String.Empty;
			}
			set
			{
				ViewState["ImageUrl"] = value;
			}
		}

		public string SoftKeyLabel
		{
			get
			{
				object o = ViewState["SoftKeyLabel"];
				if(o != null)
					return (string)o;
				return String.Empty;
			}
			set
			{
				ViewState["SoftKeyLabel"] = value;
			}
		}

		protected virtual void OnClick(EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events[ClickEvent]);
			if(eh != null)
				eh(this, e);
		}

		protected virtual void OnItemCommand(CommandEventArgs e)
		{
			CommandEventHandler ceh = (CommandEventHandler)(Events[ItemCommandEvent]);
			if(ceh != null)
				ceh(this, e);
		}

		protected virtual bool IsFormSubmitControl()
		{
			return true;
		}
	}
}
