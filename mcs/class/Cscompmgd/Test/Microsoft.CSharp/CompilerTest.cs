//
// CompilerTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C) Ximian, Inc.
//

using System;
using Microsoft.CSharp;

using NUnit.Framework;

namespace MonoTests.Cscompmgd
{
	[TestFixture]
	public class CompilerTest : Assertion
	{
		Compiler compiler;

		[SetUp]
		public void GetReady ()
		{
		}

		[Test]
		public void constructor ()
		{
			compiler = new Compiler ();
		}

	}
}
