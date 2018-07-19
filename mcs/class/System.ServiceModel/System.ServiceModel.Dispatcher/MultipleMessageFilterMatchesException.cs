//
// System.ServiceModel.MultipleFilterMatchesException.cs
//
// Author: Duncan Mak (duncan@novell.com)
//
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

using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace System.ServiceModel.Dispatcher
{
	[Serializable]
	public class MultipleFilterMatchesException : SystemException
	{
		Collection<MessageFilter> filters;

		public MultipleFilterMatchesException () : base () {}
		public MultipleFilterMatchesException (string message) : base (message) {}
		public MultipleFilterMatchesException (string message, Exception innerException) : base (message, innerException) {}		
		protected MultipleFilterMatchesException (SerializationInfo info, StreamingContext context)
			: base (info, context) {}

		public MultipleFilterMatchesException (string message, Collection<MessageFilter> filters)
			: base (message)
		{
			this.filters = filters;
		}

		public MultipleFilterMatchesException (string message, Exception innerException, Collection<MessageFilter> filters)
			: base (message, innerException)
		{
			this.filters = filters;
		}
		
		public Collection<MessageFilter> Filters { get { return filters; } }
	}
}