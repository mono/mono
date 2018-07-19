//**************************************************************************
//
//
//                       National Institute Of Standards and Technology
//                                     DTS Version 1.0
//         
//                                 NamedNodeMap Interface
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
    public class NamedNodeMapTest
    {
        public static int i = 2;
/*
        public testResults[] RunTests()
        {
            testResults[] tests = new testResults[] {core0001M(), core0002M(), core0003M(),core0004M(),
                                                        core0005M(), core0006M(), core0007M(), core0008M(),
                                                        core0009M(), core0010M(), core0011M(),
                                                        core0014M(), core0015M(), core0016M(),
                                                        core0017M(), core0018M(), core0019M(), core0020M(),
                                                        core0021M()};
  
            return tests;
        }
*/
        //------------------------ test case core-0001M ------------------------
        //
        // Testing feature - The "getNamedItem(name)" method retrieves a node 
        //                   specified by name.
        //
        // Testing approach - Retrieve the second employee and create a NamedNodeMap 
        //                    listing of the attributes of its last child.  Once 
        //                    the list is created an invocation of the 
        //                    "getNamedItem(name)" method is done where 
        //                    name = "domestic".  This should result on the domestic 
        //                    Attr node being returned.
        //                    
        // Semantic Requirements: 1
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0001M()
        {
            string computedValue = "";
            string expectedValue = "domestic";
            System.Xml.XmlAttribute domesticAttr = null;
            System.Xml.XmlNode testNode = null;

            testResults results = new testResults("Core0001M");
            try
            {
                results.description = "The \"getNamedItem(name)\" method retrieves a node " +
                    "specified by name.";
                //
                // Retrieve targeted data.
                //
                testNode = util.nodeObject(util.SECOND,util.SIXTH);
                domesticAttr = (System.Xml.XmlAttribute)testNode.Attributes.GetNamedItem("domestic");
                computedValue = domesticAttr.Name;
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

        //------------------------ End test case core-0001M --------------------------
        //
        //--------------------------- test case core-0002M ---------------------------
        //
        // Testing feature - The "getNamedItem(name)" method returns a node of any
        //                   type specified by name.
        //
        // Testing approach - Retrieve the second employee and create a NamedNodeMap
        //                    listing of the attributes of its last child.  Once
        //                    the list is created an invocation of the
        //                    "getNamedItem(name)" method is done where 
        //                    name = "street".  This should cause the method to return 
        //                    an Attr node.
        //
        // Semantic Requirements: 2 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0002M()
        {
            string computedValue = "";
            string expectedValue = "street";
            System.Xml.XmlAttribute streetAttr = null;
            System.Xml.XmlNode testNode = null;

            testResults results = new testResults("Core0002M");
            try
            {
                results.description = "The \"getNamedItem(name)\" method returns a node "+
                    "of any type specified by name (test for Attr node).";
                //
                // Retrieve targeted data and get its attributes.
                //
                testNode =  util.nodeObject(util.SECOND,util.SIXTH);
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
        }

        //------------------------ End test case core-0002M --------------------------
        //
        //--------------------------- test case core-0003M ---------------------------
        //
        // Testing feature - The "getNamedItem(name)" method returns null if the
        //                   specified name did not identify any node in the map.
        //
        // Testing approach - Retrieve the second employee and create a NamedNodeMap
        //                    listing of the attributes of its last child.  Once
        //                    the list is created an invocation of the
        //                    "getNamedItem(name)" method is done where 
        //                    name = "district", this name does not match any names 
        //                    in the list and the method should return null.
        //
        // Semantic Requirements: 3 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0003M()
        {
            object computedValue = null;
            object expectedValue = null;
            System.Xml.XmlNode testNode = null;


            testResults results = new testResults("Core0003M");
            try
            {
                results.description = "The \"getNamedItem(name)\" method returns null if the " +
                    "specified name did not identify any node in the map.";
                //
                // Retrieve targeted data and attempt to get a non-existing attribute.
                //
                testNode = util.nodeObject(util.SECOND,util.SIXTH);
                computedValue = testNode.Attributes.GetNamedItem("district");
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

        //------------------------ End test case core-0003M --------------------------
        //
        //--------------------------- test case core-0004M ---------------------------
        //
        // Testing feature - The "setNamedItem(arg)" method adds a node using its
        //                   nodeName attribute. 
        //
        // Testing approach - Retrieve the second employee and create a NamedNodeMap 
        //                    object from the attributes in its last child 
        //                    by invoking the "attributes" attribute.  Once the
        //                    list is created, the "setNamedItem(arg)" method is 
        //                    invoked with arg = newAttr, where newAttr is a new 
        //                    Attr Node previously created.  The "setNamedItem(arg)" 
        //                    method should add the new node to the NamedNodeItem 
        //                    object by using its "nodeName" attribute ("district" 
        //                    in this case).  Further this node is retrieved by using 
        //                    the "getNamedItem(name)" method.  This test uses the 
        //                    "createAttribute(name)" method from the Document 
        //                    interface.
        //
        // Semantic Requirements: 4 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0004M()
        {
            string computedValue = "";
            string expectedValue = "district";
            System.Xml.XmlAttribute districtAttr = null;
            System.Xml.XmlAttribute newAttr = (System.Xml.XmlAttribute)util.createNode(util.ATTRIBUTE_NODE,"district");
            System.Xml.XmlNode testNode = null;

            testResults results = new testResults("Core0004M");
            try
            {
                results.description = "The \"setNamedItem(arg)\" method adds a node "+
                    "using its nodeName attribute.";
                //
                // Retrieve targeted data and add new attribute.
                //
                testNode = util.nodeObject(util.SECOND,util.SIXTH);
                testNode.Attributes.SetNamedItem(newAttr);
                districtAttr = (System.Xml.XmlAttribute)testNode.Attributes.GetNamedItem("district");
                computedValue = districtAttr.Name; 
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

        //------------------------ End test case core-0004M --------------------------
        //
        //--------------------------- test case core-0005 ---------------------------
        //
        // Testing feature - If the node to be added by the "setNamedItem(arg)" method 
        //                   already exists in the NamedNodeMap, it is replaced by the 
        //                   new one.
        //
        // Testing approach - Retrieve the second employee and create a NamedNodeMap
        //                    object from the attributes in its last child.  Once
        //                    the list is created, the "setNamedItem(arg) method is 
        //                    invoked with arg = newAttr, where newAttr is a Node Attr
        //                    previously created and whose node name already exist
        //                    in the map.   The "setNamedItem(arg)" method should 
        //                    replace the already existing node with the new one.  
        //                    Further this node is retrieved by using the 
        //                    "getNamedItem(name)" method.  This test uses the 
        //                    "createAttribute(name)" method from the Document 
        //                    interface.
        //
        // Semantic Requirements: 5 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0005M()
        {
            string computedValue = "";
            string expectedValue = "";
            System.Xml.XmlAttribute streetAttr = null;
            System.Xml.XmlAttribute newAttr = (System.Xml.XmlAttribute)util.createNode(util.ATTRIBUTE_NODE,"street");
            System.Xml.XmlNode testNode = null;

            testResults results = new testResults("Core0005M");
            try
            {
                results.description = "If the node to be replaced by the \"setNamedItem(arg)\" " +
                    "method is already in the list, the existing node should " +
                    "be replaced by the new one.";

                //
                // Retrieve targeted data and add new attribute with name matching an 
                // already existing attribute.
                //
                testNode = util.nodeObject(util.SECOND,util.SIXTH);
                testNode.Attributes.SetNamedItem(newAttr);
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

            util.resetData();
            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0005M --------------------------
        //
        //--------------------------- test case core-0006 ---------------------------
        //
        // Testing feature - If the "setNamedItem(arg)" method replaces an already 
        //                   existing node with the same name then the already existing
        //                   node is returned. 
        //
        // Testing approach - Retrieve the third employee and create a "NamedNodeMap"
        //                    object of the attributes in its last child by
        //                    invoking the "attributes" attribute.  Once the
        //                    list is created, the "setNamedItem(arg) method is 
        //                    invoked with arg = newAttr, where newAttr is a Node Attr
        //                    previously created and whose node name already exist
        //                    in the map.  The "setNamedItem(arg)" method should replace
        //                    the already existing node with the new one and return
        //                    the existing node.  Further this node is retrieved by 
        //                    using the "getNamedItem(name)" method.  This test 
        //                    uses the "createAttribute(name)" method from the Document 
        //                    interface.
        //
        // Semantic Requirements: 6 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0006M()
        {
            string computedValue = "";
            string expectedValue = "No";
            System.Xml.XmlNode returnedNode = null;
            System.Xml.XmlAttribute newAttr = (System.Xml.XmlAttribute)util.createNode(util.ATTRIBUTE_NODE,"street");
            System.Xml.XmlNode testNode = null;

            testResults results = new testResults("Core0006M");
            try
            {
                results.description = "If the \"setNamedItem(arg)\" method replaces an "+
                    "already existing node with the same name then it "+
                    "returns the already existing node.";
                //
                // Retrieve targeted data and examine value returned by the setNamedItem
                // method.
                //
                testNode = util.nodeObject(util.THIRD,util.SIXTH);
                returnedNode = testNode.Attributes.SetNamedItem(newAttr);
                computedValue = returnedNode.Value;
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

        //------------------------ End test case core-0006M --------------------------
        //
        //--------------------------- test case core-0007 ---------------------------
        //
        // Testing feature - The "setNamedItem(arg)" method replace an 
        //                   already existing node with the same name. If a node with 
        //                   that name is already present in the collection, 
        //                   it is replaced by the new one.
        //
        // Testing approach - Retrieve the third employee and create a NamedNodeMap
        //                    object from the attributes in its last child. 
        //                    Once the list is created, the "setNamedItem(arg)" 
        //                    method is invoked with arg = newAttr, where newAttr is 
        //                    a new previously created Attr node The 
        //                    "setNamedItem(arg)" method should add the new node 
        //                    and return the new one.  Further this node is retrieved by 
        //                    using the "getNamedItem(name)" method.  This test 
        //                    uses the "createAttribute(name)" method from the 
        //                    Document interface.
        //
        // Semantic Requirements: 7 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0007M()
        {
            string computedValue = "";
            string expectedValue = "district";
            System.Xml.XmlAttribute newAttr = (System.Xml.XmlAttribute)util.createNode(util.ATTRIBUTE_NODE,"district");
            System.Xml.XmlNode testNode = null;

            testResults results = new testResults("Core0007M");
            try
            {
                results.description = "If a node with that name is already present in the collection. The \"setNamedItem(arg)\" method is replacing it by the new one";
                //
                // Retrieve targeted data and set new attribute.
                //
                testNode = util.nodeObject(util.THIRD,util.SIXTH);
                computedValue = testNode.Attributes.SetNamedItem(newAttr).Name;
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

        //------------------------ End test case core-0007M --------------------------
        //
        //--------------------------- test case core-0008 ----------------------------
        //
        // Testing feature - The "removeNamedItem(name)" method removes a node
        //                   specified by name. 
        //
        // Testing approach - Retrieve the third employee and create a NamedNodeMap
        //                    object from the attributes in its last child. Once
        //                    the list is created, the "removeNamedItem(name)" 
        //                    method is invoked where "name" is the name of an 
        //                    existing attribute.  The "removeNamedItem(name)" method
        //                    should remove the specified attribute and its "specified"
        //                    attribute (since this is an Attr node) should be set
        //                    to false.  
        //
        // Semantic Requirements: 8 
        //
        //----------------------------------------------------------------------------

        [Test]
        [Ignore(".NET DOM implementation does not match W3C DOM specification.")]
        public void core0008M()
        {
            string computedValue = "";
            string expectedValue = "False";
            System.Xml.XmlNode testNode = null;
            System.Xml.XmlAttribute Attr = null;

            testResults results = new testResults("Core0008M");
            try
            {
                results.description = "The \"removeNamedItem(name)\" method removes "+
                    "a node specified by name.";
                //
                // Retrive targeted data and and remove attribute.  It should no longer
                // be specified.
                //
                testNode = (System.Xml.XmlNode)util.nodeObject(util.THIRD,util.SIXTH);
                testNode.Attributes.RemoveNamedItem("street");
                Attr = (System.Xml.XmlAttribute)testNode.Attributes.GetNamedItem("street");
                computedValue = Attr.Specified.ToString();
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

        //------------------------ End test case core-0008M --------------------------
        //
        //--------------------------- test case core-0009 ----------------------------
        //
        // Testing feature - If the node removed by the "removeNamedItem(name)" method
        //                   is an Attr node with a default value, its is immediately
        //                   replaced.
        //
        // Testing approach - Retrieve the third employee and create a NamedNodeMap
        //                    object from the attributes in its last child.  Once
        //                    the list is created, the "removeNamedItem(name)" method
        //                    is invoked where "name" is the name of an existing
        //                    attribute ("street)".  The "removeNamedItem(name)" method
        //                    should remove the "street" attribute and since it has 
        //                    a default value of "Yes", that value should immediately
        //                    be the attribute's value.
        //
        // Semantic Requirements: 9 
        //
        //----------------------------------------------------------------------------

        [Test]
        [Ignore(".NET DOM implementation does not match W3C DOM specification.")]
	public void core0009M()
        {
            string computedValue = "";
            string expectedValue = "Yes";
            System.Xml.XmlNode testNode = null;

            testResults results = new testResults("Core0009M");
            try
            {
                results.description = "If the node removed by the \"removeNamedItem(name)\" "+
                    "method is an Attr node with a default value, then "+
                    "it is immediately replaced.";
                //
                // Retrieve targeted data and remove attribute.
                //
                testNode = util.nodeObject(util.THIRD,util.SIXTH);
                testNode.Attributes.RemoveNamedItem("street");
                computedValue = testNode.Attributes.GetNamedItem("street").Value;
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

        //------------------------ End test case core-0009M --------------------------
        //
        //--------------------------- test case core-0010M ---------------------------
        //
        // Testing feature - The "removeNamedItem(name)" method returns the node removed
        //                   from the map.
        //
        // Testing approach - Retrieve the third employee and create a NamedNodeMap
        //                    object from the attributes in its last child. 
        //                    Once the list is created, the "removeNamedItem(name)" 
        //                    method is invoked where "name" is the name of an existing
        //                    attribute ("street)".  The "removeNamedItem(name)" 
        //                    method should remove the existing "street" attribute
        //                    and return it.
        //
        // Semantic Requirements: 10 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0010M()
        {
            string computedValue = "";
            string expectedValue = "No";
            System.Xml.XmlNode returnedNode = null;
            System.Xml.XmlNode testNode = null;

            testResults results = new testResults("Core0010M");
            try
            {
                results.description = "The \"removeNamedItem(name)\" method returns the "+
                    "node removed from the map.";
                //
                // Retrieve targeted data, remove attribute and examine returned value of
                // removeNamedItem method.
                //
                testNode = util.nodeObject(util.THIRD,util.SIXTH);
                returnedNode = testNode.Attributes.RemoveNamedItem("street");
                computedValue = returnedNode.Value;
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

        //------------------------ End test case core-0010M --------------------------
        //
        //--------------------------- test case core-0011M ---------------------------
        //
        // Testing feature - The "removeNamedItem(name)" method returns null if the
        //                   name specified does not exist in the map.
        //
        // Testing approach - Retrieve the third employee and create a NamedNodeMap
        //                    object from the attributes in its last child.
        //                    Once the list is created, the "removeNamedItem(name)" 
        //                    method is invoked where "name" does not exist in the 
        //                    map.  The method should return null.
        //
        // Semantic Requirements: 11
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0011M()
        {
            object computedValue = null;
            object expectedValue = null;
            System.Xml.XmlNode testNode = null;

            testResults results = new testResults("Core0011M");
            try
            {
                results.description = "The \"removeNamedItem(name)\" method returns null "+
                    "if the specified \"name\" is not in the map.";
                //
                // Retrieve targeted data and attempt to remove a non-existing attribute.
                //
                testNode = util.nodeObject(util.THIRD,util.SIXTH);
                computedValue = testNode.Attributes.RemoveNamedItem("district");
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

        //------------------------ End test case core-0011M --------------------------
        //
        //--------------------------- test case core-0012M ---------------------------
        //
        // Testing feature - The "item(index)" method returns the indexth item in the
        //                   map (test for first item).
        //
        // Testing approach - Retrieve the second employee and create a NamedNodeMap
        //                    object from the attributes in its last child by
        //                    by invoking the "attributes" attribute.  Once
        //                    the list is created, the "item(index)" method is
        //                    invoked with index = 0.  This should return the node at
        //                    the first position.  Since there are no guarantees that
        //                    first item in the map is the one that was listed first 
        //                    in the attribute list the test checks for all of them.
        //
        // Semantic Requirements: 12
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0012M()
        {
            //string testName = "core-0012M";
            string computedValue = "";
//            string expectedValue = "domestic or street";
            string expectedValue = "domestic";
            System.Xml.XmlNode returnedNode = null;
            System.Xml.XmlNode testNode = null;

            testResults results = new testResults("Core0012M");
            try
            {
                results.description = "Retrieve the first item in the map via the \"item(index)\" method."; 

                //
                // Retrieve targeted data and invoke "item" method.
                //  
                testNode = util.nodeObject(util.SECOND,util.SIXTH);
                returnedNode = testNode.Attributes.Item(0);
                computedValue = returnedNode.Name;
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

        //------------------------ End test case core-0012M --------------------------
        //
        //--------------------------- test case core-0013M ---------------------------
        //
        // Testing feature - The "item(index)" method returns the indexth item in the
        //                   map (test for last item).
        //
        // Testing approach - Retrieve the second employee and create a NamedNodeMap
        //                    object from the attributes in its last child. 
        //                    Once the list is created, the "item(index)" method is
        //                    invoked with index = 1.  This should return the node at
        //                    the last position.  Since there are no guarantees that
        //                    the last item in the map is the one that was listed last 
        //                    in the attribute list, the test checks for all of them.
        //
        // Semantic Requirements: 12
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0013M()
        {
            string computedValue = "";
//            string expectedValue = "domestic or street";
            string expectedValue = "street";
            System.Xml.XmlNode returnedNode = null;
            System.Xml.XmlNode testNode = null;

            testResults results = new testResults("Core0013M");
            try
            {
                results.description = "Retrieve the last item in the map via the \"item(index)\" method."; 
                //
                // Retrieve targeted data and invoke "item" attribute.
                //
                testNode = util.nodeObject(util.THIRD,util.SIXTH);
                returnedNode = testNode.Attributes.Item(1);
                computedValue = returnedNode.Name;
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

        //------------------------ End test case core-0013M --------------------------
        //
        //--------------------------- test case core-0014M ---------------------------
        //
        // Testing feature - The "item(index)" method returns null if the index is 
        //                   greater than the number of nodes in the map.
        //
        // Testing approach - Retrieve the second employee and create a NamedNodeMap
        //                    object from the attributes in its last child. 
        //                    element by invoking the "attributes" attribute.  Once
        //                    the list is created, the "item(index)" method is
        //                    invoked with index = 3.  This index value is greater than
        //                    the number of nodes in the map and under that condition
        //                    the method should return null.
        //
        // Semantic Requirements: 13
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0014M()
        {
            object computedValue = null;
            object expectedValue = null;
            System.Xml.XmlNode testNode = null;

            testResults results = new testResults("Core0014M");
            try
            {
                results.description = "The \"item(index)\" method returns null if the "+
                    "index is greater than the number of nodes in the map.";

                //
                // Retrieve targeted data and invoke "item" method.
                //
                testNode = util.nodeObject(util.THIRD,util.SIXTH);
                computedValue = testNode.Attributes.Item(3);
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

        //------------------------ End test case core-0014M --------------------------
        //
        //--------------------------- test case core-0015M ---------------------------
        //
        // Testing feature - The "item(index)" method returns null if the index is
        //                   equal to the number of nodes in the map.
        //
        // Testing approach - Retrieve the second employee and create a NamedNodeMap
        //                    object from the attributes in its last child 
        //                    Once the list is created, the "item(index)" method is
        //                    invoked with index = 2.  This index value is equal to 
        //                    the number of nodes in the map and under that condition
        //                    the method should return null (first item is at position
        //                    0).
        //
        // Semantic Requirements: 13
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0015M()
        {
            object computedValue = null;
            object expectedValue = null;
            System.Xml.XmlNode testNode = null;

            testResults results = new testResults("Core0015M");
            try
            {
                results.description = "The \"item(index)\" method returns null if the index " +
                    "is equal to the number of nodes in the map.";
                //
                // Retrieve targeted data and invoke "item" method.
                //
                testNode = util.nodeObject(util.THIRD,util.SIXTH);
                computedValue = testNode.Attributes.Item(2);
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

        //------------------------ End test case core-0015M --------------------------
        //
        //--------------------------- test case core-0016M ---------------------------
        //
        // Testing feature - The "length" attribute contains the total number of
        //                   nodes in the map.
        //
        // Testing approach - Retrieve the second employee and create a NamedNodeMap
        //                    object from the attributes in its last child. 
        //                    Once the list is created, the "length" attribute is
        //                    invoked.  That attribute should contain the number 2. 
        //
        // Semantic Requirements: 14
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0016M()
        {
            string computedValue = "";
            string expectedValue = "2";
            System.Xml.XmlNode testNode = null;

            testResults results = new testResults("Core0016M");
            try
            {
                results.description = "The \"length\" attribute contains the number of " +
                    "nodes in the map.";
                //
                // Retrieve targeted data and invoke "length" attribute.
                //
                testNode = util.nodeObject(util.THIRD,util.SIXTH);
                computedValue = testNode.Attributes.Count.ToString();
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

        //------------------------ End test case core-0016M --------------------------
        //
        //--------------------------- test case core-0017M ---------------------------
        //
        // Testing feature - The range of valid child nodes indices is 0 to length - 1.
        //
        // Testing approach - Create a NamedNodeMap object from the attributes of the
        //                    last child of the third employee and traverse the
        //                    list from index 0 to index length - 1.  All indices
        //                    should be valid.
        //
        // Semantic Requirements: 15 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0017M()
        {
            string computedValue = "";
            string expectedValue = "0 1 ";
            int lastIndex = 0;
            //string attributes = "";
            System.Xml.XmlNode testNode = null;

            testResults results = new testResults("Core0017M");
            try
            {
                results.description = "The range of valid child nodes indices is 0 to " +
                    "length - 1.";
                //
                // Retrieve targeted data and compute list length.
                //
                testNode = util.nodeObject(util.THIRD,util.SIXTH);
                lastIndex = testNode.Attributes.Count - 1;
                //
                // Traverse the list from 0 to length - 1.  All indices should be valid.
                //
                for (int index = 0;index <= lastIndex; index++)
                    computedValue += index+" ";
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

        //------------------------ End test case core-0017M --------------------------
        //
        //--------------------------- test case core-0018M ---------------------------
        //
        // Testing feature - The "setNamedItem(arg) method raises a System.ArgumentException
        //                   Exception if "arg" was created from a different 
        //                   document than the one that created the NamedNodeMap.
        //
        // Testing approach - Create a NamedNodeMap object from the attributes of the
        //                    last child of the third employee and attempt to 
        //                    add another Attr node to it that was created from a 
        //                    different DOM document.  This condition should raise
        //                    the desired exception.  This method uses the
        //                    "createAttribute(name)" method from the Document
        //                    interface. 
        //
        // Semantic Requirements: 16
        //
        //----------------------------------------------------------------------------

        [Test]
	[Ignore(".NET DOM implementation does not match W3C DOM specification.")] // MS DOM is buggy
	public void core0018M()
        {
            string computedValue = "";
            
            System.Xml.XmlAttribute newAttrNode = util.getOtherDOMDocument().CreateAttribute("newAttribute");
            System.Xml.XmlNode testNode = null;
            string expectedValue = "System.ArgumentException";

            testResults results = new testResults("Core0018M");

            results.description = "The \"setNamedItem(arg)\" method raises a "+
                "System.ArgumentException Exception if \"arg\" was " +
                "created from a document different from the one that created "+
                "the NamedNodeList.";
            //
            // Retrieve targeted data and attempt to add an element that was created
            // from a different document.  Should raise an exception.
            //
            testNode = util.nodeObject(util.THIRD,util.SIXTH);

            try 
            {
                testNode.Attributes.SetNamedItem(newAttrNode);
            } 
            catch(System.Exception ex) 
            {
                computedValue = ex.GetType().ToString();
            }


            results.expected = expectedValue;
            results.actual = computedValue;

            util.resetData();

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0018M --------------------------
        //
        //--------------------------- test case core-0019M ---------------------------
        //
        // Testing feature - The "setNamedItem(arg) method raises a 
        //                   NO_MODIFICATION_ALLOWED_ERR Exception if this
        //                   NamedNodeMap is readonly.
        //
        // Testing approach - Create a NamedNodeMap object from the first child of the
        //                    Entity named "ent4" inside the DocType node and then 
        //                    attempt to add a new item to the list.  It should raise 
        //                    the desired exception as this is a readonly NamedNodeMap.
        //                   
        // Semantic Requirements: 17
        //
        //----------------------------------------------------------------------------

        [Test]
	[Ignore(".NET DOM implementation does not match W3C DOM specification.")] // MS DOM is buggy
	public void core0019M()
        {
            string computedValue = "";
            System.Xml.XmlNode testNode = null;
            System.Xml.XmlNode entityDesc;
            System.Xml.XmlAttribute newAttrNode = (System.Xml.XmlAttribute)util.createNode(util.ATTRIBUTE_NODE,"newAttribute");
            string expectedValue = "System.ArgumentException";//util.NO_MODIFICATION_ALLOWED_ERR;

            testResults results = new testResults("Core0019M");

            results.description = "The \"setNamedItem(arg)\" method raises a " +
                "NO_MODIFICATION_ALLOWED_ERR Exception if this "+
                "NamedNodeMap is readonly.";
            //
            // Create a NamedNodeMap object and attempt to add a node to it.
            // Should raise an exception.
            //
            testNode = util.getEntity("ent4");
            entityDesc = testNode.FirstChild;

            try 
            {
                entityDesc.Attributes.SetNamedItem(newAttrNode);
            }
            catch(ArgumentException ex) 
            {
                computedValue = ex.GetType ().FullName; 
            }


            results.expected = expectedValue;
            results.actual = computedValue;

            util.resetData();

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0019M --------------------------
        //
        //--------------------------- test case core-0020M ---------------------------
        //
        // Testing feature - The "setNamedItem(arg) method raises an
        //                   INUSE_ATTRIBUTE_ERR Exception if "arg" is an Attr 
        //                   that is already an attribute of another Element.
        //
        // Testing approach - Create a NamedNodeMap object from the attributes of the
        //                    third child and attempt to add an attribute that is
        //                    already being used by the first employee.  An attempt
        //                    to add such an attribute should raise the desired
        //                    exception. 
        //
        // Semantic Requirements: 18
        //
        //----------------------------------------------------------------------------

        [Test]
	[Ignore(".NET DOM implementation does not match W3C DOM specification.")]
	public void core0020M()
        {
            string computedValue= "";
            System.Xml.XmlAttribute inUseAttribute = null;
            System.Xml.XmlElement firstEmployee = null;
            System.Xml.XmlNode testNode = null;
            string expectedValue = "System.ArgumentException";//util.INUSE_ATTRIBUTE_ERR;

            testResults results = new testResults("Core0020M");
            try
            {
                results.description = "The \"setNamedItem(arg)\" method raises an "+
                    "INUSE_ATTRIBUTE_ERR Exception if \"arg\" "+
                    "is an Attr node that is already an attribute "+
                    "of another Element.";

                firstEmployee = (System.Xml.XmlElement)util.nodeObject(util.FIRST,util.SIXTH);
                inUseAttribute = firstEmployee.GetAttributeNode("domestic");
                //
                // Attempt to add an attribute that is already used by another element 
                // should raise an exception.
                //
                testNode = util.nodeObject(util.THIRD,util.SIXTH);

                try 
                {
                    testNode.Attributes.SetNamedItem(inUseAttribute);
                }
                catch (System.Exception ex) 
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

        //------------------------ End test case core-0020M --------------------------
        //
        //--------------------------- test case core-0021M ---------------------------
        //
        // Testing feature - The "removeNamedItem(name) method raises an
        //                   NOT_FOUND_ERR Exception if there is no node
        //                   named "name" in the map.
        //
        // Testing approach - Create a NamedNodeMap object from the attributes of the
        //                    last child of the third employee and attempt to
        //                    remove the "district" attribute.  There is no node named
        //                    "district" in the list and therefore the desired 
        //                    exception should be raised.
        //
        // System.Xml       - return null, if a matching node was not found.
        //
        // Semantic Requirements: 19
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0021M()
        {
            object computedValue = null;
            System.Xml.XmlNode testNode = null;
            object expectedValue = null;//util.NOT_FOUND1_ERR;

            testResults results = new testResults("Core0021M");
            try
            {
                results.description = "The \"removeNamedItem(name)\" method raises a " +
                    "NOT_FOUND_ERR Exception if there is no node "+
                    "named \"name\" in the map.";
                //
                // Create a NamedNodeMap object and attempt to remove an attribute that
                // is not in the list should raise an exception.
                //
                testNode = util.nodeObject(util.THIRD,util.SIXTH);

                try 
                {
                    //null if a matching node was not found
                    computedValue = testNode.Attributes.RemoveNamedItem("district");
                }
                catch(System.Exception ex) 
                {
                    computedValue = "EXCEPTION " + ex.GetType () + " : " + ex.Message;
                }

            }
            catch(System.Exception ex)
            {
                computedValue = "Exception " + ex.Message;
            }

            results.expected = (expectedValue == null).ToString();
            results.actual = (computedValue == null).ToString();

            util.resetData();

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0021M --------------------------
    }
}
