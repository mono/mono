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

using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Net.Sockets;
using System.Text;

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

		private bool IsError (SmtpResponse status)
		{
			return ((int) status.StatusCode) >= 400;
		}

		public static string GenerateBoundary() 
		{
			StringBuilder  boundary = new StringBuilder ("__MONO__Boundary");

			boundary.Append ("__");

			DateTime now = DateTime.Now;
			boundary.Append (now.Year);
			boundary.Append (now.Month);
			boundary.Append (now.Day);
			boundary.Append (now.Hour);
			boundary.Append (now.Minute);
			boundary.Append (now.Second);
			boundary.Append (now.Millisecond);

			boundary.Append ("__");
			boundary.Append ((new Random ()).Next ());
			boundary.Append ("__");

			return boundary.ToString();
		}

		protected void OnSendCompleted (AsyncCompletedEventArgs e)
		{
			if (SendCompleted != null)
				SendCompleted (this, e);
		}

		[MonoTODO ("Need to work on message attachments.")]
		public void Send (MailMessage message)
		{
			SmtpResponse status;
			Sender sender = new Sender (Host, Port);

			string hostname = Dns.GetHostName ();
			string boundary = GenerateBoundary ();
			string messageID = String.Format ("<{0}@{1}>", Guid.NewGuid ().ToString ("n").ToUpper (), hostname);

			status = sender.Read ();
			if (IsError (status))
				throw new SmtpException (status.StatusCode);

			// HELO
			status = sender.SendCommand (Command.Helo, hostname);
			if (IsError (status))
				throw new SmtpException (status.StatusCode);

			// MAIL FROM:
			status = sender.SendCommand (Command.MailFrom, message.From.Address);
			if (IsError (status))
				throw new SmtpException (status.StatusCode);

			// Send RCPT TO: for all recipients in the To list
			for (int i = 0; i < message.To.Count; i += 1) {
				status = sender.SendCommand (Command.RcptTo, message.To [i].Address);
				if (IsError (status))
					throw new SmtpException (status.StatusCode);
			}

			// Send RCPT TO: for all recipients in the CC list
			for (int i = 0; i < message.CC.Count; i += 1) {
				status = sender.SendCommand (Command.RcptTo, message.CC [i].Address);
				if (IsError (status))
					throw new SmtpException (status.StatusCode);
			}

			// Send RCPT TO: for all recipients in the Bcc list
			for (int i = 0; i < message.Bcc.Count; i += 1) {
				status = sender.SendCommand (Command.RcptTo, message.Bcc [i].Address);
				if (IsError (status))
					throw new SmtpException (status.StatusCode);
			}

			// DATA
			status = sender.SendCommand (Command.Data);
			if (IsError (status))
				throw new SmtpException (status.StatusCode);

			// Send message headers
			sender.SendHeader (HeaderName.From, message.From.ToString ());
			sender.SendHeader (HeaderName.To, CreateAddressList (message.To));
			if (message.CC.Count > 0)
				sender.SendHeader (HeaderName.Cc, CreateAddressList (message.CC));
			if (message.Bcc.Count > 0)
				sender.SendHeader (HeaderName.Bcc, CreateAddressList (message.Bcc));
			sender.SendHeader (HeaderName.Subject, message.Subject);
			sender.SendHeader (HeaderName.MessageId, messageID);

			foreach (string s in message.Headers.AllKeys)
				sender.SendHeader (s, message.Headers [s]);

			bool isMultipart = (message.Attachments.Count > 0 || message.AlternateViews.Count > 0);

			if (isMultipart) {
				ContentType contentType = new ContentType ();

				contentType.Boundary = boundary;

				if (message.Attachments.Count > 0)
					contentType.MediaType = "multipart/mixed";
				else
					contentType.MediaType = "multipart/related";

				sender.SendHeader ("Content-Type", contentType.ToString ());
				sender.StartSection (boundary, message.BodyContentType);
			}
			else {
				sender.SendHeader ("Content-Type", message.BodyContentType.ToString ());
				sender.Send ("");
			}

			sender.Send (message.Body);

			if (message.AlternateViews.Count > 0) {
				ContentType contentType = new ContentType ("multipart/related");
				contentType.Boundary = GenerateBoundary ();
				sender.StartSection (boundary, contentType);
				SendAttachments (sender, message.AlternateViews, contentType);
			}

			if (message.Attachments.Count > 0) {
				ContentType contentType = new ContentType ("multipart/mixed");
				contentType.Boundary = GenerateBoundary ();
				sender.StartSection (boundary, contentType);
				SendAttachments (sender, message.Attachments, contentType);
			}

			if (isMultipart)
				sender.EndSection (boundary);

			sender.Send (".");

			status = sender.Read ();
			if (IsError (status))
				throw new SmtpException (status.StatusCode);

			status = sender.SendCommand (Command.Quit);

			sender.Close ();
		}

		private string CreateAddressList (Collection<MailAddress> addressList)
		{
			if (addressList.Count > 0) {
				StringBuilder sb = new StringBuilder ();
				for (int i = 0; i < addressList.Count; i += 1) {
					if (sb.Length > 0)
						sb.Append (", ");
					sb.Append (addressList [i].ToString ());
				}
				return sb.ToString ();
			}

			return null;
		}

		public void Send (string from, string to, string subject, string body)
		{
			Send (new MailMessage (from, to, subject, body));
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

		private void SendAttachments (Sender sender, Collection<Attachment> attachments, ContentType contentType)
		{
			for (int i = 0; i < attachments.Count; i += 1) {
				sender.StartSection (contentType.Boundary, attachments [i].ContentType);
				sender.Send ("TO BE IMPLEMENTED");
			}
			sender.EndSection (contentType.Boundary);
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

		// The Sender class is used to manage sending information to the SMTP server.
		private class Sender
		{
			#region Fields

			TcpClient client;
			NetworkStream stream;
			StreamWriter writer;
			StreamReader reader;

			#endregion // Fields

			#region Constructors

			public Sender (String host, int port)
			{
				client = new TcpClient (host, port);
				stream = client.GetStream ();
				writer = new StreamWriter (stream);
				reader = new StreamReader (stream);
			}

			#endregion // Constructors

			#region Methods

			public void Close ()
			{
				writer.Close ();
				reader.Close ();
				stream.Close ();
				client.Close ();
			}

			public void EndSection (string section)
			{
				Send (String.Format ("--{0}--", section));
			}

			public void Send (string data)
			{
				writer.WriteLine (data);
				writer.Flush ();
			}

			public SmtpResponse SendCommand (string command)
			{
				writer.WriteLine (command);
				writer.Flush ();
				return Read ();
			}

			public SmtpResponse SendCommand (string command, string data)
			{
				SmtpResponse response;
				writer.Write (command);
				writer.Write (" ");
				Send (data);

				return Read ();
			}

			public void SendHeader (string name, string value)
			{
				Send (String.Format ("{0}: {1}", name, value));
			}

			public SmtpResponse Read ()
			{
				SmtpResponse response;

				char[] buf = new char [3];
				reader.Read (buf, 0, 3);
				reader.Read ();

				response.StatusCode = (SmtpStatusCode) Int32.Parse (new String (buf));
				response.Description = reader.ReadLine ();

				return response;
			}

			public void StartSection (string section, ContentType sectionContentType)
			{
				Send (String.Format ("--{0}", section));
				SendHeader ("Content-Type", sectionContentType.ToString ());
				Send ("");
			}

			#endregion // Methods
		}
	}
}

#endif // NET_2_0
