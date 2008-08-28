//
// XslOutput.cs
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//	Oleg Tkachenko (oleg@tkachenko.com)
//	
// (C) 2003 Ben Maurer
// (C) 2003 Atsushi Enomoto
// (C) 2003 Oleg Tkachenko
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
using System.Collections;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.Text;

namespace Mono.Xml.Xsl
{
	using QName = System.Xml.XmlQualifiedName;

	internal enum OutputMethod {
		XML,
		HTML,
		Text,
		Custom,
		Unknown
	}
	
	internal enum StandaloneType {
		NONE,
		YES,
		NO
	}

	internal class XslOutput // also usable for xsl:result-document
	{
		string uri;
		QName customMethod;
		OutputMethod method = OutputMethod.Unknown; 
		string version;
		Encoding encoding = System.Text.Encoding.UTF8;
		bool omitXmlDeclaration;
		StandaloneType standalone = StandaloneType.NONE;
		string doctypePublic;
		string doctypeSystem;
		QName [] cdataSectionElements;
		string indent;
		string mediaType;
		string stylesheetVersion;

		// for compilation only.
		ArrayList cdSectsList = new ArrayList ();

		public XslOutput (string uri, string stylesheetVersion)
		{
			this.uri = uri;
			this.stylesheetVersion = stylesheetVersion;
		}

		public OutputMethod Method { get { return method; }}
		public QName CustomMethod { get { return customMethod; }}

		public string Version {
			get { return version; }
		}

		public Encoding Encoding {
			get { return encoding; }
		}

		public string Uri {
			get { return uri; }
		}

		public bool OmitXmlDeclaration {
			get { return omitXmlDeclaration; }
		}

		public StandaloneType Standalone {
			get { return standalone; }
		}

		public string DoctypePublic {
			get { return doctypePublic; }
		}

		public string DoctypeSystem {
			get { return doctypeSystem; }
		}

		public QName [] CDataSectionElements {
			get {
				if (cdataSectionElements == null)
					cdataSectionElements = cdSectsList.ToArray (typeof (QName)) as QName [];
				return cdataSectionElements;
			}
		}

		public string Indent {
			get { return indent; }
		}

		public string MediaType {
			get { return mediaType; }
		}

		public void Fill (XPathNavigator nav)
		{
			if (nav.MoveToFirstAttribute ()) {
				ProcessAttribute (nav);
				while (nav.MoveToNextAttribute ()) {
					ProcessAttribute (nav);
				}

				// move back to original position
				nav.MoveToParent ();
			}
		}

		private void ProcessAttribute (XPathNavigator nav)
		{
			// skip attributes from non-default namespace
			if (nav.NamespaceURI != string.Empty) {
				return;
			}

			string value = nav.Value;

			switch (nav.LocalName) {
			case "cdata-section-elements":
				if (value.Length > 0) {
					cdSectsList.AddRange (XslNameUtil.FromListString (value, nav));
				}
				break;
			case "method":
				if (value.Length == 0) {
					break;
				}

				switch (value) {
					case "xml":
						method = OutputMethod.XML;
						break;
					case "html":
						omitXmlDeclaration = true;
						method = OutputMethod.HTML;
						break;
					case "text":
						omitXmlDeclaration = true;
						method = OutputMethod.Text;
						break;
					default:
						method = OutputMethod.Custom;
						customMethod = XslNameUtil.FromString (value, nav);
						if (customMethod.Namespace == String.Empty) {
							IXmlLineInfo li = nav as IXmlLineInfo;
							throw new XsltCompileException (new ArgumentException (
								"Invalid output method value: '" + value + "'. It" +
								" must be either 'xml' or 'html' or 'text' or QName."),
								nav.BaseURI,
								li != null ? li.LineNumber : 0,
								li != null ? li.LinePosition : 0);
						}
						break;
				}
				break;
			case "version":
				if (value.Length > 0) {
					this.version = value;
				}
				break;
			case "encoding":
				if (value.Length > 0) {
					try {
						this.encoding = System.Text.Encoding.GetEncoding (value);
					} catch (ArgumentException) {
						// MS.NET just leaves the default encoding when encoding is unknown
					} catch (NotSupportedException) {
						// Workaround for a bug in System.Text, it throws invalid exception
					}
				}
				break;
			case "standalone":
				switch (value) {
					case "yes":
						this.standalone = StandaloneType.YES;
						break;
					case "no":
						this.standalone = StandaloneType.NO;
						break;
					default:
						if (stylesheetVersion != "1.0")
							break;

						IXmlLineInfo li = nav as IXmlLineInfo;
						throw new XsltCompileException (new XsltException (
							"'" + value + "' is an invalid value for 'standalone'" +
							" attribute.", (Exception) null),
							nav.BaseURI,
							li != null ? li.LineNumber : 0,
							li != null ? li.LinePosition : 0);
				}
				break;
			case "doctype-public":
				this.doctypePublic = value;
				break;
			case "doctype-system":
				this.doctypeSystem = value;
				break;
			case "media-type":
				if (value.Length > 0) {
					this.mediaType = value;
				}
				break;
			case "omit-xml-declaration":
				switch (value) {
					case "yes":
						this.omitXmlDeclaration = true;
						break;
					case "no":
						this.omitXmlDeclaration = false;
						break;
					default:
						if (stylesheetVersion != "1.0")
							break;

						IXmlLineInfo li = nav as IXmlLineInfo;
						throw new XsltCompileException (new XsltException (
							"'" + value + "' is an invalid value for 'omit-xml-declaration'" +
							" attribute.", (Exception) null),
							nav.BaseURI,
							li != null ? li.LineNumber : 0,
							li != null ? li.LinePosition : 0);
				}
				break;
			case "indent":
				indent = value;
				if (stylesheetVersion != "1.0")
					break;
				switch (value) {
				case "yes":
				case "no":
					break;
				default:
					switch (method) {
					case OutputMethod.Custom:
						break;
					default:
						throw new XsltCompileException (String.Format ("Unexpected 'indent' attribute value in 'output' element: '{0}'", value), null, nav);
					}
					break;
				}
				break;
			default:
				if (stylesheetVersion != "1.0")
					break;

				IXmlLineInfo xli = nav as IXmlLineInfo;
				throw new XsltCompileException (new XsltException (
					"'" + nav.LocalName + "' is an invalid attribute for 'output'" +
					" element.", (Exception) null),
					nav.BaseURI,
					xli != null ? xli.LineNumber : 0,
					xli != null ? xli.LinePosition : 0);
			}
		}
	}
}
