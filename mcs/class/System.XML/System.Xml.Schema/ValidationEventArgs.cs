// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for ValidationEventArgs.
	/// </summary>
	public sealed class ValidationEventArgs : EventArgs
	{
		private XmlSchemaException exception;
		private string message;
		private XmlSeverityType severity;

		private ValidationEventArgs()
		{}

		internal ValidationEventArgs(XmlSchemaException ex, string message, XmlSeverityType severity)
		{
			this.exception = ex;
			this.message = message;
			this.severity = severity;
		}
		public XmlSchemaException Exception 
		{
			get{ return exception; }
		}
		public string Message 
		{
			get{ return message; }
		}
		public XmlSeverityType Severity 
		{
			get{ return severity; }
		}
	}
}
