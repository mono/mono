/**
 * Namespace:   System.Web.UI.Design.WebControls
 * Class:       BaseDataListDesigner
 *
 * Author:      Gaurav Vaish
 * Maintainer:  mastergaurav AT users DOT sf DOT net
 *
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.Collections;
using System.ComponentModel.Design;
using System.Reflection;
using System.Web.UI.Design;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

namespace System.Web.UI.Design.WebControls
{
	public abstract class BaseDataListDesigner : TemplatedControlDesigner,
	                                             IDataSourceProvider
	{
		private BaseDataList baseDataList;
		private DataTable    desTimeDataTable;

		public BaseDataListDesigner()
		{
		}

		public string DataKeyField
		{
			get
			{
				return baseDataList.DataKeyField;
			}
			set
			{
				baseDataList.DataKeyField = value;
			}
		}

		public string DataMember
		{
			get
			{
				return baseDataList.DataMember;
			}
			set
			{
				baseDataList.DataMember = value;
			}
		}

		public string DataSource
		{
			get
			{
				DataBinding element = DataBindings["DataSource"];
				if(element != null)
				{
					return element.Expression;
				}
				return String.Empty;
			}
			set
			{
				if(value == null && value.Length == 0)
				{
					DataBindings.Remove("DataSource");
				} else
				{
					DataBinding element = DataBindings["DataSource"];
					if(element == null)
					{
						element = new DataBinding("DataSource",
						                          typeof(IEnumerable),
						                          value);
					} else
					{
						element.Expression = value;
					}
					DataBindings.Add(element);
				}
				OnDataSourceChanged();
				OnBindingsCollectionChanged("DataSource");
			}
		}

		protected internal virtual void OnDataSourceChanged()
		{
			desTimeDataTable = null;
		}

		public override bool DesignTimeHtmlRequiresLoadComplete
		{
			get
			{
				return (DataSource.Length > 0);
			}
		}

		public override DesignerVerbCollection Verbs
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		[MonoTODO]
		public object GetSelectedDataSource()
		{
			throw new NotImplementedException();
		}

		public IEnumerable GetResolvedSelectedDataSource()
		{
			IEnumerable retVal = null;
			DataBinding element = DataBindings["DataSource"];
			if(element != null)
			{
				retVal = DesignTimeData.GetSelectedDataSource(baseDataList,
				                                              element.Expression,
				                                              DataMember);
			}
			return retVal;
		}
	}
}
