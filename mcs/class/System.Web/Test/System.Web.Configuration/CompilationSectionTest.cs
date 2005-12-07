//
// CompilationSectionTest.cs 
//	- unit tests for System.Web.Configuration.CompilationSection
//
// Author:
//	Chris Toshok  <toshok@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using NUnit.Framework;

using System;
using System.Configuration;
using System.Web.Configuration;
using System.Web;
using System.Web.Security;

namespace MonoTests.System.Web.Configuration {

	[TestFixture]
	public class CompilationSectionTest  {

		[Test]
		public void Defaults ()
		{
			CompilationSection c = new CompilationSection ();

			Assert.IsNotNull (c.Assemblies, "A1");

			Assert.AreEqual ("", c.AssemblyPostProcessorType, "A2");
			Assert.IsTrue   (c.Batch, "A3");
			Assert.AreEqual (TimeSpan.FromMinutes (15), c.BatchTimeout, "A4");

			Assert.IsNotNull (c.BuildProviders, "A5");
			Assert.IsNotNull (c.CodeSubDirectories, "A6");
			Assert.IsNotNull (c.Compilers, "A7");

			Assert.IsFalse (c.Debug, "A8");
			Assert.AreEqual ("vb", c.DefaultLanguage, "A9");
			Assert.IsTrue (c.Explicit, "A10");
			
			Assert.IsNotNull (c.ExpressionBuilders, "A11");
			Assert.AreEqual (1000, c.MaxBatchSize, "A12");
			Assert.AreEqual (15, c.NumRecompilesBeforeAppRestart, "A13");
			Assert.IsFalse  (c.Strict, "A14");
			Assert.AreEqual ("", c.TempDirectory, "A15");
			Assert.IsFalse (c.UrlLinePragmas, "A16");
		}
	}
}

#endif
