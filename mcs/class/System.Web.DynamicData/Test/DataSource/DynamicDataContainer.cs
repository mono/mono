using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MonoTests.DataSource
{
	public abstract class DynamicDataContainer<T> : IDynamicDataContainer<T>
	{
		T containedTypeInstance;

		public string TableName {
			get;
			set;
		}

		public T ContainedTypeInstance {
			get { return containedTypeInstance; }
			set {
				if (value == null)
					throw new InvalidOperationException ("Null type instance is not allowed.");
				containedTypeInstance = value;
			}
		}

		public DynamicDataContainer ()
			: this (null)
		{ }

		public DynamicDataContainer (string tableName)
		{
			TableName = tableName;
			ContainedTypeInstance = Activator.CreateInstance<T> ();
		}

		public virtual Type ContainedType
		{
			get { return typeof (T); }
		}

		#region IDynamicDataContainer Members
		public abstract int Update (IDictionary keys, IDictionary values, IDictionary oldValues);
		public abstract int Insert (IDictionary values);
		public abstract int Delete (IDictionary keys, IDictionary oldValues);
		public abstract IEnumerable Select (DataSourceSelectArguments args, string where, ParameterCollection whereParams);
		public abstract List<DynamicDataTable> GetTables ();
		#endregion
	}
}
