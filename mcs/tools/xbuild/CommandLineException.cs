//
// CommandLineException.cs: Represents various exceptions thrown during parsing
// command line parameters.
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

namespace Mono.XBuild.CommandLine {
	[Serializable]
	public class CommandLineException : Exception {
		int errorCode;
		
		public CommandLineException ()
			: base ("Unknown command line exception has occured.")
		{
		}
		
		public CommandLineException (string message)
			: base (message) 
		{
		}
		
		public CommandLineException (string message, int errorCode)
			: base (message)
		{
			this.errorCode = errorCode;
		}
		
		public CommandLineException (string message, Exception innerException)
			: base (message, innerException)
		{
		}
		
		public CommandLineException (string message, Exception innerException, int errorCode)
			: base (message, innerException)
		{
			this.errorCode = errorCode;
		}
		
		public CommandLineException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
			errorCode = info.GetInt32 ("ErrorCode");
		}
		
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
			info.AddValue ("ErrorCode", errorCode);
		}
		
		public int ErrorCode {
			get { return errorCode; }
		}
	}
}

#endif