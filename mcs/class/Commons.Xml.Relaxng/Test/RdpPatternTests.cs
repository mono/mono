//
// RdpPatternTests.cs
//
// Authors:
//   Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// (C) 2003 Atsushi Enomoto
//

using System;
using System.IO;
using System.Xml;
using Commons.Xml.Relaxng;
using Commons.Xml.Relaxng.Derivative;
using NUnit.Framework;

namespace MonoTests.Commons.Xml.Relaxng
{
	[TestFixture]
	public class RdpPatternTests
	{
		RelaxngValidatingReader reader;
		RdpPattern result;
		RdpPattern pattern1;

		[SetUp]
		public void SetUp ()
		{
			pattern1 = new RdpElement (new RdpName ("foo", "urn:foo"), RdpEmpty.Instance);
		}
		
		private void AssertPattern (string s, RngPatternType expected, RdpPattern p)
		{
			Assertion.AssertEquals (s, expected, p.PatternType);
		}

		[Test]
		public void ElementStartTagOpenDeriv ()
		{
			result = pattern1.StartTagOpenDeriv ("bar", "urn:foo");
			AssertPattern ("#element.start.1", RngPatternType.NotAllowed, result);

			result = pattern1.StartTagOpenDeriv ("foo", "urn:bar");
			AssertPattern ("#element.start.2", RngPatternType.NotAllowed, result);

			result = pattern1.StartTagOpenDeriv ("foo", "urn:foo");
			AssertPattern ("#element.start.3", RngPatternType.After, result);
			RdpAfter after= result as RdpAfter;
			AssertPattern ("#element.start.4", RngPatternType.Empty, after.LValue);
			AssertPattern ("#element.start.5", RngPatternType.Empty, after.RValue);
		}

	}
}
