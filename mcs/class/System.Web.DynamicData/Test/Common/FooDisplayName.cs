using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace MonoTests.Common
{
	[DisplayName ("My name is FooDisplayName, and I am friendly")]
	class FooDisplayName
	{
		public string Column1 { get; set; }
		public int Column2 { get; set; }
		public string PrimaryKeyColumn1 { get; set; }
		public int PrimaryKeyColumn2 { get; set; }
		public bool PrimaryKeyColumn3 { get; set; }

		public FooDisplayName ()
		{
			Column1 = "hello";
			Column2 = 123;
			PrimaryKeyColumn1 = "primary key value";
			PrimaryKeyColumn2 = 456;
			PrimaryKeyColumn3 = true;
		}
	}
}
