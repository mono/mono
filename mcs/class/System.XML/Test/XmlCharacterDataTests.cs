//
// System.Xml.XmlTextWriterTests
//
// Author:
//   Kral Ferch <kral_ferch@hotmail.com>
//
// (C) 2002 Kral Ferch
//

using System;
using System.Xml;

using NUnit.Framework;

namespace MonoTests.System.Xml
{
	public class XmlCharacterDataTests : TestCase
	{
		public XmlCharacterDataTests () : base ("MonoTests.System.Xml.XmlCharacterDataTests testsuite") {}
		public XmlCharacterDataTests (string name) : base (name) {}

		XmlDocument document;
		XmlComment comment;
		bool changed;
		bool changing;

		protected override void SetUp ()
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

		public void TestAppendData ()
		{
			changed = false;
			changing = false;
			comment.AppendData ("bar");
			Assert (changed);
			Assert (changing);
			AssertEquals ("foobar", comment.Data);

			comment.Value = "foo";
			comment.AppendData (null);
			AssertEquals ("foo", comment.Data);
		}

		public void TestDeleteData ()
		{
			comment.Value = "bar";
			changed = false;
			changing = false;
			comment.DeleteData (1, 1);
			Assert (changed);
			Assert (changing);
			AssertEquals ("br", comment.Data);

			try 
			{
				comment.Value = "foo";
				comment.DeleteData(-1, 1);
				Fail ("Expected an ArgumentOutOfRangeException to be thrown.");
			} 
			catch (ArgumentOutOfRangeException) {}

			comment.Value = "foo";
			comment.DeleteData(1, 5);
			AssertEquals("f", comment.Data);

			comment.Value = "foo";
			comment.DeleteData(3, 10);
			AssertEquals("foo", comment.Data);
		}

		public void TestInsertData ()
		{
			comment.Value = "foobaz";
			changed = false;
			changing = false;
			comment.InsertData (3, "bar");
			Assert (changed);
			Assert (changing);
			AssertEquals ("foobarbaz", comment.Data);

			try 
			{
				comment.Value = "foo";
				comment.InsertData (-1, "bar");
				Fail ("Expected an ArgumentOutOfRangeException to be thrown.");
			} 
			catch (ArgumentOutOfRangeException) {}

			comment.Value = "foo";
			comment.InsertData (3, "bar");
			AssertEquals ("foobar", comment.Data);

			try 
			{
				comment.Value = "foo";
				comment.InsertData (4, "bar");
				Fail ("Expected an ArgumentOutOfRangeException to be thrown.");
			} 
			catch (ArgumentOutOfRangeException) {}

			try 
			{
				comment.Value = "foo";
				comment.InsertData (1, null);
				Fail ("Expected an ArgumentNullException to be thrown.");
			} 
			catch (ArgumentNullException) {}
		}

		public void TestReplaceData ()
		{
			changed = false;
			changing = false;
			comment.ReplaceData (0, 3, "bar");
			Assert (changed);
			Assert (changing);
			AssertEquals ("bar", comment.Data);

			comment.Value = "foo";
			comment.ReplaceData (2, 3, "bar");
			AssertEquals ("fobar", comment.Data);

			comment.Value = "foo";
			comment.ReplaceData (3, 3, "bar");
			AssertEquals ("foobar", comment.Data);

			try 
			{
				comment.Value = "foo";
				comment.ReplaceData (4, 3, "bar");
				Fail ("Expected an ArgumentOutOfRangeException to be thrown.");
			} 
			catch (ArgumentOutOfRangeException) {}

			try 
			{
				comment.Value = "foo";
				comment.ReplaceData (-1, 3, "bar");
				Fail ("Expected an ArgumentOutOfRangeException to be thrown.");
			} 
			catch (ArgumentOutOfRangeException) {}

			comment.Value = "foo";
			comment.ReplaceData (0, 2, "bar");
			AssertEquals ("baro", comment.Data);

			comment.Value = "foo";
			comment.ReplaceData (0, 5, "bar");
			AssertEquals ("bar", comment.Data);

			try 
			{
				comment.Value = "foo";
				comment.ReplaceData (1, 1, null);
				Fail ("Expected an ArgumentNullException to be thrown.");
			} 
			catch (ArgumentNullException) {}
		}
	}
}
