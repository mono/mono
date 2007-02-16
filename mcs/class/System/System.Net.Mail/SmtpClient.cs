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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Net.Configuration;
using System.Configuration;

namespace System.Net.Mail {
	public class SmtpClient
	{
		#region Fields

		string host;
		int port;
		int timeout = 100000;
		ICredentialsByHost credentials;
		bool useDefaultCredentials = false;
		string pickupDirectoryLocation;
		SmtpDeliveryMethod deliveryMethod;
		bool enableSsl;
		X509CertificateCollection clientCertificates;

		TcpClient client;
		NetworkStream stream;
		StreamWriter writer;
		StreamReader reader;
		int boundaryIndex;
		MailAddress defaultFrom;

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

		public SmtpClient (string host, int port) {
#if CONFIGURATION_DEP
			SmtpSection cfg = (SmtpSection) ConfigurationManager.GetSection ("system.net/mailSettings/smtp");

			if (cfg != null) {
				this.host = cfg.Network.Host;
				this.port = cfg.Network.Port;
				if (cfg.Network.UserName != null) {
					string password = String.Empty;

					if (cfg.Network.Password != null)
						password = cfg.Network.Password;

					Credentials = new CCredentialsByHost (cfg.Network.UserName, password);
				}

				if (cfg.From != null)
					defaultFrom = new MailAddress (cfg.From);
			}
#endif

			if (!String.IsNullOrEmpty (host))
				this.host = host;

			if (port != 0)
				this.port = port;
		}

		#endregion // Constructors

		#region Properties

		[MonoTODO("Client certificates are not supported")]
		public X509CertificateCollection ClientCertificates {
			get {
				throw new NotImplementedException ("Client certificates are not supported");
				return clientCertificates;
			}
		}

		public ICredentialsByHost Credentials {
			get { return credentials; }
			set { credentials = value; }
		}

		public SmtpDeliveryMethod DeliveryMethod {
			get { return deliveryMethod; }
			set { deliveryMethod = value; }
		}

		public bool EnableSsl {
			get { return enableSsl; }
			set { enableSsl = value; }
		}

		public string Host {
			get { return host; }
			// FIXME: Check to make sure an email is not being sent.
			set {
				if (value == null)
					throw new ArgumentNullException ();
				if (value.Length == 0)
					throw new ArgumentException ();
				host = value;
			}
		}

		public string PickupDirectoryLocation {
			get { return pickupDirectoryLocation; }
			set { pickupDirectoryLocation = value; }
		}

		public int Port {
			get { return port; }
			// FIXME: Check to make sure an email is not being sent.
			set { 
				if (value <= 0)
					throw new ArgumentOutOfRangeException ();
				port = value; 
			}
		}

		public ServicePoint ServicePoint {
			get { throw new NotImplementedException (); }
		}

		public int Timeout {
			get { return timeout; }
			// FIXME: Check to make sure an email is not being sent.
			set { 
				if (value < 0)
					throw new ArgumentOutOfRangeException ();
				timeout = value; 
			}
		}

		public bool UseDefaultCredentials {
			get { return useDefaultCredentials; }
			set { throw new NotImplementedException ("Default credentials are not supported"); }
		}

		#endregion // Properties

		#region Events 

		public event SendCompletedEventHandler SendCompleted;

		#endregion // Events 

		#region Methods

		private void EndSection (string section)
		{
			SendData (String.Format ("--{0}--", section));
			SendData (string.Empty);
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

		private SmtpResponse Read () {
			byte [] buffer = new byte [512];
			int position = 0;
			bool lastLine = false;

			do {
				int readLength = stream.Read (buffer, position, buffer.Length - position);
				if (readLength > 0) {
					int available = position + readLength - 1;
					if (available > 4 && (buffer [available] == '\n' || buffer [available] == '\r'))
						for (int index = available - 3; ; index--) {
							if (index < 0 || buffer [index] == '\n' || buffer [index] == '\r') {
								lastLine = buffer [index + 4] == ' ';
								break;
							}
						}

					// move position
					position += readLength;

					// check if buffer is full
					if (position == buffer.Length) {
						byte [] newBuffer = new byte [buffer.Length * 2];
						Array.Copy (buffer, 0, newBuffer, 0, buffer.Length);
						buffer = newBuffer;
					}
				}
				else {
					break;
				}
			} while (!lastLine);

			if (position > 0) {
				Encoding encoding = new ASCIIEncoding ();

				string line = encoding.GetString (buffer, 0, position - 1);

				// parse the line to the lastResponse object
				SmtpResponse response = SmtpResponse.Parse (line);

				return response;
			}
			else {
				throw new System.IO.IOException ("Connection closed");
			}
		}

		public void Send (MailMessage message) {
			CheckHostAndPort ();

			// Block while sending
			mutex.WaitOne ();

			SmtpResponse status;

			client = new TcpClient (host, port);
			stream = client.GetStream ();
			writer = new StreamWriter (stream);
			reader = new StreamReader (stream);

			status = Read ();
			if (IsError (status))
				throw new SmtpException (status.StatusCode, status.Description);

			// EHLO
			status = SendCommand (Command.Ehlo, Dns.GetHostName ());

			if (IsError (status)) {
				throw new SmtpException (status.StatusCode, status.Description);
			}

			if (EnableSsl) {
				InitiateSecureConnection ();
			}

			PerformAuthentication ();

			MailAddress from = message.From;

			if (from == null)
				from = defaultFrom;
			
			// MAIL FROM:
			status = SendCommand (Command.MailFrom, '<' + from.Address + '>');
			if (IsError (status)) {
				throw new SmtpException (status.StatusCode, status.Description);
			}

			// Send RCPT TO: for all recipients
			List<SmtpFailedRecipientException> sfre = new List<SmtpFailedRecipientException> ();

			for (int i = 0; i < message.To.Count; i ++) {
				status = SendCommand (Command.RcptTo, '<' + message.To [i].Address + '>');
				if (IsError (status)) 
					sfre.Add (new SmtpFailedRecipientException (status.StatusCode, message.To [i].Address));
			}
			for (int i = 0; i < message.CC.Count; i ++) {
				status = SendCommand (Command.RcptTo, '<' + message.CC [i].Address + '>');
				if (IsError (status)) 
					sfre.Add (new SmtpFailedRecipientException (status.StatusCode, message.CC [i].Address));
			}
			for (int i = 0; i < message.Bcc.Count; i ++) {
				status = SendCommand (Command.RcptTo, '<' + message.Bcc [i].Address + '>');
				if (IsError (status)) 
					sfre.Add (new SmtpFailedRecipientException (status.StatusCode, message.Bcc [i].Address));
			}

#if TARGET_JVM // List<T>.ToArray () is not supported
			if (sfre.Count > 0) {
				SmtpFailedRecipientException[] xs = new SmtpFailedRecipientException[sfre.Count];
				sfre.CopyTo (xs);
				throw new SmtpFailedRecipientsException ("failed recipients", xs);
			}
#else
			if (sfre.Count >0)
				throw new SmtpFailedRecipientsException ("failed recipients", sfre.ToArray ());
#endif

			// DATA
			status = SendCommand (Command.Data);
			if (IsError (status))
				throw new SmtpException (status.StatusCode, status.Description);

			// Send message headers
			SendHeader (HeaderName.From, from.ToString ());
			SendHeader (HeaderName.To, message.To.ToString ());
			if (message.CC.Count > 0)
				SendHeader (HeaderName.Cc, message.CC.ToString ());
			if (message.Bcc.Count > 0)
				SendHeader (HeaderName.Bcc, message.Bcc.ToString ());
			SendHeader (HeaderName.Subject, message.Subject);

			foreach (string s in message.Headers.AllKeys)
				SendHeader (s, message.Headers [s]);

			AddPriorityHeader (message);

			bool hasAlternateViews = (message.AlternateViews.Count > 0);
			bool hasAttachments = (message.Attachments.Count > 0);

			if (hasAttachments || hasAlternateViews) {
				SendMultipartBody (message);
			}
			else {
				SendSimpleBody (message);
			}

			SendData (".");

			status = Read ();
			if (IsError (status))
				throw new SmtpException (status.StatusCode, status.Description);

			try {
				status = SendCommand (Command.Quit);
			}
			catch (System.IO.IOException) {
				//We excuse server for the rude connection closing as a response to QUIT
			}

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
			writer.Write (data);
			// Certain SMTP servers will reject mail sent with unix line-endings; see http://cr.yp.to/docs/smtplf.html
			writer.Write ("\r\n");
			writer.Flush ();
		}

		public void SendAsync (MailMessage message, object userToken)
		{
			Send (message);
			OnSendCompleted (new AsyncCompletedEventArgs (null, false, userToken));
		}

		public void SendAsync (string from, string to, string subject, string body, object userToken)
		{
			SendAsync (new MailMessage (from, to, subject, body), userToken);
		}

		public void SendAsyncCancel ()
		{
			throw new NotImplementedException ();
		}

		private void AddPriorityHeader (MailMessage message) {
			switch (message.Priority) {
			case MailPriority.High:
				SendHeader (HeaderName.Priority, "Urgent");
				SendHeader (HeaderName.Importance, "high");
				SendHeader (HeaderName.XPriority, "1");
				break;
			case MailPriority.Low:
				SendHeader (HeaderName.Priority, "Non-Urgent");
				SendHeader (HeaderName.Importance, "low");
				SendHeader (HeaderName.XPriority, "5");
				break;
			}
		}

		private void SendSimpleBody (MailMessage message) {
			SendHeader ("Content-Type", message.BodyContentType.ToString ());
			SendData (string.Empty);

			SendData (message.Body);
		}

		private void SendMultipartBody (MailMessage message) {
			boundaryIndex = 0;
			string boundary = GenerateBoundary ();

			// Figure out the message content type
			ContentType messageContentType = message.BodyContentType;
			messageContentType.Boundary = boundary;
			messageContentType.MediaType = "multipart/mixed";

			SendHeader ("Content-Type", messageContentType.ToString ());
			SendData (string.Empty);

			SendData (message.Body);
			SendData (string.Empty);

			message.AlternateViews.Add (AlternateView.CreateAlternateViewFromString (message.Body, new ContentType ("text/plain")));

			if (message.AlternateViews.Count > 0) {
				SendAlternateViews (message, boundary);
			}

			if (message.Attachments.Count > 0) {
				SendAttachments (message, boundary);
			}

			EndSection (boundary);
		}

		private void SendAlternateViews (MailMessage message, string boundary) {
			AlternateViewCollection alternateViews = message.AlternateViews;

			string inner_boundary = GenerateBoundary ();

			ContentType messageContentType = message.BodyContentType;
			messageContentType.Boundary = inner_boundary;
			messageContentType.MediaType = "multipart/alternative";

			StartSection (boundary, messageContentType);

			for (int i = 0; i < alternateViews.Count; i += 1) {
				ContentType contentType = new ContentType (alternateViews [i].ContentType.ToString ());
				StartSection (inner_boundary, contentType, alternateViews [i].TransferEncoding);

				switch (alternateViews [i].TransferEncoding) {
				case TransferEncoding.Base64:
					byte [] content = new byte [alternateViews [i].ContentStream.Length];
					alternateViews [i].ContentStream.Read (content, 0, content.Length);
#if TARGET_JVM
					SendData (Convert.ToBase64String (content));
#else
					    SendData (Convert.ToBase64String (content, Base64FormattingOptions.InsertLineBreaks));
#endif
					break;
				case TransferEncoding.QuotedPrintable:
					StreamReader sr = new StreamReader (alternateViews [i].ContentStream);
					SendData (ToQuotedPrintable (sr.ReadToEnd ()));
					break;
				//case TransferEncoding.SevenBit:
				//case TransferEncoding.Unknown:
				default:
					SendData ("TO BE IMPLEMENTED");
					break;
				}

				SendData (string.Empty);
			}

			EndSection (inner_boundary);
		}

		private void SendAttachments (MailMessage message, string boundary) {
			AttachmentCollection attachments = message.Attachments;

			for (int i = 0; i < attachments.Count; i += 1) {
				ContentType contentType = new ContentType (attachments [i].ContentType.ToString ());
				attachments [i].ContentDisposition.FileName = attachments [i].Name;
				StartSection (boundary, contentType, attachments [i].TransferEncoding, attachments [i].ContentDisposition);

				switch (attachments [i].TransferEncoding) {
				case TransferEncoding.Base64:
					byte[] content = new byte [attachments [i].ContentStream.Length];
					attachments [i].ContentStream.Read (content, 0, content.Length);
#if TARGET_JVM
					SendData (Convert.ToBase64String (content));
#else
					SendData (Convert.ToBase64String (content, Base64FormattingOptions.InsertLineBreaks));
#endif
					break;
				case TransferEncoding.QuotedPrintable:
					StreamReader sr = new StreamReader (attachments [i].ContentStream);
					SendData (ToQuotedPrintable (sr.ReadToEnd ()));
					break;
				//case TransferEncoding.SevenBit:
				//case TransferEncoding.Unknown:
				default:
					SendData ("TO BE IMPLEMENTED");
					break;
				}

				SendData (string.Empty);
			}
		}

		private SmtpResponse SendCommand (string command, string data)
		{
			writer.Write (command);
			writer.Write (" ");
			SendData (data);
			return Read ();
		}

		private SmtpResponse SendCommand (string command)
		{
			writer.Write (command);
			// Certain SMTP servers will reject mail sent with unix line-endings; see http://cr.yp.to/docs/smtplf.html
			writer.Write ("\r\n");
			writer.Flush ();
			return Read ();
		}

		private void SendHeader (string name, string value)
		{
			SendData (String.Format ("{0}: {1}", name, value));
		}

		private void StartSection (string section, ContentType sectionContentType)
		{
			SendData (string.Empty);
			SendData (String.Format ("--{0}", section));
			SendHeader ("content-type", sectionContentType.ToString ());
			SendData (string.Empty);
		}

		private void StartSection (string section, ContentType sectionContentType,TransferEncoding transferEncoding)
		{
			SendData (String.Format ("--{0}", section));
			SendHeader ("content-type", sectionContentType.ToString ());
			SendHeader ("content-transfer-encoding", GetTransferEncodingName (transferEncoding));
			SendData (string.Empty);
		}

		private void StartSection (string section, ContentType sectionContentType, TransferEncoding transferEncoding, ContentDisposition contentDisposition) {
			SendData (String.Format ("--{0}", section));
			SendHeader ("content-type", sectionContentType.ToString ());
			SendHeader ("content-transfer-encoding", GetTransferEncodingName (transferEncoding));
			SendHeader ("content-disposition", contentDisposition.ToString ());
			SendData (string.Empty);
		}

		private string ToQuotedPrintable (string input) {
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
			case TransferEncoding.SevenBit:
				return "7bit";
			case TransferEncoding.Base64:
				return "base64";
			}
			return "unknown";
		}

		private void InitiateSecureConnection () {
			SmtpResponse response = SendCommand (Command.StartTls);

			if (IsError (response)) {
				throw new SmtpException (SmtpStatusCode.GeneralFailure, "Server does not support secure connections.");
			}

			ChangeToSSLSocket ();
		}

		private bool ChangeToSSLSocket () {
#if TARGET_JVM
			stream.ChangeToSSLSocket ();

			return true;
#else
			throw new NotImplementedException ();
#endif
		}

		void CheckHostAndPort () {
			if (String.IsNullOrEmpty (Host))
				throw new InvalidOperationException ("The SMTP host was not specified");

			if (Port == 0)
				Port = 25;
		}
		
		void PerformAuthentication () {
			if (UseDefaultCredentials) {
				Authenticate (
					CredentialCache.DefaultCredentials.GetCredential (new System.Uri ("smtp://" + host), "basic").UserName,
					CredentialCache.DefaultCredentials.GetCredential (new System.Uri ("smtp://" + host), "basic").Password);
			}
			else if (Credentials != null) {
				Authenticate (
					Credentials.GetCredential (host, port, "smtp").UserName,
					Credentials.GetCredential (host, port, "smtp").Password);
			}
		}

		void Authenticate (string Username, string Password) {
			SmtpResponse status = SendCommand (Command.AuthLogin);
			if (((int) status.StatusCode) != 334) {
				throw new SmtpException (status.StatusCode, status.Description);
			}

			status = SendCommand (Convert.ToBase64String (Encoding.ASCII.GetBytes (Username)));
			if (((int) status.StatusCode) != 334) {
				throw new SmtpException (status.StatusCode, status.Description);
			}

			status = SendCommand (Convert.ToBase64String (Encoding.ASCII.GetBytes (Password)));
			if (IsError (status)) {
				throw new SmtpException (status.StatusCode, status.Description);
			}
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
			public const string Ehlo = "EHLO";
			public const string MailFrom = "MAIL FROM:";
			public const string Quit = "QUIT";
			public const string RcptTo = "RCPT TO:";
			public const string StartTls = "STARTTLS";
			public const string AuthLogin = "AUTH LOGIN";
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
			public const string Priority = "Priority";
			public const string Importance = "Importance";
			public const string XPriority = "X-Priority";
		}

		// This object encapsulates the status code and description of an SMTP response.
		private struct SmtpResponse {
			public SmtpStatusCode StatusCode;
			public string Description;

			public static SmtpResponse Parse (string line) {
				SmtpResponse response = new SmtpResponse ();

				if (line.Length < 4)
					throw new SmtpException ("Response is to short " +
								   line.Length + ".");

				if ((line [3] != ' ') && (line [3] != '-'))
					throw new SmtpException ("Response format is wrong.(" +
								 line + ")");

				// parse the response code
				response.StatusCode = (SmtpStatusCode) Int32.Parse (line.Substring (0, 3));

				// set the rawsponse
				response.Description = line;

				return response;
			}
		}
	}

	class CCredentialsByHost : ICredentialsByHost
	{
		public CCredentialsByHost (string userName, string password) {
			this.userName = userName;
			this.password = password;
		}

		public NetworkCredential GetCredential (string host, int port, string authenticationType) {
			return new NetworkCredential (userName, password);
		}

		private string userName;
		private string password;
	}
}

#endif // NET_2_0
