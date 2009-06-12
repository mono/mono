using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace MonoTests.Common
{
	[DisplayColumn ("NoSuchColumn", "")]
	class FooEmptySortColumn
	{
	}
}
