using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.DynamicData.ModelProviders;

namespace MonoTests.ModelProviders
{
	[AttributeUsage (AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
	class DynamicDataSortableAttribute : Attribute
	{
		public bool Sortable { get; private set; }

		public DynamicDataSortableAttribute (bool sortable)
		{
			Sortable = sortable;
		}
	}
}
