//
// HttpServerChannelTest.cs
//	- Unit tests for System.Runtime.Remoting.Channels.Http
//
// Author: Jeffrey Stedfast <fejj@novell.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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

using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using System.Reflection;
using System.Net.Sockets;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;
using System.Threading;
using NUnit.Framework;

namespace MonoTests.Remoting {
	[TestFixture]
	public class HttpServerChannelTests {
		HttpServerChannel serverChannel;
		int port = 9090;
		
		[TestFixtureSetUp]
		public void StartHttpServer ()
		{
			if (serverChannel != null)
				return;
			
			serverChannel = new HttpServerChannel ("HttpServerChannelTests", port);
			ChannelServices.RegisterChannel (serverChannel);
			
			RemotingConfiguration.RegisterWellKnownServiceType (
				typeof (RemoteObject), "RemoteObject.rem", 
				WellKnownObjectMode.Singleton);
		}
		
		[TestFixtureTearDown]
		public void StopHttpServer ()
		{
			ChannelServices.UnregisterChannel (serverChannel);
		}
		
		struct ParseURLTestCase {
			public readonly string input;
			public readonly string retval;
			public readonly string objectURI;
			
			public ParseURLTestCase (string s0, string s1, string s2)
			{
				input = s0;
				retval = s1;
				objectURI = s2;
			}
		};
		
		ParseURLTestCase[] ParseURLTests = new ParseURLTestCase[] {
			//new ParseURLTestCase ("http:", "http:", null),  // KnownFailure but works on Microsoft's .NET
			new ParseURLTestCase ("http://", "http://", null),
			new ParseURLTestCase ("http:localhost", null, null),
			new ParseURLTestCase ("ftp://localhost", null, null),
			new ParseURLTestCase ("http://localhost", "http://localhost", null),
			new ParseURLTestCase ("hTtP://localhost", "hTtP://localhost", null),
			new ParseURLTestCase ("https://localhost", "https://localhost", null),
			new ParseURLTestCase ("http://localhost:/", "http://localhost:", "/"),
			new ParseURLTestCase ("http://localhost:9090", "http://localhost:9090", null),
			new ParseURLTestCase ("http://localhost:9090/", "http://localhost:9090", "/"),
			new ParseURLTestCase ("http://localhost:9090/RemoteObject.rem", "http://localhost:9090", "/RemoteObject.rem"),
			new ParseURLTestCase ("http://localhost:q24691247abc1297/RemoteObject.rem", "http://localhost:q24691247abc1297", "/RemoteObject.rem"),
		};
		
		[Test] // HttpChannel.Parse ()
		[Ignore ("Fails on MS")]
		public void ParseURL ()
		{
			HttpChannel channel;
			int i;
			
			channel = new HttpChannel ();
			
			for (i = 0; i < ParseURLTests.Length; i++) {
				string retval, objectURI;
				
				retval = channel.Parse (ParseURLTests[i].input, out objectURI);
				
				Assert.AreEqual (ParseURLTests[i].retval, retval);
				Assert.AreEqual (ParseURLTests[i].objectURI, objectURI);
			}
		}
		
		static void Send (NetworkStream stream, string str)
		{
			byte [] buf = Encoding.ASCII.GetBytes (str);
			
			Send (stream, buf);
		}
		
		static void Send (NetworkStream stream, byte[] buf)
		{
			//Console.Write ("C: ");
			//DumpByteArray (buf, 3);
			//Console.Write ("\n");
			
			stream.Write (buf, 0, buf.Length);
		}
		
		static int Receive (NetworkStream stream, int chunks, out byte[] buf)
		{
			byte[] buffer = new byte [4096];
			int n, nread = 0;
			
			do {
				if ((n = stream.Read (buffer, nread, buffer.Length - nread)) > 0)
					nread += n;
				
				chunks--;
			} while (n > 0 && chunks > 0);
			
			//Console.Write ("S: ");
			if (nread > 0) {
				buf = new byte [nread];
				
				for (int i = 0; i < nread; i++)
					buf[i] = buffer[i];
				
				//DumpByteArray (buf, 3);
				//Console.Write ("\n");
			} else {
				//Console.Write ("(null)\n");
				buf = null;
			}
			
			return nread;
		}
		
		static string ByteArrayToString (byte[] buf, int indent)
		{
			StringBuilder sb = new StringBuilder ();
			
			for (int i = 0; i < buf.Length; i++) {
				if (!Char.IsControl ((char) buf[i])) {
					sb.Append ((char) buf[i]);
				} else if (buf[i] == '\r') {
					sb.Append ("\\r");
				} else if (buf[i] == '\n') {
					sb.Append ("\\n\n");
					for (int j = 0; j < indent; j++)
						sb.Append (' ');
				} else {
					sb.Append (String.Format ("\\x{0:x2}", buf[i]));
				}
			}
			
			return sb.ToString ();
		}
		
		static void DumpByteArray (byte[] buf, int indent)
		{
			Console.Write (ByteArrayToString (buf, indent));
		}
		
		static int GetResponseContentOffset (byte[] response)
		{
			for (int i = 0; i < response.Length - 3; i++) {
				if (response[i + 0] == '\r' && response[i + 1] == '\n' &&
				    response[i + 2] == '\r' && response[i + 3] == '\n')
					return i + 3;
			}
			
			return -1;
		}
		
		static bool ResponseMatches (byte[] expected, byte[] actual)
		{
			int i, j;
			
			// First, we compare the first line of the response - they should match
			for (i = 0; i < expected.Length && i < actual.Length; i++) {
				if (actual[i] != expected[i]) {
					// HTTP/1.1 vs HTTP/1.0
					if (i == 7 && expected[0] == 'H' && expected[1] == 'T' && expected[2] == 'T' &&
					    expected[3] == 'P' && expected[4] == '/' && expected[5] == '1' && expected[6] == '.' &&
					    expected[7] == '1' && actual[7] == '0')
						continue;
					
					//Console.WriteLine ("\nFirst line of actual response did not match");
					return false;
				}
				
				if (expected[i] == '\n')
					break;
			}
			
			if (i >= actual.Length) {
				//Console.WriteLine ("Actual response too short");
				return false;
			}
			
			// now compare the content
			i = GetResponseContentOffset (expected);
			j = GetResponseContentOffset (actual);
			
			for ( ; i < expected.Length && j < actual.Length; i++, j++) {
				if (actual[j] != expected[i]) {
					//Console.WriteLine ("Content of actual response did not match");
					return false;
				}
			}
			
			if (i < expected.Length) {
				//Console.WriteLine ("Expected more content data...");
				return false;
			}
			
			if (j < actual.Length) {
				//Console.WriteLine ("Got too much content data in the server response");
				return false;
			}
			
			return true;
		}
		
		static void CreateBinaryMethodInvoke (string assemblyName, string objectName, string methodName, out byte[] content)
		{
			string text = String.Format ("{0}, {1}, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", objectName, assemblyName);
			byte[] lead = new byte [] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
						    0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
						    0x00, 0x15, 0x11, 0x00, 0x00, 0x00, 0x12 };
			byte[] buf;
			int index;
			
			content = new byte [lead.Length + 1 + methodName.Length + 1 + 1 + text.Length + 1];
			lead.CopyTo (content, 0);
			index = lead.Length;
			
			buf = Encoding.ASCII.GetBytes (methodName);
			content[index++] = (byte) buf.Length;
			buf.CopyTo (content, index);
			index += buf.Length;
			
			content[index++] = (byte) 0x12;
			
			buf = Encoding.ASCII.GetBytes (text);
			content[index++] = (byte) buf.Length;
			buf.CopyTo (content, index);
			index += buf.Length;
			
			content[index] = (byte) 0x0b;
		}
		
		[Test]
		[Category ("NotWorking")] // the faked request content string might be wrong?
		public void TestBinaryTransport ()
		{
			string assemblyName = Assembly.GetExecutingAssembly ().GetName ().Name;
			Socket sock = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			sock.Connect (new IPEndPoint (IPAddress.Loopback, port));
			NetworkStream stream = new NetworkStream (sock);
			byte[] content, buf, outbuf;
			
			CreateBinaryMethodInvoke (assemblyName, typeof (RemoteObject).FullName, "ReturnOne", out content);
			
			// send our POST request
			Send (stream, String.Format ("POST /RemoteObject.rem HTTP/1.1\r\n" +
						     "User-Agent: Mozilla/4.0+(compatible; MSIE 6.0; Windows 6.0.6000.0; MS .NET Remoting; MS .NET CLR 2.0.50727.1433 )\r\n" +
						     "Content-Type: application/octet-stream\r\n" +
						     "Host: 127.0.0.1:{0}\r\n" +
						     "Content-Length: {1}\r\n" +
						     "Expect: 100-continue\r\n" +
						     "Connection: Keep-Alive\r\n" +
						     "\r\n", port, content.Length));
			
			// create our expected response buffer
			buf = Encoding.ASCII.GetBytes ("HTTP/1.1 100 Continue\r\n\r\n");
			Receive (stream, 1, out outbuf);
			
			Assert.IsNotNull (outbuf, "Server continuation response is null");
			Assert.IsTrue (ResponseMatches (buf, outbuf), "Unexpected server continuation response:\n" + ByteArrayToString (outbuf, 0));
			
			// send our content data
			Send (stream, content);
			
			// create our expected response buffer
			buf = new byte[] { 0x48, 0x54, 0x54, 0x50, 0x2f, 0x31, 0x2e, 0x31, 
					   0x20, 0x32, 0x30, 0x30, 0x20, 0x4f, 0x4b, 0x0d, 
					   0x0a, 0x43, 0x6f, 0x6e, 0x74, 0x65, 0x6e, 0x74, 
					   0x2d, 0x54, 0x79, 0x70, 0x65, 0x3a, 0x20, 0x61, 
					   0x70, 0x70, 0x6c, 0x69, 0x63, 0x61, 0x74, 0x69, 
					   0x6f, 0x6e, 0x2f, 0x6f, 0x63, 0x74, 0x65, 0x74, 
					   0x2d, 0x73, 0x74, 0x72, 0x65, 0x61, 0x6d, 0x0d, 
					   0x0a, 0x53, 0x65, 0x72, 0x76, 0x65, 0x72, 0x3a, 
					   0x20, 0x4d, 0x53, 0x20, 0x2e, 0x4e, 0x45, 0x54, 
					   0x20, 0x52, 0x65, 0x6d, 0x6f, 0x74, 0x69, 0x6e, 
					   0x67, 0x2c, 0x20, 0x4d, 0x53, 0x20, 0x2e, 0x4e, 
					   0x45, 0x54, 0x20, 0x43, 0x4c, 0x52, 0x20, 0x32, 
					   0x2e, 0x30, 0x2e, 0x35, 0x30, 0x37, 0x32, 0x37, 
					   0x2e, 0x31, 0x34, 0x33, 0x33, 0x0d, 0x0a, 0x43, 
					   0x6f, 0x6e, 0x74, 0x65, 0x6e, 0x74, 0x2d, 0x4c, 
					   0x65, 0x6e, 0x67, 0x74, 0x68, 0x3a, 0x20, 0x32, 
					   0x38, 0x0d, 0x0a, 0x0d, 0x0a, 0x00, 0x00, 0x00, 
					   0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 
					   0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x16, 0x11, 
					   0x08, 0x00, 0x00, 0x08, 0x01, 0x00, 0x00, 0x00, 
					   0x0b };
			
			Receive (stream, 2, out outbuf);
			
			Assert.IsNotNull (outbuf, "Server method-invoke response is null");
			Assert.IsTrue (ResponseMatches (buf, outbuf), "Unexpected server method-invoke response:\n" + ByteArrayToString (outbuf, 0));
			
			stream.Close ();
		}
		
		[Test]
		[Category ("NotWorking")] // The test itself passes, but the runtime goes infinite loop at end of nunit-console udner 2.4.8.
		public void TestSoapTransport ()
		{
			string assemblyName = Assembly.GetExecutingAssembly ().GetName ().Name;
			Socket sock = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			sock.Connect (new IPEndPoint (IPAddress.Loopback, port));
			NetworkStream stream = new NetworkStream (sock);
			string methodName = "ReturnOne";
			string headers, content;
			byte[] buf, outbuf;
			
			content = String.Format ("<SOAP-ENV:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" " +
						 "xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" " +
						 "xmlns:SOAP-ENC=\"http://schemas.xmlsoap.org/soap/encoding/\" " +
						 "xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" " +
						 "xmlns:clr=\"http://schemas.microsoft.com/soap/encoding/clr/1.0/\" " +
						 "SOAP-ENV:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">\r\n" +
						 "<SOAP-ENV:Body>\r\n" +
						 "<i2:{0} id=\"ref-1\" xmlns:i2=\"http://schemas.microsoft.com/clr/nsassem/{1}/{2}\">\r\n" +
						 "</i2:{0}>\r\n" +
						 "</SOAP-ENV:Body>\r\n" +
						 "</SOAP-ENV:Envelope>", methodName, typeof (RemoteObject).FullName, assemblyName);
			
			headers = String.Format ("POST /RemoteObject.rem HTTP/1.1\r\n" +
						 "User-Agent: Mozilla/4.0+(compatible; MSIE 6.0; Windows 6.0.6000.0; MS .NET Remoting; MS .NET CLR 2.0.50727.1433 )\r\n" +
						 "Content-Type: text/xml; charset=\"utf-8\"\r\n" +
						 "SOAPAction: \"http://schemas.microsoft.com/clr/nsassem/{0}/{1}#{2}\"\r\n" +
						 "Host: 127.0.0.1:{3}\r\n" +
						 "Content-Length: {4}\r\n" +
						 "Expect: 100-continue\r\n" +
						 "Connection: Keep-Alive\r\n" +
						 "\r\n", typeof (RemoteObject).FullName, assemblyName, methodName, port, content.Length);
			
			Send (stream, headers);
			
			// create our expected response buffer
			buf = Encoding.ASCII.GetBytes ("HTTP/1.1 100 Continue\r\n\r\n");
			Receive (stream, 1, out outbuf);
			
			Assert.IsNotNull (outbuf, "Server continuation response is null");
			Assert.IsTrue (ResponseMatches (buf, outbuf), "Unexpected server continuation response:\n" + ByteArrayToString (outbuf, 0));
			
			// send our content data
			Send (stream, content);
			
			// create our expected response buffer
#if MICROSOFT_DOTNET_SERVER
			content = String.Format ("<SOAP-ENV:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" " +
						 "xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" " +
						 "xmlns:SOAP-ENC=\"http://schemas.xmlsoap.org/soap/encoding/\" " +
						 "xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" " +
						 "xmlns:clr=\"http://schemas.microsoft.com/soap/encoding/clr/1.0\" " +
						 "SOAP-ENV:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">\r\n" +
						 "<SOAP-ENV:Body>\r\n" +
						 "<i2:{0}Response id=\"ref-1\" xmlns:i2=\"http://schemas.microsoft.com/clr/nsassem/{1}/{2}\">\r\n" +
						 "<return>1</return>\r\n" +
						 "</i2:{0}Response>\r\n" +
						 "</SOAP-ENV:Body>\r\n" +
						 "</SOAP-ENV:Envelope>\r\n", methodName, typeof (RemoteObject).FullName, assemblyName);
#else
			//slight differences in formatting
			content = String.Format ("<SOAP-ENV:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" " +
						 "xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" " +
						 "xmlns:SOAP-ENC=\"http://schemas.xmlsoap.org/soap/encoding/\" " +
						 "xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" " +
						 "xmlns:clr=\"http://schemas.microsoft.com/clr/\" " +
						 "SOAP-ENV:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">\n" +
						 "  <SOAP-ENV:Body>\n" +
						 "    <i2:{0}Response id=\"ref-1\" xmlns:i2=\"http://schemas.microsoft.com/clr/nsassem/{1}/{2}\">\n" +
						 "      <return xsi:type=\"xsd:int\">1</return>\n" +
						 "    </i2:{0}Response>\n" +
						 "  </SOAP-ENV:Body>\n" +
						 "</SOAP-ENV:Envelope>", methodName, typeof (RemoteObject).FullName, assemblyName);
#endif
			
			headers = String.Format ("HTTP/1.1 200 OK\r\nContent-Type: text/xml; charset=\"utf-8\"\r\n" +
						 "Server: MS .NET Remoting, MS .NET CLR 2.0.50727.1433\r\n" +
						 "Content-Length: {0}\r\n\r\n", content.Length);
			
			buf = Encoding.ASCII.GetBytes (headers + content);
			
			Receive (stream, 2, out outbuf);
			
			Assert.IsNotNull (outbuf, "Server method-invoke response is null");
			Assert.IsTrue (ResponseMatches (buf, outbuf), "Unexpected server method-invoke response:\n" + ByteArrayToString (outbuf, 0));
			
			stream.Close ();
		}
		
		object mutex = new object ();
		bool []retvals;
		
		void MultiClientStart ()
		{
			int rv = 0;
			
			// the prupose of this is just to block until all clients have been created
			lock (mutex) {
				rv++;
			}
			
			RemoteObject remObj = new RemoteObject ();
			
			rv = remObj.Increment ();
			
			// make sure the value returned hasn't been returned to another thread as well
			lock (retvals) {
				Assert.IsTrue (!retvals[rv], "RemoteObject.Increment() has already returned " + rv);
				retvals[rv] = true;
			}
		}
		
		[Test]
		[Category ("NotWorking")] // disabled as it got not working by NUnit upgrade to 2.4.8
		public void MultiClientConnection ()
		{
			int num_clients = 20;
			
			Hashtable options = new Hashtable ();
			options ["timeout"] = 10000; // 10s
			options ["name"] = "MultiClientConnection"; // 10s
			HttpClientChannel clientChannel = new HttpClientChannel (options, null);
			ChannelServices.RegisterChannel (clientChannel);
			try {
			
			WellKnownClientTypeEntry remoteType = new WellKnownClientTypeEntry (
				typeof (RemoteObject), "http://127.0.0.1:9090/RemoteObject.rem");
			RemotingConfiguration.RegisterWellKnownClientType (remoteType);
			
			// start a bunch of clients...
			Thread []clients = new Thread [num_clients];
			retvals = new bool [num_clients];
			
			lock (mutex) {
				for (int i = 0; i < num_clients; i++) {
					clients[i] = new Thread (MultiClientStart);
					clients[i].Start ();
					retvals[i] = false;
				}
			}
			
			// wait for all clients to finish...
			for (int i = 0; i < num_clients; i++)
				clients[i].Join ();
			
			for (int i = 0; i < num_clients; i++)
				Assert.IsTrue (retvals[i], "RemoteObject.Incrememnt() didn't return a value of " + i);

			} finally {
			ChannelServices.UnregisterChannel (clientChannel);
			}
		}
	}
}
