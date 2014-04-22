//
// BuildParametersTest.cs
//
// Author:
//   Atsushi Enomoto (atsushi@xamarin.com)
//
// Copyright (C) 2013 Xamarin Inc. (http://www.xamarin.com)
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
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using NUnit.Framework;

namespace MonoTests.Microsoft.Build.Execution
{
	[TestFixture]
	public class BuildParametersTest
	{
		[Test]
		public void GetToolset ()
		{
			var bp = new BuildParameters (ProjectCollection.GlobalProjectCollection);
			Assert.IsNull (bp.GetToolset ("0.1"), "#1");
			var ts = bp.GetToolset ("2.0");
			// They are equal
			Assert.AreEqual (ProjectCollection.GlobalProjectCollection.Toolsets.First (t => t.ToolsVersion == "2.0"), ts, "#2");

			bp = new BuildParameters ();
			Assert.IsNull (bp.GetToolset ("0.1"), "#1");
			ts = bp.GetToolset ("2.0");
			// They are NOT equal, because ProjectCollection seems to be different.
			Assert.AreNotEqual (ProjectCollection.GlobalProjectCollection.Toolsets.First (t => t.ToolsVersion == "2.0"), ts, "#2");			
		}
		
		[Test]
		public void PropertiesDefault ()
		{
			var bp = new BuildParameters ();
			Assert.IsTrue (bp.EnableNodeReuse, "#1");
			Assert.IsTrue (bp.EnvironmentProperties.Count > 0, "#2");
			Assert.AreEqual (CultureInfo.CurrentCulture, bp.Culture, "#3");
		}
	}
}
