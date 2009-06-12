using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Linq.Mapping;
using System.Text;

namespace MonoTests.Common
{
	class FooWithDefaults
	{
		public string Column1 { get; set; }
		public int Column2 { get; set; }

		public string PrimaryKeyColumn1 { get; set; }
		public int PrimaryKeyColumn2 { get; set; }
		public bool PrimaryKeyColumn3 { get; set; }

		public string ForeignKeyColumn1 { get; set; }
		public int ForeignKeyColumn2 { get; set; }
		public bool ForeignKeyColumn3 { get; set; }

		public FooWithDefaults ()
		{
			Column1 = "hello";
			Column2 = 123;

			PrimaryKeyColumn1 = "primary key value";
			PrimaryKeyColumn2 = 456;
			PrimaryKeyColumn3 = true;

			ForeignKeyColumn1 = "foreign key";
			ForeignKeyColumn2 = 789;
			ForeignKeyColumn3 = true;
		}
	}
}
