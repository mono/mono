// Adapted from bug #78085 by Simon Brys

using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

class Test {

	public static void Main (string[] args)
	{
		new Poller (args).PollingLoop ();
	}
}

class Poller : ICertificatePolicy {

	private const int DEFAULTPOLLRATE = 5*1000;

	private int _pollRate;
	private ArrayList _polledObjects;

	public Poller (string[] args)
	{
		ServicePointManager.CertificatePolicy = this;

		_pollRate = DEFAULTPOLLRATE;

		_polledObjects = new ArrayList ();
		if (args.Length > 0) {
			// poll from user supplied list of web sites
			for (int i = 0; i < args.Length; i++) {
			
				if (args [i].StartsWith ("--") && (i < args.Length - 1)) {
					switch (args [i]) {
					case "--rate":
						_pollRate = Convert.ToInt32 (args [++i]);
						break;
					}
				} else {
					_polledObjects.Add (new PolledObject (args [i]));
				}
			}
		} else {
			// default sites to poll
			_polledObjects.Add (new PolledObject ("https://www.example.org/"));
			_polledObjects.Add (new PolledObject ("https://www.example.com/"));
		}
	}

	public bool CheckValidationResult (ServicePoint servicePoint, X509Certificate certificate,
		WebRequest webRequest, int certificateProblem)
	{
		return true;
	}

	public void PollingLoop ()
	{
		while (true) {
			foreach (PolledObject polledObject in _polledObjects) {
				polledObject.Poll ();
			}

			Console.WriteLine ("Waiting {0} ms...", _pollRate);
			Thread.Sleep (_pollRate);
		}
	}
}

class PolledObject {

	private const int MAXRESULTLENGTH = 20;
	private string _url;

	public PolledObject (string url) 
	{
		_url = url;
	}

	public void Poll ()
	{
		try {
			Uri uri = new Uri (_url);
			HttpWebRequest httpWebRequest = (HttpWebRequest) WebRequest.Create (uri);
			
			WebResponse httpWebResponse = httpWebRequest.GetResponse ();
			Stream responseStream = httpWebResponse.GetResponseStream ();
			StreamReader responseStreamReader = new StreamReader (responseStream);
			string response = responseStreamReader.ReadLine ();
			responseStreamReader.Close ();
			responseStream.Close ();
			httpWebResponse.Close ();

			Console.WriteLine ("Response for {0}: {1}", _url, 
				response.Substring (0, response.Length > MAXRESULTLENGTH ? MAXRESULTLENGTH : response.Length));
		}
		catch (WebException e) {
			Console.WriteLine ("*** WebException raised in Poll() for {0}!", _url);
			Console.WriteLine ("\tSource: {0}", e.Source);
			Console.WriteLine ("\tMessage: {0}", e.Message);
			Console.WriteLine ("\tStatus: {0}", e.Status);
		}
		catch (Exception e) {
			Console.WriteLine ("*** Exception raised in Poll() for {0}!", _url);
			Console.WriteLine ("\tSource: {0}", e.Source);
			Console.WriteLine ("\tMessage: {0}", e.Message);
		}
	}
}
