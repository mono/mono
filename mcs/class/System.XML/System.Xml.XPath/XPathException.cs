//
// System.Xml.XPath.XPathException
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Copyright 2002 Tim Coleman
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
using System.Security.Permissions;

namespace System.Xml.XPath
{
	[Serializable]
	public class XPathException : SystemException
	{
#region Constructors

#if NET_2_0
		public XPathException ()
			: base ("")
		{
		}
#endif

		protected XPathException (SerializationInfo info, StreamingContext context) : base (info, context) {}

		public XPathException (string message, Exception innerException) : base (message, innerException) {}

#if NET_2_0
		public XPathException (string message)
#else
		internal XPathException (string message)
#endif
			: base (message, null)
		{
		}

#endregion

#region Properties

		public override string Message {
			get { return base.Message; }
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
