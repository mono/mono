//
// FtpWebRequestTest.cs - NUnit Test Cases for System.Net.FtpWebRequest
//
// Authors:
//	Carlos Alberto Cortez <calberto.cortez@gmail.com>
// 	Gonzalo Paniagua Javier <gonzalo@novell.com>
//
// Copyright (c) 2006,2007,2008 Novell, Inc. (http://www.novell.com)
//
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using MonoTests.Helpers;

namespace MonoTests.System.Net 
{
	[TestFixture]
	public class FtpWebRequestTest
	{
		FtpWebRequest _defaultRequest;
		FtpWebRequest defaultRequest {
			get { return _defaultRequest ?? (_defaultRequest = (FtpWebRequest) WebRequest.Create ("ftp://www.contoso.com")); }
		}
		
		private string _tempDirectory;
		private string _tempFile;

		[SetUp]
		public void SetUp ()
		{
			_tempDirectory = new TempDirectory ();
			_tempFile = Path.Combine (_tempDirectory.Path, "FtpWebRequestTest.tmp");
		}

		[TearDown]
		public void TearDown ()
		{
			_tempDirectory.Dispose ();
		}

		[Test]
		public void ContentLength ()
		{
			try {
				long l = defaultRequest.ContentLength;
#if FEATURE_NO_BSD_SOCKETS
				Assert.Fail ("#1a");
			} catch (PlatformNotSupportedException) {
				// OK.
#else
			} catch (NotSupportedException) {
				Assert.Fail ("#1"); // Not overriden
#endif
			}

			try {
				defaultRequest.ContentLength = 2;
#if FEATURE_NO_BSD_SOCKETS
				Assert.Fail ("#2a");
			} catch (PlatformNotSupportedException) {
				// OK.
#else
			} catch (NotSupportedException) {
				Assert.Fail ("#2"); // Not overriden
#endif
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
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void ContentOffset ()
		{
			try {
				defaultRequest.ContentOffset = -2;
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException) {
			}
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Credentials ()
		{
			try {
				defaultRequest.Credentials = null;
				Assert.Fail ("#1");
			} catch (ArgumentNullException) {
			}

		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
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
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void ReadWriteTimeout ()
		{
			try {
				defaultRequest.ReadWriteTimeout = -2;
				Assert.Fail ("#2");
			} catch (ArgumentOutOfRangeException) {
			}
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Timeout ()
		{
			try {
				defaultRequest.Timeout = -2;
				Assert.Fail ("#2");
			} catch (ArgumentOutOfRangeException) {
			}
		}
		
		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
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
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
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
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void UploadFile1_v4 ()
		{
			UploadFile1 (false);
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void UploadFile1_v6 ()
		{
			if (!Socket.OSSupportsIPv6)
				Assert.Ignore ("IPv6 not supported.");

			UploadFile1 (true);
		}

		void UploadFile1 (bool ipv6)
		{
			ServerPut sp = new ServerPut (ipv6);
			sp.Start ();
			string uri = String.Format ("ftp://{0}:{1}/uploads/file.txt", EncloseIPv6 (sp.IPAddress), sp.Port);
			try {
				FtpWebRequest ftp = (FtpWebRequest) WebRequest.Create (uri);
				ftp.KeepAlive = false;
				ftp.Timeout = 5000;
				ftp.Method = WebRequestMethods.Ftp.UploadFile;
				ftp.ContentLength = 10;
				ftp.UseBinary = true;
				Stream stream = ftp.GetRequestStream ();
				for (int i = 0; i < 10; i++)
					stream.WriteByte ((byte)i);
				stream.Close ();
				FtpWebResponse response = (FtpWebResponse) ftp.GetResponse ();
				Assert.IsTrue ((int) response.StatusCode >= 200 && (int) response.StatusCode < 300, "UP#01");
				Assert.AreEqual (10, sp.result.Count, "UP#02");
				response.Close ();
			} catch (Exception) {
				if (!String.IsNullOrEmpty (sp.Where))
					throw new Exception (sp.Where);
				throw;
			} finally {
				sp.Stop ();
			}
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void UploadFile_WebClient_v4 ()
		{
			UploadFile_WebClient (false);
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void UploadFile_WebClient_v6 ()
		{
			if (!Socket.OSSupportsIPv6)
				Assert.Ignore ("IPv6 not supported.");

			UploadFile_WebClient (true);
		}

		public void UploadFile_WebClient (bool ipv6)
		{
			ServerPut sp = new ServerPut (ipv6);
			File.WriteAllText (_tempFile, "0123456789");
			sp.Start ();

			using (WebClient m_WebClient = new WebClient())
			{
				string uri = String.Format ("ftp://{0}:{1}/uploads/file.txt", EncloseIPv6 (sp.IPAddress), sp.Port);
				
				m_WebClient.UploadFile(uri, _tempFile);
			}
			Assert.AreEqual (10, sp.result.Count, "WebClient/Ftp#01");
	    
			sp.Stop ();
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void DownloadFile1_v4 ()
		{
			DownloadFile (new ServerDownload (false));
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void DownloadFile1_v6 ()
		{
			if (!Socket.OSSupportsIPv6)
				Assert.Ignore ("IPv6 not supported.");

			DownloadFile (new ServerDownload (true));
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void DownloadFileNonLatinChars ()
		{
			string filename = "\u0411\u0430\u0448\u043DRowan-\u041F\u0435\u0441\u043D\u043F\u0440\u043E\u043C\u043E\u043D\u0430\u0445\u0430\u0422\u0435\u043E\u0434\u043E\u0440\u0443\u0441\u0430\u0438\u0437\u0413\u0430\u043C\u043C\u0435\u043B\u044C\u043D\u0430.mp3";
			DownloadFile (new ServerDownload (null, null, filename, false), "ftp://{0}:{1}/" + filename);
		}

		void DownloadFile (ServerDownload sp, string uriTemplate = "ftp://{0}:{1}/file.txt")
		{
			sp.Start ();
			string uri = String.Format (uriTemplate, EncloseIPv6 (sp.IPAddress), sp.Port);
			try {
				FtpWebRequest ftp = (FtpWebRequest) WebRequest.Create (uri);
				ftp.KeepAlive = false;
				ftp.Timeout = 5000;
				ftp.Method = WebRequestMethods.Ftp.DownloadFile;
				ftp.UseBinary = true;
				FtpWebResponse response = (FtpWebResponse) ftp.GetResponse ();
				Assert.IsTrue ((int) response.StatusCode >= 100 && (int) response.StatusCode < 200, "DL#01");
				using (Stream st = response.GetResponseStream ()) {
				}
				// This should be "220 Bye" or similar (no KeepAlive)
				Assert.IsTrue ((int) response.StatusCode >= 200 && (int) response.StatusCode < 300, "DL#02");
				response.Close ();
			} catch (Exception) {
				if (!String.IsNullOrEmpty (sp.Where))
					throw new Exception (sp.Where);
				throw;
			} finally {
				sp.Stop ();
			}
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void DownloadFile2_v4 ()
		{
			// Some embedded FTP servers in Industrial Automation Hardware report
			// the PWD using backslashes, but allow forward slashes for CWD.
			DownloadFile (new ServerDownload (@"\Users\someuser", "/Users/someuser/", null, false));
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void DownloadFile2_v6 ()
		{
			if (!Socket.OSSupportsIPv6)
				Assert.Ignore ("IPv6 not supported.");

			// Some embedded FTP servers in Industrial Automation Hardware report
			// the PWD using backslashes, but allow forward slashes for CWD.
			DownloadFile (new ServerDownload (@"\Users\someuser", "/Users/someuser/", null, true));
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void DeleteFile1_v4 ()
		{
			DeleteFile1 (false);
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void DeleteFile1_v6 ()
		{
			if (!Socket.OSSupportsIPv6)
				Assert.Ignore ("IPv6 not supported.");

			DeleteFile1 (true);
		}

		void DeleteFile1 (bool ipv6)
		{
			ServerDeleteFile sp = new ServerDeleteFile (ipv6);
			sp.Start ();
			string uri = String.Format ("ftp://{0}:{1}/file.txt", EncloseIPv6 (sp.IPAddress), sp.Port);
			try {
				FtpWebRequest ftp = (FtpWebRequest) WebRequest.Create (uri);
				Console.WriteLine (ftp.RequestUri);
				ftp.KeepAlive = false;
				ftp.Timeout = 5000;
				ftp.Method = WebRequestMethods.Ftp.DeleteFile;
				ftp.UseBinary = true;
				FtpWebResponse response = (FtpWebResponse) ftp.GetResponse ();
				Assert.IsTrue ((int) response.StatusCode >= 200 && (int) response.StatusCode < 300, "DF#01");
				response.Close ();
			} catch (Exception e) {
				Console.WriteLine (e);
				if (!String.IsNullOrEmpty (sp.Where))
					throw new Exception (sp.Where);
				throw;
			} finally {
				sp.Stop ();
			}
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void ListDirectory1_v4 ()
		{
			ListDirectory1 (false);
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void ListDirectory1_v6 ()
		{
			if (!Socket.OSSupportsIPv6)
				Assert.Ignore ("IPv6 not supported.");

			ListDirectory1 (true);
		}

		void ListDirectory1 (bool ipv6)
		{
			ServerListDirectory sp = new ServerListDirectory (ipv6);
			sp.Start ();
			string uri = String.Format ("ftp://{0}:{1}/somedir/", EncloseIPv6 (sp.IPAddress), sp.Port);
			try {
				FtpWebRequest ftp = (FtpWebRequest) WebRequest.Create (uri);
				Console.WriteLine (ftp.RequestUri);
				ftp.KeepAlive = false;
				ftp.Timeout = 5000;
				ftp.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
				ftp.UseBinary = true;
				using (FtpWebResponse response = (FtpWebResponse) ftp.GetResponse ()) {
					StreamReader reader = new StreamReader (response.GetResponseStream ());
					string result = reader.ReadToEnd ();
					Assert.IsTrue ((int) response.StatusCode >= 200 && (int) response.StatusCode < 300, "DF#01");
				}
			} catch (Exception e) {
				Console.WriteLine (e);
				if (!String.IsNullOrEmpty (sp.Where))
					throw new Exception (sp.Where);
				throw;
			} finally {
				sp.Stop ();
			}
		}

		string EncloseIPv6 (IPAddress address)
		{
			if (address.AddressFamily == AddressFamily.InterNetwork)
				return address.ToString ();
			
			return String.Format ("[{0}]", address.ToString ());
		}

		class ServerListDirectory : FtpServer {
			public ServerListDirectory (bool ipv6)
				: base (ipv6)
			{
			}

			protected override void Run ()
			{
				Socket client = control.Accept ();
				NetworkStream ns = new NetworkStream (client, false);
				StreamWriter writer = new StreamWriter (ns, Encoding.ASCII);
				StreamReader reader = new StreamReader (ns, Encoding.UTF8);
				if (!DoAnonymousLogin (writer, reader)) {
					client.Close ();
					return;
				}

				if (!DoInitialDialog (writer, reader, "/home/someuser", "/home/someuser/somedir/")) {
					client.Close ();
					return;
				}

				string str = reader.ReadLine ();
				string resp = FormatPassiveResponse (str);
				if (resp == null) {
					client.Close ();
					return;
				}
				writer.WriteLine (resp);
				writer.Flush ();

				str = reader.ReadLine ();
				if (str != "LIST") {
					Where = "LIST - '" + str + "'";
					client.Close ();
					return;
				}
				writer.WriteLine ("150 Here comes the directory listing");
				writer.Flush ();

				Socket data_cnc = data.Accept ();
				byte [] dontcare = Encoding.ASCII.GetBytes ("drwxr-xr-x    2 ftp      ftp          4096 Oct 27 20:17 tests");
				data_cnc.Send (dontcare, 1, SocketFlags.None);
				data_cnc.Close ();
				writer.WriteLine ("226 Directory send Ok");
				writer.Flush ();
				if (!EndConversation (writer, reader)) {
					client.Close ();
					return;
				}
				client.Close ();
			}
		}

		class ServerDeleteFile : FtpServer {
			public ServerDeleteFile (bool ipv6)
				: base (ipv6)
			{
			}

			protected override void Run ()
			{
				Socket client = control.Accept ();
				NetworkStream ns = new NetworkStream (client, false);
				StreamWriter writer = new StreamWriter (ns, Encoding.ASCII);
				StreamReader reader = new StreamReader (ns, Encoding.UTF8);
				if (!DoAnonymousLogin (writer, reader)) {
					client.Close ();
					return;
				}

				if (!DoInitialDialog (writer, reader, "/home/someuser", "/home/someuser/")) {
					client.Close ();
					return;
				}

				string str = reader.ReadLine ();
				if (str.Trim () != "DELE file.txt") {
					Where = "DELE - " + str;
					client.Close ();
					return;
				}
				writer.WriteLine ("250 Delete operation successful");
				writer.Flush ();
				if (!EndConversation (writer, reader)) {
					client.Close ();
					return;
				}
				client.Close ();
			}
		}

		class ServerDownload : FtpServer {

			string Pwd, Cwd, Filename;

			public ServerDownload (bool ipv6)
				: this (null, null, null, ipv6)
			{
			}

			public ServerDownload (string pwd, string cwd, string filename, bool ipv6)
				: base (ipv6)
			{
				Pwd = pwd ?? "/home/someuser";
				Cwd = cwd ?? "/home/someuser/";
				Filename = filename ?? "file.txt";
			}

			protected override void Run ()
			{
				Socket client = control.Accept ();
				NetworkStream ns = new NetworkStream (client, false);
				StreamWriter writer = new StreamWriter (ns, Encoding.ASCII);
				StreamReader reader = new StreamReader (ns, Encoding.UTF8);
				if (!DoAnonymousLogin (writer, reader)) {
					client.Close ();
					return;
				}

				if (!DoInitialDialog (writer, reader, Pwd, Cwd)) {
					client.Close ();
					return;
				}

				string str = reader.ReadLine ();
				string resp = FormatPassiveResponse (str);
				if (resp == null) {
					client.Close ();
					return;
				}
				writer.WriteLine (resp);
				writer.Flush ();

				str = reader.ReadLine ();
				if (str != $"RETR {Filename}") {
					Where = $"RETR - got: {str}, expected: RETR {Filename}";
					client.Close ();
					return;
				}
				writer.WriteLine ("150 Opening BINARY mode data connection for blah (n bytes)");
				writer.Flush ();

				Socket data_cnc = data.Accept ();
				byte [] dontcare = new byte [1];
				data_cnc.Receive (dontcare, 1, SocketFlags.None);
				data_cnc.Close ();
				writer.WriteLine ("226 File send Ok");
				writer.Flush ();
				if (!EndConversation (writer, reader)) {
					client.Close ();
					return;
				}
				client.Close ();
			}
		}

		class ServerPut : FtpServer {
			public List<byte> result = new List<byte> ();
			
			public ServerPut (bool ipv6)
				: base (ipv6)
			{
			}

			protected override void Run ()
			{
				Socket client = control.Accept ();
				NetworkStream ns = new NetworkStream (client, false);
				StreamWriter writer = new StreamWriter (ns, Encoding.ASCII);
				StreamReader reader = new StreamReader (ns, Encoding.UTF8);
				if (!DoAnonymousLogin (writer, reader)) {
					client.Close ();
					return;
				}

				if (!DoInitialDialog (writer, reader, "/home/someuser", "/home/someuser/uploads/")) {
					client.Close ();
					return;
				}

				string str = reader.ReadLine ();
				string resp = FormatPassiveResponse (str);
				if (resp == null) {
					client.Close ();
					return;
				}
				writer.WriteLine (resp);
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
				var datastr = new NetworkStream (data_cnc, false);
				int ch;
				while ((ch = datastr.ReadByte ()) != -1){
					result.Add ((byte)ch);

				}
				data_cnc.Close ();
				writer.WriteLine ("226 File received Ok");
				writer.Flush ();
				if (!EndConversation (writer, reader)) {
					client.Close ();
					return;
				}
				client.Close ();
			}
		}

		abstract class FtpServer {
			protected Socket control;
			protected Socket data;
			protected ManualResetEvent evt;
			protected bool ipv6;
			public string Where = "";

			public FtpServer (bool ipv6)
			{
				control = new Socket (ipv6 ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				control.Bind (new IPEndPoint (ipv6 ? IPAddress.IPv6Loopback : IPAddress.Loopback, 0));
				control.Listen (1);
				data = new Socket (ipv6 ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				data.Bind (new IPEndPoint (ipv6 ? IPAddress.IPv6Loopback : IPAddress.Loopback, 0));
				data.Listen (1);
				this.ipv6 = ipv6;
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

			// PWD, CWD and TYPE I (type could be moved out of here)
			protected bool DoInitialDialog (StreamWriter writer, StreamReader reader, string pwd, string cwd)
			{
				string str = reader.ReadLine ();
				if (!str.StartsWith ("OPTS utf8 on")) {
					Where = "OPTS utf8 - " + str;
					return false;
				}
				writer.WriteLine ("200 Always in UTF8 mode"); // vsftpd
				writer.Flush ();
				str = reader.ReadLine ();
				if (!str.StartsWith ("PWD")) {
					Where = "PWD - " + str;
					return false;
				}
				writer.WriteLine ("257 \"{0}\"", pwd);
				writer.Flush ();
				str = reader.ReadLine ();
				if (str != ("CWD " + cwd)) {
					Where = "CWD - " + str;
					return false;
				}
				writer.WriteLine ("250 Directory changed");
				writer.Flush ();
				str = reader.ReadLine ();
				if (str != ("TYPE I")) {
					Where = "TYPE - " + str;
					return false;
				}
				writer.WriteLine ("200 Switching to binary mode");
				writer.Flush ();
				return true;
			}

			protected bool EndConversation (StreamWriter writer, StreamReader reader)
			{
				string str = reader.ReadLine ();
				if (str != "QUIT") {
					Where = "QUIT";
					return false;
				}
				writer.WriteLine ("220 Bye");
				writer.Flush ();
				Thread.Sleep (250);
				return true;
			}

			protected bool DoAnonymousLogin (StreamWriter writer, StreamReader reader)
			{
				writer.WriteLine ("220 Welcome to the jungle");
				writer.Flush ();
				string str = reader.ReadLine ();
				if (!str.StartsWith ("USER ")) {
					Where = "USER";
					return false;
				}
				writer.WriteLine ("331 Say 'Mellon'");
				writer.Flush ();
				str = reader.ReadLine ();
				if (!str.StartsWith ("PASS ")) {
					Where = "PASS";
					return false;
				}
				writer.WriteLine ("230 Logged in");
				writer.Flush ();
				return true;
			}

			protected string FormatPassiveResponse (string request)
			{
				if (ipv6) {
					if (request != "EPSV") {
						Where = "EPSV";
						return null;
					}

					IPEndPoint end_data = (IPEndPoint) data.LocalEndPoint;
					return String.Format ("229 Extended Passive (|||{0}|)", end_data.Port);
				}
				else {
					if (request != "PASV") {
						Where = "PASV";
						return null;
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
					return sb.ToString ();
				}
			}

			public IPAddress IPAddress {
				get { return ((IPEndPoint) control.LocalEndPoint).Address; }
			}
			
			public int Port {
				get { return ((IPEndPoint) control.LocalEndPoint).Port; }
			}

			protected abstract void Run ();
		}
	}
}


