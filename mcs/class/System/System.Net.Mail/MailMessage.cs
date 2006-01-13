//
// System.Net.Mail.MailMessage.cs
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

#if NET_2_0

using System.Collections.Specialized;
using System.Globalization;
using System.Net.Mime;
using System.Text;

namespace System.Net.Mail {
	[MonoTODO]
	public class MailMessage : IDisposable
	{
		#region Fields

		AlternateViewCollection alternateViews;
		AttachmentCollection attachments;
		MailAddressCollection bcc;
		string body;
		MailPriority priority;
		MailAddress replyTo, sender;
		DeliveryNotificationOptions deliveryNotificationOptions;
		MailAddressCollection cc;
		MailAddress from;
		NameValueCollection headers;
		MailAddressCollection to;
		string subject;
		Encoding subjectEncoding;
		ContentType bodyContentType;

		#endregion // Fields

		#region Constructors

		public MailMessage ()
		{
		}

		[MonoTODO ("FormatException")]
		public MailMessage (MailAddress from, MailAddress to)
		{
			if (from == null || to == null)
				throw new ArgumentNullException ();
			
			From = from;

			this.to = new MailAddressCollection ();
			this.to.Add (to);

			alternateViews = new AlternateViewCollection ();
			attachments = new AttachmentCollection ();
			bcc = new MailAddressCollection ();
			cc = new MailAddressCollection ();
			headers = new NameValueCollection ();

			headers.Add ("MIME-Version", "1.0");
		}

		public MailMessage (string from, string to)
			: this (new MailAddress (from), new MailAddress (to))
		{
			if (from == null || to == null)
				throw new ArgumentNullException ();
		}

		public MailMessage (string from, string to, string subject, string body)
			: this (new MailAddress (from), new MailAddress (to))
		{
			if (from == null || to == null)
				throw new ArgumentNullException ();
			Body = body;
			Subject = subject;
		}

		#endregion // Constructors

		#region Properties

		public AlternateViewCollection AlternateViews {
			get { return alternateViews; }
		}

		public AttachmentCollection Attachments {
			get { return attachments; }
		}

		public MailAddressCollection Bcc {
			get { return bcc; }
		}

		public string Body {
			get { return body; }
			set { body = value; }
		}

		internal ContentType BodyContentType {
			get {
				if (bodyContentType == null)
					bodyContentType = new ContentType ("text/plain; charset=us-ascii");
				return bodyContentType;
			}
		}

		public Encoding BodyEncoding {
			get { return Encoding.GetEncoding (BodyContentType.CharSet); }
			set { BodyContentType.CharSet = value.WebName; }
		}

		public MailAddressCollection CC {
			get { return cc; }
		}

		public DeliveryNotificationOptions DeliveryNotificationOptions {
			get { return deliveryNotificationOptions; }
			set { deliveryNotificationOptions = value; }
		}

		public MailAddress From {
			get { return from; }
			set { from = value; }
		}

		public NameValueCollection Headers {
			get { return headers; }
		}

		public bool IsBodyHtml {
			get { return String.Compare (BodyContentType.MediaType, "text/html", true, CultureInfo.InvariantCulture) == 0; }
			set {
				if (value)
					BodyContentType.MediaType = "text/html";
				else
					BodyContentType.MediaType = "text/plain";
			}
		}

		public MailPriority Priority {
			get { return priority; }
			set { priority = value; }
		}

		public MailAddress ReplyTo {
			get { return replyTo; }
			set { replyTo = value; }
		}

		public MailAddress Sender {
			get { return sender; }
			set { sender = value; }
		}

		public string Subject {
			get { return subject; }
			set { subject = value; }
		}

		public Encoding SubjectEncoding {
			get { return subjectEncoding; }
			set { subjectEncoding = value; }
		}

		public MailAddressCollection To {
			get { return to; }
		}

		#endregion // Properties

		#region Methods

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
		}

		#endregion // Methods
	}
}

#endif // NET_2_0
