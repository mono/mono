//
// System.Net.Mail.SmtpClient.cs
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

using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace System.Net.Mail {
	public class SmtpClient : IDisposable //, IGetContextAwareResult
	{
		#region Fields

		string host;
		int port;
		int timeout;
		ICredentialsByHost credentials;
		bool useDefaultCredentials;

		TcpClient client;
		NetworkStream stream;
		StreamWriter writer;
		StreamReader reader;
		int boundaryIndex;

		Mutex mutex = new Mutex ();

		const string MimeVersion = "1.0 (produced by Mono System.Net.Mail.SmtpClient)";

		#endregion // Fields

		#region Constructors

		public SmtpClient ()
			: this (null, 0)
		{
		}

		public SmtpClient (string host)
			: this (host, 0)
		{
		}

		[MonoTODO ("Load default settings from configuration.")]
		public SmtpClient (string host, int port)
		{
			Host = host;
			Port = port;
		}

		#endregion // Constructors

		#region Properties

		public ICredentialsByHost Credentials {
			get { return credentials; }
			set { credentials = value; }
		}

		public string Host {
			get { return host; }
			set { host = value; }
		}

		public int Port {
			get { return port; }
			[MonoTODO ("Check to make sure an email is not being sent.")]
			set { 
				if (value <= 0)
					throw new ArgumentOutOfRangeException ();
				port = value; 
			}
		}

		[MonoTODO]
		public ServicePoint ServicePoint {
			get { throw new NotImplementedException (); }
		}

		public int Timeout {
			get { return timeout; }
			[MonoTODO ("Check to make sure an email is not being sent.")]
			set { 
				if (value <= 0)
					throw new ArgumentOutOfRangeException ();
				timeout = value; 
			}
		}

		public bool UseDefaultCredentials {
			get { return useDefaultCredentials; }
			set { useDefaultCredentials = value; }
		}

		#endregion // Properties

		#region Events 

		public event SendCompletedEventHandler SendCompleted;

		#endregion // Events 

		#region Methods

		[MonoTODO]
		public void Dispose ()
		{
		}

		private void EndSection (string section)
		{
			SendData (String.Format ("--{0}--", section));
		}

		private string GenerateBoundary ()
		{
			string output = GenerateBoundary (boundaryIndex);
			boundaryIndex += 1;
			return output;
		}

		private static string GenerateBoundary (int index)
		{
			return String.Format ("--boundary_{0}_{1}", index, Guid.NewGuid ().ToString ("D"));
		}

		private bool IsError (SmtpResponse status)
		{
			return ((int) status.StatusCode) >= 400;
		}

		protected void OnSendCompleted (AsyncCompletedEventArgs e)
		{
			if (SendCompleted != null)
				SendCompleted (this, e);
		}

		private SmtpResponse Read ()
		{
			SmtpResponse response;

			char[] buf = new char [3];
			reader.Read (buf, 0, 3);
			reader.Read ();

			response.StatusCode = (SmtpStatusCode) Int32.Parse (new String (buf));
			response.Description = reader.ReadLine ();

			return response;
		}

		[MonoTODO ("Need to work on message attachments.")]
		public void Send (MailMessage message)
		{
			// Block while sending
			mutex.WaitOne ();

			SmtpResponse status;

			client = new TcpClient (host, port);
			stream = client.GetStream ();
			writer = new StreamWriter (stream);
			reader = new StreamReader (stream);
			boundaryIndex = 0;
			string boundary = GenerateBoundary ();

			bool hasAlternateViews = (message.AlternateViews.Count > 0);
			bool hasAttachments = (message.Attachments.Count > 0);

			status = Read ();
			if (IsError (status))
				throw new SmtpException (status.StatusCode);

			// HELO
			status = SendCommand (Command.Helo, Dns.GetHostName ());
			if (IsError (status))
				throw new SmtpException (status.StatusCode);

			// MAIL FROM:
			status = SendCommand (Command.MailFrom, message.From.Address);
			if (IsError (status))
				throw new SmtpException (status.StatusCode);

			// Send RCPT TO: for all recipients in the To list
			for (int i = 0; i < message.To.Count; i += 1) {
				status = SendCommand (Command.RcptTo, message.To [i].Address);
				if (IsError (status))
					throw new SmtpException (status.StatusCode);
			}

			// Send RCPT TO: for all recipients in the CC list
			for (int i = 0; i < message.CC.Count; i += 1) {
				status = SendCommand (Command.RcptTo, message.CC [i].Address);
				if (IsError (status))
					throw new SmtpException (status.StatusCode);
			}

			// Send RCPT TO: for all recipients in the Bcc list
			for (int i = 0; i < message.Bcc.Count; i += 1) {
				status = SendCommand (Command.RcptTo, message.Bcc [i].Address);
				if (IsError (status))
					throw new SmtpException (status.StatusCode);
			}

			// DATA
			status = SendCommand (Command.Data);
			if (IsError (status))
				throw new SmtpException (status.StatusCode);

			// Figure out the message content type
			ContentType messageContentType = message.BodyContentType;
			if (hasAttachments || hasAlternateViews) {
				messageContentType = new ContentType ();
				messageContentType.Boundary = boundary;

				if (hasAttachments)
					messageContentType.MediaType = "multipart/mixed";
				else
					messageContentType.MediaType = "multipart/alternative";
			}

			// Send message headers
			SendHeader (HeaderName.From, message.From.ToString ());
			SendHeader (HeaderName.To, message.To.ToString ());
			if (message.CC.Count > 0)
				SendHeader (HeaderName.Cc, message.CC.ToString ());
			if (message.Bcc.Count > 0)
				SendHeader (HeaderName.Bcc, message.Bcc.ToString ());
			SendHeader (HeaderName.Subject, message.Subject);

			foreach (string s in message.Headers.AllKeys)
				SendHeader (s, message.Headers [s]);

			SendHeader ("Content-Type", messageContentType.ToString ());
			SendData ("");

			if (hasAlternateViews) {
				string innerBoundary = boundary;

				// The body is *technically* an alternative view.  The body text goes FIRST because
				// that is most compatible with non-MIME readers.
				//
				// If there are attachments, then the main content-type is multipart/mixed and
				// the subpart has type multipart/alternative.  Then all of the views have their
				// own types.  
				//
				// If there are no attachments, then the main content-type is multipart/alternative
				// and we don't need this subpart.

				if (hasAttachments) {
					innerBoundary = GenerateBoundary ();
					ContentType contentType = new ContentType ("multipart/alternative");
					contentType.Boundary = innerBoundary;
					StartSection (boundary, contentType);
				}
				
				// Start the section for the body text.  This is either section "1" or "0" depending
				// on whether there are attachments.

				StartSection (innerBoundary, message.BodyContentType, TransferEncoding.QuotedPrintable);
				SendData (message.Body);

				// Send message attachments.
				SendAttachments (message.AlternateViews, innerBoundary);

				if (hasAttachments) 
					EndSection (innerBoundary);
			}
			else {
				// If this is multipart then we need to send a boundary before the body.
				if (hasAttachments)
					StartSection (boundary, message.BodyContentType, TransferEncoding.QuotedPrintable);
				SendData (message.Body);
			}

			// Send attachments
			if (hasAttachments) {
				string innerBoundary = boundary;

				// If we have alternate views and attachments then we need to nest this part inside another
				// boundary.  Otherwise, we are cool with the boundary we have.

				if (hasAlternateViews) {
					innerBoundary = GenerateBoundary ();
					ContentType contentType = new ContentType ("multipart/mixed");
					contentType.Boundary = innerBoundary;
					StartSection (boundary, contentType);
				}

				SendAttachments (message.Attachments, innerBoundary);

				if (hasAlternateViews)
					EndSection (innerBoundary);
			}

			SendData (".");

			status = Read ();
			if (IsError (status))
				throw new SmtpException (status.StatusCode);

			status = SendCommand (Command.Quit);

			writer.Close ();
			reader.Close ();
			stream.Close ();
			client.Close ();

			// Release the mutex to allow other threads access
			mutex.ReleaseMutex ();
		}

		public void Send (string from, string to, string subject, string body)
		{
			Send (new MailMessage (from, to, subject, body));
		}

		private void SendData (string data)
		{
			writer.WriteLine (data);
			writer.Flush ();
		}

		[MonoTODO]
		public void SendAsync (MailMessage message, object userToken)
		{
			Send (message);
			OnSendCompleted (new AsyncCompletedEventArgs (null, false, userToken));
		}

		public void SendAsync (string from, string to, string subject, string body, object userToken)
		{
			SendAsync (new MailMessage (from, to, subject, body), userToken);
		}

		[MonoTODO]
		public void SendAsyncCancel ()
		{
			throw new NotImplementedException ();
		}

		private void SendAttachments (AttachmentCollection attachments, string boundary)
		{
			for (int i = 0; i < attachments.Count; i += 1) {
				StartSection (boundary, attachments [i].ContentType, attachments [i].TransferEncoding);

				switch (attachments [i].TransferEncoding) {
				case TransferEncoding.Base64:
					StreamReader reader = new StreamReader (attachments [i].ContentStream);
					byte[] content = new byte [attachments [i].ContentStream.Length];
					attachments [i].ContentStream.Read (content, 0, content.Length);
					SendData (Convert.ToBase64String (content, Base64FormattingOptions.InsertLineBreaks));
					break;
				case TransferEncoding.QuotedPrintable:
					SendData (ToQuotedPrintable (attachments [i].ContentString));
					break;
				default:
					SendData ("TO BE IMPLEMENTED");
					break;
				}
			}
		}

		private SmtpResponse SendCommand (string command, string data)
		{
			SmtpResponse response;
			writer.Write (command);
			writer.Write (" ");
			SendData (data);
			return Read ();
		}

		private SmtpResponse SendCommand (string command)
		{
			writer.WriteLine (command);
			writer.Flush ();
			return Read ();
		}

		private void SendHeader (string name, string value)
		{
			SendData (String.Format ("{0}: {1}", name, value));
		}

		private void StartSection (string section, ContentType sectionContentType)
		{
			SendData (String.Format ("--{0}", section));
			SendHeader ("content-type", sectionContentType.ToString ());
			SendData ("");
		}

		private void StartSection (string section, ContentType sectionContentType, TransferEncoding transferEncoding)
		{
			SendData (String.Format ("--{0}", section));
			SendHeader ("content-type", sectionContentType.ToString ());
			SendHeader ("content-transfer-encoding", GetTransferEncodingName (transferEncoding));
			SendData ("");
		}

		private string ToQuotedPrintable (string input)
		{
			StringReader reader = new StringReader (input);
			StringWriter writer = new StringWriter ();
			int i;

			while ((i = reader.Read ()) > 0) {
				if (i > 127) {
					writer.Write ("=");
					writer.Write (Convert.ToString (i, 16).ToUpper ());
				}
				else
					writer.Write (Convert.ToChar (i));
			}

			return writer.GetStringBuilder ().ToString ();
		}

		private static string GetTransferEncodingName (TransferEncoding encoding)
		{
			switch (encoding) {
			case TransferEncoding.QuotedPrintable:
				return "quoted-printable";
			case TransferEncoding.EightBit:
				return "8bit";
			case TransferEncoding.SevenBit:
				return "7bit";
			case TransferEncoding.Base64:
				return "base64";
			case TransferEncoding.Binary:
				return "binary";
			}
			return "unknown";
		}

/*
		[MonoTODO]
		private sealed ContextAwareResult IGetContextAwareResult.GetContextAwareResult ()
		{
			throw new NotImplementedException ();
		}
*/
		#endregion // Methods

		// The Command struct is used to store constant string values representing SMTP commands.
		private struct Command {
			public const string Data = "DATA";
			public const string Helo = "HELO";
			public const string MailFrom = "MAIL FROM:";
			public const string Quit = "QUIT";
			public const string RcptTo = "RCPT TO:";
		}

		// The HeaderName struct is used to store constant string values representing mail headers.
		private struct HeaderName {
			public const string ContentTransferEncoding = "Content-Transfer-Encoding";
			public const string ContentType = "Content-Type";
			public const string Bcc = "Bcc";
			public const string Cc = "Cc";
			public const string From = "From";
			public const string Subject = "Subject";
			public const string To = "To";
			public const string MimeVersion = "MIME-Version";
			public const string MessageId = "Message-ID";
		}

		// This object encapsulates the status code and description of an SMTP response.
		private struct SmtpResponse {
			public SmtpStatusCode StatusCode;
			public string Description;
		}
	}
}

#endif // NET_2_0
