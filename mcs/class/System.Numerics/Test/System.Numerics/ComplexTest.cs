// ComplexTest.cs
//
// Authors:
//   Christoph Ruegg <git@cdrnet.ch>
//
// Copyright (C) 2013 Novell, Inc (http://www.novell.com)
//

using System;
using System.Numerics;
using System.Globalization;
using NUnit.Framework;

namespace MonoTests.System.Numerics
{
	[TestFixture]
	public class ComplexTest
	{
		[Test]
		public void TestToStringFormats ()
		{
			Assert.AreEqual ("(1, 2)", new Complex (1, 2).ToString (), "#1");
			Assert.AreEqual ("(1, 2)", new Complex (1, 2).ToString ("G"), "#2");
			Assert.AreEqual ("(1, 2)", new Complex (1, 2).ToString ((string)null), "#3");

			IFormatProvider provider = CultureInfo.InvariantCulture;
			Assert.AreEqual ("(1, 2)", new Complex (1, 2).ToString (provider), "#4");
			Assert.AreEqual ("(1, 2)", new Complex (1, 2).ToString ("G", provider), "#5");
			Assert.AreEqual ("(1, 2)", new Complex (1, 2).ToString ((string)null, provider), "#6");
		}
	}
}
