using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MonoTests.DataSource
{
	public class DynamicDataSourceView<T> : DataSourceView
	{
		#region Private fields
		DynamicDataSource owner;
		IDynamicDataContainer<T> dataContainer;
		#endregion

		#region Can* overrides
		public override bool CanDelete
		{
			get { return true; }
		}

		public override bool CanInsert
		{
			get { return true; }
		}

		public override bool CanPage
		{
			get { return true; }
		}

		public override bool CanRetrieveTotalRowCount
		{
			get { return true; }
		}

		public override bool CanSort
		{
			get { return true; }
		}

		public override bool CanUpdate
		{
			get { return true; }
		}
		#endregion

		public IDynamicDataContainer<T> DataContainerInstance
		{
			get
			{
				if (dataContainer != null)
					return dataContainer;

				object inst = owner.DataContainerInstance;
				if (inst != null) {
					dataContainer = inst as IDynamicDataContainer<T>;
					if (dataContainer == null)
						throw new InvalidOperationException (
						    "DynamicDataSource.DataContainerInstance must be set to an instance of '" + typeof (IDynamicDataContainer <T>) + "'.");
					if (String.IsNullOrEmpty (dataContainer.TableName))
						dataContainer.TableName = owner.EntitySetName;
					return dataContainer;
				}

				string typeName = owner.DataContainerTypeName;
				if (String.IsNullOrEmpty (typeName))
					throw new InvalidOperationException ("DynamicDataSource '" + owner.ID + "' does not specify either data container instance or its type name.");

				Type type = Type.GetType (typeName, true);
				if (!typeof (IDynamicDataContainer<T>).IsAssignableFrom (type))
					throw new InvalidOperationException ("Data container type '" + typeName + "' specified by DynamicDataSource '" + owner.ID + "' does not implement the IDynamicDataContainer interface.");
				dataContainer = Activator.CreateInstance (type, owner.EntitySetName) as IDynamicDataContainer<T>;
				if (dataContainer == null)
					throw new InvalidOperationException ("Failed to create instance of data container type '" + typeName + "'.");
				return dataContainer;
			}
		}

		#region Constructors
		public DynamicDataSourceView (DynamicDataSource owner, string viewName)
			: base (owner, viewName)
		{
			this.owner = owner;
		}

		#endregion

		#region DataSourceView methods
		protected override int ExecuteDelete (IDictionary keys, IDictionary oldValues)
		{
			return DataContainerInstance.Delete (keys, oldValues);
		}

		protected override int ExecuteInsert (IDictionary values)
		{
			return DataContainerInstance.Insert (values);
		}

		protected override int ExecuteUpdate (IDictionary keys, IDictionary values, IDictionary oldValues)
		{
			return DataContainerInstance.Update (keys, values, oldValues);
		}

		protected override IEnumerable ExecuteSelect (DataSourceSelectArguments arguments)
		{
			return DataContainerInstance.Select (arguments, owner.Where, owner.WhereParameters);
		}
		#endregion
	}
}
