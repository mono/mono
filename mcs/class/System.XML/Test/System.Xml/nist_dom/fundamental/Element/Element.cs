//**************************************************************************
//
//
//                       National Institute Of Standards and Technology
//                                     DTS Version 1.0
//         
//                                   Element Interface
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
    public class ElementTest
    {
        public static int i = 2;
/*
        public testResults[] RunTests()
        {
            testResults[] tests = new testResults[] {core0001E(), core0002E(), core0003E(),core0004E(),
                                                        core0005E(), core0006E(), core0007E(), core0008E(),
                                                        core0009E(), core0010E(), core0011E(), core0012E(),
                                                        core0013E(), core0014E(), core0015E(), core0016E(),
                                                        core0017E(), core0018E(), core0019E(), core0020E(),
                                                        core0021E(), core0022E(), core0023E(), core0024E(),
                                                        core0025E(), core0026E(), core0027E(), core0028E(),
                                                        core0029E(), core0030E()};
  
            return tests;
        }
*/
        //------------------------ test case core-0001E ------------------------
        //
        // Testing feature - Elements may have attributes associated with them. 
        //
        // Testing approach - Retrieve the first attribute from the last child of
        //                    the first employee and examine its "specified"
        //                    attribute.  This test is only intended to show
        //                    that Elements can actually have attributes.
        //                    This test uses the "getNamedItem(name)" method from 
        //                    the NamedNodeMap interface.
        //
        // Semantic Requirements: 1
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0001E()
        {
            string computedValue = "0";//0
            string expectedValue = "True";//true
            System.Xml.XmlNode addressElement = null;
            System.Xml.XmlAttributeCollection attrList = null;
            System.Xml.XmlAttribute domesticAttr = null;

            testResults results = new testResults("Core0001E");
            try
            {
                results.description = "Element nodes may have associated attributes.";
                //
                // Retrieve the "address" element from the first employee.
                //
                addressElement = util.nodeObject(util.FIRST,util.SIXTH);
                //
                // Access its "domestic" attribute by creating a list of all attributes
                // and then retrieving the desired attribute from the list by name. 
                //
                attrList = addressElement.Attributes;//.node.
                domesticAttr = (System.Xml.XmlAttribute)attrList.GetNamedItem("domestic");
                //
                // Access its "specified" attribute.
                //
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
        }
        //------------------------ End test case core-0001E --------------------------
        //
        //------------------------ test case core-0002E ------------------------
        //
        // Testing feature - The generic Attribute "attributes" (Node interface) may 
        //                   be used to retrieve the set of all attributes of an
        //                   element.
        //
        // Testing approach - Create a list of all the attributes of the last child of
        //                    of the first employee by using the generic "attributes"
        //                    attribute from the Node interface.  Further the length
        //                    of the attribute list is examined.  This test makes
        //                    use of the "Count" attribute from the NameNodeMap 
        //                    interface.
        //
        // Semantic Requirements: 1, 2 
        //
        //----------------------------------------------------------------------------

        [Test]
	[Ignore(".NET DOM implementation does not match W3C DOM specification.")] // MS DOM is buggy
	public void core0002E()
        {
            string computedValue = "";
            string expectedValue = "2";
            System.Xml.XmlNode addressElement = null;
            System.Xml.XmlAttributeCollection attrList = null;

            testResults results = new testResults("Core0002E");
            try
            {
                results.description = "The generic \"attributes\" (from the Node interface) may " +
                    "be used to retrieve the set of all attributes of an element.";
                //
                // Retrieve the "address" element from the first employee.
                //
                addressElement = util.nodeObject(util.FIRST,util.SIXTH);
                //
                // Access its attributes list.
                //
                attrList = addressElement.Attributes;
                //
                // Access its "length" attribute.
                //
                computedValue = attrList.Count.ToString();
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

        //------------------------ End test case core-0002E --------------------------
        //
        //-------------------------- test case core-0003E ----------------------------
        //
        // Testing feature - The "tagName" attribute contains the name of the
        //                   element. 
        //
        // Testing approach - Retrieve the third child of the second employee and
        //                    examine its "tagName" attribute.  It should return a 
        //                    string containing the name of the element ("position",
        //                    in this case). 
        //
        // Semantic Requirements: 3 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0003E()
        {
            string computedValue = "";
            string expectedValue = "position";
            System.Xml.XmlNode positionElement = null;

            testResults results = new testResults("Core0003E");
            try
            {
                results.description = "The \"tagName\" of an Element contains the " +
                    "element's name.";
                //
                // Access its third child of the second employee.
                //
                positionElement = util.nodeObject(util.SECOND,util.THIRD);
                //
                // Access its "tagName" attribute.
                //
                computedValue = positionElement.Name;//tagName;//.node.
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

        //------------------------ End test case core-0003E --------------------------
        //
        //-------------------------- test case core-0004E ----------------------------
        //
        // Testing feature - The "getAttribute(name)" method returns an attribute value
        //                   by name. 
        //
        // Testing approach - Retrieve the the last child of the third employee, then  
        //                    invoke its "getAttribute(name)" method.  It should
        //                    return the value of the attribute("No", in this case).
        //
        // Semantic Requirements: 1, 4
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0004E()
        {
            string computedValue = "";
            string expectedValue = "No";
            System.Xml.XmlElement addressElement = null;

            testResults results = new testResults("Core0004E");
            try
            {
                results.description = "The \"getAttribute(name)\" method of an Element returns " +
                    "the value of an attribute by name.";
                //
                // Retrieve the targeted data. 
                //
                addressElement = (System.Xml.XmlElement)util.nodeObject(util.THIRD,util.SIXTH);
                computedValue = addressElement.GetAttribute("street");//addressElement.node.GetAttribute("street");
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

        //------------------------ End test case core-0004E --------------------------
        //
        //-------------------------- test case core-0005E ----------------------------
        //
        // Testing feature - The "getAttribute(name)" method returns an empty
        //                   string if no value was assigned to an attribute and
        //                   no default value was given in the DTD file.
        //
        // Testing approach - Retrieve the the last child of the last employee, then
        //                    invoke its "getAttribute(name)" method, where "name" is an
        //                    attribute with no specified or DTD default value.  The
        //                    "getAttribute(name)" method should return the empty
        //                    string.  This method makes use of the 
        //                    "createAttribute(newAttr)" method from the Document
        //                    interface.
        //
        // Semantic Requirements: 1, 4, 5 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0005E()
        {
            string computedValue = "";
            string expectedValue = "";
            System.Xml.XmlElement addressElement = null;
            System.Xml.XmlAttribute newAttribute = null;


            testResults results = new testResults("Core0005E");
            try
            {
                results.description = "The \"getAttribute(name)\" method of an Element returns " +
                    "the empty string if the attribue does not have a default " +
                    "or specified value.";
                //
                // Access the sixth child of the last employee.
                //
                newAttribute = (System.Xml.XmlAttribute)util.createNode(util.ATTRIBUTE_NODE,"district");
                addressElement = (System.Xml.XmlElement)util.nodeObject(util.FOURTH,util.SIXTH);
                //
                // Invoke its "setAttributeNode(newAttr)" method where
                // newAttr = "newAttribute".  Since no value was specified or given
                // by default, the value returned by the "getAttribute(name)" method 
                // should be the empty string.
                //
                addressElement.SetAttributeNode(newAttribute);//.node.
                computedValue = addressElement.GetAttribute("district");//.node.
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

        //------------------------ End test case core-0005E --------------------------
        //
        //-------------------------- test case core-0006E ----------------------------
        //
        // Testing feature - The "setAttribute(name,value)" method adds a new attribute
        //                   to the Element.
        //
        // Testing approach - Retrieve the last child of the last employee, then
        //                    add an attribute to it by invoking its 
        //                    "setAttribute(name,value)" method.  It should create 
        //                    a "name" attribute with an assigned value equal to 
        //                    "value".  
        //
        // Semantic Requirements: 1, 4, 6 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0006E()
        {
            string computedValue = "";
            System.Xml.XmlElement addressElement = null;
            string name = "district";
            string expectedValue = "dallas"; 


            testResults results = new testResults("Core0006E");
            try
            {
                results.description = "The \"setAttribute(name,value)\" method of an Element " +
                    "creates an new \"name\" attribute whose value is equal to \"value\".";
                //
                // Access the last child of the last employee.
                //
                addressElement = (System.Xml.XmlElement)util.nodeObject(util.FIFTH,util.SIXTH);
                //
                // Invoke its "setAttribute(name,value)" method and create a new attribute
                //
                addressElement.SetAttribute(name,expectedValue);//.node.
                //
                // This Element should now have a new attribute that we can be retrieved
                // by name. 
                //
                computedValue = addressElement.GetAttribute(name);//.node.
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

        //------------------------ End test case core-0006E --------------------------
        //
        //-------------------------- test case core-0007E ----------------------------
        //
        // Testing feature - The "setAttribute(name,value)" method adds a new attribute
        //                   to the Element.  If the "name" is already present, then
        //                   its value should be changed to the new one of the
        //                   "value" parameter.
        //
        // Testing approach - Retrieve the last child of the fourth employee,
        //                    then add an attribute to it by invoking its
        //                    "setAttribute(name,value)" method.  Since the name 
        //                    of the used attribute ("street") is already present
        //                    in this element, then its value should be
        //                    changed to the new one of the "value" parameter.
        //
        // Semantic Requirements: 1, 4, 7 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0007E()
        {
            string computedValue = "";
            string expectedValue = "Neither";
            System.Xml.XmlElement addressElement = null;

            testResults results = new testResults("Core0007E");
            try
            {
                results.description = "The \"setAttribute(name,value)\" method of an Element " +
                    "where the \"name\" attribute is already present in this Element.";
                //
                // Access the sixth child of the fourth employee.
                //
                addressElement = (System.Xml.XmlElement)util.nodeObject(util.FOURTH,util.SIXTH);
                //
                // Invoke its "setAttribute(name,value)" method where name = "street"
                // and value = "Neither".
                //
                addressElement.SetAttribute("street","Neither");//.node.
                //
                // The "street" attribute should now have a value of "Neither" 
                //
                computedValue = addressElement.GetAttribute("street");//.node.
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

        //------------------------ End test case core-0007E --------------------------
        //
        //-------------------------- test case core-0008E ----------------------------
        //
        // Testing feature - The "removeAttribute(name)" removes an attribute
        //                   by name.  If the removed attribute is known to have a
        //                   default value, an attribute immediately appears 
        //                   containing the default value.
        //
        // Testing approach - Retrieve the attribute named "street" from the last
        //                    child of the fourth employee, then remove the "street"
        //                    attribute by invoking its "removeAttribute(name) method.
        //                    The "street" attribute has a default value defined in the 
        //                    DTD file, that value should immediately replace the 
        //                    old value.   
        //
        // Semantic Requirements: 1, 8 
        //
        //----------------------------------------------------------------------------

        [Test]
        [Ignore(".NET DOM implementation does not match W3C DOM specification.")]
	public void core0008E()
        {
            string computedValue = "";
            string expectedValue = "Yes";
            System.Xml.XmlElement addressElement = null;
            string streetAttr = "";

            testResults results = new testResults("Core0008E");
            try
            {
                results.description = "The \"removeAttribute(name)\" method of an Element " +
                    "removes the \"name\" attribute and restores any " +
                    "known default values.";
                //
                // Access the last child of the fourth employee.
                //
                addressElement = (System.Xml.XmlElement)util.nodeObject(util.FOURTH,util.SIXTH);
                //
                // Invoke its "removeAttribute(name)" method where name = "street"
                //
                addressElement.RemoveAttribute("street");//.node.
                //
                // Now access that attribute.
                //
                streetAttr = addressElement.GetAttribute("street");//.node.
                //
                // The "street" attribute should now have a default values 
                //
                computedValue = addressElement.GetAttribute("street");//.node.
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

        //------------------------ End test case core-0008E --------------------------
        //
        //-------------------------- test case core-0009E ----------------------------
        //
        // Testing feature - The "getAttributeNode(name)" retrieves an attribute
        //                   node by name.  
        //
        // Testing approach - Retrieve the attribute named "domestic" from the last 
        //                    child of the first employee.  Since the method returns
        //                    an Attr object, its name attribute can be examined to 
        //                    ensure the proper attribute was retrieved.
        //
        // Semantic Requirements: 1, 9 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0009E()
        {
            string computedValue = "";
            string expectedValue = "domestic";
            System.Xml.XmlElement addressElement = null;
            System.Xml.XmlAttribute domesticAttrNode = null;

            testResults results = new testResults("Core0009E");
            try
            {
                results.description = "The \"getAttributeNode(name)\" method of an Element " +
                    "returns the \"name\" Attr node.";
                //
                // Access the last child of the first employee.
                //
                addressElement = (System.Xml.XmlElement)util.nodeObject(util.FIRST,util.SIXTH);
                //
                // Invoke its "getAttributeNode(name)" method where name = "domestic"
                // and create an Attr object.  
                //
                domesticAttrNode = addressElement.GetAttributeNode("domestic");//.node.
                //
                // Now access the "name" attribute of that Attr node.  Since the "domestic"
                // attribute was retrieved, the name of the Attr node should also be
                // "domestic". 
                //
                computedValue = domesticAttrNode.Name;
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

        //------------------------ End test case core-0009E --------------------------
        //
        //-------------------------- test case core-00010E ----------------------------
        //
        // Testing feature - The "getAttributeNode(name)" retrieves an attribute
        //                   node by name.  It should return null if the "name" 
        //                   attribute does not exist.
        //
        // Testing approach - Retrieve the last child of the first employee and 
        //                    attempt to retrieve a non-existing attribute.
        //                    The method should return null.  The non-existing
        //                    attribute to be used is "invalidAttribute".
        //
        // Semantic Requirements: 1, 10
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0010E()
        {
            object computedValue = null;
            object expectedValue = null;
            System.Xml.XmlElement addressElement = null;

            testResults results = new testResults("Core0010E");
            try
            {
                results.description = "The \"getAttributeNode(name)\" method returns null " +
                    "if the \"name\" attribute does not exist.";
                //
                // Access the last child of the first employee.
                //
                addressElement = (System.Xml.XmlElement)util.nodeObject(util.FIRST,util.SIXTH);
                //
                // Invoke its "getAttributeNode(name)" method where name = "invalidAttribute"
                // This should result in a null value being returned by the method.
                //
                computedValue = addressElement.GetAttributeNode("invalidAttribute");//.node.
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
        }

        //------------------------ End test case core-0010E --------------------------
        //
        //-------------------------- test case core-0011E ----------------------------
        //
        // Testing feature - The "setAttributeNode(newAttr)" adds a new attribute
        //                   to the Element.
        //
        // Testing approach - Retrieve the last child of the first employee and
        //                    add a new attribute node to it by invoking its 
        //                    "setAttributeNode(newAttr)" method.  This test makes 
        //                    use of the "createAttribute(name)" method from the 
        //                    Document interface.
        //
        // Semantic Requirements: 1, 11
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0011E()
        {
            string computedValue = "";
            string expectedValue = "";
            System.Xml.XmlElement addressElement = null;
            System.Xml.XmlAttribute newAttribute = null;
            string name = "district";

            testResults results = new testResults("Core0011E");
            try
            {
                results.description = "The \"setAttributeNode(newAttr)\" method adds a new " +
                    "attribute node to the element.";
                //
                // Access the last child of the first employee.
                //
                newAttribute = (System.Xml.XmlAttribute)util.createNode(util.ATTRIBUTE_NODE,name);
                addressElement = (System.Xml.XmlElement)util.nodeObject(util.FIRST,util.SIXTH);
                //
                // Invoke its "setAttributeNode(newAttr)" method where 
                // newAttr = "newAttribute".  Since no value was specified or given 
                // by default, its value should be the empty string. 
                //
                addressElement.SetAttributeNode(newAttribute);//.node.
                computedValue = addressElement.GetAttribute(name);//.node.
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

        //------------------------ End test case core-0011E --------------------------
        //
        //-------------------------- test case core-00012E ----------------------------
        //
        // Testing feature - The "setAttributeNode(newAttr)" method adds a new attribute
        //                   to the Element.  If the "newAttr" Attr node is already
        //                   present in this element, it should replace the existing
        //                   one.
        //
        // Testing approach - Retrieve the last child of the third employee and
        //                    add a new attribute node to it by invoking its
        //                    "setAttributeNode(newAttr)" method.  The new attribute 
        //                    node to be added is "street", which is already
        //                    present in this element.  The method should replace the 
        //                    existing Attr node with the new one.  This test make use 
        //                    of the "createAttribute(name)" method from the Document
        //                    interface.
        //
        // Semantic Requirements: 1, 12
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0012E()
        {
            string computedValue = "";
            string expectedValue = "";
            System.Xml.XmlElement addressElement = null;
            System.Xml.XmlAttribute newAttribute = null;

            testResults results = new testResults("Core0012E");
            try
            {
                results.description = "The \"setAttributeNode(newAttr)\" method when " +
                    "the \"newAttr\" node is already part of this " +
                    "element.  The existing attribute node should be "+
                    "replaced with the new one."; 
                //
                // Access the last child of the third employee.
                //
                newAttribute = (System.Xml.XmlAttribute)util.createNode(util.ATTRIBUTE_NODE,"street");  
                addressElement = (System.Xml.XmlElement)util.nodeObject(util.THIRD,util.SIXTH);
                //
                // Invoke its "setAttributeNode(newAttr)" method where 
                // newAttr = "newAttribute".  That attribute is already part of this 
                // element.  The existing attribute should be replaced with the new one 
                //    (newAttribute).
                //
                addressElement.SetAttributeNode(newAttribute);//.node.
                computedValue = addressElement.GetAttribute("street");//.node.
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

        //------------------------ End test case core-0012E --------------------------
        //
        //-------------------------- test case core-00013E ----------------------------
        //
        // Testing feature - If The "setAttributeNode(newAttr)" method replaces 
        //                   an existing Attr node with the same name, then it 
        //                   should return the previously existing Attr node.
        //
        // Testing approach - Retrieve the last child of the third employee and add
        //                    a new attribute node to it.  The new attribute node to 
        //                    be added is "street", which is already present in this
        //                    Element.  The method should return the existing Attr 
        //                    node(old "street" Attr).  This test make use of the 
        //                    "createAttribute(name)" method from the Document 
        //                    interface.
        //
        // Semantic Requirements: 1, 13
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0013E()
        {
            string computedValue = "";
            string expectedValue = "No";
            System.Xml.XmlElement addressElement = null;
            System.Xml.XmlAttribute oldStreetAttribute = null;
            System.Xml.XmlAttribute newAttribute = null;

            testResults results = new testResults("Core0013E");
            try
            {
                results.description = "The \"setAttributeNode(newAttr)\" method when the " +
                    "\"newAttr\" attribute node is already present in " +
                    "this element.  The method should return the previously " +
                    "existing Attr node."; 
                //
                // Access the last child of the third employee.
                //
                newAttribute = (System.Xml.XmlAttribute)util.createNode(util.ATTRIBUTE_NODE,"street");
                addressElement = (System.Xml.XmlElement)util.nodeObject(util.THIRD,util.SIXTH);
                //
                // Invoke its "setAttributeNode(newAttr)" method where 
                // newAttr was just created with the same name as an already existing
                // attribute("street"). The existing attribute should be replaced with the 
                // new one and the method should return the existing "street" Attr node.  
                //
                oldStreetAttribute = addressElement.SetAttributeNode(newAttribute);//.node.
                //
                // The "oldStreetAttribute" now contains the old Attr node and its 
                // "value" attribute should be available for examination.
                //
                computedValue = oldStreetAttribute.Value;
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

        //------------------------ End test case core-0013E --------------------------
        //
        //-------------------------- test case core-00014E ----------------------------
        //
        // Testing feature - The "setAttributeNode(newAttr)" method returns the 
        //                   null value if no previously existing Attr node with the 
        //                   same name was replaced.
        //
        // Testing approach - Retrieve the last child of the third and add a new 
        //                    attribute node to it.  The new attribute node to be 
        //                    added is "district", which is not part of this Element.  
        //                    The method should return the null value.  This test makes
        //                    use of the "createAttribute(name)" method from the
        //                    Document interface.
        //
        // Semantic Requirements: 1, 15
        //
        //----------------------------------------------------------------------------

        [Test]
	[Ignore(".NET DOM implementation does not match W3C DOM specification.")] // MS DOM is buggy
	public void core0014E()
        {
            object computedValue = null;
            object expectedValue = null;
            System.Xml.XmlElement addressElement = null;
            System.Xml.XmlAttribute newAttribute = null;

            testResults results = new testResults("Core0014E");
            try
            {
                results.description = "The \"setAttributeNode(newAttr)\" method returns a " +
                    "null value if no previously existing Attr node was replaced.";
                //
                // Access the sixth child of the third employee.
                //
                newAttribute = (System.Xml.XmlAttribute)util.createNode(util.ATTRIBUTE_NODE,"district");
                addressElement = (System.Xml.XmlElement)util.nodeObject(util.THIRD,util.SIXTH);
                //
                // Invoke its "setAttributeNode(newAttr)" method where name = "newAttribute".
                // This attribute is not part of this element.  The method should add the
                // new Attribute and return a null value.
                //
                computedValue = addressElement.SetAttributeNode(newAttribute);//.node.
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

            util.resetData();

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0014E --------------------------
        //
        //-------------------------- test case core-00015E ----------------------------
        //
        // Testing feature - The "removeAttributeNode(oldAttr)" method removes the 
        //                   specified attribute. 
        //
        // Testing approach - Retrieve the last child of the third employee, add
        //                    a new "district" node to it and the try to remove it. 
        //                    To verify that the node was removed this test uses the 
        //                    "getNamedItem(name)" from the NamedNodeMap interface.   
        //                    This test also makes use of the "attributes" attribute 
        //                    from the Node interface.
        //
        // Semantic Requirements: 1, 14
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0015E()
        {
            object computedValue = null;
            object expectedValue = null;
            System.Xml.XmlElement addressElement = null;
            System.Xml.XmlAttributeCollection attrList = null;
            System.Xml.XmlAttribute newAttribute = null;
            newAttribute = (System.Xml.XmlAttribute)util.createNode(util.ATTRIBUTE_NODE,"district");

            testResults results = new testResults("Core0015E");
            try
            {
                results.description = "The \"removeAttributeNode(oldAttr)\" method removes the " +
                    "specified attribute node.";
                //
                // Access the sixth child of the third employee and add the new
                // attribute to it.
                //
                addressElement = (System.Xml.XmlElement)util.nodeObject(util.THIRD,util.SIXTH);
                addressElement.SetAttributeNode(newAttribute);//.node.
                //
                // Invoke its "removeAttributeNode(oldAttr)" method where 
                // name = "newAttribute" and remove that attribute node.
                //
                addressElement.RemoveAttributeNode(newAttribute);//.node.
                //
                // To ensure that the "district" attribute was indeed removed, a listing
                // of all attributes is created by invoking the "attributes" attribute
                // of "addressElement".  After the list is created, we attempt to
                // retrieve the "district" element from the list.  A null value should
                // be return in its place.
                //
                attrList = addressElement.Attributes;
                computedValue = attrList.GetNamedItem("district");
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

            util.resetData();

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0015E --------------------------
        //
        //-------------------------- test case core-00016E ----------------------------
        //
        // Testing feature - The "removeAttributeNode(oldAttr)" method removes the 
        //                   specified attribute node and restore any default values.
        //
        // Testing approach - Retrieve the last child of the third employee and
        //                    remove its "street" Attr node.  Since this node has
        //                    default value defined in the DTD file, that default
        //                    value should immediately be the new value.  
        //
        // Semantic Requirements: 1, 15
        //
        //----------------------------------------------------------------------------

        [Test]
        [Ignore(".NET DOM implementation does not match W3C DOM specification.")]
	public void core0016E()
        {
            string computedValue = "";
            string expectedValue = "Yes";
            System.Xml.XmlElement addressElement = null;
            System.Xml.XmlAttribute streetAttr = null;
            //System.Xml.XmlNode thirdEmployee = null;

            testResults results = new testResults("Core0016E");
            try
            {
                results.description = "The \"removeAttributeNode(oldAttr)\" method removes the " +
                    "specified attribute node and restores any default values.";
                //
                // Access the sixth child of the third employee.
                //
                addressElement = (System.Xml.XmlElement)util.nodeObject(util.THIRD,util.SIXTH);
                //
                // Create an instance of an Attr object by retrieving the "street"
                // attribute node, invoke its "removeAttributeNode(oldAttr)" method
                // where name = "streetAttr" and remove that attribute node.  Note that 
                // "the removeAttributeNode(oldAttr)" takes an Attr object as its 
                // parameter, that is why an Attr object (named "street") is first created. 
                //
                streetAttr = addressElement.GetAttributeNode("street");//.node.
                addressElement.RemoveAttributeNode(streetAttr);//.node.
                //
                // Since there is a default value defined for the "street" attribute, it
                // should immediately be the new value for that attribute. 
                //
                computedValue = addressElement.GetAttribute("street");//.node.
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

        //------------------------ End test case core-0016E --------------------------
        //
        //-------------------------- test case core-00017E ----------------------------
        //
        // Testing feature - The "removeAttributeNode(oldAttr)" method returns the 
        //                   node that was removed.
        //
        // Testing approach - Retrieve the last child of the third employee and 
        //                    remove its "street" Attr node.  The method should 
        //                    return the old attribute node.
        //
        // Semantic Requirements: 1, 16
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0017E()
        {
            string computedValue = "";
            string expectedValue = "No";
            System.Xml.XmlElement addressElement = null;
            System.Xml.XmlAttribute streetAttr = null;
            System.Xml.XmlAttribute oldStreetAttribute = null;

            testResults results = new testResults("Core0017E");
            try
            {
                results.description = "The \"removeAttributeNode(oldAttr)\" method returns the "+
                    "removed attribute node.";
                //
                // Access the sixth child of the third employee.
                //
                addressElement = (System.Xml.XmlElement)util.nodeObject(util.THIRD,util.SIXTH);

                // create an instance of an Attr object by retrieving the "street"
                // attribute node, invoke its "removeAttributeNode(oldAttr)" method
                // where name = "streetAttr" and remove that attribute node.  Note that
                // "the removeAttributeNode(oldAttr)" takes an Attr object as its
                // parameter, that is why an Attr object (named "street") is first created.
                //
                streetAttr = addressElement.GetAttributeNode("street");//.node.
                oldStreetAttribute = addressElement.RemoveAttributeNode(streetAttr);//.node.
                //
                // The method should return the removed attribute node.  Its value can then
                // be examined.
                //
                computedValue = oldStreetAttribute.Value;
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

        //------------------------ End test case core-0017E --------------------------
        //
        //-------------------------- test case core-00018E ----------------------------
        //
        // Testing feature - The "getElementsByTagName(name)" method returns a list 
        //                   of all descendant Elements with the given tag name.
        //
        // Testing approach - Get a listing of all the descendant elements of the
        //                    root element using the string "employee" as the tag
        //                    name.  The  method should return a Node list of length 
        //                    equal to 5.  This test makes use of the "length" 
        //                    attribute from the NodeList interface.
        //
        // Semantic Requirements: 1, 17
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0018E()
        {
            int computedValue = 0;
            int expectedValue = 5;
            System.Xml.XmlNodeList employeeList = null;
            System.Xml.XmlElement docElement = null;

            testResults results = new testResults("Core0018E");

            results.description = "The \"getElementsByTagName(name)\" method returns a "+
                "NodeList of all descendant elements with the given " +
                "tag name(method returning a non-empty list)";
            //
            // get a listing of all the elements that match the tag "employee".
            //
            docElement = util.getRootNode();
            employeeList = docElement.GetElementsByTagName("employee");
            //
            // The method should return a NodeList whose length can then be examined. 
            //
            computedValue = employeeList.Count;
            //
            // Write out results
            //
            results.expected = expectedValue.ToString();
            results.actual = computedValue.ToString();

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0018E --------------------------
        //
        //-------------------------- test case core-00019E ----------------------------
        //
        // Testing feature - The "getElementsByTagName(name)" returns a list of all
        //                   descendant Elements with the given tag name.  Test
        //                   for an empty list.
        //
        // Testing approach - Get a listing of all the descendant elements of the
        //                    root element using the string "noMatches" as the tag
        //                    name.  The  method should return a NodeList of length
        //                    equal to 0 since no descendant elements match the given
        //                    tag name.  This test makes use of the "length" attribute
        //                    from the NodeList interface.
        //
        // Semantic Requirements: 1, 17
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0019E()
        {
            int computedValue = 0;
            int expectedValue = 0;
            System.Xml.XmlNodeList employeeList = null;
            System.Xml.XmlElement docElement = null;

            testResults results = new testResults("Core0019E");

            results.description = "The \"getElementsByTagName(name)\" method returns a "+
                "NodeList of all descendant elements with the given " +
                "tag name (method returns an empty list)";
            //
            // get a listing of all the elements that match the tag "noMatch".
            //
            docElement = util.getRootNode();
            employeeList = docElement.GetElementsByTagName("noMatch");
            //
            // The method should return a NodeList whose length can then be examined.
            //
            computedValue = employeeList.Count;
            //
            // Write out results
            //
            results.expected = expectedValue.ToString();
            results.actual = computedValue.ToString();

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0019E --------------------------
        //
        //-------------------------- test case core-00020E ----------------------------
        //
        // Testing feature - The "getElementsByTagName(name)" returns a list of all
        //                   descendant Elements in the order the children were
        //                   encountered in a pre order traversal of the element tree.
        //
        // Testing approach - Get a listing of all the descendant elements of the
        //                    root node using the string "employee" as the tag
        //                    name.  The  method should return a Node list of length
        //                    equal to 5 in the order the children were encountered.
        //                    Item number four in the list is accessed using a 
        //                    subscript.  Item number four is itself an Element node
        //                    with children and whose first child should be 
        //                    "employeeId".
        //
        // Semantic Requirements: 1, 18 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0020E()
        {
            string computedValue = "";
            string expectedValue = "employeeId";
            System.Xml.XmlNodeList employeeList = null;
            System.Xml.XmlNode fourthEmployee = null;
            System.Xml.XmlElement docElement = null;

            testResults results = new testResults("Core0020E");

            results.description = "The \"getElementsByTagName(name)\" returns a NodeList " +
                "of all descendant elements in the order the " +
                "children were encountered in a preorder traversal " +
                "of the element tree.";
            //
            // get a listing of all the elements that match the tag "employee".
            //
            docElement = util.getRootNode();
            employeeList = docElement.GetElementsByTagName("employee");

            //
            // The method should return a NodeList of the children in the order the 
            // children were encountered.  Since "employeeList" is a NodeList we should 
            // be able to access its elements by using a subscript.  Item number four 
            // is itself an Element node with six children and the first child 
            // is "employeeId". 
            //
            fourthEmployee = employeeList.Item(util.FOURTH);
            computedValue = fourthEmployee.FirstChild.Name;
            //
            // Write out results
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0020E --------------------------
        //
        //-------------------------- test case core-00021E ----------------------------
        //
        // Testing feature - The "getElementsByTagName(name)" method may use the 
        //                   special value "*" to match all the tags in the element 
        //                   tree. 
        //
        // Testing approach - Get a listing of all the descendant elements of the
        //                    last employee by using the special value of "*".  The 
        //                    method should return all of the descendant children
        //                    (total of 6) in the order the children were encountered.
        //
        // Semantic Requirements: 1, 19 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0021E()
        {
            string computedValue = "";
            string expectedValue = "employeeId name position salary gender address ";
            System.Xml.XmlNodeList elementList = null;
            System.Xml.XmlElement lastEmployee = null;

            testResults results = new testResults("Core0021E");

            results.description = "The \"getElementsByTagName(name)\" method may use the " +
                "special value \"*\" to match all the tags in the " +
                "element tree.";
            //
            // get a listing of all the descendant elements of the last employee by using
            // the special value of "*".
            //
            lastEmployee = (System.Xml.XmlElement)util.nodeObject(util.FIFTH,-1);
            elementList = lastEmployee.GetElementsByTagName("*");//.node.
            //
            // Traverse the list.
            //
            for (int index = 0;index <= elementList.Count - 1;index++)
                computedValue += elementList.Item(index).Name+" ";
            //
            // Write out results
            //
            results.expected = expectedValue;
            results.actual = computedValue;

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0021E --------------------------
        //
        //-------------------------- test case core-00022E ----------------------------
        //
        // Testing feature - The "normalize()" method puts all the nodes in the
        //                   full depth of the sub-tree underneath this element
        //                   into a "normal" form.
        //
        // Testing approach - Retrieve the third employee and access its second 
        //                    child.  This child contains a block of text that spread
        //                    accross multiple lines.  The content of the "name" 
        //                    child should be parsed and treated as a single Text node.
        //
        // Semantic Requirements: 1, 20
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0022E()
        {
            string computedValue = "";
            string expectedValue = "Roger\n Jones";
            System.Xml.XmlNode idElement = null;
            System.Xml.XmlNode textNode = null;

            testResults results = new testResults("Core0022E");
            try
            {
                results.description = "The \"normalize()\" method puts all the nodes in the " +
                    "full depth of the sub-tree of this element into a normal form.";
                //
                // The "normalize() method should combine all the contiguous blocks of text
                // and form a single "Text" node.  The "nodeValue" of that final Text node
                // should be the combination of all continuos blocks of text that do not
                // contain any markup language. 
                //
                idElement = util.nodeObject(util.THIRD,util.SECOND);
                idElement.Normalize();//.node.
                textNode = idElement.LastChild;//.node.
                //
                // text should be in normal form now
                //
                computedValue = textNode.Value;
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

        //------------------------ End test case core-0022E --------------------------
        //
        //-------------------------- test case core-00023E ---------------------------
        //
        // Testing feature - The "setAttribute(name,value)" method raises an
        //                   INVALID_CHARACTER_ERR Exception if the specified  
        //                   name contains an invalid character.
        //
        // Testing approach - Retrieve the last child of the first employee
        //                    and call its "setAttribute(name,value)" method with
        //                    "name" containing an invalid character.
        //
        // Semantic Requirements: 1, 21
        //
        //----------------------------------------------------------------------------

        [Test]
	[Ignore(".NET DOM implementation does not match W3C DOM specification.")] // MS DOM is buggy
	public void core0023E()
        {
            string computedValue = "";
            System.Xml.XmlElement addressElement = null;
            string expectedValue = "System.ArgumentException";

            testResults results = new testResults("Core0023E");
            try
            {
                results.description = "The \"setAttribute(name,value)\" method raises an " +
                    "ArgumentException if the specified " +
                    "name contains an invalid character.";
                //
                // Access the "address" element of the first employee. 
                //
                addressElement = (System.Xml.XmlElement)util.nodeObject(util.FIRST,util.SIXTH);
                //
                // Attempt to set an attribute with an invalid character in its name.
                //
                try 
                {
                    addressElement.SetAttribute("invalid^Name","thisValue");//.node.
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

        //------------------------ End test case core-0023E --------------------------
        //
        //-------------------------- test case core-0024E ----------------------------
        //
        // Testing feature - The "setAttribute(name,value)" method raises a
        //                   NO_MODIFICATION_ALLOWED_ERR Exception if this 
        //                   node is readonly.
        //
        // Testing approach - Retrieve the Element node inside the Entity node 
        //                    named "ent4" and attempt to set an attribute for
        //                    it.  Descendants of Entity nodes are readonly nodes
        //                    and therefore the desired exception should be raised.
        //
        // Semantic Requirements: 22
        //
        //----------------------------------------------------------------------------

        [Test]
	[Ignore(".NET DOM implementation does not match W3C DOM specification.")] // MS DOM is buggy
	public void core0024E()
        {
            string computedValue = "";
            System.Xml.XmlEntity entityNode = null;
            System.Xml.XmlElement entityDesc = null;
            string expectedValue = "System.ArgumentException";//util.NO_MODIFICATION_ALLOWED_ERR;

            testResults results = new testResults("Core0024E");
            try
            {
                results.description = "The \"setAttribute(name,value)\" method raises a " +
                    "NO_MODIFICATION_ALLOWED_ERR Exception if the node is readonly.";

                //
                // Retreive the targeted data.
                //
                entityNode = util.getEntity("ent4");
                entityDesc = (System.Xml.XmlElement)entityNode.FirstChild;
                //
                // Attempt to set an attribute for a readonly node should raise an exception.
                //
                try 
                {
                    entityDesc.SetAttribute("newAttribute","thisValue");
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

        //------------------------ End test case core-0024E --------------------------
        //
        //-------------------------- test case core-00025E ---------------------------
        //
        // Testing feature - The "removeAttribute(name)" method raises a
        //                   NO_MODIFICATION_ALLOWED_ERR Exception if this
        //                   node is readonly.
        //
        // Testing approach - Retrieve the Element node inside the Entity node
        //                    named "ent4" and attempt to remove an attribute from
        //                    it.  Descendants of Entity nodes are readonly nodes
        //                    and therefore the desired exception should be raised.
        //
        // Semantic Requirements: 23
        //
        //----------------------------------------------------------------------------

        [Test]
	[Ignore(".NET DOM implementation does not match W3C DOM specification.")] // MS DOM is buggy
	public void core0025E()
        {
            string computedValue = "";
            System.Xml.XmlEntity entityNode = null;
            System.Xml.XmlElement entityDesc = null;
            string expectedValue = "System.ArgumentException";//util.NO_MODIFICATION_ALLOWED_ERR;

            testResults results = new testResults("Core0025E");
            try
            {
                results.description = "The \"removeAttribute(name)\" method raises a " +
                    "NO_MODIFICATION_ALLOWED_ERR Exception if the node is readonly.";
                //
                // Retrieve the targeted data.
                //
                entityNode = util.getEntity("ent4");
                entityDesc = (System.Xml.XmlElement)entityNode.FirstChild;
                //
                // Attempt to set an attribute for a readonly node should raise an exception.
                //
                try 
                {
                    entityDesc.RemoveAttribute("attr1");
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

        //------------------------ End test case core-0025E --------------------------
        //
        //-------------------------- test case core-00026E ---------------------------
        //
        // Testing feature - The "setAttributeNode(newAttr)" method raises a
        //                   NO_MODIFICATION_ALLOWED_ERR Exception if this
        //                   node is readonly.
        //
        // Testing approach - Retrieve the Element node inside the Entity node
        //                    named "ent4" and attempt to add a newly created Attr 
        //                    node to it.  Descendants of Entity nodes are readonly 
        //                    nodes and therefore the desired exception should be
        //                    raised.
        //
        // Semantic Requirements: 24
        //
        //----------------------------------------------------------------------------

        [Test]
	[Ignore(".NET DOM implementation does not match W3C DOM specification.")] // MS DOM is buggy
	public void core0026E()
        {
            string computedValue = "";
            System.Xml.XmlEntity entityNode = null;
            System.Xml.XmlElement entityDesc = null;
            System.Xml.XmlAttribute newAttr = (System.Xml.XmlAttribute)util.createNode(util.ATTRIBUTE_NODE,"newAttribute");
            string expectedValue = "System.ArgumentException";//util.NO_MODIFICATION_ALLOWED_ERR;

            testResults results = new testResults("Core0026E");
            try
            {
                results.description = "The \"setAttributeNode(newAttr)\" method raises a " +
                    "NO_MODIFICATION_ALLOWED_ERR Exception if the node is readonly.";
                //
                // Retrieve targeted data
                //
                entityNode = util.getEntity("ent4");
                entityDesc = (System.Xml.XmlElement)entityNode.FirstChild;
                //
                // Attempt to set an attribute for a readonly node should raise an exception.
                //
                try 
                {
                    entityDesc.SetAttributeNode(newAttr);
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

        //------------------------ End test case core-0026E --------------------------
        //
        //-------------------------- test case core-00027E ---------------------------
        //
        // Testing feature - The "removeAttributeNode(newAttr)" method raises a
        //                   NO_MODIFICATION_ALLOWED_ERR Exception if this
        //                   node is readonly.
        //
        // Testing approach - Retrieve the Element node inside the Entity node
        //                    named "ent4" and attempt to remove its "attr1"
        //                    attribute.  Descendants of Entity nodes are readonly
        //                    nodes and therefore the desired exception should be
        //                    raised.
        //
        // Semantic Requirements: 25
        //
        //----------------------------------------------------------------------------

        [Test]
	[Ignore(".NET DOM implementation does not match W3C DOM specification.")] // MS DOM is buggy
	public void core0027E()
        {
            string computedValue = "";
            System.Xml.XmlEntity entityNode = null;
            System.Xml.XmlElement entityDesc = null;
            System.Xml.XmlAttribute oldAttribute = null;
            string expectedValue = "System.ArgumentException";//util.NO_MODIFICATION_ALLOWED_ERR;

            testResults results = new testResults("Core0027E");
            try
            {
                results.description = "The \"removeAttributeNode(newAttr)\" method raises a " +
                    "NO_MODIFICATION_ALLOWED_ERR Exception if the node is readonly.";
                //
                // Get an instance of an attribute node and retrieve targeted data.
                //
                entityNode = util.getEntity("ent4");
                entityDesc = (System.Xml.XmlElement)entityNode.FirstChild;
                oldAttribute = ((System.Xml.XmlElement)entityNode.FirstChild).GetAttributeNode("attr1");
                //
                // Attempt to set remove an attribute node from a readonly node (lastChild).  
                // Should raise an exception. 
                //
                try 
                {
                    entityDesc.RemoveAttributeNode(oldAttribute);
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

        //------------------------ End test case core-0027E --------------------------
        //
        //-------------------------- test case core-00028E ---------------------------
        //
        // Testing feature - The "setAttributeNode(newAttr)" method raises a
        //                   System.ArgumentException Exception if the "newAttr" was 
        //                   created from a different document than the one that
        //                   created this document. 
        //
        // Testing approach - Retrieve the last employee and attempt to set
        //                    a new attribute node for its "employee" element.
        //                    The new attribute was created from a document 
        //                    other than the one that crated this element,
        //                    therefore the desired exception should be raised. 
        //                    This test uses the "createAttribute(newAttr)" method
        //                    from the Document interface.
        //
        // Semantic Requirements: 26
        //
        //----------------------------------------------------------------------------

        [Test]
	[Ignore(".NET DOM implementation does not match W3C DOM specification.")] // MS DOM is buggy
	public void core0028E()
        {
            System.Xml.XmlElement addressElement = null;
            string computedValue = "";
            System.Xml.XmlAttribute newAttr = null;
            string expectedValue = "System.ArgumentException";

            testResults results = new testResults("Core0028E");
            try
            {
                results.description = "The \"setAttributeNode(newAttr)\" method raises a " +
                    "System.ArgumentException Exception if \"newAttr\" was created " +
                    "from a different document than the one who created this node.";
                //
                // Access the address Element of the last employee and attempt to set 
                // a new attribute node. 
                //
                newAttr = util.getOtherDOMDocument().CreateAttribute("newAttribute");
                addressElement = (System.Xml.XmlElement)util.nodeObject(util.FIFTH,util.SIXTH);
                //
                // The new attribute was created from a different document and therefore 
                // an exception should be raised.
                //
                try 
                {
                    addressElement.SetAttributeNode(newAttr);//.node.
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

        //------------------------ End test case core-0028E --------------------------
        //
        //-------------------------- test case core-00029E ---------------------------
        //
        // Testing feature - The "setAttributeNode(newAttr)" method raises an
        //                   InvalidOperationException if the "newAttr"
        //                   attribute is already an attribute of another element. 
        //
        // Testing approach - Retrieve the last employee and attempt to set an
        //                    attribute node to one of its children that
        //                    already exist in another children.  The attribute
        //                    node used is "street", which already exist in the
        //                    "address" element.  An instance of that attribute
        //                    node is first retrived from the "address" element and
        //                    then attempted to be set in the "employeeId" element.  
        //                    This should cause the intended exception to be raised.
        //
        // Semantic Requirements: 27
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0029E()
        {
            string computedValue = "";
            System.Xml.XmlElement employeeIdElement = null;
            System.Xml.XmlElement addressElement = null;
            System.Xml.XmlAttribute newAttribute = null; 
            string expectedValue = "InvalidOperationException";

            testResults results = new testResults("Core0029E");
            try
            {
                results.description = "The \"setAttributeNode(newAttr)\" method raises an "+
                    "InvalidOperationException if \"newAttr\" attribute "+
                    "is already being used by another element.";
                //
                // Retrieve an already existing attribute from the "address" element.
                // 
                addressElement =  (System.Xml.XmlElement)util.nodeObject(util.FIFTH,util.SIXTH);
                newAttribute = addressElement.GetAttributeNode("street");//.node.
                //
                // Access the "employeeId" element of the last employee.
                //
                employeeIdElement = (System.Xml.XmlElement)util.nodeObject(util.FIFTH,util.FIRST);
                //
                // Attempt to set an attribute node with an already existing attribute node  
                // in another element.
                //
                try 
                {
                    employeeIdElement.SetAttributeNode(newAttribute);//.node.
                }
                catch(System.InvalidOperationException) 
                { 
                    computedValue = "InvalidOperationException"; 
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

        //------------------------ End test case core-0029E -------------------------
        //
        //-------------------------- test case core-0030E ---------------------------
        //
        // Testing feature - The "removeAttributeNode(oldAttr)" method raises a 
        //                   NOT_FOUND_ERR Exception if the "oldAttr" attribute
        //                   is not an attribute of the element.
        //
        // Testing approach - Retrieve the last employee and attempt to remove
        //                    a non existing attribute node.   This should cause 
        //                    the intended exception be raised.  This test makes use
        //                    of the "createAttribute(name)" method from the
        //                    Document interface.
        //
        // Semantic Requirements: 28
        //
        //----------------------------------------------------------------------------

        [Test]
	[Ignore(".NET DOM implementation does not match W3C DOM specification.")] // MS DOM is buggy
	public void core0030E()
        {
            string computedValue = "";
            System.Xml.XmlElement addressElement = null;
            System.Xml.XmlAttribute oldAttribute = (System.Xml.XmlAttribute)util.createNode(util.ATTRIBUTE_NODE,"oldAttribute");
            string expectedValue = "System.ArgumentException";//util.NOT_FOUND1_ERR;

            testResults results = new testResults("Core0030E");
            try
            {
                results.description = "The \"removeAttributeNode(oldAttr)\" method raises a " +
                    "NOT_FOUND_ERR Exception if \"oldAttr\" attribute " +
                    "is not an attribute of the element.";
                //
                // Access the "address" element of the last employee.
                //
                addressElement = (System.Xml.XmlElement)util.nodeObject(util.FIFTH,util.SIXTH);
                //
                // Attempt to remove a non-existing attribute. Should raise exception.
                //
                try 
                {
                    addressElement.RemoveAttributeNode(oldAttribute);//.node.
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

        //------------------------ End test case core-0030E --------------------------
    }
}
