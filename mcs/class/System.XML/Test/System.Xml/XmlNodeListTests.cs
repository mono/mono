//
// System.Xml.XmlTextWriterTests
//
// Authors:
//   Kral Ferch <kral_ferch@hotmail.com>
//   Martin Willemoes Hansen <mwh@sysrq.dk>
//
// (C) 2002 Kral Ferch
// (C) 2003 Martin Willemoes Hansen
//

using System;
using System.Xml;
using System.Collections;

using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlNodeListTests
	{
		XmlDocument document;
		XmlElement documentElement;
		XmlElement element;
		XmlNode node;
		Object obj;
		IEnumerator enumerator;
		int index;

		[SetUp]
		public void GetReady ()
		{
			document = new XmlDocument ();
		}

		[Test]
		public void NodeTypesThatCantHaveChildren ()
		{
			document.LoadXml ("<foo>bar</foo>");
			documentElement = document.DocumentElement;
			node = documentElement.FirstChild;
			Assert.AreEqual (node.NodeType, XmlNodeType.Text, "Expected a text node.");
			Assert.AreEqual (node.HasChildNodes, false, "Shouldn't have children.");
			Assert.AreEqual (node.ChildNodes.Count, 0, "Should be empty node list.");
			Assert.AreEqual (node.GetEnumerator().MoveNext(), false, "Should be empty node list.");
		}

		[Test]
		public void ZeroChildren ()
		{
			document.LoadXml ("<foo/>");
			documentElement = document.DocumentElement;
			Assert.AreEqual (documentElement.GetEnumerator().MoveNext(), false, "Should be empty node list.");
		}

		[Test]
		public void OneChild ()
		{
			document.LoadXml ("<foo><child1/></foo>");
			documentElement = document.DocumentElement;
			Assert.AreEqual (documentElement.ChildNodes.Count, 1, "Incorrect number of children returned from Count property.");
			index = 1;
			foreach (XmlNode childNode in documentElement.ChildNodes) 
			{
				Assert.AreEqual ("child" + index.ToString(), childNode.LocalName, "Enumerator didn't return correct node.");
				index++;
			}
			Assert.AreEqual (index, 2, "foreach didn't loop over all children correctly.");
		}

		[Test]
		public void MultipleChildren ()
		{
			document.LoadXml ("<foo><child1/><child2/><child3/></foo>");
			element = document.DocumentElement;
			Assert.AreEqual (element.ChildNodes.Count, 3, "Incorrect number of children returned from Count property.");
			Assert.IsNull (element.ChildNodes [-1], "Index less than zero should have returned null.");
			Assert.IsNull (element.ChildNodes [3], "Index greater than or equal to Count should have returned null.");
			Assert.AreEqual (element.FirstChild, element.ChildNodes[0], "Didn't return the correct child.");
			Assert.AreEqual ("child1", element.ChildNodes[0].LocalName, "Didn't return the correct child.");
			Assert.AreEqual ("child2", element.ChildNodes[1].LocalName, "Didn't return the correct child.");
			Assert.AreEqual ("child3", element.ChildNodes[2].LocalName, "Didn't return the correct child.");

			index = 1;
			foreach (XmlNode childNode in element.ChildNodes) 
			{
				Assert.AreEqual ("child" + index.ToString(), childNode.LocalName, "Enumerator didn't return correct node.");
				index++;
			}
			Assert.AreEqual (index, 4, "foreach didn't loop over all children correctly.");
		}

		[Test]
		public void AppendChildAffectOnEnumeration ()
		{
			document.LoadXml ("<foo><child1/></foo>");
			element = document.DocumentElement;
			enumerator = element.GetEnumerator();
			Assert.AreEqual (enumerator.MoveNext(), true, "MoveNext should have succeeded.");
			Assert.AreEqual (enumerator.MoveNext(), false, "MoveNext should have failed.");
			enumerator.Reset();
			Assert.AreEqual (enumerator.MoveNext(), true, "MoveNext should have succeeded.");
			element.AppendChild(document.CreateElement("child2"));
			Assert.AreEqual (enumerator.MoveNext(), true, "MoveNext should have succeeded.");
			Assert.AreEqual (enumerator.MoveNext(), false, "MoveNext should have failed.");
		}

		[Test]
		public void RemoveChildAffectOnEnumeration ()
		{
			document.LoadXml ("<foo><child1/><child2/></foo>");
			element = document.DocumentElement;
			enumerator = element.GetEnumerator();
			element.RemoveChild(element.FirstChild);
			enumerator.MoveNext();
			Assert.AreEqual (((XmlElement)enumerator.Current).LocalName, "child2", "Expected child2 element.");
		}

		[Test]
		public void RemoveChildAffectOnEnumerationWhenEnumeratorIsOnRemovedChild ()
		{
			document.LoadXml ("<foo><child1/><child2/><child3/></foo>");
			element = document.DocumentElement;
			enumerator = element.GetEnumerator ();
			enumerator.MoveNext ();
			enumerator.MoveNext ();
			Assert.AreEqual ("child2", ((XmlElement)enumerator.Current).LocalName, "Expected child2 element.");
			Assert.AreEqual ("child2", element.FirstChild.NextSibling.LocalName, "Expected child2 element.");
			element.RemoveChild (element.FirstChild.NextSibling);
			enumerator.MoveNext ();
			
			try {
				element = (XmlElement) enumerator.Current;
				Assert.Fail ("Expected an InvalidOperationException.");
			} catch (InvalidOperationException) { }
		}

		// TODO:  Take the word save off front of this method when XmlNode.ReplaceChild() is implemented.

		public void saveTestReplaceChildAffectOnEnumeration ()
		{
			document.LoadXml ("<foo><child1/><child2/></foo>");
			element = document.DocumentElement;
			node = document.CreateElement("child3");
			enumerator = element.GetEnumerator();
			Assert.AreEqual (enumerator.MoveNext(), true, "MoveNext should have succeeded.");
			element.ReplaceChild(node, element.LastChild);
			enumerator.MoveNext();
			Assert.AreEqual (((XmlElement)enumerator.Current).LocalName, "child3", "Expected child3 element.");
			Assert.AreEqual (enumerator.MoveNext(), false, "MoveNext should have failed.");
		}

		[Test]
		public void RemoveOnlyChildAffectOnEnumeration ()
		{
			document.LoadXml ("<foo><child1/></foo>");
			element = document.DocumentElement;
			enumerator = element.GetEnumerator();
			element.RemoveChild(element.FirstChild);
			Assert.AreEqual (enumerator.MoveNext(), false, "MoveNext should have failed.");
		}

		// TODO:  Take the word save off front of this method when XmlNode.RemoveAll() is fully implemented.

		public void saveTestRemoveAllAffectOnEnumeration ()
		{
			document.LoadXml ("<foo><child1/><child2/><child3/></foo>");
			element = document.DocumentElement;
			enumerator = element.GetEnumerator();
			Assert.AreEqual (element.ChildNodes.Count, 3, "Expected 3 children.");
			Assert.AreEqual (enumerator.MoveNext(), true, "MoveNext should have succeeded.");
			element.RemoveAll();
			Assert.AreEqual (enumerator.MoveNext(), false, "MoveNext should have failed.");
		}

		[Test]
		public void CurrentBeforeFirstNode ()
		{
			document.LoadXml ("<foo><child1/></foo>");
			element = document.DocumentElement;
			enumerator = element.GetEnumerator();
			try 
			{
				obj = enumerator.Current;
				Assert.Fail ("Calling Current property before first node in list should have thrown InvalidOperationException.");
			} catch (InvalidOperationException) { }
		}

		[Test]
		public void CurrentAfterLastNode ()
		{
			document.LoadXml ("<foo><child1/></foo>");
			element = document.DocumentElement;
			enumerator = element.GetEnumerator();
			enumerator.MoveNext();
			enumerator.MoveNext();
			try 
			{
				obj = enumerator.Current;
				Assert.Fail ("Calling Current property after last node in list should have thrown InvalidOperationException.");
			} 
			catch (InvalidOperationException) { }
		}

		[Test]
		public void CurrentDoesntMove ()
		{
			document.LoadXml ("<foo><child1/></foo>");
			element = document.DocumentElement;
			enumerator = element.GetEnumerator();
			enumerator.MoveNext();
			Assert.AreEqual (Object.ReferenceEquals(enumerator.Current, enumerator.Current), true, "Consecutive calls to Current property should yield same reference.");
		}

		[Test]
		public void Reset ()
		{
			document.LoadXml ("<foo><child1/><child2/></foo>");
			element = document.DocumentElement;
			enumerator = element.GetEnumerator();
			enumerator.MoveNext();
			enumerator.MoveNext();
			Assert.AreEqual (((XmlElement)enumerator.Current).LocalName, "child2", "Expected child2.");
			enumerator.Reset();
			enumerator.MoveNext();
			Assert.AreEqual (((XmlElement)enumerator.Current).LocalName, "child1", "Expected child1.");
		}

		[Test]
		public void ReturnNullWhenIndexIsOutOfRange ()
		{
			document.LoadXml ("<root><foo/></root>");
			XmlNodeList nl = document.DocumentElement.GetElementsByTagName ("bar");
			Assert.AreEqual (0, nl.Count, "empty list. count");
			try {
				Assert.IsNull (nl [0], "index 0");
				Assert.IsNull (nl [1], "index 1");
				Assert.IsNull (nl [-1], "index -1");
			} catch (ArgumentOutOfRangeException) {
				Assert.Fail ("don't throw index out of range.");
			}
		}
	}
}
