
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
using System.ComponentModel;
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
		private DataTable    dummyDataTable;

		private static readonly string[] validNames = new string[] {
			"AlternatingItemStyle",
			"BackColor",
			"DataSource",
			"DataMember",
			"EditItemStyle",
			"Font",
			"ForeColor",
			"HeaderStyle",
			"FooterStyle",
			"ItemStyle",
			"SelectedItemStyle",
			"SeparatorStyle"
		};

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

		public object GetSelectedDataSource()
		{
			object retVal = null;
			DataBinding element = DataBindings["DataSource"];
			if(element != null)
			{
				retVal = DesignTimeData.GetSelectedDataSource(baseDataList,
				                                       element.Expression);
			}
			return retVal;
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

		public override IEnumerable GetTemplateContainerDataSource(
		                                               string templateName)
		{
			return GetResolvedSelectedDataSource();
		}

		public override void Initialize(IComponent component)
		{
			baseDataList = (BaseDataList)component;
			base.Initialize(component);
		}

		public override void OnComponentChanged(object sender,
		                                        ComponentChangedEventArgs e)
		{
			if(e.Member != null)
			{
				string name = e.Member.Name;
				foreach(string current in validNames)
				{
					if(name == current)
					{
						OnStylesChanged();
						break;
					}
				}
			}
			base.OnComponentChanged(sender, e);
		}

		protected internal void OnStylesChanged()
		{
			OnTemplateEditingVerbsChanged();
		}

		protected abstract void OnTemplateEditingVerbsChanged();

		protected override void Dispose(bool disposing)
		{
			if(disposing)
				baseDataList = null;
			base.Dispose(disposing);
		}

		protected IEnumerable GetDesignTimeDataSource(int minimumRows,
		                                      out bool dummyDataSource)
		{
			return GetDesignTimeDataSource(GetResolvedSelectedDataSource(),
			                               minimumRows,
			                               out dummyDataSource);
		}

		protected IEnumerable GetDesignTimeDataSource(IEnumerable selectedDataSource,
		                                              int minimumRows,
		                                              out bool dummyDataSource)
		{
			DataTable toDeploy = desTimeDataTable;
			dummyDataSource = false;
			if(minimumRows == 0)
			{
				if(selectedDataSource != null)
				{
					desTimeDataTable = DesignTimeData.CreateSampleDataTable(
					                                  selectedDataSource);
					toDeploy = desTimeDataTable;
				}
				if(toDeploy == null)
				{
					if(dummyDataTable == null)
						dummyDataTable = DesignTimeData.CreateDummyDataTable();
					toDeploy = dummyDataTable;
					dummyDataSource = true;
				}
			}
			return DesignTimeData.GetDesignTimeDataSource(toDeploy, minimumRows);
		}
	}
}
