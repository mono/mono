//**************************************************************************
//
//
//                       National Institute Of Standards and Technology
//                                     DTS Version 1.0
//         
//                                      Text Interface
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
    public class TextTest
    {
        public static int i = 2;
/*
        public testResults[] RunTests()
        {
            testResults[] tests = new testResults[] {core0001T(), core0002T(), core0003T(),core0004T(),
                                                        core0005T(), core0006T(), core0007T(), core0008T(),
                                                        core0009T()};
  
            return tests;
        }
*/
        //------------------------ test case core-0001T ------------------------
        //
        // Testing feature -  If there is no markup inside an Element or Attr node
        //                    content, then the text is contained in a single object 
        //                    implementing the Text interface that is the only child
        //                    of the element.
        //
        // Testing approach - Retrieve the textual data from the second child of the
        //                    third employee.  That Text node contains a block of 
        //                    multiple text lines without markup, so they should be 
        //                    treated as a single Text node.  The "nodeValue" attribute 
        //                    should contain the combination of the two lines.
        //                    
        // Semantic Requirements: 1
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0001T()
        {
            string computedValue = "";
            string expectedValue = "Roger\n Jones";
            System.Xml.XmlNode testNode = null;
            System.Xml.XmlCharacterData testNodeData = null;

            testResults results = new testResults("Core0001T");
            try
            {
                results.description = "If there is no markup language in a block of text, " +
                    "then the content of the text is contained into " +
                    "an object implementing the Text interface that is " +
                    "the only child of the element."; 
                //
                // Retrieve the second child of the second employee and access its
                // textual data. 
                //
                testNode = util.nodeObject(util.THIRD,util.SECOND); 
                testNode.Normalize();
                testNodeData = (System.Xml.XmlCharacterData)testNode.FirstChild;
                computedValue = testNodeData.Value;
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }

            //
            // Write out results 
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0001T --------------------------
        //
        //-------------------------- test case core-0002T ----------------------------
        //
        // Testing feature -  If there is markup inside the Text element content,
        //                    then the text is parsed into a list of elements and text
        //                    that forms the list of children of the element.
        //
        // Testing approach - Retrieve the textual data from the last child of the
        //                    third employee.  That node is composed of two
        //                    EntityReferences nodes and two Text nodes.  After the
        //                    content of the node is parsed, the "address" Element
        //                    should contain four children with each one of the
        //                    EntityReferences containing one child in turn. 
        //
        // Semantic Requirements: 2 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0002T()
        {
            string computedValue = "";
            string expectedValue = "1900 Dallas Road Dallas, Texas\n 98554";
            System.Xml.XmlNode testNode = null;
            System.Xml.XmlNode textBlock1 = null;
            System.Xml.XmlNode textBlock2 = null;
            System.Xml.XmlNode textBlock3 = null;
            System.Xml.XmlNode textBlock4 = null; 

            testResults results = new testResults("Core0002T");
            try
            {
                results.description = "If there is markup language in the content of the " +
                    "element then the content is parsed into a " +
                    "list of elements and Text that are the children of " +
                    "the element";
                //
                // This last child of the second employee should now have four children, 
                // two Text nodes and two EntityReference nodes.  Retrieve each one of them 
                // and in the case of EntityReferences retrieve their respective children. 
                //
                testNode = util.nodeObject(util.SECOND,util.SIXTH);
                textBlock1 = testNode.ChildNodes.Item(util.FIRST).FirstChild;
                textBlock2 = testNode.ChildNodes.Item(util.SECOND);
                textBlock3 = testNode.ChildNodes.Item(util.THIRD).FirstChild;
                textBlock4 = testNode.ChildNodes.Item(util.FOURTH);

                computedValue += textBlock1.Value;
                computedValue += textBlock2.Value;
                computedValue += textBlock3.Value;
                computedValue += textBlock4.Value;
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }

            //
            // Write out results
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0002 --------------------------
        //
        //-------------------------- test case core-0003T ---------------------------
        //
        // Testing feature -  The "splitText(offset)" method breaks the Text node
        //                    into two Text nodes at the specified offset keeping  
        //                    each node as siblings in the tree.
        //
        // Testing approach - Retrieve the textual data from the second child of the 
        //                    third employee and invoke its "splitText(offset)" method.
        //                    The method splits the Text node into two new sibling
        //                    Text Nodes keeping both of them in the tree.  This test
        //                    checks the "nextSibling" attribute of the original node
        //                    to ensure that the two nodes are indeed siblings. 
        //
        // Semantic Requirements: 3 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0003T()
        {
            string computedValue = "";
            string expectedValue = "Jones";
            System.Xml.XmlText oldTextNode = null;
            System.Xml.XmlNode testNode = null;

            testResults results = new testResults("Core0003T");
            try
            {
                results.description = "The \"splitText(offset)\" method breaks the Text node " +
                    "into two Text nodes at the specified offset, keeping each " +
                    "node in the tree as siblings.";
                //
                // Retrieve the targeted data.
                //
                testNode = util.nodeObject(util.THIRD,util.SECOND);
                oldTextNode = (System.Xml.XmlText)testNode.FirstChild;
                //
                // Split the two lines of text into two different Text nodes. 
                //
                oldTextNode.SplitText(util.EIGHT);
                computedValue = oldTextNode.NextSibling.Value;
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

            util.resetData();

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0003T --------------------------
        //
        //-------------------------- test case core-0004T ---------------------------
        //
        // Testing feature -  After The "splitText(offset)" method breaks the Text node
        //                    into two Text nodes, the original node contains all the
        //                    content up to the offset point. 
        //
        // Testing approach - Retrieve the textual data from the second child
        //                    of the third employee and invoke the "splitText(offset)"
        //                    method.  The original Text node should contain all the
        //                    content up to the offset point.  The "nodeValue" 
        //                    attribute is invoke to check that indeed the original 
        //                    node now contains the first five characters
        //
        // Semantic Requirements: 4 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0004T()
        {
            string computedValue = "";
            string expectedValue = "Roger";
            System.Xml.XmlText oldTextNode = null;
            System.Xml.XmlNode testNode = null;

            testResults results = new testResults("Core0004T");
            try
            {
                results.description = "After the \"splitText(offset)\" method is invoked, the " +
                    "original Text node contains all of the content up to the " +
                    "offset point.";
                //
                // Retrieve targeted data.
                //
                testNode = util.nodeObject(util.THIRD,util.SECOND);
                oldTextNode = (System.Xml.XmlText)testNode.FirstChild;
                //
                // Split the two lines of text into two different Text nodes.
                //
                oldTextNode.SplitText(util.SIXTH);
                computedValue = oldTextNode.Value;
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

            util.resetData();

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0004T --------------------------
        //
        //-------------------------- test case core-0005T ---------------------------
        //
        // Testing feature -  After The "splitText(offset)" method breaks the Text node
        //                    into two Text nodes, the new Text node contains all the
        //                    content at and after the offset point.
        //
        // Testing approach - Retrieve the textual data from the second child of the
        //                    third employee and invoke the "splitText(offset)" method.
        //                    The new Text node should contain all the content at 
        //                    and after the offset point.  The "nodeValue" attribute 
        //                    is invoked to check that indeed the new node now
        //                    contains the first characters at and after position
        //                    seven (starting from 0).
        //
        // Semantic Requirements: 5 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0005T()
        {
            string computedValue = "";
            string expectedValue = " Jones";
            System.Xml.XmlText oldTextNode = null;
            System.Xml.XmlText newTextNode = null;
            System.Xml.XmlNode testNode = null;
  
            testResults results = new testResults("Core0005T");
            try
            {
                results.description = "After the \"splitText(offset)\" method is invoked, the " +
                    "new Text node contains all of the content from the offset " +
                    "point to the end of the text.";
                //
                // Retrieve the targeted data.
                //
                testNode = util.nodeObject(util.THIRD,util.SECOND);
                oldTextNode = (System.Xml.XmlText)testNode.FirstChild;
                //
                // Split the two lines of text into two different Text nodes.
                //
                newTextNode = oldTextNode.SplitText(util.SEVENTH);
                computedValue = newTextNode.Value;
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }

            //
            // Write out results
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            util.resetData(); 

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0005T --------------------------
        //
        //-------------------------- test case core-0006T ---------------------------
        //
        // Testing feature -  The "splitText(offset)" method returns the new Text 
        //                    node.
        //
        // Testing approach - Retrieve the textual data from the last child of the
        //                    first employee and invoke its "splitText(offset)" method.
        //                    The method should return the new Text node.  The offset
        //                    value used for this test is 30.  The "nodeValue" 
        //                    attribute is invoked to check that indeed the new node 
        //                    now contains the characters at and after postion 30 
        //                    (counting from 0).
        //
        // Semantic Requirements: 6 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0006T()
        {
            string computedValue = "";
            string expectedValue = "98551";
            System.Xml.XmlText oldTextNode = null;
            System.Xml.XmlText newTextNode = null;
            System.Xml.XmlNode testNode = null;

            testResults results = new testResults("Core0006T");
            try
            {
                results.description = "The \"splitText(offset)\" method returns the " +
                    "new Text node.";
                //
                // Retrieve the targeted data.
                //
                testNode = util.nodeObject(util.FIRST,util.SIXTH);
                oldTextNode = (System.Xml.XmlText)testNode.FirstChild;
                //
                // Split the two lines of text into two different Text nodes. 
                //
                newTextNode = oldTextNode.SplitText(30);
                computedValue = newTextNode.Value;
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }
            //
            // Write out results
            //
            results.expected = expectedValue;
            results.actual = computedValue;
    
            util.resetData();

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0006T --------------------------
        //
        //-------------------------- test case core-0007T ---------------------------
        //
        // Testing feature -  The "splitText(offset)" method raises an INDEX_SIZE_ERR
        //                    Exception if the specified offset is negative. 
        //
        // Testing approach - Retrieve the textual data from the second child of 
        //                    the third employee and invoke its "splitText(offset)" 
        //                    method with "offset" equals to a negative number.  It 
        //                    should raise the desired exception.
        //
        // Semantic Requirements: 7 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0007T()
        {
            string computedValue = "";
            System.Xml.XmlText oldTextNode = null;
            System.Xml.XmlText newTextNode = null;
            System.Xml.XmlNode testNode = null;
            string expectedValue = "System.ArgumentOutOfRangeException";//util.INDEX_SIZE_ERR;

            testResults results = new testResults("Core0007T");
            try
            {
                results.description = "The \"splitText(offset)\" method raises an " +
                    "INDEX_SIZE_ERR Exception if the specified " +
                    "offset is negative.";

                //
                // Retrieve the targeted data
                //
                testNode = util.nodeObject(util.THIRD,util.SECOND);
                oldTextNode = (System.Xml.XmlText)testNode.FirstChild;
                //
                // Call the "spitText(offset)" method with "offset" equal to a negative 
                // number should raise an exception.
                //
                try 
                {
                    oldTextNode.SplitText(-69);
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
        //------------------------ End test case core-0007T --------------------------
        //
        //-------------------------- test case core-0008T ----------------------------
        //
        // Testing feature -  The "splitText(offset)" method raises an 
		//                    ArgumentOutOfRangeException if the specified offset is greater than the 
        //                    number of 16-bit units in the Text node.
        //
        // Testing approach - Retrieve the textual data from the second child of
        //                    third employee and invoke its "splitText(offset)" 
        //                    method with "offset" greater than the number of
        //                    characters in the Text node.  It should raise the 
        //                    desired exception.
        //
        // Semantic Requirements: 7
        //
        //----------------------------------------------------------------------------

            [Test]
	public void core0008T()
            {
                string computedValue = "";
                System.Xml.XmlText oldTextNode = null;
                System.Xml.XmlNode testNode = null;
                string expectedValue = "System.ArgumentOutOfRangeException";

                testResults results = new testResults("Core0008T");
                try
                {
                    results.description = "The \"splitText(offset)\" method raises an " +
                        "ArgumentOutOfRangeException if the specified " +
                        "offset is greater than the number of 16-bit units " +
                        "in the Text node.";
                    //
                    // Retrieve the targeted data.
                    //
                    testNode = util.nodeObject(util.THIRD,util.SECOND);
                    oldTextNode = (System.Xml.XmlText)testNode.FirstChild;
                    //
                    // Call the "spitText(offset)" method with "offset" greater than the numbers 
                    // of characters in the Text node, it should raise an exception.

                    try 
                    {
                        oldTextNode.SplitText(300);
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
        //------------------------ End test case core-0008T --------------------------
        //
        //-------------------------- test case core-0009T ----------------------------
        //
        // Testing feature -  The "splitText(offset)" method raises a
        //                    NO_MODIFICATION_ALLOWED_ERR Exception if
        //                    the node is readonly. 
        //
        // Testing approach - Retrieve the textual data from the first EntityReference 
        //                    inside the last child of the second employee and invoke 
        //                    its splitText(offset) method.  Descendants of 
        //                    EntityReference nodes are readonly and therefore the 
        //                    desired exception should be raised. 
        //
        // Semantic Requirements: 8 
        //
        //----------------------------------------------------------------------------

        [Test]
	[Category ("NotDotNet")] // MS DOM is buggy
	public void core0009T()
        {
            string computedValue = "";
            System.Xml.XmlNode testNode = null;
            System.Xml.XmlText readOnlyText = null;
            string expectedValue = "System.ArgumentException";//util.NO_MODIFICATION_ALLOWED_ERR;

            testResults results = new testResults("Core0009T");
            try
            {
                results.description = "The \"splitText(offset)\" method raises a " +
                    "NO_MODIFICATION_ALLOWED_ERR Exception if the " +
                    "node is readonly.";
                //
                // Attempt to modify descendants of an EntityReference node should raise 
                // an exception. 
                //
                testNode = util.nodeObject(util.SECOND,util.SIXTH);
                readOnlyText = (System.Xml.XmlText)testNode.ChildNodes.Item(util.FIRST).FirstChild;

                try 
                {
                    readOnlyText.SplitText(5);
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

        //------------------------ End test case core-0009T --------------------------
    }
}