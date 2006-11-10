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

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for ValidationEventArgs.
	/// </summary>
#if NET_2_0
	public class ValidationEventArgs : EventArgs
#else
	public sealed class ValidationEventArgs : EventArgs
#endif
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
