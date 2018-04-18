//
// System.Net.FtpWebRequest.cs
//
// Authors:
//	Carlos Alberto Cortez (calberto.cortez@gmail.com)
//
// (c) Copyright 2006 Novell, Inc. (http://www.novell.com)
//

#if SECURITY_DEP
#if MONO_SECURITY_ALIAS
extern alias MonoSecurity;
using MSI = MonoSecurity::Mono.Security.Interface;
#else
using MSI = Mono.Security.Interface;
#endif
#endif

using System;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Net.Cache;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using Mono.Net.Security;

namespace System.Net
{
	public sealed class FtpWebRequest : WebRequest
	{
		Uri requestUri;
		string file_name; // By now, used for upload
		ServicePoint servicePoint;
		Stream origDataStream;
		Stream dataStream;
		Stream controlStream;
		StreamReader controlReader;
		NetworkCredential credentials;
		IPHostEntry hostEntry;
		IPEndPoint localEndPoint;
		IPEndPoint remoteEndPoint;
		IWebProxy proxy;
		int timeout = 100000;
		int rwTimeout = 300000;
		long offset = 0;
		bool binary = true;
		bool enableSsl = false;
		bool usePassive = true;
		bool keepAlive = false;
		string method = WebRequestMethods.Ftp.DownloadFile;
		string renameTo;
		object locker = new object ();
		
		RequestState requestState = RequestState.Before;
		FtpAsyncResult asyncResult;
		FtpWebResponse ftpResponse;
		Stream requestStream;
		string initial_path;

		const string ChangeDir = "CWD";
		const string UserCommand = "USER";
		const string PasswordCommand = "PASS";
		const string TypeCommand = "TYPE";
		const string PassiveCommand = "PASV";
		const string ExtendedPassiveCommand = "EPSV";
		const string PortCommand = "PORT";
		const string ExtendedPortCommand = "EPRT";
		const string AbortCommand = "ABOR";
		const string AuthCommand = "AUTH";
		const string RestCommand = "REST";
		const string RenameFromCommand = "RNFR";
		const string RenameToCommand = "RNTO";
		const string QuitCommand = "QUIT";
		const string EOL = "\r\n"; // Special end of line

		enum RequestState
		{
			Before,
			Scheduled,
			Connecting,
			Authenticating,
			OpeningData,
			TransferInProgress,
			Finished,
			Aborted,
			Error
		}

		// sorted commands
		static readonly string [] supportedCommands = new string [] {
			WebRequestMethods.Ftp.AppendFile, // APPE
			WebRequestMethods.Ftp.DeleteFile, // DELE
			WebRequestMethods.Ftp.ListDirectoryDetails, // LIST
			WebRequestMethods.Ftp.GetDateTimestamp, // MDTM
			WebRequestMethods.Ftp.MakeDirectory, // MKD
			WebRequestMethods.Ftp.ListDirectory, // NLST
			WebRequestMethods.Ftp.PrintWorkingDirectory, // PWD
			WebRequestMethods.Ftp.Rename, // RENAME
			WebRequestMethods.Ftp.DownloadFile, // RETR
			WebRequestMethods.Ftp.RemoveDirectory, // RMD
			WebRequestMethods.Ftp.GetFileSize, // SIZE
			WebRequestMethods.Ftp.UploadFile, // STOR
			WebRequestMethods.Ftp.UploadFileWithUniqueName // STUR
			};

		Encoding dataEncoding = Encoding.UTF8;

		internal FtpWebRequest (Uri uri) 
		{
			this.requestUri = uri;
#pragma warning disable 618
			this.proxy = GlobalProxySelection.Select;
#pragma warning restore 618
		}

		static Exception GetMustImplement ()
		{
			return new NotImplementedException ();
		}
		
		[MonoTODO]
		public X509CertificateCollection ClientCertificates
		{
			get {
				throw GetMustImplement ();
			}
			set {
				throw GetMustImplement ();
			}
		}
		
		[MonoTODO]
		public override string ConnectionGroupName
		{
			get {
				throw GetMustImplement ();
			}
			set {
				throw GetMustImplement ();
			}
		}

		public override string ContentType {
			get {
				throw new NotSupportedException ();
			}
			set {
				throw new NotSupportedException ();
			}
		}

		public override long ContentLength {
			get {
				return 0;
			} 
			set {
				// DO nothing
			}
		}

		public long ContentOffset {
			get {
				return offset;
			}
			set {
				CheckRequestStarted ();
				if (value < 0)
					throw new ArgumentOutOfRangeException ();

				offset = value;
			}
		}

		public override ICredentials Credentials {
			get {
				return credentials;
			}
			set {
				CheckRequestStarted ();
				if (value == null)
					throw new ArgumentNullException ();
				if (!(value is NetworkCredential))
					throw new ArgumentException ();

				credentials = value as NetworkCredential;
			}
		}

#if !MOBILE
		[MonoTODO]
		public static new RequestCachePolicy DefaultCachePolicy
		{
			get {
				throw GetMustImplement ();
			}
			set {
				throw GetMustImplement ();
			}
		}
#endif

		public bool EnableSsl {
			get {
				return enableSsl;
			}
			set {
				CheckRequestStarted ();
				enableSsl = value;
			}
		}

		[MonoTODO]
		public override WebHeaderCollection Headers
		{
			get {
				throw GetMustImplement ();
			}
			set {
				throw GetMustImplement ();
			}
		}

		[MonoTODO ("We don't support KeepAlive = true")]
		public bool KeepAlive {
			get {
				return keepAlive;
			}
			set {
				CheckRequestStarted ();
				//keepAlive = value;
			}
		}

		public override string Method {
			get {
				return method;
			}
			set {
				CheckRequestStarted ();
				if (value == null)
					throw new ArgumentNullException ("Method string cannot be null");

				if (value.Length == 0 || Array.BinarySearch (supportedCommands, value) < 0)
					throw new ArgumentException ("Method not supported", "value");
				
				method = value;
			}
		}

		public override bool PreAuthenticate {
			get {
				throw new NotSupportedException ();
			}
			set {
				throw new NotSupportedException ();
			}
		}

		public override IWebProxy Proxy {
			get {
				return proxy;
			}
			set {
				CheckRequestStarted ();
				proxy = value;
			}
		}

		public int ReadWriteTimeout {
			get {
				return rwTimeout;
			}
			set {
				CheckRequestStarted ();

				if (value < - 1)
					throw new ArgumentOutOfRangeException ();
				else
					rwTimeout = value;
			}
		}

		public string RenameTo {
			get {
				return renameTo;
			}
			set {
				CheckRequestStarted ();
				if (value == null || value.Length == 0)
					throw new ArgumentException ("RenameTo value can't be null or empty", "RenameTo");

				renameTo = value;
			}
		}

		public override Uri RequestUri {
			get {
				return requestUri;
			}
		}

		public ServicePoint ServicePoint {
			get {
				return GetServicePoint ();
			}
		}

		public bool UsePassive {
			get {
				return usePassive;
			}
			set {
				CheckRequestStarted ();
				usePassive = value;
			}
		}

		[MonoTODO]
		public override bool UseDefaultCredentials
		{
			get {
				throw GetMustImplement ();
			}
			set {
				throw GetMustImplement ();
			}
		}
		
		public bool UseBinary {
			get {
				return binary;
			} set {
				CheckRequestStarted ();
				binary = value;
			}
		}

		public override int Timeout {
			get {
				return timeout;
			}
			set {
				CheckRequestStarted ();

				if (value < -1)
					throw new ArgumentOutOfRangeException ();
				else
					timeout = value;
			}
		}

		string DataType {
			get {
				return binary ? "I" : "A";
			}
		}

		RequestState State {
			get {
				lock (locker) {
					return requestState;
				}
			}

			set {
				lock (locker) {
					CheckIfAborted ();
					CheckFinalState ();
					requestState = value;
				}
			}
		}

		public override void Abort () {
			lock (locker) {
				if (State == RequestState.TransferInProgress) {
					/*FtpStatus status = */
					SendCommand (false, AbortCommand);
				}

				if (!InFinalState ()) {
					State = RequestState.Aborted;
					ftpResponse = new FtpWebResponse (this, requestUri, method, FtpStatusCode.FileActionAborted, "Aborted by request");
				}
			}
		}

		public override IAsyncResult BeginGetResponse (AsyncCallback callback, object state) {
			if (asyncResult != null && !asyncResult.IsCompleted) {
				throw new InvalidOperationException ("Cannot re-call BeginGetRequestStream/BeginGetResponse while a previous call is still in progress");
			}

			CheckIfAborted ();
			
			asyncResult = new FtpAsyncResult (callback, state);

			lock (locker) {
				if (InFinalState ())
					asyncResult.SetCompleted (true, ftpResponse);
				else {
					if (State == RequestState.Before)
						State = RequestState.Scheduled;

					Thread thread = new Thread (ProcessRequest);
					thread.IsBackground = true;
					thread.Start ();
				}
			}

			return asyncResult;
		}

		public override WebResponse EndGetResponse (IAsyncResult asyncResult) {
			if (asyncResult == null)
				throw new ArgumentNullException ("AsyncResult cannot be null!");

			if (!(asyncResult is FtpAsyncResult) || asyncResult != this.asyncResult)
				throw new ArgumentException ("AsyncResult is from another request!");

			FtpAsyncResult asyncFtpResult = (FtpAsyncResult) asyncResult;
			if (!asyncFtpResult.WaitUntilComplete (timeout, false)) {
				Abort ();
				throw new WebException ("Transfer timed out.", WebExceptionStatus.Timeout);
			}

			CheckIfAborted ();

			asyncResult = null;

			if (asyncFtpResult.GotException)
				throw asyncFtpResult.Exception;

			return asyncFtpResult.Response;
		}

		public override WebResponse GetResponse () {
			IAsyncResult asyncResult = BeginGetResponse (null, null);
			return EndGetResponse (asyncResult);
		}

		public override IAsyncResult BeginGetRequestStream (AsyncCallback callback, object state) {
			if (method != WebRequestMethods.Ftp.UploadFile && method != WebRequestMethods.Ftp.UploadFileWithUniqueName &&
					method != WebRequestMethods.Ftp.AppendFile)
				throw new ProtocolViolationException ();

			lock (locker) {
				CheckIfAborted ();

				if (State != RequestState.Before)
					throw new InvalidOperationException ("Cannot re-call BeginGetRequestStream/BeginGetResponse while a previous call is still in progress");

				State = RequestState.Scheduled;
			}

			asyncResult = new FtpAsyncResult (callback, state);
			Thread thread = new Thread (ProcessRequest);
			thread.IsBackground = true;
			thread.Start ();

			return asyncResult;
		}

		public override Stream EndGetRequestStream (IAsyncResult asyncResult) {
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");

			if (!(asyncResult is FtpAsyncResult))
				throw new ArgumentException ("asyncResult");

			if (State == RequestState.Aborted) {
				throw new WebException ("Request aborted", WebExceptionStatus.RequestCanceled);
			}
			
			if (asyncResult != this.asyncResult)
				throw new ArgumentException ("AsyncResult is from another request!");

			FtpAsyncResult res = (FtpAsyncResult) asyncResult;

			if (!res.WaitUntilComplete (timeout, false)) {
				Abort ();
				throw new WebException ("Request timed out");
			}

			if (res.GotException)
				throw res.Exception;

			return res.Stream;
		}

		public override Stream GetRequestStream () {
			IAsyncResult asyncResult = BeginGetRequestStream (null, null);
			return EndGetRequestStream (asyncResult);
		}
		
		ServicePoint GetServicePoint ()
		{
			if (servicePoint == null)
				servicePoint = ServicePointManager.FindServicePoint (requestUri, proxy);

			return servicePoint;
		}

		// Probably move some code of command connection here
		void ResolveHost ()
		{
			CheckIfAborted ();
			hostEntry = GetServicePoint ().HostEntry;

			if (hostEntry == null) {
				ftpResponse.UpdateStatus (new FtpStatus(FtpStatusCode.ActionAbortedLocalProcessingError, "Cannot resolve server name"));
				throw new WebException ("The remote server name could not be resolved: " + requestUri,
					null, WebExceptionStatus.NameResolutionFailure, ftpResponse);
			}
		}

		void ProcessRequest () {

			if (State == RequestState.Scheduled) {
				ftpResponse = new FtpWebResponse (this, requestUri, method, keepAlive);

				try {
					ProcessMethod ();
					//State = RequestState.Finished;
					//finalResponse = ftpResponse;
					asyncResult.SetCompleted (false, ftpResponse);
				}
				catch (Exception e) {
					if (!GetServicePoint ().UsesProxy)
						State = RequestState.Error;
					SetCompleteWithError (e);
				}
			}
			else {
				if (InProgress ()) {
					FtpStatus status = GetResponseStatus ();

					ftpResponse.UpdateStatus (status);

					if (ftpResponse.IsFinal ()) {
						State = RequestState.Finished;
					}
				}

				asyncResult.SetCompleted (false, ftpResponse);
			}
		}

		void SetType ()
		{
			if (binary) {
				FtpStatus status = SendCommand (TypeCommand, DataType);
				if ((int) status.StatusCode < 200 || (int) status.StatusCode >= 300)
					throw CreateExceptionFromResponse (status);
			}
		}

		string GetRemoteFolderPath (Uri uri)
		{
			string result;
			string local_path = Uri.UnescapeDataString (uri.LocalPath);
			if (initial_path == null || initial_path == "/") {
				result = local_path;
			} else {
				if (local_path [0] == '/')
					local_path = local_path.Substring (1);

				UriBuilder initialBuilder = new UriBuilder () {
					Scheme  = "ftp",
					Host    = "dummy-host",
					Path    = initial_path,
				};
				Uri initial = initialBuilder.Uri;
				result = new Uri (initial, local_path).LocalPath;
			}

			int last = result.LastIndexOf ('/');
			if (last == -1)
				return null;

			return result.Substring (0, last + 1);
		}

		void CWDAndSetFileName (Uri uri)
		{
			string remote_folder = GetRemoteFolderPath (uri);
			FtpStatus status;
			if (remote_folder != null) {
				status = SendCommand (ChangeDir, remote_folder);
				if ((int) status.StatusCode < 200 || (int) status.StatusCode >= 300)
					throw CreateExceptionFromResponse (status);

				int last = uri.LocalPath.LastIndexOf ('/');
				if (last >= 0) {
					file_name = Uri.UnescapeDataString (uri.LocalPath.Substring (last + 1));
				}
			}
		}

		void ProcessMethod ()
		{
			ServicePoint sp = GetServicePoint ();
			if (sp.UsesProxy) {
				if (method != WebRequestMethods.Ftp.DownloadFile)
					throw new NotSupportedException ("FTP+proxy only supports RETR");

				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (proxy.GetProxy (requestUri));
				req.Address = requestUri;
				requestState = RequestState.Finished;
				WebResponse response = req.GetResponse ();
				ftpResponse.Stream = new FtpDataStream (this, response.GetResponseStream (), true);
				ftpResponse.StatusCode = FtpStatusCode.CommandOK;
				return;
			}
			State = RequestState.Connecting;

			ResolveHost ();

			OpenControlConnection ();
			CWDAndSetFileName (requestUri);
			SetType ();

			switch (method) {
			// Open data connection and receive data
			case WebRequestMethods.Ftp.DownloadFile:
			case WebRequestMethods.Ftp.ListDirectory:
			case WebRequestMethods.Ftp.ListDirectoryDetails:
				DownloadData ();
				break;
			// Open data connection and send data
			case WebRequestMethods.Ftp.AppendFile:
			case WebRequestMethods.Ftp.UploadFile:
			case WebRequestMethods.Ftp.UploadFileWithUniqueName:
				UploadData ();
				break;
			// Get info from control connection
			case WebRequestMethods.Ftp.GetFileSize:
			case WebRequestMethods.Ftp.GetDateTimestamp:
			case WebRequestMethods.Ftp.PrintWorkingDirectory:
			case WebRequestMethods.Ftp.MakeDirectory:
			case WebRequestMethods.Ftp.Rename:
			case WebRequestMethods.Ftp.DeleteFile:
				ProcessSimpleMethod ();
				break;
			default: // What to do here?
				throw new Exception (String.Format ("Support for command {0} not implemented yet", method));
			}

			CheckIfAborted ();
		}

		private void CloseControlConnection () {
			if (controlStream != null) {
				SendCommand (QuitCommand);
				controlStream.Close ();
				controlStream = null;
			}
		}

		internal void CloseDataConnection () {
			if(origDataStream != null) {
				origDataStream.Close ();
				origDataStream = null;
			}
		}

		private void CloseConnection () {
			CloseControlConnection ();
			CloseDataConnection ();
		}
		
		void ProcessSimpleMethod ()
		{
			State = RequestState.TransferInProgress;
			
			FtpStatus status;
			
			if (method == WebRequestMethods.Ftp.PrintWorkingDirectory)
				method = "PWD";

			if (method == WebRequestMethods.Ftp.Rename)
				method = RenameFromCommand;
			
			status = SendCommand (method, file_name);

			ftpResponse.Stream = Stream.Null;
			
			string desc = status.StatusDescription;

			switch (method) {
			case WebRequestMethods.Ftp.GetFileSize: {
					if (status.StatusCode != FtpStatusCode.FileStatus)
						throw CreateExceptionFromResponse (status);

					int i, len;
					long size;
					for (i = 4, len = 0; i < desc.Length && Char.IsDigit (desc [i]); i++, len++)
						;

					if (len == 0)
						throw new WebException ("Bad format for server response in " + method);

					if (!Int64.TryParse (desc.Substring (4, len), out size))
						throw new WebException ("Bad format for server response in " + method);

					ftpResponse.contentLength = size;
				}
				break;
			case WebRequestMethods.Ftp.GetDateTimestamp:
				if (status.StatusCode != FtpStatusCode.FileStatus)
					throw CreateExceptionFromResponse (status);
				ftpResponse.LastModified = DateTime.ParseExact (desc.Substring (4), "yyyyMMddHHmmss", null);
				break;
			case WebRequestMethods.Ftp.MakeDirectory:
				if (status.StatusCode != FtpStatusCode.PathnameCreated)
					throw CreateExceptionFromResponse (status);
				break;
			case ChangeDir:
				method = WebRequestMethods.Ftp.PrintWorkingDirectory;

				if (status.StatusCode != FtpStatusCode.FileActionOK)
					throw CreateExceptionFromResponse (status);

				status = SendCommand (method);

				if (status.StatusCode != FtpStatusCode.PathnameCreated)
					throw CreateExceptionFromResponse (status);
				break;
			case RenameFromCommand:
				method = WebRequestMethods.Ftp.Rename;
				if (status.StatusCode != FtpStatusCode.FileCommandPending) 
					throw CreateExceptionFromResponse (status);
				// Pass an empty string if RenameTo wasn't specified
				status = SendCommand (RenameToCommand, renameTo != null ? renameTo : String.Empty);
				if (status.StatusCode != FtpStatusCode.FileActionOK)
					throw CreateExceptionFromResponse (status);
				break;
			case WebRequestMethods.Ftp.DeleteFile:
				if (status.StatusCode != FtpStatusCode.FileActionOK)  {
					throw CreateExceptionFromResponse (status);
				}
				break;
			}

			State = RequestState.Finished;
		}

		void UploadData ()
		{
			State = RequestState.OpeningData;

			OpenDataConnection ();

			State = RequestState.TransferInProgress;
			requestStream = new FtpDataStream (this, dataStream, false);
			asyncResult.Stream = requestStream;
		}

		void DownloadData ()
		{
			State = RequestState.OpeningData;

			OpenDataConnection ();

			State = RequestState.TransferInProgress;
			ftpResponse.Stream = new FtpDataStream (this, dataStream, true);
		}

		void CheckRequestStarted ()
		{
			if (State != RequestState.Before)
				throw new InvalidOperationException ("There is a request currently in progress");
		}

		void OpenControlConnection ()
		{
			Exception exception = null;
			Socket sock = null;
			foreach (IPAddress address in hostEntry.AddressList) {
				sock = new Socket (address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

				remoteEndPoint = new IPEndPoint (address, requestUri.Port);

				if (!ServicePoint.CallEndPointDelegate (sock, remoteEndPoint)) {
					sock.Close ();
					sock = null;
				} else {
					try {
						sock.Connect (remoteEndPoint);
						localEndPoint = (IPEndPoint) sock.LocalEndPoint;
						break;
					} catch (SocketException exc) {
						exception = exc;
						sock.Close ();
						sock = null;
					}
				}
			}

			// Couldn't connect to any address
			if (sock == null)
				throw new WebException ("Unable to connect to remote server", exception,
						WebExceptionStatus.UnknownError, ftpResponse);

			controlStream = new NetworkStream (sock);
			controlReader = new StreamReader (controlStream, Encoding.ASCII);

			State = RequestState.Authenticating;

			Authenticate ();
			FtpStatus status = SendCommand ("OPTS", "utf8", "on");
			if ((int)status.StatusCode < 200 || (int)status.StatusCode > 300)
				dataEncoding = Encoding.Default;
			else
				dataEncoding = Encoding.UTF8;

			status = SendCommand (WebRequestMethods.Ftp.PrintWorkingDirectory);
			initial_path = GetInitialPath (status);
		}

		static string GetInitialPath (FtpStatus status)
		{
			int s = (int) status.StatusCode;
			if (s < 200 || s > 300 || status.StatusDescription.Length <= 4)
				throw new WebException ("Error getting current directory: " + status.StatusDescription, null,
						WebExceptionStatus.UnknownError, null);

			string msg = status.StatusDescription.Substring (4);
			if (msg [0] == '"') {
				int next_quote = msg.IndexOf ('\"', 1);
				if (next_quote == -1)
					throw new WebException ("Error getting current directory: PWD -> " + status.StatusDescription, null,
								WebExceptionStatus.UnknownError, null);

				msg = msg.Substring (1, next_quote - 1);
			}

			if (!msg.EndsWith ("/"))
				msg += "/";
			return msg;
		}

		// Probably we could do better having here a regex
		Socket SetupPassiveConnection (string statusDescription, bool ipv6)
		{
			// Current response string
			string response = statusDescription;
			if (response.Length < 4)
				throw new WebException ("Cannot open passive data connection");

			int port = ipv6 ? GetPortV6 (response) : GetPortV4 (response);

			if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
				throw new WebException ("Cannot open passive data connection");

			IPEndPoint ep = new IPEndPoint (remoteEndPoint.Address, port);
			Socket sock = new Socket (ep.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			try {
				sock.Connect (ep);
			} catch (SocketException) {
				sock.Close ();
				throw new WebException ("Cannot open passive data connection");
			}

			return sock;
		}

		// GetPortV4, GetPortV6, FormatAddress and FormatAddressV6 are copied from referencesource
		// TODO: replace FtpWebRequest completely.
		private int GetPortV4(string responseString)
		{
			string [] parsedList = responseString.Split(new char [] {' ', '(', ',', ')'});

			// We need at least the status code and the port
			if (parsedList.Length <= 7) {
				throw new FormatException(SR.GetString(SR.net_ftp_response_invalid_format, responseString));
			}

			int index = parsedList.Length-1;
			// skip the last non-number token (e.g. terminating '.')
#if MONO
			// the MS code expects \r\n here in parsedList[index],
			// but we're stripping the EOL off earlier so the array contains
			// an empty string here which would make Char.IsNumber throw
			// TODO: this can be removed once we switch FtpWebRequest to referencesource
			if (parsedList[index] == "" || !Char.IsNumber(parsedList[index], 0))
#else
			if (!Char.IsNumber(parsedList[index], 0))
#endif
				index--;

			int port = Convert.ToByte(parsedList[index--], NumberFormatInfo.InvariantInfo);
			port = port |
				(Convert.ToByte(parsedList[index--], NumberFormatInfo.InvariantInfo) << 8);

			return port;
		}

		private int GetPortV6(string responseString)
		{
			int pos1 = responseString.LastIndexOf("(");
			int pos2 = responseString.LastIndexOf(")");
			if (pos1 == -1 || pos2 <= pos1) 
				throw new FormatException(SR.GetString(SR.net_ftp_response_invalid_format, responseString));

			// addressInfo will contain a string of format "|||<tcp-port>|"
			string addressInfo = responseString.Substring(pos1+1, pos2-pos1-1);

			// Although RFC2428 recommends using "|" as the delimiter,
			// It allows ASCII characters in range 33-126 inclusive.
			// We should consider allowing the full range.

			string [] parsedList = addressInfo.Split(new char [] {'|'});
			if (parsedList.Length < 4)
				throw new FormatException(SR.GetString(SR.net_ftp_response_invalid_format, responseString));
			
			return Convert.ToInt32(parsedList[3], NumberFormatInfo.InvariantInfo);
		}

		private String FormatAddress(IPAddress address, int Port )
		{
			byte [] localAddressInBytes = address.GetAddressBytes();

			// produces a string in FTP IPAddress/Port encoding (a1, a2, a3, a4, p1, p2), for sending as a parameter
			// to the port command.
			StringBuilder sb = new StringBuilder(32);
			foreach (byte element in localAddressInBytes) {
				sb.Append(element);
				sb.Append(',');
			}
			sb.Append(Port / 256 );
			sb.Append(',');
			sb.Append(Port % 256 );
			return sb.ToString();
		}

		private string FormatAddressV6(IPAddress address, int port) {
			StringBuilder sb = new StringBuilder(43); // based on max size of IPv6 address + port + seperators
			String addressString = address.ToString();
			sb.Append("|2|");
			sb.Append(addressString);
			sb.Append('|');
			sb.Append(port.ToString(NumberFormatInfo.InvariantInfo));
			sb.Append('|');
			return sb.ToString();
		}
		//

		Exception CreateExceptionFromResponse (FtpStatus status)
		{
			FtpWebResponse ftpResponse = new FtpWebResponse (this, requestUri, method, status);
			
			WebException exc = new WebException ("Server returned an error: " + status.StatusDescription, 
				null, WebExceptionStatus.ProtocolError, ftpResponse);
			return exc;
		}
		
		// Here we could also get a server error, so be cautious
		internal void SetTransferCompleted ()
		{
			if (InFinalState ())
				return;

			State = RequestState.Finished;
			FtpStatus status = GetResponseStatus ();
			ftpResponse.UpdateStatus (status);
			if(!keepAlive)
				CloseConnection ();
		}

		internal void OperationCompleted ()
		{
			if(!keepAlive)
				CloseConnection ();
		}

		void SetCompleteWithError (Exception exc)
		{
			if (asyncResult != null) {
				asyncResult.SetCompleted (false, exc);
			}
		}

		Socket InitDataConnection ()
		{
			FtpStatus status;
			bool ipv6 = remoteEndPoint.AddressFamily == AddressFamily.InterNetworkV6;

			if (usePassive) {
				status = SendCommand (ipv6 ? ExtendedPassiveCommand : PassiveCommand);
				if (status.StatusCode != (ipv6 ? (FtpStatusCode)229 : FtpStatusCode.EnteringPassive)) { // FtpStatusCode doesn't contain code 229 for EPSV so we need to cast...
					throw CreateExceptionFromResponse (status);
				}
				
				return SetupPassiveConnection (status.StatusDescription, ipv6);
			}

			// Open a socket to listen the server's connection
			Socket sock = new Socket (remoteEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			try {
				sock.Bind (new IPEndPoint (localEndPoint.Address, 0));
				sock.Listen (1); // We only expect a connection from server

			} catch (SocketException e) {
				sock.Close ();

				throw new WebException ("Couldn't open listening socket on client", e);
			}

			IPEndPoint ep = (IPEndPoint) sock.LocalEndPoint;

			var portParam = ipv6 ? FormatAddressV6 (ep.Address, ep.Port) : FormatAddress (ep.Address, ep.Port);

			status = SendCommand (ipv6 ? ExtendedPortCommand : PortCommand, portParam);
			
			if (status.StatusCode != FtpStatusCode.CommandOK) {
				sock.Close ();
				throw (CreateExceptionFromResponse (status));
			}

			return sock;
		}

		void OpenDataConnection ()
		{
			FtpStatus status;
			
			Socket s = InitDataConnection ();

			// Handle content offset
			if (offset > 0) {
				status = SendCommand (RestCommand, offset.ToString ());
				if (status.StatusCode != FtpStatusCode.FileCommandPending)
					throw CreateExceptionFromResponse (status);
			}

			if (method != WebRequestMethods.Ftp.ListDirectory && method != WebRequestMethods.Ftp.ListDirectoryDetails &&
			    method != WebRequestMethods.Ftp.UploadFileWithUniqueName) {
				status = SendCommand (method, file_name);
			} else {
				status = SendCommand (method);
			}

			if (status.StatusCode != FtpStatusCode.OpeningData && status.StatusCode != FtpStatusCode.DataAlreadyOpen)
				throw CreateExceptionFromResponse (status);

			if (usePassive) {
				origDataStream = new NetworkStream (s, true);
				dataStream = origDataStream;
				if (EnableSsl)
					ChangeToSSLSocket (ref dataStream);
			}
			else {

				// Active connection (use Socket.Blocking to true)
				Socket incoming = null;
				try {
					incoming = s.Accept ();
				}
				catch (SocketException) {
					s.Close ();
					if (incoming != null)
						incoming.Close ();

					throw new ProtocolViolationException ("Server commited a protocol violation.");
				}

				s.Close ();
				origDataStream = new NetworkStream (incoming, true);
				dataStream = origDataStream;
				if (EnableSsl)
					ChangeToSSLSocket (ref dataStream);
			}

			ftpResponse.UpdateStatus (status);
		}

		void Authenticate ()
		{
			string username = null;
			string password = null;
			string domain = null;

			if (credentials != null) {
				username = credentials.UserName;
				password = credentials.Password;
				domain = credentials.Domain;
			}

			if (username == null)
				username = "anonymous";
			if (password == null)
				password = "@anonymous";
			if (!string.IsNullOrEmpty (domain))
				username = domain + '\\' + username;

			// Connect to server and get banner message
			FtpStatus status = GetResponseStatus ();
			ftpResponse.BannerMessage = status.StatusDescription;

			if (EnableSsl) {
				InitiateSecureConnection (ref controlStream);
				controlReader = new StreamReader (controlStream, Encoding.ASCII);
				status = SendCommand ("PBSZ", "0");
				int st = (int) status.StatusCode;
				if (st < 200 || st >= 300)
					throw CreateExceptionFromResponse (status);
				// TODO: what if "PROT P" is denied by the server? What does MS do?
				status = SendCommand ("PROT", "P");
				st = (int) status.StatusCode;
				if (st < 200 || st >= 300)
					throw CreateExceptionFromResponse (status);

				status = new FtpStatus (FtpStatusCode.SendUserCommand, "");
			}
			
			if (status.StatusCode != FtpStatusCode.SendUserCommand)
				throw CreateExceptionFromResponse (status);

			status = SendCommand (UserCommand, username);

			switch (status.StatusCode) {
			case FtpStatusCode.SendPasswordCommand:
				status = SendCommand (PasswordCommand, password);
				if (status.StatusCode != FtpStatusCode.LoggedInProceed)
					throw CreateExceptionFromResponse (status);
				break;
			case FtpStatusCode.LoggedInProceed:
				break;
			default:
				throw CreateExceptionFromResponse (status);
			}

			ftpResponse.WelcomeMessage = status.StatusDescription;
			ftpResponse.UpdateStatus (status);
		}

		FtpStatus SendCommand (string command, params string [] parameters) {
			return SendCommand (true, command, parameters);
		}

		FtpStatus SendCommand (bool waitResponse, string command, params string [] parameters)
		{
			byte [] cmd;
			string commandString = command;
			if (parameters.Length > 0)
				commandString += " " + String.Join (" ", parameters);

			commandString += EOL;
			cmd = dataEncoding.GetBytes (commandString);
			try {
				controlStream.Write (cmd, 0, cmd.Length);
			} catch (IOException) {
				//controlStream.Close ();
				return new FtpStatus(FtpStatusCode.ServiceNotAvailable, "Write failed");
			}

			if(!waitResponse)
				return null;
			
			FtpStatus result = GetResponseStatus ();
			if (ftpResponse != null)
				ftpResponse.UpdateStatus (result);
			return result;
		}

		internal static FtpStatus ServiceNotAvailable ()
		{
			return new FtpStatus (FtpStatusCode.ServiceNotAvailable, Locale.GetText ("Invalid response from server"));
		}
		
		internal FtpStatus GetResponseStatus ()
		{
			while (true) {
				string response = null;

				try {
					response = controlReader.ReadLine ();
				} catch (IOException) {
				}

				if (response == null || response.Length < 3)
					return ServiceNotAvailable ();

				int code;
				if (!Int32.TryParse (response.Substring (0, 3), out code))
					return ServiceNotAvailable ();

				if (response.Length > 3 && response [3] == '-'){
					string line = null;
					string find = code.ToString() + ' ';
					while (true){
						line = null;
						try {
							line = controlReader.ReadLine();
						} catch (IOException) {
						}
						if (line == null)
							return ServiceNotAvailable ();
						
						response += Environment.NewLine + line;

						if (line.StartsWith(find, StringComparison.Ordinal))
							break;
					} 
				}
				return new FtpStatus ((FtpStatusCode) code, response);
			}
		}

		private void InitiateSecureConnection (ref Stream stream) {
			FtpStatus status = SendCommand (AuthCommand, "TLS");
			if (status.StatusCode != FtpStatusCode.ServerWantsSecureSession)
				throw CreateExceptionFromResponse (status);

			ChangeToSSLSocket (ref stream);
		}

		internal bool ChangeToSSLSocket (ref Stream stream) {
#if SECURITY_DEP
			var provider = MonoTlsProviderFactory.GetProviderInternal ();
			var settings = MSI.MonoTlsSettings.CopyDefaultSettings ();
			settings.UseServicePointManagerCallback = true;
			var sslStream = provider.CreateSslStream (stream, true, settings);
			sslStream.AuthenticateAsClient (requestUri.Host, null, SslProtocols.Default, false);
			stream = sslStream.AuthenticatedStream;
			return true;
#else
			throw new NotImplementedException ();
#endif
		}
		
		bool InFinalState () {
			return (State == RequestState.Aborted || State == RequestState.Error || State == RequestState.Finished);
		}

		bool InProgress () {
			return (State != RequestState.Before && !InFinalState ());
		}

		internal void CheckIfAborted () {
			if (State == RequestState.Aborted)
				throw new WebException ("Request aborted", WebExceptionStatus.RequestCanceled);
		}

		void CheckFinalState () {
			if (InFinalState ())
				throw new InvalidOperationException ("Cannot change final state");
		}
	}
}


