//
// LazyFormattedBuildEventArgs.cs
//
// Author:
//   Atsushi Enomoto <atsushi@xamarin.com>
// 
// (C) 2013 Xamarin Inc.
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

#if NET_4_0

using System;

namespace Microsoft.Build.Framework
{
	[Serializable]		
	public abstract class LazyFormattedBuildEventArgs : BuildEventArgs {

		string message, format;
		object[] args;

		protected LazyFormattedBuildEventArgs ()
		{
		}

		public LazyFormattedBuildEventArgs (string message,
				string helpKeyword, string senderName)
			: base (message, helpKeyword, senderName)

		{
			this.message = message;
		}

		public LazyFormattedBuildEventArgs (string message,
				string helpKeyword, string senderName,
				DateTime eventTimestamp, params object[] messageArgs)
			: base (message, helpKeyword, senderName, eventTimestamp)
		{
			if (messageArgs != null && messageArgs.Length > 0) {
				args = messageArgs;
				format = message;
			} else {
				this.message = message;
			}
		}

		public override string Message {
			get {
				if (message == null && format != null)
					message = string.Format (format, args);
				return message;
			}
		}
	}
}

#endif
