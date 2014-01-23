//
// System.Xml.Xsl.XsltCompileException.cs
//
// Authors:
//  Tim Coleman (tim@timcoleman.com)
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Copyright 2002 Tim Coleman
// (C) 2003 Andreas Nahr
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.Runtime.Serialization;
using System.Xml.XPath;
using System.Security.Permissions;

namespace System.Xml.Xsl
{
	[Serializable]
	public class XsltCompileException : XsltException
	{
		#region Constructors

		public XsltCompileException ()
		{
		}

		public XsltCompileException (string message)
			: base (message)
		{
		}

		public XsltCompileException (string message, Exception innerException)
			: base (message, innerException)
		{
		}

		protected XsltCompileException (SerializationInfo info, StreamingContext context )
			: base (info, context)
		{
		}

		public XsltCompileException (Exception inner, String sourceUri, int lineNumber, int linePosition)
			: base (lineNumber != 0 ? "{0} at {1}({2},{3}). See InnerException for details." : "{0}.",
				"XSLT compile error", inner, lineNumber, linePosition, sourceUri)
		{
		}

		internal XsltCompileException (string message, Exception innerException, XPathNavigator nav)
			: base (message, innerException, nav)
		{
		}
		#endregion

		#region Methods

		[SecurityPermission (SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
		}

		#endregion
	}
}
