/**
 * Project   : Mono
 * Namespace : System.Web.UI.MobileControls
 * Class     : List
 * Author    : Gaurav Vaish
 *
 * Copyright : 2003 with Gaurav Vaish, and with
 *             Ximian Inc
 */

using System.Collections;
using System.Web.UI;
using System.Web.Mobile;

namespace System.Web.UI.MobileControls
{
	public class List : PagedControl, INamingContainer, IListControl,
	                    ITemplateable, IPostBackEventHandler
	{
		private static readonly object ItemDataBindEvent = new object();
		private static readonly object ItemCommandEvent  = new object();

		private ListDecoration decoration = ListDecoration.None;

		public List()
		{
		}

		public event ListCommandEventHandler ItemCommand
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

		public event ListDataBindEventHandler ItemDataBind
		{
			add
			{
				Events.AddHandler(ItemDataBindEvent, value);
			}
			remove
			{
				Events.RemoveHandler(ItemDataBindEvent, value);
			}
		}

		private void CreateChildControls(bool doDataBind)
		{
			if(IsTemplated)
			{
				throw new NotImplementedException();
			}
			ChildControlsCreated = true;
		}

		public virtual string DataMember
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public virtual object DataSource
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public string DataTextField
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public string DataValueField
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public ListDecoration Decoration
		{
			get
			{
				return decoration;
			}
			set
			{
				decoration = value;
			}
		}

		public bool HasItemCommandHandler
		{
			get
			{
				return (Events[ItemCommandEvent] != null);
			}
		}

		protected override int InternalItemCount
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public MobileListItemCollection Items
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public bool ItemsAsLinks
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		private void CreateControlItem(MobileListItemType itemType,
		                        ITemplate itemTemplate, bool doDataBind)
		{
			// Create control.
			// Add control at the end of this "List".
			throw new NotImplementedException();
		}

		private int TranslateVirtualItemIndex(int itemIndex)
		{
			throw new NotImplementedException();
		}

		protected override void AddParsedSubObject(object obj)
		{
			if(obj is LiteralControl || obj is MobileControl)
			{
				throw new NotImplementedException();
			}
		}

		protected override void CreateChildControls()
		{
			CreateChildControls(true);
		}

		protected virtual void CreateItems(IEnumerable dataSource)
		{
			throw new NotImplementedException();
		}

		protected override void LoadViewState(object state)
		{
			throw new NotImplementedException();
		}

		protected override bool OnBubbleEvent(object sender, EventArgs e)
		{
			if(e is ListCommandEventArgs)
			{
				OnItemCommand((ListCommandEventArgs)e);
				return true;
			}
			return false;
		}

		protected override void OnDataBinding(EventArgs e)
		{
			base.OnDataBinding(e);
			throw new NotImplementedException();
		}

		protected void OnItemDataBind(ListDataBindEventArgs e)
		{
			ListDataBindEventHandler ldbeh = (ListDataBindEventHandler)(Events[ItemDataBindEvent]);
			if(ldbeh != null)
				ldbeh(this, e);
		}

		protected virtual void OnItemCommand(ListCommandEventArgs e)
		{
			ListCommandEventHandler lceh = (ListCommandEventHandler)(Events[ItemCommandEvent]);
			if(lceh != null)
				lceh(this, e);
		}

		protected override void OnLoadItems(LoadItemsEventArgs e)
		{
			throw new NotImplementedException();
		}

		protected override void OnPageChange(int oldPageIndex,
		                                     int newPageIndex)
		{
			base.OnPageChange(oldPageIndex, newPageIndex);
			throw new NotImplementedException();
		}

		protected override void OnPreRender(EventArgs e)
		{
			throw new NotImplementedException();
		}

		protected override object SaveViewState()
		{
			throw new NotImplementedException();
		}

		protected override void TrackViewState()
		{
			throw new NotImplementedException();
		}

		public override void CreateDefaultTemplatedUI(bool doDataBind)
		{
			throw new NotImplementedException();
		}

		public override void EnsureTemplatedUI()
		{
			EnsureChildControls();
		}

		void IPostBackEventHandler.RaisePostBackEvent(string eventArgument)
		{
			throw new NotImplementedException();
		}

		void IListControl.OnItemDataBind(ListDataBindEventArgs e)
		{
			OnItemDataBind(e);
		}

		bool IListControl.TrackingViewState
		{
			get
			{
				return IsTrackingViewState;
			}
		}
	}
}
