using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.DynamicData;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MonoTests.DataSource
{
	[PersistChildren (false)]
	[ParseChildren (true)]
	public class DynamicDataSource : DataSourceControl, IDynamicDataSource
	{
		const string DEFAULT_VIEW_NAME = "DefaultView";
		static readonly string[] emptyNames = new string[] { "DefaultView" };

		DataSourceView defaultView;
		ParameterCollection whereCollection;
		Type dataContainerType;

		public DynamicDataSource ()
			: this (null)
		{
		}

		public DynamicDataSource (string dataContainerTypeName)
		{
			this.DataContainerTypeName = dataContainerTypeName;
		}

		public DataSourceView DefaultView {
			get {
				if (defaultView == null)
					defaultView = CreateView (DEFAULT_VIEW_NAME);
				return defaultView;
			}
		}

		public Type DataContainerType {
			get {
				if (dataContainerType == null)
					dataContainerType = Type.GetType (DataContainerTypeName, true);

				return dataContainerType;
			}
		}

		public string DataContainerTypeName {
			get;
			set;
		}

		public object DataContainerInstance
		{
			get;
			set;
		}

		DataSourceView CreateView (string viewName)
		{
			Type genType = typeof (DynamicDataSourceView<>).GetGenericTypeDefinition ();

			return Activator.CreateInstance (genType.MakeGenericType (new Type[] { ContextType }), this, viewName) as DataSourceView;
		}

		#region DataSourceControl Members
		protected override DataSourceView GetView (string viewName)
		{
			if (String.IsNullOrEmpty (viewName))
				return DefaultView;

			return CreateView (viewName);
		}
		#endregion

		#region IDynamicDataSource Members

		public bool AutoGenerateWhereClause
		{
			get;
			set;
		}

		public Type ContextType
		{
			get;
			set;
		}

		public bool EnableDelete
		{
			get;
			set;
		}

		public bool EnableInsert
		{
			get;
			set;
		}

		public bool EnableUpdate
		{
			get;
			set;
		}

		public string EntitySetName
		{
			get;
			set;
		}

		public event EventHandler<DynamicValidatorEventArgs> Exception;

		public string Where
		{
			get;
			set;
		}

		[PersistenceMode (PersistenceMode.InnerProperty)]
		public ParameterCollection WhereParameters
		{
			get
			{
				if (whereCollection == null)
					whereCollection = new ParameterCollection ();
				return whereCollection;
			}
		}

		#endregion

		#region IDataSource Members
		DataSourceView IDataSource.GetView (string viewName)
		{
			return GetView (viewName);
		}

		ICollection IDataSource.GetViewNames ()
		{
			return emptyNames;
		}

		#endregion
	}
}
