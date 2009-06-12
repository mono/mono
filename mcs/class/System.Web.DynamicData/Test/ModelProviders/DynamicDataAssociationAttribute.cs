using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.DynamicData.ModelProviders;

namespace MonoTests.ModelProviders
{
	[AttributeUsage (AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
	class DynamicDataAssociationAttribute : Attribute
	{
		public string ColumnName { get; private set; }
		public AssociationDirection Direction { get; private set; }

		public DynamicDataAssociationAttribute (string columnName, AssociationDirection direction)
		{
			this.ColumnName = columnName;
			this.Direction = direction;
		}
	}
}
