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

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Mime;
using System.Text;

namespace System.Net.Mail {
	[MonoTODO]
	public class MailMessage : IDisposable
	{
		#region Fields

		Collection<Attachment> alternateViews;
		Collection<Attachment> attachments;
		Collection<MailAddress> bcc;
		string body;
		ContentType bodyContentType;
		Encoding bodyEncoding;
		Collection<MailAddress> cc;
		MailAddress from;
		NameValueCollection headers;
		Collection<MailAddress> to;
		string subject;
		Encoding subjectEncoding;

		#endregion // Fields

		#region Constructors

		public MailMessage (MailAddress from, MailAddress to)
		{
			From = from;

			this.to = new Collection<MailAddress> ();
			this.to.Add (to);

			alternateViews = new Collection<Attachment> ();
			attachments = new Collection<Attachment> ();
			bcc = new Collection<MailAddress> ();
			cc = new Collection<MailAddress> ();
			bodyContentType = new ContentType (MediaTypeNames.Application.Octet);
			headers = new NameValueCollection ();
		}

		public MailMessage (string from, string to)
			: this (new MailAddress (from), new MailAddress (to))
		{
		}

		public MailMessage (string from, string to, string subject, string body)
			: this (new MailAddress (from), new MailAddress (to))
		{
			Body = body;
			Subject = subject;
		}

		#endregion // Constructors

		#region Properties

		public Collection<Attachment> AlternateViews {
			get { return alternateViews; }
		}

		public Collection<Attachment> Attachments {
			get { return attachments; }
		}

		public Collection<MailAddress> Bcc {
			get { return bcc; }
		}

		public string Body {
			get { return body; }
			set { body = value; }
		}

		public ContentType BodyContentType {
			get { return bodyContentType; }
		}

		public Encoding BodyEncoding {
			get { return bodyEncoding; }
			set { 
				bodyEncoding = value;
				bodyContentType.CharSet = value.WebName; 
			}
		}

		public Collection<MailAddress> CC {
			get { return cc; }
		}

		public MailAddress From {
			get { return from; }
			set { from = value; }
		}

		public NameValueCollection Headers {
			get { return headers; }
		}

		public string Subject {
			get { return subject; }
			set { subject = value; }
		}

		public Encoding SubjectEncoding {
			get { return subjectEncoding; }
			set { subjectEncoding = value; }
		}

		public Collection<MailAddress> To {
			get { return to; }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public void Dispose ()
		{
		}

		[MonoTODO]
		~MailMessage ()
		{
		}

		#endregion // Methods
	}
}

#endif // NET_2_0
