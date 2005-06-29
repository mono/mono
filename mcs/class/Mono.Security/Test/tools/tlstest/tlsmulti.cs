//
// tlsmulti.cs: Multi-sessions TLS/SSL Test Program with async HttpWebRequest
//	based on tlstest.cs
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

	static bool alone;

	public static void Main (string[] args) 
	{
		if (args.Length == 0) {
			Console.WriteLine ("usage: mono tlsmulti.exe url1 [url ...]");
			return;
		} else if (args.Length > 64) {
			Console.WriteLine ("WaitHandle has a limit of 64 handles so you cannot process {0} URLs.", args.Length);
			return;
		}

		alone = (args.Length == 1);
		ServicePointManager.CertificatePolicy = new TestCertificatePolicy ();

		int id = 1;
		foreach (string url in args) {
			Console.WriteLine ("GET #{0} at {1}", id, url);
			HttpWebRequest wreq = (HttpWebRequest) WebRequest.Create (url);
			State s = new State (id++, wreq);
			wreq.BeginGetResponse (new AsyncCallback (ResponseCallback), s);
		}

		State.WaitAll ();
	}

	private static void ResponseCallback (IAsyncResult result)
	{
		State state = ((State) result.AsyncState);
		Console.WriteLine ("END #{0}", state.Id);
		HttpWebResponse response = (HttpWebResponse) state.Request.EndGetResponse (result);

		Stream stream = response.GetResponseStream ();
		StreamReader sr = new StreamReader (stream, Encoding.UTF8);
		string data = sr.ReadToEnd ();

		if (alone)
			Console.WriteLine (data);
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
