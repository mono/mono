//
// XmlSchemaValidationException.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C)2004 Novell Inc.
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

#if NET_2_0

using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Xml.Schema
{
	[Serializable]
	public class XmlSchemaValidationException : XmlSchemaException
	{
		object source_object;

		public XmlSchemaValidationException ()
			: base ()
		{
		}

		public XmlSchemaValidationException (string message)
			: base (message)
		{
		}

		protected XmlSchemaValidationException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}

		public XmlSchemaValidationException (string message, Exception innerException, int lineNumber, int linePosition)
			: base (message, lineNumber, linePosition, null, null, innerException)
		{
		}

		internal XmlSchemaValidationException (string message, int lineNumber, int linePosition,
			XmlSchemaObject sourceObject, string sourceUri, Exception innerException)
			: base (message, lineNumber, linePosition, sourceObject, sourceUri, innerException)
		{
		}

		internal XmlSchemaValidationException (string message, object sender,
			string sourceUri, XmlSchemaObject sourceObject, Exception innerException)
			: base (message, sender, sourceUri, sourceObject, innerException)
		{
		}

		internal XmlSchemaValidationException (string message, XmlSchemaObject sourceObject,
			Exception innerException)
			: base (message, sourceObject, innerException)
		{
		}

		public XmlSchemaValidationException (string message, Exception innerException)
			: base (message, innerException)
		{
		}

		[SecurityPermission (SecurityAction.LinkDemand,
			Flags=SecurityPermissionFlag.SerializationFormatter)]
		public override void GetObjectData (
			SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
		}

		// Never use it. It actually does nothing even on MS.NET 2.0.
		protected internal void SetSourceObject (object sourceObject)
		{
			this.source_object = sourceObject;
		}

		public object SourceObject {
			get { return source_object; }
		}
	}
}

#endif
