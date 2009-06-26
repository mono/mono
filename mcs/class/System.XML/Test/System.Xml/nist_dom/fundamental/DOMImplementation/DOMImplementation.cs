//**************************************************************************
//
//
//                       National Institute Of Standards and Technology
//                                     DTS Version 1.0
//
//                              DOMImplementation Interface
//
// Written by: Carmelo Montanez
// Modified by:  Mary Brady
//
// Ported to System.Xml by: Mizrahi Rafael rafim@mainsoft.com
// Mainsoft Corporation (c) 2003-2004
//**************************************************************************
using System;
using nist_dom;
using NUnit.Framework;

namespace nist_dom.fundamental
{
    /// <summary>
    /// Summary description for Comment.
    /// </summary>
    [TestFixture]
    public class DOMImplementationTest
    {
        public static int i = 2;
/*
        public testResults[] RunTests()
        {
            testResults[] tests = new testResults[] {core0001DI(), core0002DI(), core0003DI(),
                                                        core0004DI(), core0005DI()};
            return tests;
        }
*/
        //------------------------ test case core-0001DI ------------------------
        //
        // Testing feature - The "feature" parameter in the 
        //                   "hasFeature(feature,version)" method is the package 
        //                   name of the feature.  Legal values are HTML and XML.
        //                   (test for XML, upper case)
        //
        // Testing approach - Retrieve the entire DOM document and invoke its
        //                    "implementation" attribute.  This should create
        //                    a DOMImplementation object whose "hasFeature(feature,
        //                    version)" method is invoked with feature = "XML".  The
        //                    method should return a true value. 
        //
        // Semantic Requirements: 1, 2, 4
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0001DI()
        {
            bool computedValue = false;
            bool expectedValue = true;
            System.Xml.XmlDocument testNode = null;

            testResults results = new testResults("Core0001DI");

            results.description = "Check for feature = XML in the \"hasFeature(feature,version)\" method."; 
            //
            // Retrieve the targeted data and invoke its "hasFeature(feature,version)".
            // method.
            //
            testNode = util.getDOMDocument();
            computedValue = testNode.Implementation.HasFeature("XML","1.0");
            //
            // Write out results.
            //

            results.expected = expectedValue.ToString();
            results.actual = computedValue.ToString();

            Assert.AreEqual (results.expected, results.actual);
        }
        //------------------------ End test case core-0001DI --------------------------
        //
        //------------------------ test case core-0002DI ------------------------
        //
        // Testing feature - The "feature" parameter in the
        //                   "hasFeature(feature,version)" method is the package
        //                   name of the feature.  Legal values are HTML and XML.
        //                   (test for XML, lower case)
        //
        // Testing approach - Retrieve the entire DOM document and invoke its
        //                    "implementation" attribute.  This should create
        //                    a DOMImplementation object whose "hasFeature(feature,
        //                    version)" method is invoked with feature = "xml".  The
        //                    method should return a true value.
        //
        // Semantic Requirements: 1, 2, 4
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0002DI()
        {
            bool computedValue = false;
            bool expectedValue = true;
            System.Xml.XmlDocument testNode = null;

            testResults results = new testResults("Core0002DI");

            results.description = "Check for feature = xml in the \"hasFeature(feature,version)\" method."; 
            //
            // Retrieve the targeted data and invoke its "hasFeature(feature,version)".
            // method.
            //
            testNode = util.getDOMDocument();
            computedValue = testNode.Implementation.HasFeature("xml","1.0");
            //
            // Write out results.
            //

            results.expected = expectedValue.ToString();
            results.actual = computedValue.ToString();

            Assert.AreEqual (results.expected, results.actual);
        }
        //------------------------ End test case core-0002DI --------------------------
        //
        //------------------------ test case core-0003DI ------------------------
        //
        // Testing feature - The "feature" parameter in the
        //                   "hasFeature(feature,version)" method is the package
        //                   name of the feature.  Legal values are HTML and XML.
        //                   (test for HTML, upper case)
        //
        // Testing approach - Retrieve the entire DOM document and invoke its
        //                    "implementation" attribute.  This should create
        //                    a DOMImplementation object whose "hasFeature(feature,
        //                    version)" method is invoked with feature = "HTML".  The
        //                    method should return a true or false value.  Since this
        //                    is the XML section of the specs, either value for the
        //                    HTML feature will be acceptable.
        //
        // Semantic Requirements: 1, 2, 4, 5
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0003DI()
        {
            bool computedValue = false;
            bool expectedValue = false;//(true, false);
            System.Xml.XmlDocument testNode = null;

            testResults results = new testResults("Core0003DI");

            results.description = "Check for feature = HTML in the \"hasFeature(feature,version)\" method."; 
            //
            // Retrieve the targeted data and invoke its "hasFeature(feature,version)".
            // method.
            //
            testNode = util.getDOMDocument();
            computedValue = testNode.Implementation.HasFeature("HTML","1.0");
            //
            // Write out results.
            //

            results.expected = expectedValue.ToString();
            results.actual = computedValue.ToString();

            Assert.AreEqual (results.expected, results.actual);
        }
        //------------------------ End test case core-0003DI --------------------------
        //
        //------------------------ test case core-0004DI ------------------------
        //
        // Testing feature - The "feature" parameter in the
        //                   "hasFeature(feature,version)" method is the package
        //                   name of the feature.  Legal values are HTML and XML.
        //                   (test for HTML, lower case)
        //
        // Testing approach - Retrieve the entire DOM document and invoke its
        //                    "implementation" attribute.  This should create
        //                    a DOMImplementation object whose "hasFeature(feature,
        //                    version)" method is invoked with feature = "html".  The
        //                    method should return a true or false value.  Since this
        //                    is the XML section of the specs, either value for the
        //                    HTML feature will be acceptable.
        //
        // Semantic Requirements: 1, 2, 4, 5
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0004DI()
        {
            bool computedValue = false;
            bool expectedValue = false;//(true, false);
            System.Xml.XmlDocument testNode = null;

            testResults results = new testResults("Core0004DI");

            results.description = "Check for feature = html in the \"hasFeature(feature,version)\" method."; 
            //
            // Retrieve the targeted data and invoke its "hasFeature(feature,version)".
            // method.
            //
            testNode = util.getDOMDocument();
            computedValue = testNode.Implementation.HasFeature("html","1.0");
            //
            // Write out results.
            //
            results.expected = expectedValue.ToString();
            results.actual = computedValue.ToString();

            Assert.AreEqual (results.expected, results.actual);
        }
        //------------------------ End test case core-0004DI --------------------------
        //
        //------------------------ test case core-0005DI ------------------------
        //
        // Testing feature - if the The "version" parameter is not specified in the 
        //                   "hasFeature(feature,version)" method then supporting
        //                   any version of the feature will cause the method to 
        //                   return true.
        //
        // Testing approach - Retrieve the entire DOM document and invoke its
        //                    "implementation" attribute.  This should create
        //                    a DOMImplementation object whose "hasFeature(feature,
        //                    version)" method is invoked with version = "".  The
        //                    method should return a true value for any supported 
        //                    version of the feature.
        //
        // Semantic Requirements: 3
        //
        //----------------------------------------------------------------------------

        [Test]
	public void core0005DI()
        {
            bool computedValue = false;
            bool expectedValue = true;
            string NullString = null;
            System.Xml.XmlDocument testNode = null;

            testResults results = new testResults("Core0005DI");

            results.description = "Check for version not specified in the " +
                "\"hasFeature(feature,version)\" method."; 
            //
            // Retrieve the targeted data and invoke its "hasFeature(feature,version)".
            // method.
            //
            testNode = util.getDOMDocument();
            computedValue = testNode.Implementation.HasFeature("XML", NullString);
            //
            // Write out results.
            //

            results.expected = expectedValue.ToString();
            results.actual = computedValue.ToString();

            Assert.AreEqual (results.expected, results.actual);
        }
        //------------------------ End test case core-0005DI --------------------------
    }

}
