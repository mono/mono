//
// System.Net.Mail.SmtpException.cs
//
// Author:
//	Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2004
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

using System.Runtime.Serialization;

namespace System.Net.Mail {
	[Serializable]
	public class SmtpException : Exception, ISerializable
	{
		#region Fields

		SmtpStatusCode statusCode;

		#endregion // Fields

		#region Constructors

		public SmtpException ()
			: this (SmtpStatusCode.GeneralFailure)
		{
		}

		public SmtpException (SmtpStatusCode statusCode)
			: this (statusCode, "Syntax error, command unrecognized.")
		{
		}

		public SmtpException (string message)
			: this (SmtpStatusCode.GeneralFailure, message)
		{
		}

		protected SmtpException (SerializationInfo serializationInfo, StreamingContext streamingContext)
			: base (serializationInfo, streamingContext)
		{
			try {
				statusCode = (SmtpStatusCode) serializationInfo.GetValue ("Status", typeof (int));
			} catch (SerializationException) {
				//For compliance with previously serialized version:
				statusCode = (SmtpStatusCode) serializationInfo.GetValue ("statusCode", typeof (SmtpStatusCode));
			}
		}

		public SmtpException (SmtpStatusCode statusCode, string message)
			: base (message)
		{
			this.statusCode = statusCode;
		}

		public SmtpException (string message, Exception innerException)
			: base (message, innerException)
		{
			statusCode = SmtpStatusCode.GeneralFailure;
		}

		#endregion // Constructors

		#region Properties

		public SmtpStatusCode StatusCode {
			get { return statusCode; }
			set { statusCode = value; }
		}

		#endregion // Properties

		public override void GetObjectData (SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
			if (serializationInfo == null)
				throw new ArgumentNullException ("serializationInfo");
			base.GetObjectData (serializationInfo, streamingContext);
			serializationInfo.AddValue ("Status", statusCode, typeof (int));
		}
		void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context)
		{
			GetObjectData (info, context);
		}
	}
}

