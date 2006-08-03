//
// XslDecimalFormat.jvm.cs
//
// Authors:
//	Andrew Skiba <andrews@mainsoft.com>
//	
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

using QName = System.Xml.XmlQualifiedName;

namespace Mono.Xml.Xsl {
	internal class XslDecimalFormat {
		
		java.text.DecimalFormatSymbols javaFormat;
		string baseUri;
		int lineNumber;
		int linePosition;

		public static readonly XslDecimalFormat Default = new XslDecimalFormat ();
		
		XslDecimalFormat ()
		{
			javaFormat = new java.text.DecimalFormatSymbols ();
			javaFormat.setNaN ("NaN");
			javaFormat.setInfinity ("Infinity");
		}

		public XslDecimalFormat (Compiler c)
			:this ()
		{
			Initialize(c); 
		}

		private void Initialize(Compiler c)
		{
			XPathNavigator n = c.Input;

			IXmlLineInfo li = n as IXmlLineInfo;
			if (li != null) {
				lineNumber = li.LineNumber;
				linePosition = li.LinePosition;
			}
			baseUri = n.BaseURI;

			if (n.MoveToFirstAttribute ()) {
				do {
					if (n.NamespaceURI != String.Empty)
						continue;
					
					switch (n.LocalName) {
					case "name": break; // already handled
					case "decimal-separator":
						if (n.Value.Length != 1)
							throw new XsltCompileException ("XSLT decimal-separator value must be exact one character.", null, n);
						javaFormat.setDecimalSeparator (n.Value[0]);
						break;
						
					case "grouping-separator":
						if (n.Value.Length != 1)
							throw new XsltCompileException ("XSLT grouping-separator value must be exact one character.", null, n);
						javaFormat.setGroupingSeparator (n.Value[0]);
						break;
						
					case "infinity":
						javaFormat.setInfinity (n.Value);
						break;
					case "minus-sign":
						if (n.Value.Length != 1)
							throw new XsltCompileException ("XSLT minus-sign value must be exact one character.", null, n);
						javaFormat.setMinusSign (n.Value[0]);
						break;
					case "NaN":
						javaFormat.setNaN (n.Value);
						break;
					case "percent":
						if (n.Value.Length != 1)
							throw new XsltCompileException ("XSLT percent value must be exact one character.", null, n);
						javaFormat.setPercent (n.Value[0]);
						break;
					case "per-mille":
						if (n.Value.Length != 1)
							throw new XsltCompileException ("XSLT per-mille value must be exact one character.", null, n);
						javaFormat.setPerMill (n.Value[0]);
						break;
					case "digit":
						if (n.Value.Length != 1)
							throw new XsltCompileException ("XSLT digit value must be exact one character.", null, n);
						javaFormat.setDigit (n.Value[0]);
						break;
					case "zero-digit":
						if (n.Value.Length != 1)
							throw new XsltCompileException ("XSLT zero-digit value must be exact one character.", null, n);
						javaFormat.setZeroDigit (n.Value [0]);
						break;
					case "pattern-separator":
						if (n.Value.Length != 1)
							throw new XsltCompileException ("XSLT pattern-separator value must be exact one character.", null, n);
						javaFormat.setPatternSeparator (n.Value [0]);
						break;
					}
				} while (n.MoveToNextAttribute ());
				n.MoveToParent ();
			}
		}

		public void CheckSameAs (XslDecimalFormat other)
		{
			if (! this.javaFormat.Equals (other.javaFormat))
				throw new XsltCompileException (null, other.baseUri, other.lineNumber, other.linePosition);
		}

		public string FormatNumber (double number, string pattern)
		{
			java.text.DecimalFormat frm = new java.text.DecimalFormat("", javaFormat);

			frm.applyLocalizedPattern (pattern);

			//TODO: the next 4 string could be replaced by just 
			//return frm.format (number);
			//I don't want to do that before release
			java.lang.StringBuffer buffer= new java.lang.StringBuffer ();
			java.text.FieldPosition fld = new java.text.FieldPosition (0);

			frm.format (number, buffer, fld);
			return buffer.ToString();
		}
	}
}
