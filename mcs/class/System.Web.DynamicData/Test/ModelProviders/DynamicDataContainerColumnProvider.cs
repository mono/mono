using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.DynamicData;
using System.Web.DynamicData.ModelProviders;

using MonoTests.DataSource;

namespace MonoTests.ModelProviders
{
	public class DynamicDataContainerColumnProvider : ColumnProvider
	{
		DynamicDataColumn column;

		public DynamicDataContainerColumnProvider (DynamicDataContainerTableProvider owner, DynamicDataColumn column)
			: base (owner)
		{
			if (column == null)
				throw new ArgumentNullException ("column");

			this.column = column;

			Type columnType = column.DataType;
			if (columnType == null)
				throw new InvalidOperationException ("column.DataType must not be null for column '" + column.Name + "'");

			Name = column.Name;
			ColumnType = columnType;
			Nullable = columnType.IsGenericType && typeof (Nullable<>).IsAssignableFrom (columnType.GetGenericTypeDefinition ());
			IsPrimaryKey = column.PrimaryKey;
			IsForeignKeyComponent = column.ForeignKey;
		}
	}
}
