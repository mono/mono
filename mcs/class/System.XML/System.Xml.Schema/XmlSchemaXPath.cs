// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com

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
using System.Xml.Serialization;
using System.ComponentModel;
using System.Xml;
using Mono.Xml;
using Mono.Xml.Schema;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaXPath.
	/// </summary>
	public class XmlSchemaXPath : XmlSchemaAnnotated
	{
		private string xpath;
		XmlNamespaceManager nsmgr;
		internal bool isSelector;
		XsdIdentityPath [] compiledExpression;

		XsdIdentityPath currentPath;

		public XmlSchemaXPath()
		{
		}
		[DefaultValue("")]
		[System.Xml.Serialization.XmlAttribute("xpath")]
		public string XPath 
		{
			get{ return  xpath; } 
			set{ xpath = value; }
		}

		internal override int Compile(ValidationEventHandler h, XmlSchema schema)
		{
			// If this is already compiled this time, simply skip.
			if (CompilationId == schema.CompilationId)
				return 0;

			if (nsmgr == null) {
				nsmgr = new XmlNamespaceManager (new NameTable ());
				if (Namespaces != null)
					foreach (XmlQualifiedName qname in Namespaces.ToArray ())
						nsmgr.AddNamespace (qname.Name, qname.Namespace);
			}

			currentPath = new XsdIdentityPath ();
			ParseExpression (xpath, h, schema);

			XmlSchemaUtil.CompileID(Id, this, schema.IDCollection, h);
			this.CompilationId = schema.CompilationId;
			return errorCount;
		}

		internal XsdIdentityPath [] CompiledExpression {
			get { return compiledExpression; }
		}

		private void ParseExpression (string xpath, ValidationEventHandler h, XmlSchema schema)
		{
			ArrayList paths = new ArrayList ();
			ParsePath (xpath, 0, paths, h, schema);
			this.compiledExpression = (XsdIdentityPath []) paths.ToArray (typeof (XsdIdentityPath));
		}

		private void ParsePath (string xpath, int pos, ArrayList paths,
			ValidationEventHandler h, XmlSchema schema)
		{
			pos = SkipWhitespace (xpath, pos);
			if (xpath.Length >= pos + 3 && xpath [pos] == '.') {
				int tmp = pos;
				pos++;
				pos = SkipWhitespace (xpath, pos);
				if (xpath.Length > pos + 2 && xpath.IndexOf ("//", pos, 2) == pos) {
					currentPath.Descendants = true;
					pos += 2;
				}
				else
					pos = tmp;	// revert
			}
			ArrayList al = new ArrayList ();
			ParseStep (xpath, pos, al, paths, h, schema);
		}

		private void ParseStep (string xpath, int pos, ArrayList steps,
			ArrayList paths, ValidationEventHandler h, XmlSchema schema)
		{
			pos = SkipWhitespace (xpath, pos);
			if (xpath.Length == pos) {
				error (h, "Empty xpath expression is specified");
				return;
			}

			XsdIdentityStep step = new XsdIdentityStep ();
			switch (xpath [pos]) {
			case '@':
				if (isSelector) {
					error (h, "Selector cannot include attribute axes.");
					currentPath = null;
					return;
				}
				pos++;
				step.IsAttribute = true;
				pos = SkipWhitespace (xpath, pos);
				if (xpath.Length > pos && xpath [pos] == '*') {
					pos++;
					step.IsAnyName = true;
					break;
				}
				goto default;
			case '.':
				pos++;	// do nothing ;-)
				step.IsCurrent = true;
				break;
			case '*':
				pos++;
				step.IsAnyName = true;
				break;
			case 'c':
				if (xpath.Length > pos + 5 && xpath.IndexOf ("child", pos, 5) == pos) {
					int tmp = pos;
					pos += 5;
					pos = SkipWhitespace (xpath, pos);
					if (xpath.Length > pos && xpath [pos] == ':' && xpath [pos+1] == ':') {
						pos += 2;
						if (xpath.Length > pos && xpath [pos] == '*') {
							pos++;
							step.IsAnyName = true;
							break;
						}
						pos = SkipWhitespace (xpath, pos);
					}
					else
						pos = tmp;
				}
				goto default;
			case 'a':
				if (xpath.Length > pos + 9 && xpath.IndexOf ("attribute", pos, 9) == pos) {
					int tmp = pos;
					pos += 9;
					pos = SkipWhitespace (xpath, pos);
					if (xpath.Length > pos && xpath [pos] == ':' && xpath [pos+1] == ':') {
						if (isSelector) {
							error (h, "Selector cannot include attribute axes.");
							currentPath = null;
							return;
						}
						pos += 2;
						step.IsAttribute = true;
						if (xpath.Length > pos && xpath [pos] == '*') {
							pos++;
							step.IsAnyName = true;
							break;
						}
						pos = SkipWhitespace (xpath, pos);
					}
					else
						pos = tmp;
				}
				goto default;
			default:
				int nameStart = pos;
				while (xpath.Length > pos) {
					if (!XmlChar.IsNCNameChar (xpath [pos]))
						break;
					else
						pos++;
				}
				if (pos == nameStart) {
					error (h, "Invalid path format for a field.");
					this.currentPath = null;
					return;
				}
				if (xpath.Length == pos || xpath [pos] != ':')
					step.Name = xpath.Substring (nameStart, pos - nameStart);
				else {
					string prefix = xpath.Substring (nameStart, pos - nameStart);
					pos++;
					if (xpath.Length > pos && xpath [pos] == '*') {
						string ns = nsmgr.LookupNamespace (prefix, false);
						if (ns == null) {
							error (h, "Specified prefix '" + prefix + "' is not declared.");
							this.currentPath = null;
							return;
						}
						step.NsName = ns;
						pos++;
					} else {
						int localNameStart = pos;
						while (xpath.Length > pos) {
							if (!XmlChar.IsNCNameChar (xpath [pos]))
								break;
							else
								pos++;
						}
						step.Name = xpath.Substring (localNameStart, pos - localNameStart);
						string ns = nsmgr.LookupNamespace (prefix, false);
						if (ns == null) {
							error (h, "Specified prefix '" + prefix + "' is not declared.");
							this.currentPath = null;
							return;
						}
						step.Namespace = ns;
					}
				}
				break;
			}
			if (!step.IsCurrent)	// Current step is meaningless, other than its representation.
				steps.Add (step);
			pos = SkipWhitespace (xpath, pos);
			if (xpath.Length == pos) {
				currentPath.OrderedSteps = (XsdIdentityStep []) steps.ToArray (typeof (XsdIdentityStep));
				paths.Add (currentPath);
				return;
			}
			else if (xpath [pos] == '/') {
				pos++;
				if (step.IsAttribute) {
					error (h, "Unexpected xpath token after Attribute NameTest.");
					this.currentPath = null;
					return;
				}
				this.ParseStep (xpath, pos, steps, paths, h, schema);
				if (currentPath == null) // For ValidationEventHandler
					return;
				currentPath.OrderedSteps = (XsdIdentityStep []) steps.ToArray (typeof (XsdIdentityStep));
			} else if (xpath [pos] == '|') {
				pos++;
				currentPath.OrderedSteps = (XsdIdentityStep []) steps.ToArray (typeof (XsdIdentityStep));
				paths.Add (this.currentPath);
				this.currentPath = new XsdIdentityPath ();
				this.ParsePath (xpath, pos, paths, h, schema);
			} else {
				error (h, "Unexpected xpath token after NameTest.");
				this.currentPath = null;
				return;
			}
		}

		private int SkipWhitespace (string xpath, int pos)
		{
			bool loop = true;
			while (loop && xpath.Length > pos) {
				switch (xpath [pos]) {
				case ' ':
				case '\t':
				case '\r':
				case '\n':
					pos++;
					continue;
				default:
					loop = false;
					break;
				}
			}
			return pos;
		}

		//<selector 
		//  id = ID 
		//  xpath = a subset of XPath expression, see below 
		//  {any attributes with non-schema namespace . . .}>
		//  Content: (annotation?)
		//</selector>
		internal static XmlSchemaXPath Read(XmlSchemaReader reader, ValidationEventHandler h,string name)
		{
			XmlSchemaXPath path = new XmlSchemaXPath();
			reader.MoveToElement();

			if(reader.NamespaceURI != XmlSchema.Namespace || reader.LocalName != name)
			{
				error(h,"Should not happen :1: XmlSchemaComplexContentRestriction.Read, name="+reader.Name,null);
				reader.Skip();
				return null;
			}

			path.LineNumber = reader.LineNumber;
			path.LinePosition = reader.LinePosition;
			path.SourceUri = reader.BaseURI;

			XmlNamespaceManager currentMgr = XmlSchemaUtil.GetParserContext (reader.Reader).NamespaceManager;
			if (currentMgr != null) {
				path.nsmgr = new XmlNamespaceManager (reader.NameTable);
				IEnumerator e = currentMgr.GetEnumerator ();
				while (e.MoveNext ()) {
					string prefix = e.Current as string;
					switch (prefix) {
					case "xml":
					case "xmlns":
						continue;
					default:
						path.nsmgr.AddNamespace (prefix, currentMgr.LookupNamespace (prefix, false));
						break;
					}
				}
			}

			while(reader.MoveToNextAttribute())
			{
				if(reader.Name == "id")
				{
					path.Id = reader.Value;
				}
				else if(reader.Name == "xpath")
				{
					path.xpath = reader.Value;
				}
				else if((reader.NamespaceURI == "" && reader.Name != "xmlns") || reader.NamespaceURI == XmlSchema.Namespace)
				{
					error(h,reader.Name + " is not a valid attribute for "+name,null);
				}
				else
				{
					XmlSchemaUtil.ReadUnhandledAttribute(reader,path);
				}
			}

			reader.MoveToElement();	
			if(reader.IsEmptyElement)
				return path;

			//  Content: (annotation?)
			int level = 1;
			while(reader.ReadNextElement())
			{
				if(reader.NodeType == XmlNodeType.EndElement)
				{
					if(reader.LocalName != name)
						error(h,"Should not happen :2: XmlSchemaXPath.Read, name="+reader.Name,null);
					break;
				}
				if(level <= 1 && reader.LocalName == "annotation")
				{
					level = 2;	//Only one annotation
					XmlSchemaAnnotation annotation = XmlSchemaAnnotation.Read(reader,h);
					if(annotation != null)
						path.Annotation = annotation;
					continue;
				}
				reader.RaiseInvalidElementError();
			}
			return path;
		}

	}
}
