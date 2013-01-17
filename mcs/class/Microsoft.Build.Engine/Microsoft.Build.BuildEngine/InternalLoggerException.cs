//
// InternalLoggerException.cs:
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

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Microsoft.Build.Framework;

namespace Microsoft.Build.BuildEngine {
	[Serializable]
	public sealed class InternalLoggerException : Exception {
		
		BuildEventArgs	buildEventArgs;
		string		errorCode;
		string		helpKeyword;
		
		public InternalLoggerException ()
		{
			throw new System.InvalidOperationException (
				"An InternalLoggerException can only be thrown by the MSBuild engine. " +
				"The public constructors of this class cannot be used to create an " +
				"instance of the exception.");
		}

		public InternalLoggerException (string message)
			: this ()
		{
		}

		public InternalLoggerException (string message,	Exception innerException)
			: this ()
		{
		}

		// FIXME: I made it private temporarily, later we can change it to internal (but not protected)
		private InternalLoggerException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
			buildEventArgs = (BuildEventArgs) info.GetValue ("BuildEventArgs", typeof (BuildEventArgs));
			errorCode = info.GetString ("ErrorCode");
			helpKeyword = info.GetString ("HelpKeywordPrefix");
		}

		[SecurityPermission (SecurityAction.LinkDemand, SerializationFormatter = true)]
		public override void GetObjectData (SerializationInfo info,
						    StreamingContext context)
		{
			base.GetObjectData (info, context);
			info.AddValue ("BuildEventArgs", buildEventArgs);
			info.AddValue ("ErrorCode", errorCode);
			info.AddValue ("HelpKeywordPrefix", helpKeyword);
		}

		public BuildEventArgs BuildEventArgs {
			get {
				return buildEventArgs;
			}
		}

		public string ErrorCode {
			get {
				return errorCode;
			}
		}

		public string HelpKeyword {
			get {
				return helpKeyword;
			}
		}
	}
}
