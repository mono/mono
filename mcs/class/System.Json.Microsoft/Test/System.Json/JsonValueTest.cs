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
		// Test that we correctly serialize JsonArray with null elements.
		[Test]
		public void ToStringOnJsonArrayWithNulls () {
			var j = JsonValue.Load (new StringReader ("[1,2,3,null]"));
			Assert.AreEqual (4, j.Count, "itemcount");
			Assert.AreEqual (JsonType.Array, j.JsonType, "type");
			var str = j.ToString ();
			Assert.AreEqual ("[1,2,3,null]", str);
		}
	}
}
