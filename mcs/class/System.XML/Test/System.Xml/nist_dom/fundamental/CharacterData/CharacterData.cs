//**************************************************************************
//
//
//                       National Institute Of Standards and Technology
//                                     DTS Version 1.0
//         
//                                 CharacterData Interface
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
	public class CharacterDataTest//,ITest
	{
		public static int i = 2;
/*
		public testResults[] RunTests()
		{
			testResults[] tests = new testResults[] {core0001C(), core0002C(), core0003C(),core0004C(),
														core0005C(), core0006C(), core0007C(), core0008C(),
														core0009C(), core0010C(), core0011C(), core0012C(),
														core0013C(), core0014C(), core0015C(),
														core0016C(), core0017C(), core0018C(), core0019C(),
														core0020C(), core0021C(), core0022C(), core0023C(),
														core0024C(), core0025C(), core0026C(), core0027C(),
														core0028C(), core0029C(), core0030C(), core0031C(),
														core0032C(), core0033C(), core0034C(), core0035C(),
														core0036C()};
  
			return tests;
		}
*/
		//------------------------ test case core-0001C ------------------------
		//
		// Testing feature -  The "data" attribute is the character data that
		//                    implements this interface.
		//
		// Testing approach - Retrieve the character data from the second child of
		//                    the first employee and invoke its "data" attribute.  The 
		//                    attribute should return the actual data.
		//                    
		// Semantic Requirements: 1
		//
		//----------------------------------------------------------------------------

		[Test]
		public void core0001C()
		{
			string computedValue = "";
			string expectedValue = "Margaret Martin";
			System.Xml.XmlNode testNode = null;
			System.Xml.XmlCharacterData testNodeData = null;

			testResults results = new testResults("Core0001C");
			try
			{
				results.description = "The \"data\" attribute is the character data that " +
					"implements this interface.";
				//
				// Access the targeted data.
				//
				testNode = util.nodeObject(util.FIRST,util.SECOND);
				testNodeData = (System.Xml.XmlCharacterData)testNode.FirstChild; 
				computedValue = testNodeData.Data;
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

		//------------------------ End test case core-0001C --------------------------
		//
		//--------------------------- test case core-0002C ---------------------------
		//
		// Testing feature -  The "length" attribute contains the number of 16-bit 
		//                    units that are available through the data attribute and
		//                    the substringData method.  Test for the "data" attribute.
		//
		// Testing approach - Retrieve the character data from the second child of
		//                    the first employee and access its data by using the
		//                    "data" attribute.  Finally the "length" attribute
		//                    is used on the character data returned by the "data"
		//                    attribute to determine the number of 16 bit units in
		//                    the data.
		//
		// Semantic Requirements: 2 
		//
		//----------------------------------------------------------------------------

		[Test]
		public void core0002C()
		{
			string computedValue = "";
			string expectedValue = "15";
			System.Xml.XmlNode testNode = null;
			System.Xml.XmlCharacterData testNodeData = null;

			testResults results = new testResults("Core0002C");
			try
			{
				results.description = "The \"length\" attribute is the number of 16-bit units " +
					"that are available through the \"data\" attribute " +
					"and the \"substringData\" method (test for \"data\").";
				//
				// Retrieve the targeted data and invoke its "data" attribute.
				//
				testNode = util.nodeObject(util.FIRST,util.SECOND);
				testNodeData = (System.Xml.XmlCharacterData)testNode.FirstChild;
				computedValue = testNodeData.Length.ToString();
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

		//------------------------ End test case core-0002C --------------------------
		//
		//--------------------------- test case core-0003C ---------------------------
		//
		// Testing feature -  The "length" attribute contains the number of 16-bit units 
		//                    that are available through the data attribute and the
		//                    substringData method.  Test for the "substringData"
		//                    method.
		//
		// Testing approach - Retrieve the character data of the second child of the
		//                    first employee and access part of the data by using the 
		//                    "substringData(offset,count)" method.  Finally the 
		//                    "length" attribute is used on the character data 
		//                    returned by the "substringData(offset,count)" method 
		//                    to determine the number of 16-bit units in the data.
		//
		// Semantic Requirements: 2
		//
		//----------------------------------------------------------------------------

		[Test]
		public void core0003C()
		{
			string computedValue = "";
			string expectedValue = "8";
			System.Xml.XmlNode testNode = null;
			System.Xml.XmlCharacterData testNodeData = null;
			string subString = "";

			testResults results = new testResults("Core0003C");
			try
			{
				results.description = "The \"length\" attribute is the number of 16-bit units " +
					"that are available through the \"data\" attribute " +
					"and the \"substringData\" method (test for \"substringData\").";
				//
				// Retrieve the targeted data and invoke its "substringData" method.
				//
				testNode = util.nodeObject(util.FIRST,util.SECOND);
				testNodeData = (System.Xml.XmlCharacterData)testNode.FirstChild;
				subString = testNodeData.Substring(0,8);
				computedValue = subString.Length.ToString();
			}
			catch(System.Exception ex)
			{
				computedValue = "Exception " + ex.Message;
			}
			//
			// write out results
			//
			results.expected = expectedValue;
			results.actual = computedValue;

			Assert.AreEqual (results.expected, results.actual);
		}

		//------------------------ End test case core-0003C --------------------------
		//
		//--------------------------- test case core-0004C ---------------------------
		//
		// Testing feature -  The "substringData(offset,count)" method returns the
		//                    specified substring.
		//
		// Testing approach - Retrieve the character data from the second child of the
		//                    first employee and access part of the data by using the
		//                    "substringData(offset,count)" method.  The method should
		//                    return the specified substring starting at position
		//                    "offset" and extract "count" characters.  The method
		//                    method should return the string "Margaret".
		//
		// Semantic Requirements: 3 
		//
		//----------------------------------------------------------------------------

		[Test]
		public void core0004C()
		{
			//string testName = "core-0004C";
			string computedValue = "";
			string expectedValue = "Margaret";
			System.Xml.XmlNode testNode = null;
			System.Xml.XmlCharacterData testNodeData = null;

			testResults results = new testResults("Core0004C");
			try
			{
				results.description = "The \"substringData(offset,count)\" method returns the " +
					"specified substring.";
				//
				// Retrieve the targeted data and invoke its "substringData" method.
				//
				testNode = util.nodeObject(util.FIRST,util.SECOND);
				testNodeData = (System.Xml.XmlCharacterData)testNode.FirstChild;
				computedValue = testNodeData.Substring(0,8);
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

		//------------------------ End test case core-0004C --------------------------
		//
		//--------------------------- test case core-0005C ---------------------------
		//
		// Testing feature -  If the sum of "offset" and "count" exceeds "length" then 
		//                    the substringData(offset,count) method returns all 
		//                    the 16-bit units to the end of the data. 
		//
		// Testing approach - Retrieve the character data from the second child of the
		//                    first employee and access part of the data by using the
		//                    "substringData(offset,count)" method with offset = 9 and
		//                    count = 10.  The method should return the substring
		//                    "Martin" since offset + count > length (19 > 15).
		//
		// Semantic Requirements: 4 
		//
		//----------------------------------------------------------------------------

		[Test]
		public void core0005C()
		{
			string computedValue = "";
			string expectedValue = "Martin";
			System.Xml.XmlNode testNode = null;
			System.Xml.XmlCharacterData testNodeData = null;

			testResults results = new testResults("Core0005C");
			try
			{
				results.description = "If the sum of \"offset\" and \"count\" exceeds " +
					"the value of the \"length\" attribute then the " +
					"\"substringData(offset,count)\" method returns all " +
					"the 16-bit units to the end of the data.";
				//
				// Retrieve the targeted data and invoke its "substringData" method.
				//
				testNode = util.nodeObject(util.FIRST,util.SECOND);
				testNodeData = (System.Xml.XmlCharacterData)testNode.FirstChild;
				computedValue = testNodeData.Substring(9,10);
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

		//------------------------ End test case core-0005C --------------------------
		//
		//--------------------------- test case core-0006C ---------------------------
		//
		// Testing feature - The "appendData(arg)" method appends a string to the end
		//                   of the character data of the node.
		//
		// Testing approach - Retrieve the character data from the second child of the
		//                    first employee.  The "appendData(arg)" method is then 
		//                    called with arg = ", Esquire".  The method should append 
		//                    the specified data to the already existing character 
		//                    data.  The new value of the "length" attribute should 
		//                    be 24.
		//
		// Semantic Requirements: 5 
		//
		//----------------------------------------------------------------------------

		[Test]
		public void core0006C()
		{
			string computedValue = "";
			string expectedValue = "24";
			System.Xml.XmlNode testNode = null;
			System.Xml.XmlCharacterData testNodeData = null;

			testResults results = new testResults("Core0006C");
			try
			{
				results.description = "The \"appendData(arg)\" method appends the specified " +
					"string to the end of the character data of the node.";
				//
				// Retrieve targeted data and invoke the "appendData" method.
				//
				testNode = util.nodeObject(util.FIRST,util.SECOND); 
				testNodeData = (System.Xml.XmlCharacterData)testNode.FirstChild;
				testNodeData.AppendData(", Esquire");;
				computedValue = testNodeData.Length.ToString();
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

		//------------------------ End test case core-0006C --------------------------
		//
		//--------------------------- test case core-0007C ---------------------------
		//
		// Testing feature - Upon successful invocation of the "appendData(arg)" 
		//                   method, the "data" attribute provides access to the
		//                   concatenation of data and the specified DOMString.
		//
		// Testing approach - Retrieve the character data from the second child of 
		//                    the first employee.  The "appendData(arg)" method is 
		//                    then called with arg = ", Esquire".  The method should 
		//                    append the specified data to the already existing 
		//                    character data.  The new value of the "data" attribute 
		//                    should be "Margaret Martin, Esquire".
		//
		// Semantic Requirements: 6 
		//
		//----------------------------------------------------------------------------

		[Test]
		public void core0007C()
		{
			string computedValue = "";
			string expectedValue = "Margaret Martin, Esquire";
			System.Xml.XmlNode testNode = null;
			System.Xml.XmlCharacterData testNodeData = null;

			testResults results = new testResults("Core0007C");
			try
			{
				results.description = "Upon successful invocation of the \"appendData(arg)\" " +
					"method ,the \"data\" attribute provides access " +
					"to the concatenation of \"data\" and the specified DOMString.";
				//
				// Retrieve targeted data and invoke its "appendData" method.
				//
				testNode = util.nodeObject(util.FIRST,util.SECOND);
				testNodeData = (System.Xml.XmlCharacterData)testNode.FirstChild;
				testNodeData.AppendData(", Esquire");
				computedValue = testNodeData.Data;
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

		//------------------------ End test case core-0007C --------------------------
		//
		//--------------------------- test case core-0008C ---------------------------
		//
		// Testing feature - The "insertData(offset,arg)" method insert a string at the
		//                   specified 16-bit unit offset.  Insert at the beginning of
		//                   the character data.
		//
		// Testing approach - Retrieve the character data from the second child of 
		//                    the first employee.  The "insertData(offset,arg)" 
		//                    method is then called with offset = 0 and arg = "Mss.".
		//                    The method should insert the string "Mss." at position 
		//                    0.  The new value of the character data should be "Mss.
		//                    Margaret Martin".
		//
		// Semantic Requirements: 7 
		//
		//----------------------------------------------------------------------------

		[Test]
		public void core0008C()
		{
			string computedValue = "";
			string expectedValue = "Mss. Margaret Martin";
			System.Xml.XmlNode testNode = null;
			System.Xml.XmlCharacterData testNodeData = null;

			testResults results = new testResults("Core0008C");
			try
			{
				results.description = "Insert a string at the beginning of character data.";
				//
				// Retrieve the targeted data and invoke its "insertData" method.
				//
				testNode = util.nodeObject(util.FIRST,util.SECOND);
				testNodeData = (System.Xml.XmlCharacterData)testNode.FirstChild;
				testNodeData.InsertData(0,"Mss. ");
				computedValue = testNodeData.Data;
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

			util.resetData();

			Assert.AreEqual (results.expected, results.actual);
		}

		//------------------------ End test case core-0008C --------------------------
		//
		//--------------------------- test case core-0009C ---------------------------
		//
		// Testing feature - The "insertData(offset,arg)" method insert a string at the
		//                   specified 16-bit units offset.  Insert in the middle of
		//                   the character data.
		//
		// Testing approach - Retrieve the character data from the second child of the
		//                    first employee.  The "insertData(offset,arg)" method is 
		//                    then called with offset = 9 and arg = "Ann".  The 
		//                    method should insert the string "Ann" at position 9.  
		//                    The new value of the character data should be 
		//                    "Margaret Ann Martin".
		//
		// Semantic Requirements: 7
		//
		//----------------------------------------------------------------------------

		[Test]
		public void core0009C()
		{
			string computedValue = "";//0;
			string expectedValue = "Margaret Ann Martin";
			System.Xml.XmlNode testNode = null;
			System.Xml.XmlCharacterData testNodeData = null;

			testResults results = new testResults("Core0009C");
			try
			{
				results.description = "Insert a character string in the middle of character data."; 
				//
				// Retrieve targeted data and invoke its "insertData" method.
				//
				testNode = util.nodeObject(util.FIRST,util.SECOND);
				testNodeData = (System.Xml.XmlCharacterData)testNode.FirstChild; 
				testNodeData.InsertData(9,"Ann ");
				computedValue = testNodeData.Data;
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

			util.resetData();

			Assert.AreEqual (results.expected, results.actual);
		}

		//------------------------ End test case core-0009C --------------------------
		//
		//--------------------------- test case core-0010C ---------------------------
		//
		// Testing feature - The "insertData(offset,arg)" method insert a string at the
		//                   specified 16-bit units offset.  Insert at the end of the
		//                   character data.
		//
		// Testing approach - Retrieve the character data from the second child of the
		//                    first employee.  The "insertData(offset,arg)" method 
		//                    is then called with offset = 14 and arg = ", Esquire".  
		//                    The method should insert the string ", Esquire" at 
		//                    position 14.  The new value of the character data 
		//                    should be "Margaret Martin, Esquire"
		//
		// Semantic Requirements: 7
		//
		//----------------------------------------------------------------------------

		[Test]
		public void core0010C()
		{
			string computedValue = "";
			string expectedValue = "Margaret Martin, Esquire";
			System.Xml.XmlNode testNode = null;
			System.Xml.XmlCharacterData testNodeData = null;

			testResults results = new testResults("Core0010C");
			try
			{
				results.description = "Insert a string at the end of character data.";
				//
				// Retrieve the targeted data and invoke its "insertData" method.
				//
				testNode = util.nodeObject(util.FIRST,util.SECOND);
				testNodeData = (System.Xml.XmlCharacterData)testNode.FirstChild; 
				testNodeData.InsertData(15,", Esquire");
				computedValue = testNodeData.Data;
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

		//------------------------ End test case core-0010C --------------------------
		//
		//--------------------------- test case core-0011C ---------------------------
		//
		// Testing feature - The "deleteData(offset,count)" method removes a range of
		//                   16-bit units from the node.  Delete at the beginning of the
		//                   character data.
		//
		// Testing approach - Retrieve the character data from the last child of the
		//                    first employee.  The "deleteData(offset,count)" 
		//                    method is then called with offset = 0 and count = 16.  
		//                    The method should delete the characters from position 0 
		//                    thru position 16.  The new value of the character data 
		//                    should be "Dallas, Texas 98551".
		//
		// Semantic Requirements: 8, 9 
		//
		//----------------------------------------------------------------------------

		[Test]
		public void core0011C()
		{
			string computedValue = "";
			string expectedValue = "Dallas, Texas 98551";
			System.Xml.XmlNode testNode = null;
			System.Xml.XmlCharacterData testNodeData = null;

			testResults results = new testResults("Core0011C");
			try
			{
				results.description = "Delete character string from beginning of character data.";
				//
				// Retrieve the targeted data and invoke its "deleteData" method..
				//
				testNode = util.nodeObject(util.FIRST,util.SIXTH);
				testNodeData = (System.Xml.XmlCharacterData)testNode.FirstChild;
				testNodeData.DeleteData(0,16);
				computedValue = testNodeData.Data;
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

		//------------------------ End test case core-0011C --------------------------
		//
		//--------------------------- test case core-0012C ---------------------------
		//
		// Testing feature - The "deleteData(offset,count)" method removes a range of
		//                   16-bit units from the node.  Delete in the middle of
		//                   the character data.
		//
		// Testing approach - Retrieve the character data from the last child of 
		//                    the first employee.  The "deleteData(offset,count)"
		//                    method is then called with offset = 16 and count = 8.
		//                    The method should delete the characters from position 16
		//                    thru position 24.  The new value of the character data
		//                    should be "1230 North Ave. Texas 98551".
		//
		// Semantic Requirements: 8, 9
		//
		//----------------------------------------------------------------------------

		[Test]
		public void core0012C()
		{
			string computedValue = "";
			string expectedValue = "1230 North Ave. Texas 98551";
			System.Xml.XmlNode testNode = null;
			System.Xml.XmlCharacterData testNodeData = null;

			testResults results = new testResults("Core0012C");
			try
			{
				results.description = "Delete character string from the middle of character data.";
				//
				// Retrieve the targeted data and invoke its "deleteData" method.
				//
				testNode = util.nodeObject(util.FIRST,util.SIXTH);
				testNodeData = (System.Xml.XmlCharacterData)testNode.FirstChild;
				testNodeData.DeleteData(16,8);
				computedValue = testNodeData.Data;
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

		//------------------------ End test case core-0012C --------------------------
		//
		//--------------------------- test case core-0013C ---------------------------
		//
		// Testing feature - The "deleteData(offset,count)" method removes a range of
		//                   16-bit units from the node.  Delete at the end of the
		//                   character data.
		//
		// Testing approach - Retrieve the character data from the last child of 
		//                    the first employee.  The "deleteData(offset,count)"
		//                    method is then called with offset = 30 and count = 5.
		//                    The method should delete the characters from position 30 
		//                    thru position 35.  The new value of the character data
		//                    should be "1230 North Ave. Dallas, Texas".
		//
		// Semantic Requirements: 8, 9
		//
		//----------------------------------------------------------------------------

		[Test]
		public void core0013C()
		{
			string computedValue = "";
			string expectedValue = "1230 North Ave. Dallas, Texas ";
			System.Xml.XmlNode testNode = null;
			System.Xml.XmlCharacterData testNodeData = null;

			testResults results = new testResults("Core0013C");
			try
			{
				results.description = "Delete character string from the end of character data.";
				//
				// Retrieve the targeted data and invoke its "deleteData" method.
				//
				testNode = util.nodeObject(util.FIRST,util.SIXTH);
				testNodeData = (System.Xml.XmlCharacterData)testNode.FirstChild;
				testNodeData.DeleteData(30,5);
				computedValue = testNodeData.Data;
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

		//------------------------ End test case core-0013C --------------------------
		//
		//--------------------------- test case core-0014C ---------------------------
		//
		// Testing feature - Upon successful invocation of the 
		//                   "deleteData(offset,count)" method, the data and length 
		//                   attributes reflect that change.
		//
		// Testing approach - Retrieve the character data of the last child of the
		//                    first employee.  The "deleteData(offset,count)"
		//                    method is then called with offset = 30 and count = 5.
		//                    The method should delete the characters from position 30 
		//                    thru position 35.  The new value of the character data
		//                    should be "1230 North Ave. Dallas, Texas" (the data
		//                    attribute) and its length attribute should be 30.  This
		//                    new values should be reflected immediately upon
		//                    its invocation. 
		//
		// Semantic Requirements: 9
		//
		//----------------------------------------------------------------------------

		[Test]
		public void core0014C()
		{
			string computedValue = "";
			string expectedValue = "1230 North Ave. Dallas, Texas 30";
			System.Xml.XmlNode testNode = null;
			System.Xml.XmlCharacterData testNodeData = null;

			testResults results = new testResults("Core0014C");
			try
			{
				results.description = "Data and length attributes are updated as a result " +
					"of the \"deleteData(offset, count)\" method.";
				//
				// Retrieve the targeted data and invoke its "deleteData" attribute.
				//
				testNode = util.nodeObject(util.FIRST,util.SIXTH);
				testNodeData = (System.Xml.XmlCharacterData)testNode.FirstChild;
				testNodeData.DeleteData(30,5);
				computedValue += testNodeData.Data;
				computedValue += testNodeData.Length;
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

		//------------------------ End test case core-0014C --------------------------
		//
		//--------------------------- test case core-0015C ---------------------------
		//
		// Testing feature - If the sum of the offset and count attributes (from the
		//                   deleteData method) is greater than the length of the 
		//                   character data then all the 16-bit units from the offset 
		//                   to the end of the data are deleted.
		//
		// Testing approach - Retrieve the character data from the last child of the
		//                    first employee.  The "deleteData(offset,count)"
		//                    method is then called with offset = 4 and count = 50.
		//                    The method should delete the characters from position 4
		//                    to the end of the data since offset + count (50+4) is
		//                    greater than the length of the character data (35).  
		//                    The new value for the character data should be "1230".
		//
		// Semantic Requirements: 10 
		//
		//----------------------------------------------------------------------------

		[Test]
		public void core0015C()
		{
			string computedValue = "";
			string expectedValue = "1230";
			System.Xml.XmlNode testNode = null;
			System.Xml.XmlCharacterData testNodeData = null;

			testResults results = new testResults("Core0015C");
			try
			{
				results.description = "If the sum of \"offset\" and \"count\" exceeds the " +
					"length of the character data then all the " +
					"16-bit units from the offset thru the end of the " +
					"data are removed.<br>";
				//
				// Retrieve the targeted data and invoke its "deleteData" method.
				//
				testNode = util.nodeObject(util.FIRST,util.SIXTH);
				testNodeData = (System.Xml.XmlCharacterData)testNode.FirstChild;
				testNodeData.DeleteData(4,50);
				computedValue = testNodeData.Data;
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

		//------------------------ End test case core-0015C --------------------------
		//
		//--------------------------- test case core-0016C ---------------------------
		//
		// Testing feature - The "replaceData(offset,count,arg)" method replace the 
		//                   characters starting at the specified 16-bit units with the
		//                   specified string.  Test for replacement at the 
		//                   beginning of the data.
		//
		// Testing approach - Retrieve the character data from the last child of the
		//                    first employee.  The "replaceData(offset,count,arg)"
		//                    method is then called with offset = 0 and count = 4 and
		//                    arg = "2500".  The method should replace the first four
		//                    characters of the character data with "2500".
		//
		// Semantic Requirements: 11 
		//
		//----------------------------------------------------------------------------

		[Test]
		public void core0016C()
		{
			string computedValue = "";//0;
			string expectedValue = "2500 North Ave. Dallas, Texas 98551";
			System.Xml.XmlNode testNode = null;
			System.Xml.XmlCharacterData testNodeData = null;

			testResults results = new testResults("Core0016C");
			try
			{
				results.description = "Replace a character string at the beginning of character data.";
				//
				// Retrieve the targeted data and invoke its "replaceData" method .
				//
				testNode = util.nodeObject(util.FIRST,util.SIXTH);
				testNodeData = (System.Xml.XmlCharacterData)testNode.FirstChild;
				testNodeData.ReplaceData(0,4,"2500");
				computedValue = testNodeData.Data;
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

		//------------------------ End test case core-0016C --------------------------
		//
		//--------------------------- test case core-0017C ---------------------------
		// Testing feature - The "replaceData(offset,count,arg)" method replace the
		//                   characters starting at the specified 16-bit units with the
		//                   specified string.  Test for replacement in the
		//                   middle of the data.
		//
		// Testing approach - Retrieve the character data from the last child of the
		//                    first employee.  The "replaceData(offset,count,arg)"
		//                    method is then called with offset = 5 and count = 5 and
		//                    arg = "South".  The method should replace characters
		//                    five thru nine of the character data with "South".
		//
		// Semantic Requirements: 11
		//
		//----------------------------------------------------------------------------

		[Test]
		public void core0017C()
		{
			string computedValue = "";
			string expectedValue = "1230 South Ave. Dallas, Texas 98551";
			System.Xml.XmlNode testNode = null;
			System.Xml.XmlCharacterData testNodeData = null;

			testResults results = new testResults("Core0017C");
			try
			{
				results.description = "Replace a character string in the middle of character data.";
				//
				// Retrieve the targeted data and invoke its "replaceData" method.
				//
				testNode = util.nodeObject(util.FIRST,util.SIXTH);
				testNodeData = (System.Xml.XmlCharacterData)testNode.FirstChild;
				testNodeData.ReplaceData(5,5,"South");
				computedValue = testNodeData.Data;
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

		//------------------------ End test case core-0017C --------------------------
		//
		//--------------------------- test case core-0018C ---------------------------
		//
		// Testing feature - The "replaceData(offset,count,arg)" method replace the
		//                   characters starting at the specified 16-bit units with the
		//                   specified string.  Test for replacement at the
		//                   end of the data.
		//
		// Testing approach - Retrieve the character data from the last child of the
		//                    first employee.  The "replaceData(offset,count,arg)"
		//                    method is then called with offset = 30 and count = 5 and
		//                    arg = "98665".  The method should replace characters
		//                    30 thru 34 of the character data with "98665".
		//
		// Semantic Requirements: 11
		//
		//----------------------------------------------------------------------------

		[Test]
		public void core0018C()
		{
			string computedValue = "";//0;
			string expectedValue = "1230 North Ave. Dallas, Texas 98665";
			System.Xml.XmlNode testNode = null;
			System.Xml.XmlCharacterData testNodeData = null;

			testResults results = new testResults("Core0018C");
			try
			{
				results.description = "Replace a character substring at the end of character data.";
				//
				// Retrieve the targeted data and invoke its "replaceData" method.
				//
				testNode = util.nodeObject(util.FIRST,util.SIXTH);
				testNodeData = (System.Xml.XmlCharacterData)testNode.FirstChild;
				testNodeData.ReplaceData(30,5,"98665");
				computedValue = testNodeData.Data;
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

		//------------------------ End test case core-0018C --------------------------
		//
		//--------------------------- test case core-0019C ---------------------------
		//
		// Testing feature - The "replaceData(offset,count,arg)" method replace the
		//                   characters starting at the specified 16-bit units with the
		//                   specified string.  Test situation where the length of
		//                   the arg string is greater than the specified offset.
		//
		// Testing approach - Retrieve the character data from the last child of the
		//                    first employee.  The "replaceData(offset,count,arg)"
		//                    method is then called with offset = 0 and count = 4 and
		//                    arg = "260030".  The method should replace characters
		//                    one thru four with the string "260030".  Note that the
		//                    length of the specified string is greater than the
		//                    specified offset. 
		//
		// Semantic Requirements: 11
		//
		//----------------------------------------------------------------------------

		[Test]
		public void core0019C()
		{
			string computedValue = "";
			string expectedValue = "260030 North Ave. Dallas, Texas 98551";
			System.Xml.XmlNode testNode = null;
			System.Xml.XmlCharacterData testNodeData = null;
   
			testResults results = new testResults("Core0019C");
			try
			{
				results.description = "Checks \"replaceData(offset,count,arg)\" method when " +
					"length of arg > count.";
				//
				// Retrieve targeted data and invoke its "replaceData" method.
				//
				testNode = util.nodeObject(util.FIRST,util.SIXTH);
				testNodeData = (System.Xml.XmlCharacterData)testNode.FirstChild;
				testNodeData.ReplaceData(0,4,"260030");
				computedValue = testNodeData.Data;
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

		//------------------------ End test case core-0019C --------------------------
		//
		//--------------------------- test case core-0020C ---------------------------
		//
		// Testing feature - If the sum of offset and count exceeds length then all 
		//                   the 16-bit units to the end of data are replaced. 
		//
		// Testing approach - Retrieve the character data from the last child of the
		//                    first employee.  The "replaceData(offset,count,arg)"
		//                    method is then called with offset = 0 and count = 50 and
		//                    arg = "2600".  The method should replace all the
		//                    characters in the character data with "2600".  This
		//                    is because the sum of offset and count exceeds the
		//                    length of the character data. 
		//
		// Semantic Requirements: 12
		//
		//----------------------------------------------------------------------------

		[Test]
		public void core0020C()
		{
			//string testName = "core-0020C";
			string computedValue = "";
			string expectedValue = "2600";
			System.Xml.XmlNode testNode = null;
			System.Xml.XmlCharacterData testNodeData = null;

			testResults results = new testResults("Core0020C");
			try
			{
				results.description = "If the sum of offset and count exceeds the length " +
					"of the character data then the \"replaceData(offset,count,arg)\" " +
					"method replaces all the 16-bit units to the end of the data.";
				//
				// Retrieve the targeted data and invoke its "replaceData" method.
				//
				testNode = util.nodeObject(util.FIRST,util.SIXTH);
				testNodeData = (System.Xml.XmlCharacterData)testNode.FirstChild;
				testNodeData.ReplaceData(0,50,"2600");
				computedValue = testNodeData.Data;
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

		//------------------------ End test case core-0020C --------------------------
		//
		//--------------------------- test case core-0021C ---------------------------
		//
		// Testing feature - The "data" attribute raises a 
		//                   NO_MODIFICATION_ALLOWED_ERR Exception when 
		//                   this node is readonly.
		//
		// Testing approach - Retrieve the character data from the first
		//                    EntityReference node of the last child of the second
		//                    employee and attempt to set its "data" attribute.  Since
		//                    the the descendants of EntityReference nodes are readonly,
		//                    the desired exception should be raised.
		//
		// Semantic Requirements: 13
		//
		//----------------------------------------------------------------------------

		[Test]
		[Category ("NotDotNet")] // MS DOM is buggy
		public void core0021C()
		{
			string computedValue = "";
			System.Xml.XmlNode testNode = null;
			System.Xml.XmlText readOnlyText = null;
			string expectedValue = "System.ArgumentException";//util.NO_MODIFICATION_ALLOWED_ERR;

			testResults results = new testResults("Core0021C");
			try
			{
				results.description = "The \"data\" attribute raises a NO_MODIFICATION_ALLOWED_ERR " +
					"DOMException when this node is readonly.";
				//
				// Retrieve targeted data.
				//
				testNode = util.nodeObject(util.SECOND,util.SIXTH);
				readOnlyText = (System.Xml.XmlText)testNode.FirstChild.FirstChild;
				//
				// Attempt to modify the "data" attribute should raise exception. 
				//
				try 
				{
					readOnlyText.Data = "ABCD";
				}
				catch (ArgumentException ex) 
				{
					computedValue = ex.GetType ().FullName; 
				}
			}
			catch(InvalidOperationException ex)
			{
				computedValue = "Exception " + ex.Message;
			}

			results.expected = expectedValue;
			results.actual = computedValue;

			util.resetData();

			Assert.AreEqual (results.expected, results.actual);
		}

		//------------------------ End test case core-0021C --------------------------
		//
		//--------------------------- test case core-0022C ---------------------------
		//
		// Testing feature - The "appendData(arg) method raises a
		//                   NO_MODIFICATION_ALLOWED_ERR Exception when
		//                   this node is readonly.
		//
		// Testing approach - Retrieve the textual data from the the first
		//                    EntityReference node of the last child of the
		//                    second employee and attempt to append data to it.  
		//                    Descendants of EntityReference nodes are readonly 
		//                    nodes and therefore the desired exception should be
		//                    raised.
		//
		// Semantic Requirements: 14
		//
		//----------------------------------------------------------------------------

		[Test]
		[Category ("NotDotNet")] // MS DOM is buggy
		public void core0022C()
		{
			string computedValue = "";
			System.Xml.XmlNode testNode = null;
			System.Xml.XmlCharacterData readOnlyNode = null;
			string expectedValue = "System.ArgumentException";//util.NO_MODIFICATION_ALLOWED_ERR;

			testResults results = new testResults("Core0022C");
			try
			{
				results.description = "The \"appendData(arg)\" method raises a " +
					"NO_MODIFICATION_ALLOWED_ERR Exception when this " +
					"node is readonly.";
				//
				// Retrieve the targeted data.
				//
				testNode = util.nodeObject(util.SECOND,util.SIXTH);
				readOnlyNode = (System.Xml.XmlCharacterData)testNode.FirstChild.FirstChild;
				//
				// Attempt to append data to a readonly node should raise an exception. 
				//
				try 
				{
					readOnlyNode.AppendData("002");
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

		//------------------------ End test case core-0022C --------------------------
		//
		//--------------------------- test case core-0023C ---------------------------
		//
		// Testing feature - The "insertData(offset,arg) method raises a
		//                   NO_MODIFICATION_ALLOWED_ERR Exception when
		//                   this node is readonly.
		//
		// Testing approach - Retrieve the Text data of the first EntityReference
		//                    node from the last child of the second employee and 
		//                    attempt to insert data into it.  Since the descendants
		//                    of EntityReference nodes are readonly, the desired
		//                    exception should be raised.
		//
		// Semantic Requirements: 15
		//
		//----------------------------------------------------------------------------

		[Test]
		[Category ("NotDotNet")] // MS DOM is buggy
		public void core0023C()
		{
			string computedValue = "";
			System.Xml.XmlNode testNode = null;
			System.Xml.XmlCharacterData readOnlyNode = null;
			string expectedValue = "System.ArgumentException";//util.NO_MODIFICATION_ALLOWED_ERR;

			testResults results = new testResults("Core0023C");
			try
			{
				results.description = "The \"insertData(offset,arg)\" method raises a " +
					"NO_MODIFICATION_ALLOWED_ERR Exception when this " +
					"node is readonly.";
				//
				// Retrieve the targeted data
				//
				testNode = util.nodeObject(util.SECOND,util.SIXTH);
				readOnlyNode = (System.Xml.XmlCharacterData)testNode.FirstChild.FirstChild;
				//
				// Attempt to insert data into a readonly node should raise an exception.
				//
				try 
				{
					readOnlyNode.InsertData(2,"ABCD");
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

		//------------------------ End test case core-0023C --------------------------
		//
		//--------------------------- test case core-0024C ---------------------------
		//
		// Testing feature - The "deleteData(offset,count) method raises a
		//                   NO_MODIFICATION_ALLOWED_ERR Exception when
		//                   ths is readonly.
		//
		// Testing approach - Retrieve the textual data of the the first
		//                    EntityReference node from the last child of the
		//                    second employee and attempt to delete data from it.  
		//                    Since the descendants of EntityReference nodes are 
		//                    readonly, the desired exception should be raised.
		//
		// Semantic Requirements: 16
		//
		//----------------------------------------------------------------------------

		[Test]
		[Category ("NotDotNet")] // MS DOM is buggy
		public void core0024C()
		{
			string computedValue = "";
			System.Xml.XmlCharacterData readOnlyNode = null;
			System.Xml.XmlNode testNode = null;
			string expectedValue = "System.ArgumentException";//util.NO_MODIFICATION_ALLOWED_ERR;

			testResults results = new testResults("Core0024C");
			try
			{
				results.description = "The \"deleteData(offset,count)\" method raises a " +
					"NO_MODIFICATION_ALLOWED_ERR Exception when this " +
					"node is readonly.";
				//
				// Retrieve the targeted data. 
				//
				testNode = util.nodeObject(util.SECOND,util.SIXTH); 
				readOnlyNode = (System.Xml.XmlCharacterData)testNode.FirstChild.FirstChild;
				//
				// Attempt to delete data from a readonly node should raise an
				// exception.
				//
				try 
				{
					readOnlyNode.DeleteData(2,4);
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

		//------------------------ End test case core-0024C --------------------------
		//
		//--------------------------- test case core-0025C ---------------------------
		//
		// Testing feature - The "replaceData(offset,count,arg) method raises a
		//                   NO_MODIFICATION_ALLOWED_ERR Exception when
		//                   this node is readonly.
		//
		// Testing approach - Retrieve the textual data of the first EntityReference
		//                    node from the last child of the second employee and
		//                    attempt to replace data from it.  Since the descendants
		//                    of EntityReference nodes are readonly, the desired 
		//                    exception should be raised.
		//
		// Semantic Requirements: 17
		//
		//----------------------------------------------------------------------------

		[Test]
		[Category ("NotDotNet")] // MS DOM is buggy
		public void core0025C()
		{
			string computedValue = "";
			System.Xml.XmlCharacterData readOnlyNode = null;
			System.Xml.XmlNode testNode = null;
			string expectedValue = "System.ArgumentException";//util.NO_MODIFICATION_ALLOWED_ERR;

			testResults results = new testResults("Core0025C");
			try
			{
				results.description = "The \"replaceData(offset,count,arg)\" method raises a " +
					"NO_MODIFICATION_ALLOWED_ERR Exception " +
					"when this node is readonly.";
				//
				// Retrieve the targeted data.
				//
				testNode = util.nodeObject(util.SECOND,util.SIXTH);
				readOnlyNode = (System.Xml.XmlCharacterData)testNode.FirstChild.FirstChild;
				//
				// Attempt to replace data from a readonly node should raise an
				// exception.
				//
				try 
				{
					readOnlyNode.ReplaceData(2,4,"ABCD");
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

		//------------------------ End test case core-0025C --------------------------
		//
		//--------------------------- test case core-0026C ---------------------------
		//
		// Testing feature - The "substringData(offset,count)" method raises an 
		//                   INDEX_SIZE_ERR Exception if the specified offset is
		//                   negative.
		//
		// Testing approach - Retrieve the character data of the last child of the
		//                    of the first employee and invoke its 
		//                    "substringData(offset,count)" method with offset = -5
		//                    count = 3.  It should raise the desired exception since
		//                    the offset is negative.
		//
		// Semantic Requirements: 19
		//
		//----------------------------------------------------------------------------

		[Test]
		public void core0026C()
		{
			string computedValue = "";
			string returnedValue = "";
			System.Xml.XmlNode testNode = null;
			System.Xml.XmlCharacterData testNodeData = null;
			string expectedValue = "System.ArgumentOutOfRangeException";//util.INDEX_SIZE_ERR;

			testResults results = new testResults("Core0026C");
			try
			{
				results.description = "The \"substringData(offset,count)\" method raises an " +
					"INDEX_SIZE_ERR Exception if the specified " +
					"offset is negative.";
				//
				// Retrieve the targeted data.
				//
				testNode = util.nodeObject(util.FIRST,util.SIXTH);
				testNodeData = (System.Xml.XmlCharacterData)testNode.FirstChild;  
				//
				// A negative offset value should raise and exception.
				//
				try 
				{
					returnedValue = testNodeData.Substring(-5,3); 
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

			Assert.AreEqual (results.expected, results.actual);
		}

		//------------------------ End test case core-0026C --------------------------
		//
		//--------------------------- test case core-0027C ---------------------------
		//
		// Testing feature - The "substringData(offset,count)" method raises an
		//                   INDEX_SIZE_ERR Exception if the specified offset is
		//                   greater than the number of 16-bit units in the "data"
		//                   attribute. 
		//
		// Testing approach - Retrieve the character data of the last child of the
		//                    first employee and invoke its 
		//                    "substringData(offset,count)" method with offset = 40 and 
		//                    count = 3.  The value of the offset is greater than that
		//                    one of the "data" attribute, therefore the desired 
		//                    exception should be raised.
		//
		// Semantic Requirements: 20 
		//
		//----------------------------------------------------------------------------

		[Test]
		public void core0027C()
		{
			string computedValue = "";
			string returnedValue = "";
			System.Xml.XmlNode testNode = null;
			System.Xml.XmlCharacterData testNodeData = null;
			string expectedValue = "System.ArgumentOutOfRangeException";

			testResults results = new testResults("Core0027C");
			try
			{
				results.description = "The \"substringData(offset,count)\" method raises an " +
					"ArgumentOutOfRangeException Exception if the specified " +
					"offset is greater than the number of 16-bit units " +
					"in the \"data\" attribute.";
				//
				// Retrieve the targeted data.
				//
				testNode = util.nodeObject(util.FIRST,util.SIXTH);
				testNodeData = (System.Xml.XmlCharacterData)testNode.FirstChild;
				//
				// Offset greater than number of characters in data should raise an
				// exception.
				//
				try 
				{
					returnedValue = testNodeData.Substring(40,3);
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

			Assert.AreEqual (results.expected, results.actual);
		}

		//------------------------ End test case core-0027C --------------------------
		//
		//--------------------------- test case core-0028C ---------------------------
		//
		// Testing feature - The "substringData(offset,count)" method raises an
		//                   INDEX_SIZE_ERR Exception if the count is negative 
		//
		// Testing approach - Retrieve the character data of the last child of the
		//                    first employee and invoke its
		//                    "substringData(offset,count)" method with offset = 10
		//                    and count = -3.  Since the value of count is negative,
		//                    the desired exception should be raised.
		//
		// Semantic Requirements: 21 
		//
		//----------------------------------------------------------------------------

		[Test]
		public void core0028C()
		{
			string computedValue = "";
			string returnedValue = "";
			System.Xml.XmlNode testNode = null;
			System.Xml.XmlCharacterData testNodeData = null;
			string expectedValue = "System.ArgumentOutOfRangeException";

			testResults results = new testResults("Core0028C");
			try
			{
				results.description = "The \"substringData(offset,count)\" method raises an " +
					"INDEX_SIZE_ERR Exception if the count is negative.";
				//
				// Retrieve the targeted data.
				//
				testNode = util.nodeObject(util.FIRST,util.SIXTH);
				testNodeData = (System.Xml.XmlCharacterData)testNode.FirstChild;
				//
				// A negative value for "count" should raise an exception.
				//
				try 
				{
					returnedValue = testNodeData.Substring(10,-3);
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

			Assert.AreEqual (results.expected, results.actual);
		}

		//------------------------ End test case core-0028C --------------------------
		//
		//--------------------------- test case core-0029C ---------------------------
		//
		// Testing feature - The "deleteData(offset,count)" method raises an
		//                   ArgumentOutOfRangeException if the specified offset is
		//                   negative.
		//
		// Testing approach - Retrieve the character data of the last child of the
		//                    first employee and invoke its "deleteData(offset,count)"
		//                    method with offset = -5 and count = 3.  The value of the
		//                    offset is negative and therefore the desired exception
		//                    should be raised.
		//
		// Semantic Requirements: 22 
		//
		//----------------------------------------------------------------------------

		[Test]
		public void core0029C()
		{
			string computedValue = "";
			System.Xml.XmlNode testNode = null;
			System.Xml.XmlCharacterData testNodeData = null;
			string expectedValue = "System.ArgumentOutOfRangeException";

			testResults results = new testResults("Core0029C");
			try
			{
				results.description = "The \"deleteData(offset,count)\" method raises an " +
					"ArgumentOutOfRangeException if the specified " +
					"offset is negative.";
				//
				// Retrieve the targeted data.
				//
				testNode = util.nodeObject(util.FIRST,util.SIXTH);
				testNodeData = (System.Xml.XmlCharacterData)testNode.FirstChild;
				//
				// Invoke "deleteData(offset,count)" method with negative offset should
				// should raise an excetion.
				//
				try 
				{
					testNodeData.DeleteData(-5,3);
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

		//------------------------ End test case core-0029C --------------------------
		//
		//--------------------------- test case core-0030C ---------------------------
		//
		// Testing feature - The "deleteData(offset,count)" method raises an
		//                   ArgumentOutOfRangeException if the specified offset is
		//                   greater than the number of characters in the "data"
		//                   attribute.
		//
		// Testing approach - Retrieve the character data of the last child of the
		//                    first employee and invoke its "deleteData(offset,count)"
		//                    method with offset = 40 and count = 3.  The value of the
		//                    offset is greater than the number of characters in the
		//                    "data" attribute (35) and therefore the intended 
		//                    exception should be raised.
		//
		// Semantic Requirements: 23 
		//
		//----------------------------------------------------------------------------

		[Test]
		public void core0030C()
		{
			string computedValue = "";
			System.Xml.XmlNode testNode = null;
			System.Xml.XmlCharacterData testNodeData = null;
			string expectedValue = "System.ArgumentOutOfRangeException";

			testResults results = new testResults("Core0030C");
			try
			{
				results.description = "The \"deleteData(offset,count)\" method raises an " +
					"ArgumentOutOfRangeException if the specified " +
					"offset is greater than the number of characters " +
					"in the \"data\" attribute.";
				//
				// Retrieve the targeted data.
				//
				testNode = util.nodeObject(util.FIRST,util.SIXTH);
				testNodeData = (System.Xml.XmlCharacterData)testNode.FirstChild;
				//
				// Invocation of "deleteData(offset,count)" method with offset > data
				// attribute should raise and exception.
				//
				try 
				{
					testNodeData.DeleteData(40,3);
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

		//------------------------ End test case core-0030C --------------------------
		//
		//--------------------------- test case core-0031C ---------------------------
		//
		// Testing feature - The "deleteData(offset,count)" method raises an
		//                   ArgumentOutOfRangeException if the specified count is
		//                   negative.
		//
		// Testing approach - Retrieve the character data of the last child of the
		//                    first employee and invoke its "deleteData(offset,count)" 
		//                    method with offset = 10 and count = -3.  The value
		//                    of the specified count is negative and therefore the
		//                    intended exception should be raised.
		//
		// Semantic Requirements: 24 
		//
		//----------------------------------------------------------------------------

		[Test]
		public void core0031C()
		{
			string computedValue = "";
			System.Xml.XmlNode testNode = null;
			System.Xml.XmlCharacterData testNodeData = null;
			string expectedValue = "System.ArgumentOutOfRangeException";

			testResults results = new testResults("Core0031C");
			try
			{
				results.description = "The \"deleteData(offset,count)\" method raises an " +
					"ArgumentOutOfRangeException if the specified " +
					"count is negative.";
				//
				// Retrieve the targeted data.
				//
				testNode = util.nodeObject(util.FIRST,util.SIXTH);
				testNodeData = (System.Xml.XmlCharacterData)testNode.FirstChild;
				//
				// Invocation of "deleteData(offset,count)" method with count < 0 
				// should raise and exception. 
				//
				try 
				{
					testNodeData.DeleteData(10,-3);
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

		//------------------------ End test case core-0031C --------------------------
		//
		//--------------------------- test case core-0032C ---------------------------
		//
		// Testing feature - The "replaceData(offset,count,arg)" method raises an
		//                   ArgumentOutOfRangeException if the specified offset is
		//                   negative.
		//
		// Testing approach - Retrieve the character data of the last child of the
		//                    first employee and invoke its 
		//                    "replaceData(offset,count,arg)" method with 
		//                    offset = -5 and count = 3 and arg = "ABC".  The value 
		//                    of the offset is negative and therefore the desired 
		//                    exception should be raised.
		//
		// Semantic Requirements: 25
		//
		//----------------------------------------------------------------------------

		[Test]
		public void core0032C()
		{
			string computedValue = "";
			System.Xml.XmlNode testNode = null;
			System.Xml.XmlCharacterData testNodeData = null;
			string expectedValue = "System.ArgumentOutOfRangeException";

			testResults results = new testResults("Core0032C");
			try
			{
				results.description = "The \"replaceData(offset,count,arg)\" method raises an " +
					"ArgumentOutOfRangeException if the specified " +
					"offset is negative.";
				//
				// Retrieve the targeted data.
				//
				testNode = util.nodeObject(util.FIRST,util.SIXTH);
				testNodeData = (System.Xml.XmlCharacterData)testNode.FirstChild;
				//
				// Invocation of "replaceData(offset,count,arg)" method offset < 0
				// should raise an exception.
				//
				try 
				{
					testNodeData.ReplaceData(-5,3,"ABC");
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

		//------------------------ End test case core-0032C --------------------------
		//
		//--------------------------- test case core-0033C ---------------------------
		//
		// Testing feature - The "replaceData(offset,count,arg)" method raises an
		//                   INDEX_RANGE_ERR Exception if the specified offset is
		//                   greater than the number of 16-bit units in the "data"
		//                   attribute.
		//
		// Testing approach - Retrieve the character data of the last child of the
		//                    first employee and invoke its 
		//                    "replaceData(offset,count,arg)" method with offset = 40,
		//                    count = 3 and arg = "ABC".  The value of the offset is 
		//                    greater than the number of characters in the "data" 
		//                    attribute (35) and therefore the intended exception 
		//                    should be raised.
		//
		// Semantic Requirements: 26
		//
		//----------------------------------------------------------------------------

		[Test]
		public void core0033C()
		{
			string computedValue = "";
			System.Xml.XmlNode testNode = null;
			System.Xml.XmlCharacterData testNodeData = null;
			string expectedValue = "System.ArgumentOutOfRangeException";

			testResults results = new testResults("Core0033C");
			try
			{
				results.description = "The \"replaceData(offset,count,arg)\" method raises an " +
					"ArgumentOutOfRangeException if the specified " +
					"offset is greater than the number of 16-bit units " +
					"in the \"data\" attribute.";
				//
				// Retrieve the targeted data.
				//
				testNode = util.nodeObject(util.FIRST,util.SIXTH);
				testNodeData = (System.Xml.XmlCharacterData)testNode.FirstChild;
				//
				// Invocation of "replaceData(offset,count,arg)" method with offset > data  
				// attribute should raise and exception.
				//
				try 
				{
					testNodeData.ReplaceData(40,3,"ABC");
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

		//------------------------ End test case core-0033C --------------------------
		//
		//--------------------------- test case core-0034C ---------------------------
		//
		// Testing feature - The "replaceData(offset,count,arg)" method raises an
		//                   ArgumentOutOfRangeException if the specified count is
		//                   negative.
		//
		// Testing approach - Retrieve the character data of the last child of the
		//                    first employee and invoke its 
		//                    "replaceData(offset,count,arg)" method with offset = 10,
		//                    count = -3 and arg = "ABC".  The value of the specified 
		//                    count is negative and therefore the intended exception 
		//                    should be raised.
		//
		// Semantic Requirements: 27
		//
		//----------------------------------------------------------------------------

		[Test]
		public void core0034C()
		{
			string computedValue = "";
			System.Xml.XmlNode testNode = null;
			System.Xml.XmlCharacterData testNodeData = null;
			string expectedValue = "System.ArgumentOutOfRangeException";

			testResults results = new testResults("Core0034C");
			try
			{
				results.description = "The \"replaceData(offset,count,arg)\" method raises an " +
					"ArgumentOutOfRangeException if the specified " +
					"count is negative.";
				//
				// Retrieve the targeted data.
				//
				testNode = util.nodeObject(util.FIRST,util.SIXTH);
				testNodeData = (System.Xml.XmlCharacterData)testNode.FirstChild;
				//
				// Invocation of "replaceData(offset,count,arg)" method with count < 0
				// should raise an exception.
				//
				try 
				{
					testNodeData.ReplaceData(10,-3,"ABC");
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

		//------------------------ End test case core-0034C --------------------------
		//
		//--------------------------- test case core-0035C ---------------------------
		//
		// Testing feature - The "insertData(offset,arg)" method raises an
		//                   ArgumentOutOfRangeException if the specified offset is
		//                   negative.
		//
		// Testing approach - Retrieve the character data from the last child of the
		//                    first employee and invoke its "insertData(offset,arg)" 
		//                    method with offset = -5 arg = "ABC".  The value
		//                    of the offset is negative and therefore the desired
		//                    exception should be raised.
		//
		// Semantic Requirements: 28
		//
		//----------------------------------------------------------------------------

		[Test]
		public void core0035C()
		{
			string computedValue = "";
			System.Xml.XmlNode testNode = null;
			System.Xml.XmlCharacterData testNodeData = null;
			string expectedValue = "System.ArgumentOutOfRangeException";

			testResults results = new testResults("Core0035C");
			try
			{
				results.description = "The \"insertData(offset,arg)\" method raises an " +
					"ArgumentOutOfRangeException if the specified offset is negative.";
				//
				// Retrieve the targeted data.
				//
				testNode = util.nodeObject(util.FIRST,util.SIXTH);
				testNodeData = (System.Xml.XmlCharacterData)testNode.FirstChild;
				//
				// Invocation of insertData(offset,arg)" method with offset < 0
				// should raise an exception.
				//
				try 
				{
					testNodeData.InsertData(-5,"ABC");
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

		//------------------------ End test case core-0035C --------------------------
		//
		//--------------------------- test case core-0036C ---------------------------
		//
		// Testing feature - The "insertData(offset,arg)" method raises an
		//                   ArgumentOutOfRangeException if the specified offset is
		//                   greater than the number of 16-bit units in the "data"
		//                   attribute.
		//
		// Testing approach - Retrieve the character data from the last child of the
		//                    first employee and invoke its "insertData(offset,arg)" 
		//                    method with offset = 40 and arg = "ABC".  The value of 
		//                    the offset is greater than the number of characters in 
		//                    the "data" attribute(35) and therefore the intended
		//                    exception should be raised.
		//
		// Semantic Requirements: 29
		//
		//----------------------------------------------------------------------------

		[Test]
		public void core0036C()
		{
			string computedValue = "";
			System.Xml.XmlNode testNode = null;
			System.Xml.XmlCharacterData testNodeData = null;
			string expectedValue = "System.ArgumentOutOfRangeException";

			testResults results = new testResults("Core0036C");
			try
			{
				results.description = "The \"insertData(offset,arg)\" method raises an " +
					"ArgumentOutOfRangeException if the specified " +
					"offset is greater than the number of 16-bit units " +
					"in the \"data\" attribute.<br>";
				//
				// Retrieve the targeted data.
				//
				testNode = util.nodeObject(util.FIRST,util.SIXTH);
				testNodeData = (System.Xml.XmlCharacterData)testNode.FirstChild;
				//
				// Invocation of "insertData(offset arg)" method with offset > data 
				// attribute should raise an exception.
				//
				try 
				{
					testNodeData.InsertData(40,"ABC");
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

		//------------------------ End test case core-0036C --------------------------
	}
}