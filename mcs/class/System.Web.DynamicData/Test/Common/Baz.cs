using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonoTests.Common
{
	class Baz
	{
		// DO NOT change the order of properties - tests depend on it
		public int Column1 { get; set; }
		public int PrimaryKeyColumn1 { get; set; }
		public string PrimaryKeyColumn2 { get; set; }
		public bool PrimaryKeyColumn3 { get; set; }

		public Baz ()
		{
			Column1 = 123;
			PrimaryKeyColumn1 = 456;
			PrimaryKeyColumn2 = "primary key value";
			PrimaryKeyColumn3 = true;
		}
	}
}
