//
// MessageHeaderException.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2006,2009 Novell, Inc (http://www.novell.com)
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
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace System.ServiceModel {
	[Serializable]
	public class MessageHeaderException : ProtocolException
	{
		public MessageHeaderException () : this ("Message header exception") {}
		public MessageHeaderException (string message) : this (message, null) {}
		public MessageHeaderException (string message, Exception innerException) : base (message, innerException) {}
		protected MessageHeaderException (SerializationInfo info, StreamingContext context) :
			base (info, context)
		{
		}

		public MessageHeaderException (string message, bool isDuplicate)
			: this (message, null, null, isDuplicate, null)
		{
		}

		public MessageHeaderException (string message, string headerName, string ns)
			: this (message, headerName, ns, null)
		{
		}

		public MessageHeaderException (string message, string headerName, string ns, bool isDuplicate)
			: this (message, headerName, ns, isDuplicate, null)
		{
		}

		public MessageHeaderException (string message, string headerName, string ns, Exception innerException)
			: this (message, headerName, ns, false, innerException)
		{
		}

		public MessageHeaderException (string message, string headerName, string ns, bool isDuplicate, Exception innerException)
			: this (message, innerException)
		{
			HeaderName = headerName;
			HeaderNamespace = ns;
			IsDuplicate = isDuplicate;
		}

		public string HeaderName { get; private set; }
		public string HeaderNamespace { get; private set; }
		public bool IsDuplicate { get; private set; }
	}
}