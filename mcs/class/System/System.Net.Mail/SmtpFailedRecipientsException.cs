//
// System.Net.Mail.SmtpFailedRecipientsException.cs
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

using System.Collections;
using System.Runtime.Serialization;

namespace System.Net.Mail {
	[Serializable]
	public class SmtpFailedRecipientsException : SmtpFailedRecipientException, ISerializable
	{
		#region Fields

		SmtpFailedRecipientException[] innerExceptions;

		#endregion // Fields

		#region Constructors

		public SmtpFailedRecipientsException ()
		{
		}

		public SmtpFailedRecipientsException (string message) : base (message)
		{
		}

		public SmtpFailedRecipientsException (string message, Exception innerException) : base (message, innerException)
		{
		}

		public SmtpFailedRecipientsException (string message, SmtpFailedRecipientException[] innerExceptions) : base (message)
		{
			this.innerExceptions = innerExceptions;
		}

		protected SmtpFailedRecipientsException (SerializationInfo info, StreamingContext context)
		: base(info, context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");
			innerExceptions = (SmtpFailedRecipientException []) info.GetValue ("innerExceptions", typeof (SmtpFailedRecipientException []));
		}
		
		#endregion

		#region Properties

		public SmtpFailedRecipientException[] InnerExceptions {
			get { return innerExceptions; }
		}

		#endregion // Properties

		#region Methods

		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");
			base.GetObjectData (info, context);
			info.AddValue ("innerExceptions", innerExceptions);
		}

#if !TARGET_JVM //remove private implementation
		void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context)
		{
			GetObjectData (info, context);
		}
#endif

		#endregion // Methods
	}
}

