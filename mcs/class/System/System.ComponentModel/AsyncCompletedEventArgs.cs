// 
// System.Web.Services.Protocols.AsyncCompletedEventArgs.cs
//
// Author:
//   Lluis Sanchez (lluis@novell.com)
//
// Copyright (C) Novell, Inc., 2004
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

namespace System.ComponentModel 
{
	public class AsyncCompletedEventArgs : EventArgs
	{
		Exception _error;
		bool _cancelled;
		object _userState;
		
		public AsyncCompletedEventArgs (Exception error, bool cancelled, object userState)
		{
			_error = error;
			_cancelled = cancelled;
			_userState = userState;
		}
		
		protected void RaiseExceptionIfNecessary()
		{
			if (_error != null)
				throw new System.Reflection.TargetInvocationException (_error);
			else if (_cancelled)
				throw new InvalidOperationException ("The operation was cancelled");	
		}
		
		public bool Cancelled {
			get { return _cancelled; }
		}
		
		public Exception Error {
			get { return _error; }
		}
		
		public object UserState {
			get { return _userState; }
		}
	}
}

