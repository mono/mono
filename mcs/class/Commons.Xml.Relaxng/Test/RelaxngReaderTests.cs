//
// RelaxngReaderTests.cs
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
using NUnit.Framework;

namespace MonoTests.Commons.Xml.Relaxng
{
	[TestFixture]
	public class RelaxngReaderTests
	{
		RelaxngReader reader;

		[SetUp]
		public void SetUp ()
		{
		}
		
		private void loadGrammarFromUrl (string url)
		{
			reader = new RelaxngReader (new XmlTextReader (url));
		}
		
		[Test]
		public void SimpleRead ()
		{
			loadGrammarFromUrl ("XmlFiles/SimpleElementPattern1.rng");
			RngPattern p = reader.ReadPattern ();

			Assertion.AssertEquals (RngPatternType.Element, p.PatternType);
		}

/*
		[Test]
		public void ValidateRelaxngGrammar ()
		{
			loadGrammarFromUrl ("XmlFiles/relaxng.rng");
			RngPattern p = reader.ReadPattern ();

			Assertion.AssertEquals (RngPatternType.Grammar, p.PatternType);
		}
*/

	}
}
