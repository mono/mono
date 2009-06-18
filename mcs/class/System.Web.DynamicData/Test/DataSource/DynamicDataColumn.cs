using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.DynamicData.ModelProviders;

namespace MonoTests.DataSource
{
	public abstract class DynamicDataColumn
	{
		public Type DataType { get; protected set; }
		public string Name { get; protected set; }
		public bool PrimaryKey { get; protected set; }
		public string AssociatedTo { get; protected set; }
		public AssociationDirection AssociationDirection { get; protected set; }
		public bool CustomProperty { get; protected set; }
		public bool Generated { get; protected set; }
		public bool Sortable { get; protected set; }
	}
}
