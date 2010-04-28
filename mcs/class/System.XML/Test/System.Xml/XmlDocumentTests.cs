
// System.Xml.XmlDocumentTests
//
// Authors:
//   Jason Diamond <jason@injektilo.org>
//   Kral Ferch <kral_ferch@hotmail.com>
//   Martin Willemoes Hansen <mwh@sysrq.dk>
//
// (C) 2002 Jason Diamond, Kral Ferch
// (C) 2003 Martin Willemoes Hansen
//

using System;
using System.Collections;
using System.Xml;
using System.IO;
using System.Text;

using NUnit.Framework;

#if NET_2_0
using InvalidNodeTypeArgException = System.ArgumentException;
#else // it makes less sense
using InvalidNodeTypeArgException = System.ArgumentOutOfRangeException;
#endif

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlDocumentTests
	{
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

		[SetUp]
		public void GetReady ()
		{
			document = new XmlDocument ();
			document.PreserveWhitespace = true;
		}

		[Test]
		public void CreateNodeNodeTypeNameEmptyParams ()
		{
			try {
				document.CreateNode (null, null, null);
				Assert.Fail ("Expected an ArgumentException to be thrown.");
			} catch (ArgumentException) {}

			try {
				document.CreateNode ("attribute", null, null);
				Assert.Fail ("Expected a NullReferenceException to be thrown.");
			} catch (NullReferenceException) {}

			try {
				document.CreateNode ("attribute", "", null);
				Assert.Fail ("Expected an ArgumentException to be thrown.");
			} catch (ArgumentException) {}

			try {
				document.CreateNode ("element", null, null);
				Assert.Fail ("Expected a NullReferenceException to be thrown.");
			} catch (NullReferenceException) {}

			try {
				document.CreateNode ("element", "", null);
				Assert.Fail ("Expected an ArgumentException to be thrown.");
			} catch (ArgumentException) {}

			try {
				document.CreateNode ("entityreference", null, null);
				Assert.Fail ("Expected a NullReferenceException to be thrown.");
			} catch (NullReferenceException) {}
		}

		[Test]
		public void CreateNodeInvalidXmlNodeType ()
		{
			XmlNode node;

			try {
				node = document.CreateNode (XmlNodeType.EndElement, null, null);
				Assert.Fail ("Expected an ArgumentOutOfRangeException to be thrown.");
			} catch (InvalidNodeTypeArgException) {}

			try {
				node = document.CreateNode (XmlNodeType.EndEntity, null, null);
				Assert.Fail ("Expected an ArgumentOutOfRangeException to be thrown.");
			} catch (InvalidNodeTypeArgException) {}

			try {
				node = document.CreateNode (XmlNodeType.Entity, null, null);
				Assert.Fail ("Expected an ArgumentOutOfRangeException to be thrown.");
			} catch (InvalidNodeTypeArgException) {}

			try {
				node = document.CreateNode (XmlNodeType.None, null, null);
				Assert.Fail ("Expected an ArgumentOutOfRangeException to be thrown.");
			} catch (InvalidNodeTypeArgException) {}

			try {
				node = document.CreateNode (XmlNodeType.Notation, null, null);
				Assert.Fail ("Expected an ArgumentOutOfRangeException to be thrown.");
			} catch (InvalidNodeTypeArgException) {}

			// TODO:  undocumented allowable type.
			node = document.CreateNode (XmlNodeType.XmlDeclaration, null, null);
			Assert.AreEqual (XmlNodeType.XmlDeclaration, node.NodeType);
		}

		[Test]
		public void CreateNodeWhichParamIsUsed ()
		{
			XmlNode node;

			// No constructor params for Document, DocumentFragment.

			node = document.CreateNode (XmlNodeType.CDATA, "a", "b", "c");
			Assert.AreEqual (String.Empty, ((XmlCDataSection)node).Value);

			node = document.CreateNode (XmlNodeType.Comment, "a", "b", "c");
			Assert.AreEqual (String.Empty, ((XmlComment)node).Value);

			node = document.CreateNode (XmlNodeType.DocumentType, "a", "b", "c");
			Assert.IsNull (((XmlDocumentType)node).Value);

// TODO: add this back in to test when it's implemented.
//			node = document.CreateNode (XmlNodeType.EntityReference, "a", "b", "c");
//			Assert.IsNull (((XmlEntityReference)node).Value);

// TODO: add this back in to test when it's implemented.
//			node = document.CreateNode (XmlNodeType.ProcessingInstruction, "a", "b", "c");
//			Assert.AreEqual (String.Empty, ((XmlProcessingInstruction)node).Value);

			node = document.CreateNode (XmlNodeType.SignificantWhitespace, "a", "b", "c");
			Assert.AreEqual (String.Empty, ((XmlSignificantWhitespace)node).Value);

			node = document.CreateNode (XmlNodeType.Text, "a", "b", "c");
			Assert.AreEqual (String.Empty, ((XmlText)node).Value);

			node = document.CreateNode (XmlNodeType.Whitespace, "a", "b", "c");
			Assert.AreEqual (String.Empty, ((XmlWhitespace)node).Value);

			node = document.CreateNode (XmlNodeType.XmlDeclaration, "a", "b", "c");
			Assert.AreEqual ("version=\"1.0\"", ((XmlDeclaration)node).Value);
		}

		[Test]
#if NET_2_0
		[Category ("NotDotNet")] // enbug in 2.0
#endif
		public void CreateNodeNodeTypeName ()
		{
			XmlNode node;

			try {
				node = document.CreateNode ("foo", null, null);
				Assert.Fail ("Expected an ArgumentException to be thrown.");
			} catch (ArgumentException) {}

			// .NET 2.0 fails here.
			node = document.CreateNode("attribute", "foo", null);
			Assert.AreEqual (XmlNodeType.Attribute, node.NodeType);

			node = document.CreateNode("cdatasection", null, null);
			Assert.AreEqual (XmlNodeType.CDATA, node.NodeType);

			node = document.CreateNode("comment", null, null);
			Assert.AreEqual (XmlNodeType.Comment, node.NodeType);

			node = document.CreateNode("document", null, null);
			Assert.AreEqual (XmlNodeType.Document, node.NodeType);
			// TODO: test which constructor this ended up calling,
			// i.e. reuse underlying NameTable or not?

			node = document.CreateNode("documentfragment", null, null);
			Assert.AreEqual (XmlNodeType.DocumentFragment, node.NodeType);

			node = document.CreateNode("documenttype", null, null);
			Assert.AreEqual (XmlNodeType.DocumentType, node.NodeType);

			node = document.CreateNode("element", "foo", null);
			Assert.AreEqual (XmlNodeType.Element, node.NodeType);

// TODO: add this back in to test when it's implemented.
// ---> It is implemented, but it is LAMESPEC that allows null entity reference name.
//			node = document.CreateNode("entityreference", "foo", null);
//			Assert.AreEqual (XmlNodeType.EntityReference, node.NodeType);

// LAMESPEC: null PI name is silly.
//			node = document.CreateNode("processinginstruction", null, null);
//			Assert.AreEqual (XmlNodeType.ProcessingInstruction, node.NodeType);

			node = document.CreateNode("significantwhitespace", null, null);
			Assert.AreEqual (XmlNodeType.SignificantWhitespace, node.NodeType);

			node = document.CreateNode("text", null, null);
			Assert.AreEqual (XmlNodeType.Text, node.NodeType);

			node = document.CreateNode("whitespace", null, null);
			Assert.AreEqual (XmlNodeType.Whitespace, node.NodeType);
		}

		[Test]
		public void DocumentElement ()
		{
			Assert.IsNull (document.DocumentElement);
			XmlElement element = document.CreateElement ("foo", "bar", "http://foo/");
			Assert.IsNotNull (element);

			Assert.AreEqual ("foo", element.Prefix);
			Assert.AreEqual ("bar", element.LocalName);
			Assert.AreEqual ("http://foo/", element.NamespaceURI);

			Assert.AreEqual ("foo:bar", element.Name);

			Assert.AreSame (element, document.AppendChild (element));

			Assert.AreSame (element, document.DocumentElement);
		}

		[Test]
		public void DocumentEmpty()
		{
			Assert.AreEqual ("", document.OuterXml, "Incorrect output for empty document.");
		}

		[Test]
		public void EventNodeChanged()
		{
			XmlElement element;
			XmlComment comment;

			document.NodeChanged += new XmlNodeChangedEventHandler (this.EventNodeChanged);

			// Node that is part of the document.
			document.AppendChild (document.CreateElement ("foo"));
			comment = document.CreateComment ("bar");
			document.DocumentElement.AppendChild (comment);
			Assert.AreEqual ("<!--bar-->", document.DocumentElement.InnerXml);
			comment.Value = "baz";
			Assert.IsTrue (eventStrings.Contains ("NodeChanged, Change, <!--baz-->, foo, foo"));
			Assert.AreEqual ("<!--baz-->", document.DocumentElement.InnerXml);

			// Node that isn't part of the document but created by the document.
			element = document.CreateElement ("foo");
			comment = document.CreateComment ("bar");
			element.AppendChild (comment);
			Assert.AreEqual ("<!--bar-->", element.InnerXml);
			comment.Value = "baz";
			Assert.IsTrue (eventStrings.Contains ("NodeChanged, Change, <!--baz-->, foo, foo"));
			Assert.AreEqual ("<!--baz-->", element.InnerXml);

/*
 TODO:  Insert this when XmlNode.InnerText() and XmlNode.InnerXml() have been implemented.
 
			// Node that is part of the document.
			element = document.CreateElement ("foo");
			element.InnerText = "bar";
			document.AppendChild(element);
			element.InnerText = "baz";
			Assert.IsTrue (eventStrings.Contains("NodeChanged, Change, baz, foo, foo"));
			
			// Node that isn't part of the document but created by the document.
			element = document.CreateElement("qux");
			element.InnerText = "quux";
			element.InnerText = "quuux";
			Assert.IsTrue (eventStrings.Contains("NodeChanged, Change, quuux, qux, qux"));
*/
		}

		[Test]
		public void EventNodeChanging()
		{
			XmlElement element;
			XmlComment comment;

			document.NodeChanging += new XmlNodeChangedEventHandler (this.EventNodeChanging);

			// Node that is part of the document.
			document.AppendChild (document.CreateElement ("foo"));
			comment = document.CreateComment ("bar");
			document.DocumentElement.AppendChild (comment);
			Assert.AreEqual ("<!--bar-->", document.DocumentElement.InnerXml);
			comment.Value = "baz";
			Assert.IsTrue (eventStrings.Contains ("NodeChanging, Change, <!--bar-->, foo, foo"));
			Assert.AreEqual ("<!--baz-->", document.DocumentElement.InnerXml);

			// Node that isn't part of the document but created by the document.
			element = document.CreateElement ("foo");
			comment = document.CreateComment ("bar");
			element.AppendChild (comment);
			Assert.AreEqual ("<!--bar-->", element.InnerXml);
			comment.Value = "baz";
			Assert.IsTrue (eventStrings.Contains ("NodeChanging, Change, <!--bar-->, foo, foo"));
			Assert.AreEqual ("<!--baz-->", element.InnerXml);

			// If an exception is thrown the Document returns to original state.
			document.NodeChanging += new XmlNodeChangedEventHandler (this.EventNodeChangingException);
			element = document.CreateElement("foo");
			comment = document.CreateComment ("bar");
			element.AppendChild (comment);
			Assert.AreEqual ("<!--bar-->", element.InnerXml);
			try 
			{
				comment.Value = "baz";
				Assert.Fail ("Expected an exception to be thrown by the NodeChanging event handler method EventNodeChangingException().");
			} catch (Exception) {}
			Assert.AreEqual ("<!--bar-->", element.InnerXml);

			// Yes it's a bit anal but this tests whether the node changing event exception fires before the
			// ArgumentOutOfRangeException.  Turns out it does so that means our implementation needs to raise
			// the node changing event before doing any work.
			try 
			{
				comment.ReplaceData(-1, 0, "qux");
				Assert.Fail ("Expected an ArgumentOutOfRangeException to be thrown.");
			} 
			catch (Exception) {}

			/*
 TODO:  Insert this when XmlNode.InnerText() and XmlNode.InnerXml() have been implemented.
 
			// Node that is part of the document.
			element = document.CreateElement ("foo");
			element.InnerText = "bar";
			document.AppendChild(element);
			element.InnerText = "baz";
			Assert.IsTrue (eventStrings.Contains("NodeChanging, Change, bar, foo, foo"));

			// Node that isn't part of the document but created by the document.
			element = document.CreateElement("foo");
			element.InnerText = "bar";
			element.InnerText = "baz";
			Assert.IsTrue (eventStrings.Contains("NodeChanging, Change, bar, foo, foo"));

			// If an exception is thrown the Document returns to original state.
			document.NodeChanging += new XmlNodeChangedEventHandler (this.EventNodeChangingException);
			element = document.CreateElement("foo");
			element.InnerText = "bar";
			try {
				element.InnerText = "baz";
				Assert.Fail ("Expected an exception to be thrown by the NodeChanging event handler method EventNodeChangingException().");
			} catch (Exception) {}
			Assert.AreEqual ("bar", element.InnerText);
*/
		}

		[Test]
		public void EventNodeInserted()
		{
			XmlElement element;

			document.NodeInserted += new XmlNodeChangedEventHandler (this.EventNodeInserted);

			// Inserted 'foo' element to the document.
			element = document.CreateElement ("foo");
			document.AppendChild (element);
			Assert.IsTrue (eventStrings.Contains ("NodeInserted, Insert, <foo />, <none>, #document"));

			// Append child on node in document
			element = document.CreateElement ("foo");
			document.DocumentElement.AppendChild (element);
			Assert.IsTrue (eventStrings.Contains ("NodeInserted, Insert, <foo />, <none>, foo"));

			// Append child on node not in document but created by document
			element = document.CreateElement ("bar");
			element.AppendChild(document.CreateElement ("bar"));
			Assert.IsTrue (eventStrings.Contains("NodeInserted, Insert, <bar />, <none>, bar"));
		}

		[Test]
		public void EventNodeInserting()
		{
			XmlElement element;

			document.NodeInserting += new XmlNodeChangedEventHandler (this.EventNodeInserting);

			// Inserting 'foo' element to the document.
			element = document.CreateElement ("foo");
			document.AppendChild (element);
			Assert.IsTrue (eventStrings.Contains ("NodeInserting, Insert, <foo />, <none>, #document"));

			// Append child on node in document
			element = document.CreateElement ("foo");
			document.DocumentElement.AppendChild (element);
			Assert.IsTrue (eventStrings.Contains ("NodeInserting, Insert, <foo />, <none>, foo"));

			// Append child on node not in document but created by document
			element = document.CreateElement ("bar");
			Assert.AreEqual (0, element.ChildNodes.Count);
			element.AppendChild (document.CreateElement ("bar"));
			Assert.IsTrue (eventStrings.Contains ("NodeInserting, Insert, <bar />, <none>, bar"));
			Assert.AreEqual (1, element.ChildNodes.Count);

			// If an exception is thrown the Document returns to original state.
			document.NodeInserting += new XmlNodeChangedEventHandler (this.EventNodeInsertingException);
			Assert.AreEqual (1, element.ChildNodes.Count);
			try 
			{
				element.AppendChild (document.CreateElement("baz"));
				Assert.Fail ("Expected an exception to be thrown by the NodeInserting event handler method EventNodeInsertingException().");
			} 
			catch (Exception) {}
			Assert.AreEqual (1, element.ChildNodes.Count);
		}

		[Test]
		public void EventNodeRemoved()
		{
			XmlElement element;
			XmlElement element2;

			document.NodeRemoved += new XmlNodeChangedEventHandler (this.EventNodeRemoved);

			// Removed 'bar' element from 'foo' outside document.
			element = document.CreateElement ("foo");
			element2 = document.CreateElement ("bar");
			element.AppendChild (element2);
			Assert.AreEqual (1, element.ChildNodes.Count);
			element.RemoveChild (element2);
			Assert.IsTrue (eventStrings.Contains ("NodeRemoved, Remove, <bar />, foo, <none>"));
			Assert.AreEqual (0, element.ChildNodes.Count);

/*
 * TODO:  put this test back in when AttributeCollection.RemoveAll() is implemented.

			// RemoveAll.
			element = document.CreateElement ("foo");
			element2 = document.CreateElement ("bar");
			element.AppendChild(element2);
			Assert.AreEqual (1, element.ChildNodes.Count);
			element.RemoveAll();
			Assert.IsTrue (eventStrings.Contains ("NodeRemoved, Remove, <bar />, foo, <none>"));
			Assert.AreEqual (0, element.ChildNodes.Count);
*/

			// Removed 'bar' element from 'foo' inside document.
			element = document.CreateElement ("foo");
			document.AppendChild (element);
			element = document.CreateElement ("bar");
			document.DocumentElement.AppendChild (element);
			Assert.AreEqual (1, document.DocumentElement.ChildNodes.Count);
			document.DocumentElement.RemoveChild (element);
			Assert.IsTrue (eventStrings.Contains ("NodeRemoved, Remove, <bar />, foo, <none>"));
			Assert.AreEqual (0, document.DocumentElement.ChildNodes.Count);
		}
	
		[Test]
		public void EventNodeRemoving()
		{
			XmlElement element;
			XmlElement element2;

			document.NodeRemoving += new XmlNodeChangedEventHandler (this.EventNodeRemoving);

			// Removing 'bar' element from 'foo' outside document.
			element = document.CreateElement ("foo");
			element2 = document.CreateElement ("bar");
			element.AppendChild (element2);
			Assert.AreEqual (1, element.ChildNodes.Count);
			element.RemoveChild (element2);
			Assert.IsTrue (eventStrings.Contains ("NodeRemoving, Remove, <bar />, foo, <none>"));
			Assert.AreEqual (0, element.ChildNodes.Count);

/*
 * TODO:  put this test back in when AttributeCollection.RemoveAll() is implemented.

			// RemoveAll.
			element = document.CreateElement ("foo");
			element2 = document.CreateElement ("bar");
			element.AppendChild(element2);
			Assert.AreEqual (1, element.ChildNodes.Count);
			element.RemoveAll();
			Assert.IsTrue (eventStrings.Contains ("NodeRemoving, Remove, <bar />, foo, <none>"));
			Assert.AreEqual (0, element.ChildNodes.Count);
*/

			// Removing 'bar' element from 'foo' inside document.
			element = document.CreateElement ("foo");
			document.AppendChild (element);
			element = document.CreateElement ("bar");
			document.DocumentElement.AppendChild (element);
			Assert.AreEqual (1, document.DocumentElement.ChildNodes.Count);
			document.DocumentElement.RemoveChild (element);
			Assert.IsTrue (eventStrings.Contains ("NodeRemoving, Remove, <bar />, foo, <none>"));
			Assert.AreEqual (0, document.DocumentElement.ChildNodes.Count);

			// If an exception is thrown the Document returns to original state.
			document.NodeRemoving += new XmlNodeChangedEventHandler (this.EventNodeRemovingException);
			element.AppendChild (element2);
			Assert.AreEqual (1, element.ChildNodes.Count);
			try 
			{
				element.RemoveChild(element2);
				Assert.Fail ("Expected an exception to be thrown by the NodeRemoving event handler method EventNodeRemovingException().");
			} 
			catch (Exception) {}
			Assert.AreEqual (1, element.ChildNodes.Count);
		}

		[Test]
		public void GetElementsByTagNameNoNameSpace ()
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
			Assert.AreEqual (4, bookList.Count, "GetElementsByTagName (string) returned incorrect count.");
		}

		[Test]
		public void GetElementsByTagNameUsingNameSpace ()
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
			Assert.AreEqual (2, bookList.Count, "GetElementsByTagName (string, uri) returned incorrect count.");
		}

		[Test]
		public void GetElementsByTagNameNs2 ()
		{
			document.LoadXml (@"<root>
			<x:a xmlns:x='urn:foo' id='a'>
			<y:a xmlns:y='urn:foo' id='b'/>
			<x:a id='c' />
			<z id='d' />
			text node
			<?a processing instruction ?>
			<x:w id='e'/>
			</x:a>
			</root>");
			// id='b' has different prefix. Should not caught by (name),
			// while should caught by (name, ns).
			XmlNodeList nl = document.GetElementsByTagName ("x:a");
			Assert.AreEqual (2, nl.Count);
			Assert.AreEqual ("a", nl [0].Attributes ["id"].Value);
			Assert.AreEqual ("c", nl [1].Attributes ["id"].Value);

			nl = document.GetElementsByTagName ("a", "urn:foo");
			Assert.AreEqual (3, nl.Count);
			Assert.AreEqual ("a", nl [0].Attributes ["id"].Value);
			Assert.AreEqual ("b", nl [1].Attributes ["id"].Value);
			Assert.AreEqual ("c", nl [2].Attributes ["id"].Value);

			// name wildcard
			nl = document.GetElementsByTagName ("*");
			Assert.AreEqual (6, nl.Count);
			Assert.AreEqual ("root", nl [0].Name);
			Assert.AreEqual ("a", nl [1].Attributes ["id"].Value);
			Assert.AreEqual ("b", nl [2].Attributes ["id"].Value);
			Assert.AreEqual ("c", nl [3].Attributes ["id"].Value);
			Assert.AreEqual ("d", nl [4].Attributes ["id"].Value);
			Assert.AreEqual ("e", nl [5].Attributes ["id"].Value);

			// wildcard - local and ns
			nl = document.GetElementsByTagName ("*", "*");
			Assert.AreEqual (6, nl.Count);
			Assert.AreEqual ("root", nl [0].Name);
			Assert.AreEqual ("a", nl [1].Attributes ["id"].Value);
			Assert.AreEqual ("b", nl [2].Attributes ["id"].Value);
			Assert.AreEqual ("c", nl [3].Attributes ["id"].Value);
			Assert.AreEqual ("d", nl [4].Attributes ["id"].Value);
			Assert.AreEqual ("e", nl [5].Attributes ["id"].Value);

			// namespace wildcard - namespace
			nl = document.GetElementsByTagName ("*", "urn:foo");
			Assert.AreEqual (4, nl.Count);
			Assert.AreEqual ("a", nl [0].Attributes ["id"].Value);
			Assert.AreEqual ("b", nl [1].Attributes ["id"].Value);
			Assert.AreEqual ("c", nl [2].Attributes ["id"].Value);
			Assert.AreEqual ("e", nl [3].Attributes ["id"].Value);

			// namespace wildcard - local only. I dare say, such usage is not XML-ish!
			nl = document.GetElementsByTagName ("a", "*");
			Assert.AreEqual (3, nl.Count);
			Assert.AreEqual ("a", nl [0].Attributes ["id"].Value);
			Assert.AreEqual ("b", nl [1].Attributes ["id"].Value);
			Assert.AreEqual ("c", nl [2].Attributes ["id"].Value);
		}

		[Test]
		public void Implementation ()
		{
			Assert.IsNotNull (new XmlDocument ().Implementation);
		}

		[Test]
		public void InnerAndOuterXml ()
		{
			Assert.AreEqual (String.Empty, document.InnerXml);
			Assert.AreEqual (document.InnerXml, document.OuterXml);

			XmlDeclaration declaration = document.CreateXmlDeclaration ("1.0", null, null);
			document.AppendChild (declaration);
			Assert.AreEqual ("<?xml version=\"1.0\"?>", document.InnerXml);
			Assert.AreEqual (document.InnerXml, document.OuterXml);

			XmlElement element = document.CreateElement ("foo");
			document.AppendChild (element);
			Assert.AreEqual ("<?xml version=\"1.0\"?><foo />", document.InnerXml);
			Assert.AreEqual (document.InnerXml, document.OuterXml);

			XmlComment comment = document.CreateComment ("bar");
			document.DocumentElement.AppendChild (comment);
			Assert.AreEqual ("<?xml version=\"1.0\"?><foo><!--bar--></foo>", document.InnerXml);
			Assert.AreEqual (document.InnerXml, document.OuterXml);

			XmlText text = document.CreateTextNode ("baz");
			document.DocumentElement.AppendChild (text);
			Assert.AreEqual ("<?xml version=\"1.0\"?><foo><!--bar-->baz</foo>", document.InnerXml);
			Assert.AreEqual (document.InnerXml, document.OuterXml);

			element = document.CreateElement ("quux");
			element.SetAttribute ("quuux", "squonk");
			document.DocumentElement.AppendChild (element);
			Assert.AreEqual ("<?xml version=\"1.0\"?><foo><!--bar-->baz<quux quuux=\"squonk\" /></foo>", document.InnerXml);
			Assert.AreEqual (document.InnerXml, document.OuterXml);
		}

		[Test]
		public void LoadWithSystemIOStream ()
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
			Assert.AreEqual (true, document.HasChildNodes, "Not Loaded From IOStream");
		}

		[Test]
		public void LoadXmlReaderNamespacesFalse ()
		{
			XmlTextReader xtr = new XmlTextReader (
				"<root xmlns='urn:foo' />", XmlNodeType.Document, null);
			xtr.Namespaces = false;
			document.Load (xtr); // Don't complain about xmlns attribute with its namespaceURI == String.Empty.
		}

		[Test]
		public void LoadXmlCDATA ()
		{
			document.LoadXml ("<foo><![CDATA[bar]]></foo>");
			Assert.IsTrue (document.DocumentElement.FirstChild.NodeType == XmlNodeType.CDATA);
			Assert.AreEqual ("bar", document.DocumentElement.FirstChild.Value);
		}

		[Test]
		public void LoadXMLComment()
		{
// XmlTextReader needs to throw this exception
//			try {
//				document.LoadXml("<!--foo-->");
//				Assert.Fail ("XmlException should have been thrown.");
//			}
//			catch (XmlException e) {
//				Assert.AreEqual ("The root element is missing.", e.Message, "Exception message doesn't match.");
//			}

			document.LoadXml ("<foo><!--Comment--></foo>");
			Assert.IsTrue (document.DocumentElement.FirstChild.NodeType == XmlNodeType.Comment);
			Assert.AreEqual ("Comment", document.DocumentElement.FirstChild.Value);

			document.LoadXml (@"<foo><!--bar--></foo>");
			Assert.AreEqual ("bar", ((XmlComment)document.FirstChild.FirstChild).Data, "Incorrect target.");
		}

		[Test]
		public void LoadXmlElementSingle ()
		{
			Assert.IsNull (document.DocumentElement);
			document.LoadXml ("<foo/>");

			Assert.IsNotNull (document.DocumentElement);
			Assert.AreSame (document.FirstChild, document.DocumentElement);

			Assert.AreEqual (String.Empty, document.DocumentElement.Prefix);
			Assert.AreEqual ("foo", document.DocumentElement.LocalName);
			Assert.AreEqual (String.Empty, document.DocumentElement.NamespaceURI);
			Assert.AreEqual ("foo", document.DocumentElement.Name);
		}

		[Test]
		public void LoadXmlElementWithAttributes ()
		{
			Assert.IsNull (document.DocumentElement);
			document.LoadXml ("<foo bar='baz' quux='quuux' hoge='hello &amp; world' />");

			XmlElement documentElement = document.DocumentElement;

			Assert.AreEqual ("baz", documentElement.GetAttribute ("bar"));
			Assert.AreEqual ("quuux", documentElement.GetAttribute ("quux"));
			Assert.AreEqual ("hello & world", documentElement.GetAttribute ("hoge"));
			Assert.AreEqual ("hello & world", documentElement.Attributes ["hoge"].Value);
			Assert.AreEqual (1, documentElement.GetAttributeNode ("hoge").ChildNodes.Count);
		}

		[Test]
		public void LoadXmlElementWithChildElement ()
		{
			document.LoadXml ("<foo><bar/></foo>");
			Assert.IsTrue (document.ChildNodes.Count == 1);
			Assert.IsTrue (document.FirstChild.ChildNodes.Count == 1);
			Assert.AreEqual ("foo", document.DocumentElement.LocalName);
			Assert.AreEqual ("bar", document.DocumentElement.FirstChild.LocalName);
		}

		[Test]
		public void LoadXmlElementWithTextNode ()
		{
			document.LoadXml ("<foo>bar</foo>");
			Assert.IsTrue (document.DocumentElement.FirstChild.NodeType == XmlNodeType.Text);
			Assert.AreEqual ("bar", document.DocumentElement.FirstChild.Value);
		}

		[Test]
		public void LoadXmlExceptionClearsDocument ()
		{
			document.LoadXml ("<foo/>");
			Assert.IsTrue (document.FirstChild != null);
			
			try {
				document.LoadXml ("<123/>");
				Assert.Fail ("An XmlException should have been thrown.");
			} catch (XmlException) {}

			Assert.IsTrue (document.FirstChild == null);
		}

		[Test]
		public void LoadXmlProcessingInstruction ()
		{
			document.LoadXml (@"<?foo bar='baaz' quux='quuux'?><quuuux></quuuux>");
			Assert.AreEqual ("foo", ((XmlProcessingInstruction)document.FirstChild).Target, "Incorrect target.");
			Assert.AreEqual ("bar='baaz' quux='quuux'", ((XmlProcessingInstruction)document.FirstChild).Data, "Incorrect data.");
		}

		[Test]
		public void OuterXml ()
		{
			string xml;
			
			xml = "<root><![CDATA[foo]]></root>";
			document.LoadXml (xml);
			Assert.AreEqual (xml, document.OuterXml, "XmlDocument with cdata OuterXml is incorrect.");

			xml = "<root><!--foo--></root>";
			document.LoadXml (xml);
			Assert.AreEqual (xml, document.OuterXml, "XmlDocument with comment OuterXml is incorrect.");

			xml = "<root><?foo bar?></root>";
			document.LoadXml (xml);
			Assert.AreEqual (xml, document.OuterXml, "XmlDocument with processing instruction OuterXml is incorrect.");
		}

		[Test]
		public void ParentNodes ()
		{
			document.LoadXml ("<foo><bar><baz/></bar></foo>");
			XmlNode node = document.FirstChild.FirstChild.FirstChild;
			Assert.AreEqual ("baz", node.LocalName, "Wrong child found.");
			Assert.AreEqual ("bar", node.ParentNode.LocalName, "Wrong parent.");
			Assert.AreEqual ("foo", node.ParentNode.ParentNode.LocalName, "Wrong parent.");
			Assert.AreEqual ("#document", node.ParentNode.ParentNode.ParentNode.LocalName, "Wrong parent.");
			Assert.IsNull (node.ParentNode.ParentNode.ParentNode.ParentNode, "Expected parent to be null.");
		}

		[Test]
		public void RemovedElementNextSibling ()
		{
			XmlNode node;
			XmlNode nextSibling;

			document.LoadXml ("<foo><child1/><child2/></foo>");
			node = document.DocumentElement.FirstChild;
			document.DocumentElement.RemoveChild (node);
			nextSibling = node.NextSibling;
			Assert.IsNull (nextSibling, "Expected removed node's next sibling to be null.");
		}

		// ImportNode
		[Test]
		public void ImportNode ()
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
			Assert.AreEqual ("href", n.LocalName, "#ImportNode.Attr.NS.LocalName");
			Assert.AreEqual (xlinkURI, n.NamespaceURI, "#ImportNode.Attr.NS.NSURI");
			Assert.AreEqual ("#foo", n.Value, "#ImportNode.Attr.NS.Value");

			// CDATA
			n = newDoc.ImportNode(bar.FirstChild.FirstChild, true);
			Assert.AreEqual ("cdata section.\n\titem 1\n\titem 2\n", n.Value, "#ImportNode.CDATA");

			// Element
			XmlElement e = newDoc.ImportNode(bar, true) as XmlElement;
			Assert.AreEqual ("bar", e.Name, "#ImportNode.Element.Name");
			Assert.AreEqual ("#foo", e.GetAttribute("href", xlinkURI), "#ImportNode.Element.Attr");
			Assert.AreEqual ("baz", e.FirstChild.Name, "#ImportNode.Element.deep");

			// Entity Reference:
			//   [2002/10/14] CreateEntityReference was not implemented.
//			document.LoadXml("<!DOCTYPE test PUBLIC 'dummy' [<!ENTITY FOOENT 'foo'>]><root>&FOOENT;</root>");
//			n = newDoc.ImportNode(document.DocumentElement.FirstChild);
//			Assert.AreEqual ("FOOENT", n.Name, "#ImportNode.EntityReference");
//			Assert.AreEqual ("foo_", n.Value, "#ImportNode.EntityReference");

			// Processing Instruction
			document.LoadXml("<foo><?xml-stylesheet href='foo.xsl' ?></foo>");
			XmlProcessingInstruction pi = (XmlProcessingInstruction)newDoc.ImportNode(document.DocumentElement.FirstChild, false);
			Assert.AreEqual ("xml-stylesheet", pi.Name, "#ImportNode.ProcessingInstruction.Name");
			Assert.AreEqual ("href='foo.xsl'", pi.Data.Trim(), "#ImportNode.ProcessingInstruction.Data");
			
			// Text
			document.LoadXml(xml1);
			n = newDoc.ImportNode((XmlText)bar.FirstChild.ChildNodes[1], true);
			Assert.AreEqual ("From here, simple text node.", n.Value, "#ImportNode.Text");

			// XmlDeclaration
			document.LoadXml(xml1);
			XmlDeclaration decl = (XmlDeclaration)newDoc.ImportNode(document.FirstChild, false);
			Assert.AreEqual (XmlNodeType.XmlDeclaration, decl.NodeType, "#ImportNode.XmlDeclaration.Type");
			Assert.AreEqual ("utf-8", decl.Encoding, "#ImportNode.XmlDeclaration.Encoding");
		}

		[Test]
		public void NameTable()
		{
			XmlDocument doc = new XmlDocument();
			Assert.IsNotNull (doc.NameTable);
		}

		[Test]
		public void SingleEmptyRootDocument()
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml("<root />");
			Assert.IsNotNull (doc.DocumentElement);
		}

		[Test]
		public void DocumentWithDoctypeDecl ()
		{
			XmlDocument doc = new XmlDocument ();
			// In fact it is invalid, but it doesn't fail with MS.NET 1.0.
			doc.LoadXml ("<!DOCTYPE test><root />");
			Assert.IsNotNull (doc.DocumentType);
#if NetworkEnabled
			try 
			{
				doc.LoadXml ("<!DOCTYPE test SYSTEM 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'><root />");
			} catch (XmlException) {
				Assert.Fail ("#DoctypeDecl.System");
			}
			try {
				doc.LoadXml ("<!DOCTYPE test PUBLIC '-//test' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'><root />");
			} catch (XmlException) {
				Assert.Fail ("#DoctypeDecl.Public");
			}
#endif
			// Should this be commented out?
			doc.LoadXml ("<!DOCTYPE test [<!ELEMENT foo EMPTY>]><test><foo/></test>");
		}

		[Test]
		public void CloneNode ()
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<foo><bar /><baz hoge='fuga'>TEST Text</baz></foo>");
			XmlDocument doc2 = (XmlDocument)doc.CloneNode (false);
			Assert.AreEqual (0, doc2.ChildNodes.Count, "ShallowCopy");
			doc2 = (XmlDocument)doc.CloneNode (true);
			Assert.AreEqual ("foo", doc2.DocumentElement.Name, "DeepCopy");
		}

		[Test]
		public void OuterXmlWithDefaultXmlns ()
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<iq type=\"get\" id=\"ATECLIENT_1\"><query xmlns=\"jabber:iq:auth\"><username></username></query></iq>");
			Assert.AreEqual ("<iq type=\"get\" id=\"ATECLIENT_1\"><query xmlns=\"jabber:iq:auth\"><username></username></query></iq>", doc.OuterXml);
		}

		[Test]
		public void PreserveWhitespace ()
		{
			string input = 
				"<?xml version=\"1.0\" encoding=\"utf-8\" ?><!-- --> <foo/>";

			XmlDocument dom = new XmlDocument ();
			XmlTextReader reader = new XmlTextReader (new StringReader (input));
			dom.Load (reader);

			Assert.AreEqual (XmlNodeType.Element, dom.FirstChild.NextSibling.NextSibling.NodeType);
		}

		[Test]
		public void PreserveWhitespace2 ()
		{
			XmlDocument doc = new XmlDocument ();
			Assert.IsTrue (!doc.PreserveWhitespace);
			doc.PreserveWhitespace = true;
			XmlDocument d2 = doc.Clone () as XmlDocument;
			Assert.IsTrue (!d2.PreserveWhitespace); // i.e. not cloned
			d2.AppendChild (d2.CreateElement ("root"));
			d2.DocumentElement.AppendChild (d2.CreateWhitespace ("   "));
			StringWriter sw = new StringWriter ();
			d2.WriteTo (new XmlTextWriter (sw));
			Assert.AreEqual ("<root>   </root>", sw.ToString ());
		}

		[Test]
		public void CreateAttribute ()
		{
			XmlDocument dom = new XmlDocument ();

			// Check that null prefix and namespace are allowed and
			// equivalent to ""
			XmlAttribute attr = dom.CreateAttribute (null, "FOO", null);
			Assert.AreEqual (attr.Prefix, "");
			Assert.AreEqual (attr.NamespaceURI, "");
		}

		[Test]
		public void DocumentTypeNodes ()
		{
			string entities = "<!ENTITY foo 'foo-ent'>";
			string dtd = "<!DOCTYPE root [<!ELEMENT root (#PCDATA)*> " + entities + "]>";
			string xml = dtd + "<root>&foo;</root>";
			XmlValidatingReader xvr = new XmlValidatingReader (xml, XmlNodeType.Document, null);
			document.Load (xvr);
			Assert.IsNotNull (document.DocumentType);
			Assert.AreEqual (1, document.DocumentType.Entities.Count);

			XmlEntity foo = document.DocumentType.Entities.GetNamedItem ("foo") as XmlEntity;
			Assert.IsNotNull (foo);
			Assert.IsNotNull (document.DocumentType.Entities.GetNamedItem ("foo", ""));
			Assert.AreEqual ("foo", foo.Name);
			Assert.IsNull (foo.Value);
			Assert.AreEqual ("foo-ent", foo.InnerText);
		}

		[Test]
		public void DTDEntityAttributeHandling ()
		{
			string dtd = "<!DOCTYPE root[<!ATTLIST root hoge CDATA 'hoge-def'><!ENTITY foo 'ent-foo'>]>";
			string xml = dtd + "<root>&foo;</root>";
			XmlValidatingReader xvr = new XmlValidatingReader (xml, XmlNodeType.Document,null);
			xvr.EntityHandling = EntityHandling.ExpandCharEntities;
			xvr.ValidationType = ValidationType.None;
			document.Load (xvr);
			// Don't include default attributes here.
			Assert.AreEqual (xml, document.OuterXml);
			Assert.AreEqual ("hoge-def", document.DocumentElement.GetAttribute ("hoge"));
		}

//		[Test]  Comment out in the meantime.
//		public void LoadExternalUri ()
//		{
//			// set any URL of well-formed XML.
//			document.Load ("http://www.go-mono.com/index.rss");
//		}

//		[Test] comment out in the meantime.
//		public void LoadDocumentWithIgnoreSection ()
//		{
//			// set any URL of well-formed XML.
//			document.Load ("xmlfiles/test.xml");
//		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void LoadThrowsUndeclaredEntity ()
		{
			string ent1 = "<!ENTITY ent 'entity string'>";
			string ent2 = "<!ENTITY ent2 '<foo/><foo/>'>]>";
			string dtd = "<!DOCTYPE root[<!ELEMENT root (#PCDATA|foo)*>" + ent1 + ent2;
			string xml = dtd + "<root>&ent3;&ent2;</root>";
			XmlTextReader xtr = new XmlTextReader (xml, XmlNodeType.Document, null);
			document.Load (xtr);
			xtr.Close ();
		}

		[Test]
		public void CreateEntityReferencesWithoutDTD ()
		{
			document.RemoveAll ();
			document.AppendChild (document.CreateElement ("root"));
			document.DocumentElement.AppendChild (document.CreateEntityReference ("foo"));
		}

		[Test]
		public void LoadEntityReference ()
		{
			string xml = "<!DOCTYPE root [<!ELEMENT root (#PCDATA)*><!ENTITY ent 'val'>]><root attr='a &ent; string'>&ent;</root>";
			XmlTextReader xtr = new XmlTextReader (xml, XmlNodeType.Document, null);
			XmlDocument doc = new XmlDocument ();
			doc.Load (xtr);
			Assert.AreEqual (XmlNodeType.EntityReference, doc.DocumentElement.FirstChild.NodeType, "#text node");
			Assert.AreEqual (XmlNodeType.EntityReference, doc.DocumentElement.Attributes [0].ChildNodes [1].NodeType, "#attribute");
		}

		[Test]
		public void ReadNodeEmptyContent ()
		{
			XmlTextReader xr = new XmlTextReader ("", XmlNodeType.Element, null);
			xr.Read ();
			Console.WriteLine (xr.NodeType);
			XmlNode n = document.ReadNode (xr);
			Assert.IsNull (n);
		}

		[Test]
		public void ReadNodeWhitespace ()
		{
			XmlTextReader xr = new XmlTextReader ("  ", XmlNodeType.Element, null);
			xr.Read ();
			Console.WriteLine (xr.NodeType);
			document.PreserveWhitespace = false; // Note this line.
			XmlNode n = document.ReadNode (xr);
			Assert.IsNotNull (n);
			Assert.AreEqual (XmlNodeType.Whitespace, n.NodeType);
		}

		[Test]
		public void SavePreserveWhitespace ()
		{
			string xml = "<root>  <element>text\n</element></root>";
			XmlDocument doc = new XmlDocument ();
			doc.PreserveWhitespace = true;
			doc.LoadXml (xml);
			StringWriter sw = new StringWriter ();
			doc.Save (sw);
			Assert.AreEqual ("<?xml version=\"1.0\" encoding=\"utf-16\"?>" + xml, sw.ToString ());

			doc.PreserveWhitespace = false;
			sw = new StringWriter ();
			doc.Save (sw);
			string NEL = Environment.NewLine;
			Assert.AreEqual ("<?xml version=\"1.0\" encoding=\"utf-16\"?>"
				+ NEL + "<root>  <element>text" 
				+ "\n</element></root>",
				sw.ToString ());
		}

		[Test]
		public void ReadNodeEntityReferenceFillsChildren ()
		{
			string dtd = "<!DOCTYPE root [<!ELEMENT root (#PCDATA)*><!ENTITY ent 'val'>]>";
			
			string xml = dtd + "<root attr='a &ent; string'>&ent;</root>";
			XmlValidatingReader reader = new XmlValidatingReader (
				xml, XmlNodeType.Document, null);

			reader.EntityHandling = EntityHandling.ExpandCharEntities;
			reader.ValidationType = ValidationType.None;

			//skip the doctype delcaration
			reader.Read ();
			reader.Read ();

			XmlDocument doc = new XmlDocument ();
			doc.Load (reader);

			Assert.AreEqual (1,
				doc.DocumentElement.FirstChild.ChildNodes.Count);
		}

		[Test]
		public void LoadTreatsFixedAttributesAsIfItExisted ()
		{
			string xml = @"<!DOCTYPE foo [<!ELEMENT foo EMPTY><!ATTLIST foo xmlns CDATA #FIXED 'urn:foo'>]><foo />";
			XmlDocument doc = new XmlDocument ();
			doc.Load (new StringReader (xml));
			Assert.AreEqual ("urn:foo", doc.DocumentElement.NamespaceURI);
		}

		[Test]
		public void Bug79468 () // XmlNameEntryCache bug
		{
			string xml = "<?xml version='1.0' encoding='UTF-8'?>"
				+ "<ns0:DebtAmountRequest xmlns:ns0='http://whatever'>"
				+ "  <Signature xmlns='http://www.w3.org/2000/09/xmldsig#' />"
				+ "</ns0:DebtAmountRequest>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (xml);
			XmlNodeList nodeList = doc.GetElementsByTagName ("Signature");
		}

		class MyXmlDocument : XmlDocument
		{
			public override XmlAttribute CreateAttribute (string p, string l, string n)
			{
				return base.CreateAttribute (p, "hijacked", n);
			}
		}

		[Test]
		public void UseOverridenCreateAttribute ()
		{
			XmlDocument doc = new MyXmlDocument ();
			doc.LoadXml ("<root a='sane' />");
			Assert.IsNotNull (doc.DocumentElement.GetAttributeNode ("hijacked"));
			Assert.IsNull (doc.DocumentElement.GetAttributeNode ("a"));
		}

		[Test]
		public void LoadFromMiddleOfDocument ()
		{
			// bug #598953
			string xml = @"<?xml version='1.0' encoding='utf-8' ?>
<Racal>
  <Ports>
    <ConsolePort value='9998' />
  </Ports>
</Racal>";
			var r = new XmlTextReader (new StringReader (xml));
			r.WhitespaceHandling = WhitespaceHandling.All;
			r.MoveToContent ();
			r.Read ();
			var doc = new XmlDocument ();
			doc.Load (r);
			Assert.AreEqual (XmlNodeType.EndElement, r.NodeType, "#1");
		}
	}
}
