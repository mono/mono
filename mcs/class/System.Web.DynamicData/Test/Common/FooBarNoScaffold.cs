using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web.DynamicData.ModelProviders;

using MonoTests.ModelProviders;

namespace MonoTests.Common
{
	[ScaffoldTable (false)]
	class FooBarNoScaffold
	{
		public string Column1 { get; set; }

		public string CustomPropertyColumn1 { get; set; }

		[UIHint ("UI Hint")]
		public string CustomPropertyColumn2 { get; set; }

		public int GeneratedColumn1 { get; set; }

		[UIHint ("UI Hint")]
		public int GeneratedColumn2 { get; set; }

		[DynamicDataAssociation ("AssociatedBarTable.Column1", AssociationDirection.OneToOne)]
		public string ForeignKeyColumn1 { get; set; }
	}
}
