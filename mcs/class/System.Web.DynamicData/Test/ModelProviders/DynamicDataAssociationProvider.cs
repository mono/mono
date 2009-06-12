using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.DynamicData;
using System.Web.DynamicData.ModelProviders;

using MonoTests.DataSource;

namespace MonoTests.ModelProviders
{
	class DynamicDataAssociationProvider : AssociationProvider
	{
		public DynamicDataAssociationProvider (AssociationDirection direction, ColumnProvider owner, ColumnProvider to)
		{
			this.Direction = direction;
			this.IsPrimaryKeyInThisTable = owner.IsPrimaryKey;
			this.FromColumn = owner;
			this.ToTable = to.Table;
		}
	}
}
