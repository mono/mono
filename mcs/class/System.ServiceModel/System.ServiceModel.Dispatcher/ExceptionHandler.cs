//
// ExceptionHandler.cs
//
// Author: Duncan Mak (duncan@novell.com)
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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
using System.Runtime.ConstrainedExecution;

namespace System.ServiceModel.Dispatcher {

	public abstract class ExceptionHandler
	{
		static ExceptionHandler async_handler;
		static ExceptionHandler always_handler = new AlwaysHandler ();
		static ExceptionHandler transport_handler = new AlwaysHandler ();

		protected ExceptionHandler () {}

		public static ExceptionHandler AlwaysHandle {
			get { return always_handler; }
		}

		class AlwaysHandler : ExceptionHandler
		{
			public AlwaysHandler () {}

			public override bool HandleException (Exception e)
			{
				return true;
			}
		}

		public static ExceptionHandler AsynchronousThreadExceptionHandler {

			[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
			get { return async_handler; }

			set { async_handler = value; }
		}

		public static ExceptionHandler TransportExceptionHandler {
			get { return transport_handler; }
			set { transport_handler = value; }
		}

		public abstract bool HandleException (Exception exception);
	}
}