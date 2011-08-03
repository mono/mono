//
// DataServiceException.cs
//
// Author:
//   Eric Maupin  <me@ermau.com>
//
// Copyright (c) 2009 Eric Maupin (http://www.ermau.com)
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

using System.Diagnostics;
using System.Runtime.Serialization;
using System.Security;

namespace System.Data.Services
{
	[DebuggerDisplay ("{statusCode}: {Message}")]
	[Serializable]
	public sealed class DataServiceException : InvalidOperationException
	{
		public DataServiceException()
		{
		}

		public DataServiceException (string message)
			: base (message)
		{
		}

		public DataServiceException (int statusCode, string message)
			: base (message)
		{
			this.StatusCode = statusCode;
		}

		public DataServiceException (string message, Exception innerException)
			: base (message, innerException)
		{
		}

		public DataServiceException (int statusCode, string errorCode, string message, string messageXmlLang, Exception innerException)
			: base (message, innerException)
		{
			this.StatusCode = statusCode;
			this.ErrorCode = errorCode;
			this.MessageLanguage = messageXmlLang;
		}

		protected DataServiceException (SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
			throw new NotImplementedException();
		}

		public int StatusCode {
			get; private set;
		}

		public string ErrorCode {
			get; private set;
		}

		public string MessageLanguage {
			get; private set;
		}

		[SecurityCritical]
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException();
		}
	}
}