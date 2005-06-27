//
// MonoTests.MS.Web.Services.Timestamp.AllTests.cs
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using Microsoft.Web.Services.Timestamp;
using NUnit.Framework;

// note: due to compiler confusion between classes and namespace (like Timestamp)
// I renamed the test namespace from "MonoTests.Microsoft.Web.Services.Timestamp"
// to "MonoTests.MS.Web.Services.Timestamp".
namespace MonoTests.MS.Web.Services.Timestamp {

	public class AllTests {

		// some missing overloads for the tests

		public static void AssertEquals (string message, DateTime expected, DateTime actual) 
		{
			// Format before compare (Timestamp doesn't have a good resolution)
			Assertion.Assert (message, (expected.ToString (WSTimestamp.TimeFormat) == actual.ToString (WSTimestamp.TimeFormat)));
		}

		public static void AssertEquals (string message, long expected, long actual) 
		{
			Assertion.Assert (message, (expected == actual));
		}
	}
}
