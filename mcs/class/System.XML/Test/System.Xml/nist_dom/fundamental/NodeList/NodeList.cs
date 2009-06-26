//**************************************************************************
//
//
//                       National Institute Of Standards and Technology
//                                    DTS Version 1.0
//         
//                                   NodeList Interface
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
    public class NodeListTest
    {
        public static int i = 2;
/*
        public testResults[] RunTests()
        {
            testResults[] tests = new testResults[] {core0001N(), core0002N(), core0003N(),core0004N(),
                                                        core0005N(), core0006N(), core0007N(), core0008N(),
                                                        core0009N()};
  
            return tests;
        }
*/
        //------------------------ test case core-0001N ------------------------
        //
        // Testing feature - The items in the list are accessible via an integral
        //                   index starting from zero. (index equal 0)
        //
        // Testing approach - Create a list of all the children elements of the
        //                    third employee and access its first child by using
        //                    an index of 0.  This should result in "employeeId"
        //                    being selected.  Further we evaluate its content
        //                    (by examining its "nodeName" attribute) to ensure 
        //                    the proper element was accessed.
        //
        // Semantic Requirements: 1
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0001N()
        {
            string computedValue = "";
            string expectedValue = "employeeId";
            System.Xml.XmlNode employeeId = null;
            System.Xml.XmlElement testNode = null;

            testResults results = new testResults("Core0001N");
            try
            {
                results.description = "The elements in the list are accessible via an "+
                    "integral index starting from 0 (this test checks "+
                    "for index equal to 0).";
                //
                // Retrieve targeted data.
                //
                testNode = (System.Xml.XmlElement)util.nodeObject(util.THIRD,-1); 
                employeeId = util.getSubNodes(testNode).Item(util.FIRST); 
                computedValue = employeeId.Name;
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

        //------------------------ End test case core-0001N --------------------------
        //
        //--------------------------- test case core-0002N ---------------------------
        //
        // Testing feature - The items in the list are accessible via an integral
        //                   index starting from zero. (index not equal 0)
        //
        // Testing approach - Create a list of all the children elements of the
        //                    third employee and access its fourth child by 
        //                    using an index of 3.  This should result in "salary"
        //                    being selected.  Further we evaluate its "nodeName"
        //                    attribute to ensure the proper element was accessed.
        //
        // Semantic Requirements: 1
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0002N()
        {
            string computedValue = "";
            string expectedValue = "salary";
            System.Xml.XmlNode salary = null;
            System.Xml.XmlElement testNode = null;

            testResults results = new testResults("Core0002N");
            try
            {
                results.description = "The elements in the list are accessible via an "+
                    "integral index starting from 0 (this test checks "+
                    "for index not equal 0).";
                //
                //  Retrieve targeted data.
                //
                testNode = (System.Xml.XmlElement)util.nodeObject(util.THIRD,-1); 
                salary = util.getSubNodes(testNode).Item(util.FOURTH);
                computedValue = salary.Name;
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
        }

        //------------------------ End test case core-0002N --------------------------
        //
        //--------------------------- test case core-0003N ---------------------------
        //
        // Testing feature - The "item(index)" method returns the indexth item 
        //                   in the collection.
        //
        // Testing approach - Create a list of all the Element children of the
        //                    third employee and access its first child by invoking 
        //                    the "item(index)" method with index = 0.  This should 
        //                    cause the method to return the "employeeId" child.
        //                    Further we evaluate the returned item's "nodeName" 
        //                    attribute to ensure the correct item was returned. 
        //          
        // Semantic Requirements: 2 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0003N()
        {
            string computedValue = "";
            string expectedValue = "employeeId";
            System.Xml.XmlNode employeeId = null;
            System.Xml.XmlElement testNode = null;

            testResults results = new testResults("Core0003N");
            try
            {
                results.description = "The \"item(index)\" method returns the indexth "+
                    "item in the collection (return first item).";
                //
                // Retrieve targeted data.
                //
                testNode = (System.Xml.XmlElement)util.nodeObject(util.THIRD,-1);
                employeeId = util.getSubNodes(testNode).Item(util.FIRST);
                computedValue = employeeId.Name;
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

        //------------------------ End test case core-0003N --------------------------
        //
        //--------------------------- test case core-0004N ---------------------------
        //
        // Testing feature - The "item(index)" method returns the indexth item 
        //                   in the collection.
        //
        // Testing approach - Create a list of all the Element children of the
        //                    third employee and access its first child by invoking
        //                    the "item(index)" method with index equals to the last 
        //                    item in the list.  This should cause the method to 
        //                    return the "address" child.  Further we evaluate the
        //                    returned item's "nodeName" attribute to ensure the 
        //                    correct item was returned.
        //
        // Semantic Requirements: 2
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0004N()
        {
            string computedValue = "";
            string expectedValue = "address";
            System.Xml.XmlNode address = null;
            System.Xml.XmlElement testNode = null;

            testResults results = new testResults("Core0004N");
            try
            {
                results.description = "The \"item(index)\" method returns the indxth "+
                    "item in the collection (return last item).";
                //
                // Retrieve targeted data.
                //
                testNode = (System.Xml.XmlElement)util.nodeObject(util.THIRD,-1);
                address = util.getSubNodes(testNode).Item(util.SIXTH);
                computedValue = address.Name;
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

        //------------------------ End test case core-0004N --------------------------
        //
        //--------------------------- test case core-0005N ---------------------------
        //
        // Testing feature - If the index is greater than or equal to number of  
        //                   nodes, the "item(index)" method returns null.
        //
        // Testing approach - Create a list of all the Element children of the third
        //                    employee and then invoke its "item(index)" method with 
        //                    index equal to 6 (the number of nodes in the list).  This
        //                    should cause the method to return null.
        //
        // Semantic Requirements: 3 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0005N()
        {
            object computedValue = null;
            object expectedValue = null; 
            System.Xml.XmlElement testNode = null;

            testResults results = new testResults("Core0005N");
            try
            {
                results.description = "The \"item(index)\" method returns null if the "+
                    "index is greater than or equal to the number of "+
                    "nodes (index = number of nodes).";
                //
                // invoke the "item(index)" method with index equal to the number of nodes
                // in the list (6, count starts at zero).  It should return null.
                //
                testNode = (System.Xml.XmlElement)util.nodeObject(util.THIRD,-1);
                computedValue = util.getSubNodes(testNode).Item(util.SEVENTH);
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

        //------------------------ End test case core-0005N --------------------------
        //
        //--------------------------- test case core-0006N ---------------------------
        //
        // Testing feature - If the index is greater than or equal to number of
        //                   nodes, the "item(index)" method returns null.
        //
        // Testing approach - Create a list of all the Element children of the third
        //                    employee and then invoke the "item(index)" with index
        //                    equal to 7 (index is greater than number of nodes).  
        //                    This should cause the method to return null.
        //
        // Semantic Requirements: 3
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0006N()
        {
            object computedValue = null;
            object expectedValue = null;
            System.Xml.XmlElement testNode = null;

            testResults results = new testResults("Core0006N");
            try
            {
                results.description = "The \"item(index)\" method returns null if the "+ 
                    "index is greater than or equal to the number of "+
                    "nodes (index > number of nodes).";
                //
                // Retrieve targeted data.  All counts start from zero
                //
                testNode = (System.Xml.XmlElement)util.nodeObject(util.THIRD,-1);
                computedValue = util.getSubNodes(testNode).Item(util.EIGHT);
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

        //------------------------ End test case core-0006N --------------------------
        //
        //--------------------------- test case core-0007N ---------------------------
        //
        // Testing feature - The "length" attribute contains the number of items in
        //                   the list.
        //
        // Testing approach - Create a list of all the Element children of the third
        //                    employee and then access the "length" attribute.
        //                    It should contain the value 6.
        //
        // Semantic Requirements: 4 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0007N()
        {
            string computedValue = "0";
            string expectedValue = "6";
            //System.Xml.XmlNodeList thirdEmployeeList = null;

            testResults results = new testResults("Core0007N");
            try
            {
                results.description = "The \"length\" attribute contains the number of "+
                    "nodes in the list (non empty list).";
                //
                // retrieve the targeted data and access the "length" attribute. 
                //
                System.Xml.XmlElement testNode = (System.Xml.XmlElement)util.nodeObject(util.THIRD,-1);
                computedValue = util.getSubNodes(testNode).Count.ToString();
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

        //------------------------ End test case core-0007N --------------------------
        //
        //--------------------------- test case core-0008N ---------------------------
        //
        // Testing feature - The "length" attribute contains the number of items in
        //                   the list (test for empty list).
        //
        // Testing approach - Create a list of all the children of the Text node 
        //                    inside the first child o the third employee and
        //                    then access its "length" attribute.  It should 
        //                    contain the value 0.
        //
        // Semantic Requirements: 4
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0008N()
        {
            string computedValue = "0";
            string expectedValue = "0";
            System.Xml.XmlNode testNode = null;
            System.Xml.XmlNode textNode = null;

            testResults results = new testResults("Core0008N");
            try
            {
                results.description = "The \"length\" attribute contains the number of "+
                    "nodes in the list (test for empty list).";
                //
                // Access the targeted data and examine the "length" attribute of an
                // empty list.
                //
                testNode = util.nodeObject(util.THIRD,util.FIRST);
                textNode = testNode.FirstChild;
                computedValue = textNode.ChildNodes.Count.ToString();
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

        //------------------------ End test case core-0008N --------------------------
        //
        //--------------------------- test case core-0009 ---------------------------
        //
        // Testing feature - The range of valid child nodes indices is 0 to length - 1.
        // 
        // Testing approach - Create a list of all the Element children of the 
        //                    third employee and traverse the list from index
        //                    0 to index length - 1.
        //
        // Semantic Requirements: 5 
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0009N()
        {
            string computedValue = "";
            string expectedValue = "employeeId name position salary gender address ";
            int lastIndex = 0;
            int listLength = 0;
            System.Xml.XmlElement testNode = null;

            testResults results = new testResults("Core0009N");
            try
            {
                results.description = "The range of valid child nodes indices is 0 to "+
                    "length - 1.";
                //
                // Retrieve the targeted data and determine the length of the list.
                //
                testNode =  (System.Xml.XmlElement)util.nodeObject(util.THIRD,-1);
                listLength = util.getSubNodes(testNode).Count; 
                lastIndex = listLength - 1;
                //
                // Traverse the list from 0 to length - 1.  All indices should be valid.
                //
                for (int index = 0; index <= lastIndex; index++)
                    computedValue += util.getSubNodes(testNode).Item(index).Name+" ";
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

        //------------------------ End test case core-0009N --------------------------
    }
}