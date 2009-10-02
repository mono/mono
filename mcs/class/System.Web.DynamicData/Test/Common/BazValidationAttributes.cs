using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace MonoTests.Common
{
	public class BazValidationAttributes
	{
		[Range (-5, 5)]
		public int Column1 { get; set; }
	}
}
