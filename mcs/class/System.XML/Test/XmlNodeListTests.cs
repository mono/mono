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
			Assertion.AssertEquals ("Expected a text node.", node.NodeType, XmlNodeType.Text);
			Assertion.AssertEquals ("Shouldn't have children.", node.HasChildNodes, false);
			Assertion.AssertEquals ("Should be empty node list.", node.ChildNodes.Count, 0);
			Assertion.AssertEquals ("Should be empty node list.", node.GetEnumerator().MoveNext(), false);
		}

		[Test]
		public void ZeroChildren ()
		{
			document.LoadXml ("<foo/>");
			documentElement = document.DocumentElement;
			Assertion.AssertEquals ("Should be empty node list.", documentElement.GetEnumerator().MoveNext(), false);
		}

		[Test]
		public void OneChild ()
		{
			document.LoadXml ("<foo><child1/></foo>");
			documentElement = document.DocumentElement;
			Assertion.AssertEquals ("Incorrect number of children returned from Count property.", documentElement.ChildNodes.Count, 1);
			index = 1;
			foreach (XmlNode childNode in documentElement.ChildNodes) 
			{
				Assertion.AssertEquals ("Enumerator didn't return correct node.", "child" + index.ToString(), childNode.LocalName);
				index++;
			}
			Assertion.AssertEquals ("foreach didn't loop over all children correctly.", index, 2);
		}

		[Test]
		public void MultipleChildren ()
		{
			document.LoadXml ("<foo><child1/><child2/><child3/></foo>");
			element = document.DocumentElement;
			Assertion.AssertEquals ("Incorrect number of children returned from Count property.", element.ChildNodes.Count, 3);
			Assertion.AssertNull ("Index less than zero should have returned null.", element.ChildNodes [-1]);
			Assertion.AssertNull ("Index greater than or equal to Count should have returned null.", element.ChildNodes [3]);
			Assertion.AssertEquals ("Didn't return the correct child.", element.FirstChild, element.ChildNodes[0]);
			Assertion.AssertEquals ("Didn't return the correct child.", "child1", element.ChildNodes[0].LocalName);
			Assertion.AssertEquals ("Didn't return the correct child.", "child2", element.ChildNodes[1].LocalName);
			Assertion.AssertEquals ("Didn't return the correct child.", "child3", element.ChildNodes[2].LocalName);

			index = 1;
			foreach (XmlNode childNode in element.ChildNodes) 
			{
				Assertion.AssertEquals ("Enumerator didn't return correct node.", "child" + index.ToString(), childNode.LocalName);
				index++;
			}
			Assertion.AssertEquals ("foreach didn't loop over all children correctly.", index, 4);
		}

		[Test]
		public void AppendChildAffectOnEnumeration ()
		{
			document.LoadXml ("<foo><child1/></foo>");
			element = document.DocumentElement;
			enumerator = element.GetEnumerator();
			Assertion.AssertEquals ("MoveNext should have succeeded.", enumerator.MoveNext(), true);
			Assertion.AssertEquals ("MoveNext should have failed.", enumerator.MoveNext(), false);
			enumerator.Reset();
			Assertion.AssertEquals ("MoveNext should have succeeded.", enumerator.MoveNext(), true);
			element.AppendChild(document.CreateElement("child2"));
			Assertion.AssertEquals ("MoveNext should have succeeded.", enumerator.MoveNext(), true);
			Assertion.AssertEquals ("MoveNext should have failed.", enumerator.MoveNext(), false);
		}

		[Test]
		public void RemoveChildAffectOnEnumeration ()
		{
			document.LoadXml ("<foo><child1/><child2/></foo>");
			element = document.DocumentElement;
			enumerator = element.GetEnumerator();
			element.RemoveChild(element.FirstChild);
			enumerator.MoveNext();
			Assertion.AssertEquals ("Expected child2 element.", ((XmlElement)enumerator.Current).LocalName, "child2");
		}

		[Test]
		public void RemoveChildAffectOnEnumerationWhenEnumeratorIsOnRemovedChild ()
		{
			document.LoadXml ("<foo><child1/><child2/><child3/></foo>");
			element = document.DocumentElement;
			enumerator = element.GetEnumerator ();
			enumerator.MoveNext ();
			enumerator.MoveNext ();
			Assertion.AssertEquals ("Expected child2 element.", "child2", ((XmlElement)enumerator.Current).LocalName);
			Assertion.AssertEquals ("Expected child2 element.", "child2", element.FirstChild.NextSibling.LocalName);
			element.RemoveChild (element.FirstChild.NextSibling);
			enumerator.MoveNext ();
			
			try {
				element = (XmlElement) enumerator.Current;
				Assertion.Fail ("Expected an InvalidOperationException.");
			} catch (InvalidOperationException) { }
		}

		// TODO:  Take the word save off front of this method when XmlNode.ReplaceChild() is implemented.

		public void saveTestReplaceChildAffectOnEnumeration ()
		{
			document.LoadXml ("<foo><child1/><child2/></foo>");
			element = document.DocumentElement;
			node = document.CreateElement("child3");
			enumerator = element.GetEnumerator();
			Assertion.AssertEquals ("MoveNext should have succeeded.", enumerator.MoveNext(), true);
			element.ReplaceChild(node, element.LastChild);
			enumerator.MoveNext();
			Assertion.AssertEquals ("Expected child3 element.", ((XmlElement)enumerator.Current).LocalName, "child3");
			Assertion.AssertEquals ("MoveNext should have failed.", enumerator.MoveNext(), false);
		}

		[Test]
		public void RemoveOnlyChildAffectOnEnumeration ()
		{
			document.LoadXml ("<foo><child1/></foo>");
			element = document.DocumentElement;
			enumerator = element.GetEnumerator();
			element.RemoveChild(element.FirstChild);
			Assertion.AssertEquals ("MoveNext should have failed.", enumerator.MoveNext(), false);
		}

		// TODO:  Take the word save off front of this method when XmlNode.RemoveAll() is fully implemented.

		public void saveTestRemoveAllAffectOnEnumeration ()
		{
			document.LoadXml ("<foo><child1/><child2/><child3/></foo>");
			element = document.DocumentElement;
			enumerator = element.GetEnumerator();
			Assertion.AssertEquals ("Expected 3 children.", element.ChildNodes.Count, 3);
			Assertion.AssertEquals ("MoveNext should have succeeded.", enumerator.MoveNext(), true);
			element.RemoveAll();
			Assertion.AssertEquals ("MoveNext should have failed.", enumerator.MoveNext(), false);
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
				Assertion.Fail ("Calling Current property before first node in list should have thrown InvalidOperationException.");
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
				Assertion.Fail ("Calling Current property after last node in list should have thrown InvalidOperationException.");
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
			Assertion.AssertEquals("Consecutive calls to Current property should yield same reference.", Object.ReferenceEquals(enumerator.Current, enumerator.Current), true);
		}

		[Test]
		public void Reset ()
		{
			document.LoadXml ("<foo><child1/><child2/></foo>");
			element = document.DocumentElement;
			enumerator = element.GetEnumerator();
			enumerator.MoveNext();
			enumerator.MoveNext();
			Assertion.AssertEquals("Expected child2.", ((XmlElement)enumerator.Current).LocalName, "child2");
			enumerator.Reset();
			enumerator.MoveNext();
			Assertion.AssertEquals("Expected child1.", ((XmlElement)enumerator.Current).LocalName, "child1");
		}

		[Test]
		public void ReturnNullWhenIndexIsOutOfRange ()
		{
			document.LoadXml ("<root><foo/></root>");
			XmlNodeList nl = document.DocumentElement.GetElementsByTagName ("bar");
			Assertion.AssertEquals ("empty list. count", 0, nl.Count);
			try {
				Assertion.AssertNull ("index 0", nl [0]);
				Assertion.AssertNull ("index 1", nl [1]);
				Assertion.AssertNull ("index -1", nl [-1]);
			} catch (ArgumentOutOfRangeException) {
				Assertion.Fail ("don't throw index out of range.");
			}
		}
	}
}
