
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
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
