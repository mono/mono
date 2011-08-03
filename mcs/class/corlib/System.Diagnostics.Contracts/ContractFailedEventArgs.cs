//
// System.Diagnostics.Contracts.ContractFailedEventArgs.cs
//
// Authors:
//    Miguel de Icaza (miguel@gnome.org)
//
// Copyright 2009 Novell (http://www.novell.com)
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

#if NET_4_0 || MOONLIGHT

using System;
using System.Runtime.ConstrainedExecution;

namespace System.Diagnostics.Contracts {

	public sealed class ContractFailedEventArgs : EventArgs {
		
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		public ContractFailedEventArgs (ContractFailureKind failureKind, string message, string condition, Exception originalException)
		{
			Condition = condition;
			FailureKind = failureKind;
			Handled = false;
			Unwind = false; // MS docs are incorrect - this should default to false.
			Message = message;
			OriginalException = originalException;
		}

		public void SetHandled ()
		{
			Handled = true;
		}

		public void SetUnwind ()
		{
			Unwind = true;
		}

		public string Condition { get; private set; }
		public ContractFailureKind FailureKind { get; private set; }
		public bool Handled { get; private set; }
		public bool Unwind { get; private set; }
		public string Message { get; private set; }
		public Exception OriginalException { get; private set; }
	}
}

#endif
