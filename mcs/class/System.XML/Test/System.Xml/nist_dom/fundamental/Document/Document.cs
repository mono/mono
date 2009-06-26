//**************************************************************************
//
//
//                       National Institute Of Standards and Technology
//                                     DTS Version 1.0
//         
//                                   Document Interface
//
// Written by: Carmelo Montanez
// Modified by:  Mary Brady
//
// Ported to System.Xml by: Mizrahi Rafael rafim@mainsoft.com
// Mainsoft Corporation (c) 2003-2004
//**************************************************************************
using System;
using System.Xml;

using nist_dom;
using NUnit.Framework;

namespace nist_dom.fundamental
{
    [TestFixture]
    public class DocumentTest
    {
        public static int i = 2;
/*
        public testResults[] RunTests()
        {
            testResults[] tests = new testResults[] {core0001D(), core0002D(), core0003D(), core0004D(),
                                                        core0005D(), core0006D(), core0007D(), core0008D(),
                                                        core0009D(), core0010D(), core0011D(), core0012D(),
                                                        core0013D(), core0014D(), core0015D(), 
                                                        core0019D(), core0020D(),
                                                        core0021D(), core0022D(), core0023D(), core0024D(),
                                                        core0025D()};
  
            return tests;
        }
*/


        //------------------------ test case core-0001T ------------------------
        //
        // Testing feature - The doctype attribute contains the Document Type 
        //                   Declaration associated with this Document. 
        //
        // Testing approach - Retrieve the entire DOM document and invoke its
        //                    doctype attribute.  It should return the Document
        //                    type of this document.  Its document Type name 
        //                    should be equal to "staff".
        //                    
        // Semantic Requirements: 1
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0001D()
        {
            string computedValue = "";
            string expectedValue = "staff";
            System.Xml.XmlDocument testNode = null;

            testResults results = new testResults("Core0001D");

            results.description = "The doctype attribute contains the Document Type "+
                "Declaration associated with this object.";  
            //
            // Retrieve the targeted data and access its "doctype" attribute.
            //
            testNode = util.getDOMDocument(); 
            System.Xml.XmlDocumentType dtype = testNode.DocumentType;
            computedValue = dtype.Name;

            //
            // Write out results. 
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0001D --------------------------
        //
        //-------------------------- test case core-0002D ----------------------------
        //
        // Testing feature - The doctype attribute returns null for HTML documents.
        //
        // Testing approach - Retrieve the an HTML DOM document and invoke its
        //                    doctype attribute.  It should return null. 
        //
        // Semantic Requirements: 2 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0002D()
        {
            string testName = "core-0002D";
            object computedValue = null;
            object expectedValue = null;
            System.Xml.XmlDocument testNode = null;

            testResults results = new testResults("Core0002D");

            results.description = "The doctype attribute returns null for HTML "+
                "documents";
            //
            // Retrieve the targeted data and access its "doctype" attribute.
            //
            testNode = util.getDOMHTMLDocument();
            computedValue = (testNode.DocumentType == null).ToString();
            //
            // Write out results.
            //
            results.expected = (expectedValue == null).ToString();
            results.actual = computedValue.ToString();

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0002D --------------------------
        //
        //-------------------------- test case core-0003D ----------------------------
        //
        // Testing feature - The doctype attribute returns null for XML documents
        //                   without a document type declaration.
        //
        // Testing approach - Retrieve an XML DOM document without a Document 
        //                    Type Declaration and invoke its doctype attribute.  
        //                    It should return null.
        //
        // Semantic Requirements: 2
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0003D()
        {
            object computedValue = null;
            object expectedValue = null;
            System.Xml.XmlDocument testNode = null;

            testResults results = new testResults("Core0003D");

            results.description = "The doctype attribute returns null for XML "+
                " documents without a Document Type Declaration.";
            //
            // Retrieve the targeted data and access its "doctype" attribute.
            //
            testNode = util.getnoDTDXMLDocument();
            computedValue = (testNode.DocumentType == null).ToString();
            //
            // Write out results.
            //
            results.expected = (expectedValue == null).ToString();
            results.actual = computedValue.ToString();

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0003D --------------------------
        //
        //-------------------------- test case core-0004D ----------------------------
        //
        // Testing feature - The implementation attribute contains the 
        //                   DOMImplementation object that handles this document.
        //
        // Testing approach - Retrieve the entire DOM document and invoke its
        //                    "implementation" attribute.  It should return a
        //                    DOMImplementation object whose "hasFeature("XML,"1.0")
        //                    method is invoke and a true value expected.  
        //
        // Semantic Requirements: 3 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0004D()
        {
            string computedValue = "";
            string expectedValue = "True";
            System.Xml.XmlImplementation domImp = null;
            System.Xml.XmlDocument testNode = null;

            testResults results = new testResults("Core0004D"); 

            results.description = "The implementation attribute contains the "+
                "DOMImplementation object that handles this"+
                " document.";
            //
            // Retrieve the targeted data and access its "implementation" attribute.
            //
            testNode = util.getDOMDocument();
            domImp = testNode.Implementation;
            //
            // The "hasFeature" method should return true.
            //
            computedValue = domImp.HasFeature("XML","1.0").ToString(); 
            //
            // Write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0004D --------------------------
        //
        //-------------------------- test case core-0005D ----------------------------
        //
        // Testing feature - The documentElement attribute provides direct access
        //                   to the child node that is the root element of the
        //                   document.
        //
        // Testing approach - Retrieve the entire DOM document and invoke its
        //                    "documentElement" attribute.  It should return an 
        //                    Element node whose "tagName" attribute is "staff". 
        //
        // Semantic Requirements: 4 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0005D()
        {
            string computedValue = "";
            string expectedValue = "staff";
            System.Xml.XmlElement rootNode = null;
            System.Xml.XmlDocument testNode = null;

            testResults results = new testResults("Core0005D");

            results.description = "The documentElement attribute provides direct "+
                "to the root node of the document.";
            //
            // Retrieve the targeted data and access its "documentElement" attribute.
            //
            testNode = util.getDOMDocument();
            rootNode = testNode.DocumentElement;
            //
            // Its tagName should be set to "staff".
            //
            computedValue = rootNode.Name;//tagName;
            //
            // Write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0005D --------------------------
        //
        //-------------------------- test case core-0006D ----------------------------
        //
        // Testing feature - For HTML documents, the documentElement attribute returns
        //                   the Element with the HTML tag.
        //
        // Testing approach - Retrieve an HTML DOM document and invoke its
        //                    "documentElement" attribute.  It should return the 
        //                    Element whose "tagName" is "HTML".
        //
        // Semantic Requirements: 5 
        //
        //----------------------------------------------------------------------------

        [Test]
	[Category ("NotDotNet")] // MS DOM is buggy
	public void core0006D()
        {
            string computedValue = "";
            string expectedValue = "HTML";
            System.Xml.XmlElement rootNode = null;
            System.Xml.XmlDocument testNode = null;

            testResults results = new testResults("Core0006D");

            results.description = "For HTML documents, the documentElement attribute "+
                "returns the element with the HTML tag.";
            //
            // Retrieve the targeted data and access its "documentElement" attribute.
            //
            testNode = util.getDOMHTMLDocument();
            rootNode = testNode.DocumentElement;
            //
            // Its tagName should be set to "HTML".
            //
            computedValue = rootNode.Name;//tagName;
            //
            // Write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0006D --------------------------
        //
        //-------------------------- test case core-0007D ----------------------------
        //
        // Testing feature - The "createElement(tagName)" method creates an Element of
        //                   the type specified. 
        //
        // Testing approach - Retrieve the entire DOM document and invoke its
        //                    "createElement(tagName)" method with tagName="address".
        //                    The method should create an instance of an Element 
        //                    node whose tagName is "address".  The type, value and
        //                    are further checked.
        //
        // Semantic Requirements: 6 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0007D()
        {
            string computedValue = "";
            string expectedValue = "address Element ";
            System.Xml.XmlElement newElement = null;
            System.Xml.XmlDocument testNode = null;

            testResults results = new testResults("Core0007D");

            results.description = "The \"createElement(tagName)\" method creates an "+
                "Element of the specified type.";
            //
            // Retrieve the targeted data and invoke its "createElement" attribute.
            //
            testNode = util.getDOMDocument();
            newElement = testNode.CreateElement("address");
            //
            // Retrieve the characteristics of this new object.
            //
            computedValue = newElement.Name+" ";//tagName
            computedValue += newElement.NodeType +" ";
            computedValue += newElement.Value;
            //
            // Write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            util.resetData();
            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0007D --------------------------
        //
        //-------------------------- test case core-0008D ----------------------------
        //
        // Testing feature - The tagName parameter in the "createElement(tagName)"
        //                   method is case-sensitive for XML documents.
        //
        // Testing approach - Retrieve the entire DOM document and invoke its
        //                    "createElement(tagName)" method twice for tagName 
        //                    equal "address" and "ADDRESS".  Each call should
        //                    create two distinct Element nodes.  Each Element
        //                    is in turn assigned an attribute and then that 
        //                    attribute is retrieved. 
        //
        // Semantic Requirements: 7 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0008D()
        {
            string computedValue = "";
            string expectedValue = "Fort Worth Dallas";
            System.Xml.XmlElement newElement1 = null;
            System.Xml.XmlElement newElement2 = null;
            System.Xml.XmlDocument testNode = null;

            testResults results = new testResults("Core0008D");

            results.description = "The tagName parameter in the \"createElement( "+
                "tagName)\" method is case-sensitive for XML "+
                "documents.";
            //
            // Retrieve the targeted data and invoke its "createElement" method.
            //
            testNode = util.getDOMDocument();
            newElement1 = testNode.CreateElement("ADDRESS");
            newElement2 = testNode.CreateElement("address");
            //
            // Assign attributes for each one of the created Elements.
            //
            newElement1.SetAttribute("district","Fort Worth");
            newElement2.SetAttribute("county","Dallas");
            //
            // Now retrieve the values of each Element's attribute.
            //
            computedValue += newElement1.GetAttribute("district")+" ";
            computedValue += newElement2.GetAttribute("county");
            //
            // Write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            util.resetData();
            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0008D --------------------------
        //
        //-------------------------- test case core-0009D ----------------------------
        //
        // Testing feature - The "createDocumentFragment()" method creates an 
        //                   empty DocumentFragment object.
        //
        // Testing approach - Retrieve the entire DOM document and invoke its
        //                    createDocumentFragment() method.  The content, name,
        //                    type and value of the newly created object are
        //                    further retrieved and checked.
        //
        // Semantic Requirements: 8 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0009D()
        {
            string computedValue = "";
            string expectedValue = "0 #document-fragment DocumentFragment ";//"0 #document-fragment 11 null";
            System.Xml.XmlDocumentFragment newDocFragment = null;
            System.Xml.XmlDocument testNode = null;

            testResults results = new testResults("Core0009D");

            results.description = "The \"createDocumentFragment()\" method creates "+
                "an empty DocumentFragment object.";
            //
            // Retrieve the targeted data and invoke its "createDocumentFragment()" 
            // method.
            //
            testNode = util.getDOMDocument();
            newDocFragment = testNode.CreateDocumentFragment();
            //
            // Retrieve the characterstics of the newly created object.
            //
            computedValue += newDocFragment.ChildNodes.Count +" ";
            computedValue += newDocFragment.Name+" ";
            computedValue += newDocFragment.NodeType+" ";
            computedValue += newDocFragment.Value;
            //
            // Write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            util.resetData();
            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0009D --------------------------
        //
        //-------------------------- test case core-0010D ----------------------------
        //
        // Testing feature - The "createTextNode(data)" method creates a Text node
        //                   given by the specified string. 
        //
        // Testing approach - Retrieve the entire DOM document and invoke its 
        //                    "createTextNode(data)" method.  It should create a
        //                    new Text node whose data is the specified string.  The
        //                    name and type of the newly created object are further
        //                    retrieved and checked.
        //
        // Semantic Requirements: 9 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0010D()
        {
            string computedValue = "";
            string expectedValue = "This is a new Text node #text Text";//"This is a new Text node #text 3";
            System.Xml.XmlText newTextNode = null;
            System.Xml.XmlDocument testNode = null;

            testResults results = new testResults("Core0010D");

            results.description = "The \"createTextNode(data)\" method creates "+
                "a Text node given by the specified string.";
            //
            // Retrieve the targeted data and invoke its "createTextNode(data)" method.
            //
            testNode = util.getDOMDocument();
            newTextNode = testNode.CreateTextNode("This is a new Text node");
            //
            // Retrieve the characteristics of the newly created object.
            //
            computedValue += newTextNode.Data+" ";
            computedValue += newTextNode.Name+" ";
            computedValue += newTextNode.NodeType;
            //
            // Write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;
    
            util.resetData();
            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0010D --------------------------
        //
        //-------------------------- test case core-0011D ----------------------------
        //
        // Testing feature - The "createComment(data)" method creates a new Comment 
        //                   node given the specified string.
        //
        // Testing approach - Retrieve the entire DOM document and invoke its
        //                    "createComment(data)" method.  It should create a
        //                    new Comment node whose data is the specified string. 
        //                    The content, name and type of the newly created 
        //                    object are further retrieved and examined.
        //
        // Semantic Requirements: 10
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0011D()
        {
            string computedValue = "";
            string expectedValue = "This is a new Comment node #comment Comment";//"This is a new Comment node #comment 8";
            System.Xml.XmlComment newCommentNode = null;
            System.Xml.XmlDocument testNode = null;

            testResults results = new testResults("Core0011D");

            results.description = "The \"createComment(data)\" method creates "+
                "a new comment node given by the specified string.";
            //
            // Retrieve the targeted data and invoke its "createComment(data)" method.
            //
            testNode = util.getDOMDocument();
            newCommentNode = testNode.CreateComment("This is a new Comment node");
            //
            // Retrieve the characteristics of the new object.
            //
            computedValue += newCommentNode.Data+" ";
            computedValue += newCommentNode.Name+" ";
            computedValue += newCommentNode.NodeType;
            //
            // Write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            util.resetData();
            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0011D --------------------------
        //
        //-------------------------- test case core-0012D ----------------------------
        //
        // Testing feature - The "createCDATASection(data)" method creates a new 
        //                   CDATASection node whose value is the specified string.
        //
        // Testing approach - Retrieve the entire DOM document and invoke its
        //                    "createCDATASection(data)" method.  It should create a
        //                    new CDATASection node whose data is the specified string.
        //                    The content, name and type of the newly created
        //                    object are further retrieved and examined.
        //
        // Semantic Requirements: 11
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0012D()
        {
            string computedValue = "";
            string expectedValue = "This is a new CDATASection node #cdata-section CDATA";//"This is a new CDATASection node #cdata-section 4";
            System.Xml.XmlCDataSection newCDATASectionNode = null;
            System.Xml.XmlDocument testNode = null;

            testResults results = new testResults("Core0012D");

            results.description = "The \"createCDATASection(data)\" method creates "+
                "a new CDATASection node whose value is the "+
                "specified string.";
            //
            // Retrieve the targeted data and invoke its "createCDATASection(data)"
            // method.
            //
            testNode = util.getDOMDocument();
            newCDATASectionNode = testNode.CreateCDataSection("This is a new CDATASection node");
            //
            // Retrieve the characteristics of the new object.
            //
            computedValue += newCDATASectionNode.Data+" ";
            computedValue += newCDATASectionNode.Name+" ";
            computedValue += newCDATASectionNode.NodeType;
            //
            // Write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            util.resetData();
            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0012D --------------------------
        //
        //-------------------------- test case core-0013D ----------------------------
        //
        // Testing feature - The "createProcessingInstruction(target,data)" method
        //                   creates a new ProcessingInstruction node with the 
        //                   specified name and data strings.
        //
        // Testing approach - Retrieve the entire DOM document and invoke its
        //                    "createProcessingInstruction(target,data)" method.  It 
        //                    should create a new PI node with the specified target
        //                    and data.  The target, data and type of the newly created
        //                    object are further retrieved and examined.
        //
        // Semantic Requirements: 12
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0013D()
        {
            string computedValue = "";
            string expectedValue = "XML This is a new PI node ProcessingInstruction";//"XML This is a new PI node 7";
            System.Xml.XmlProcessingInstruction newPINode = null;
            System.Xml.XmlDocument testNode = null;

            testResults results = new testResults("Core0013D");

            results.description = "The \"createProcessingInstruction(target,data)\" "+
                "method creates a new processingInstruction node.";
            //
            // Retrieve the targeted data and invoke its 
            // "createProcessingInstruction(target,data)" method.
            //
            testNode = util.getDOMDocument();
            newPINode = testNode.CreateProcessingInstruction("XML","This is a new PI node");
            //
            // Retrieve the characteristics of the new object.
            //
            computedValue += newPINode.Target+" ";
            computedValue += newPINode.Data+" ";
            computedValue += newPINode.NodeType;
            //
            // Write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            util.resetData();
            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0013D --------------------------
        //
        //-------------------------- test case core-0014D ----------------------------
        //
        // Testing feature - The "createAttribute(name)" method creates an Attr 
        //                   node of the given name.
        //
        // Testing approach - Retrieve the entire DOM document and invoke its
        //                    "createAttribute(name)" method.  It should create a 
        //                    new Attr node with the given name.  The name, value 
        //                    and type of the newly created object are further 
        //                    retrieved and examined.
        //
        // Semantic Requirements: 13
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0014D()
        {
            string computedValue = "";
            string expectedValue = "district Attribute";//"district 2";
            System.Xml.XmlAttribute newAttrNode = null;
            System.Xml.XmlDocument testNode = null;

            testResults results = new testResults("Core0014D"); 

            results.description = "The \"createAttribute(name)\" method creates "+
                "a new Attr node of the given name.";
            //
            // Retrieve the targeted data and invoke its "createAttribute(name)"
            // method.
            //
            testNode = util.getDOMDocument();
            newAttrNode = testNode.CreateAttribute("district");
            //
            // Retrieve the characteristics of the new object.
            //
            computedValue += newAttrNode.Name+" ";
            computedValue += newAttrNode.Value;
            computedValue += newAttrNode.NodeType;
            //
            // Write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            util.resetData();
            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0014D --------------------------
        //
        //-------------------------- test case core-0015D ----------------------------
        //
        // Testing feature - The "createEntityReference(name)" method creates an 
        //                   EntityReference node. 
        //
        // Testing approach - Retrieve the entire DOM document and invoke its
        //                    "createEntityReference(name)" method.  It should
        //                    create a new EntityReference node for the Entity
        //                    with the given name.  The name, value and type of 
        //                    the newly created object are further retrieved 
        //                    and examined.
        //
        // Semantic Requirements: 14
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0015D()
        {
            string computedValue = "";
            string expectedValue = "ent1  EntityReference";//"ent1 null 5";
            System.Xml.XmlEntityReference newEntRefNode = null;
            System.Xml.XmlDocument testNode = null;

            testResults results = new testResults("Core0015D");

            results.description = "The \"createEntityReference(name)\" method creates "+
                "a new EntityReference node.";
            //
            // Retrieve the targeted data and invoke its "createEntityReference(name)"
            // method.
            //
            testNode = util.getDOMDocument();
            newEntRefNode = testNode.CreateEntityReference("ent1");
            //
            // Retrieve the characteristics of the new object.
            //
            computedValue += newEntRefNode.Name+" ";
            computedValue += newEntRefNode.Value+" ";
            computedValue += newEntRefNode.NodeType;
            //
            // Write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            util.resetData();
            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0015D --------------------------
        //
        //-------------------------- test case core-0016D ----------------------------
        //
        // Testing feature - The "getElementsByTagName(tagName)" method returns a
        //                   NodeList of all the Elements with a given tagName.
        //
        // Testing approach - Retrieve the entire DOM document and invoke its
        //                    "getElementsByTagName(tagName)" method with tagName
        //                    equal to "name".  The method should return a NodeList
        //                    that contains 5 elements.  
        //
        // Semantic Requirements: 15
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0016D()
        {
            string computedValue = "0";//0;
            string expectedValue = "5";//5;
            System.Xml.XmlDocument testNode = null;

            testResults results = new testResults("Core0016D");

            results.description = "The \"getElementsByTagName(tagName)\" method "+
                "returns a NodeList of all the Elements with a "+
                "given tag name.";
            //
            // Retrieve the targeted data and invoke its "getElementsByTagName(tagName)"
            // method.
            //
            testNode = util.getDOMDocument();
            System.Xml.XmlNodeList elementList = testNode.GetElementsByTagName("name");
            //
            // Retrieve the length of the list.
            //
            computedValue = elementList.Count.ToString();
            //
            // Write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            util.resetData();
            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0016D --------------------------
        //
        //-------------------------- test case core-0017D ----------------------------
        //
        // Testing feature - The "getElementsByTagName(tagName)" method returns a
        //                   NodeList of all the Elements with a given tagName in
        //                   a pre-order traversal of the tree.
        //
        // Testing approach - Retrieve the entire DOM document and invoke its
        //                    "getElementsByTagName(tagName)" method with tagName
        //                    equal to "name".  The method should return a NodeList
        //                    that contains 5 elements.  Further the fourth item in
        //                    the list is retrieved and checked.
        //
        // Semantic Requirements: 16
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0017D()
        {
            string computedValue = "0";//0;
            string expectedValue = "Jeny Oconnor";
            System.Xml.XmlDocument testNode = null;

            testResults results = new testResults("Core0017D");

            results.description = "The \"getElementsByTagName(tagName)\" method "+
                "returns a NodeList of all the Elements with a "+
                "given tag name in a preorder traversal of the tree.";
                //
                // Retrieve the targeted data and invoke its "getElementsByTagName(tagName)"
                // method.
                //
                testNode = util.getDOMDocument();
            System.Xml.XmlNodeList elementList = testNode.GetElementsByTagName("name");
            //
            // Retrieve the fourth item and its data.
            //
            computedValue = elementList.Item(util.FOURTH).FirstChild.Value;//Data;
            //
            // Write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            util.resetData();
            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0017D --------------------------
        //
        //-------------------------- test case core-0018D ----------------------------
        //
        // Testing feature - The "getElementsByTagName(tagName)" method returns a
        //                   NodeList of all the Elements in the tree when the
        //                   tagName is equal to "*".
        //
        // Testing approach - Retrieve the entire DOM document and invoke its
        //                    "getElementsByTagName(tagName)" method with tagName
        //                    equal to "*".  The method should return a NodeList
        //                    that contains 41 elements, which is the total number
        //                    of Elements in the document.
        //
        // Semantic Requirements: 17
        //
        //----------------------------------------------------------------------------


        [Test]
	public void core0018D()
        {
            string computedValue = "0";//0;
	// Mmm, shouldn't the count be 36?
            string expectedValue = "36";//37;
            System.Xml.XmlDocument testNode = null;

            testResults results = new testResults("Core0018D");

            results.description = "The \"getElementsByTagName(tagName)\" method "+
                "returns a NodeList of all the Elements in the "+
                "tree when the tag name is equal to \"*\".";
            //
            // Retrieve the targeted data and invoke its "getElementsByTagName(tagName)"
            // method.
            //
            testNode = util.getDOMDocument();
            System.Xml.XmlNodeList elementList = testNode.GetElementsByTagName("*");
            //
            // Retrieve the length of the list.
            //
            computedValue = elementList.Count.ToString();
            //
            // Write out results.
            //
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            util.resetData();
            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0018D --------------------------
        //
        //------------------------- test case core-0019D -----------------------------
        //
        // Testing feature - The "createElement(tagName)" method raises an
        //                   INVALID_CHARACTER_ERR Exception if the
        //                   specified name contains an invalid character.
        //
        // Testing approach - Retrieve the entire DOM document and invoke its
        //                    "createElement(tagName)" method with the tagName
        //                    equals to the string "invalid^Name" which contains
        //                    an invalid character ("^") in it.  The desired
        //                    exception should be raised.
        //
        // Semantic Requirements: 18 
        //
        //----------------------------------------------------------------------------

        [Test]
	[Category ("NotDotNet")] // MS DOM is buggy
	public void core0019D()
        {
            string computedValue = "";
            System.Xml.XmlDocument testNode = null;
            System.Xml.XmlElement invalidElement = null;
            string expectedValue = util.INVALID_CHARACTER_ERR;

            testResults results = new testResults("Core0019D");

            results.description = "The \"createElement(tagName)\" method raises an "+
                "INVALID_CHARACTER_ERR Exception if the "+
                "specified name contains an invalid character."; 
            //
            // Retrieve the targeted data.
            //
            testNode = util.getDOMDocument();
            //
            // Attempt to create an Element with an invalid tagName should raise 
            // an exception.
            //
            try 
            {
                invalidElement =  testNode.CreateElement("invalid^Name");
            }
            catch(System.Exception ex)
            {
                computedValue = ex.GetType ().FullName;
            }

            //
            // Write out results.
            //
            results.expected = typeof (XmlException).FullName; // MS.NET BUG: It never raises an error.
            results.actual = computedValue;

            util.resetData();
            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0019D -------------------------
        //
        //------------------------- test case core-0020D -----------------------------
        //
        // Testing feature - The "createAttribute(name)" method raises an
        //                   INVALID_CHARACTER_ERR Exception if the
        //                   specified name contains an invalid character.
        //
        // Testing approach - Retrieve the entire DOM document and invoke its
        //                    "createAttribute(name)" method with the name
        //                    equals to the string "invalid^Name" which contains
        //                    an invalid character ("^") in it.  The desired
        //                    exception should be raised.
        //
        // Semantic Requirements: 19 
        //
        //----------------------------------------------------------------------------

        [Test]
	[Category ("NotDotNet")] // MS DOM is buggy
	public void core0020D()
        {
            string computedValue = "";
            string expectedValue = util.INVALID_CHARACTER_ERR;
            System.Xml.XmlDocument testNode = null;
            System.Xml.XmlAttribute invalidAttr = null;

            testResults results = new testResults("Core0020D");

            results.description = "The \"createAttribute(name)\" method raises an "+
                "INVALID_CHARACTER_ERR Exception if the "+
                "specified name contains an invalid character.";
            //
            // Retrieve the targeted data.
            //
            testNode = util.getDOMDocument();
            //
            // Attempt to create an Attr node with an invalid name should raise
            // an exception.
            //
            try 
            {
                invalidAttr =  testNode.CreateAttribute("invalid^Name");
            }
            catch(System.Exception ex)
            {
                computedValue = ex.GetType ().FullName;
            }

            //
            // Write out results.
            //
            results.expected = typeof (ArgumentException).FullName; // MS.NET BUG: It never raises an error.
            results.actual = computedValue;

            util.resetData();
            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0020D -------------------------
        //
        //------------------------- test case core-0021D -----------------------------
        //
        // Testing feature - The "createEntityReference(name)" method raises an
        //                   INVALID_CHARACTER_ERR Exception if the
        //                   specified name contains an invalid character.
        //
        // Testing approach - Retrieve the entire DOM document and invoke its
        //                    "createEntityReference(name)" method with the name
        //                    equals to the string "invalid^Name" which contains
        //                    an invalid character ("^") in it.  The desired
        //                    exception should be raised.
        //
        // Semantic Requirements: 20
        //
        //----------------------------------------------------------------------------

        [Test]
	[Category ("NotDotNet")] // MS DOM is buggy
	public void core0021D()
        {
            string computedValue = "";
            string expectedValue = "System.Xml.XmlException";//util.INVALID_CHARACTER_ERR;
            System.Xml.XmlDocument testNode = null;
            System.Xml.XmlEntityReference invalidEntRef = null;

            testResults results = new testResults("Core0021D");

            results.description = "The \"createEntityReference(name)\" method raises "+
                "an INVALID_CHARACTER_ERR Exception if the "+
                "specified name contains an invalid character.";
            //
            // Retrieve the targeted data.
            //
            testNode = util.getDOMDocument();
            //
            // Attempt to create an EntityReference node with an invalid name should 
            // raise an exception.
            //
            try 
            {
                invalidEntRef =  testNode.CreateEntityReference("invalid^Name");
            }
            catch(XmlException ex)
            {
                computedValue = ex.GetType ().FullName;
            }
            //
            // Write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;
    
            util.resetData();
            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0021D -------------------------
        //
        //------------------------- test case core-0022D ----------------------------
        //
        // Testing feature - The "createProcessingInstruction(target,data)" method
        //                   raises an INVALID_CHARACTER_ERR Exception if an
        //                   invalid character was specified. (test for invalid
        //                   target) 
        //
        // Testing approach - Retrieve the entire DOM document and invoke its
        //                    "createProcessingInstruction(target,data)" method with 
        //                    the target equals to the string "invalid^target" which
        //                    contains an invalid character ("^") in it.  The desired
        //                    exception should be raised.
        //
        // Semantic Requirements: 21
        //
        //----------------------------------------------------------------------------

        [Test]
	[Category ("NotDotNet")] // MS DOM is buggy
	public void core0022D()
        {
            string computedValue = "";
            string expectedValue = "System.Xml.XmlException";//util.INVALID_CHARACTER_ERR;
            System.Xml.XmlDocument testNode = null;
            System.Xml.XmlProcessingInstruction invalidPI = null;

            testResults results = new testResults("Core0022D"); 

            results.description = "The \"createProcessingInstruction(target,data)\" "+
                "method raises an INVALID_CHARACTER_ERR "+
                "DOMException if an invalid character was specified "+                          "(invalid target).";
            //
            // Retrieve the targeted data.
            //
            testNode = util.getDOMDocument();
            //
            // Attempt to create a ProcessingInstruction node with an invalid
            // target name should raise an exception.
            //
            try 
            {
                invalidPI =  testNode.CreateProcessingInstruction("invalid^target","data");
            }
            catch(XmlException ex)
            {
                computedValue = ex.GetType ().FullName;
            }

            //
            // Write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            util.resetData();
            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0022D -------------------------
        //
        //------------------------- test case core-0023D ----------------------------
        //
        // Testing feature - The "createCDATASection(data)" method raises a
        //                   NOT_SUPPORTED_ERR Exception if this is an
        //                   HTML document.
        //
        // Testing approach - Retrieve an HTML based DOM document and invoke its
        //                    "createCDATASection(data)" method.  Since this DOM
        //                    document was based on an HTML document, the desired
        //                    exception should be raised.
        //
		// System.Xml       -  Microsoft System.Xml does not supporting this requirement
		//
        // Semantic Requirements: 22
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0023D()
        {
            string computedValue = "";
            string expectedValue = "";//util.NOT_SUPPORTED_ERR;
            System.Xml.XmlDocument testNode = null;
            System.Xml.XmlCDataSection invalidCData = null;

            testResults results = new testResults("Core0023D");

            results.description = "The \"createCDATASection(data)\" method raises "+
                "a NOT_SUPPORTED_ERR Exception if this is "+
                "an HTML document.";
            //
            // Retrieve the targeted data.
            //
            testNode = util.getDOMHTMLDocument();
            //
            // Attempt to create a CDATASection node for an HTML based DOM Document 
            // should raise an exception.
            //
            try 
            {
                invalidCData =  testNode.CreateCDataSection("This is a new CDATA Section");
            }
            catch(System.Exception ex)
            {
                computedValue = ex.Message;
            }
            //
            // Write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            util.resetData();
            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0023D -------------------------
        //
        //------------------------- test case core-0024D ----------------------------
        //
        // Testing feature - The "createProcessingInstruction(target,data)" method
        //                   raises a NOT_SUPPORTED_ERR Exception if this is an
        //                   HTML document.
        //
        // Testing approach - Retrieve an HTML based DOM document and invoke its
        //                    "createProcessingInstruction(target,data)" method. 
        //                    Since this DOM document was based on an HTML document, 
        //                    the desired exception should be raised.
        //
		// System.Xml       -  Microsoft System.Xml does not supporting this requirement
        // Semantic Requirements: 23
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0024D()
        {
            string computedValue = "";
            string expectedValue = "";//util.NOT_SUPPORTED_ERR;
            System.Xml.XmlDocument testNode = null;
            System.Xml.XmlProcessingInstruction invalidPI = null;

            testResults results = new testResults("Core0024D");

            results.description = "The \"createProcessingInstruction(target,data)\" "+
                "method raises a NOT_SUPPORTED_ERR Exception "+
                "if this is an HTML document.";
            //
            // Retrieve the targeted data.
            //
            testNode = util.getDOMHTMLDocument();
            //
            // Attempt to create a ProcessingInstruction node for an HTML based DOM
            // Document should raise an exception.
            //
            try 
            {
                invalidPI =  testNode.CreateProcessingInstruction("XML","This is a new PI node"); 
            }
            catch(System.Exception ex)
            {
                computedValue = ex.Message;
            }
            //
            // Write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            util.resetData();
            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0024D -------------------------
        //
        //------------------------- test case core-0025D ----------------------------
        //
        // Testing feature - The "createEntityReference(data)" method raises
        //                   a NOT_SUPPORTED_ERR Exception if this is an
        //                   HTML document.
        //
        // Testing approach - Retrieve an HTML based DOM document and invoke its
        //                    "createEntityReference(name)" method.  Since this DOM 
        //                    document was based on an HTML document, the desired 
        //                    exception should be raised.
        //
		// System.Xml       -  Microsoft System.Xml does not supporting this requirement
		//
        // Semantic Requirements: 24
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0025D()
        {
            string computedValue = "";
            string expectedValue = "";//util.NOT_SUPPORTED_ERR;
            System.Xml.XmlDocument testNode = null;
            System.Xml.XmlEntityReference invalidEntRef = null;

            testResults results = new testResults("Core0025D");

            results.description = "The \"createEntityReference(name)\" method raises "+
                "a NOT_SUPPORTED_ERR Exception if this is an "+
                "HTML document.";
            //
            // Retrieve the targeted data.
            //
            testNode = util.getDOMHTMLDocument();
            //
            // Attempt to create an EntityReference node for an HTML based DOM
            // Document should raise an exception.
            //
            try 
            {
                invalidEntRef =  testNode.CreateEntityReference("ent1");
            }
            catch(System.Exception ex)
            {
                computedValue = ex.GetType().ToString();

            }
            //
            // Write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            util.resetData();
            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0025D -------------------------
    }
}