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

#if NET_2_0

namespace System.Net
{
	public class FtpWebResponse : WebResponse
	{
		Stream stream = Stream.Null;
		Uri uri;
		FtpStatusCode statusCode;
		DateTime lastModified = DateTime.MinValue;
		string bannerMessage = String.Empty;
		string welcomeMessage = String.Empty;
		string exitMessage = String.Empty;
		string statusDescription;
		string method;
		bool keepAlive;
		bool disposed;
		internal long contentLength = -1;
		
		internal FtpWebResponse (Uri uri, string method, bool keepAlive)
		{
			this.uri = uri;
			this.method = method;
			this.keepAlive = keepAlive;
		}
		
		public override long ContentLength {
			get {
				return contentLength;
			}
		}

		public override WebHeaderCollection Headers {
			get {
				return new WebHeaderCollection (true);
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
			internal set {
				statusCode = value;
			}
		}

		public string StatusDescription {
			get {
				return statusDescription;
			}
			internal set {
				statusDescription = value;
			}
		}
		
		public override void Close ()
		{
			if (disposed)
				return;
			
			disposed = true;
			stream.Close ();
			stream = null;
		}

		public override Stream GetResponseStream ()
		{
			if (method != WebRequestMethods.Ftp.DownloadFile &&
					method != WebRequestMethods.Ftp.ListDirectory)
				CheckDisposed ();
			
			return stream;
		}

		internal Stream Stream {
			set {
				stream = value;
			}
		}

		internal void UpdateStatus (FtpStatusCode code, string desc)
		{
			statusCode = code;
			statusDescription = desc;
		}

		~FtpWebResponse ()
		{
			((IDisposable) this).Dispose ();
		}

		void CheckDisposed ()
		{
			if (disposed)
				throw new ObjectDisposedException (GetType ().FullName);
		}
	}
}

#endif

