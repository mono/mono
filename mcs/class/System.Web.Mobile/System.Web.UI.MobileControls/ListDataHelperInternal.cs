/**
 * Project   : Mono
 * Namespace : System.Web.UI.MobileControls
 * Class     : ListDataHelperInternal
 * Author    : Gaurav Vaish
 *
 * Copyright : 2003 with Gaurav Vaish, and with
 *             Ximian Inc
 */

using System.Collections;
using System.Web.UI;

namespace System.Web.UI.MobileControls
{
	internal class ListDataHelperInternal
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
		public ListDataHelperInternal(IListControl parent,
		                              StateBag parentViewState)
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
					resolvedDataSrc = DataSourceHelper.GetResolvedDataSource(DataSoure, DataMember);
				}
				return resolvedDataSrc;
			}
		}
		
		
	}
}
