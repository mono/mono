//
// System.Net.FtpWebResponse.cs
//
// Authors:
//	Carlos Alberto Cortez (calberto.cortez@gmail.com)
//
// (c) Copyright 2006 Novell, Inc. (http://www.novell.com)
//

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Net;

#if NET_2_0

namespace System.Net
{
	public class FtpWebResponse : WebResponse
	{
		Stream stream;
		Uri uri;
		FtpStatusCode statusCode;
		DateTime lastModified = DateTime.MinValue;
		string bannerMessage = String.Empty;
		string welcomeMessage = String.Empty;
		string exitMessage = String.Empty;
		string statusDescription;
		string method;
		//bool keepAlive;
		bool disposed;
		internal long contentLength = -1;
		
		internal FtpWebResponse (Uri uri, string method, bool keepAlive)
		{
			this.uri = uri;
			this.method = method;
			//this.keepAlive = keepAlive;
		}

		internal FtpWebResponse (Uri uri, string method, FtpStatusCode statusCode, string statusDescription) {
			this.uri = uri;
			this.method = method;
			this.statusCode = statusCode;
			this.statusDescription = statusDescription;
		}

		internal FtpWebResponse (Uri uri, string method, FtpStatus status) :
			this (uri, method, status.StatusCode, status.StatusDescription)
		{
		}
		
		public override long ContentLength {
			get {
				return contentLength;
			}
		}

		public override WebHeaderCollection Headers {
			get {
				return new WebHeaderCollection ();
			}
		}

		public override Uri ResponseUri {
			get {
				return uri;
			}
		}

		public DateTime LastModified {
			get {
				return lastModified;
			}
			internal set {
				lastModified = value;
			}
		}

		public string BannerMessage {
			get {
				return bannerMessage;
			}
			internal set {
				bannerMessage = value;
			}
		}
		
		public string WelcomeMessage {
			get {
				return welcomeMessage;
			}
			internal set {
				welcomeMessage = value;
			}
		}

		public string ExitMessage {
			get {
				return exitMessage;
			}
			internal set {
				exitMessage = value;
			}
		}

		public FtpStatusCode StatusCode {
			get {
				return statusCode;
			}
			private set {
				statusCode = value;
			}
		}

		public string StatusDescription {
			get {
				return statusDescription;
			}
			private set {
				statusDescription = value;
			}
		}
		
		public override void Close ()
		{
			if (disposed)
				return;
			
			disposed = true;
			if (stream != null)
				stream.Close ();
			stream = null;
		}

		public override Stream GetResponseStream ()
		{
			if (stream == null)
				return Stream.Null; // After a STOR we get this
			
			if (method != WebRequestMethods.Ftp.DownloadFile &&
					method != WebRequestMethods.Ftp.ListDirectory)
				CheckDisposed ();
			
			return stream;
		}

		internal Stream Stream {
			set {
				stream = value;
			}

			get { return stream; }
		}

		internal void UpdateStatus (FtpStatus status) {
			statusCode = status.StatusCode;
			statusDescription = status.StatusDescription;
		}

		void CheckDisposed ()
		{
			if (disposed)
				throw new ObjectDisposedException (GetType ().FullName);
		}

		internal bool IsFinal () {
			return ((int) statusCode >= 200);
		}
	}
}

#endif

