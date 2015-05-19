//
// OutputCacheParametersTest.cs
//	- Unit tests for System.Web.UI.OutputCacheParameters
//
// Author:
//	Noam Lampert  (noaml@mainsoft.com)
//
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//


using System;
using System.IO;
using System.Web.UI;
using NUnit.Framework;
using System.Collections;

namespace MonoTests.System.Web.UI
{
	[TestFixture]
	public class OutputCacheParametersTest
	{
		[Test]
		public void InitialValues () {
			OutputCacheParameters o = new OutputCacheParameters ();
			Assert.IsNull (o.CacheProfile, "CacheProfile");
			Assert.IsTrue (o.Duration == 0, "Duration");
			Assert.IsTrue (o.Enabled, "Enabled");
			Assert.IsTrue (o.Location == OutputCacheLocation.Any, "OutputCacheLocation");
			Assert.IsFalse (o.NoStore, "NoStore");
			Assert.IsNull (o.SqlDependency, "SqlDependency");
			Assert.IsNull (o.VaryByControl, "VaryByControl");
			Assert.IsNull (o.VaryByCustom, "VaryByCustom");
			Assert.IsNull (o.VaryByHeader, "VaryByHeader");
			Assert.IsNull (o.VaryByParam, "VaryByParam");
		}
	}
}

