using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.DynamicData;
using System.Web.DynamicData.ModelProviders;

using MonoTests.DataSource;

namespace MonoTests.ModelProviders
{
	public class DynamicDataContainerColumnProvider <T> : ColumnProvider
	{
		DynamicDataColumn column;
		bool associationResolved;

		public override AssociationProvider Association	{
			get {
				ResolveAssociations ();
				return base.Association;
			}

			protected set {
				base.Association = value;
			}
		}

		public DynamicDataContainerColumnProvider (DynamicDataContainerTableProvider <T> owner, DynamicDataColumn column)
			: base (owner)
		{
			if (column == null)
				throw new ArgumentNullException ("column");

			this.column = column;

			Type columnType = column.DataType;
			if (columnType == null)
				throw new InvalidOperationException ("column.TContext must not be null for column '" + column.Name + "'");

			Name = column.Name;
			ColumnType = columnType;
			Nullable = columnType.IsGenericType && typeof (Nullable<>).IsAssignableFrom (columnType.GetGenericTypeDefinition ());
			IsPrimaryKey = column.PrimaryKey;
			EntityTypeProperty = GetPropertyInfo (owner.EntityType, Name);
			IsCustomProperty = column.CustomProperty;
			IsGenerated = column.Generated;
			MaxLength = GetMaxLength (EntityTypeProperty);
			IsSortable = column.Sortable;
		}

		public void ResolveAssociations ()
		{
			if (associationResolved)
				return;

			associationResolved = true;
			string associated = column.AssociatedTo;
			if (String.IsNullOrEmpty (associated))
				return;

			string[] names = associated.Split (new char[] { '.' });
			if (names.Length != 2)
				throw new ApplicationException ("Only associations of type Table.Column are supported");
			string tableName = names[0];
			string columnName = names[1];
			
			TableProvider tableProvider = null;
			try {
				tableProvider = Table.DataModel.Tables.First<TableProvider> ((TableProvider tp) => {
					if (tp.Name == tableName)
						return true;
					return false;
				});
			} catch {
				return;
			}

			if (tableProvider == null)
				return;

			ColumnProvider toColumn = null;

			try {
				toColumn = tableProvider.Columns.First<ColumnProvider> ((ColumnProvider cp) => {
					if (cp.Name == columnName)
						return true;
					return false;
				});
			} catch {
				return;
			}

			if (toColumn == null)
				return;

			IsForeignKeyComponent = true;
			Association = new DynamicDataAssociationProvider (column.AssociationDirection, this, toColumn);
		}

		int GetMaxLength (PropertyInfo pi)
		{
			if (pi == null)
				return 0;

			object[] attrs = pi.GetCustomAttributes (typeof (DynamicDataStringLengthAttribute), true);
			if (attrs == null || attrs.Length == 0)
				return 0;

			var attr = attrs[0] as DynamicDataStringLengthAttribute;
			if (attr == null)
				return 0;

			return attr.MaxLength;
		}

		PropertyInfo GetPropertyInfo (Type type, string name)
		{
			try {
				PropertyInfo ret = type.GetProperties (BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy).
					First<PropertyInfo> ((pi) => {
						if (String.Compare (pi.Name, name, StringComparison.Ordinal) == 0)
							return true;
						return false;
					}
				);
				return ret;
			} catch (InvalidOperationException ex) {
				return null;
			}
		}
	}
}
