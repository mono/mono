
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
