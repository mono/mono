//
// XmlSchemaInferenceException.cs
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
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Xml;

namespace System.Xml.Schema
{
	[Serializable]
	public class XmlSchemaInferenceException : XmlSchemaException
	{
		public XmlSchemaInferenceException ()
			: base ()
		{
		}

		public XmlSchemaInferenceException (string message)
			: base (message)
		{
		}

		protected XmlSchemaInferenceException (
			SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}

		public XmlSchemaInferenceException (string message,
			Exception innerException)
			: base (message, innerException)
		{
		}

		public XmlSchemaInferenceException (string message,
			Exception innerException, int lineNumber, int linePosition)
			: base (message, innerException, lineNumber, linePosition)
		{
		}

#if NET_2_0
		[SecurityPermission (SecurityAction.LinkDemand,
			Flags=SecurityPermissionFlag.SerializationFormatter)]
#endif
		public override void GetObjectData (
			SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
		}
	}
}

#endif
