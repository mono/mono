//**************************************************************************
//
//
//                       National Institute Of Standards and Technology
//                                     DTS Version 1.0
//         
//                                   Attr Interface
//
// Written by: Carmelo Montanez
//
// Ported to System.Xml by: Mizrahi Rafael rafim@mainsoft.com
// Mainsoft Corporation (c) 2003-2004
//
//**************************************************************************
using System;
using System.Xml;
using nist_dom;
using NUnit.Framework;

namespace nist_dom.fundamental
{
    [TestFixture]
    public class AttrTest//, ITest
    {
        public static int i = 1;
/*
        public testResults[] RunTests()
        {
            testResults[] tests = new testResults[] {core0001A(), core0002A(), core0003A(),core0004A(),
                                                        core0005A(), core0006A(), core0007A(), core0008A(),
                                                        core0009A(), core0010A(), core0011A(), core0012A(),
                                                        core0013A(), core0014A()};
  
            return tests;
        }
*/
        //------------------------ test case core-0001A ------------------------
        //
        // Testing feature - The parentNode attribute for an Attr object should 
        //                   be null.
        //
        // Testing approach - Retrieve the attribute named "domestic" from the last 
        //                    child of of the first employee and examine its 
        //                    parentNode attribute.  This test uses the 
        //                    "GetNamedItem(name)" method from the NamedNodeMap 
        //                    interface.
        //
        // Semantic Requirements: 1
        //
        //----------------------------------------------------------------------------

        [Test]
        public void core0001A()
        {
            object computedValue = null;
            object expectedValue = null; 
            System.Xml.XmlNode testNode = null;
            System.Xml.XmlAttribute domesticAttr = null;

            testResults results = new testResults("Core0001A");

            try
            {
                results.description = "The ParentNode attribute should be null for" +
                    " an Attr object.";
                //
                //   Retrieve targeted data and examine parentNode attribute.
                //
                testNode = util.nodeObject(util.FIRST, util.SIXTH);
                domesticAttr = (System.Xml.XmlAttribute)testNode.Attributes.GetNamedItem("domestic");
                computedValue = domesticAttr.ParentNode;
                //
                //    Write out results 
                //
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }

            results.expected = (expectedValue == null).ToString();
            results.actual = (computedValue == null).ToString();
	    Assert.AreEqual (results.expected, results.actual);
            // return results;

        }

        //------------------------ End test case core-0001A --------------------------
        //
        //-------------------------  test case core-0002A ---------------------------- 
        //
        //         
        // Written By: Carmelo Montanez
        //
        // Testing feature - The previousSibling attribute for an Attr object 
        //                   should be null. 
        //
        // Testing approach - Retrieve the attribute named "domestic" from the last 
        //                    child of of the first employee and examine its 
        //                    previousSibling attribute.  This test uses the 
        //                    "GetNamedItem(name)" method from the NamedNodeMap 
        //                    interface.
        //
        // Semantic Requirements: 1
        //
        //----------------------------------------------------------------------------

        [Test]
        public void core0002A()
        {
            object computedValue = null;
            object expectedValue = null;
            System.Xml.XmlAttribute domesticAttr = null;
            System.Xml.XmlNode testNode = null;

            testResults results = new testResults("Core0002A");
            try
            {
                results.description =  "The previousSibling attribute should be " +
                    "null for an Attr object.";
                //
                // Retrieve the targeted data and examine its previousSibling attribute.
                //
                testNode = util.nodeObject(util.FIRST,util.SIXTH);
                domesticAttr = (System.Xml.XmlAttribute)testNode.Attributes.GetNamedItem("domestic");
                computedValue = domesticAttr.PreviousSibling;
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }
            //
            // Write out results 
            //
            results.expected = (expectedValue == null).ToString();
            results.actual = (computedValue == null).ToString();

	    Assert.AreEqual (results.expected, results.actual);
            // return results;
        }

        //------------------------ End test case core-0002A --------------------------
        //
        //-------------------------  test case core-0003A ----------------------------
        // Written By: Carmelo Montanez
        //
        // Testing feature - The nextSibling attribute for an Attr object should 
        //                   be null. 
        //
        // Testing approach - Retrieve the attribute named "domestic" from the 
        //                    last child of of the first employee and examine 
        //                    its nextSibling attribute.  This test uses the 
        //                    "GetNamedItem(name)" method from the NamedNodeMap 
        //                    interface.
        //                      
        // Semantic Requirements: 1
        //
        //----------------------------------------------------------------------------

        [Test]
        public void core0003A()
        {
            object computedValue = null;
            object expectedValue = null;
            System.Xml.XmlAttribute domesticAttr = null;
            System.Xml.XmlNode testNode = null;

            testResults results = new testResults("Core0003A");
            try
            {
                results.description =  "The nextSibling attribute should be null " +
                    "for an Attr object.";
                //
                // Retrieve the targeted data and examine its nextSibling attribute.
                //
                testNode = util.nodeObject(util.FIRST,util.SIXTH); 
                domesticAttr = (System.Xml.XmlAttribute)testNode.Attributes.GetNamedItem("domestic");
                computedValue = domesticAttr.NextSibling;
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }
            //
            // Write out results 
            //
            results.expected = (expectedValue == null).ToString();
            results.actual = (computedValue == null).ToString();

	    Assert.AreEqual (results.expected, results.actual);
            // return results;
        }

        //------------------------ End test case core-0003A --------------------------
        //
        //-------------------------  test case core-0004A ----------------------------
        //
        // Written By: Carmelo Montanez
        //
        // Testing feature - Attr objects may be associated with Element nodes 
        //                   contained within a DocumentFragment.
        //
        // Testing approach - Create a new DocumentFragment object and add a newly
        //                    created Element node to it (with one attribute).  Once
        //                    the element is added, its attribute should be available
        //                    as an attribute associated with an Element within a 
        //                    DocumentFragment.
        //
        // Semantic Requirements: 2
        //
        //----------------------------------------------------------------------------

        [Test]
        public void core0004A()
        {
            string computedValue = "";
            string expectedValue = "domestic";
            System.Xml.XmlAttribute domesticAttr = null;

            testResults results = new testResults("Core0004A");
            try
            {
                results.description = "Attr objects may be associated with Element " +
                    "nodes contained within a DocumentFragment.";

                System.Xml.XmlDocumentFragment docFragment = util.getDOMDocument().CreateDocumentFragment();
                System.Xml.XmlElement newElement = (System.Xml.XmlElement)util.createNode(util.ELEMENT_NODE,"element1");
                //
                // The new DocumentFragment is empty upon creation.  Set an attribute for 
                // a newly created element and add the element to the documentFragment.  
                //
                newElement.SetAttribute("domestic","Yes");
                docFragment.AppendChild(newElement);
                //
                // Access the attributes of the only child of the documentFragment
                //
                domesticAttr = (System.Xml.XmlAttribute)docFragment.FirstChild.Attributes.Item(0) ;
                computedValue = domesticAttr.Name;
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
            // return results;
        }

        //------------------------ End test case core-0004A --------------------------
        //
        //-------------------------- test case core-0005A ----------------------------
        //
        // Testing feature - If an Attr is explicitly assigned any value, then that 
        //                   value is the attribute's effective value. 
        //
        // Testing approach - Retrieve the attribute name "domestic" from  the last 
        //                    child of of the first employee element and examine its 
        //                    assigned value.  This test uses the
        //                    "GetNamedItem(name)" method from the NamedNodeMap
        //                    interface.
        //
        // Semantic Requirements: 3
        //
        //----------------------------------------------------------------------------

        [Test]
        public void core0005A()
        {
            string computedValue = "";
            string expectedValue = "Yes";
            System.Xml.XmlAttribute domesticAttr = null;
            System.Xml.XmlNode testNode = null;

            testResults results = new testResults("Core0005A");
            try
            {
                results.description = "If an attribute is explicitly assigned any value, " +
                    "then that value is the attribute's effective value."; 
                //
                //  Retrieve the targeted data and examine its assigned value.
                //
                testNode = util.nodeObject(util.FIRST,util.SIXTH);
                domesticAttr = (System.Xml.XmlAttribute)testNode.Attributes.GetNamedItem("domestic");
                computedValue = domesticAttr.Value;
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
            // return results;
        }

        //------------------------ End test case core-0005A --------------------------
        //
        //-------------------------- test case core-0006A ----------------------------
        //
        // Testing feature - If there is no explicit value assigned to an attribute 
        //                   and there is a declaration for this attribute and that 
        //                   declaration includes a default value, then that default 
        //                   value is the Attribute's default value. 
        //
        // Testing approach - Retrieve the attribute named "street" from the 
        //                    last child of of the first employee and examine its
        //                    value.  That value should be the value given during 
        //                    the declaration of the attribute in the DTD file.
        //                    This test uses the "GetNamedItem(name)" method from 
        //                    the NamedNodeMap interface.
        //
        // Semantic Requirements: 4 
        //
        //----------------------------------------------------------------------------

        [Test]
        [Ignore(".NET DOM implementation does not match W3C DOM specification.")]
        public void core0006A()
        {
            string computedValue = "";
            string expectedValue = "Yes";
            System.Xml.XmlAttribute streetAttr = null;
            System.Xml.XmlNode testNode = null;

            testResults results = new testResults("Core0006A");
            try
            {
                results.description = "If there is no explicit value assigned to an " +
                    "attribute and there is a declaration for this " +
                    "attribute and  that declaration includes a default " +
                    "value, then that default value is the Attribute's " +
                    "default value.";
                //
                // Retrieve the targeted data and examine its default value.
                //
                testNode = util.nodeObject(util.FIRST,util.SIXTH);
                streetAttr = (System.Xml.XmlAttribute)testNode.Attributes.GetNamedItem("street");
                computedValue = streetAttr.Value;
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
            // return results;
        }

        //------------------------ End test case core-0006A --------------------------
        //
        //--------------------------  test case core-0007A ---------------------------
        //
        // Testing feature - The "name" Attribute of an Attribute node. 
        //
        // Testing approach - Retrieve the attribute named "street" from the
        //                    last child of the second employee and examine its "name" 
        //                    attribute.  This test uses the "GetNamedItem(name)" 
        //                    method from the NamedNodeMap interface.
        //
        // Semantic Requirements: 5 
        //
        //----------------------------------------------------------------------------

        [Test]
        public void core0007A()
        {
            string computedValue = "";
            string expectedValue = "street";
            System.Xml.XmlAttribute streetAttr = null;
            System.Xml.XmlNode testNode = null;

            testResults results = new testResults("Core0007A");
            try
            {
                results.description = "The \"name\" attribute of an Attr object contains " +
                    "the name of that attribute.";
                //
                // Retrieve the targeted data and capture its assigned name.
                //
                testNode = util.nodeObject(util.SECOND,util.SIXTH);
                streetAttr = (System.Xml.XmlAttribute)testNode.Attributes.GetNamedItem("street");
                computedValue = streetAttr.Name;
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
            // return results;
        }

        //------------------------ End test case core-0007A --------------------------
        //
        //--------------------------  test case core-0008A ---------------------------
        //
        // Testing feature - The "specified" attribute of an Attr node should be set 
        //                   to true if the attribute was explicitly given a value. 
        //
        // Testing approach - Retrieve the attribute named "doestic" from the last
        //                    child of the first employee and examine its "specified"
        //                    attribute.  It should be set to true.  This test
        //                    uses the "GetNamedItem(name)" method from the 
        //                    NamedNodeMap interface.
        //
        // Semantic Requirements: 6 
        //
        //----------------------------------------------------------------------------

        [Test]
        public void core0008A()
        {
            string computedValue = "";//0;
            string expectedValue = "True";
            System.Xml.XmlNode testNode = null;
            System.Xml.XmlAttribute domesticAttr = null;

            testResults results = new testResults("Core0008A");
            try
            {
                results.description = "The \"specified\" attribute for an Attr object " +
                    "should be set to true if the attribute was " + 
                    "explictly given a value.";
                //
                // Retrieve the targeted data and capture its "specified" value.
                //
                testNode = util.nodeObject(util.FIRST,util.SIXTH);
                domesticAttr = (System.Xml.XmlAttribute)testNode.Attributes.GetNamedItem("domestic");;
                computedValue = domesticAttr.Specified.ToString();
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
            // return results;
        }

        //------------------------ End test case core-0008A --------------------------
        //
        //--------------------------  test case core-0009A ---------------------------
        //
        // Testing feature - The "specified" attribute for an Attr node should be
        //                   set to false if the attribute was not explicitly given
        //                   a value.
        //
        // Testing approach - Retrieve the attribute named "street"(not explicity
        //                    given a value) from the last child of the first employee  
        //                    and examine its "specified" attribute.  It should be 
        //                    set to false.  This test uses the
        //                    "GetNamedItem(name)" method from the NamedNodeMap
        //                    interface.
        //
        // Semantic Requirements: 6 
        //
        //----------------------------------------------------------------------------

        [Test]
        [Ignore(".NET DOM implementation does not match W3C DOM specification.")]
        public void core0009A()
        {
            string computedValue = "";//0;
            string expectedValue = "False";
            System.Xml.XmlAttribute streetAttr = null;
            System.Xml.XmlNode testNode = null;

            testResults results = new testResults("Core0009A");
            try
            {
                results.description = "The \"specified\" attribute for an Attr node " +
                    "should be set to false if the attribute was " +
                    "not explictly given a value.";
                //
                // Retrieve the targeted data and capture its "specified" attribute.
                //
                testNode = util.nodeObject(util.FIRST,util.SIXTH);
                streetAttr = (System.Xml.XmlAttribute)testNode.Attributes.GetNamedItem("street");
                computedValue = streetAttr.Specified.ToString();
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
            // return results;
        }

        //------------------------ End test case core-0009A --------------------------
        //
        //--------------------------  test case core-0010A ---------------------------
        //
        // Testing feature - The "specified" attribute for an Attr node should be
        //                   automatically flipped to true if value of the attribute 
        //                   is changed (even its ends up having a default DTD value)
        //
        // Testing approach - Retrieve the attribute named "street" from the last
        //                    child of the third employee and change its value to "Yes"
        //                    (which is its default DTD value).  This should cause the
        //                    "specified" attribute to be flipped to true.  This test
        //                    makes use of the "setAttribute(name,value )" method from
        //                    the Element interface and the "GetNamedItem(name)" 
        //                    method from the NamedNodeMap interface.
        //
        // Semantic Requirements: 7 
        //
        //----------------------------------------------------------------------------

        [Test]
        public void core0010A()
        {
            string computedValue = "";//"";
            string expectedValue = "True";
            System.Xml.XmlAttribute streetAttr = null;
            System.Xml.XmlElement testNode = null;

            testResults results = new testResults("Core0010A");
            try
            {
                results.description = "The \"specified\" attribute for an Attr node " +
                    "should be flipped to true if the attribute value " + 
                    "is changed (even it changed to its default value).";
                //
                // Retrieve the targeted data and set a new attribute for it, then 
                // capture its "specified" attribute.
                //
                testNode = (System.Xml.XmlElement)util.nodeObject(util.THIRD,util.FIFTH);
                testNode.SetAttribute("street","Yes");//testNode.node.setAttribute("street","Yes");
                streetAttr = (System.Xml.XmlAttribute)testNode.Attributes.GetNamedItem("street");
                computedValue = streetAttr.Specified.ToString();
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
            // return results;
        }

        //------------------------ End test case core-0010A --------------------------
        //
        //--------------------------  test case core-0011A ---------------------------
        //
        // Testing feature - To respecify the attribute to its default value from the
        //                   DTD, the attribute must be deleted.  The implementation
        //                   will then make a new attribute available with the
        //                   "specified" attribute set to false.

        // Testing approach - Retrieve the attribute named "street" from the last
        //                    child of the third employee and delete it.  The 
        //                    implementation should then create a new attribute with 
        //                    its default value and "specified" set to false.  This 
        //                    test uses the "removeAttribute(name)" from the Element 
        //                    interface and the "GetNamedItem(name)" method from the 
        //                    NamedNodeMap interface.
        //
        // Semantic Requirements: 8 
        //
        //----------------------------------------------------------------------------

        [Test]
        [Ignore(".NET DOM implementation does not match W3C DOM specification.")]
        public void core0011A()
        {
            string computedValue = "";//"";
            string expectedValue = "False";
            System.Xml.XmlAttribute streetAttr = null;
            System.Xml.XmlElement testNode = null;

            testResults results = new testResults("Core0011A");
            try
            {
                results.description = "Re-setting an attribute to its default value " +
                    "requires that the attribute be deleted.  The " +
                    "implementation should create a new attribute " +
                    "with its \"specified\" attribute set to false.";
                //
                // Retrieve the targeted data, remove the "street" attribute and capture 
                // its specified attribute.
                //
                testNode = (System.Xml.XmlElement)util.nodeObject(util.THIRD,util.SIXTH);
                testNode.RemoveAttribute("street");//testNode.node.removeAttribute("street");
                streetAttr = (System.Xml.XmlAttribute)testNode.Attributes.GetNamedItem("street");
                computedValue = streetAttr.Specified.ToString();
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
            // return results;
        }

        //------------------------ End test case core-0011A --------------------------
        //
        //--------------------------  test case core-0012A ---------------------------
        //
        // Testing feature - Upon retrieval, the "value" of an attribute is returned 
        //                   as a string with characters and general entity references 
        //                   replaced with their values.

        // Testing approach - Retrieve the attribute named "street" from the last 
        //                    child of the fourth employee and examine its value 
        //                    attribute.  This value should be "Yes" after the
        //                    EntityReference is replaced with its value.   This 
        //                    test uses the "GetNamedItem(name)" method from 
        //                    the NamedNodeMap interface.
        //
        // Semantic Requirements: 9 
        //
        //----------------------------------------------------------------------------

        [Test]
        public void core0012A()
        {
            string computedValue = "";
            string expectedValue = "Yes";
            System.Xml.XmlAttribute streetAttr = null;
            System.Xml.XmlNode testNode = null;

            testResults results = new testResults("Core0012A");
            try
            {
                results.description = "Upon retrieval the \"value\" attribute of an Attr" +
                    "object is returned as a string with any Entity " +
                    "References replaced with their values.";
                //
                // Retrieve the targeted data.
                //
                testNode = util.nodeObject(util.FOURTH,util.SIXTH);
                streetAttr = (System.Xml.XmlAttribute)testNode.Attributes.GetNamedItem("street");
                computedValue = streetAttr.Value;
            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }
            //
            //    Write out results
            //
            results.expected = expectedValue;
            results.actual = computedValue;

	    Assert.AreEqual (results.expected, results.actual);
            // return results;
        }

        //------------------------ End test case core-0012A --------------------------
        //
        //--------------------------  test case core-0013A ---------------------------
        //
        // Testing feature - On setting, the "value" attribute of an Attr node 
        //                   creates a Text node with the unparsed content of 
        //                   the string.

        // Testing approach - Retrieve the attribute named "street" from the last 
        //                    child of the fourth employee and assign the "Y%ent1;" 
        //                    string to its value attribute.  This value is not yet
        //                    parsed and therefore should still be the same upon 
        //                    retrieval.  This test uses the "GetNamedItem(name)" 
        //                    method from the NamedNodeMap interface.
        //
        // Semantic Requirements: 10
        //
        //----------------------------------------------------------------------------

        [Test]
        public void core0013A()
        {
            string computedValue = "";
            string expectedValue = "Y%ent1;";
            System.Xml.XmlAttribute streetAttr = null;
            System.Xml.XmlNode testNode = null;

            testResults results = new testResults("Core0013A");
            try
            {
                results.description = "On setting, the \"value\" attribute of an Attr " +
                    "object creates a Text node with the unparsed " +
                    "content of the string.";
                //
                // Retrieve the targeted data, assign a value to it and capture its
                // "value" attribute.
                //
                testNode = util.nodeObject(util.FOURTH,util.SIXTH);
                streetAttr = (System.Xml.XmlAttribute)testNode.Attributes.GetNamedItem("street");
                streetAttr.Value = "Y%ent1;"; 
                computedValue = streetAttr.Value;
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
            // return results;
        }

        //------------------------ End test case core-0013A --------------------------
        //
        //--------------------------  test case core-0014A ---------------------------
        //
        // Testing feature - Setting the "value" attribute raises a
        //                   NO_MODIFICATION_ALLOWED_ERR Exception if the 
        //                   node is readonly.
        //
        // Testing approach - Retrieve the first attribute of the Entity node named
        //                    "ent4" and attempt to change its value attribute.
        //                    Since the descendants of Entity nodes are readonly, the
        //                    desired exception should be raised.
        //
        // Semantic Requirements: 11
        //
        //----------------------------------------------------------------------------

        [Test]
	[Ignore(".NET DOM implementation does not match W3C DOM specification.")] // MS DOM is buggy
        public void core0014A()
        {
            string computedValue = "";
            System.Xml.XmlNode testNode = null;
            System.Xml.XmlAttribute readOnlyAttribute = null;
            string expectedValue = "System.ArgumentException";//util.NO_MODIFICATION_ALLOWED_ERR;

            testResults results = new testResults("Core0014A");
            try
            {
                results.description =  "Setting the \"value\" attribute raises a " +
                    "NO_MODIFICATION_ALLOWED_ERR Exception if " +
                    "the node is readonly.";

                //
                // Retrieve the targeted data.
                //
                testNode = util.getEntity("ent4");
                readOnlyAttribute = (System.Xml.XmlAttribute)testNode.FirstChild.Attributes.Item(0);
                //
                // attempt to set a value on a readonly node should raise an exception.
                //
                try 
                {
                    readOnlyAttribute.Value = "ABCD";
                }
                catch (ArgumentException ex)
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
            // return results;
        }
        //------------------------ End test case core-0014A --------------------------
    }

}
