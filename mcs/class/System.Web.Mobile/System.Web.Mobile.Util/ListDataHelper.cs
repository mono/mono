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


	}
}
