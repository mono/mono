//
// SmtpServer.cs - Dummy SMTP server used to test SmtpClient
//
// Author:
//   Raja R Harinath <harinath@hurrynot.org>
//

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace MonoTests.System.Net.Mail {
	public class SmtpServer
	{
		public string mail_from, rcpt_to;
		public StringBuilder data;

		TcpListener server;
		public IPEndPoint EndPoint {
			get { return (IPEndPoint) server.LocalEndpoint; }
		}

		public SmtpServer ()
		{
			server = new TcpListener (0);
			server.Start (1);
		}

		private static void WriteNS (NetworkStream ns, string s)
		{
			Trace ("response", s);
			byte [] bytes = Encoding.ASCII.GetBytes (s);
			ns.Write (bytes, 0, bytes.Length);
		}

		public void Run ()
		{
			string s;
			using (TcpClient client = server.AcceptTcpClient ()) {
				Trace ("connection", EndPoint.Port);
				using (NetworkStream ns = client.GetStream ()) {
					WriteNS (ns, "220 localhost\r\n");
					using (StreamReader r = new StreamReader (ns, Encoding.UTF8)) {
						while ((s = r.ReadLine ()) != null && Dispatch (ns, r, s))
							;
					}
				}
			}
		}

		// return false == terminate
		public bool Dispatch (NetworkStream ns, StreamReader r, string s)
		{
			Trace ("command", s);
			if (s.Length < 4) {
				WriteNS (ns, "502 Huh\r\n");
				return false;
			}

			bool retval = true;
			switch (s.Substring (0, 4)) {
			case "HELO":
				break;
			case "QUIT":
				WriteNS (ns, "221 Quit\r\n");
				return false;
			case "MAIL":
				mail_from = s.Substring (10);
				break;
			case "RCPT":
				rcpt_to = s.Substring (8);
				break;
			case "DATA":
				WriteNS (ns, "354 Continue\r\n");
				data = new StringBuilder ();
				while ((s = r.ReadLine ()) != null) {
					if (s == ".")
						break;
					data.AppendLine (s);
				}
				Trace ("end of data", s);
				retval = (s != null);
				break;
			default:
				WriteNS (ns, "502 Huh\r\n");
				return true;
			}

			WriteNS (ns, "250 OK\r\n");
			return retval;
		}

		[Conditional ("TEST")]
		static void Trace (string key, object value)
		{
			Console.Error.WriteLine ("{0}: {1}", key, value);
		}

#if TEST
		static void DoTest (SmtpServer s, SmtpClient c, MailMessage m)
		{
			Thread t = new Thread (s.Run);
			t.Start ();
			c.Send (m);
			t.Join ();

			Console.WriteLine ("Message From: {0}", m.From);
			Console.WriteLine ("Message Sender: {0}", m.Sender);
			Console.WriteLine ("Mail From: {0}", s.mail_from);
			Console.WriteLine ("Rcpt To: {0}", s.rcpt_to);
			Console.WriteLine ("-------------------------------------");
			Console.Write (s.data);
			Console.WriteLine ("-------------------------------------");
		}

		static void Main ()
		{
			var server = new SmtpServer ();
			var client = new SmtpClient ("localhost", server.EndPoint.Port);
			var msg = new MailMessage ("foo@example.com", "bar@example.com", "hello", "howdydoo");

			DoTest (server, client, msg);

			msg.Sender = new MailAddress ("baz@example.com");

			DoTest (server, client, msg);
		}
#endif
	}
}
