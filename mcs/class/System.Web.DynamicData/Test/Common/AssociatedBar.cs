using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonoTests.Common
{
	public class AssociatedBar
	{
		public string Column1 { get; set; }
		public int Column2 { get; set; }
		public string Column3 { get; set; }
		public int Column4 { get; set; }

		public AssociatedBar ()
		{
			Column1 = "Column 1";
			Column2 = 123;
			Column3 = "Column 3";
		}
	}
}
