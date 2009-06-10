using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonoTests.Common
{
	class FooNoPrimaryColumns
	{
		public string Column1 { get; set; }
		public int Column2 { get; set; }

		public FooNoPrimaryColumns ()
		{
			Column1 = "hello";
			Column2 = 123;
		}
	}
}
