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
	public class XmlNodeListTests : Assertion
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
			AssertEquals ("Expected a text node.", node.NodeType, XmlNodeType.Text);
			AssertEquals ("Shouldn't have children.", node.HasChildNodes, false);
			AssertEquals ("Should be empty node list.", node.ChildNodes.Count, 0);
			AssertEquals ("Should be empty node list.", node.GetEnumerator().MoveNext(), false);
		}

		[Test]
		public void ZeroChildren ()
		{
			document.LoadXml ("<foo/>");
			documentElement = document.DocumentElement;
			AssertEquals ("Should be empty node list.", documentElement.GetEnumerator().MoveNext(), false);
		}

		[Test]
		public void OneChild ()
		{
			document.LoadXml ("<foo><child1/></foo>");
			documentElement = document.DocumentElement;
			AssertEquals ("Incorrect number of children returned from Count property.", documentElement.ChildNodes.Count, 1);
			index = 1;
			foreach (XmlNode childNode in documentElement.ChildNodes) 
			{
				AssertEquals ("Enumerator didn't return correct node.", "child" + index.ToString(), childNode.LocalName);
				index++;
			}
			AssertEquals ("foreach didn't loop over all children correctly.", index, 2);
		}

		[Test]
		public void MultipleChildren ()
		{
			document.LoadXml ("<foo><child1/><child2/><child3/></foo>");
			element = document.DocumentElement;
			AssertEquals ("Incorrect number of children returned from Count property.", element.ChildNodes.Count, 3);
			AssertNull ("Index less than zero should have returned null.", element.ChildNodes [-1]);
			AssertNull ("Index greater than or equal to Count should have returned null.", element.ChildNodes [3]);
			AssertEquals ("Didn't return the correct child.", element.FirstChild, element.ChildNodes[0]);
			AssertEquals ("Didn't return the correct child.", "child1", element.ChildNodes[0].LocalName);
			AssertEquals ("Didn't return the correct child.", "child2", element.ChildNodes[1].LocalName);
			AssertEquals ("Didn't return the correct child.", "child3", element.ChildNodes[2].LocalName);

			index = 1;
			foreach (XmlNode childNode in element.ChildNodes) 
			{
				AssertEquals ("Enumerator didn't return correct node.", "child" + index.ToString(), childNode.LocalName);
				index++;
			}
			AssertEquals ("foreach didn't loop over all children correctly.", index, 4);
		}

		[Test]
		public void AppendChildAffectOnEnumeration ()
		{
			document.LoadXml ("<foo><child1/></foo>");
			element = document.DocumentElement;
			enumerator = element.GetEnumerator();
			AssertEquals ("MoveNext should have succeeded.", enumerator.MoveNext(), true);
			AssertEquals ("MoveNext should have failed.", enumerator.MoveNext(), false);
			enumerator.Reset();
			AssertEquals ("MoveNext should have succeeded.", enumerator.MoveNext(), true);
			element.AppendChild(document.CreateElement("child2"));
			AssertEquals ("MoveNext should have succeeded.", enumerator.MoveNext(), true);
			AssertEquals ("MoveNext should have failed.", enumerator.MoveNext(), false);
		}

		[Test]
		public void RemoveChildAffectOnEnumeration ()
		{
			document.LoadXml ("<foo><child1/><child2/></foo>");
			element = document.DocumentElement;
			enumerator = element.GetEnumerator();
			element.RemoveChild(element.FirstChild);
			enumerator.MoveNext();
			AssertEquals ("Expected child2 element.", ((XmlElement)enumerator.Current).LocalName, "child2");
		}

		[Test]
		public void RemoveChildAffectOnEnumerationWhenEnumeratorIsOnRemovedChild ()
		{
			document.LoadXml ("<foo><child1/><child2/><child3/></foo>");
			element = document.DocumentElement;
			enumerator = element.GetEnumerator ();
			enumerator.MoveNext ();
			enumerator.MoveNext ();
			AssertEquals ("Expected child2 element.", "child2", ((XmlElement)enumerator.Current).LocalName);
			AssertEquals ("Expected child2 element.", "child2", element.FirstChild.NextSibling.LocalName);
			element.RemoveChild (element.FirstChild.NextSibling);
			enumerator.MoveNext ();
			
			try {
				element = (XmlElement) enumerator.Current;
				Fail ("Expected an InvalidOperationException.");
			} catch (InvalidOperationException) { }
		}

		// TODO:  Take the word save off front of this method when XmlNode.ReplaceChild() is implemented.

		public void saveTestReplaceChildAffectOnEnumeration ()
		{
			document.LoadXml ("<foo><child1/><child2/></foo>");
			element = document.DocumentElement;
			node = document.CreateElement("child3");
			enumerator = element.GetEnumerator();
			AssertEquals ("MoveNext should have succeeded.", enumerator.MoveNext(), true);
			element.ReplaceChild(node, element.LastChild);
			enumerator.MoveNext();
			AssertEquals ("Expected child3 element.", ((XmlElement)enumerator.Current).LocalName, "child3");
			AssertEquals ("MoveNext should have failed.", enumerator.MoveNext(), false);
		}

		[Test]
		public void RemoveOnlyChildAffectOnEnumeration ()
		{
			document.LoadXml ("<foo><child1/></foo>");
			element = document.DocumentElement;
			enumerator = element.GetEnumerator();
			element.RemoveChild(element.FirstChild);
			AssertEquals ("MoveNext should have failed.", enumerator.MoveNext(), false);
		}

		// TODO:  Take the word save off front of this method when XmlNode.RemoveAll() is fully implemented.

		public void saveTestRemoveAllAffectOnEnumeration ()
		{
			document.LoadXml ("<foo><child1/><child2/><child3/></foo>");
			element = document.DocumentElement;
			enumerator = element.GetEnumerator();
			AssertEquals ("Expected 3 children.", element.ChildNodes.Count, 3);
			AssertEquals ("MoveNext should have succeeded.", enumerator.MoveNext(), true);
			element.RemoveAll();
			AssertEquals ("MoveNext should have failed.", enumerator.MoveNext(), false);
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
				Fail ("Calling Current property before first node in list should have thrown InvalidOperationException.");
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
				Fail ("Calling Current property after last node in list should have thrown InvalidOperationException.");
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
			AssertEquals("Consecutive calls to Current property should yield same reference.", Object.ReferenceEquals(enumerator.Current, enumerator.Current), true);
		}

		[Test]
		public void Reset ()
		{
			document.LoadXml ("<foo><child1/><child2/></foo>");
			element = document.DocumentElement;
			enumerator = element.GetEnumerator();
			enumerator.MoveNext();
			enumerator.MoveNext();
			AssertEquals("Expected child2.", ((XmlElement)enumerator.Current).LocalName, "child2");
			enumerator.Reset();
			enumerator.MoveNext();
			AssertEquals("Expected child1.", ((XmlElement)enumerator.Current).LocalName, "child1");
		}

		[Test]
		public void ReturnNullWhenIndexIsOutOfRange ()
		{
			document.LoadXml ("<root><foo/></root>");
			XmlNodeList nl = document.DocumentElement.GetElementsByTagName ("bar");
			AssertEquals ("empty list. count", 0, nl.Count);
			try {
				AssertNull ("index 0", nl [0]);
				AssertNull ("index 1", nl [1]);
				AssertNull ("index -1", nl [-1]);
			} catch (ArgumentOutOfRangeException) {
				Fail ("don't throw index out of range.");
			}
		}
	}
}
