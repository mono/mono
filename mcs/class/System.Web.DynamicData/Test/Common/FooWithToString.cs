using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonoTests.Common
{
	class FooWithToString
	{
		public string Column1 { get; set; }

		public FooWithToString ()
		{
			Column1 = "hello";
		}

		public override string  ToString()
		{
			return "ValueFrom_ToString";
		}
	}
}
