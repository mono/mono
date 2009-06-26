//**************************************************************************
//
//
//                       National Institute Of Standards and Technology
//                                     DTS Version 1.0
//         
//                                     Node Interface
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
    public class NodeTest
    {
        public static int i = 2;
/*
        public testResults[] RunTests()
        {
            testResults[] tests = new testResults[] {core0001NO(), core0002NO(), core0003NO(),core0004NO(),
                                                        core0005NO(), core0006NO(), core0007NO(), core0008NO(),
                                                        core0009NO(), core0010NO(), core0011NO(), core0012NO(),
                                                        core0013NO(), core0014NO(), core0015NO(), core0016NO(),
                                                        core0017NO(), core0018NO(), core0019NO(), core0020NO(),
                                                        core0021NO(), core0022NO(), core0023NO(), core0024NO(),
                                                        core0025NO(), core0026NO(), core0027NO(), core0028NO(),
                                                        core0029NO(), core0030NO(), core0031NO(), core0032NO(),
                                                        core0033NO(), core0034NO(), core0035NO(), core0036NO(),
                                                        core0038NO(), core0039NO(), core0040NO(),
                                                        core0041NO(), core0042NO(), core0043NO(), core0044NO(),
                                                        core0045NO(), core0046NO(), core0047NO(), core0048NO(),
                                                        core0049NO(), core0050NO(), core0051NO(), core0052NO(),
                                                        core0053NO(), core0054NO(), core0055NO(), core0056NO(),
                                                        core0057NO(), core0058NO(), core0059NO(), core0060NO(),
                                                        core0061NO(), core0062NO(), core0063NO(), core0064NO(),
                                                        core0065NO(), core0066NO(), core0067NO(), core0068NO(),
                                                        core0069NO(), core0070NO(), core0071NO(), core0072NO(),
                                                        core0073NO(), core0074NO(), core0075NO(), core0076NO(),
                                                        core0077NO(), core0078NO(), core0079NO(), core0080NO(),
                                                        core0081NO(), core0082NO(), core0083NO(), core0084NO(),
                                                        core0085NO(), core0087NO(), core0088NO(),
                                                        core0089NO(), core0090NO(), core0091NO(), core0092NO(),
                                                        core0093NO(), core0094NO(), core0095NO(), core0096NO(),
                                                        core0097NO(), core0098NO(), core0099NO(), core0100NO(),
                                                        core0101NO(), core0102NO(), core0103NO()};
  
            return tests;
        }
*/

        //------------------------ test case core-0001NO------------------------
        //
        // Testing feature - The "nodeType" attribute for an Element node is 
        //                   1 (ELEMENT_NODE). 
        //
        // Testing approach - Retrieve the root node and check its "nodeType" 
        //                    attribute.  It should be set to 1. 
        //
        // Semantic Requirements: 1, 14
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0001NO()
        {
            int computedValue = 0;
            int expectedValue = util.ELEMENT_NODE;

            testResults results = new testResults("Core0001NO");

            results.description = "The nodeType attribute for an Element Node "+
                " should be set to the constant 1.";
            //
            // The nodeType attribute for the root node should be set to the value 1.
            //
            computedValue = (int)util.getRootNode().NodeType;
            //
            // write out results.
            //
            results.expected = expectedValue.ToString();
            results.actual = computedValue.ToString();
            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0001NO --------------------------
        //
        //------------------------- test case core-0002NO -----------------------------
        //
        // Testing feature - The "nodeType" attribute for an Attribute node is
        //                   2 (ATTRIBUTE_NODE).
        //
        // Testing approach - Retrieve the first attribute from the last child of
        //                    the first employee.  Its "nodeType" attribute is then 
        //                    checked, it should be set to 2.
        //
        // Semantic Requirements: 2, 14 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0002NO()
        {
            string computedValue = "";
            System.Xml.XmlElement testNode = null;
            System.Xml.XmlAttribute attrNode = null;
            string expectedValue = util.ATTRIBUTE_NODE.ToString();

            testResults results = new testResults("Core0002NO");
            try
            {
                results.description = "The nodeType attribute for an Attribute Node "+
                    " should be set to the constant 2.";
                // 
                // Retrieve the targeted data and its type.
                //
                testNode = (System.Xml.XmlElement)util.nodeObject(util.FIRST,util.SIXTH);
                attrNode = testNode.GetAttributeNode("domestic");//.node.
                computedValue = ((int)attrNode.NodeType).ToString();
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }
            //
            // write out results.
            //
            results.expected = expectedValue.ToString();
            results.actual = computedValue;

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0002NO --------------------------
        //
        //------------------------- test case core-0003NO -----------------------------
        //
        // Testing feature - The "nodeType" attribute for a Text node is
        //                   3 (TEXT_NODE).
        //
        // Testing approach - Retrieve the Text data from the last child of the 
        //                    first employee and and examine its "nodeType" 
        //                    attribute.  It should be set to 3.
        //
        // Semantic Requirements: 3, 14 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0003NO()
        {
            string computedValue = "";
            System.Xml.XmlNode testNode = null;
            System.Xml.XmlNode textNode = null;
            string expectedValue = util.TEXT_NODE.ToString();

            testResults results = new testResults("Core0003NO");
            try
            {
                results.description = "The nodeType attribute for a Text Node "+
                    " should be set to the constant 3.";

                //
                // Retrieve the targeted data.
                //
                testNode = util.nodeObject(util.FIRST,util.SIXTH);
                textNode = testNode.FirstChild;//.node.
                //
                // The nodeType attribute should be set to the value 3.
                //
                computedValue = ((int)textNode.NodeType).ToString();
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }
            //
            // write out results.
            //
            results.expected = expectedValue.ToString();
            results.actual = computedValue;

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0003NO --------------------------
        //
        //------------------------- test case core-0004NO -----------------------------
        //
        // Testing feature - The "nodeType" attribute for a CDATASection node is
        //                   4 (CDATA_SECTION_NODE).
        //
        // Testing approach - Retrieve the CDATASection node contained inside
        //                    the second child of the second employee and
        //                    examine its "nodeType" attribute.  It should be 
        //                    set to 4.
        //
        // Semantic Requirements: 4, 14 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0004NO()
        {
            string computedValue = "";
            System.Xml.XmlNode testNode = null;
            System.Xml.XmlNode cDataNode = null;
            string expectedValue = util.CDATA_SECTION_NODE.ToString();

            testResults results = new testResults("Core0004NO");
            try
            {
                results.description = "The nodeType attribute for a CDATASection Node "+
                    " should be set to the constant 4.";
                //
                // Retrieve the targeted data.
                //
                testNode = util.nodeObject(util.SECOND,util.SECOND);
                cDataNode = testNode.LastChild; //.node.
                //
                // The nodeType attribute should be set to the value 3.
                //
                computedValue = ((int)cDataNode.NodeType).ToString();
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }
            //
            // write out results.
            //
            results.expected = expectedValue.ToString();
            results.actual = computedValue.ToString();

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0004NO --------------------------
        //
        //------------------------- test case core-0005NO -----------------------------
        //
        // Testing feature - The "nodeType" attribute for an EntityReference node 
        //                   is 5 (ENTITY_REFERENCE_NODE).
        //
        // Testing approach - Retrieve the first Entity Reference node from the 
        //                    last child of the second employee and examine its 
        //                    "nodeType" attribute.  It should be set to 5.
        //
        // Semantic Requirements:  5, 14 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0005NO()
        {
            string computedValue = "";
            System.Xml.XmlNode testNode = null;
            System.Xml.XmlNode entRefNode = null;
            string expectedValue = XmlNodeType.EntityReference.ToString ();//util.ENTITY_REFERENCE_NODE;

            testResults results = new testResults("Core0005NO");
            try
            {
                results.description = "The nodeType attribute for an EntityReference Node "+
                    " should be set to the constant 5.";

                //
                // Retrieve the targeted data.
                //
                testNode = util.nodeObject(util.SECOND,util.SIXTH);
                entRefNode = testNode.FirstChild;//.node.
                //
                // The nodeType attribute should be set to the value 5.
                //
                computedValue = entRefNode.NodeType.ToString();
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }
            //
            // write out results.
            //
            results.expected = expectedValue.ToString();
            results.actual = computedValue.ToString();

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0005NO --------------------------
        //
        //------------------------- test case core-0006NO -----------------------------
        //
        // Testing feature - The "nodeType" attribute for an Entity node 
        //                   6 (ENTITY_NODE).
        //
        // Testing approach - Retrieve the first Entity declaration in the
        //                    "DOCTYPE" section of the XML file and examine
        //                    its "nodeType" attribute.  It should be set to 6.
        //
        // Semantic Requirements: 6, 14 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0006NO()
        {
            int computedValue = 0;
            System.Xml.XmlNode testNode = null;
            int expectedValue = util.ENTITY_NODE;

            testResults results = new testResults("Core0006NO");
            results.description = "The nodeType attribute for an Entity Node "+
                " should be set to the constant 6.";
            //
            // Get the targeted data and its type.
            //
            testNode = util.getEntity("ent1");
            //
            // The nodeType attribute should be set to the value 6.
            //
            computedValue = (int)testNode.NodeType;

            //
            // write out results.
            //
            results.expected = expectedValue.ToString();
            results.actual = computedValue.ToString();

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0006NO --------------------------
        //
        //------------------------- test case core-0007NO -----------------------------
        //
        // Testing feature - The "nodeType" attribute for a ProcessingInstruction.
        //
        // Testing approach - Retrieve the first declaration in the XML file
        //                    and examine its "nodeType" attribute.  It should 
        //                    be set to ProcessingInstruction.
        //
        // Semantic Requirements: 7, 14 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0007NO()
        {
            string computedValue = "";
            System.Xml.XmlNode testNode = null;
            string expectedValue =  util.XML_DECLARATION_NODE.ToString(); //util.PROCESSING_INSTRUCTION_NODE.ToString();

            testResults results = new testResults("Core0007NO");
            results.description = "The nodeType attribute for an ProcessingInstruction Node.";
            //
            //  Get the targeted data.
            //
            testNode = util.getDOMDocument().ChildNodes.Item(0);
            //
            // The nodeType attribute should be set to the value 7.
            //
            computedValue = ((int)testNode.NodeType).ToString();
            //
            // write out results.
            //
            results.expected = expectedValue.ToString();
            results.actual = computedValue.ToString();

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0007NO --------------------------
        //
        //------------------------- test case core-0008NO -----------------------------
        //
        // Testing feature - The "nodeType" attribute for a comment node is
        //                   8 (COMMENT_NODE).
        //
        // Testing approach - Retrieve the only comment (third child) from the
        //                    main DOM document and examine its "nodeType" attribute.  
        //                    It should be set to 8.
        //
        // Semantic Requirements: 8, 14 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0008NO()
        {
            int computedValue = 0;
            System.Xml.XmlNode testNode = null;
            int expectedValue = util.COMMENT_NODE;

            testResults results = new testResults("Core0008NO");
            results.description = "The nodeType attribute for a Comment Node "+
                " should be set to the constant 8.";
            //
            // Get the targeted data.
            //
            testNode = util.getDOMDocument().ChildNodes.Item(2);
            //
            // The nodeType attribute should be set to the value 8.
            //
            computedValue = (int)testNode.NodeType;
            //
            // write out results.
            //
            results.expected = expectedValue.ToString();
            results.actual = computedValue.ToString();

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0008NO --------------------------
        //
        //------------------------- test case core-0009NO -----------------------------
        //
        // Testing feature - The "nodeType" attribute for a Document node is
        //                   9 (DOCUMENT_NODE).
        //
        // Testing approach - Retrieve the DOM Document and examine its 
        //                    "nodeType" attribute.  It should be set to 9.
        //
        // Semantic Requirements: 9, 14 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0009NO()
        {
            int computedValue = 0;
            System.Xml.XmlNode testNode = null;
            int expectedValue = util.DOCUMENT_NODE;

            testResults results = new testResults("Core0009NO");
            results.description = "The nodeType attribute for an Document Node "+
                " should be set to the constant 9.";
            //
            // Get the targeted data.
            //
            testNode = util.getDOMDocument();
            //
            // The nodeType attribute should be set to the value 9.
            //
            computedValue = (int)testNode.NodeType;
            //
            // write out results.
            //
            results.expected = expectedValue.ToString();
            results.actual = computedValue.ToString();

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0009NO --------------------------
        //
        //------------------------- test case core-0010NO -----------------------------
        //
        // Testing feature - The "nodeType" attribute for a DocumentType node is
        //                   10 (DOCUMENT_TYPE_NODE).
        //
        // Testing approach - Retrieve the DOCTYPE declaration (second child) from
        //                    the XML file and examine its "nodeType" attribute. 
        //                    It should be set to 10.
        //
        // Semantic Requirements: 10, 14 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0010NO()
        {
            int computedValue = 0;
            System.Xml.XmlNode testNode = null;
            int expectedValue = util.DOCUMENT_TYPE_NODE;

            testResults results = new testResults("Core0010NO");
            results.description = "The nodeType attribute for an DocumentType Node "+
                " should be set to the constant 10.";
            //
            // Get the targeted data.
            //
            testNode = util.getDocType();
            //
            // The nodeType attribute should be set to the value 10.
            //
            computedValue = (int)testNode.NodeType;
            //
            // write out results.
            //
            results.expected = expectedValue.ToString();
            results.actual = computedValue.ToString();

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0010NO --------------------------
        //
        //------------------------- test case core-0011NO -----------------------------
        //
        // Testing feature - The "nodeType" attribute for a DocumentFragment node
        //                   is 11 (DOCUMENT_FRAGMENT_NODE).
        //
        // Testing approach - Retrieve the whole DOM document and invoke its
        //                    "createDocumentFragment()" method and examine the
        //                    "nodeType" attribute of the returned node.  It should 
        //                    be set to 11.
        //
        // Semantic Requirements: 11, 14
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0011NO()
        {
            int computedValue = 0;
            System.Xml.XmlNode testNode = null;
            int expectedValue = util.DOCUMENT_FRAGMENT_NODE;

            testResults results = new testResults("Core0011NO");
            results.description = "The nodeType attribute for a DocumentFragment Node "+
                " should be set to the constant 11.";
            //
            // Get the targeted data.
            //
            testNode = util.getDOMDocument().CreateDocumentFragment();
            //
            // The nodeType attribute should be set to the value 11.
            //
            computedValue = (int)testNode.NodeType;
            //
            // write out results.
            //
            results.expected = expectedValue.ToString();
            results.actual = computedValue.ToString();

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0011NO --------------------------
        //
        //------------------------- test case core-0012NO -----------------------------
        //
        // Testing feature - The "nodeType" attribute for a notation node is
        //                   12 (NOTATION_NODE).
        //
        // Testing approach - Retrieve the Notation declaration inside the 
        //                    DocumentType node and examine its "nodeType"
        //                    attribute.  It should be set to 12.
        //
        // Semantic Requirements: 12, 14
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0012NO()
        {
            string computedValue = "";
            System.Xml.XmlNode testNode = null;
            string expectedValue = util.NOTATION_NODE.ToString();

            testResults results = new testResults("Core0012NO");
            try
            {
                results.description = "The nodeType attribute for a Notation Node "+
                    " should be set to the constant 12.";
                //
                // Get the targeted data.
                //
                testNode = util.getNotation("notation1");
                //
                // The nodeType attribute should be set to the value 12.
                //
                computedValue = ((int)testNode.NodeType).ToString();
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }
            //
            // write out results.
            //
            results.expected = expectedValue.ToString();
            results.actual = computedValue.ToString();

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0012NO --------------------------
        //
        //------------------------   test case core-0013NO ----------------------------
        //
        // Testing feature - The "nodeName" attribute for an Element node is
        //                   its tagName.
        //
        // Testing approach - Retrieve the first Element node (root node) of the 
        //                    DOM object and check its "nodeName" attribute.  
        //                    It should be equal to its tagName.
        //
        // Semantic Requirements: 13, 15 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0013NO()
        {
            string computedValue = "0";
            string expectedValue = "staff"; 

            testResults results = new testResults("Core0013NO");
            results.description = "The nodeName attribute for an Element Node " +
                "should be set to its tagName.";
            //
            // The nodeName attribute should be set to "staff". 
            //
            computedValue = util.getRootNode().Name;
            //
            // write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0013NO --------------------------
        //
        //------------------------- test case core-0014NO -----------------------------
        //
        // Testing feature - The "nodeName" attribute for an Attribute node is
        //                   the name of the attribute.
        //
        // Testing approach - Retrieve the attribute named "domestic" from the last 
        //                    child of the first employee.  Its "nodeName" attribute 
        //                    is then checked.  It should be set to "domestic".
        //
        // Semantic Requirements: 13, 15  
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0014NO()
        {
            string computedValue = "0";
            string expectedValue = "domestic";
            System.Xml.XmlElement testNode = null;
            System.Xml.XmlAttribute attrNode = null;

            testResults results = new testResults("Core0014NO");
            try
            {
                results.description = "The nodeName attribute for an Attribute Node " +
                    "should be set to the name of the attribute.";
                //
                // Retrieve the targeted data.
                //
                testNode = (System.Xml.XmlElement)util.nodeObject(util.FIRST,util.SIXTH);
                attrNode = testNode.GetAttributeNode("domestic"); //.node.
                //
                // The nodeName attribute should be set to the value "domestic".
                //
                computedValue = attrNode.Name;
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }
            //
            // write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0014NO --------------------------
        //
        //------------------------- test case core-0015NO -----------------------------
        //
        // Testing feature - The "nodeName" attribute for a text node is
        //                   "#text".
        //
        // Testing approach - Retrieve the Text data from the last child of the
        //                    first employee and and examine its "nodeName"
        //                    attribute.  It should be set to "#text".
        //
        // Semantic Requirements: 13, 15 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0015NO()
        {
            string computedValue = "0";
            string expectedValue = "#text";
            System.Xml.XmlNode testNode = null;
            System.Xml.XmlNode textNode = null;

            testResults results = new testResults("Core0015NO");
            try
            {
                results.description = "The nodeName attribute for a Text Node " +
                    "should be set to \"#text\".";
                //
                // Retrieve the targeted data.
                //
                testNode = util.nodeObject(util.FIRST,util.SIXTH);
                textNode = testNode.FirstChild;//.node.
                //
                // The nodeName attribute should be set to the value "#text".
                //
                computedValue = textNode.Name;
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }
            //
            // write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0015NO --------------------------
        //
        //------------------------- test case core-0016NO -----------------------------
        //
        // Testing feature - The "nodeName" attribute for a CDATASection node is
        //                   "#cdata-section".
        //
        // Testing approach - Retrieve the CDATASection node inside the second 
        //                    child of the second employee and examine its "nodeName"
        //                    attribute.  It should be set to "#cdata-section".
        //
        // Semantic Requirements: 13, 15 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0016NO()
        {
            string computedValue = "0";
            string expectedValue = "#cdata-section";
            System.Xml.XmlNode testNode = null;
            System.Xml.XmlNode cDataNode = null;

            testResults results = new testResults("Core0016NO");
            try
            {
                results.description = "The nodeName attribute for a CDATASection Node " +
                    "should be set to \"#cdata-section\".";
                //
                // Retrieve the targeted data.
                //
                testNode = util.nodeObject(util.SECOND,util.SECOND);  
                cDataNode = testNode.LastChild;;//.node.
                //
                // The nodeName attribute should be set to "#cdata-section".
                //
                computedValue = cDataNode.Name;
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }
            //
            // write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0016NO --------------------------
        //
        //------------------------- test case core-0017NO -----------------------------
        //
        // Testing feature - The "nodeName" attribute for an EntityReference node
        //                   is the name of the entity referenced.
        //
        // Testing approach - Retrieve the first Entity Reference node from the last 
        //                    child of the second employee and examine its
        //                    "nodeName" attribute.  It should be set to "ent2".
        //
        // Semantic Requirements: 13, 15 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0017NO()
        {
            string computedValue = "";
            string expectedValue = "ent2";
            System.Xml.XmlNode testNode = null;
            System.Xml.XmlNode entRefNode = null;

            testResults results = new testResults("Core0017NO");
            try
            {
                results.description = "The nodeName attribute for an EntityReference Node " +
                    "should be set to the name of the entity referenced.";
                //
                // Retrieve the targeted data.
                //
                testNode = util.nodeObject(util.SECOND,util.SIXTH);
                entRefNode = testNode.FirstChild;//.node.
                //
                // The nodeName attribute should be set to "ent2".
                //
                computedValue = entRefNode.Name;
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }
            //
            // write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            Assert.AreEqual (results.expected, results.actual);
        }
        //------------------------ End test case core-0017NO --------------------------
        //
        //------------------------- test case core-0018NO -----------------------------
        //
        // Testing feature - The "nodeName" attribute for an Entity node is
        //                   the entity name.
        //
        // Testing approach - Retrieve the first Entity declaration in the
        //                    "DOCTYPE" section of the XML file and examine
        //                    its "nodeName" attribute.  It should be set to
        //                    "ent1" .
        //
        // Semantic Requirements: 13, 15 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0018NO()
        {
            string computedValue = "";
            string expectedValue = "ent1";
            System.Xml.XmlNode testNode = null;

            testResults results = new testResults("Core0018NO");
            results.description = "The nodeName attribute for an Entity Node " +
                "should be set to the entity name.";
            //
            // Get the targeted data.
            //
            testNode = util.getEntity("ent1");
            //
            // The nodeName attribute should be set to "ent1".
            //
            computedValue = testNode.Name;
            //
            // write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0018NO --------------------------
        //
        //------------------------- test case core-0019NO -----------------------------
        //
        // Testing feature - The "nodeName" attribute for a ProcessingInstruction
        //                   node is the target.
        //
        // Testing approach - Retrieve the first declaration in the XML file
        //                    and examine its "nodeName" attribute.  It should
        //                    be set to "xml".
        //
        // Semantic Requirements: 13, 15 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0019NO()
        {
            string computedValue = "0";
            string expectedValue = "xml";
            System.Xml.XmlNode testNode = null;

            testResults results = new testResults("Core0019NO");
            results.description = "The nodeName attribute for a ProcessingInstruction "+
                "Node should be set to the target.";
            //
            // Get the targeted data.
            //
            testNode = util.getDOMDocument().ChildNodes.Item(0);
            //
            // The nodeName attribute should be set to "xml".
            //
            computedValue = testNode.Name;
            //
            // write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0019NO --------------------------
        //
        //------------------------- test case core-0020NO -----------------------------
        //
        // Testing feature - The "nodeName" attribute for a comment node is
        //                   "#comment".
        //
        // Testing approach - Retrieve the only comment in the XML file (third child)
        //                    and examine its "nodeName" attribute.  It should
        //                    be set to "#comment".
        //
        // Semantic Requirements: 13, 15 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0020NO()
        {
            string computedValue = "0";
            string expectedValue = "#comment";
            System.Xml.XmlNode testNode = null;

            testResults results = new testResults("Core0020NO");
            results.description = "The nodeName attribute for a comment Node "+
                "should be set to \"#comment\".";
            //
            // Get the targeted data.
            //
            testNode = util.getDOMDocument().ChildNodes.Item(2);      
            //
            // The nodeName attribute should be set to "#comment".
            //
            computedValue = testNode.Name;
            //
            // write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0020NO --------------------------
        //
        //------------------------- test case core-0021NO -----------------------------
        //
        // Testing feature - The "nodeName" attribute for a Document node is
        //                   "#document".
        //
        // Testing approach - Retrieve the DOM Document and examine its
        //                    "nodeName" attribute.  It should be set to "#document".
        //
        // Semantic Requirements: 13, 15 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0021NO()
        {
            string computedValue = "";
            string expectedValue = "#document";
            System.Xml.XmlNode testNodeNode = null;

            testResults results = new testResults("Core0021NO");
            results.description = "The nodeName attribute for a Document Node "+
                "should be set to \"#document\".";
            //
            // Get the targeted data.
            //
            System.Xml.XmlNode testNode = util.getDOMDocument();
            //
            // The nodeName attribute should be set to "#document". 
            //
            computedValue = testNode.Name;
            //
            // write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0021NO --------------------------
        //
        //------------------------- test case core-0022NO -----------------------------
        //
        // Testing feature - The "nodeName" attribute for a DocumentType node is
        //                   the document type name.
        //
        // Testing approach - Retrieve the DOCTYPE declaration (second child) from
        //                    the XML file and examine its "nodeName" attribute.
        //                    It should be set to "staff". 
        //
        // Semantic Requirements: 13, 15 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0022NO()
        {
            string computedValue = "";
            string expectedValue = "staff";
            System.Xml.XmlNode testNode = null;

            testResults results = new testResults("Core0022NO");
            results.description = "The nodeName attribute for a DocumentType Node " +
                "should be set to the document type name.";
            //
            // Get the targeted data.
            //
            testNode = util.getDocType();
            //
            // The nodeName attribute should be set to "staff".
            //
            computedValue = testNode.Name;
            //
            // write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0022NO ------------------------
        //
        //------------------------- test case core-0023NO -----------------------------
        //
        // Testing feature - The "nodeName" attribute for a DocumentFragment node
        //                   is "#document-fragment".
        //
        // Testing approach - Retrieve the whole DOM document and invoke its
        //                    "createDocumentFragment()" method and examine the
        //                    "nodeName" attribute of the returned node.  It should
        //                    be set to "#document-fragment". 
        //
        // Semantic Requirements: 13, 15 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0023NO()
        {
            string computedValue = "";
            string expectedValue = "#document-fragment";
            System.Xml.XmlNode testNode = null;

            testResults results = new testResults("Core0023NO");
            results.description = "The nodeName attribute for a DocumentFragment Node "+
                "should be set to \"#document-fragment\".";
            //
            // Get the targeted data.
            //
            testNode = util.getDOMDocument().CreateDocumentFragment();
            //
            // The nodeName attribute should be set to "#document-fragment".
            //
            computedValue = testNode.Name;
            //
            // write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0023NO --------------------------
        //
        //------------------------- test case core-0024NO -----------------------------
        //
        // Testing feature - The "nodeName" attribute for a notation node is
        //                   the name of the notation.
        //
        // Testing approach - Retrieve the Notation declaration inside the
        //                    DocumentType node and examine its "nodeName"
        //                    attribute.  It should be set to "notation1".
        //
        // Semantic Requirements: 13, 15 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0024NO()
        {
            string computedValue = "";
            string expectedValue = "notation1";
            System.Xml.XmlNode testNode = null;

            testResults results = new testResults("Core0024NO");
            try
            {
                results.description = "The nodeName attribute for a Notation Node " +
                    "should be set to the notation name.";
                //
                // Get the targeted data.
                //
                testNode = util.getNotation("notation1");
                //
                // The nodeName attribute should be set to "notation1".
                //
                computedValue = testNode.Name;
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }
            //
            // write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0024NO --------------------------
        //
        //------------------------   test case core-0025NO ----------------------------
        //
        // Testing feature - The "nodeValue" attribute for an Element node is
        //                   null.
        //
        // Testing approach - Retrieve the root node of the DOM object and check
        //                    its "nodeValue" attribute.  It should be equal
        //                    to null.
        //
        // Semantic Requirements: 13, 16 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0025NO()
        {
            object computedValue = null;
            object expectedValue = null;

            testResults results = new testResults("Core0025NO");
            results.description = "The nodeValue attribute for an Element Node " +
                "should be set to null.";
            //
            // The nodeValue attribute should be set to null.
            //
            computedValue = util.getRootNode().Value;
            //
            // write out results.
            //
            results.expected = (expectedValue == null).ToString();
            results.actual = (computedValue == null).ToString();

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0025NO --------------------------
        //
        //------------------------- test case core-0026NO -----------------------------
        //
        // Testing feature - The "nodeValue" attribute for an Attribute node is
        //                   the value of the attribute.
        //
        // Testing approach - Retrieve the attribute named "domestic" from the last 
        //                    child of the first employee.  Its "nodeValue" attribute 
        //                    is then checked, it should be set to "Yes".
        //
        // Semantic Requirements: 13, 16
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0026NO()
        {
            string computedValue = "";
            string expectedValue = "Yes";
            System.Xml.XmlElement testNode = null;
            System.Xml.XmlAttribute attrNode = null;

            testResults results = new testResults("Core0026NO");
            try
            {
                results.description = "The nodeValue attribute for an Attribute Node " +
                    "should be set to the value of the attribute.";
                //
                // Get the targeted data.
                //
                testNode = (System.Xml.XmlElement)util.nodeObject(util.FIRST,util.SIXTH);
                attrNode = testNode.GetAttributeNode("domestic");//.node.
                //
                // The nodeType attribute should be set to "Yes".
                //
                computedValue = attrNode.Value;
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }
            //
            // write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0026NO --------------------------
        //
        //------------------------- test case core-0027NO -----------------------------
        //
        // Testing feature - The "nodeValue" attribute for a Text node is
        //                   content of the Text node.
        //
        // Testing approach - Retrieve the Text data from the last child of the
        //                    first employee and and examine its "nodeValue"
        //                    attribute.  It should be set to 
        //                    "1230 North Ave. Dallas, Texas 98551". 
        //
        // Semantic Requirements: 13, 16
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0027NO()
        {
            string computedValue = "";
            string expectedValue = "1230 North Ave. Dallas, Texas 98551";
            System.Xml.XmlNode testNode = null;
            System.Xml.XmlNode textNode = null;

            testResults results = new testResults("Core0027NO");
            try
            {
                results.description = "The nodeValue attribute for a Text node " +
                    "should be set to contents of the of the Text node.";
                //
                // Get the targeted data.
                //
                testNode = util.nodeObject(util.FIRST,util.SIXTH);
                textNode = testNode.FirstChild;//.node.
                //
                // Retrieve the nodeValue attribute.
                //
                computedValue = textNode.Value;
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }
            //
            // write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0027NO --------------------------
        //
        //------------------------- test case core-0028NO -----------------------------
        //
        // Testing feature - The "nodeValue" attribute for a CDATASection node is
        //                   the content of the CDATASection.
        //
        // Testing approach - Retrieve the first CDATASection node inside the second 
        //                    child of the second employee and examine its "nodeValue" 
        //                    attribute.  It should be set to "This is a CDATA Section
        //                    with EntityReference number 2 &ent2;".
        //
        // Semantic Requirements: 13, 16
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0028NO()
        {
            string computedValue = "0";
            string expectedValue = "This is a CDATASection with EntityReference number 2 &ent2;";
            System.Xml.XmlNode testNode = null;
            System.Xml.XmlNode cDataNode = null;

            testResults results = new testResults("Core0028NO");
            try
            {
                results.description = "The nodeValue attribute for a CDATASection Node "+
                    "should be set to the contents of the CDATASection."; 
                //
                // Retrieve the targeted data.
                //
                testNode = util.nodeObject(util.SECOND,util.SECOND);
                cDataNode = testNode.ChildNodes.Item(1);//.node.
                //
                // Get the "nodeValue" attribute.
                //
                computedValue = cDataNode.Value;
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }
            //
            // write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0028NO --------------------------
        //
        //------------------------- test case core-0029NO -----------------------------
        //
        // Testing feature - The "nodeValue" attribute for an EntityReference node
        //                   is null.
        //
        // Testing approach - Retrieve the first Entity Reference node from the last
        //                    child of the second employee and examine its
        //                    "nodeValue" attribute.  It should be set to null.
        //
        // Semantic Requirements: 13, 16
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0029NO()
        {
            object computedValue = null;
            object expectedValue = null;
            System.Xml.XmlNode testNode = null;
            System.Xml.XmlNode entRefNode = null;

            testResults results = new testResults("Core0029NO");
            try
            {
                results.description = "The nodeValue attribute for an EntityReference "+
                    "node should be set to null.";
                //
                // Get the targeted data.
                //
                testNode = util.nodeObject(util.SECOND,util.SIXTH);
                entRefNode = testNode.FirstChild;//.node.
                //
                // The nodeValue attribute should be set to null.
                //
                computedValue = entRefNode.Value;
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }
            //
            // write out results.
            //
            results.expected = (expectedValue == null).ToString();
            results.actual = (computedValue == null).ToString();

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0029NO --------------------------
        //
        //------------------------- test case core-0030NO -----------------------------
        //
        // Testing feature - The "nodeValue" attribute for an Entity node
        //                   is null.
        //
        // Testing approach - Retrieve the first Entity declaration in the
        //                    "DOCTYPE" section of the XML file and examine
        //                    its "nodeValue" attribute.  It should be set to
        //                    null.
        //
        // Semantic Requirements: 13, 16
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0030NO()
        {
            object computedValue = null;
            object expectedValue = null;
            System.Xml.XmlNode testNode = null;

            testResults results = new testResults("Core0030NO");
            results.description = "The nodeValue attribute for an Entity node " +
                "should be set to null.";
            //
            // Get the targeted data.
            //
            testNode = util.getEntity("ent1");;
            //
            // The nodeValue attribute should be set to null.
            //
            computedValue = testNode.Value;
            //
            // write out results.
            //
            results.expected = (expectedValue == null).ToString();
            results.actual = (computedValue == null).ToString();

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0030NO --------------------------
        //
        //------------------------- test case core-0031NO -----------------------------
        //
        // Testing feature - The "nodeValue" attribute for a ProcessingInstruction
        //                   node is the entire content excluding the target.
        //
        // Testing approach - Retrieve the first declaration in the XML file
        //                    and examine its "nodeValue" attribute.  It should
        //                    be set to "version="1.0"".
        //
        // Semantic Requirements: 13, 16
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0031NO()
        {
            string computedValue = "";
            string expectedValue = "version=\"1.0\"";
            System.Xml.XmlNode testNode = null;

            testResults results = new testResults("Core0031NO");
            results.description = "The nodeValue attribute for a ProcessingInstruction "+
                "node is the entire contents excluding the target.";
            //
            // Get the targeted data.
            //
            testNode = util.getDOMDocument().ChildNodes.Item(0);
            //
            // The nodeValue attribute should be set to "version="1.0"".
            //
            computedValue = testNode.Value;
            //
            // write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0031NO --------------------------
        //
        //------------------------- test case core-0032NO -----------------------------
        //
        // Testing feature - The "nodeValue" attribute for a comment node is
        //                   the content of the comment.
        //
        // Testing approach - Retrieve the only comment in the XML file (third child)
        //                    and examine its "nodeValue" attribute.  It should
        //                    be set to " This is comment number 1.".
        //
        // Semantic Requirements: 13, 16
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0032NO()
        {
            string computedValue = "";
            string expectedValue = " This is comment number 1.";
            System.Xml.XmlNode testNode = null;

            testResults results = new testResults("Core0032NO");
            results.description = "The nodeValue attribute for a comment node " +
                "should be set to the contents of the comment.";
            //
            // Get the targeted data.
            //
            testNode = util.getDOMDocument().ChildNodes.Item(2);
            //
            // The nodeValue attribute should be set to " This is comment number 1."
            //
            computedValue = testNode.Value;
            //
            // write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0032NO --------------------------
        //
        //------------------------- test case core-0033NO -----------------------------
        //
        // Testing feature - The "nodeValue" attribute for a Document node is
        //                   null.
        //
        // Testing approach - Retrieve the DOM Document and examine its
        //                    "nodeValue" attribute.  It should be set to null.
        //
        // Semantic Requirements: 13, 16
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0033NO()
        {
            object computedValue = null;
            object expectedValue = null;
            System.Xml.XmlNode testNode = null;

            testResults results = new testResults("Core0033NO");
            results.description = "The nodeValue attribute for a Document node "+
                "should be set to null.";
            //
            // Get the targeted data.
            //
            testNode = util.getDOMDocument();
            //
            // The nodeValue attribute should be set to null.
            //
            computedValue = testNode.Value;
            //
            // write out results.
            //
            results.expected = (expectedValue == null).ToString();
            results.actual = (computedValue == null).ToString();

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0033NO --------------------------
        //
        //------------------------- test case core-0034NO -----------------------------
        //
        // Testing feature - The "nodeValue" attribute for a DocumentType node is
        //                   null.
        //
        // Testing approach - Retrieve the DOCTYPE declaration (second child) from
        //                    the XML file and examine its "nodeValue" attribute.
        //                    It should be set to null.
        //
        // Semantic Requirements: 13, 16
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0034NO()
        {
            object computedValue = null;
            object expectedValue = null;
            System.Xml.XmlNode testNode = null;

            testResults results = new testResults("Core0034NO");
            results.description = "The nodeValue attribute for a DocumentType Node " +
                "should be set to null.";
            //
            // Get the targeted data.
            //
            testNode = util.getDOMDocument().ChildNodes.Item(1);
            //
            // The nodeValue attribute should be set to null.
            //
            computedValue = testNode.Value;
            //
            // write out results.
            //
            results.expected = (expectedValue == null).ToString();
            results.actual = (computedValue == null).ToString();

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0034NO ------------------------
        //
        //------------------------- test case core-0035NO -----------------------------
        //
        // Testing feature - The "nodeValue" attribute for a DocumentFragment node
        //                   is null.
        //
        // Testing approach - Retrieve the whole DOM document and invoke its
        //                    "createDocumentFragment()" method and examine the
        //                    "nodeValue" attribute of the returned node.  It should
        //                    be set to null.
        //
        // Semantic Requirements: 13, 16
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0035NO()
        {
            object computedValue = null;
            object expectedValue = null;
            System.Xml.XmlNode testNode = null;

            testResults results = new testResults("Core0035NO");
            results.description = "The nodeValue attribute for a DocumentFragment node " +
                "should be set to null.";
            //
            // Get the targeted data.
            //
            testNode = util.getDOMDocument().CreateDocumentFragment();
            //
            // The nodeValue attribute should be set to null.
            //
            computedValue = testNode.Value;
            //
            // write out results.
            //
            results.expected = (expectedValue == null).ToString();
            results.actual = (computedValue == null).ToString();

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0035NO --------------------------
        //
        //------------------------- test case core-0036NO -----------------------------
        //
        // Testing feature - The "nodeValue" attribute for a notation node is
        //                   the name of the notation.
        //
        // Testing approach - Retrieve the Notation declaration inside the
        //                    DocumentType node and examine its nodeValue 
        //                    attribute.  It should be set to null.
        //
        // Semantic Requirements: 13, 16
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0036NO()
        {
            object computedValue = null;
            object expectedValue = null;
            System.Xml.XmlNode testNode = null;

            testResults results = new testResults("Core0036NO");
            try
            {
                results.description = "The nodeValue attribute for a Notation node " +
                    "should be set to null.";
                //
                // Get the targeted data.
                //
                testNode = util.getNotation("notation1");
                //
                // The nodeValue attribute should be set to null.
                //
                computedValue = testNode.Value;
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }
            //
            // write out results.
            //
            results.expected = (expectedValue == null).ToString();
            results.actual = (computedValue == null).ToString();

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0036NO --------------------------
        //
        //------------------------   test case core-0037NO ----------------------------
        //
        // Testing feature - The "attributes" attribute for an Element node is
        //                   a NamedNodeMap.
        //
        // Testing approach - Retrieve the last child of the third employee
        //                    and examine its "attributes" attribute.  It should be 
        //                    equal to a NamedNodeMap of its attributes. 
        //
        // Semantic Requirements: 13, 17
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0037NO()
        {
            string computedValue = "";
            string expectedValue = "";
            System.Xml.XmlNode testNode = null;
            string testName = "core-0037NO";

            testResults results = new testResults("core0037NO");
            try
            {
                results.description += "The \"attributes\" attribute for an Element node";
                results.description += " should be set to a NamedNodeMap. ";

                //
                // Retrieve the targeted data and its attributes.
                //
                testNode = util.nodeObject(util.THIRD,util.SIXTH);
                computedValue += testNode.Attributes.Item(util.FIRST).Name+" ";
                computedValue += testNode.Attributes.Item(util.SECOND).Name;
                //
                // Determine the order of the NamedNodeMap items.
                //
                if (computedValue.Substring(0,1) == "d" && computedValue.Substring(1,1) == "o")
                    expectedValue = "domestic street";
                else
                    expectedValue = "street domestic";
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }
            //
            // write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0037NO --------------------------
        //
        //------------------------- test case core-0038NO -----------------------------
        //
        // Testing feature - The "attributes" attribute for an Attribute node is
        //                   null.
        //
        // Testing approach - Retrieve the first attribute from the last child of 
        //                    the first employee and.  Its "attributes" attribute 
        //                    is then checked.  It should be set to null.
        //
        // Semantic Requirements: 13, 17
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0038NO()
        {
            object computedValue = null;
            object expectedValue = null;
            System.Xml.XmlNode testNode = null;
            System.Xml.XmlAttribute attrNode = null;

            testResults results = new testResults("Core0038NO");
            try
            {
                results.description = "The \"attributes\" attribute for an Attribute node " +
                    "should be set to null.";
                //
                // Retrieve the targeted data.
                //
                testNode = util.nodeObject(util.FIRST,util.SIXTH);
                attrNode = (System.Xml.XmlAttribute)testNode.Attributes.Item(util.FIRST);
                //
                // The "attributes" attribute should be set to null.
                //
                computedValue = attrNode.Attributes;
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }
            //
            // write out results.
            //
            results.expected = (expectedValue == null).ToString();
            results.actual = (computedValue == null).ToString();

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0038NO --------------------------
        //
        //------------------------- test case core-0039NO -----------------------------
        //
        // Testing feature - The "attributes" attribute for a Text node is
        //                   null.
        //
        // Testing approach - Retrieve the text data from the last child of the
        //                    first employee and examine its "attributes"
        //                    attribute.  It should be set to null.
        //
        // Semantic Requirements: 13, 17
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0039NO()
        {
            object computedValue = null;
            object expectedValue = null;
            System.Xml.XmlNode testNode = null;
            System.Xml.XmlNode textNode = null;

            testResults results = new testResults("Core0039NO");
            try
            {
                results.description = "The \"attributes\" attribute for a Text node "+
                    "should be set to null.";
                //
                // Retrieve the targeted data.
                //
                testNode = util.nodeObject(util.FIRST,util.SIXTH);
                textNode = testNode.FirstChild;//.node.
                //
                // The "attributes" attribute should be set to null
                //
                computedValue = textNode.Attributes;
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }
            //
            // write out results.
            //
            results.expected = (expectedValue == null).ToString();
            results.actual = (computedValue == null).ToString();

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0039NO --------------------------
        //
        //------------------------- test case core-0040NO -----------------------------
        //
        // Testing feature - The "attributes" attribute for a CDATASection node is
        //                   null.
        //
        // Testing approach - Retrieve the CDATASection node contained inside
        //                    the second child of the second employee and
        //                    examine its "attributes" attribute.  It should be
        //                    set to null.
        //
        // Semantic Requirements: 13, 17
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0040NO()
        {
            object computedValue = null;
            object expectedValue = null;
            System.Xml.XmlNode testNode = null;
            System.Xml.XmlNode cDataNode = null;

            testResults results = new testResults("Core0040NO");
            try
            {
                results.description = "The \"attributes\" attribute for a CDATASection "+
                    "node should be set to null.";
                //
                // Retrieve the targeted data.
                //
                testNode = util.nodeObject(util.SECOND,util.SECOND);
                cDataNode = testNode.LastChild;//.node.
                //
                // The "attributes" attribute should be set to null.
                //
                computedValue = cDataNode.Attributes;
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }
            //
            // write out results.
            //
            results.expected = (expectedValue == null).ToString();
            results.actual = (computedValue == null).ToString();

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0040NO --------------------------
        //
        //------------------------- test case core-0041NO -----------------------------
        //
        // Testing feature - The "attributes" attribute for an EntityReference node
        //                   is null.
        //
        // Testing approach - Retrieve the first Entity Reference node from the last 
        //                    child of the second employee and examine its
        //                    "attributes" attribute.  It should be set to null.
        //
        // Semantic Requirements: 13, 17
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0041NO()
        {
            object computedValue = null;
            object expectedValue = null;
            System.Xml.XmlNode testNode = null;
            System.Xml.XmlNode entRefNode = null;

            testResults results = new testResults("Core0041NO");
            try
            {
                results.description = "The \"attributes\" attribute for an "+
                    "EntityReference node should be set to null.";
                //
                // Retrieve the targeted data.
                //
                testNode = util.nodeObject(util.SECOND,util.SIXTH);
                entRefNode = testNode.FirstChild;//.node.
                //
                // The \"attributes\" attribute should be set to null.
                //
                computedValue = entRefNode.Attributes;
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }
            //
            // write out results.
            //
            results.expected = (expectedValue == null).ToString();
            results.actual = (computedValue == null).ToString();

            Assert.AreEqual (results.expected, results.actual);
        }
        //------------------------ End test case core-0041NO --------------------------
        //
        //------------------------- test case core-0042NO -----------------------------
        //
        // Testing feature - The "attributes" attribute for an Entity node
        //                   is null.
        //
        // Testing approach - Retrieve the first Entity declaration in the
        //                    "DOCTYPE" section of the XML file and examine
        //                    its "attributes" attribute.  It should be set to
        //                    null.
        //
        // Semantic Requirements: 13, 17
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0042NO()
        {
            object computedValue = null;
            object expectedValue = null;
            System.Xml.XmlNode testNode = null;

            testResults results = new testResults("Core0042NO");

            results.description = "The \"attributes\" attribute for an Entity node "+
                "should be set to null.";
            //
            // Get the targeted data.
            //
            testNode = util.getEntity("ent1");
            //
            // The "attributes" attribute should be set to null.
            //
            computedValue = testNode.Attributes;
            //
            // write out results.
            //
            results.expected = (expectedValue == null).ToString();
            results.actual = (computedValue == null).ToString();

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0042NO --------------------------
        //
        //------------------------- test case core-0043NO -----------------------------
        //
        // Testing feature - The "attributes" attribute for a ProcessingInstruction
        //                   node is null.
        //
        // Testing approach - Retrieve the first declaration in the XML file
        //                    and examine its "attributes" attribute.  It should
        //                    be set to null.
        //
        // Semantic Requirements: 13, 17
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0043NO()
        {
            object computedValue = null;
            object expectedValue = null;
            System.Xml.XmlNode testNode = null;

            testResults results = new testResults("Core0043NO");

            results.description = "The \"attributes\" attribute for a "+
                "ProcessingInstruction node is null.";
            //
            // Get the targeted data.
            //
            testNode = util.getDOMDocument().ChildNodes.Item(0);
            //
            // The "attributes" attribute should be set to null.
            //
            computedValue = testNode.Attributes;
            //
            // write out results.
            //
            results.expected = (expectedValue == null).ToString();
            results.actual = (computedValue == null).ToString();

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0043NO --------------------------
        //
        //------------------------- test case core-0044NO -----------------------------
        //
        // Testing feature - The "attributes" attribute for a comment node is
        //                   null.
        //
        // Testing approach - Retrieve the third child of the DOM document and
        //                    examine its "attributes" attribute.  It should
        //                    be set to null.
        //
        // Semantic Requirements: 13, 17
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0044NO()
        {
            object computedValue = null;
            object expectedValue = null; 
            System.Xml.XmlNode testNode = null;

            testResults results = new testResults("Core0044NO");

            results.description = "The \"attributes\" attribute for a comment node "+
                "should be set to null.";
            //
            // Get the targeted data.
            //
            testNode = util.getDOMDocument().ChildNodes.Item(2);
            //
            // The "attributes" attribute should be set to null. 
            //
            computedValue = testNode.Attributes;
            //
            // write out results.
            //
            results.expected = (expectedValue == null).ToString();
            results.actual = (computedValue == null).ToString();

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0044NO --------------------------
        //
        //------------------------- test case core-0045NO -----------------------------
        //
        // Testing feature - The "attributes" attribute for a Document node is
        //                   null.
        //
        // Testing approach - Retrieve the DOM Document and examine its
        //                    "attributes" attribute.  It should be set to null.
        //
        // Semantic Requirements: 13, 17
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0045NO()
        {
            object computedValue = null;
            object expectedValue = null;
            System.Xml.XmlNode testNode = null;

            testResults results = new testResults("Core0045NO");

            results.description = "The \"attributes\" attribute for a Document node "+
                "should be set to null.";
            //
            // Get the targeted data.
            //
            testNode = util.getDOMDocument();
            //
            // The "attributes" attribute should be set to null.
            //
            computedValue = testNode.Attributes;
            //
            // write out results.
            //
            results.expected = (expectedValue == null).ToString();
            results.actual = (computedValue == null).ToString();

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0045NO --------------------------
        //
        //------------------------- test case core-0046NO -----------------------------
        //
        // Testing feature - The "attributes" attribute for a DocumentType node is
        //                   null.
        //
        // Testing approach - Retrieve the DOCTYPE declaration (second child) from
        //                    the XML file and examine its "attribute" attribute.
        //                    It should be set to null.
        //
        // Semantic Requirements: 13, 17
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0046NO()
        {
            object computedValue = null;
            object expectedValue = null; 
            System.Xml.XmlNode testNode = null;

            testResults results = new testResults("Core0046NO");

            results.description = "The \"attribute\" attribute for a DocumentType "+
                "node should be set to null.";
            //
            // Get the targeted data.
            //
            testNode = util.getDocType();
            //
            // The "attributes" attribute should be set to null.
            //
            computedValue = testNode.Attributes;
            //
            // write out results.
            //
            results.expected = (expectedValue == null).ToString();
            results.actual = (computedValue == null).ToString();

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0046NO ------------------------
        //
        //------------------------- test case core-0047NO ---------------------------
        //
        // Testing feature - The "attributes" attribute for a DocumentFragment node
        //                   is null.
        //
        // Testing approach - Retrieve the whole DOM document and invoke its
        //                    "createDocumentFragment()" method and examine the
        //                    "attributes" attribute of the returned node.  It
        //                    should be set to null.
        //
        // Semantic Requirements: 13, 17
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0047NO()
        {
            object computedValue = null;
            object expectedValue = null;
            System.Xml.XmlNode testNode = null;

            testResults results = new testResults("Core0047NO");
            results.description = "The \"attributes\" attribute for a DocumentFragment "+
                "node should be set to null.";
            //
            // Get the targeted data.
            //
            testNode = util.getDOMDocument().CreateDocumentFragment();
            //
            // The "attributes" attribute should be set to null.
            //
            computedValue = testNode.Attributes;
            //
            // write out results.
            //
            results.expected = (expectedValue == null).ToString();
            results.actual = (computedValue == null).ToString();

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0047NO --------------------------
        //
        //------------------------- test case core-0048NO -----------------------------
        //
        // Testing feature - The "attributes" attribute for a notation node is
        //                   null.
        //
        // Testing approach - Retrieve the Notation declaration inside the
        //                    DocumentType node and examine its "attributes"
        //                    attribute.  It should be set to null.
        //
        // Semantic Requirements: 13, 17
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0048NO()
        {
            object computedValue = null;
            object expectedValue = null;
            System.Xml.XmlNode testNode = null;

            testResults results = new testResults("Core0048NO");
            try
            {
                results.description = "The \"attributes\" attribute for a Notation node "+
                    "should be set to null.";
                //
                // Get the targeted data.
                //
                testNode = util.getNotation("notation1");
                //
                // The "attributes" attribute should be set to null.
                //
                computedValue = testNode.Attributes;
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }
            //
            // write out results.
            //
            results.expected = (expectedValue == null).ToString();
            results.actual = (computedValue == null).ToString();

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0048NO --------------------------
        //
        //------------------------- test case core-0049NO -----------------------------
        //
        // Testing feature - The "parentNode" attribute contains the parent of 
        //                   this node.
        //
        // Testing approach - Retrieve the second employee and examine its
        //                    "parentNode" attribute.  It should be set
        //                    to "staff".
        //
        // Semantic Requirements: 18
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0049NO()
        {
            string computedValue = "";
            string expectedValue = "staff";
            System.Xml.XmlNode testNode = null;
            System.Xml.XmlNode testNodeParent = null;

            testResults results = new testResults("Core0049NO");
            try
            {
                results.description = "The parentNode attribute contains the parent "+
                    "node of this node.";
                //
                // Retrieve the targeted data and access its parent node.
                //
                testNode = util.nodeObject(util.SECOND,-1);
                testNodeParent = testNode.ParentNode; //.node.
                //
                // The nodeName attribute should be "staff".
                //
                computedValue = testNodeParent.Name;
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }
            //
            //  Write out results
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0049NO --------------------------
        //
        //------------------------- test case core-0050NO -----------------------------
        //
        // Testing feature - The "parentNode" attribute of a node that has just 
        //                   been created and not yet added to the tree is null.
        //
        // Testing approach - Create a new "employee" Element node using the 
        //                    "createElement(name)" method from the Document
        //                    interface.  Since this new node has not yet been
        //                    added to the tree, its parentNode attribute should
        //                    be null.
        //
        // Semantic Requirements: 19
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0050NO()
        {
            object computedValue = null;
            object expectedValue = null;
            System.Xml.XmlNode testNode = null;

            testResults results = new testResults("Core0050NO");

            results.description = "The parentNode attribute of a node that has just "+
                "been created, but not yet added to the tree is "+
                "null.";
            //
            // Create new node and access its parentNode attribute.
            //
            testNode = util.createNode(util.ELEMENT_NODE,"employee");
            computedValue = testNode.ParentNode;
            //
            //  Write out results.
            //
            results.expected = (expectedValue == null).ToString();
            results.actual = (computedValue == null).ToString();

            util.resetData();
            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0050NO --------------------------
        //
        //------------------------- test case core-0051NO -----------------------------
        //
        // Testing feature - The "parentNode" attribute of a node that has been 
        //                   been removed from the tree is null.
        //
        // Testing approach - Remove the first employee by invoking the 
        //                    "removeChild(oldChild)" method and examine its 
        //                    parentNode attribute.  It should be set to null.
        //
        // Semantic Requirements: 20 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0051NO()
        {
            object computedValue = null;
            object expectedValue = null;
            System.Xml.XmlNode testNode = null;
            System.Xml.XmlNode removedNode = null;

            testResults results = new testResults("Core0051NO");

            results.description = "The parentNode attribute of a node that has "+
                "been removed from the tree is null.";
            //
            // Remove the targeted data and access its parentNode attribute.
            //
            testNode = util.nodeObject(util.FIRST,-1);
            removedNode = util.getRootNode().RemoveChild(testNode);//.node
            computedValue = removedNode.ParentNode;
            //
            // Write out results.
            //
            results.expected = (expectedValue == null).ToString();
            results.actual = (computedValue == null).ToString();

            util.resetData();
            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0051NO --------------------------
        //
        //------------------------- test case core-0052NO -----------------------------
        //
        // Testing feature - The "childNodes" attribute of a node contains a 
        //                   NodeList of all the children of this node.
        //
        // Testing approach - Retrieve the second employee and examine its 
        //                    childNodes attribute.  It should be NodeList
        //                    containing all of its children.  The length of 
        //                    the list should be 9.
        //
        // Semantic Requirements: 21 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0052NO()
        {
            int computedValue = 0;
            int expectedValue = 6;
            System.Xml.XmlNode testNode = null;
            System.Xml.XmlNodeList nodeList = null;

            testResults results = new testResults("Core0052NO");

            results.description = "The childNodes attribute of a node contains a "+
                "NodeList of all the children of this node.";
            //
            // Retrieve targeted data and examine the list length.
            //
            testNode = util.nodeObject(util.SECOND,-1);
            nodeList = testNode.ChildNodes;//.node.
            computedValue = nodeList.Count;
            //
            //  Write out results.
            //
            results.expected = expectedValue.ToString();
            results.actual = computedValue.ToString();

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0052NO --------------------------
        //
        //------------------------- test case core-0053NO -----------------------------
        //
        // Testing feature - If a node has no children then the NodeList returned
        //                   by its childNodes attribute has no nodes.
        //
        // Testing approach - Retrieve the textual data from the first child of 
        //                    of the second employee and examine its childNodes 
        //                    attribute.  It should be NodeList with no nodes 
        //                    in it.
        //
        // Semantic Requirements: 22
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0053NO()
        {
            string computedValue = "";
            string expectedValue = "0";
            string testName = "core-0053NO";
            System.Xml.XmlNode testNode = null;
            System.Xml.XmlNodeList noChildNode = null;

            testResults results = new testResults("Core0053NO");
            try
            {
                results.description = "If a node has no child nodes then the NodeList "+
                    "returned by its childNodes attribute has no "+
                    "nodes."; 
                //
                // Retrieve the targeted data and access its childNodes attribute.
                //
                testNode = util.nodeObject(util.SECOND,util.FIRST);
                noChildNode = testNode.FirstChild.ChildNodes;//.node.
                computedValue = noChildNode.Count.ToString();
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }
            //
            //  Write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0053NO --------------------------
        //
        //------------------------- test case core-0054NO -----------------------------
        //
        // Testing feature - The NodeList returned by the childNodes attribute is live.
        //                   Changes on the node's children are immediately reflected
        //                   on the nodes returned by the NodeList.
        //
        // Testing approach -  Create a NodeList of the children of the second employee 
        //                     and then add a newly created element (created with the 
        //                     "createElement" method from the Document interface) to 
        //                     the second employee by using the "append" method.  The
        //                     length attribute of the NodeList should reflect this new
        //                     addition to the child list.  It should now return the
        //                     value 7.
        // 
        // Semantic Requirements: 23
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0054NO()
        {
            int computedValue = 0;
            int expectedValue = 7;
            System.Xml.XmlNode testNode = null;
            System.Xml.XmlNodeList nodeList = null;

            testResults results = new testResults("Core0054NO");

            results.description = "The NodeList returned by the childNodes attribute "+
                "is live.  Changes in the children node are "+
                "immediately reflected in the NodeList.";
            //   
            // Retrieve the targeted data and append a new Element node to it.
            //
            testNode = util.nodeObject(util.SECOND,-1);
            nodeList = testNode.ChildNodes;//.node.
            testNode.AppendChild(util.createNode(util.ELEMENT_NODE,"text3"));//.node.
            computedValue = nodeList.Count;
            //
            //  Write out results.
            //
            results.expected = expectedValue.ToString();
            results.actual = computedValue.ToString();

            util.resetData();
            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0054NO --------------------------
        //
        //------------------------- test case core-0055NO -----------------------------
        //
        // Testing feature - The firstChild attribute contains the first child of this
        //                   node.
        //
        // Testing approach - Retrieve the second employee and examine its firstChild
        //                    attribute.  It should be set to a node whose tag name
        //                    "employeeId".
        //
        // Semantic Requirements: 24
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0055NO()
        {
            string computedValue = "";
            string expectedValue = "employeeId";
            System.Xml.XmlNode testNode = null;
            System.Xml.XmlNode firstChildNode = null;

            testResults results = new testResults("Core0055NO");

            results.description = "The firstChild attribute contains the first "+
                "child of this node.";
            //
            // Retrieve the targeted data.
            //
            testNode = util.nodeObject(util.SECOND,-1);
            firstChildNode = testNode.FirstChild;//.node.
            //
            // Its firstChild attribute's tagName should be "employeeId".
            //
            computedValue = firstChildNode.Name;
            //
            // Write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0055NO --------------------------
        //
        //------------------------- test case core-0056NO -----------------------------
        //
        // Testing feature - If there is no first child then the firstChild attribute 
        //                   returns null.
        //
        // Testing approach - Retrieve the Text node from the first child of the first 
        //                    employee and examine its firstChild attribute.  It 
        //                    should be set to null.
        //
        // Semantic Requirements: 25
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0056NO()
        {
            object computedValue = null;
            object expectedValue = null;
            System.Xml.XmlNode testNode = null;
            System.Xml.XmlNode noChildNode = null;

            testResults results = new testResults("Core0056NO");
            try
            {
                results.description = "If a node does not have a first child then its "+
                    "firstChild attribute returns null.";
                //
                // Get the targeted data.
                //
                testNode = util.nodeObject(util.FIRST,util.FIRST);
                noChildNode = testNode.FirstChild;//.node.
                //
                //  Its firstChild attribute should be equal to null.
                //
                computedValue = noChildNode.FirstChild;
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }

            //
            // Write out results.
            //
            results.expected = (expectedValue == null).ToString();
            results.actual = (computedValue == null).ToString();

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0056NO --------------------------
        //
        //------------------------- test case core-0057NO -----------------------------
        //
        // Testing feature - The lastChild attribute contains the last child of this
        //                   node.
        //
        // Testing approach - Retrieve the second employee and examine its lastChild
        //                    attribute.  It should be set to a node whose tag name
        //                    is "address".
        //
        // Semantic Requirements: 26
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0057NO()
        {
            string computedValue = "";
            string expectedValue = "address";
            System.Xml.XmlNode testNode = null;
            System.Xml.XmlNode lastChildNode = null;

            testResults results = new testResults("Core0057NO"); 

            results.description = "The lastChild attribute contains the last "+
                "child of this node.";
            //
            // Retrieve the targeted data and access its lastChild attribute.
            //
            testNode = util.nodeObject(util.SECOND,-1);
            //
            // Its lastChild attribute should be equal to a node with tag name = "address".
            //
            lastChildNode = testNode.LastChild;//.node.
            computedValue = lastChildNode.Name;
            //
            // Write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0057NO --------------------------
        //
        //------------------------- test case core-0058NO -----------------------------
        //
        // Testing feature - If there is no last child then the lastChild attribute
        //                   returns null.
        //
        // Testing approach - Retrieve the Text node inside the first child of the 
        //                    second employee and examine its lastChild attribute.  
        //                    It should be set to null.
        //
        // Semantic Requirements: 27
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0058NO()
        {
            object computedValue = null;
            object expectedValue = null;
            System.Xml.XmlNode testNode = null;
            System.Xml.XmlNode noChildNode = null;

            testResults results = new testResults("Core0058NO");
            try
            {
                results.description = "If a node does not have a last child then its "+
                    "lastChild attribute returns null.";
                //
                // Retrieve the targeted data.
                //
                testNode = util.nodeObject(util.SECOND,util.FIRST);
                noChildNode = testNode.FirstChild;//.node.
                //
                // Its lastChild attribute should be equal to null.
                //
                computedValue = noChildNode.LastChild;
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }
            //
            // Write out results.
            //
            results.expected = (expectedValue == null).ToString();
            results.actual = (computedValue == null).ToString();

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0058NO --------------------------
        //
        //------------------------- test case core-0059NO -----------------------------
        //
        // Testing feature - The previousSibling attribute contains the node 
        //                   immediately preceding this node.
        //
        // Testing approach - Retrieve the second child of the second employee and 
        //                    examine its previousSibling attribute.  It should be set 
        //                    to a node whose tag name is "employeeId".
        //
        // Semantic Requirements: 28 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0059NO()
        {
            string computedValue = "";
            string expectedValue = "employeeId";
            System.Xml.XmlNode testNode = null;
            System.Xml.XmlNode itsPreviousSibling = null;

            testResults results = new testResults("Core0059NO");
            try
            {
                results.description = "The previousSibling attribute contains the node "+
                    "immediately preceding this node.";
                //
                // Retrieve the targeted data and accesss its previousiSibling attribute.
                //
                testNode = util.nodeObject(util.SECOND,util.SECOND);
                itsPreviousSibling = testNode.PreviousSibling;//.node.
                //
                // Its previousSibling attribute should have a tag name = "employeeId".
                //
                computedValue = itsPreviousSibling.Name;
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }

            //
            // Write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0059NO --------------------------
        //
        //------------------------- test case core-0060NO -----------------------------
        //
        // Testing feature - If there is no immediately preceding node then the 
        //                   previousSibling attribute returns null.
        //
        // Testing approach - Retrieve the first child of the of the second employee
        //                    employee and examine its previousSibling attribute.  
        //                    It should be set to null.
        //
        // Semantic Requirements: 29
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0060NO()
        {
            object computedValue = null;
            object expectedValue = null;
            System.Xml.XmlNode testNode = null;

            testResults results = new testResults("Core0060NO");
            try
            {
                results.description = "If there is no node immediately preceding this "+
                    "node then the previousSibling attribute returns "+
                    "null.";
                //
                // Retrieve the targeted data.
                //
                testNode = util.nodeObject(util.SECOND,util.FIRST);
                //
                // Its previousSibling attribute should be equal to null.
                //
                computedValue = testNode.PreviousSibling;//.node.
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }

            //
            // Write out results.
            //
            results.expected = (expectedValue == null).ToString();
            results.actual = (computedValue == null).ToString();

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0060NO --------------------------
        //
        //------------------------- test case core-0061NO -----------------------------
        //
        // Testing feature - The nextSibling attribute contains the node
        //                   immediately following this node.
        //
        // Testing approach - Retrieve the first child of the second employee and
        //                    examine its nextSibling attribute.  It should be set
        //                    to a node whose tag name is "name".
        //
        // Semantic Requirements: 30 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0061NO()
        {
            string computedValue = "";
            string expectedValue = "name";
            System.Xml.XmlNode testNode = null;
            System.Xml.XmlNode itsNextSibling = null;

            testResults results = new testResults("Core0061NO");
            try
            {
                results.description = "The nextSibling attribute contains the node "+
                    "immediately following this node.";
                //
                // Retrieve the targeted data and access its nextSibling attribute.
                //
                testNode = util.nodeObject(util.SECOND,util.FIRST);
                itsNextSibling = testNode.NextSibling;//.node.
                //
                // Its nextSibling attribute should be a node with tag name = "name".
                //
                computedValue = itsNextSibling.Name;
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }

            //
            // Write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0061NO --------------------------
        //
        //------------------------- test case core-0062NO -----------------------------
        //
        // Testing feature - If there is no node immediately following this node 
        //                   then the nextSibling attribute returns null.
        //
        // Testing approach - Retrieve the last child of the second employee
        //                    and examine its nextSibling attribute.  It should 
        //                    be set to null.
        //
        // Semantic Requirements: 31 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0062NO()
        {
            object computedValue = null;
            object expectedValue = null;
            System.Xml.XmlNode testNode = null;

            testResults results = new testResults("Core0062NO");
            try
            {
                results.description = "If there is no node immediately following this "+
                    "node then the nextSibling attribute returns null.";
                //
                // Retrieve the targeted data.
                //
                testNode = util.nodeObject(util.SECOND,util.SIXTH);
                //
                // Its NextSibling attribute should be equal to null.
                //
                computedValue = testNode.NextSibling;//.node.
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }

            //
            //  Write out results.
            //
            results.expected = (expectedValue == null).ToString();
            results.actual = (computedValue == null).ToString();

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0062NO --------------------------
        //
        //------------------------- test case core-0063NO -----------------------------
        //
        // Testing feature - The ownerDocument attribute contains the Document 
        //                   associated with this node.
        //
        // Testing approach - Retrieve the second employee and examine its
        //                    ownerDocument attribute.  It should contain a
        //                    document whose documentElement attribute is equal
        //                    to "staff".
        //
        // Semantic Requirements: 32
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0063NO()
        {
            string computedValue = "";
            string expectedValue = "staff";
            System.Xml.XmlNode testNode = null;
            System.Xml.XmlDocument ownerDoc = null;

            testResults results = new testResults("Core0063NO");

            results.description = "The ownerDocument attribute contains the Document "+
                "associated with this node.";
            //
            // Retrieve the targeted data and access its ownerDocument attribute.
            //
            testNode = util.nodeObject(util.SECOND,-1);
            ownerDoc = testNode.OwnerDocument;//.node.
            //
            // the nodeName of its root node should be "staff"; 
            //
            computedValue = ownerDoc.DocumentElement.Name;
            //
            // Write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0063NO --------------------------
        //
        //------------------------- test case core-0064NO -----------------------------
        //
        // Testing feature - The ownerDocument attribute returns null if the
        //                   target node is itself a Document. 
        //
        // Testing approach - Retrieve the master document by invoking the 
        //                    "getDOMDocument()" method then examine the
        //                    ownerDocument attribute of the returned object.
        //                    It should be null.
        //
        // Semantic Requirements: 33
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0064NO()
        {
            object computedValue = null;
            object expectedValue = null;
            System.Xml.XmlNode testNode = null;

            testResults results = new testResults("Core0064NO");

            results.description = "The ownerDocument attribute returns null if the "+
                "target node is itself a Document.";
            //
            // Retrieve the targeted data.
            //
            testNode = util.getDOMDocument();
            //
            //  Its ownerDocument attribute should be null. 
            //
            computedValue = testNode.OwnerDocument;
            //
            // Write out results.
            //
            results.expected = (expectedValue == null).ToString();
            results.actual = (computedValue == null).ToString();

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0064NO --------------------------
        //
        //------------------------- test case core-0065NO -----------------------------
        //
        // Testing feature - The insertBefore(newChild,refChild) method inserts the
        //                   node newChild before the node refChild. 
        //
        // Testing approach - Insert a newly created Element node before the fourth 
        //                    child of the second employee and examine the new child
        //                    and the reference child after the insertion for correct
        //                    placement.  
        //
        // Semantic Requirements: 34
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0065NO()
        {
            string computedValue = "";
            string expectedValue = "newChild salary";
            System.Xml.XmlNode refChild = null;
            System.Xml.XmlElement newChild = null;
            System.Xml.XmlElement testNode = null;

            testResults results = new testResults("Core0065NO");
            try
            {
                results.description = "The insertBefore(newChild,refChild) method inserts "+
                    "the node newChild before the node refChild.";
                //
                // Retrieve targeted data, create a new Element node to insert, define the 
                // reference node, and insert the newly created element.
                //
                testNode = (System.Xml.XmlElement)util.nodeObject(util.SECOND,-1);
                refChild = util.nodeObject(util.SECOND,util.FOURTH);
                newChild = (System.Xml.XmlElement)util.createNode(util.ELEMENT_NODE,"newChild");
                testNode.InsertBefore(newChild, refChild);//.node.
                //
                // Check that each node is in the proper position.
                //
                computedValue += util.getSubNodes(testNode).Item(util.FOURTH).Name+" ";
                computedValue += util.getSubNodes(testNode).Item(util.FIFTH).Name;
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }
            //
            // Write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            util.resetData();
            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0065NO --------------------------
        //
        //------------------------- test case core-0066NO -----------------------------
        //
        // Testing feature - If the refChild is null then the 
        //                   insertBefore(newChild,refChild) method inserts the
        //                   node newChild at the end of the list of children.
        //
        // Testing approach - Retrieve the second employee and invoke the the 
        //                    insertBefore(newChild,refChild) method with 
        //                    refChild = null.  Under these conditions the
        //                    newChild should be added at the end of the list.
        //                    The last item in the list is examined after the 
        //                    insertion.  The last Element node of the list 
        //                    should be "newChild".
        //
        // Semantic Requirements: 35
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0066NO()
        {
            string computedValue = "";
            string expectedValue = "newChild";
            System.Xml.XmlNode refChild = null;
            System.Xml.XmlNode newChild = null;
            System.Xml.XmlNode testNode = null;

            testResults results = new testResults("Core0066NO");

            results.description = "If refChild is null then the insertBefore("+
                "newChild,refChild) method inserts the node "+
                "newChild at the end of the list.";
            //
            // Retrieve targeted data, create a new Element node to insert, define 
            // the reference node and insert the newly created element
            //
            testNode = util.nodeObject(util.SECOND,-1);
            newChild = util.createNode(util.ELEMENT_NODE,"newChild");
            testNode.InsertBefore(newChild, refChild);//.node.
            //
            // Retrieve the node at the end of the list.
            //
            computedValue = testNode.LastChild.Name;//.node.
            //
            // Write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            util.resetData();
            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0066NO --------------------------
        //
        //------------------------- test case core-0067NO -----------------------------
        //
        // Testing feature - If the refChild is a DocumentFragment object then all
        //                   its children are inserted in the same order before
        //                   the refChild. 
        //
        // Testing approach - Create a DocumentFragment object and populate it with
        //                    two element nodes.  Retrieve the second employee
        //                    and insert the newly created DocumentFragment before
        //                    its fourth child.  The second employee should now
        //                    have two extra children ("childNode1" and "childNode2")
        //                    at positions fourth and fifth respectively.
        //
        // Semantic Requirements: 36
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0067NO()
        {
            string computedValue = "";
            string expectedValue = "newChild1 newChild2";
            System.Xml.XmlElement testNode = null;
            System.Xml.XmlNode refChild = null;
            System.Xml.XmlDocumentFragment newDocFragment = util.getDOMDocument().CreateDocumentFragment();

            testResults results = new testResults("Core0067NO");
            try
            {
                results.description = "If newChild is a DocumentFragment object, then all "+
                    "its children are inserted in the same order before "+
                    "the refChild node.";
                //
                // Populate the DocumentFragment object.
                //
                newDocFragment.AppendChild(util.createNode(util.ELEMENT_NODE,"newChild1"));
                newDocFragment.AppendChild(util.createNode(util.ELEMENT_NODE,"newChild2"));
                //
                // Retrieve targeted data, define reference node and insert new child.
                //
                testNode = (System.Xml.XmlElement)util.nodeObject(util.SECOND,-1);
                refChild = util.nodeObject(util.SECOND,util.FOURTH);
                testNode.InsertBefore(newDocFragment,refChild);//.node.
                //
                // Check that all the new nodes are in the proper position.
                //
                computedValue += util.getSubNodes(testNode).Item(util.FOURTH).Name+" ";
                computedValue += util.getSubNodes(testNode).Item(util.FIFTH).Name; 
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }
            //
            // Write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            util.resetData();
            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0067NO --------------------------
        //
        //------------------------- test case core-0068NO -----------------------------
        //
        // Testing feature - The insertBefore(newChild,refChild) method returns the
        //                   node being inserted.
        //
        // Testing approach - Insert an Element node before the fourth child
        //                    of the second employee and examine the returned
        //                    node from the method.  The node Element node 
        //                    returned by the method should be "newChild".
        //
        // Semantic Requirements: 37
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0068NO()
        {
            string computedValue = "";
            string expectedValue = "newChild";
            System.Xml.XmlNode testNode = null;
            System.Xml.XmlNode refChild = null;
            System.Xml.XmlNode newChild = null;
            System.Xml.XmlNode insertedNode = null;

            testResults results = new testResults("Core0068NO");
            try
            {
                results.description = "The insertBefore(newChild,refChild) method returns "+
                    "the node being inserted.";
                //
                // Retrieve targeted data, define reference and new child nodes and insert
                // new child.
                //
                testNode = util.nodeObject(util.SECOND,-1);
                refChild = util.nodeObject(util.SECOND,util.FOURTH);
                newChild = util.createNode(util.ELEMENT_NODE,"newChild");
                insertedNode = testNode.InsertBefore(newChild,refChild);//.node.
                //
                // the returned node should have a nodeName = "newChild" 
                //
                computedValue = insertedNode.Name;
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }
            //
            // Write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            util.resetData();
            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0068NO --------------------------
        //
        //------------------------- test case core-0069NO -----------------------------
        //
        // Testing feature - If the newChild is already in the tree, The 
        //                   insertBefore(newChild,refChild) method first
        //                   remove it before the insertion takes place.
        //
        // Testing approach - Insert a node element (employeeId tag) that is already 
        //                    present in the tree.  The existing node should be
        //                    remove first and the new one inserted.  The node is
        //                    inserted at a different position in the tree to assure
        //                    that it was indeed inserted.
        //
        // Semantic Requirements: 38
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0069NO()
        {
            string computedValue = "";
            string expectedValue = "name employeeId";
            System.Xml.XmlNode refChild = null;
            System.Xml.XmlNode newChild = null;
            System.Xml.XmlElement testNode = null;

            testResults results = new testResults("Core0069NO");
            try
            {
                results.description = "If newChild is already in the tree, it is first "+
                    "removed before the insertion (from insertBefore"+
                    "(newChild,refChild) method) takes place.";
                //
                // Retrieve targeted data, define reference and new child and insert the
                // new child.
                //
                testNode = (System.Xml.XmlElement)util.nodeObject(util.SECOND,-1);
                newChild = util.nodeObject(util.SECOND,util.FIRST);
                refChild = util.nodeObject(util.SECOND,util.SIXTH);
                testNode.InsertBefore(newChild,refChild);//.node.
                //
                // the newChild should now be the previous to the last item and the
                // first child should be one that used to be at the second position.
                //
                computedValue += util.getSubNodes(testNode).Item(util.FIRST).Name+" ";
                computedValue += util.getSubNodes(testNode).Item(util.FIFTH).Name;
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }
            //
            // Write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            util.resetData();
            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0069NO --------------------------
        //
        //------------------------- test case core-0070NO -----------------------------
        //
        // Testing feature - The replaceChild(newChild,oldChild) method replaces 
        //                   the node oldChild with the node newChild.
        //
        // Testing approach - Replace the first element of the second employee
        //                    with a newly created node element and examine the
        //                    first position after the replacement operation is
        //                    done.  The new element should be "newChild". 
        //
        // Semantic Requirements: 39
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0070NO()
        {
            string computedValue = "";
            string expectedValue = "newChild";
            System.Xml.XmlElement testNode = null;
            System.Xml.XmlNode refChild = null;
            System.Xml.XmlNode newChild = null;
            System.Xml.XmlNode oldChild = null;

            testResults results = new testResults("Core0070NO");
            try
            {
                results.description = "The replaceChild(newChild,oldChild) method "+
                    "replaces the node oldChild with the node newChild";
                //
                // Create a new Element node to replace, define the node to be
                // replaced and replace it.
                //
                testNode = (System.Xml.XmlElement)util.nodeObject(util.SECOND,-1);
                oldChild = util.nodeObject(util.SECOND,util.FIRST);
                newChild = util.createNode(util.ELEMENT_NODE,"newChild");
                testNode.ReplaceChild(newChild,oldChild);//.node.
                //
                // Check that the first position contains the new node.
                //
                computedValue = util.getSubNodes(testNode).Item(util.FIRST).Name;
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }
            //
            // Write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            util.resetData();
            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0070NO ------------------------
        //
        //------------------------- test case core-0071NO ---------------------------
        //
        // Testing feature - If the newChild is already in the tree, it is
        //                   first removed before the new one is added 
        //
        // Testing approach - Retrieve the second employee and replace its last child
        //                    with its first child.  After the replacement operation
        //                    The first child should now be the one that used to be at 
        //                    the second position in the list and the last one should
        //                    be the one that used to be at the first position.
        //
        // Semantic Requirements: 40 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0071NO()
        {
            string computedValue = "";
            string expectedValue = "name employeeId";
            System.Xml.XmlElement testNode = null;
            System.Xml.XmlNode oldChild = null;
            System.Xml.XmlNode newChild = null;

            testResults results = new testResults("Core0071NO");
            try
            {
                results.description = "If newChild is already in the tree, it is first "+
                    "removed before the replace(from replaceChild"+
                    "(newChild,oldChild) method) takes place.";
                //
                // Retrieve targeted data, identify new and old children and replace
                // last child with the new child.
                //
                testNode = (System.Xml.XmlElement)util.nodeObject(util.SECOND,-1);
                newChild = util.nodeObject(util.SECOND,util.FIRST);
                oldChild = util.nodeObject(util.SECOND,util.SIXTH);
                testNode.ReplaceChild(newChild,oldChild);//.node.
                //
                // The first item in the list should be the one that used to be at the
                // second position and the last one should be the one that used to be at
                // the first position in the list.
                //
                computedValue += util.getSubNodes(testNode).Item(util.FIRST).Name+" ";
                computedValue += util.getSubNodes(testNode).Item(util.FIFTH).Name;
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }
            //
            // Write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            util.resetData();
            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0071NO --------------------------
        //
        //------------------------- test case core-0072NO -----------------------------
        //
        // Testing feature - The replaceChild(newChild,oldChild) method returns
        //                   the node being replaced.
        //
        // Testing approach - Replace the first element of the second employee
        //                    with a newly created node element and examine the
        //                    the value returned by the replaceChild(newChild,oldChild)
        //                    after the replacement operation is done.  The returned 
        //                    node should have a nodeName equal to "employeeId".
        //
        // Semantic Requirements: 41 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0072NO()
        {
            string computedValue = "";
            string expectedValue = "employeeId";
            System.Xml.XmlNode testNode = null;
            System.Xml.XmlNode oldChild = null;
            System.Xml.XmlNode newChild = null;
            System.Xml.XmlNode replacedNode = null;

            testResults results = new testResults("Core0072NO");
            try
            {
                results.description = "The replaceChild(newChild,oldChild) method returns "+
                    "the node being replaced.";
                //
                // Retrieve the targeted data, define new and old children and replace
                // old child with new child.
                //
                testNode = util.nodeObject(util.SECOND,-1);
                oldChild = util.nodeObject(util.SECOND,util.FIRST);
                newChild = util.createNode(util.ELEMENT_NODE,"newChild");
                replacedNode = testNode.ReplaceChild(newChild, oldChild);//.node.
                //
                // The returned node should be the one being replaced.
                //
                computedValue = replacedNode.Name;
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }
            //
            // Write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            util.resetData();
            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0072NO --------------------------
        //
        //------------------------- test case core-0073NO -----------------------------
        //
        // Testing feature - The removeChild(oldChild) method removes the node
        //                   indicated by oldChild.
        //
        // Testing approach - Retrieve the second employee and remove its first
        //                    child. After the removal operation takes place, the
        //                    second employee should have 5 children and the first
        //                    one should be the one that used to be at the second
        //                    position in the list.
        //
        // Semantic Requirements: 42 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0073NO()
        {
            string computedValue = "";
            string expectedValue = "name 5";
            System.Xml.XmlElement testNode = null;
            System.Xml.XmlNode oldChild = null;

            testResults results = new testResults("Core0073NO");
            try
            {
                results.description = "The removeChild(oldChild) method removes the "+
                    "node indicated by oldChild.";
                //
                // Retrieve targeted data, identify old child and remove it from the
                // list of children.
                //
                testNode = (System.Xml.XmlElement)util.nodeObject(util.SECOND,-1);
                oldChild = util.nodeObject(util.SECOND,util.FIRST);  
                testNode.RemoveChild(oldChild);//.node.
                //
                // Check that the node was indeed removed from the list.
                //
                computedValue += util.getSubNodes(testNode).Item(util.FIRST).Name+" ";
                computedValue += util.getSubNodes(testNode).Count;
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }
            //
            // Write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            util.resetData();
            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0073NO --------------------------
        //
        //------------------------- test case core-0074NO -----------------------------
        //
        // Testing feature - The removeChild(oldChild) method returns the node
        //                   removed.
        //
        // Testing approach - Remove the first element of the second employee
        //                    and examine the value returned by the 
        //                    removeChild(oldChild) after removal operation is 
        //                    done.  The returned node should have a tag name equal
        //                    to "employeeId".
        //
        // Semantic Requirements: 43
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0074NO()
        {
            string computedValue = "";
            string expectedValue = "employeeId";
            System.Xml.XmlNode testNode = null;
            System.Xml.XmlNode oldChild = null;
            System.Xml.XmlNode removedNode = null;

            testResults results = new testResults("Core0074NO");
            try
            {
                results.description = "The removeChild(oldChild) method returns the "+
                    "node removed.";
                //
                // Retrieve the targeted data and remove it.
                //
                testNode = util.nodeObject(util.SECOND,-1);
                oldChild = util.nodeObject(util.SECOND,util.FIRST);
                removedNode = testNode.RemoveChild(oldChild);//.node.
                //
                // The returned node should be the node removed.
                //
                computedValue = removedNode.Name;
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }
            //
            // Write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            util.resetData();
            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0074NO --------------------------
        //
        //------------------------- test case core-0075NO -----------------------------
        //
        // Testing feature - The appendChild(newChild) method adds the node newChild 
        //                   the end of the list of children of the node.
        //
        // Testing approach - Retrieve the second employee and append a new Element 
        //                    node to its list of children.  The last node in the
        //                    list is then retrieved and its nodeName attribute
        //                    examined.  It should be equal to "newChild". 
        //
        // Semantic Requirements: 44 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0075NO()
        {
            string computedValue = "";
            string expectedValue = "newChild";
            System.Xml.XmlElement testNode = null;
            System.Xml.XmlNode newChild = null;

            testResults results = new testResults("Core0075NO");
            try
            {
                results.description = "The appendChild(newChild) method adds the node "+
                    "newChild to the end of the list of children of "+
                    "the node.";
                //
                // Create a new Element node and append it to the end of the list.
                //
                testNode = (System.Xml.XmlElement)util.nodeObject(util.SECOND,-1);
                newChild = util.createNode(util.ELEMENT_NODE,"newChild");
                testNode.AppendChild(newChild);//.node.
                //
                // Retrieve the new last child.
                //
                computedValue = util.getSubNodes(testNode).Item(util.SEVENTH).Name;
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }
            //
            // Write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            util.resetData();
            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0075NO ------------------------
        //
        //------------------------- test case core-0076NO ---------------------------
        //
        // Testing feature - If the newChild is already in the tree, it is first 
        //                   removed before the new one is appended.
        //
        // Testing approach - Retrieve the second employee and its first child, then 
        //                    append the first child to the end of the list.  After
        //                    the append operation is done, retrieve the element at
        //                    at the top of the list and the one at the end of the 
        //                    list.  The last node should be the one that used to be
        //                    at the top of the list and the first one should be the
        //                    one that used to be second in the list.
        //
        // Semantic Requirements: 45
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0076NO()
        {
            string computedValue = "";
            string expectedValue = "name employeeId";
            System.Xml.XmlElement testNode = null;
            System.Xml.XmlNode newChild = null;

            testResults results = new testResults("Core0076NO");
            try
            {
                results.description = "If newChild is already in the tree, it is first " + "removed before the append takes place.";
                //
                // Retrieve targeted data, define the new child and append it to the
                // end of the list.
                //
                testNode = (System.Xml.XmlElement)util.nodeObject(util.SECOND,-1);
                newChild = util.nodeObject(util.SECOND,util.FIRST);
                testNode.AppendChild(newChild);//.node.
                //
                // Access the relevant new nodes and its nodeName attributes.
                //
                computedValue += util.getSubNodes(testNode).Item(util.FIRST).Name+" ";
                computedValue += util.getSubNodes(testNode).Item(util.SIXTH).Name;
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }
            //
            // Write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            util.resetData();
            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0076NO --------------------------
        //
        //------------------------- test case core-0077NO -----------------------------
        //
        // Testing feature - If the newChild is a DocumentFragment object then all
        //                   its content is added to the child list of this node.
        //
        // Testing approach - Create and populate a new DocumentFragment object and
        //                    append it to the second employee.  After the append
        //                    operation is done then retrieve the new nodes at the
        //                    end of the list, they should be the two Element nodes
        //                    from the DocumentFragment.
        //
        // Semantic Requirements: 46
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0077NO()
        {
            string computedValue = "";
            string expectedValue = "newChild1 newChild2";
            System.Xml.XmlElement testNode = null;
            System.Xml.XmlDocumentFragment newDocFragment = util.getDOMDocument().CreateDocumentFragment();

            testResults results = new testResults("Core0077NO");
            try
            {
                results.description = "If newChild is a DocumentFragment object, then the "+
                    "entire content of the DocumentFragment is appended "+
                    "to the child list of this node.";
                //
                // Populate the DocumentFragment object.
                //
                newDocFragment.AppendChild(util.createNode(util.ELEMENT_NODE,"newChild1"));
                newDocFragment.AppendChild(util.createNode(util.ELEMENT_NODE,"newChild2"));
                //
                // Retrieve targeted data and append new DocumentFragment object. 
                //
                testNode = (System.Xml.XmlElement)util.nodeObject(util.SECOND,-1);
                testNode.AppendChild(newDocFragment);//.node.
                //
                // Retrieve all the new nodes from the proper position.
                //
                computedValue += util.getSubNodes(testNode).Item(util.SEVENTH).Name+" ";
                computedValue += util.getSubNodes(testNode).Item(util.EIGHT).Name;
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }
            //
            // Write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            util.resetData();
            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0077NO --------------------------
        //
        //------------------------- test case core-0078NO -----------------------------
        //
        // Testing feature - The appendChild(newChild) method returns the node
        //                   added.
        //
        // Testing approach - Append a newly created node to the child list of the 
        //                    second employee and examine the returned value
        //                    The returned value should be "newChild".
        //
        // Semantic Requirements: 47
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0078NO()
        {
            string computedValue = "";
            string expectedValue = "newChild";
            System.Xml.XmlNode testNode = null;
            System.Xml.XmlNode appendedNode = null;
            System.Xml.XmlNode newChild = null;

            testResults results = new testResults("Core0078NO");

            results.description = "The appendChild(newChild) method returns the node "+
                "added.";
            //
            // Retrieve the targeted data and append a new node to it.
            //
            testNode = util.nodeObject(util.SECOND,-1);
            newChild = util.createNode(util.ELEMENT_NODE,"newChild");
            appendedNode = testNode.AppendChild(newChild);//.node.
            //
            // The returned node should be the node appended.
            //
            computedValue = appendedNode.Name;
            //
            // Write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            util.resetData();
            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0078NO --------------------------
        //
        //------------------------- test case core-0079NO -----------------------------
        //
        // Testing feature - The hasChildNodes method returns true if the node has
        //                   any children. 
        //
        // Testing approach - Retrieve the root node (tag name = "staff") and
        //                    invoke its hasChildNodes.Item() method.  It should return
        //                    the value true.
        //
        // Semantic Requirements: 48
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0079NO()
        {
            string computedValue = "";
            string expectedValue = "True";
            System.Xml.XmlNode testNode = null;

            testResults results = new testResults("Core0079NO");

            results.description = "The hasChildNodes method returns true if the "+
                "node has any children.";
            //
            // Retrieve the targeted data and access its hasChilNodes method.
            //
            testNode = util.nodeObject(util.SECOND,-1);
            computedValue = testNode.HasChildNodes.ToString();//.Item();//.node.
            //
            // Write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0079NO --------------------------
        //
        //------------------------- test case core-0080NO -----------------------------
        //
        // Testing feature - The hasChildNodes method returns false if the node has
        //                   no children.
        //
        // Testing approach - Retrieve the Text node inside the first child of the
        //                    second employee and invoke its hasChildNodes.Item() method.  
        //                    It should return the value false as this node has no
        //                    children.
        //
        // Semantic Requirements: 49
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0080NO()
        {
            string computedValue = "";
            string expectedValue = "False";
            System.Xml.XmlNode testNode = null;

            testResults results = new testResults("Core0080NO");
            try
            {
                results.description = "The hasChildNodes method returns false if the "+
                    "node has no children.";
                //
                // Retrieve the targeted data and access its hasChildNodes method.
                //
                testNode = util.nodeObject(util.SECOND,util.FIRST);
                System.Xml.XmlNode textNode = testNode.FirstChild;//.node.
                computedValue = textNode.HasChildNodes.ToString();//.Item();
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }
            //
            // Write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0080NO --------------------------
        //
        //------------------------- test case core-0081NO -----------------------------
        //
        // Testing feature - The cloneNode(deep) method returns a copy of the node only
        //                   if deep = false.
        //
        // Testing approach - Retrieve the second employee and invoke its 
        //                    cloneNode(deep) method with deep = false.  The
        //                    method should clone this node only.  The nodeName,
        //                    and length of the childNode list are checked, 
        //                    they should be "employee" and 0.
        //
        // Semantic Requirements: 50 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0081NO()
        {
            string computedValue = "";
            string expectedValue = "employee 0";
            System.Xml.XmlNode testNode = null;
            System.Xml.XmlNode clonedNode = null;

            testResults results = new testResults("Core0081NO");
            try
            {
                results.description = "The cloneNode(deep) method returns a copy of this "+
                    "node only if deep = false.";
                //
                // Retrieve the targeted data and access its cloneNode method.
                //
                testNode = util.nodeObject(util.SECOND,-1);
                clonedNode = testNode.CloneNode(false);//.node.
                //
                // Retrieve values of the cloned node.
                //      
                computedValue += clonedNode.Name+" ";
                computedValue += clonedNode.ChildNodes.Count; 
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }
            //
            // Write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0081NO --------------------------
        //
        //------------------------- test case core-0082NO -----------------------------
        //
        // Testing feature - The cloneNode(deep) method returns a copy of the node and 
        //                   the subtree under it if deep = true.
        //
        // Testing approach - Retrieve the second employee and invoke its
        //                    cloneNode(deep) method with deep = true.  The
        //                    method should clone this node and the subtree under
        //                    it.  The tag name of each child of the returned
        //                    node is checked to insure the entire subtree under
        //                    the second employee was cloned.  
        //
        // Semantic Requirements: 51
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0082NO()
        {
            string computedValue = "";
            string expectedValue = "employeeId name position salary gender address ";
            System.Xml.XmlNode testNode = null;
            System.Xml.XmlNodeList subTree = null;
            System.Xml.XmlNode clonedNode = null;

            testResults results = new testResults("Core0082NO");
            try
            {
                results.description = "The cloneNode(deep) method returns a copy of this "+
                    "node and the subtree under it if deep = true.";
                //
                // Retrieve the targeted data and invoke its cloneNode method.
                //
                testNode = util.nodeObject(util.SECOND,-1);
                clonedNode = testNode.CloneNode(true);//.node.
                subTree = clonedNode.ChildNodes;
                //
                // Retrieve the cloned node children.
                //
                for (int index = 0;index < subTree.Count; index++) 
                    computedValue += subTree.Item(index).Name+" ";
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }
            //
            // Write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0082NO --------------------------
        //
        //------------------------- test case core-0083NO -----------------------------
        //
        // Testing feature - The duplicate node returned by the cloneNode(deep) method 
        //                   has no parent (parentNode = null).
        //
        // Testing approach - Retrieve the second employee and invoke its
        //                    cloneNode(deep) method with deep = false.  The
        //                    duplicate node returned by the method should have its 
        //                    parentNode attribute set to null.
        //
        // Semantic Requirements: 52
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0083NO()
        {
            object computedValue = null;
            object expectedValue = null;
            System.Xml.XmlNode testNode = null;
            System.Xml.XmlNode clonedNode = null;

            testResults results = new testResults("Core0083NO");
            try
            {
                results.description = "The duplicate node returned by the cloneNode(deep) "+
                    "method has no parent (parentNode = null).";
                //
                // Retrieve the targeted data and invoke the cloneNode method.
                //
                testNode = util.nodeObject(util.SECOND,-1);
                clonedNode = testNode.CloneNode(false);//.node.
                //
                // Its parentNode attribute should be null.
                //
                computedValue = clonedNode.ParentNode;
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }
            //
            // Write out results.
            //
            results.expected = (expectedValue == null).ToString();
            results.actual = (computedValue == null).ToString();

            util.resetData();
            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0083NO --------------------------
        //
        //------------------------- test case core-0084NO -----------------------------
        //
        // Testing feature - The cloneNode(deep) method does not copy text unless it is
        //                   deep cloned. (test for deep clone = false)
        //
        // Testing approach - Retrieve the fourth child of the second employee and 
        //                    invoke its cloneNode(deep) method with deep = false.  The
        //                    duplicate node returned by the method should not copy any
        //                    text data contained in this node. 
        //
        // Semantic Requirements: 53
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0084NO()
        {
            string testName = "core-0084NO";
            object computedValue = null;
            object expectedValue = null;
            System.Xml.XmlNode testNode = null;
            System.Xml.XmlNode clonedNode = null;

            testResults results = new testResults("Core0084NO");
            try
            {
                results.description = "The cloneNode(deep) method does not copy any text "+
                    "unless it is deep cloned(deep = false).";
                //
                // Retrieve the targeted data and invoke its clonedNode method.
                //
                testNode = util.nodeObject(util.SECOND,util.FOURTH);
                clonedNode = testNode.CloneNode(false);//.node.
                //
                // The cloned node should have no text data in it.
                //
                computedValue = clonedNode.LastChild;
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }
            //
            //  Write out results.
            //
            results.expected = (expectedValue == null).ToString();
            results.actual = (computedValue == null).ToString();

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0084NO --------------------------
        //
        //------------------------- test case core-0085NO -----------------------------
        //
        // Testing feature - The cloneNode(deep) method does not copy text unless it is
        //                   deep cloned. (test for deep clone = true)
        //
        // Testing approach - Retrieve the fourth child of the second employee and
        //                    invoke its cloneNode(deep) method with deep = true.  The
        //                    duplicate node returned by the method should copy any
        //                    text data contained in this node.
        //
        // Semantic Requirements: 53
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0085NO()
        {
            string computedValue = "";
            string expectedValue = "35,000";
            System.Xml.XmlNode testNode = null;
            System.Xml.XmlNode clonedNode = null;

            testResults results = new testResults("Core0085NO");
            try
            {
                results.description = "The cloneNode(deep) method does not copy any text "+
                    "unless it is deep cloned(deep = true).";
                //
                // Retrieve the targeted data and invoke its cloneNode method.
                //
                testNode = util.nodeObject(util.SECOND,util.FOURTH);
                clonedNode = testNode.CloneNode(true);//.node.
                //
                // Retrieve the text data inside the cloned node.
                //
                computedValue = clonedNode.LastChild.Value;
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }
            //
            // Write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0085NO --------------------------
        //
        //------------------------- test case core-0086NO -----------------------------
        //
        // Testing feature - If the cloneNode(deep) method was used to clone an Element
        //                   node, all the attributes of the Element are copied (and
        //                   their values).
        //
        // Testing approach - Retrieve the last child of the second employee and
        //                    invoke its cloneNode(deep) method with deep = true.  The
        //                    duplicate node returned by the method should copy the
        //                    attributes associated with this node. 
        //
        // Semantic Requirements: 54
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0086NO()
        {
            string testName = "core-0086NO";
            string computedValue = "";
            string expectedValue = "";
            System.Xml.XmlNode testNode = null;
            System.Xml.XmlNode clonedNode = null;

            testResults results = new testResults("Core0086NO");
            try
            {
                results.description = "If the cloneNode(deep) method was used to clone an "+
                    "Element node then all the attributes associated "+
                    "associated with this node are copied too."; 
                //
                // Retrieve the targeted data and invoke its cloneNode method.
                //
                testNode = util.nodeObject(util.SECOND,util.SIXTH);
                clonedNode = testNode.CloneNode(true);//.node.
                //
                // Retreive cloned node and its attributes.
                //
                computedValue += clonedNode.Attributes.Item(0).Name+" ";
                computedValue += clonedNode.Attributes.Item(1).Name;
                //
                // Determine order of NamedNodeMap items.
                //
                if (computedValue.Substring(0,1) == "d" && computedValue.Substring(1,1) == "o")
                    expectedValue = "domestic street";
                else
                    expectedValue = "street domestic";
            }
            catch(Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }

            //
            // Write out results.
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0086NO --------------------------
        //
        //------------------------- test case core-0087NO -----------------------------
        //
        // Testing feature - The "nodeValue" attribute of a node raises a
        //                   NO_MODIFICATION_ALLOWED_ERR DOM exception
        //                   if the node is readonly. 
        //
        // Testing approach - Retrieve the Text node inside the Entity node named 
        //                    "ent1" and attempt to change its nodeValue attribute.
        //                    Since the descendants of Entity nodes are readonly, the
        //                    desired exception should be raised.
        //
        // Semantic Requirements: 55 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0087NO()
        {
            string computedValue = "";
            System.Xml.XmlEntity testNode = null;
            System.Xml.XmlText entityDesc = null;
            string expectedValue = "System.ArgumentException";//util.NO_MODIFICATION_ALLOWED_ERR;

            testResults results = new testResults("Core0087NO");
            try
            {
                results.description = "The \"Value\" attribute of a node raises a "+
                    "NO_MODIFICATION_ALLOWED_ERR Exception if the "+
                    "node is readonly.";
                //
                // Retrieve the targeted data.
                //
                testNode = util.getEntity("ent1");
                entityDesc = (System.Xml.XmlText)testNode.FirstChild;
                //
                // attempt to set a value on a readonly node should raise an exception.
                //
                try 
                {
                    entityDesc.Value = "ABCD"; 
                }
                catch(System.Exception ex) 
                {
                    computedValue = ex.GetType ().FullName; 
                }
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }

            results.expected = expectedValue;
            results.actual = computedValue;

            util.resetData();
            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0087NO -------------------------
        //
        //------------------------- test case core-0088NO -----------------------------
        //
        // Testing feature - The "insertBefore" method of a node raises a
        //                   NO_MODIFICATION_ALLOWED_ERR Exception if 
        //                   the node is readonly.
        //
        // Testing approach - Retrieve the first EntityReference inside the second 
        //                    employee and invoke the insertBefore(newChild,refChild) 
        //                    method on its first descendant.  Descendants of
        //                    EntityReference nodes are readonly and therefore the
        //                    desired exception should be raised.  This test also 
        //                    makes use of the "createElement" method from the 
        //                    Document interface.
        //
        // Semantic Requirements: 56
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0088NO()
        {
            string computedValue = "";
            System.Xml.XmlNode testNode = null;
            System.Xml.XmlEntityReference entityRefNode = null;
            System.Xml.XmlNode refChild = null;
            System.Xml.XmlElement newChild = (System.Xml.XmlElement)util.createNode(util.ELEMENT_NODE,"newChild");
            string expectedValue = "System.InvalidOperationException";//util.NO_MODIFICATION_ALLOWED_ERR;
 
            testResults results = new testResults("Core0088NO");
            try
            {
                results.description = "The \"insertBefore()\" method of a node raises "+
                    "a NO_MODIFICATION_ALLOWED_ERR Exception "+
                    "if this node is readonly.";
                //
                // Retrieve targeted data and define reference child.
                //
                testNode = util.nodeObject(util.SECOND,util.SIXTH);
                entityRefNode = (System.Xml.XmlEntityReference)testNode.FirstChild;//.node.
                refChild = entityRefNode.FirstChild;
                //
                // Attempt to insert a node to an EntityReference descendant should raise 
                // an exception.
                //
                try 
                {
                    entityRefNode.InsertBefore(newChild,refChild);
                }
                catch(InvalidOperationException ex) 
                {
                    computedValue = ex.GetType ().FullName; 
                }
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }
            results.expected = expectedValue;
            results.actual = computedValue;

            util.resetData();
            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0088NO -------------------------
        //
        //------------------------- test case core-0089NO ----------------------------
        //
        // Testing feature - The "replaceChild" method of a node raises a
        //                   NO_MODIFICATION_ALLOWED_ERR Exception if the 
        //                   node is readonly.
        //
        // Testing approach - Retrieve the first EntityReference inside the second
        //                    employee and invoke the replaceChild(newChild,oldChild)
        //                    method where oldChild is one of the EntityReference 
        //                    descendants.  Descendants of EntityReference nodes are 
        //                    readonly and therefore the desired exception should be 
        //                    raised.  This test also makes use of the "createElement" 
        //                    method from the Document interface.
        //
        // Semantic Requirements: 57
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0089NO()
        {
            string computedValue = "";
            System.Xml.XmlNode testNode = null;
            System.Xml.XmlEntityReference entityRefNode = null;
            System.Xml.XmlNode oldChild = null;
            System.Xml.XmlElement newChild = (System.Xml.XmlElement)util.createNode(util.ELEMENT_NODE,"newChild");
            string expectedValue = "System.ArgumentException";//util.NO_MODIFICATION_ALLOWED_ERR;

            testResults results = new testResults("Core0089NO");
            try
            {
                results.description =  "The \"replaceChild(newChild, oldChild)\" method "+
                    "of a node raises a<br>NO_MODIFICATION_ALLOWED_ERR "+
                    " Exception if this node is readonly.";
                //
                // Retrieve targeted data and define oldChild.
                //
                testNode = util.nodeObject(util.SECOND,util.SIXTH);
                entityRefNode = (System.Xml.XmlEntityReference)testNode.FirstChild; //.node.
                oldChild = entityRefNode.FirstChild;
                //
                // Attempt to replace a descendant of an EntityReference should raise an
                // exception.
                //
                try 
                {
                    entityRefNode.ReplaceChild(newChild,oldChild);
                }
                catch(ArgumentException ex) 
                {
                    computedValue = ex.GetType ().FullName; 
                }
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }
            results.expected = expectedValue;
            results.actual = computedValue;

            util.resetData();
            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0089NO -------------------------
        //
        //------------------------- test case core-0090NO ----------------------------
        //
        // Testing feature - The "removeChild" method of a node raises a
        //                   NO_MODIFICATION_ALLOWED_ERR Exception if the 
        //                   node is readonly.
        //
        // Testing approach - Retrieve the first EntityReference inside the second
        //                    employee and invoke its removeChild(oldChild) method
        //                    where oldChild is one of the EntityReference descendants.
        //                    Descendants of EntityReference nodes are readonly and
        //                    therefore the desired exception should be raised. 
        //
        // Semantic Requirements: 58
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0090NO()
        {
            string computedValue = "";
            System.Xml.XmlEntityReference entityRefNode = null;
            System.Xml.XmlNode testNode = null;
            System.Xml.XmlNode oldChild = null;
            string expectedValue = "System.ArgumentException";//util.NO_MODIFICATION_ALLOWED_ERR;

            testResults results = new testResults("Core0090NO");
            try
            {
                results.description = "The \"removeChild(oldChild)\" method of a node "+
                    "raises NO_MODIFICATION_ALLOWED_ERR Exception "+
                    "if this node is readonly.";
                //
                // Retreive targeted data and define oldChild.
                //
                testNode = util.nodeObject(util.SECOND,util.SIXTH);
                entityRefNode = (System.Xml.XmlEntityReference)testNode.FirstChild;//.node.
                oldChild = entityRefNode.FirstChild;
                //
                // Attempt to remove a descendant of an EntityReference node should
                // raise an exception.
                //
                try 
                {
                    entityRefNode.RemoveChild(oldChild);
                }
                catch(ArgumentException ex) 
                {
                    computedValue = ex.GetType ().FullName; 
                }
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }

            results.expected = expectedValue;
            results.actual = computedValue;

            util.resetData();
            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0090NO -------------------------
        //
        //------------------------- test case core-0091NO ----------------------------
        //
        // Testing feature - The "appendChild" method of a node raises a
        //                   NO_MODIFICATION_ALLOWED_ERR Exception if 
        //                   the node is readonly.
        //
        // Testing approach - Retrieve the first EntityReference inside the second
        //                    employee and invoke its append(newChild) method.  
        //                    Descendants of EntityReference nodes are readonly and
        //                    therefore attempts to append nodes to such podes  
        //                    should raise the desired exception.  This test 
        //                    also makes use of the "createElement" method from
        //                    the Document interface.
        //
        // Semantic Requirements: 59
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0091NO()
        {
            string computedValue = "";
            System.Xml.XmlEntityReference entityRefNode = null;
            System.Xml.XmlNode testNode = null;
            System.Xml.XmlElement newChild = (System.Xml.XmlElement)util.createNode(util.ELEMENT_NODE,"newChild");
            string expectedValue = "System.InvalidOperationException";//util.NO_MODIFICATION_ALLOWED_ERR;

            testResults results = new testResults("Core0091NO");
            try
            {
                results.description = "The \"appendChild(newChild)\" method of a node "+
                    "raises NO_MODIFICATION_ALLOWED_ERR Exception "+
                    "if this node is readonly.";
                //
                // Retrieve targeted data. 
                //
                testNode = util.nodeObject(util.SECOND,util.SIXTH);
                entityRefNode = (System.Xml.XmlEntityReference)testNode.FirstChild;//.node.
                //
                // Attempt to append nodes to descendants of EntityReference nodes should 
                // raise an exception.
                //
                try 
                {
                    entityRefNode.AppendChild(newChild);
                }
                catch(InvalidOperationException ex) 
                {
                    computedValue = ex.GetType ().FullName; 
                }
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }
            results.expected = expectedValue;
            results.actual = computedValue;

            util.resetData();
            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0091NO -------------------------
        //
        //------------------------- test case core-0092NO -----------------------------
        //
        // Testing feature - The "insertBefore()" method of a node raises
        //                   a System.ArgumentException Exception if this node
        //                   is of a type that does not allow children of the
        //                   type of "newChild" to be inserted.
        //
        // Testing Approach - Retrieve the root node and attempt to insert a newly 
        //                    created Attr node.  An Element node can not have
        //                    children of the "Attr" type, therefore the desired
        //                    exception should be raised. 
        //
        // Semantic Requirements: 60
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0092NO()
        {
            string computedValue = "";
            System.Xml.XmlElement rootNode = null;
            System.Xml.XmlAttribute newChild = (System.Xml.XmlAttribute)util.createNode(util.ATTRIBUTE_NODE,"newAttribute");
            System.Xml.XmlNode refChild = null;
            string expectedValue = "System.InvalidOperationException";

            testResults results = new testResults("Core0092NO");
            try
            {
                results.description = "The \"insertBefore()\" method of a node raises "+
                    "a System.ArgumentException Exception if this node "+
                    "does not allow nodes of type of \"newChild\" to be "+
                    "inserted.";
                //
                // Retrieve targeted data.
                //
                rootNode = util.getRootNode();
                refChild = util.nodeObject(util.SECOND,-1);
                //
                // Attempt to insert an invalid child should raise an exception.
                //
                try 
                {
                    rootNode.InsertBefore(newChild,refChild);//.node
                }
                catch(System.Exception ex) 
                {
                    computedValue = ex.GetType().ToString(); 
                }
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }

            results.expected = expectedValue;
            results.actual = computedValue;

            util.resetData();
            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0092NO -------------------------
        //
        //------------------------- test case core-0093NO ----------------------------
        //
        // Testing feature - The "insertBefore()" method of a node raises
        //                   a System.ArgumentException Exception if the node
        //                   to be inserted is one of this node's ancestors.
        //
        // Testing Approach - Retrieve the second employee and attempt to insert
        //                    a node that is one of its ancestors (root node).
        //                    An attempt to insert such a node should raise the
        //                    desired exception.
        //
        // Semantic Requirements: 61
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0093NO()
        {
            string computedValue = "";
            System.Xml.XmlNode newChild = null;
            System.Xml.XmlNode refChild = null;
            string expectedValue = "System.ArgumentException";

            testResults results = new testResults("Core0093NO");
            try
            {
                results.description = "The \"insertBefore()\" method of a node raises "+
                    "an System.ArgumentException Exception if the node "+
                    "to be inserted is one of this node's ancestors.";
                //
                // Retrieve targeted data and define reference and new childs.
                //
                newChild = util.getRootNode();
                System.Xml.XmlElement testNode = (System.Xml.XmlElement)util.nodeObject(util.SECOND,-1);
                refChild = util.getSubNodes(testNode).Item(util.FIRST);
                //
                // Attempt to insert a node that is one of this node ancestors should
                // raise an exception.
                //
                try 
                {
                    testNode.InsertBefore(newChild, refChild);//.node.
                }
                catch(System.Exception ex) 
                {
                    computedValue = ex.GetType().ToString();
                }

            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }
            results.expected = expectedValue;
            results.actual = computedValue;

            util.resetData();

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0093NO -------------------------
        //
        //------------------------- test case core-0094NO -----------------------------
        //
        // Testing feature - The "replaceChild" method of a node raises a
        //                   System.ArgumentException Exception if this node
        //                   is of a type that does not allow children of the
        //                   type of "newChild" to be inserted.
        //
        // Testing Approach - Retrieve the root node and attempt to replace one of
        //                    its children with a newly created Attr node.  An 
        //                    Element node can not have children of the "Attr"
        //                    type, therefore the desired exception should be raised.
        //
        // Semantic Requirements: 62
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0094NO()
        {
            string computedValue = "";
            System.Xml.XmlElement rootNode = null;
            System.Xml.XmlAttribute newChild = (System.Xml.XmlAttribute)util.createNode(util.ATTRIBUTE_NODE,"newAttribute");
            System.Xml.XmlNode oldChild = null;
            string expectedValue = "System.InvalidOperationException";

            testResults results = new testResults("Core0094NO");
            try
            {
                results.description = "The \"replaceChild()\" method of a node raises "+
                    "a System.ArgumentException Exception if this node "+
                    "does not allow nodes of type of \"newChild\".";
                //
                // Retrieve targeted data and define oldChild. 
                //
                rootNode = util.getRootNode();
                oldChild = util.nodeObject(util.SECOND,-1);
                //
                // Attempt to replace a child with an invalid child should raise an exception.
                //
                try 
                {
                    rootNode.ReplaceChild(newChild,oldChild);
                }
                catch(System.Exception ex) 
                {
                    computedValue = ex.GetType().ToString(); 
                }
    
            }
            catch(System.Exception ex) 
            {
                computedValue = ex.Message; 
            }
            results.expected = expectedValue;
            results.actual = computedValue;

            util.resetData();

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0094NO -------------------------
        //
        //------------------------- test case core-0095NO ----------------------------
        //
        // Testing feature - The "replaceChild()" method of a node raises
        //                   a System.ArgumentException Exception if the node
        //                   to be inserted is one of this node's ancestors.
        //
        // Testing Approach - Retrieve the second employee and attempt to replace one
        //                    of its children with an ancestor node (root node).
        //                    An attempt to make such a replacement should raise the
        //                    desired exception.
        //
        // Semantic Requirements: 63
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0095NO()
        {
            string computedValue = "";
            System.Xml.XmlNode newChild = null;
            System.Xml.XmlNode oldChild = null;
            System.Xml.XmlElement testNode = null;
            string expectedValue = "System.InvalidOperationException";

            testResults results = new testResults("Core0095NO");
            try
            {
                results.description = "The \"replaceChild()\" method of a node raises "+
                    "a System.ArgumentException Exception if the node "+
                    "to be put is one of this node's ancestors.";
                //
                // Retrieve targeted data and define new and old childs. 
                //
                newChild = util.getRootNode();
                testNode = (System.Xml.XmlElement)util.nodeObject(util.SECOND,-1);
                oldChild = util.getSubNodes(testNode).Item(util.FIRST);
                //
                // Attempt to replace a child with an ancestor should raise an exception.
                //
                try 
                {
                    testNode.ReplaceChild(newChild,oldChild);//.node.
                }
                catch(InvalidOperationException ex) 
                {
                    computedValue = ex.GetType().ToString(); 
                }

            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }

            results.expected = expectedValue;
            results.actual = computedValue;

            util.resetData();

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0095NO -------------------------
        //
        //------------------------- test case core-0096NO ----------------------------
        //
        // Testing feature - The "appendChild" method of a node raises a
        //                   System.ArgumentException Exception if this node
        //                   is of a type that does not allow children of the
        //                   type of "newChild".
        //
        // Testing Approach - Retrieve the root node and attempt to append a
        //                    newly created Attr node to it.  An Element
        //                    node can not have children of the "Attr" type,
        //                    therefore the desired exception should be raised.
        //
        // Semantic Requirements: 64
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0096NO()
        {
            string computedValue = "";
            System.Xml.XmlElement rootNode = null;
            System.Xml.XmlAttribute newChild = (System.Xml.XmlAttribute)util.createNode(util.ATTRIBUTE_NODE,"newAttribute");
            string expectedValue = "System.InvalidOperationException";

            testResults results = new testResults("Core0096NO");
            try
            {
                results.description = "The \"appendChild()\" method of a node raises "+
                    "a System.ArgumentException Exception if this node "+
                    "does not allow nodes of type of \"newChild\".";
                //
                // Retrieve the targeted data.
                //
                rootNode = util.getRootNode();
                //
                // Attempt to append an invalid child should raise an exception.
                //
                try 
                {
                    rootNode.AppendChild(newChild);
                }
                catch(System.Exception ex) 
                {
                    computedValue = ex.GetType().ToString(); 
                }
    
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }
            results.expected = expectedValue;
            results.actual = computedValue;

			util.resetData();

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0096NO -------------------------
        //
        //------------------------- test case core-0097NO ----------------------------
        //
        // Testing feature - The "appendChild" method of a node raises
        //                   an System.ArgumentException Exception if the node
        //                   to be appended is one of this node's ancestors.
        //
        // Testing Approach - Retrieve the second employee and attempt to append to 
        //                    it an ancestor node (root node). An attempt to make 
        //                    such an insertion should raise the desired exception.
        //
        // Semantic Requirements: 65
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0097NO()
        {
            string computedValue = "";
            System.Xml.XmlNode newChild = null;
            System.Xml.XmlNode testNode = null;
            string expectedValue = "System.ArgumentException";

            testResults results = new testResults("Core0097NO");
            try
            {
                results.description = "The \"appendChild()\" method of a node raises "+
                    "a System.ArgumentException Exception if the node "+
                    "to append is one of this node's ancestors.";
                //
                // Retrieve the targeted data and define the new child.
                //
                newChild = util.getRootNode();
                testNode = util.nodeObject(util.SECOND,-1);
                //
                // Attempt to replace a child with an ancestor should raise an exception.
                //
                try 
                {
                    testNode.AppendChild(newChild);//.node.
                }
                catch(System.Exception ex) 
                {
                    computedValue = ex.GetType().ToString(); 
                }
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }

            results.expected = expectedValue;
            results.actual = computedValue;

            util.resetData();

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0097NO -------------------------
        //
        //------------------------- test case core-0098NO ----------------------------
        //
        // Testing feature - The "insertBefore" method of a node raises a
        //                   NOT_FOUND_ERR Exception if the reference child is not
        //                   child of this node.
        //
        // Testing Approach - Retrieve the second employee and attempt to insert
        //                    a new node before a reference node that is not
        //                    a child of this node.  An attempt to insert before a
        //                    non child node should raise the desired exception.
        //
        // Semantic Requirements: 66
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0098NO()
        {
            string computedValue = "";
            System.Xml.XmlElement newChild = (System.Xml.XmlElement)util.createNode(util.ELEMENT_NODE,"newChild");
            System.Xml.XmlElement refChild = (System.Xml.XmlElement)util.createNode(util.ELEMENT_NODE,"refChild");
            System.Xml.XmlNode testNode = null;
            string expectedValue = "System.ArgumentException";//util.NOT_FOUND2_ERR;

            testResults results = new testResults("Core0098NO");
            try
            {
                results.description = "The \"insertBefore\" method of a node raises "+
                    "a NOT_FOUND_ERR Exception if the reference "+
                    "child is not a child of this node.";
                //
                // Retrieve targeted data.
                //
                testNode = util.nodeObject(util.SECOND,-1);
                //
                // Attempt to insert before a reference child that is not a child of
                // this node should raise an exception.
                //
                try 
                {
                    testNode.InsertBefore(newChild,refChild);//.node.
                }
                catch(ArgumentException ex) 
                {
                    computedValue = ex.GetType ().FullName; 
                }
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }
            results.expected = expectedValue;
            results.actual = computedValue;

            util.resetData();

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0098NO -----------------------
        //
        //------------------------- test case core-0099NO --------------------------
        //
        // Testing feature - The "replaceChild" method of a node raises a
        //                   NOT_FOUND_ERR Exception if the old child is not
        //                   child of this node.
        //
        // Testing Approach - Retrieve the second employee and attempt to replace 
        //                    a node that is not one of its children.  An attempt 
        //                    to replace such a node should raise the desired 
        //                    exception.
        //
        // Semantic Requirements: 67
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0099NO()
        {
            string computedValue = "";
            System.Xml.XmlElement newChild = (System.Xml.XmlElement)util.createNode(util.ELEMENT_NODE,"newChild");
            System.Xml.XmlElement oldChild = (System.Xml.XmlElement)util.createNode(util.ELEMENT_NODE,"oldChild");
            System.Xml.XmlNode testNode = null;
            string expectedValue = "System.ArgumentException";//util.NOT_FOUND2_ERR;

            testResults results = new testResults("Core0099NO");
            try
            {
                results.description = "The \"replaceChild\" method of a node raises "+
                    "a NOT_FOUND_ERR Exception if the old child "+
                    "is not a child of this node.";
                //
                // Retrieve the targeted data..
                //
                testNode = util.nodeObject(util.SECOND,-1);
                //
                // Attempt to replace a non child node should raise an exception.
                //
                try 
                {
                    testNode.ReplaceChild(newChild,oldChild);//.node.
                }
                catch(ArgumentException ex) 
                {
                    computedValue = ex.GetType ().FullName; 
                }
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }
            results.expected = expectedValue;
            results.actual = computedValue;

            util.resetData();

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0099NO -----------------------
        //
        //------------------------- test case core-0100NO --------------------------
        //
        // Testing feature - The "removeChild" method of a node raises a
        //                   NOT_FOUND_ERR Exception if the old child is not
        //                   child of this node.
        //
        // Testing Approach - Retrieve the second employee and attempt to remove
        //                    a node that is not one of its children.  An attempt
        //                    to remove such a node should raise the desired
        //                    exception.
        //
        // Semantic Requirements: 68
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0100NO()
        {
            string computedValue = "";
            System.Xml.XmlElement oldChild = (System.Xml.XmlElement)util.createNode(util.ELEMENT_NODE,"oldChild");
            System.Xml.XmlNode testNode = null;
            string expectedValue = typeof (ArgumentException).FullName;//util.NOT_FOUND3_ERR;

            testResults results = new testResults("Core0100NO");
            try
            {
                results.description = "The \"removeChild\" method of a node raises "+
                    "a NOT_FOUND_ERR Exception if the old "+
                    "child is not a child of this node.";
                //
                // Retrieve targeted data.
                //
                testNode = util.nodeObject(util.SECOND,-1);
                //
                // Attempt to remove a non child node should raise an exception.
                //
                try 
                {
                    testNode.RemoveChild(oldChild);//.node.
                }
                catch(ArgumentException ex) 
                {
                    computedValue = ex.GetType ().FullName; 
                }
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }
            results.expected = expectedValue;
            results.actual = computedValue;

			util.resetData();

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0100NO -----------------------
        //
        //------------------------- test case core-0101NO ----------------------------
        //
        // Testing feature - The "insertBefore" method of a node raises a
        //                   System.ArgumentException Exception if the new child was
        //                   created from a different document than the one that 
        //                   created this node.
        //
        // Testing Approach - Retrieve the second employee and attempt to insert
        //                    a new child that was created from a different
        //                    document than the one that created the second employee.
        //                    An attempt to insert such a child should raise 
        //                    the desired exception.
        //
        // Semantic Requirements: 69
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0101NO()
        {
            string computedValue = "";
            System.Xml.XmlElement newChild = util.getOtherDOMDocument().CreateElement("newChild");
            System.Xml.XmlNode refChild = null;
            System.Xml.XmlElement testNode = null;
            string expectedValue = "System.ArgumentException";

            testResults results = new testResults("Core0101NO");
            try
            {
                results.description = "The \"insertBefore\" method of a node raises "+
                    "a System.ArgumentException Exception if the new "+
                    "child was created from a document different "+
                    "from the one that created this node.";
                //
                // Retrieve targeted data and define reference child.
                //
                testNode = (System.Xml.XmlElement)util.nodeObject(util.SECOND,-1);
                refChild = util.getSubNodes(testNode).Item(util.FOURTH);
                //
                // Attempt to insert a child from a different document should raise an
                // exception.
                //
                try 
                {
                    testNode.InsertBefore(newChild,refChild);//.node.
                }
                catch(System.Exception ex) 
                {
                    computedValue = ex.GetType().ToString(); 
                }
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }

            results.expected = expectedValue;
            results.actual = computedValue;

			util.resetData();
            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0101NO -----------------------
        //
        //------------------------- test case core-0102NO --------------------------
        //
        // Testing feature - The "replaceChild" method of a node raises a
        //                   System.ArgumentException Exception if the new child was
        //                   created from a different document than the one that
        //                   created this node.
        //
        // Testing Approach - Retrieve the second employee and attempt to  
        //                    replace one of its children with a node created
        //                    from a different document.  An attempt to make such
        //                    replacement should raise the desired exception.
        //
        // Semantic Requirements: 70
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0102NO()
        {
            string computedValue = "";
            System.Xml.XmlElement newChild = util.getOtherDOMDocument().CreateElement("newChild");
            System.Xml.XmlNode oldChild = null;
            System.Xml.XmlElement testNode = null;
            string expectedValue = "System.ArgumentException";

            testResults results = new testResults("Core0102NO");
            try
            {
                results.description = "The \"replaceChild\" method of a node raises "+
                    "a System.ArgumentException Exception if the new "+
                    "child was created from a document different "+
                    "from the one that created this node.";
                //
                // Retrieve targeted data and define oldChild.
                //
                testNode = (System.Xml.XmlElement)util.nodeObject(util.SECOND,-1);
                oldChild = util.getSubNodes(testNode).Item(util.FIRST);
                //
                // Attempt to replace a child with a child from a different document
                // should raise an exception.
                //
                try 
                {
                    testNode.ReplaceChild(newChild,oldChild);//.node.
                }
                catch(System.Exception ex) 
                {
                    computedValue = ex.GetType().ToString(); 
                }
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }

            results.expected = expectedValue;
            results.actual = computedValue;

                util.resetData();
			Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0102NO -----------------------
        //
        //------------------------- test case core-0103NO --------------------------
        //
        // Testing feature - The "appendChild" method of a node raises a
        //                   System.ArgumentException Exception if the new child was
        //                   created from a different document than the one that
        //                   created this node.
        //
        // Testing Approach - Retrieve the second employee and attempt to append 
        //                    to it a node that was created from different
        //                    document.  An attempt to make such an insertion 
        //                    should raise the desired exception.
        //
        // Semantic Requirements: 71
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0103NO()
        {
            string computedValue = "";
            System.Xml.XmlElement newChild = util.getOtherDOMDocument().CreateElement("newChild");
            System.Xml.XmlNode testNode = null;
            string expectedValue = "System.ArgumentException";

            testResults results = new testResults("Core0103NO");
            try
            {
                results.description = "The \"appendChild\" method of a node raises a "+
                    "a System.ArgumentException Exception if the new "+
                    "child was created from a document different "+
                    "from the one that created this node.";
                //
                // Retrieve the targeted data.
                //
                testNode = util.nodeObject(util.SECOND,-1);
                //
                // Attempt to append a child from a different document should raise an
                // exception.
                //
                try 
                {
                    testNode.AppendChild(newChild);//.node.
                }
                catch(System.Exception ex) 
                {
                    computedValue = ex.GetType().ToString(); 
                }
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }
            results.expected = expectedValue;
            results.actual = computedValue;
                
			util.resetData();
            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0103NO -----------------------
    }
}