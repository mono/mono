//
// BuildEventArgs.cs: Provides data for the Microsoft.Framework.IEventSource.
// AnyEventRaised event.
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
// 
// (C) 2005 Marek Sieradzki
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

#if NET_2_0

using System;
using System.Threading;

namespace Microsoft.Build.Framework
{
	[Serializable]		
	public abstract class BuildEventArgs : System.EventArgs {
	
		string	helpKeyword;
		string	message;
		string	senderName;
		int	threadId;
		DateTime	timestamp;
		
		protected BuildEventArgs ()
			: this (null, null, null)
		{
		}

		protected BuildEventArgs (string message, string helpKeyword,
					  string senderName)
		{
			this.message = message;
			this.helpKeyword = helpKeyword;
			this.senderName = senderName;
			this.threadId = Thread.CurrentThread.GetHashCode ();
			this.timestamp = DateTime.Now;
		}

		public string HelpKeyword {
			get {
				return helpKeyword;
			}
		}

		public string Message {
			get {
				return message;
			}
		}

		public string SenderName {
			get {
				return senderName;
			}
		}
		// Gets the integer hash code value of the thread that raised
		// the event
		public int ThreadId {
			get {
				return threadId;
			}
		}
		// Time when event was fired
		public DateTime Timestamp {
			get {
				return timestamp;
			}
		}
	}
}

#endif