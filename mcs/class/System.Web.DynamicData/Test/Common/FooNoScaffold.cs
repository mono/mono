using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace MonoTests.Common
{
	[ScaffoldTable(false)]
	class FooNoScaffold
	{
		public string Column1 { get; set; }
	}
}
