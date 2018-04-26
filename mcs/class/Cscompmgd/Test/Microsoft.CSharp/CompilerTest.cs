//
// CompilerTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C) Ximian, Inc.
//

using System;
using System.Collections.Generic;
using Microsoft.CSharp;

using NUnit.Framework;

namespace MonoTests.Cscompmgd
{
	[TestFixture]
	public class CompilerTest
	{
		[SetUp]
		public void GetReady ()
		{
		}

		[TestCase]
		public void EmptySourceTexts ()
		{
			Assert.Throws<IndexOutOfRangeException> (() => Compiler.Compile (Array.Empty<string> (), Array.Empty<string> (), "", null, null));
		}
	}
}
