//
// System.Xml.XmlDocumentTests
//
// Authors:
//   Jason Diamond <jason@injektilo.org>
//   Kral Ferch <kral_ferch@hotmail.com>
//
// (C) 2002 Jason Diamond, Kral Ferch
//

using System;
using System.Collections;
using System.Xml;
using System.IO;
using System.Text;

using NUnit.Framework;

namespace MonoTests.System.Xml
{
	public class XmlDocumentTests : TestCase
	{
		public XmlDocumentTests () : base ("MonoTests.System.Xml.XmlDocumentTests testsuite") {}
		public XmlDocumentTests (string name) : base (name) {}

		private XmlDocument document;
		private ArrayList eventStrings = new ArrayList();

		// These Event* methods support the TestEventNode* Tests in this file.
		// Most of them are event handlers for the XmlNodeChangedEventHandler
		// delegate.
		private void EventStringAdd(string eventName, XmlNodeChangedEventArgs e)
		{
			string oldParent = (e.OldParent != null) ? e.OldParent.Name : "<none>";
			string newParent = (e.NewParent != null) ? e.NewParent.Name : "<none>";
			eventStrings.Add (String.Format ("{0}, {1}, {2}, {3}, {4}", eventName, e.Action.ToString (), e.Node.OuterXml, oldParent, newParent));
		}

		private void EventNodeChanged(Object sender, XmlNodeChangedEventArgs e)
		{
			EventStringAdd ("NodeChanged", e);
		}

		private void EventNodeChanging (Object sender, XmlNodeChangedEventArgs e)
		{
			EventStringAdd ("NodeChanging", e);
		}

		private void EventNodeChangingException (Object sender, XmlNodeChangedEventArgs e)
		{
			throw new Exception ("don't change the value.");
		}

		private void EventNodeInserted(Object sender, XmlNodeChangedEventArgs e)
		{
			EventStringAdd ("NodeInserted", e);
		}

		private void EventNodeInserting(Object sender, XmlNodeChangedEventArgs e)
		{
			EventStringAdd ("NodeInserting", e);
		}

		private void EventNodeInsertingException(Object sender, XmlNodeChangedEventArgs e)
		{
			throw new Exception ("don't insert the element.");
		}

		private void EventNodeRemoved(Object sender, XmlNodeChangedEventArgs e)
		{
			EventStringAdd ("NodeRemoved", e);
		}

		private void EventNodeRemoving(Object sender, XmlNodeChangedEventArgs e)
		{
			EventStringAdd ("NodeRemoving", e);
		}

		private void EventNodeRemovingException(Object sender, XmlNodeChangedEventArgs e)
		{
			throw new Exception ("don't remove the element.");
		}

		protected override void SetUp ()
		{
			document = new XmlDocument ();
			document.PreserveWhitespace = true;
		}

		public void TestCreateNodeNodeTypeNameEmptyParams ()
		{
			XmlNode node;

			try {
				node = document.CreateNode (null, null, null);
				Fail ("Expected an ArgumentException to be thrown.");
			} catch (ArgumentException) {}

			try {
				node = document.CreateNode ("attribute", null, null);
				Fail ("Expected a NullReferenceException to be thrown.");
			} catch (NullReferenceException) {}

			try {
				node = document.CreateNode ("attribute", "", null);
				Fail ("Expected an ArgumentException to be thrown.");
			} catch (ArgumentException) {}

			try {
				node = document.CreateNode ("element", null, null);
				Fail ("Expected a NullReferenceException to be thrown.");
			} catch (NullReferenceException) {}

			try {
				node = document.CreateNode ("element", "", null);
				Fail ("Expected an ArgumentException to be thrown.");
			} catch (ArgumentException) {}

			try {
				node = document.CreateNode ("entityreference", null, null);
				Fail ("Expected a NullReferenceException to be thrown.");
			} catch (NullReferenceException) {}
		}

		public void TestCreateNodeInvalidXmlNodeType ()
		{
			XmlNode node;

			try {
				node = document.CreateNode (XmlNodeType.EndElement, null, null);
				Fail ("Expected an ArgumentOutOfRangeException to be thrown.");
			} catch (ArgumentOutOfRangeException) {}

			try {
				node = document.CreateNode (XmlNodeType.EndEntity, null, null);
				Fail ("Expected an ArgumentOutOfRangeException to be thrown.");
			} catch (ArgumentOutOfRangeException) {}

			try {
				node = document.CreateNode (XmlNodeType.Entity, null, null);
				Fail ("Expected an ArgumentOutOfRangeException to be thrown.");
			} catch (ArgumentOutOfRangeException) {}

			try {
				node = document.CreateNode (XmlNodeType.None, null, null);
				Fail ("Expected an ArgumentOutOfRangeException to be thrown.");
			} catch (ArgumentOutOfRangeException) {}

			try {
				node = document.CreateNode (XmlNodeType.Notation, null, null);
				Fail ("Expected an ArgumentOutOfRangeException to be thrown.");
			} catch (ArgumentOutOfRangeException) {}

			// TODO:  undocumented allowable type.
			node = document.CreateNode (XmlNodeType.XmlDeclaration, null, null);
			AssertEquals (XmlNodeType.XmlDeclaration, node.NodeType);
		}

		public void TestCreateNodeWhichParamIsUsed ()
		{
			XmlNode node;

			// No constructor params for Document, DocumentFragment.

			node = document.CreateNode (XmlNodeType.CDATA, "a", "b", "c");
			AssertEquals (String.Empty, ((XmlCDataSection)node).Value);

			node = document.CreateNode (XmlNodeType.Comment, "a", "b", "c");
			AssertEquals (String.Empty, ((XmlComment)node).Value);

			node = document.CreateNode (XmlNodeType.DocumentType, "a", "b", "c");
			AssertNull (((XmlDocumentType)node).Value);

// TODO: add this back in to test when it's implemented.
//			node = document.CreateNode (XmlNodeType.EntityReference, "a", "b", "c");
//			AssertNull (((XmlEntityReference)node).Value);

			node = document.CreateNode (XmlNodeType.ProcessingInstruction, "a", "b", "c");
			AssertEquals (String.Empty, ((XmlProcessingInstruction)node).Value);

			node = document.CreateNode (XmlNodeType.SignificantWhitespace, "a", "b", "c");
			AssertEquals (String.Empty, ((XmlSignificantWhitespace)node).Value);

			node = document.CreateNode (XmlNodeType.Text, "a", "b", "c");
			AssertEquals (String.Empty, ((XmlText)node).Value);

			node = document.CreateNode (XmlNodeType.Whitespace, "a", "b", "c");
			AssertEquals (String.Empty, ((XmlWhitespace)node).Value);

			node = document.CreateNode (XmlNodeType.XmlDeclaration, "a", "b", "c");
			AssertEquals ("version=\"1.0\"", ((XmlDeclaration)node).Value);
		}

		public void TestCreateNodeNodeTypeName ()
		{
			XmlNode node;

			try {
				node = document.CreateNode ("foo", null, null);
				Fail ("Expected an ArgumentException to be thrown.");
			} catch (ArgumentException) {}

			node = document.CreateNode("attribute", "foo", null);
			AssertEquals (XmlNodeType.Attribute, node.NodeType);

			node = document.CreateNode("cdatasection", null, null);
			AssertEquals (XmlNodeType.CDATA, node.NodeType);

			node = document.CreateNode("comment", null, null);
			AssertEquals (XmlNodeType.Comment, node.NodeType);

			node = document.CreateNode("document", null, null);
			AssertEquals (XmlNodeType.Document, node.NodeType);
			// TODO: test which constructor this ended up calling,
			// i.e. reuse underlying NameTable or not?

// TODO: add this back in to test when it's implemented.
//			node = document.CreateNode("documentfragment", null, null);
//			AssertEquals (XmlNodeType.DocumentFragment, node.NodeType);

			node = document.CreateNode("documenttype", null, null);
			AssertEquals (XmlNodeType.DocumentType, node.NodeType);

			node = document.CreateNode("element", "foo", null);
			AssertEquals (XmlNodeType.Element, node.NodeType);

// TODO: add this back in to test when it's implemented.
//			node = document.CreateNode("entityreference", "foo", null);
//			AssertEquals (XmlNodeType.EntityReference, node.NodeType);

			node = document.CreateNode("processinginstruction", null, null);
			AssertEquals (XmlNodeType.ProcessingInstruction, node.NodeType);

			node = document.CreateNode("significantwhitespace", null, null);
			AssertEquals (XmlNodeType.SignificantWhitespace, node.NodeType);

			node = document.CreateNode("text", null, null);
			AssertEquals (XmlNodeType.Text, node.NodeType);

			node = document.CreateNode("whitespace", null, null);
			AssertEquals (XmlNodeType.Whitespace, node.NodeType);
		}

		public void TestDocumentElement ()
		{
			AssertNull (document.DocumentElement);
			XmlElement element = document.CreateElement ("foo", "bar", "http://foo/");
			AssertNotNull (element);

			AssertEquals ("foo", element.Prefix);
			AssertEquals ("bar", element.LocalName);
			AssertEquals ("http://foo/", element.NamespaceURI);

			AssertEquals ("foo:bar", element.Name);

			AssertSame (element, document.AppendChild (element));

			AssertSame (element, document.DocumentElement);
		}

		public void TestDocumentEmpty()
		{
			AssertEquals ("Incorrect output for empty document.", "", document.OuterXml);
		}

		public void TestEventNodeChanged()
		{
			XmlElement element;
			XmlComment comment;

			document.NodeChanged += new XmlNodeChangedEventHandler (this.EventNodeChanged);

			// Node that is part of the document.
			document.AppendChild (document.CreateElement ("foo"));
			comment = document.CreateComment ("bar");
			document.DocumentElement.AppendChild (comment);
			AssertEquals ("<!--bar-->", document.DocumentElement.InnerXml);
			comment.Value = "baz";
			Assert (eventStrings.Contains ("NodeChanged, Change, <!--baz-->, foo, foo"));
			AssertEquals ("<!--baz-->", document.DocumentElement.InnerXml);

			// Node that isn't part of the document but created by the document.
			element = document.CreateElement ("foo");
			comment = document.CreateComment ("bar");
			element.AppendChild (comment);
			AssertEquals ("<!--bar-->", element.InnerXml);
			comment.Value = "baz";
			Assert (eventStrings.Contains ("NodeChanged, Change, <!--baz-->, foo, foo"));
			AssertEquals ("<!--baz-->", element.InnerXml);

/*
 TODO:  Insert this when XmlNode.InnerText() and XmlNode.InnerXml() have been implemented.
 
			// Node that is part of the document.
			element = document.CreateElement ("foo");
			element.InnerText = "bar";
			document.AppendChild(element);
			element.InnerText = "baz";
			Assert(eventStrings.Contains("NodeChanged, Change, baz, foo, foo"));
			
			// Node that isn't part of the document but created by the document.
			element = document.CreateElement("qux");
			element.InnerText = "quux";
			element.InnerText = "quuux";
			Assert(eventStrings.Contains("NodeChanged, Change, quuux, qux, qux"));
*/
		}

		public void TestEventNodeChanging()
		{
			XmlElement element;
			XmlComment comment;

			document.NodeChanging += new XmlNodeChangedEventHandler (this.EventNodeChanging);

			// Node that is part of the document.
			document.AppendChild (document.CreateElement ("foo"));
			comment = document.CreateComment ("bar");
			document.DocumentElement.AppendChild (comment);
			AssertEquals ("<!--bar-->", document.DocumentElement.InnerXml);
			comment.Value = "baz";
			Assert (eventStrings.Contains ("NodeChanging, Change, <!--bar-->, foo, foo"));
			AssertEquals ("<!--baz-->", document.DocumentElement.InnerXml);

			// Node that isn't part of the document but created by the document.
			element = document.CreateElement ("foo");
			comment = document.CreateComment ("bar");
			element.AppendChild (comment);
			AssertEquals ("<!--bar-->", element.InnerXml);
			comment.Value = "baz";
			Assert (eventStrings.Contains ("NodeChanging, Change, <!--bar-->, foo, foo"));
			AssertEquals ("<!--baz-->", element.InnerXml);

			// If an exception is thrown the Document returns to original state.
			document.NodeChanging += new XmlNodeChangedEventHandler (this.EventNodeChangingException);
			element = document.CreateElement("foo");
			comment = document.CreateComment ("bar");
			element.AppendChild (comment);
			AssertEquals ("<!--bar-->", element.InnerXml);
			try 
			{
				comment.Value = "baz";
				Fail("Expected an exception to be thrown by the NodeChanging event handler method EventNodeChangingException().");
			} catch (Exception) {}
			AssertEquals ("<!--bar-->", element.InnerXml);

			// Yes it's a bit anal but this tests whether the node changing event exception fires before the
			// ArgumentOutOfRangeException.  Turns out it does so that means our implementation needs to raise
			// the node changing event before doing any work.
			try 
			{
				comment.ReplaceData(-1, 0, "qux");
				Fail("Expected an ArgumentOutOfRangeException to be thrown.");
			} 
			catch (Exception) {}

			/*
 TODO:  Insert this when XmlNode.InnerText() and XmlNode.InnerXml() have been implemented.
 
			// Node that is part of the document.
			element = document.CreateElement ("foo");
			element.InnerText = "bar";
			document.AppendChild(element);
			element.InnerText = "baz";
			Assert(eventStrings.Contains("NodeChanging, Change, bar, foo, foo"));

			// Node that isn't part of the document but created by the document.
			element = document.CreateElement("foo");
			element.InnerText = "bar";
			element.InnerText = "baz";
			Assert(eventStrings.Contains("NodeChanging, Change, bar, foo, foo"));

			// If an exception is thrown the Document returns to original state.
			document.NodeChanging += new XmlNodeChangedEventHandler (this.EventNodeChangingException);
			element = document.CreateElement("foo");
			element.InnerText = "bar";
			try {
				element.InnerText = "baz";
				Fail("Expected an exception to be thrown by the NodeChanging event handler method EventNodeChangingException().");
			} catch (Exception) {}
			AssertEquals("bar", element.InnerText);
*/
		}

		public void TestEventNodeInserted()
		{
			XmlElement element;

			document.NodeInserted += new XmlNodeChangedEventHandler (this.EventNodeInserted);

			// Inserted 'foo' element to the document.
			element = document.CreateElement ("foo");
			document.AppendChild (element);
			Assert (eventStrings.Contains ("NodeInserted, Insert, <foo />, <none>, #document"));

			// Append child on node in document
			element = document.CreateElement ("foo");
			document.DocumentElement.AppendChild (element);
			Assert (eventStrings.Contains ("NodeInserted, Insert, <foo />, <none>, foo"));

			// Append child on node not in document but created by document
			element = document.CreateElement ("bar");
			element.AppendChild(document.CreateElement ("bar"));
			Assert(eventStrings.Contains("NodeInserted, Insert, <bar />, <none>, bar"));
		}

		public void TestEventNodeInserting()
		{
			XmlElement element;

			document.NodeInserting += new XmlNodeChangedEventHandler (this.EventNodeInserting);

			// Inserting 'foo' element to the document.
			element = document.CreateElement ("foo");
			document.AppendChild (element);
			Assert (eventStrings.Contains ("NodeInserting, Insert, <foo />, <none>, #document"));

			// Append child on node in document
			element = document.CreateElement ("foo");
			document.DocumentElement.AppendChild (element);
			Assert(eventStrings.Contains ("NodeInserting, Insert, <foo />, <none>, foo"));

			// Append child on node not in document but created by document
			element = document.CreateElement ("bar");
			AssertEquals (0, element.ChildNodes.Count);
			element.AppendChild (document.CreateElement ("bar"));
			Assert (eventStrings.Contains ("NodeInserting, Insert, <bar />, <none>, bar"));
			AssertEquals (1, element.ChildNodes.Count);

			// If an exception is thrown the Document returns to original state.
			document.NodeInserting += new XmlNodeChangedEventHandler (this.EventNodeInsertingException);
			AssertEquals (1, element.ChildNodes.Count);
			try 
			{
				element.AppendChild (document.CreateElement("baz"));
				Fail ("Expected an exception to be thrown by the NodeInserting event handler method EventNodeInsertingException().");
			} 
			catch (Exception) {}
			AssertEquals (1, element.ChildNodes.Count);
		}

		public void TestEventNodeRemoved()
		{
			XmlElement element;
			XmlElement element2;

			document.NodeRemoved += new XmlNodeChangedEventHandler (this.EventNodeRemoved);

			// Removed 'bar' element from 'foo' outside document.
			element = document.CreateElement ("foo");
			element2 = document.CreateElement ("bar");
			element.AppendChild (element2);
			AssertEquals (1, element.ChildNodes.Count);
			element.RemoveChild (element2);
			Assert (eventStrings.Contains ("NodeRemoved, Remove, <bar />, foo, <none>"));
			AssertEquals (0, element.ChildNodes.Count);

/*
 * TODO:  put this test back in when AttributeCollection.RemoveAll() is implemented.

			// RemoveAll.
			element = document.CreateElement ("foo");
			element2 = document.CreateElement ("bar");
			element.AppendChild(element2);
			AssertEquals(1, element.ChildNodes.Count);
			element.RemoveAll();
			Assert (eventStrings.Contains ("NodeRemoved, Remove, <bar />, foo, <none>"));
			AssertEquals(0, element.ChildNodes.Count);
*/

			// Removed 'bar' element from 'foo' inside document.
			element = document.CreateElement ("foo");
			document.AppendChild (element);
			element = document.CreateElement ("bar");
			document.DocumentElement.AppendChild (element);
			AssertEquals (1, document.DocumentElement.ChildNodes.Count);
			document.DocumentElement.RemoveChild (element);
			Assert (eventStrings.Contains ("NodeRemoved, Remove, <bar />, foo, <none>"));
			AssertEquals (0, document.DocumentElement.ChildNodes.Count);
		}
	
		public void TestEventNodeRemoving()
		{
			XmlElement element;
			XmlElement element2;

			document.NodeRemoving += new XmlNodeChangedEventHandler (this.EventNodeRemoving);

			// Removing 'bar' element from 'foo' outside document.
			element = document.CreateElement ("foo");
			element2 = document.CreateElement ("bar");
			element.AppendChild (element2);
			AssertEquals (1, element.ChildNodes.Count);
			element.RemoveChild (element2);
			Assert (eventStrings.Contains ("NodeRemoving, Remove, <bar />, foo, <none>"));
			AssertEquals (0, element.ChildNodes.Count);

/*
 * TODO:  put this test back in when AttributeCollection.RemoveAll() is implemented.

			// RemoveAll.
			element = document.CreateElement ("foo");
			element2 = document.CreateElement ("bar");
			element.AppendChild(element2);
			AssertEquals(1, element.ChildNodes.Count);
			element.RemoveAll();
			Assert (eventStrings.Contains ("NodeRemoving, Remove, <bar />, foo, <none>"));
			AssertEquals(0, element.ChildNodes.Count);
*/

			// Removing 'bar' element from 'foo' inside document.
			element = document.CreateElement ("foo");
			document.AppendChild (element);
			element = document.CreateElement ("bar");
			document.DocumentElement.AppendChild (element);
			AssertEquals (1, document.DocumentElement.ChildNodes.Count);
			document.DocumentElement.RemoveChild (element);
			Assert (eventStrings.Contains ("NodeRemoving, Remove, <bar />, foo, <none>"));
			AssertEquals (0, document.DocumentElement.ChildNodes.Count);

			// If an exception is thrown the Document returns to original state.
			document.NodeRemoving += new XmlNodeChangedEventHandler (this.EventNodeRemovingException);
			element.AppendChild (element2);
			AssertEquals (1, element.ChildNodes.Count);
			try 
			{
				element.RemoveChild(element2);
				Fail ("Expected an exception to be thrown by the NodeRemoving event handler method EventNodeRemovingException().");
			} 
			catch (Exception) {}
			AssertEquals (1, element.ChildNodes.Count);
		}

		public void TestGetElementsByTagNameNoNameSpace ()
		{
			string xml = @"<library><book><title>XML Fun</title><author>John Doe</author>
				<price>34.95</price></book><book><title>Bear and the Dragon</title>
				<author>Tom Clancy</author><price>6.95</price></book><book>
				<title>Bourne Identity</title><author>Robert Ludlum</author>
				<price>9.95</price></book><Fluffer><Nutter><book>
				<title>Bourne Ultimatum</title><author>Robert Ludlum</author>
				<price>9.95</price></book></Nutter></Fluffer></library>";

			MemoryStream memoryStream = new MemoryStream (Encoding.UTF8.GetBytes (xml));
			document = new XmlDocument ();
			document.Load (memoryStream);
			XmlNodeList bookList = document.GetElementsByTagName ("book");
			AssertEquals ("GetElementsByTagName (string) returned incorrect count.", 4, bookList.Count);
		}

		public void TestGetElementsByTagNameUsingNameSpace ()
		{
			StringBuilder xml = new StringBuilder ();
			xml.Append ("<?xml version=\"1.0\" ?><library xmlns:North=\"http://www.foo.com\" ");
			xml.Append ("xmlns:South=\"http://www.goo.com\"><North:book type=\"non-fiction\"> ");
			xml.Append ("<North:title type=\"intro\">XML Fun</North:title> " );
			xml.Append ("<North:author>John Doe</North:author> " );
			xml.Append ("<North:price>34.95</North:price></North:book> " );
			xml.Append ("<South:book type=\"fiction\"> " );
			xml.Append ("<South:title>Bear and the Dragon</South:title> " );
			xml.Append ("<South:author>Tom Clancy</South:author> " );
                        xml.Append ("<South:price>6.95</South:price></South:book> " );
			xml.Append ("<South:book type=\"fiction\"><South:title>Bourne Identity</South:title> " );
			xml.Append ("<South:author>Robert Ludlum</South:author> " );
			xml.Append ("<South:price>9.95</South:price></South:book></library>");

			MemoryStream memoryStream = new MemoryStream (Encoding.UTF8.GetBytes (xml.ToString ()));
			document = new XmlDocument ();
			document.Load (memoryStream);
			XmlNodeList bookList = document.GetElementsByTagName ("book", "http://www.goo.com");
			AssertEquals ("GetElementsByTagName (string, uri) returned incorrect count.", 2, bookList.Count);
		}

	
		public void TestInnerAndOuterXml ()
		{
			AssertEquals (String.Empty, document.InnerXml);
			AssertEquals (document.InnerXml, document.OuterXml);

			XmlDeclaration declaration = document.CreateXmlDeclaration ("1.0", null, null);
			document.AppendChild (declaration);
			AssertEquals ("<?xml version=\"1.0\"?>", document.InnerXml);
			AssertEquals (document.InnerXml, document.OuterXml);

			XmlElement element = document.CreateElement ("foo");
			document.AppendChild (element);
			AssertEquals ("<?xml version=\"1.0\"?><foo />", document.InnerXml);
			AssertEquals (document.InnerXml, document.OuterXml);

			XmlComment comment = document.CreateComment ("bar");
			document.DocumentElement.AppendChild (comment);
			AssertEquals ("<?xml version=\"1.0\"?><foo><!--bar--></foo>", document.InnerXml);
			AssertEquals (document.InnerXml, document.OuterXml);

			XmlText text = document.CreateTextNode ("baz");
			document.DocumentElement.AppendChild (text);
			AssertEquals ("<?xml version=\"1.0\"?><foo><!--bar-->baz</foo>", document.InnerXml);
			AssertEquals (document.InnerXml, document.OuterXml);

			element = document.CreateElement ("quux");
			element.SetAttribute ("quuux", "squonk");
			document.DocumentElement.AppendChild (element);
			AssertEquals ("<?xml version=\"1.0\"?><foo><!--bar-->baz<quux quuux=\"squonk\" /></foo>", document.InnerXml);
			AssertEquals (document.InnerXml, document.OuterXml);
		}

		public void TestLoadWithSystemIOStream ()
		{			
			string xml = @"<library><book><title>XML Fun</title><author>John Doe</author>
				<price>34.95</price></book><book><title>Bear and the Dragon</title>
				<author>Tom Clancy</author><price>6.95</price></book><book>
				<title>Bourne Identity</title><author>Robert Ludlum</author>
				<price>9.95</price></book><Fluffer><Nutter><book>
				<title>Bourne Ultimatum</title><author>Robert Ludlum</author>
				<price>9.95</price></book></Nutter></Fluffer></library>";

			MemoryStream memoryStream = new MemoryStream (Encoding.UTF8.GetBytes (xml));
			document = new XmlDocument ();
			document.Load (memoryStream);
			AssertEquals ("Not Loaded From IOStream", true, document.HasChildNodes);
		}

		public void TestLoadXmlCDATA ()
		{
			document.LoadXml ("<foo><![CDATA[bar]]></foo>");
			Assert (document.DocumentElement.FirstChild.NodeType == XmlNodeType.CDATA);
			AssertEquals ("bar", document.DocumentElement.FirstChild.Value);
		}

		public void TestLoadXMLComment()
		{
// XmlTextReader needs to throw this exception
//			try {
//				document.LoadXml("<!--foo-->");
//				Fail("XmlException should have been thrown.");
//			}
//			catch (XmlException e) {
//				AssertEquals("Exception message doesn't match.", "The root element is missing.", e.Message);
//			}

			document.LoadXml ("<foo><!--Comment--></foo>");
			Assert (document.DocumentElement.FirstChild.NodeType == XmlNodeType.Comment);
			AssertEquals ("Comment", document.DocumentElement.FirstChild.Value);

			document.LoadXml (@"<foo><!--bar--></foo>");
			AssertEquals ("Incorrect target.", "bar", ((XmlComment)document.FirstChild.FirstChild).Data);
		}

		public void TestLoadXmlElementSingle ()
		{
			AssertNull (document.DocumentElement);
			document.LoadXml ("<foo/>");

			AssertNotNull (document.DocumentElement);
			AssertSame (document.FirstChild, document.DocumentElement);

			AssertEquals (String.Empty, document.DocumentElement.Prefix);
			AssertEquals ("foo", document.DocumentElement.LocalName);
			AssertEquals (String.Empty, document.DocumentElement.NamespaceURI);
			AssertEquals ("foo", document.DocumentElement.Name);
		}

		public void TestLoadXmlElementWithAttributes ()
		{
			AssertNull (document.DocumentElement);
			document.LoadXml ("<foo bar='baz' quux='quuux'/>");

			XmlElement documentElement = document.DocumentElement;

			AssertEquals ("baz", documentElement.GetAttribute ("bar"));
			AssertEquals ("quuux", documentElement.GetAttribute ("quux"));
		}
		public void TestLoadXmlElementWithChildElement ()
		{
			document.LoadXml ("<foo><bar/></foo>");
			Assert (document.ChildNodes.Count == 1);
			Assert (document.FirstChild.ChildNodes.Count == 1);
			AssertEquals ("foo", document.DocumentElement.LocalName);
			AssertEquals ("bar", document.DocumentElement.FirstChild.LocalName);
		}

		public void TestLoadXmlElementWithTextNode ()
		{
			document.LoadXml ("<foo>bar</foo>");
			Assert (document.DocumentElement.FirstChild.NodeType == XmlNodeType.Text);
			AssertEquals ("bar", document.DocumentElement.FirstChild.Value);
		}

		public void TestLoadXmlExceptionClearsDocument ()
		{
			document.LoadXml ("<foo/>");
			Assert (document.FirstChild != null);
			
			try {
				document.LoadXml ("<123/>");
				Fail ("An XmlException should have been thrown.");
			} catch (XmlException) {}

			Assert (document.FirstChild == null);
		}

		public void TestLoadXmlProcessingInstruction ()
		{
			document.LoadXml (@"<?foo bar='baaz' quux='quuux'?><quuuux></quuuux>");
			AssertEquals ("Incorrect target.", "foo", ((XmlProcessingInstruction)document.FirstChild).Target);
			AssertEquals ("Incorrect data.", "bar='baaz' quux='quuux'", ((XmlProcessingInstruction)document.FirstChild).Data);
		}

		public void TestOuterXml ()
		{
			string xml;
			
			xml = "<root><![CDATA[foo]]></root>";
			document.LoadXml (xml);
			AssertEquals("XmlDocument with cdata OuterXml is incorrect.", xml, document.OuterXml);

			xml = "<root><!--foo--></root>";
			document.LoadXml (xml);
			AssertEquals("XmlDocument with comment OuterXml is incorrect.", xml, document.OuterXml);

			xml = "<root><?foo bar?></root>";
			document.LoadXml (xml);
			AssertEquals("XmlDocument with processing instruction OuterXml is incorrect.", xml, document.OuterXml);
		}

		public void TestParentNodes ()
		{
			document.LoadXml ("<foo><bar><baz/></bar></foo>");
			XmlNode node = document.FirstChild.FirstChild.FirstChild;
			AssertEquals ("Wrong child found.", "baz", node.LocalName);
			AssertEquals ("Wrong parent.", "bar", node.ParentNode.LocalName);
			AssertEquals ("Wrong parent.", "foo", node.ParentNode.ParentNode.LocalName);
			AssertEquals ("Wrong parent.", "#document", node.ParentNode.ParentNode.ParentNode.LocalName);
			AssertNull ("Expected parent to be null.", node.ParentNode.ParentNode.ParentNode.ParentNode);
		}

		public void TestRemovedElementNextSibling ()
		{
			XmlNode node;
			XmlNode nextSibling;

			document.LoadXml ("<foo><child1/><child2/></foo>");
			node = document.DocumentElement.FirstChild;
			document.DocumentElement.RemoveChild (node);
			nextSibling = node.NextSibling;
			AssertNull ("Expected removed node's next sibling to be null.", nextSibling);
		}

		// ImportNode
		public void TestImportNode ()
		{
			XmlNode n;

			string xlinkURI = "http://www.w3.org/1999/XLink";
			string xml1 = "<?xml version='1.0' encoding='utf-8' ?><foo xmlns:xlink='" + xlinkURI + "'><bar a1='v1' xlink:href='#foo'><baz><![CDATA[cdata section.\n\titem 1\n\titem 2\n]]>From here, simple text node.</baz></bar></foo>";
			document.LoadXml(xml1);
			XmlDocument newDoc = new XmlDocument();
			newDoc.LoadXml("<hoge><fuga /></hoge>");
			XmlElement bar = document.DocumentElement.FirstChild as XmlElement;

			// Attribute
			n = newDoc.ImportNode(bar.GetAttributeNode("href", xlinkURI), true);
			AssertEquals("#ImportNode.Attr.NS.LocalName", "href", n.LocalName);
			AssertEquals("#ImportNode.Attr.NS.NSURI", xlinkURI, n.NamespaceURI);
			AssertEquals("#ImportNode.Attr.NS.Value", "#foo", n.Value);

			// CDATA
			n = newDoc.ImportNode(bar.FirstChild.FirstChild, true);
			AssertEquals("#ImportNode.CDATA", "cdata section.\n\titem 1\n\titem 2\n", n.Value);

			// Element
			XmlElement e = newDoc.ImportNode(bar, true) as XmlElement;
			AssertEquals("#ImportNode.Element.Name", "bar", e.Name);
			AssertEquals("#ImportNode.Element.Attr", "#foo", e.GetAttribute("href", xlinkURI));
			AssertEquals("#ImportNode.Element.deep", "baz", e.FirstChild.Name);

			// Entity Reference:
			//   [2002/10/14] CreateEntityReference was not implemented.
//			document.LoadXml("<!DOCTYPE test PUBLIC 'dummy' [<!ENTITY FOOENT 'foo'>]><root>&FOOENT;</root>");
//			n = newDoc.ImportNode(document.DocumentElement.FirstChild);
//			AssertEquals("#ImportNode.EntityReference", "FOOENT", n.Name);
//			AssertEquals("#ImportNode.EntityReference", "foo_", n.Value);

			// Processing Instruction
			document.LoadXml("<foo><?xml-stylesheet href='foo.xsl' ?></foo>");
			XmlProcessingInstruction pi = (XmlProcessingInstruction)newDoc.ImportNode(document.DocumentElement.FirstChild, false);
			AssertEquals("#ImportNode.ProcessingInstruction.Name", "xml-stylesheet", pi.Name);
			AssertEquals("#ImportNode.ProcessingInstruction.Data", "href='foo.xsl'", pi.Data.Trim());
			
			// Text
			document.LoadXml(xml1);
			n = newDoc.ImportNode((XmlText)bar.FirstChild.ChildNodes[1], true);
			AssertEquals("#ImportNode.Text", "From here, simple text node.", n.Value);

			// XmlDeclaration
			document.LoadXml(xml1);
			XmlDeclaration decl = (XmlDeclaration)newDoc.ImportNode(document.FirstChild, false);
			AssertEquals("#ImportNode.XmlDeclaration.Type", XmlNodeType.XmlDeclaration, decl.NodeType);
			AssertEquals("#ImportNode.XmlDeclaration.Encoding", "utf-8", decl.Encoding);
		}

		public void TestNameTable()
		{
			XmlDocument doc = new XmlDocument();
			AssertNotNull(doc.NameTable);
		}

		public void TestSingleEmptyRootDocument()
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml("<root />");
			AssertNotNull(doc.DocumentElement);
		}

		public void TestDocumentWithDoctypeDecl ()
		{
			XmlDocument doc = new XmlDocument ();
			try {
				doc.LoadXml ("<!DOCTYPE test><root />");
			} catch (XmlException) {
				Fail ("#DoctypeDecl.OnlyName");
			}
			try 
			{
				doc.LoadXml ("<!DOCTYPE test SYSTEM 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'><root />");
			} catch (XmlException) {
				Fail("#DoctypeDecl.System");
			}
			try {
				doc.LoadXml ("<!DOCTYPE test PUBLIC '-//test' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'><root />");
			} catch (XmlException) {
				Fail ("#DoctypeDecl.Public");
			}
			// Should this be commented out?
//			try {
//				doc.LoadXml ("<!DOCTYPE test [<!ELEMENT foo >]><root />");
//			} catch (XmlException) {
//				Fail("#DoctypeDecl.ElementDecl");
//			}
		}

		public void TestCloneNode ()
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<foo><bar /><baz hoge='fuga'>TEST Text</baz></foo>");
			XmlDocument doc2 = (XmlDocument)doc.CloneNode (false);
			AssertEquals ("ShallowCopy", 0, doc2.ChildNodes.Count);
			doc2 = (XmlDocument)doc.CloneNode (true);
			AssertEquals ("DeepCopy", "foo", doc2.DocumentElement.Name);
		}

		public void TestOuterXmlWithDefaultXmlns ()
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<iq type=\"get\" id=\"ATECLIENT_1\"><query xmlns=\"jabber:iq:auth\"><username></username></query></iq>");
			AssertEquals ("<iq type=\"get\" id=\"ATECLIENT_1\"><query xmlns=\"jabber:iq:auth\"><username /></query></iq>", doc.OuterXml);
		}
	}
}
