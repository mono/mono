using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonoTests.Common
{
	class BazNoStringsNoPrimary
	{
		// DO NOT change the order of properties - tests depend on it
		public int Column1 { get; set; }
		public int Column2 { get; set; }

		public BazNoStringsNoPrimary ()
		{
			Column1 = 123;
			Column2 = 456;
		}
	}
}
