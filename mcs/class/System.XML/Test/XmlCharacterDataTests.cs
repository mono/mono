//
// System.Xml.XmlTextWriterTests
//
// Author: Kral Ferch <kral_ferch@hotmail.com>
// Author: Martin Willemoes Hansen <mwh@sysrq.dk>
//
// (C) 2002 Kral Ferch
// (C) 2003 Martin Willemoes Hansen
//

using System;
using System.Xml;

using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlCharacterDataTests
	{
		XmlDocument document;
		XmlComment comment;
		bool changed;
		bool changing;

		[SetUp]
		public void GetReady ()
		{
			document = new XmlDocument ();
			document.NodeChanged += new XmlNodeChangedEventHandler (this.EventNodeChanged);
			document.NodeChanging += new XmlNodeChangedEventHandler (this.EventNodeChanging);
			comment = document.CreateComment ("foo");
		}

		private void EventNodeChanged(Object sender, XmlNodeChangedEventArgs e)
		{
			changed = true;
		}

		private void EventNodeChanging (Object sender, XmlNodeChangedEventArgs e)
		{
			changing = true;
		}

		[Test]
		public void AppendData ()
		{
			changed = false;
			changing = false;
			comment.AppendData ("bar");
			Assertion.Assert (changed);
			Assertion.Assert (changing);
			Assertion.AssertEquals ("foobar", comment.Data);

			comment.Value = "foo";
			comment.AppendData (null);
			Assertion.AssertEquals ("foo", comment.Data);
		}

		[Test]
		public void DeleteData ()
		{
			comment.Value = "bar";
			changed = false;
			changing = false;
			comment.DeleteData (1, 1);
			Assertion.Assert (changed);
			Assertion.Assert (changing);
			Assertion.AssertEquals ("br", comment.Data);

			try 
			{
				comment.Value = "foo";
				comment.DeleteData(-1, 1);
				Assertion.Fail ("Expected an ArgumentOutOfRangeException to be thrown.");
			} 
			catch (ArgumentOutOfRangeException) {}

			comment.Value = "foo";
			comment.DeleteData(1, 5);
			Assertion.AssertEquals("f", comment.Data);

			comment.Value = "foo";
			comment.DeleteData(3, 10);
			Assertion.AssertEquals("foo", comment.Data);
		}

		[Test]
		public void InsertData ()
		{
			comment.Value = "foobaz";
			changed = false;
			changing = false;
			comment.InsertData (3, "bar");
			Assertion.Assert (changed);
			Assertion.Assert (changing);
			Assertion.AssertEquals ("foobarbaz", comment.Data);

			try 
			{
				comment.Value = "foo";
				comment.InsertData (-1, "bar");
				Assertion.Fail ("Expected an ArgumentOutOfRangeException to be thrown.");
			} 
			catch (ArgumentOutOfRangeException) {}

			comment.Value = "foo";
			comment.InsertData (3, "bar");
			Assertion.AssertEquals ("foobar", comment.Data);

			try 
			{
				comment.Value = "foo";
				comment.InsertData (4, "bar");
				Assertion.Fail ("Expected an ArgumentOutOfRangeException to be thrown.");
			} 
			catch (ArgumentOutOfRangeException) {}

			try 
			{
				comment.Value = "foo";
				comment.InsertData (1, null);
				Assertion.Fail ("Expected an ArgumentNullException to be thrown.");
			} 
			catch (ArgumentNullException) {}
		}

		[Test]
		public void ReplaceData ()
		{
			changed = false;
			changing = false;
			comment.ReplaceData (0, 3, "bar");
			Assertion.Assert (changed);
			Assertion.Assert (changing);
			Assertion.AssertEquals ("bar", comment.Data);

			comment.Value = "foo";
			comment.ReplaceData (2, 3, "bar");
			Assertion.AssertEquals ("fobar", comment.Data);

			comment.Value = "foo";
			comment.ReplaceData (3, 3, "bar");
			Assertion.AssertEquals ("foobar", comment.Data);

			try 
			{
				comment.Value = "foo";
				comment.ReplaceData (4, 3, "bar");
				Assertion.Fail ("Expected an ArgumentOutOfRangeException to be thrown.");
			} 
			catch (ArgumentOutOfRangeException) {}

			try 
			{
				comment.Value = "foo";
				comment.ReplaceData (-1, 3, "bar");
				Assertion.Fail ("Expected an ArgumentOutOfRangeException to be thrown.");
			} 
			catch (ArgumentOutOfRangeException) {}

			comment.Value = "foo";
			comment.ReplaceData (0, 2, "bar");
			Assertion.AssertEquals ("baro", comment.Data);

			comment.Value = "foo";
			comment.ReplaceData (0, 5, "bar");
			Assertion.AssertEquals ("bar", comment.Data);

			try 
			{
				comment.Value = "foo";
				comment.ReplaceData (1, 1, null);
				Assertion.Fail ("Expected an ArgumentNullException to be thrown.");
			} 
			catch (ArgumentNullException) {}
		}
	}
}
