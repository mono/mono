// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Runtime.Serialization;


namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaException.
	/// </summary>
	[Serializable]
	public class XmlSchemaException : System.SystemException
	{
		//fields
		private int lineNumber;
		private int linePosition;
		private XmlSchemaObject sourceObj;
		private string sourceUri;

		[MonoTODO]
		protected XmlSchemaException(SerializationInfo info, StreamingContext context){}
		
		protected XmlSchemaException(string message, int lineNumber, int linePosition,
			XmlSchemaObject sourceObject, string sourceUri, Exception innerException)
			: base(message, innerException)
		{
			this.lineNumber		= lineNumber;
			this.linePosition	= linePosition;
			this.sourceObj		= sourceObject;
			this.sourceUri		= sourceUri;
		}
		protected XmlSchemaException(string message, XmlSchemaObject sourceObject,
			Exception innerException)
			: base(message, innerException)
		{
			this.lineNumber = sourceObject.LineNumber;
			this.linePosition = sourceObject.LinePosition;
			this.sourceObj	=	sourceObject;
			this.sourceUri	=	sourceObject.SourceUri;
		}

		public XmlSchemaException(string message, Exception innerException)
			: base(message,innerException){}

		// Properties
		public int LineNumber 
		{ 
			get{ return this.lineNumber;} 
		}
		public int LinePosition 
		{ 
			get{ return this.linePosition;} 
		}
		public override string Message 
		{  
			get{ return this.Message; } 
		}
		public XmlSchemaObject SourceSchemaObject 
		{
			get{ return this.sourceObj; } 
		}
		public string SourceUri 
		{ 
			get{ return this.sourceUri; } 
		}

		// Methods
		[MonoTODO]
		public override void GetObjectData(SerializationInfo info, StreamingContext context){}
	}
}
