//
// System.Xml.Schema.XmlSchemaObject.cs
//
// Authors:
//	Dwivedi, Ajay kumar  Adwiv@Yahoo.com
//	Enomoto, Atsushi     ginga@kit.hi-ho.ne.jp
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
using System.Xml.Serialization;
using System.Xml;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaObject.
	/// </summary>
	public abstract class XmlSchemaObject
	{
		private int lineNumber;
		private int linePosition;
		private string sourceUri;
		private XmlSerializerNamespaces namespaces;
		internal ArrayList unhandledAttributeList ;
		internal bool isCompiled = false;
		internal int errorCount = 0;
		internal Guid compilation_id;
		internal Guid ValidationId;
		internal bool isRedefineChild;
		internal bool isRedefinedComponent;
		internal XmlSchemaObject redefinedObject;

		private XmlSchemaObject parent;


		protected XmlSchemaObject()
		{
			namespaces = new XmlSerializerNamespaces();
			unhandledAttributeList = null;
			compilation_id = Guid.Empty;
		}
		
		internal Guid CompilationId {
			get { return compilation_id; }
			set {
				if (value.Equals (Guid.Empty))
					throw new ArgumentException ("value must not be empty");
				compilation_id = value;
			}
		}

		[XmlIgnore]
		public int LineNumber 
		{ 
			get{ return lineNumber; } 
			set{ lineNumber = value; } 
		}
		[XmlIgnore]
		public int LinePosition 
		{ 
			get{ return linePosition; } 
			set{ linePosition = value; } 
		}
		[XmlIgnore]
		public string SourceUri 
		{ 
			get{ return sourceUri; } 
			set{ sourceUri = value; } 
		}

#if NET_2_0
		[XmlIgnore]
		public
#else
		internal
#endif
		XmlSchemaObject Parent {
			get { return parent; }
			set { parent = value; }
		}

		internal XmlSchema AncestorSchema {
			get {
				
				for (XmlSchemaObject o = Parent; o != null; o = o.Parent)
					if (o is XmlSchema)
						return (XmlSchema) o;
				throw new Exception (String.Format ("INTERNAL ERROR: Parent object is not set properly : {0} ({1},{2})", SourceUri, LineNumber, LinePosition));
			}
		}

		internal virtual void SetParent (XmlSchemaObject parent)
		{
			Parent = parent;
		}

		// Undocumented Property
		[XmlNamespaceDeclarations]
		public XmlSerializerNamespaces Namespaces 
		{ 
			get{ return namespaces; } 
			set{ namespaces = value; } 
		}

		internal void error(ValidationEventHandler handle,string message)
		{
			errorCount++;
			error (handle, message, null, this, null);
		}
		internal void warn(ValidationEventHandler handle,string message)
		{
			warn (handle, message, null, this, null);
		}
		internal static void error(ValidationEventHandler handle, string message, Exception innerException)
		{
			error (handle, message, innerException, null, null);
		}
		internal static void warn(ValidationEventHandler handle, string message, Exception innerException)
		{
			warn (handle, message, innerException, null, null);
		}
		internal static void error(ValidationEventHandler handle,
			string message,
			Exception innerException,
			XmlSchemaObject xsobj,
			object sender)
		{
			ValidationHandler.RaiseValidationEvent (handle,
				innerException,
				message,
				xsobj,
				sender,
				null,
				XmlSeverityType.Error);
		}
		internal static void warn(ValidationEventHandler handle,
			string message,
			Exception innerException,
			XmlSchemaObject xsobj,
			object sender)
		{
			ValidationHandler.RaiseValidationEvent (handle,
				innerException,
				message,
				xsobj,
				sender,
				null,
				XmlSeverityType.Warning);
		}

		internal virtual int Compile (ValidationEventHandler h, XmlSchema schema)
		{
			return 0;
		}

		internal virtual int Validate (ValidationEventHandler h, XmlSchema schema)
		{
			return 0;
		}

		internal bool IsValidated (Guid validationId)
		{
			return this.ValidationId == validationId;
		}

		// This method is used only by particles
		internal virtual void CopyInfo (XmlSchemaParticle obj)
		{
			obj.LineNumber = LineNumber;
			obj.LinePosition = LinePosition;
			obj.SourceUri = SourceUri;
			obj.errorCount = errorCount;
			// Other fields and properties may be useless for Particle.
		}
	}
}
