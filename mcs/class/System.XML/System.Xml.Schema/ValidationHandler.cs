using System;

namespace System.Xml.Schema
{
	/// <summary>
	/// </summary>
	public delegate void ValidationEventHandler(object sender,ValidationEventArgs e);

	/// <summary>
	/// Docs say we need to raise an exception if ValidationEventHandler is not set(null)
	/// So we use this class to raise the events rather than calling the delegate by itself
	/// </summary>
	internal class ValidationHandler
	{
		public static void RaiseValidationEvent(ValidationEventHandler handle,
			Exception innerException,
			string message,
			XmlSchemaObject xsobj,
			object sender,
			string sourceUri,
			XmlSeverityType severity)
		{
			XmlSchemaException ex = new XmlSchemaException (
				message, sender, sourceUri, xsobj, innerException);
			ValidationEventArgs e = new ValidationEventArgs(ex,message,severity);
			if(handle == null)
			{
				if (e.Severity == XmlSeverityType.Error)
					throw e.Exception;
			}
			else
			{
				handle(sender,e);
			}
		}

		/*
		public static void RaiseValidationEvent(ValidationEventHandler handle, Exception innerException,  object sender, string message, XmlSeverityType severity)
		{
			RaiseValidationEvent(handle,null,sender,message,XmlSeverityType.Error);
		}

		public static void RaiseValidationError(ValidationEventHandler handle, object sender, string message)
		{
			RaiseValidationEvent(handle,null,sender,message,XmlSeverityType.Error);
		}
		
		public static void RaiseValidationError(ValidationEventHandler handle, string message, Exception innerException)
		{
			RaiseValidationEvent(handle, innerException, null, message, XmlSeverityType.Error);
		}

		public static void RaiseValidationWarning (ValidationEventHandler handle, object sender, string message)
		{
			RaiseValidationEvent(handle,null,sender,message,XmlSeverityType.Warning);
		}

		public static void RaiseValidationWarning(ValidationEventHandler handle, string message, Exception innerException)
		{
			RaiseValidationEvent(handle, innerException, null, message, XmlSeverityType.Warning);
		}
		*/
	}
}
