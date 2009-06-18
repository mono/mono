using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using MonoTests.DataSource;
using MonoTests.ModelProviders;

namespace MonoTests.Common
{
	public class TestDataColumn<DataType> : DynamicDataColumn
	{
		public TestDataColumn (MemberInfo member)
		{
			if (member == null)
				throw new ArgumentNullException ("member");

			string name = this.Name = member.Name;
			switch (member.MemberType) {
				case MemberTypes.Field:
					var fi = member as FieldInfo;
					this.DataType = fi.FieldType;
					break;

				case MemberTypes.Property:
					var pi = member as PropertyInfo;
					this.DataType = pi.PropertyType;
					break;

				default:
					throw new ArgumentException ("Member information must refer to either a field or a property.", "member");
			}

			this.PrimaryKey = name.StartsWith ("PrimaryKeyColumn", StringComparison.Ordinal);
			this.CustomProperty = name.StartsWith ("CustomProperty", StringComparison.Ordinal);
			this.Generated = name.StartsWith ("GeneratedColumn", StringComparison.Ordinal);
			
			object[] attrs = member.GetCustomAttributes (true);
			DynamicDataAssociationAttribute associationAttr;

			try {
				associationAttr = attrs.OfType<DynamicDataAssociationAttribute> ().First<DynamicDataAssociationAttribute> ();
			} catch (InvalidOperationException) {
				associationAttr = null;
			}

			if (associationAttr != null) {
				AssociatedTo = associationAttr.ColumnName;
				AssociationDirection = associationAttr.Direction;
			}

			DynamicDataSortableAttribute sortableAttr;

			try {
				sortableAttr = attrs.OfType<DynamicDataSortableAttribute> ().First<DynamicDataSortableAttribute> ();
			} catch (InvalidOperationException) {
				sortableAttr = null;
			}

			if (sortableAttr != null)
				Sortable = sortableAttr.Sortable;
		}
	}
}
