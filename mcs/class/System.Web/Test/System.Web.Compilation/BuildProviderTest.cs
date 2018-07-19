﻿//
// RouteValueExpressionBuilder.cs
//
// Authors:
//      Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2010 Novell, Inc. (http://novell.com/)
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
using System.CodeDom;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Compilation;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Routing;

using NUnit.Framework;

using MonoTests.SystemWeb.Framework;
using MonoTests.stand_alone.WebHarness;

namespace MonoTests.System.Web.Compilation
{
	[TestFixture]
	public class BuildProviderTest
	{
		[Test]
		public void RegisterBuildProvider ()
		{
			Assert.Throws<ArgumentException> (() => {
				BuildProvider.RegisterBuildProvider (null, typeof (FooBuildProvider));
			}, "#A1-1");

			Assert.Throws<ArgumentException> (() => {
				BuildProvider.RegisterBuildProvider (String.Empty, typeof (FooBuildProvider));
			}, "#A1-2");

			Assert.Throws<ArgumentNullException> (() => {
				BuildProvider.RegisterBuildProvider (".foo", null);
			}, "#A1-3");

			Assert.Throws<ArgumentException> (() => {
				BuildProvider.RegisterBuildProvider (".foo", typeof (string));
			}, "#A1-4");

			// This would have worked if we called the method during the pre-application start stage
			// (we have a test for this in the standalone test suite)
			Assert.Throws<InvalidOperationException> (() => {
				BuildProvider.RegisterBuildProvider (".foo", typeof (BuildProvider));
			}, "#A1-5");
		}
	}

	class FooBuildProvider : BuildProvider
	{
	}
}
