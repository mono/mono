using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.DynamicData.ModelProviders;

using MonoTests.ModelProviders;

namespace MonoTests.Common
{
	class AssociatedFoo
	{
		[DynamicDataAssociation ("AssociatedBarTable.Column1", AssociationDirection.OneToOne)]
		public string ForeignKeyColumn1 { get; set; }

		[DynamicDataAssociation ("AssociatedBarTable.Column3", AssociationDirection.ManyToOne)]
		public string PrimaryKeyColumn1 { get; set; }

		[DynamicDataAssociation ("AssociatedBarTable.Column2", AssociationDirection.OneToMany)]
		public int PrimarykeyColumn2 { get; set; }

		public AssociatedFoo ()
		{
		}
	}
}
