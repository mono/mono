using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MonoTests.System.Web.DynamicData;
using MonoTests.ModelProviders;
using MonoTests.DataSource;

namespace MonoTests.Common
{
	interface ITestDataContext
	{
		List <DynamicDataTable> GetTables ();
	}
}
