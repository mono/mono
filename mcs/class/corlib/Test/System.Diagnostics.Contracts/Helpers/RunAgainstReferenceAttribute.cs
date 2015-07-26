
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace MonoTests.System.Diagnostics.Contracts.Helpers {

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	class RunAgainstReferenceAttribute : CategoryAttribute
	{
	}

}

