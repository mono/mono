//
// JsonValueTest.cs: Tests for JSonValue
//
// Copyright 2011 Xamarin, Inc.
//
// Authors:
//   Miguel de Icaza
//
using NUnit.Framework;
using System;
using System.IO;
using System.Text;
using System.Json;

namespace MonoTests.System
{
	[TestFixture]
	public class JsonValueTests {
		// Tests that a trailing comma is allowed in dictionary definitions
		[Test]
		public void LoadWithTrailingComma ()
		{
			var j = JsonValue.Load (new StringReader ("{ \"a\": \"b\",}"));
			Assert.AreEqual (1, j.Count, "itemcount");
			Assert.AreEqual (JsonType.String, j ["a"].JsonType, "type");
			Assert.AreEqual ("b", (string) j ["a"], "value");
		}
	}
}
