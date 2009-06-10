using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonoTests.Common
{
	class FooNoDefaultsWithPrimaryKey
	{
		public string Column1 { get; set; }
		public int Column2 { get; set; }
		public string PrimaryKeyColumn1 { get; set; }

		public FooNoDefaultsWithPrimaryKey ()
		{
		}
	}
}
