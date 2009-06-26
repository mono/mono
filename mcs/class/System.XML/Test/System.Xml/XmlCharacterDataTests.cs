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
			Assert.IsTrue (changed);
			Assert.IsTrue (changing);
			Assert.AreEqual ("foobar", comment.Data);

			comment.Value = "foo";
			comment.AppendData (null);
			Assert.AreEqual ("foo", comment.Data);
		}

		[Test]
		public void DeleteData ()
		{
			comment.Value = "bar";
			changed = false;
			changing = false;
			comment.DeleteData (1, 1);
			Assert.IsTrue (changed);
			Assert.IsTrue (changing);
			Assert.AreEqual ("br", comment.Data);

			try 
			{
				comment.Value = "foo";
				comment.DeleteData(-1, 1);
				Assert.Fail ("Expected an ArgumentOutOfRangeException to be thrown.");
			} 
			catch (ArgumentOutOfRangeException) {}

			comment.Value = "foo";
			comment.DeleteData(1, 5);
			Assert.AreEqual ("f", comment.Data);

			comment.Value = "foo";
			comment.DeleteData(3, 10);
			Assert.AreEqual ("foo", comment.Data);
		}

		[Test]
#if NET_2_0
		[Category ("NotDotNet")] // enbug in 2.0
#endif
		public void InsertData ()
		{
			comment.Value = "foobaz";
			changed = false;
			changing = false;
			comment.InsertData (3, "bar");
			Assert.IsTrue (changed);
			Assert.IsTrue (changing);
			Assert.AreEqual ("foobarbaz", comment.Data);

			try 
			{
				comment.Value = "foo";
				comment.InsertData (-1, "bar");
				Assert.Fail ("Expected an ArgumentOutOfRangeException to be thrown.");
			} 
			catch (ArgumentOutOfRangeException) {}

			comment.Value = "foo";
			comment.InsertData (3, "bar");
			Assert.AreEqual ("foobar", comment.Data);

			try 
			{
				comment.Value = "foo";
				comment.InsertData (4, "bar");
				Assert.Fail ("Expected an ArgumentOutOfRangeException to be thrown.");
			} 
			catch (ArgumentOutOfRangeException) {}

			try 
			{
				comment.Value = "foo";
				comment.InsertData (1, null);
				Assert.Fail ("Expected an ArgumentNullException to be thrown.");
			} 
			catch (ArgumentNullException) {}
		}

		[Test]
#if NET_2_0
		[Category ("NotDotNet")] // enbug in 2.0
#endif
		public void ReplaceData ()
		{
			changed = false;
			changing = false;
			comment.ReplaceData (0, 3, "bar");
			Assert.IsTrue (changed);
			Assert.IsTrue (changing);
			Assert.AreEqual ("bar", comment.Data);

			comment.Value = "foo";
			comment.ReplaceData (2, 3, "bar");
			Assert.AreEqual ("fobar", comment.Data);

			comment.Value = "foo";
			comment.ReplaceData (3, 3, "bar");
			Assert.AreEqual ("foobar", comment.Data);

			try 
			{
				comment.Value = "foo";
				comment.ReplaceData (4, 3, "bar");
				Assert.Fail ("Expected an ArgumentOutOfRangeException to be thrown.");
			} 
			catch (ArgumentOutOfRangeException) {}

			try 
			{
				comment.Value = "foo";
				comment.ReplaceData (-1, 3, "bar");
				Assert.Fail ("Expected an ArgumentOutOfRangeException to be thrown.");
			} 
			catch (ArgumentOutOfRangeException) {}

			comment.Value = "foo";
			comment.ReplaceData (0, 2, "bar");
			Assert.AreEqual ("baro", comment.Data);

			comment.Value = "foo";
			comment.ReplaceData (0, 5, "bar");
			Assert.AreEqual ("bar", comment.Data);

			try 
			{
				comment.Value = "foo";
				comment.ReplaceData (1, 1, null);
				Assert.Fail ("Expected an ArgumentNullException to be thrown.");
			} 
			catch (ArgumentNullException) {}
		}

		[Test]
		public void Substring ()
		{
			comment.Value = "test string";
			Assert.AreEqual (comment.Substring (0, 50), "test string");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void SubstringStartOutOfRange ()
		{
			comment.Value = "test string";
			comment.Substring (-5, 10);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void SubstringCountOutOfRange ()
		{
			comment.Value = "test string";
			comment.Substring (10, -5);
		}
	}
}
