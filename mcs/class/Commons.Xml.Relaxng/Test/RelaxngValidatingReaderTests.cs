//
// RelaxngValidatingReaderTests.cs
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
	public class RelaxngValidatingReaderTests
	{
		RelaxngValidatingReader reader;

		[SetUp]
		public void SetUp ()
		{
		}
		
		private void SetupReaderFromUrl (string instanceUrl, string grammarUrl)
		{
			reader = new RelaxngValidatingReader (
				new XmlTextReader (instanceUrl),
				new XmlTextReader (grammarUrl));
		}

		private void SetupReader (string instance, string grammar)
		{
			reader = new RelaxngValidatingReader (
				new XmlTextReader (new StringReader (instance)),
				new XmlTextReader (new StringReader (grammar)));
		}

		[Test]
		public void SimpleElementPattern1 ()
		{
			SetupReaderFromUrl ("XmlFiles/SimpleElementPattern1.xml",
				"XmlFiles/SimpleElementPattern1.rng");

			while (!reader.EOF)
				reader.Read ();
		}

		[Test]
		public void SimpleElementPattern2 ()
		{
			SetupReaderFromUrl ("XmlFiles/SimpleElementPattern2.xml",
				"XmlFiles/SimpleElementPattern2.rng");

			while (!reader.EOF)
				reader.Read ();
		}

		[Test]
		public void ReadPracticalSample1 ()
		{
			SetupReaderFromUrl ("XmlFiles/team.xml", "XmlFiles/team.rng");
			while (!reader.EOF)
				reader.Read ();
		}

/*
		[Test]
		public void ReadPracticalSample2 ()
		{
			SetupReaderFromUrl ("XmlFiles/team.rng", "XmlFiles/relaxng.rng");
			while (!reader.EOF)
				reader.Read ();
		}
*/
	}
}
