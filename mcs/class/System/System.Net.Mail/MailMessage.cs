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

using System.Collections.Specialized;
using System.Globalization;
using System.Net.Mime;
using System.Text;

namespace System.Net.Mail {
	public class MailMessage : IDisposable
	{
		#region Fields

		AlternateViewCollection alternateViews;
		AttachmentCollection attachments;
		MailAddressCollection bcc;
		MailAddressCollection replyTo;		
		string body;
		MailPriority priority;
		MailAddress sender;
		DeliveryNotificationOptions deliveryNotificationOptions;
		MailAddressCollection cc;
		MailAddress from;
		NameValueCollection headers;
		MailAddressCollection to;
		string subject;
		Encoding subjectEncoding, bodyEncoding, headersEncoding = Encoding.UTF8;
		bool isHtml;

		#endregion // Fields

		#region Constructors

		public MailMessage () {
			this.to = new MailAddressCollection ();

			alternateViews = new AlternateViewCollection ();
			attachments = new AttachmentCollection ();
			bcc = new MailAddressCollection ();
			cc = new MailAddressCollection ();
			replyTo = new MailAddressCollection ();
			headers = new NameValueCollection ();

			headers.Add ("MIME-Version", "1.0");
		}

		// FIXME: should it throw a FormatException if the addresses are wrong? 
		// (How is it possible to instantiate such a malformed MailAddress?)
		public MailMessage (MailAddress from, MailAddress to) : this ()
		{
			if (from == null || to == null)
				throw new ArgumentNullException ();

			From = from;

			this.to.Add (to);
		}

		public MailMessage (string from, string to) : this ()
		{
			if (from == null || from == String.Empty)
				throw new ArgumentNullException ("from");
			if (to == null || to == String.Empty)
				throw new ArgumentNullException ("to");
			
			this.from = new MailAddress (from);
			foreach (string recipient in to.Split (new char [] {','}))
				this.to.Add (new MailAddress (recipient.Trim ()));
		}

		public MailMessage (string from, string to, string subject, string body) : this ()
		{
			if (from == null || from == String.Empty)
				throw new ArgumentNullException ("from");
			if (to == null || to == String.Empty)
				throw new ArgumentNullException ("to");
			
			this.from = new MailAddress (from);
			foreach (string recipient in to.Split (new char [] {','}))
				this.to.Add (new MailAddress (recipient.Trim ()));

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
			set {
				// autodetect suitable body encoding (ASCII or UTF-8), if it is not initialized yet.
				if (value != null && bodyEncoding == null)
					bodyEncoding = GuessEncoding (value) ?? Encoding.ASCII;
				body = value;
			}
		}

		internal ContentType BodyContentType {
			get {
				ContentType ct = new ContentType (isHtml ? "text/html" : "text/plain");
				ct.CharSet = (BodyEncoding ?? Encoding.ASCII).HeaderName;
				return ct;
			}
		}

		internal TransferEncoding ContentTransferEncoding {
			get { return GuessTransferEncoding (BodyEncoding); }
		}

		public Encoding BodyEncoding {
			get { return bodyEncoding; }
			set { bodyEncoding = value; }
		}

		public TransferEncoding BodyTransferEncoding {
			get { return GuessTransferEncoding (BodyEncoding); }
			set { throw new NotImplementedException (); }
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
			get { return isHtml; }
			set { isHtml = value; }
		}

		public MailPriority Priority {
			get { return priority; }
			set { priority = value; }
		}

		public
		Encoding HeadersEncoding {
			get { return headersEncoding; }
			set { headersEncoding = value; } 
		}

		public
		MailAddressCollection ReplyToList {
			get { return replyTo; }
		}

		[Obsolete ("Use ReplyToList instead")]
		public MailAddress ReplyTo {
			get {
				if (replyTo.Count == 0)
					return null;
				return replyTo [0];
			}
			set {
				replyTo.Clear ();
				replyTo.Add (value);
			}
		}

		public MailAddress Sender {
			get { return sender; }
			set { sender = value; }
		}

		public string Subject {
			get { return subject; }
			set {
				if (value != null && subjectEncoding == null)
					subjectEncoding = GuessEncoding (value);
				subject = value;
			}
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

		private Encoding GuessEncoding (string s)
		{
			for (int i = 0; i < s.Length; i++)
				if (s [i] >= '\u0080')
					return UTF8Unmarked;
			return null;
		}

		internal static TransferEncoding GuessTransferEncoding (Encoding enc)
		{
			if (Encoding.ASCII.Equals (enc))
				return TransferEncoding.SevenBit;
			else if (Encoding.UTF8.CodePage == enc.CodePage ||
#if !MOBILE
			    Encoding.Unicode.CodePage == enc.CodePage || Encoding.UTF32.CodePage == enc.CodePage
#else
			    Encoding.Unicode.CodePage == enc.CodePage
#endif
					 )
				return TransferEncoding.Base64;
			else
				return TransferEncoding.QuotedPrintable;
		}

		static char [] hex = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
		internal static string To2047(byte [] bytes)
		{
			StringBuilder sb = new StringBuilder ();
			foreach (byte i in bytes) {
				if (i < 0x21 || i > 0x7E || i == '?' || i == '=' || i == '_') {
					sb.Append ('=');
					sb.Append (hex [(i >> 4) & 0x0f]);
					sb.Append (hex [i & 0x0f]);
				} else
					sb.Append ((char) i);
			}
			return sb.ToString ();
		}

		internal static string EncodeSubjectRFC2047 (string s, Encoding enc)
		{
			if (s == null || Encoding.ASCII.Equals (enc))
				return s;
			for (int i = 0; i < s.Length; i++)
				if (s [i] >= '\u0080') {
					string quoted = To2047(enc.GetBytes (s));
					return String.Concat ("=?", enc.HeaderName, "?Q?", quoted, "?=");
				}
			return s;
		}

		static Encoding utf8unmarked;
		static Encoding UTF8Unmarked {
			get {
				if (utf8unmarked == null)
					utf8unmarked = new UTF8Encoding (false);
				return utf8unmarked;
			}
		}
		#endregion // Methods
	}
}

