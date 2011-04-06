//
// Tests for the properties of the builtin types.
// 
// Author:
//   David Sheldon <dave-mono@earth.li>
//
//

using System;
using System.Xml;
using System.Xml.Schema;
using System.IO;
using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlSchemaBuiltInDatatypeTests
	{

    [Test]
    public void TestWhiteSpaceCollapse () {
       WhiteSpaceTest("date", "2003-10-10");
       WhiteSpaceTest("decimal", "2003.10");
       WhiteSpaceTest("integer", "0004");
       WhiteSpaceTest("float", "1.3");
       WhiteSpaceTest("boolean", "true");
       WhiteSpaceTest("double", "2.3");
       WhiteSpaceTest("time", "12:34:56");
       WhiteSpaceTest("duration", "P1347Y");
       WhiteSpaceTest("dateTime", "2003-10-10T12:34:56.78");
       WhiteSpaceTest("gYearMonth", "2003-10");
       WhiteSpaceTest("gYear", "2003");
       WhiteSpaceTest("gMonthDay", "--12-12");  // 
       WhiteSpaceTest("gMonth", "--12--");      //  These three will fail, due to 
       WhiteSpaceTest("gDay", "---12");         //  bug 52274
       WhiteSpaceTest("hexBinary", "0fB7");
    }


    /* Takes a type name, and a valid example of that type, 
       Creates a schema consisting of a single element of that
       type, and validates a bit of xml containing the valid 
       value, with whitespace against that schema.
     
FIXME: Really we want to test the value of whitespace more
       directly that by creating a schema then parsing a string.
     
     */

    public void WhiteSpaceTest(string type, string valid) {
      passed = true;
      XmlSchema schema = new XmlSchema();

      schema.TargetNamespace= "http://example.com/testCase";
      XmlSchemaElement element = new XmlSchemaElement();
      element.Name = "a";
      element.SchemaTypeName = new XmlQualifiedName(type, "http://www.w3.org/2001/XMLSchema");
      schema.Items.Add(element);
      schema.Compile(new ValidationEventHandler(ValidationCallbackOne));

      XmlValidatingReader vr = new XmlValidatingReader(new XmlTextReader(new StringReader("<a xmlns='http://example.com/testCase'>\n\n"+valid+"\n\n</a>" )));
      vr.Schemas.Add(schema);
      vr.ValidationType = ValidationType.Schema;
//      vr.ValidationEventHandler += new ValidationEventHandler(ValidationCallbackOne);
      while(vr.Read()) { };
      vr.Close();
      
      Assert.IsTrue (passed, type + " doesn't collapse whitespace: " + (errorInfo != null ? errorInfo.Message : null));
    }

    bool passed = true;
    ValidationEventArgs errorInfo;

    public void ValidationCallbackOne(object sender, ValidationEventArgs args) {
      passed = false;
      errorInfo = args;
    }


  }  
}
