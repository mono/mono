// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
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

		protected XmlSchemaObject()
		{
			namespaces = new XmlSerializerNamespaces();
			unhandledAttributeList = null;
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
			ValidationHandler.RaiseValidationError(handle,this,message);
		}
		internal void warn(ValidationEventHandler handle,string message)
		{
			errorCount++;
			ValidationHandler.RaiseValidationWarning(handle,this,message);
		}
		internal static void error(ValidationEventHandler handle, string message, Exception innerException)
		{
			ValidationHandler.RaiseValidationError(handle,message, innerException);
		}
		internal static void warn(ValidationEventHandler handle, string message, Exception innerException)
		{
			ValidationHandler.RaiseValidationWarning(handle,message, innerException);
		}

		internal bool IsComplied (Guid CompilationId)
		{
			return this.CompilationId == CompilationId;
		}
	}
}