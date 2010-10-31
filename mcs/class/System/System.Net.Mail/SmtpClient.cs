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

#if SECURITY_DEP
extern alias PrebuiltSystem;
#endif

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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
using System.Net.Security;
using System.Security.Authentication;

#if SECURITY_DEP
using X509CertificateCollection = PrebuiltSystem::System.Security.Cryptography.X509Certificates.X509CertificateCollection;
#endif

namespace System.Net.Mail {
	public class SmtpClient
	{
		#region Fields

		string host;
		int port;
		int timeout = 100000;
		ICredentialsByHost credentials;
		string pickupDirectoryLocation;
		SmtpDeliveryMethod deliveryMethod;
		bool enableSsl;
#if SECURITY_DEP		
		X509CertificateCollection clientCertificates;
#endif		

		TcpClient client;
		Stream stream;
		StreamWriter writer;
		StreamReader reader;
		int boundaryIndex;
		MailAddress defaultFrom;

		MailMessage messageInProcess;

		BackgroundWorker worker;
		object user_async_state;

		[Flags]
		enum AuthMechs {
			None        = 0,
			Login       = 0x01,
			Plain       = 0x02,
		}

		class CancellationException : Exception
		{
		}

		AuthMechs authMechs;
		Mutex mutex = new Mutex ();

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
#if NET_4_0
				this.enableSsl = cfg.Network.EnableSsl;
#endif
				TargetName = cfg.Network.TargetName;
				if (this.TargetName == null)
					TargetName = "SMTPSVC/" + (host != null ? host : "");

				
				if (cfg.Network.UserName != null) {
					string password = String.Empty;

					if (cfg.Network.Password != null)
						password = cfg.Network.Password;

					Credentials = new CCredentialsByHost (cfg.Network.UserName, password);
				}

				if (!String.IsNullOrEmpty (cfg.From))
					defaultFrom = new MailAddress (cfg.From);
			}
#else
			// Just to eliminate the warning, this codepath does not end up in production.
			defaultFrom = null;
#endif

			if (!String.IsNullOrEmpty (host))
				this.host = host;

			if (port != 0)
				this.port = port;
		}

		#endregion // Constructors

		#region Properties

#if SECURITY_DEP
		[MonoTODO("Client certificates not used")]
		public X509CertificateCollection ClientCertificates {
			get {
				if (clientCertificates == null)
					clientCertificates = new X509CertificateCollection ();
				return clientCertificates;
			}
		}
#endif

#if NET_4_0
		public
#endif
		string TargetName { get; set; }

		public ICredentialsByHost Credentials {
			get { return credentials; }
			set {
				CheckState ();
				credentials = value;
			}
		}

		public SmtpDeliveryMethod DeliveryMethod {
			get { return deliveryMethod; }
			set {
				CheckState ();
				deliveryMethod = value;
			}
		}

		public bool EnableSsl {
			get { return enableSsl; }
			set {
				CheckState ();
				enableSsl = value;
			}
		}

		public string Host {
			get { return host; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				if (value.Length == 0)
					throw new ArgumentException ("An empty string is not allowed.", "value");
				CheckState ();
				host = value;
			}
		}

		public string PickupDirectoryLocation {
			get { return pickupDirectoryLocation; }
			set { pickupDirectoryLocation = value; }
		}

		public int Port {
			get { return port; }
			set { 
				if (value <= 0)
					throw new ArgumentOutOfRangeException ("value");
				CheckState ();
				port = value;
			}
		}

		[MonoTODO]
		public ServicePoint ServicePoint {
			get { throw new NotImplementedException (); }
		}

		public int Timeout {
			get { return timeout; }
			set { 
				if (value < 0)
					throw new ArgumentOutOfRangeException ("value");
				CheckState ();
				timeout = value; 
			}
		}

		public bool UseDefaultCredentials {
			get { return false; }
			[MonoNotSupported ("no DefaultCredential support in Mono")]
			set {
				if (value)
					throw new NotImplementedException ("Default credentials are not supported");
				CheckState ();
			}
		}

		#endregion // Properties

		#region Events 

		public event SendCompletedEventHandler SendCompleted;

		#endregion // Events 

		#region Methods

		private void CheckState ()
		{
			if (messageInProcess != null)
				throw new InvalidOperationException ("Cannot set Timeout while Sending a message");
		}
		
		private static string EncodeAddress(MailAddress address)
		{
			string encodedDisplayName = ContentType.EncodeSubjectRFC2047 (address.DisplayName, Encoding.UTF8);
			return "\"" + encodedDisplayName + "\" <" + address.Address + ">";
		}

		private static string EncodeAddresses(MailAddressCollection addresses)
		{
			StringBuilder sb = new StringBuilder();
			bool first = true;
			foreach (MailAddress address in addresses) {
				if (!first) {
					sb.Append(", ");
				}
				sb.Append(EncodeAddress(address));
				first = false;
			}
			return sb.ToString();
		}

		private string EncodeSubjectRFC2047 (MailMessage message)
		{
			return ContentType.EncodeSubjectRFC2047 (message.Subject, message.SubjectEncoding);
		}

		private string EncodeBody (MailMessage message)
		{
			string body = message.Body;
			Encoding encoding = message.BodyEncoding;
			// RFC 2045 encoding
			switch (message.ContentTransferEncoding) {
			case TransferEncoding.SevenBit:
				return body;
			case TransferEncoding.Base64:
				return Convert.ToBase64String (encoding.GetBytes (body), Base64FormattingOptions.InsertLineBreaks);
			default:
				return ToQuotedPrintable (body, encoding);
			}
		}

		private string EncodeBody (AlternateView av)
		{
			//Encoding encoding = av.ContentType.CharSet != null ? Encoding.GetEncoding (av.ContentType.CharSet) : Encoding.UTF8;

			byte [] bytes = new byte [av.ContentStream.Length];
			av.ContentStream.Read (bytes, 0, bytes.Length);

			// RFC 2045 encoding
			switch (av.TransferEncoding) {
			case TransferEncoding.SevenBit:
				return Encoding.ASCII.GetString (bytes);
			case TransferEncoding.Base64:
				return Convert.ToBase64String (bytes, Base64FormattingOptions.InsertLineBreaks);
			default:
				return ToQuotedPrintable (bytes);
			}
		}


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
			try {
				if (SendCompleted != null)
					SendCompleted (this, e);
			} finally {
				worker = null;
				user_async_state = null;
			}
		}

		private void CheckCancellation ()
		{
			if (worker != null && worker.CancellationPending)
				throw new CancellationException ();
		}

		private SmtpResponse Read () {
			byte [] buffer = new byte [512];
			int position = 0;
			bool lastLine = false;

			do {
				CheckCancellation ();

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
			} else {
				throw new System.IO.IOException ("Connection closed");
			}
		}

		void ResetExtensions()
		{
			authMechs = AuthMechs.None;
		}

		void ParseExtensions (string extens)
		{
			string[] parts = extens.Split ('\n');

			foreach (string part in parts) {
				if (part.Length < 4)
					continue;

				string start = part.Substring (4);
				if (start.StartsWith ("AUTH ", StringComparison.Ordinal)) {
					string[] options = start.Split (' ');
					for (int k = 1; k < options.Length; k++) {
						string option = options[k].Trim();
						// GSSAPI, KERBEROS_V4, NTLM not supported
						switch (option) {
						/*
						case "CRAM-MD5":
							authMechs |= AuthMechs.CramMD5;
							break;
						case "DIGEST-MD5":
							authMechs |= AuthMechs.DigestMD5;
							break;
						*/
						case "LOGIN":
							authMechs |= AuthMechs.Login;
							break;
						case "PLAIN":
							authMechs |= AuthMechs.Plain;
							break;
						}
					}
				}
			}
		}

		public void Send (MailMessage message)
		{
			if (message == null)
				throw new ArgumentNullException ("message");

			if (deliveryMethod == SmtpDeliveryMethod.Network && (Host == null || Host.Trim ().Length == 0))
				throw new InvalidOperationException ("The SMTP host was not specified");
			else if (deliveryMethod == SmtpDeliveryMethod.PickupDirectoryFromIis)
				throw new NotSupportedException("IIS delivery is not supported");

			if (port == 0)
				port = 25;
			
			// Block while sending
			mutex.WaitOne ();
			try {
				messageInProcess = message;
				if (deliveryMethod == SmtpDeliveryMethod.SpecifiedPickupDirectory)
					SendToFile (message);
				else
					SendInternal (message);
			} catch (CancellationException) {
				// This exception is introduced for convenient cancellation process.
			} catch (SmtpException) {
				throw;
			} catch (Exception ex) {
				throw new SmtpException ("Message could not be sent.", ex);
			} finally {
				// Release the mutex to allow other threads access
				mutex.ReleaseMutex ();
				messageInProcess = null;
			}
		}

		private void SendInternal (MailMessage message)
		{
			CheckCancellation ();

			try {
				client = new TcpClient (host, port);
				stream = client.GetStream ();
				// FIXME: this StreamWriter creation is bogus.
				// It expects as if a Stream were able to switch to SSL
				// mode (such behavior is only in Mainsoft Socket API).
				writer = new StreamWriter (stream);
				reader = new StreamReader (stream);

				SendCore (message);
			} finally {
				if (writer != null)
					writer.Close ();
				if (reader != null)
					reader.Close ();
				if (stream != null)
					stream.Close ();
				if (client != null)
					client.Close ();
			}
		}
 
		// FIXME: simple implementation, could be brushed up.
		private void SendToFile (MailMessage message)
		{
			if (!Path.IsPathRooted (pickupDirectoryLocation))
				throw new SmtpException("Only absolute directories are allowed for pickup directory.");

			string filename = Path.Combine (pickupDirectoryLocation,
				Guid.NewGuid() + ".eml");

			try {
				writer = new StreamWriter(filename);

				// FIXME: See how Microsoft fixed the bug about envelope senders, and how it actually represents the info in .eml file headers
				// 	  For all we know, defaultFrom may be the envelope sender
				// For now, we are no worse than some versions of .NET
				MailAddress from = message.From;
				if (from == null)
					from = defaultFrom;
				
				SendHeader (HeaderName.Date, DateTime.Now.ToString ("ddd, dd MMM yyyy HH':'mm':'ss zzz", DateTimeFormatInfo.InvariantInfo));
				SendHeader (HeaderName.From, EncodeAddress(from));
				SendHeader (HeaderName.To, EncodeAddresses(message.To));
				if (message.CC.Count > 0)
					SendHeader (HeaderName.Cc, EncodeAddresses(message.CC));
				SendHeader (HeaderName.Subject, EncodeSubjectRFC2047 (message));

				foreach (string s in message.Headers.AllKeys)
					SendHeader (s, message.Headers [s]);

				AddPriorityHeader (message);

				boundaryIndex = 0;
				if (message.Attachments.Count > 0)
					SendWithAttachments (message);
				else
					SendWithoutAttachments (message, null, false);


			} finally {
				if (writer != null) writer.Close(); writer = null;
			}
		}

		private void SendCore (MailMessage message)
		{
			SmtpResponse status;

			status = Read ();
			if (IsError (status))
				throw new SmtpException (status.StatusCode, status.Description);

			// EHLO
			
			// FIXME: parse the list of extensions so we don't bother wasting
			// our time trying commands if they aren't supported.
			status = SendCommand ("EHLO " + Dns.GetHostName ());
			
			if (IsError (status)) {
				status = SendCommand ("HELO " + Dns.GetHostName ());
				
				if (IsError (status))
					throw new SmtpException (status.StatusCode, status.Description);
			} else {
				// Parse ESMTP extensions
				string extens = status.Description;
				
				if (extens != null)
					ParseExtensions (extens);
			}
			
			if (enableSsl) {
				InitiateSecureConnection ();
				ResetExtensions();
				writer = new StreamWriter (stream);
				reader = new StreamReader (stream);
				status = SendCommand ("EHLO " + Dns.GetHostName ());
			
				if (IsError (status)) {
					status = SendCommand ("HELO " + Dns.GetHostName ());
				
					if (IsError (status))
						throw new SmtpException (status.StatusCode, status.Description);
				} else {
					// Parse ESMTP extensions
					string extens = status.Description;
					if (extens != null)
						ParseExtensions (extens);
				}
			}
			
			if (authMechs != AuthMechs.None)
				Authenticate ();

			// The envelope sender: use 'Sender:' in preference of 'From:'
			MailAddress sender = message.Sender;
			if (sender == null)
				sender = message.From;
			if (sender == null)
				sender = defaultFrom;
			
			// MAIL FROM:
			status = SendCommand ("MAIL FROM:<" + sender.Address + '>');
			if (IsError (status)) {
				throw new SmtpException (status.StatusCode, status.Description);
			}

			// Send RCPT TO: for all recipients
			List<SmtpFailedRecipientException> sfre = new List<SmtpFailedRecipientException> ();

			for (int i = 0; i < message.To.Count; i ++) {
				status = SendCommand ("RCPT TO:<" + message.To [i].Address + '>');
				if (IsError (status)) 
					sfre.Add (new SmtpFailedRecipientException (status.StatusCode, message.To [i].Address));
			}
			for (int i = 0; i < message.CC.Count; i ++) {
				status = SendCommand ("RCPT TO:<" + message.CC [i].Address + '>');
				if (IsError (status)) 
					sfre.Add (new SmtpFailedRecipientException (status.StatusCode, message.CC [i].Address));
			}
			for (int i = 0; i < message.Bcc.Count; i ++) {
				status = SendCommand ("RCPT TO:<" + message.Bcc [i].Address + '>');
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
			status = SendCommand ("DATA");
			if (IsError (status))
				throw new SmtpException (status.StatusCode, status.Description);

			// Send message headers
			string dt = DateTime.Now.ToString ("ddd, dd MMM yyyy HH':'mm':'ss zzz", DateTimeFormatInfo.InvariantInfo);
			// remove ':' from time zone offset (e.g. from "+01:00")
			dt = dt.Remove (dt.Length - 3, 1);
			SendHeader (HeaderName.Date, dt);

			MailAddress from = message.From;
			if (from == null)
				from = defaultFrom;

			SendHeader (HeaderName.From, EncodeAddress (from));
			SendHeader (HeaderName.To, EncodeAddresses (message.To));
			if (message.CC.Count > 0)
				SendHeader (HeaderName.Cc, EncodeAddresses (message.CC));
			SendHeader (HeaderName.Subject, EncodeSubjectRFC2047 (message));

			string v = "normal";
				
			switch (message.Priority){
			case MailPriority.Normal:
				v = "normal";
				break;
				
			case MailPriority.Low:
				v = "non-urgent";
				break;
				
			case MailPriority.High:
				v = "urgent";
				break;
			}
			SendHeader ("Priority", v);
			if (message.Sender != null)
				SendHeader ("Sender", EncodeAddress (message.Sender));
			if (message.ReplyToList.Count > 0)
				SendHeader ("Reply-To", EncodeAddresses (message.ReplyToList));

#if NET_4_0
			foreach (string s in message.Headers.AllKeys)
				SendHeader (s, ContentType.EncodeSubjectRFC2047 (message.Headers [s], message.HeadersEncoding));
#else
			foreach (string s in message.Headers.AllKeys)
				SendHeader (s, message.Headers [s]);
#endif
	
			AddPriorityHeader (message);

			boundaryIndex = 0;
			if (message.Attachments.Count > 0)
				SendWithAttachments (message);
			else
				SendWithoutAttachments (message, null, false);

			SendDot ();

			status = Read ();
			if (IsError (status))
				throw new SmtpException (status.StatusCode, status.Description);

			try {
				status = SendCommand ("QUIT");
			} catch (System.IO.IOException) {
				// We excuse server for the rude connection closing as a response to QUIT
			}
		}

		public void Send (string from, string to, string subject, string body)
		{
			Send (new MailMessage (from, to, subject, body));
		}

		private void SendDot()
		{
			writer.Write(".\r\n");
			writer.Flush();
		}

		private void SendData (string data)
		{
			if (String.IsNullOrEmpty (data)) {
				writer.Write("\r\n");
				writer.Flush();
				return;
			}

			StringReader sr = new StringReader (data);
			string line;
			bool escapeDots = deliveryMethod == SmtpDeliveryMethod.Network;
			while ((line = sr.ReadLine ()) != null) {
				CheckCancellation ();

				if (escapeDots) {
					int i;
					for (i = 0; i < line.Length; i++) {
						if (line[i] != '.')
							break;
					}
					if (i > 0 && i == line.Length) {
						line += ".";
					}
				}
				writer.Write (line);
				writer.Write ("\r\n");
			}
			writer.Flush ();
		}

		public void SendAsync (MailMessage message, object userToken)
		{
			if (worker != null)
				throw new InvalidOperationException ("Another SendAsync operation is in progress");

			worker = new BackgroundWorker ();
			worker.DoWork += delegate (object o, DoWorkEventArgs ea) {
				try {
					user_async_state = ea.Argument;
					Send (message);
				} catch (Exception ex) {
					ea.Result = ex;
					throw ex;
				}
			};
			worker.WorkerSupportsCancellation = true;
			worker.RunWorkerCompleted += delegate (object o, RunWorkerCompletedEventArgs ea) {
				// Note that RunWorkerCompletedEventArgs.UserState cannot be used (LAMESPEC)
				OnSendCompleted (new AsyncCompletedEventArgs (ea.Error, ea.Cancelled, user_async_state));
			};
			worker.RunWorkerAsync (userToken);
		}

		public void SendAsync (string from, string to, string subject, string body, object userToken)
		{
			SendAsync (new MailMessage (from, to, subject, body), userToken);
		}

		public void SendAsyncCancel ()
		{
			if (worker == null)
				throw new InvalidOperationException ("SendAsync operation is not in progress");
			worker.CancelAsync ();
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
			SendHeader (HeaderName.ContentType, message.BodyContentType.ToString ());
			if (message.ContentTransferEncoding != TransferEncoding.SevenBit)
				SendHeader (HeaderName.ContentTransferEncoding, GetTransferEncodingName (message.ContentTransferEncoding));
			SendData (string.Empty);

			SendData (EncodeBody (message));
		}

		private void SendBodylessSingleAlternate (AlternateView av) {
			SendHeader (HeaderName.ContentType, av.ContentType.ToString ());
			if (av.TransferEncoding != TransferEncoding.SevenBit)
				SendHeader (HeaderName.ContentTransferEncoding, GetTransferEncodingName (av.TransferEncoding));
			SendData (string.Empty);

			SendData (EncodeBody (av));
		}

		private void SendWithoutAttachments (MailMessage message, string boundary, bool attachmentExists)
		{
			if (message.Body == null && message.AlternateViews.Count == 1)
				SendBodylessSingleAlternate (message.AlternateViews [0]);
			else if (message.AlternateViews.Count > 0)
				SendBodyWithAlternateViews (message, boundary, attachmentExists);
			else
				SendSimpleBody (message);
		}


		private void SendWithAttachments (MailMessage message) {
			string boundary = GenerateBoundary ();

			// first "multipart/mixed"
			ContentType messageContentType = new ContentType ();
			messageContentType.Boundary = boundary;
			messageContentType.MediaType = "multipart/mixed";
			messageContentType.CharSet = null;

			SendHeader (HeaderName.ContentType, messageContentType.ToString ());
			SendData (String.Empty);

			// body section
			Attachment body = null;

			if (message.AlternateViews.Count > 0)
				SendWithoutAttachments (message, boundary, true);
			else {
				body = Attachment.CreateAttachmentFromString (message.Body, null, message.BodyEncoding, message.IsBodyHtml ? "text/html" : "text/plain");
				message.Attachments.Insert (0, body);
			}

			try {
				SendAttachments (message, body, boundary);
			} finally {
				if (body != null)
					message.Attachments.Remove (body);
			}

			EndSection (boundary);
		}

		private void SendBodyWithAlternateViews (MailMessage message, string boundary, bool attachmentExists)
		{
			AlternateViewCollection alternateViews = message.AlternateViews;

			string inner_boundary = GenerateBoundary ();

			ContentType messageContentType = new ContentType ();
			messageContentType.Boundary = inner_boundary;
			messageContentType.MediaType = "multipart/alternative";

			if (!attachmentExists) {
				SendHeader (HeaderName.ContentType, messageContentType.ToString ());
				SendData (String.Empty);
			}

			// body section
			AlternateView body = null;
			if (message.Body != null) {
				body = AlternateView.CreateAlternateViewFromString (message.Body, message.BodyEncoding, message.IsBodyHtml ? "text/html" : "text/plain");
				alternateViews.Insert (0, body);
				StartSection (boundary, messageContentType);
			}

try {
			// alternate view sections
			foreach (AlternateView av in alternateViews) {

				string alt_boundary = null;
				ContentType contentType;
				if (av.LinkedResources.Count > 0) {
					alt_boundary = GenerateBoundary ();
					contentType = new ContentType ("multipart/related");
					contentType.Boundary = alt_boundary;
					
					contentType.Parameters ["type"] = av.ContentType.ToString ();
					StartSection (inner_boundary, contentType);
					StartSection (alt_boundary, av.ContentType, av.TransferEncoding);
				} else {
					contentType = new ContentType (av.ContentType.ToString ());
					StartSection (inner_boundary, contentType, av.TransferEncoding);
				}

				switch (av.TransferEncoding) {
				case TransferEncoding.Base64:
					byte [] content = new byte [av.ContentStream.Length];
					av.ContentStream.Read (content, 0, content.Length);
#if TARGET_JVM
					SendData (Convert.ToBase64String (content));
#else
					    SendData (Convert.ToBase64String (content, Base64FormattingOptions.InsertLineBreaks));
#endif
					break;
				case TransferEncoding.QuotedPrintable:
					byte [] bytes = new byte [av.ContentStream.Length];
					av.ContentStream.Read (bytes, 0, bytes.Length);
					SendData (ToQuotedPrintable (bytes));
					break;
				case TransferEncoding.SevenBit:
				case TransferEncoding.Unknown:
					content = new byte [av.ContentStream.Length];
					av.ContentStream.Read (content, 0, content.Length);
					SendData (Encoding.ASCII.GetString (content));
					break;
				}

				if (av.LinkedResources.Count > 0) {
					SendLinkedResources (message, av.LinkedResources, alt_boundary);
					EndSection (alt_boundary);
				}

				if (!attachmentExists)
					SendData (string.Empty);
			}

} finally {
			if (body != null)
				alternateViews.Remove (body);
}
			EndSection (inner_boundary);
		}

		private void SendLinkedResources (MailMessage message, LinkedResourceCollection resources, string boundary)
		{
			foreach (LinkedResource lr in resources) {
				StartSection (boundary, lr.ContentType, lr.TransferEncoding, lr);

				switch (lr.TransferEncoding) {
				case TransferEncoding.Base64:
					byte [] content = new byte [lr.ContentStream.Length];
					lr.ContentStream.Read (content, 0, content.Length);
#if TARGET_JVM
					SendData (Convert.ToBase64String (content));
#else
					    SendData (Convert.ToBase64String (content, Base64FormattingOptions.InsertLineBreaks));
#endif
					break;
				case TransferEncoding.QuotedPrintable:
					byte [] bytes = new byte [lr.ContentStream.Length];
					lr.ContentStream.Read (bytes, 0, bytes.Length);
					SendData (ToQuotedPrintable (bytes));
					break;
				case TransferEncoding.SevenBit:
				case TransferEncoding.Unknown:
					content = new byte [lr.ContentStream.Length];
					lr.ContentStream.Read (content, 0, content.Length);
					SendData (Encoding.ASCII.GetString (content));
					break;
				}
			}
		}

		private void SendAttachments (MailMessage message, Attachment body, string boundary) {
			foreach (Attachment att in message.Attachments) {
				ContentType contentType = new ContentType (att.ContentType.ToString ());
				if (att.Name != null) {
					contentType.Name = att.Name;
					if (att.NameEncoding != null)
						contentType.CharSet = att.NameEncoding.HeaderName;
					att.ContentDisposition.FileName = att.Name;
				}
				StartSection (boundary, contentType, att.TransferEncoding, att == body ? null : att.ContentDisposition);

				byte [] content = new byte [att.ContentStream.Length];
				att.ContentStream.Read (content, 0, content.Length);
				switch (att.TransferEncoding) {
				case TransferEncoding.Base64:
#if TARGET_JVM
					SendData (Convert.ToBase64String (content));
#else
					SendData (Convert.ToBase64String (content, Base64FormattingOptions.InsertLineBreaks));
#endif
					break;
				case TransferEncoding.QuotedPrintable:
					SendData (ToQuotedPrintable (content));
					break;
				case TransferEncoding.SevenBit:
				case TransferEncoding.Unknown:
					SendData (Encoding.ASCII.GetString (content));
					break;
				}

				SendData (string.Empty);
			}
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

		private void StartSection(string section, ContentType sectionContentType, TransferEncoding transferEncoding, LinkedResource lr)
		{
			SendData (String.Format("--{0}", section));
			SendHeader ("content-type", sectionContentType.ToString ());
			SendHeader ("content-transfer-encoding", GetTransferEncodingName (transferEncoding));

			if (lr.ContentId != null && lr.ContentId.Length > 0)
				SendHeader("content-ID", "<" + lr.ContentId + ">");

			SendData (string.Empty);
		}

		private void StartSection (string section, ContentType sectionContentType, TransferEncoding transferEncoding, ContentDisposition contentDisposition) {
			SendData (String.Format ("--{0}", section));
			SendHeader ("content-type", sectionContentType.ToString ());
			SendHeader ("content-transfer-encoding", GetTransferEncodingName (transferEncoding));
			if (contentDisposition != null)
				SendHeader ("content-disposition", contentDisposition.ToString ());
			SendData (string.Empty);
		}

		// use proper encoding to escape input
		private string ToQuotedPrintable (string input, Encoding enc)
		{
			byte [] bytes = enc.GetBytes (input);
			return ToQuotedPrintable (bytes);
		}

		private string ToQuotedPrintable (byte [] bytes)
		{
			StringWriter writer = new StringWriter ();
			int charsInLine = 0;
			int curLen;
			StringBuilder sb = new StringBuilder("=", 3);
			byte equalSign = (byte)'=';
			char c = (char)0;

			foreach (byte i in bytes) {
				if (i > 127 || i == equalSign) {
					sb.Length = 1;
					sb.Append(Convert.ToString (i, 16).ToUpperInvariant ());
					curLen = 3;
				} else {
					c = Convert.ToChar (i);
					if (c == '\r' || c == '\n') {
						writer.Write (c);
						charsInLine = 0;
						continue;
					}
					curLen = 1;
				}
				
				charsInLine += curLen;
				if (charsInLine > 75) {
					writer.Write ("=\r\n");
					charsInLine = curLen;
				}
				if (curLen == 1)
					writer.Write (c);
				else
					writer.Write (sb.ToString ());
			}

			return writer.ToString ();
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

#if SECURITY_DEP
		RemoteCertificateValidationCallback callback = delegate (object sender,
									 X509Certificate certificate,
									 X509Chain chain,
									 SslPolicyErrors sslPolicyErrors) {
			// honor any exciting callback defined on ServicePointManager
			if (ServicePointManager.ServerCertificateValidationCallback != null)
				return ServicePointManager.ServerCertificateValidationCallback (sender, certificate, chain, sslPolicyErrors);
			// otherwise provide our own
			if (sslPolicyErrors != SslPolicyErrors.None)
				throw new InvalidOperationException ("SSL authentication error: " + sslPolicyErrors);
			return true;
			};
#endif

		private void InitiateSecureConnection () {
			SmtpResponse response = SendCommand ("STARTTLS");

			if (IsError (response)) {
				throw new SmtpException (SmtpStatusCode.GeneralFailure, "Server does not support secure connections.");
			}

#if TARGET_JVM
			((NetworkStream) stream).ChangeToSSLSocket ();
#elif SECURITY_DEP
			SslStream sslStream = new SslStream (stream, false, callback, null);
			CheckCancellation ();
			sslStream.AuthenticateAsClient (Host, this.ClientCertificates, SslProtocols.Default, false);
			stream = sslStream;

#else
			throw new SystemException ("You are using an incomplete System.dll build");
#endif
		}
		
		void Authenticate ()
		{
			string user = null, pass = null;
			
			if (UseDefaultCredentials) {
				user = CredentialCache.DefaultCredentials.GetCredential (new System.Uri ("smtp://" + host), "basic").UserName;
				pass = 	CredentialCache.DefaultCredentials.GetCredential (new System.Uri ("smtp://" + host), "basic").Password;
			} else if (Credentials != null) {
				user = Credentials.GetCredential (host, port, "smtp").UserName;
				pass = Credentials.GetCredential (host, port, "smtp").Password;
			} else {
				return;
			}
			
			Authenticate (user, pass);
		}

		void CheckStatus (SmtpResponse status, int i)
		{
			if (((int) status.StatusCode) != i)
				throw new SmtpException (status.StatusCode, status.Description);
		}

		void ThrowIfError (SmtpResponse status)
		{
			if (IsError (status))
				throw new SmtpException (status.StatusCode, status.Description);
		}

		void Authenticate (string user, string password)
		{
			if (authMechs == AuthMechs.None)
				return;

			SmtpResponse status;
			/*
			if ((authMechs & AuthMechs.DigestMD5) != 0) {
				status = SendCommand ("AUTH DIGEST-MD5");
				CheckStatus (status, 334);
				string challenge = Encoding.ASCII.GetString (Convert.FromBase64String (status.Description.Substring (4)));
				Console.WriteLine ("CHALLENGE: {0}", challenge);
				DigestSession session = new DigestSession ();
				session.Parse (false, challenge);
				string response = session.Authenticate (this, user, password);
				status = SendCommand (Convert.ToBase64String (Encoding.UTF8.GetBytes (response)));
				CheckStatus (status, 235);
			} else */
			if ((authMechs & AuthMechs.Login) != 0) {
				status = SendCommand ("AUTH LOGIN");
				CheckStatus (status, 334);
				status = SendCommand (Convert.ToBase64String (Encoding.UTF8.GetBytes (user)));
				CheckStatus (status, 334);
				status = SendCommand (Convert.ToBase64String (Encoding.UTF8.GetBytes (password)));
				CheckStatus (status, 235);
			} else if ((authMechs & AuthMechs.Plain) != 0) {
				string s = String.Format ("\0{0}\0{1}", user, password);
				s = Convert.ToBase64String (Encoding.UTF8.GetBytes (s));
				status = SendCommand ("AUTH PLAIN " + s);
				CheckStatus (status, 235);
			} else {
				throw new SmtpException ("AUTH types PLAIN, LOGIN not supported by the server");
			}
		}

		#endregion // Methods
		
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
			public const string Date = "Date";
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

				// set the raw response
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
