//**************************************************************************
//
//
//                       National Institute Of Standards and Technology
//                                     DTS Version 1.0
//         
//                                      Comment Interface
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
    /// <summary>
    /// Summary description for Comment.
    /// </summary>
    [TestFixture]
    public class CommentTest//,ITest
    {
        public static int i = 1;
/*
        public testResults[] RunTests()
        {
            testResults[] tests = new testResults[] {core0001CO()};
  
            return tests;
        }
*/
        //------------------------ test case core-0001CO ------------------------
        //
        // Testing feature - A comment is all the characters between the starting 
        //                   "<!--" and ending "-->" strings. 
        //
        // Testing approach - Retrieve the third child in the DOM document.  
        //                    This node is a comment node and its value is the 
        //                    content of the node.
        //
        // Semantic Requirements: 1
        //
        //----------------------------------------------------------------------------

	[Test]
        public void core0001CO()
        {
            string computedValue = "";
            string expectedValue = " This is comment number 1.";
            System.Xml.XmlNode testNode = null;

            testResults results = new testResults("Core0001CO");

            results.description = "A comment is all the characters between the " + "start comment and end comment strings.";
            //
            // Retrieve the targeted data and access its nodeValue.
            //
            testNode = util.getDOMDocument().ChildNodes.Item(2); 
            computedValue = testNode.Value;//nodeValue;
            //
            // Write out results 
            //

            results.expected = expectedValue;
            results.actual = computedValue;

            Assert.AreEqual (results.expected, results.actual);
        }

        //------------------------ End test case core-0001CO --------------------------
    }

}
