// IPHostEntryTest.cs - NUnit Test Cases for the System.Net.IPHostEntry class
//
// Author: Mads Pultz (mpultz@diku.dk)
//
// (C) Mads Pultz, 2001

using NUnit.Framework;
using System;

public class IPHostEntryTest: TestCase {
	
	public IPHostEntryTest(String name): base(name) {}

	public static ITest Suite {
		get { return new TestSuite(typeof(IPHostEntryTest)); }
	}
} 
