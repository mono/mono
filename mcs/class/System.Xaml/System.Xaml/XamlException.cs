//
// Copyright (C) 2010 Novell Inc. http://novell.com
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
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace System.Xaml
{
	[Serializable]
	public class XamlException : Exception
	{
		public XamlException ()
			: this ("XAML error")
		{
		}

		public XamlException (string message)
			: this (message, null)
		{
		}

		public XamlException (string message, Exception innerException)
			: this (message, null, 0, 0)
		{
		}

		static string FormatLine (string message, int lineNumber, int linePosition)
		{
			if (lineNumber <= 0)
				return message;
			if (linePosition <= 0)
				return String.Format ("{0} at line {1}", message, lineNumber);
			return String.Format ("{0} at line {1}, position {2}", message, lineNumber, linePosition);
		}

		public XamlException (string message, Exception innerException, int lineNumber, int linePosition)
			: base (message, innerException)
		{
			LineNumber = lineNumber;
			LinePosition = linePosition;
		}

		protected XamlException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
			LineNumber = info.GetInt32 ("lineNumber");
			LinePosition = info.GetInt32 ("linePosition");
		}

		public int LineNumber { get; protected internal set; }
		public int LinePosition { get; protected internal set; }
		public override string Message {
			get { return FormatLine (base.Message, LineNumber, LinePosition); }
		}

		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			info.AddValue ("lineNumber", LineNumber);
			info.AddValue ("linePosition", LinePosition);
		}
	}
}
