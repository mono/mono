//
// System.Xml.Schema.XmlSchemaObject.cs
//
// Authors:
//	Dwivedi, Ajay kumar  Adwiv@Yahoo.com
//	Enomoto, Atsushi     ginga@kit.hi-ho.ne.jp
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
		internal Guid CompilationId;
		internal Guid ValidationId;
		internal bool isRedefineChild;
		internal bool isRedefinedComponent;
		internal XmlSchemaObject redefinedObject;

		internal bool AttributeGroupRecursionCheck;

		protected XmlSchemaObject()
		{
			namespaces = new XmlSerializerNamespaces();
			unhandledAttributeList = null;
			CompilationId = Guid.Empty;
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

		internal bool IsComplied (Guid compilationId)
		{
			return this.CompilationId == compilationId;
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