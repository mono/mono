//
// StringInfoTest.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
//

using NUnit.Framework;
using System;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace MonoTests.System.Globalization
{

	[TestFixture]
	public class StringInfoTest
	{
		[Test]
		public void GetNextTextElement ()
		{
			Assert.AreEqual ("A", StringInfo.GetNextTextElement ("ABC", 0), "#1");
			Assert.AreEqual ("C", StringInfo.GetNextTextElement ("ABC", 2), "#2");
			Assert.AreEqual ("A\u0330", StringInfo.GetNextTextElement ("A\u0330BC", 0), "#3");
			Assert.AreEqual ("B", StringInfo.GetNextTextElement ("A\u0330BC", 2), "#4");

			// hmm ...
#if NET_2_0 // it causes ArgumentOutOfRangeException in 1.x, not worthy to test it anymore
			Assert.AreEqual (String.Empty, StringInfo.GetNextTextElement ("A\u0330BC", 4), "#4");
#endif
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void GetNextTextElementOutOfRange1 ()
		{
			StringInfo.GetNextTextElement ("ABC", -1);
		}

#if NET_2_0
		[Test]
		public void LengthInTextElements ()
		{
			Assert.AreEqual (3, new StringInfo ("ABC").LengthInTextElements, "#1");
			Assert.AreEqual (5, new StringInfo (" ABC ").LengthInTextElements, "#2");
			Assert.AreEqual (3, new StringInfo ("A\u0330BC\u0330").LengthInTextElements, "#3");
			Assert.AreEqual (3, new StringInfo ("A\u0330\u0331BC\u0330").LengthInTextElements, "#4");
		}

		[Test]
		public void SubstringByTextElements ()
		{
			StringInfo si = new StringInfo ("A\u0330BC\u0330");
			Assert.AreEqual ("A\u0330BC\u0330", si.SubstringByTextElements (0), "#1");
			Assert.AreEqual ("BC\u0330", si.SubstringByTextElements (1), "#2");
			Assert.AreEqual ("C\u0330", si.SubstringByTextElements (2), "#3");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void SubstringByTextElementsOutOfRange1 ()
		{
			new StringInfo ("A\u0330BC\u0330").SubstringByTextElements (-1);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void SubstringByTextElementsOutOfRange2 ()
		{
			new StringInfo ("A\u0330BC\u0330").SubstringByTextElements (4);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void SubstringByTextElementsOutOfRange3 ()
		{
			new StringInfo (String.Empty).SubstringByTextElements (0);
		}
#endif
	}

}
