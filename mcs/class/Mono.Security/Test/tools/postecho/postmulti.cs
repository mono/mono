//
// postmulti.cs: Multi-sessions TLS/SSL Test Program
//	based on tlstest.cs, tlsmulti.cs and postecho.cs
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004-2005 Novell (http://www.novell.com)
//

using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

using Mono.Security.Protocol.Tls;

public class State {

	static ArrayList handleList = new ArrayList ();

	private int id;
	private HttpWebRequest request;
	private ManualResetEvent handle;

	public State (int id, HttpWebRequest req)
	{
		this.id = id;
		request = req;
		handle = new ManualResetEvent (false);
		handleList.Add (handle);
	}

	public int Id {
		get { return id; }
	}

	public HttpWebRequest Request {
		get { return request; }
	}

	public void Complete ()
	{
		handle.Set ();
	}

	static public void WaitAll ()
	{
		if (handleList.Count > 0) {
			WaitHandle[] handles = (WaitHandle[]) handleList.ToArray (typeof (WaitHandle));
			WaitHandle.WaitAll (handles);
			handleList.Clear ();
		}
	}
}

public class MultiTest {

	public const int buffersize = 1024 * 1024;

	static byte[] data = new byte [buffersize];

	public static void Main (string[] args) 
	{
		ServicePointManager.CertificatePolicy = new TestCertificatePolicy ();

		string postdata = "TEST=";
		byte[] bytes = Encoding.Default.GetBytes (postdata);

		// prepare test buffer
		for (int i = 0; i < buffersize; i++)
			data[i] = 65;

		int id = 1;
		foreach (string url in args) {
			Console.WriteLine ("POST #{0} at {1}", id, url);
			HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
			req.Method = "POST";
			req.ContentType = "application/x-www-form-urlencoded";
			req.ContentLength = 5 + data.Length;

			Stream output = req.GetRequestStream ();
			output.Write (bytes, 0, bytes.Length);
			output.Write (data, 0, data.Length);
			output.Close ();

			State s = new State (id++, req);
			req.BeginGetResponse (new AsyncCallback (ResponseCallback), s);
		}

		State.WaitAll ();
	}

	private static void ResponseCallback (IAsyncResult result)
	{
		State state = ((State) result.AsyncState);
		HttpWebResponse response = (HttpWebResponse) state.Request.EndGetResponse (result);

		Stream stream = response.GetResponseStream ();
		StreamReader sr = new StreamReader (stream, Encoding.UTF8);
		string received = sr.ReadToEnd ();

		if (data.Length != received.Length) {
			Console.WriteLine ("ECHO #{0} - Invalid length {1}. Expected {2}", state.Id, received.Length, data.Length);
		} else {
			bool ok = true;
			for (int i = 0; i < received.Length; i++) {
				if (received[i] != 'A') {
					ok = false;
					Console.WriteLine ("ECHO #{0} - Error at position #{1} - received '{2}'", state.Id, i, received[i]);
					break;
				}
			}
			if (ok)
				Console.WriteLine ("ECHO #{0} - Result OK (length: {1})", state.Id, received.Length);
		}

		state.Complete ();
	}

	public class TestCertificatePolicy : ICertificatePolicy {

		public bool CheckValidationResult (ServicePoint sp, X509Certificate certificate, WebRequest request, int error)
		{
			// whatever the reason we do not stop the SSL connection
			return true;
		}
	}
}
