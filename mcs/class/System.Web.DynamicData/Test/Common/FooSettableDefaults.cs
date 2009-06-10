using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonoTests.Common
{
	class FooSettableDefaults
	{
		public string Column1 { get; set; }
		public int Column2 { get; set; }
		public string PrimaryKeyColumn1 { get; set; }
		public string PrimaryKeyColumn2 { get; set; }
		public string PrimaryKeyColumn3 { get; set; }

		public FooSettableDefaults ()
			: this ("primary one", "primary two", "primary three")
		{
		}

		public FooSettableDefaults (string p1, string p2, string p3)
		{
			Column1 = "hello";
			Column2 = 123;
			PrimaryKeyColumn1 = p1;
			PrimaryKeyColumn2 = p2;
			PrimaryKeyColumn3 = p3;
		}
	}
}
