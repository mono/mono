//
// LoggerException.cs: Exception class for logger.
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
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.Build.Framework {
	[Serializable]
	public class LoggerException : Exception {
	
		string errorCode;
		string helpKeyword;

		public LoggerException ()
			: base ("Logger exception has occured.")
		{
		}

		public LoggerException (string message)
			: base (message)
		{
		}

		public LoggerException (string message,
					Exception innerException)
			: base (message, innerException)
		{
		}

		public LoggerException (string message,
					Exception innerException,
					string errorCode, string helpKeyword)
			: base (message, innerException)
		{
			this.errorCode = errorCode;
			this.helpKeyword = helpKeyword;
		}

		protected LoggerException (SerializationInfo info,
					   StreamingContext context)
			: base (info, context)
		{
			errorCode = info.GetString ("errorCode");
			helpKeyword = info.GetString ("helpKeyword");
		}
		
		[SecurityPermission (SecurityAction.LinkDemand, SerializationFormatter = true)]
		public override void GetObjectData (SerializationInfo info,
						    StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ();
		
			base.GetObjectData (info, context);
			
			info.AddValue ("errorCode", errorCode);
			info.AddValue ("helpKeyword", helpKeyword);
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

#endif
