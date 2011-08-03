//
// Tests for the length facets.
// 
// Author:
//   David Sheldon <dave-mono@earth.li>
//
//

using System;
using System.Xml;
using System.Xml.Schema;
using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlSchemaLengthFacetTests
	{

    [Test]
    public void TestValidCombinations () {
      CreateSimpletypeLength("5","-","-", true);
      CreateSimpletypeLength("5","1","-", false);
      CreateSimpletypeLength("5","-","1", false);
      
      CreateSimpletypeLength("-","1","10", true);
      CreateSimpletypeLength("-","10","1", false);
      CreateSimpletypeLength("-","1","-", true);
      CreateSimpletypeLength("-","-","1", true);
      
      CreateSimpletypeLength("-5","-","-", false);
      CreateSimpletypeLength("-","-1","-", false);
      CreateSimpletypeLength("-","-","-1", false);

      CreateSimpletypeLength("5.4","-","-", false);
      CreateSimpletypeLength("-","1.0","-", false);
      CreateSimpletypeLength("-","-","1.3", false);

      CreateSimpletypeLength("+5","-","-", true);
      CreateSimpletypeLength("-","+1","-", true);
      CreateSimpletypeLength("-","-","+1", true);
     }

    private void CreateSimpletypeLength(string length, string minLength, string maxLength, bool expected) {
      passed = true;

      XmlSchema schema = new XmlSchema();

      XmlSchemaSimpleType testType = new XmlSchemaSimpleType();
      testType.Name = "TestType";

      XmlSchemaSimpleTypeRestriction testTypeRestriction = new XmlSchemaSimpleTypeRestriction();
      testTypeRestriction.BaseTypeName = new XmlQualifiedName("string", "http://www.w3.org/2001/XMLSchema");

      if (length != "-") {
        XmlSchemaLengthFacet _length = new XmlSchemaLengthFacet();
        _length.Value = length;
        testTypeRestriction.Facets.Add(_length);
      }
      if (minLength != "-") {
        XmlSchemaMinLengthFacet _minLength = new XmlSchemaMinLengthFacet();
        _minLength.Value = minLength;
        testTypeRestriction.Facets.Add(_minLength);
      }
      if (maxLength != "-") {
        XmlSchemaMaxLengthFacet _maxLength = new XmlSchemaMaxLengthFacet();
        _maxLength.Value = maxLength;
        testTypeRestriction.Facets.Add(_maxLength);
      }

      testType.Content = testTypeRestriction;
      schema.Items.Add(testType);
      schema.Compile(new ValidationEventHandler(ValidationCallbackOne));

      Assert.IsTrue (expected == passed, (passed ? "Test passed, should have failed" : "Test failed, should have passed") + ": " + length + " " + minLength + " " + maxLength);
      
    }

    bool passed = true;

    private void ValidationCallbackOne(object sender, ValidationEventArgs args) {
      passed = false;
    }
  }  
}
