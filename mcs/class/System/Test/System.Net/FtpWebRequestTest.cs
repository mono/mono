//
// FtpWebRequestTest.cs - NUnit Test Cases for System.Net.FtpWebRequest
//
// Authors:
//	Carlos Alberto Cortez <calberto.cortez@gmail.com>
// 	Gonzalo Paniagua Javier <gonzalo@novell.com>
//
// Copyright (c) 2006 Novell, Inc. (http://www.novell.com)
//
#if NET_2_0
using NUnit.Framework;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace MonoTests.System.Net 
{
	[TestFixture]
	public class FtpWebRequestTest
	{
		FtpWebRequest defaultRequest;
		
		[TestFixtureSetUp]
		public void Init ()
		{
			defaultRequest = (FtpWebRequest) WebRequest.Create ("ftp://www.contoso.com");
		}
		
		[Test]
		public void ContentLength ()
		{
			try {
				long l = defaultRequest.ContentLength;
			} catch (NotSupportedException) {
				Assert.Fail ("#1"); // Not overriden
			}

			try {
				defaultRequest.ContentLength = 2;
			} catch (NotSupportedException) {
				Assert.Fail ("#2"); // Not overriden
			}
		}

		[Test]
		public void ContentType ()
		{
			try {
				string t = defaultRequest.ContentType;
				Assert.Fail ("#1");
			} catch (NotSupportedException) {
			}

			try {
				defaultRequest.ContentType = String.Empty;
				Assert.Fail ("#2");
			} catch (NotSupportedException) {
			}
		}

		[Test]
		public void ContentOffset ()
		{
			try {
				defaultRequest.ContentOffset = -2;
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException) {
			}
		}

		[Test]
		public void Credentials ()
		{
			try {
				defaultRequest.Credentials = null;
				Assert.Fail ("#1");
			} catch (ArgumentNullException) {
			}

		}

		[Test]
		public void Method ()
		{
			try {
				defaultRequest.Method = null;
				Assert.Fail ("#1");
			} catch (ArgumentNullException) {
			}

			try {
				defaultRequest.Method = String.Empty;
				Assert.Fail ("#2");
			} catch (ArgumentException) {
			}

			try {
				defaultRequest.Method = "WrongValue";
				Assert.Fail ("#3");
			} catch (ArgumentException) {
			}
		}

		[Test]
		public void PreAuthenticate ()
		{
			try {
				bool p = defaultRequest.PreAuthenticate;
				Assert.Fail ("#1");
			} catch (NotSupportedException) {
			}

			try {
				defaultRequest.PreAuthenticate = true;
			} catch (NotSupportedException) {
			}
		}

		[Test]
		public void ReadWriteTimeout ()
		{
			try {
				defaultRequest.ReadWriteTimeout = -2;
				Assert.Fail ("#2");
			} catch (ArgumentOutOfRangeException) {
			}
		}

		[Test]
		public void Timeout ()
		{
			try {
				defaultRequest.Timeout = -2;
				Assert.Fail ("#2");
			} catch (ArgumentOutOfRangeException) {
			}
		}
		
		[Test]
		public void DefaultValues ()
		{
			FtpWebRequest request = (FtpWebRequest) WebRequest.Create ("ftp://www.contoso.com");
			
			Assert.AreEqual (0, request.ContentOffset, "ContentOffset");
			Assert.AreEqual (false, request.EnableSsl, "EnableSsl");
			// FIXME: Disabled this one by now. KeepAlive is not well supported.
			// Assert.AreEqual (true, request.KeepAlive, "KeepAlive");
			Assert.AreEqual (WebRequestMethods.Ftp.DownloadFile, request.Method, "#1");
			Assert.AreEqual (300000, request.ReadWriteTimeout, "ReadWriteTimeout");
			Assert.IsNull (request.RenameTo, "RenameTo");
			Assert.AreEqual (true, request.UseBinary, "UseBinary");
			Assert.AreEqual (100000, request.Timeout, "Timeout");
			Assert.AreEqual (true, request.UsePassive, "UsePassive");
		}

		[Test]
		public void RenameTo ()
		{
			try {
				defaultRequest.RenameTo = null;
				Assert.Fail ("#1");
			} catch (ArgumentException) {
			}

			try {
				defaultRequest.RenameTo = String.Empty;
				Assert.Fail ("#2");
			} catch (ArgumentException) {
			}
		}

		[Test]
		public void UploadFile1 ()
		{
			ServerPut sp = new ServerPut ();
			sp.Start ();
			string uri = String.Format ("ftp://{0}:{1}/uploads/file.txt", sp.IPAddress, sp.Port);
			try {
				FtpWebRequest ftp = (FtpWebRequest) WebRequest.Create (uri);
				ftp.Timeout = 5000;
				ftp.Method = WebRequestMethods.Ftp.UploadFile;
				ftp.ContentLength = 1;
				ftp.UseBinary = true;
				Stream stream = ftp.GetRequestStream ();
				stream.WriteByte (0);
				stream.Close ();
				FtpWebResponse response = (FtpWebResponse) ftp.GetResponse ();
				Assert.IsTrue ((int) response.StatusCode >= 200 && (int) response.StatusCode < 300, "#01");
			} catch (Exception e) {
				throw new Exception (sp.Where);
			} finally {
				sp.Stop ();
			}
		}

		class ServerPut : FtpServer {
			public string Where = "";

			protected override void Run ()
			{
				Socket client = control.Accept ();
				NetworkStream ns = new NetworkStream (client, false);
				StreamWriter writer = new StreamWriter (ns, Encoding.ASCII);
				writer.WriteLine ("220 Welcome to the jungle");
				writer.Flush ();
				StreamReader reader = new StreamReader (ns, Encoding.ASCII);
				string str = reader.ReadLine ();
				if (!str.StartsWith ("USER ")) {
					Where = "USER";
					client.Close ();
					return;
				}
				writer.WriteLine ("331 Say 'Mellon'");
				writer.Flush ();
				str = reader.ReadLine ();
				if (!str.StartsWith ("PASS ")) {
					Where = "PASS";
					client.Close ();
					return;
				}
				writer.WriteLine ("230 Logged in");
				writer.Flush ();
				str = reader.ReadLine ();
				if (!str.StartsWith ("PWD")) {
					Where = "PWD";
					client.Close ();
					return;
				}
				writer.WriteLine ("257 \"/home/someuser\"");
				writer.Flush ();
				str = reader.ReadLine ();
				if (str != ("CWD /home/someuser/uploads/")) {
					Where = "CWD - " + str;
					client.Close ();
					return;
				}
				writer.WriteLine ("250 Directory changed");
				writer.Flush ();
				str = reader.ReadLine ();
				if (str != ("TYPE I")) {
					Where = "TYPE - " + str;
					client.Close ();
					return;
				}
				writer.WriteLine ("200 Switching to binary mode");
				writer.Flush ();
				str = reader.ReadLine ();
				if (str != "PASV") {
					Where = "PASV";
					client.Close ();
					return;
				}

				IPEndPoint end_data = (IPEndPoint) data.LocalEndPoint;
				byte [] addr_bytes = end_data.Address.GetAddressBytes ();
				byte [] port = new byte [2];
				port[0] = (byte) ((end_data.Port >> 8) & 255);
				port[1] = (byte) (end_data.Port & 255);
				StringBuilder sb = new StringBuilder ("227 Passive (");
				foreach (byte b in addr_bytes) {
					sb.AppendFormat ("{0},", b);	
				}
				sb.AppendFormat ("{0},", port [0]);	
				sb.AppendFormat ("{0})", port [1]);	
				writer.WriteLine (sb.ToString ());
				writer.Flush ();

				str = reader.ReadLine ();
				if (str != "STOR file.txt") {
					Where = "STOR - " + str;
					client.Close ();
					return;
				}
				writer.WriteLine ("150 Ok to send data");
				writer.Flush ();

				Socket data_cnc = data.Accept ();
				byte [] dontcare = new byte [1];
				data_cnc.Receive (dontcare, 1, SocketFlags.None);
				data_cnc.Close ();
				writer.WriteLine ("226 File received Ok");
				writer.Flush ();
				str = reader.ReadLine ();
				if (str != "QUIT") {
					Where = "QUIT";
					client.Close ();
					return;
				}
				writer.WriteLine ("220 Bye");
				writer.Flush ();
				Thread.Sleep (250);
				client.Close ();
			}
		}

		abstract class FtpServer
		{
			protected Socket control;
			protected Socket data;
			protected Exception error;
			protected ManualResetEvent evt;

			public FtpServer ()
			{
				control = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				control.Bind (new IPEndPoint (IPAddress.Loopback, 0));
				control.Listen (1);
				data = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				data.Bind (new IPEndPoint (IPAddress.Loopback, 0));
				data.Listen (1);
			}

			public void Start ()
			{
				evt = new ManualResetEvent (false);
				Thread th = new Thread (new ThreadStart (Run));
				th.Start ();
			}

			public void Stop ()
			{
				evt.Set ();
				data.Close ();
				control.Close ();
			}
			
			public IPAddress IPAddress {
				get { return ((IPEndPoint) control.LocalEndPoint).Address; }
			}
			
			public int Port {
				get { return ((IPEndPoint) control.LocalEndPoint).Port; }
			}

			public Exception Error { 
				get { return error; }
			}

			protected abstract void Run ();
		}

	}
}

#endif

