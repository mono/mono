//
// System.Xml.Xsl.MsxslScriptTests.cs
//
// Author:
//   Atsushi Enomoto <atsushi@ximian.com>
//
// (C) 2004 Novell Inc.
//

using System;
using System.IO;
using System.Xml;
using System.Xml.Xsl;
using NUnit.Framework;

namespace MonoTests.System.Xml.Xsl
{
	[TestFixture]
	public class MsxslScriptTests : Assertion
	{
		// PI calc stuff are one of MSDN samples.

		static XmlDocument doc;
		static MsxslScriptTests ()
		{
			string inputxml = @"<?xml version='1.0'?>
<data>
  <circle>
    <radius>12</radius>
  </circle>
  <circle>
    <radius>37.5</radius>
  </circle>
</data>";
			doc = new XmlDocument ();
			doc.LoadXml (inputxml);
		}

		static string xslstring = @"<xsl:stylesheet version='1.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
    xmlns:msxsl='urn:schemas-microsoft-com:xslt'
    xmlns:user='urn:my-scripts'>

    ***** rewrite here *****

  <xsl:template match='data'>  
    <circles>

      <xsl:for-each select='circle'>
        <circle>
          <xsl:copy-of select='node()'/>
          <circumference>
             <!-- xsl:value-of select='user:circumference(radius)'/ -->
               TEST:
             <xsl:value-of select='user:PadRight(&quot;test-string&quot;, 20)'/>
          </circumference>
        </circle>
      </xsl:for-each>
    </circles>
  </xsl:template>
</xsl:stylesheet>";

		string cs1 = @"<msxsl:script language='C#' implements-prefix='user'>
    <![CDATA[
        string PadRight( string str, int padding) {
            return str.PadRight(padding);
        }
    ]]>
    </msxsl:script>";
		string cs2 = @"<msxsl:script language='C#' implements-prefix='user'>
     <![CDATA[
     public double circumference(double radius){
       double pi = 3.14;
       double circ = pi*radius*2;
       return circ;
     }
      ]]>
   </msxsl:script>";
		string vb1 = @"<msxsl:script language='VB' implements-prefix='user'>
     <![CDATA[
     public function circumference(radius as double) as double
       dim pi as double = 3.14
       dim circ as double = pi*radius*2
       return circ
     end function
        public function greet () as string
                return " + "\"Hey! you should not depend on proprietary scripting!!\"" + @"
        end function
      ]]>
   </msxsl:script>";
		string js1 = @"<msxsl:script language='JScript' implements-prefix='user'>
     <![CDATA[
     function circumference(radius : double) : double {
       var pi : double = 3.14;
       var circ : double = pi*radius*2;
       return circ;
     }
     function greet () : String {
        return " + "\"Hey! you should not depend on proprietary scripting!!\"" + @";
     }
      ]]>
   </msxsl:script>";


		XslTransform xslt;

		[SetUp]
		public void GetReady ()
		{
			xslt = new XslTransform ();
		}

		[Test]
		public void TestCSharp ()
		{
			XmlTextReader xr = new XmlTextReader (cs1, XmlNodeType.Document, null);
			xslt.Load (xr);
			xslt.Transform (doc.CreateNavigator (), null, new XmlTextWriter (new StringWriter ()));

			xr = new XmlTextReader (cs2, XmlNodeType.Document, null);
			xslt.Load (xr);
			xslt.Transform (doc.CreateNavigator (), null, new XmlTextWriter (new StringWriter ()));
		}

		[Test]
		public void TestVB ()
		{
			XmlTextReader xr = new XmlTextReader (vb1, XmlNodeType.Document, null);
			xslt.Load (xr);
			xslt.Transform (doc.CreateNavigator (), null, new XmlTextWriter (new StringWriter ()));
		}

		[Test]
		public void TestJScript ()
		{
			XmlTextReader xr = new XmlTextReader (js1, XmlNodeType.Document, null);
			xslt.Load (xr);
			xslt.Transform (doc.CreateNavigator (), null, new XmlTextWriter (new StringWriter ()));
		}

		[Test]
		[ExpectedException (typeof (XsltCompileException))]
		public void InvalidScript ()
		{
			string script = @"<xsl:stylesheet version='1.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:user='urn:my-scripts'
    xmlns:msxsl='urn:schemas-microsoft-com:xslt'>
    <!-- -->
    <xsl:output method='html' indent='no' />
    <!-- -->
    <xsl:template match='/project'>
        <xsl:if test='user:BadScriptFunction(&apos;test&apos;)'></xsl:if>
    </xsl:template>
    <!-- -->
    <msxsl:script language='C#' implements-prefix='user'>
        <![CDATA[
            string BadScriptFunction(string test) {
                xxx;
            }
        ]]>
    </msxsl:script>
    <!-- -->
</xsl:stylesheet>";
			xslt.Load (new XmlTextReader (script, XmlNodeType.Document, null));
		}
	}
}
