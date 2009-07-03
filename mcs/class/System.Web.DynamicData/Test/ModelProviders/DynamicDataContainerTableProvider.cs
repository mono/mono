using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using System.Web.DynamicData;
using System.Web.DynamicData.ModelProviders;

using MonoTests.DataSource;

namespace MonoTests.ModelProviders
{
	public class DynamicDataContainerTableProvider <T> : TableProvider
	{
		ReadOnlyCollection<ColumnProvider> columns;
		DynamicDataTable table;

		public DynamicDataContainerTableProvider (DynamicDataContainerModelProvider <T> owner, DynamicDataTable table)
			: base (owner)
		{
			if (table == null)
				throw new ArgumentNullException ("table");

			this.EntityType = table.DataType;
			this.Name = table.Name;
			this.table = table;
		}

		public override ReadOnlyCollection<ColumnProvider> Columns
		{
			get
			{
				if (columns != null)
					return columns;

				columns = LoadColumns ();
				return columns;
			}
		}

		public override IQueryable GetQuery (object context)
		{
			throw new NotImplementedException ();
		}

		ReadOnlyCollection<ColumnProvider> LoadColumns ()
		{
			List<DynamicDataColumn> containerColumns = table.GetColumns ();

			if (containerColumns == null || containerColumns.Count == 0)
				return new ReadOnlyCollection<ColumnProvider> (new List<ColumnProvider> ());

			var columns = new List<ColumnProvider> ();
			foreach (var column in containerColumns)
				columns.Add (new DynamicDataContainerColumnProvider <T> (this, column));

			return new ReadOnlyCollection<ColumnProvider> (columns);
		}

		public void ResolveAssociations ()
		{
			DynamicDataContainerColumnProvider <T> column;
			foreach (var cp in Columns) {
				column = cp as DynamicDataContainerColumnProvider <T>;
				if (column == null)
					continue;
				column.ResolveAssociations ();
			}
		}
	}
}
