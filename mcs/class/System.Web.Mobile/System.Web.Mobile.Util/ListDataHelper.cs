/**
 * Project   : Mono
 * Namespace : System.Web.Mobile.Util
 * Class     : ListDataHelper
 * Author    : Gaurav Vaish
 *
 * Copyright : 2003 with Gaurav Vaish, and with
 *             Ximian Inc
 */

using System.Collections;
using System.Web.UI;
using System.Web.Mobile;
using System.Web.UI.MobileControls;

namespace System.Web.Mobile.Util
{
	internal class ListDataHelper
	{
		private object dataSource;
		private int    dataSourceCount = -1;
		private IEnumerable resolvedDataSrc;
		private MobileListItemCollection items;
		private string dataTextField;
		private string dataValueField;
		private bool   bindFromFields;

		private IListControl parent;
		private StateBag     parentViewState;

		public ListDataHelper(IListControl parent, StateBag parentViewState)
		{
			this.parent          = parent;
			this.parentViewState = parentViewState;
		}

		public string DataMember
		{
			get
			{
				object o = parentViewState["DataMember"];
				if(o != null)
					return (string)o;
				return String.Empty;
			}
			set
			{
				parentViewState["DataMember"] = value;
			}
		}

		public string DataTextField
		{
			get
			{
				object o = parentViewState["DataTextField"];
				if(o != null)
					return (string)o;
				return String.Empty;
			}
			set
			{
				parentViewState["DataTextField"] = value;
			}
		}

		public string DataValueField
		{
			get
			{
				object o = parentViewState["DataValueField"];
				if(o != null)
					return (string)o;
				return String.Empty;
			}
			set
			{
				parentViewState["DataValueField"] = value;
			}
		}

		public object DataSource
		{
			get
			{
				return this.dataSource;
			}
			set
			{
				this.dataSource = value;
			}
		}

		public int DataSourceCount
		{
			get
			{
				if(dataSourceCount == -1)
				{
					if(ResolvedDataSource != null)
					{
						if(ResolvedDataSource is ICollection)
							dataSourceCount = ((ICollection)ResolvedDataSource).Count;
					}
				}
				return dataSourceCount;
			}
		}

		public IEnumerable ResolvedDataSource
		{
			get
			{
				if(this.resolvedDataSrc == null)
				{
					resolvedDataSrc = DataSourceHelper.GetResolvedDataSource(DataSource, DataMember);
				}
				return resolvedDataSrc;
			}
		}

		public MobileListItemCollection Items
		{
			get
			{
				if(items == null)
				{
					items = new MobileListItemCollection();
					if(parent.TrackingViewState)
						((IStateManager)items).TrackViewState();
				}
				return items;
			}
		}

		public void AddItem(MobileListItem item)
		{
			Items.Add(item);
		}
		
		public MobileListItem CreateItem(object dataItem)
		{
			MobileListItem retVal;
			string itemText = null;
			string itemValue = null;
			if(bindFromFields)
			{
				if(this.dataTextField.Length > 0)
				{
					itemText = DataBinder.GetPropertyValue(dataItem,
					                        dataTextField, "{0}");
				}
				if(this.dataValueField.Length > 0)
				{
					itemValue = DataBinder.GetPropertyValue(dataItem,
					                         dataValueField, "{0}");
				}
			} else
			{
				itemText = dataItem.ToString();
			}
			retVal = new MobileListItem(dataItem, itemText, itemValue);
			if(dataItem != null)
			{
				parent.OnItemDataBind(new ListDataBindEventArgs(retVal, dataItem));
			}
			return retVal;
		}
	}
}
