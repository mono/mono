using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MonoTests.Common
{
	// Parameters: display column, sort column, whether sort is descending
	[DisplayColumn ("NoSuchColumn", "NoSuchColumn", false)]
	class FooInvalidDisplayColumnAttribute
	{
		public string Column1 { get; set; }
		public int Column2 { get; set; }
		public string PrimaryKeyColumn1 { get; set; }
		public int PrimaryKeyColumn2 { get; set; }
		public bool PrimaryKeyColumn3 { get; set; }

		public FooInvalidDisplayColumnAttribute ()
		{
			Column1 = "hello";
			Column2 = 123;
			PrimaryKeyColumn1 = "primary key value";
			PrimaryKeyColumn2 = 456;
			PrimaryKeyColumn3 = true;
		}
	}
}
