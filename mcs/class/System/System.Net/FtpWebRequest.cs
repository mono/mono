//
// System.Net.FtpWebRequest.cs
//
// Authors:
//	Carlos Alberto Cortez (calberto.cortez@gmail.com)
//
// (c) Copyright 2006 Novell, Inc. (http://www.novell.com)
//
#if NET_2_0

using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Net.Cache;
using System.Security.Cryptography.X509Certificates;
using System.Net;


namespace System.Net
{
	public sealed class FtpWebRequest : WebRequest
	{
		Uri requestUri;
		string file_name; // By now, used for upload
		ServicePoint servicePoint;
		Socket dataSocket;
		NetworkStream controlStream;
		StreamReader controlReader;
		NetworkCredential credentials;
		IPHostEntry hostEntry;
		IPEndPoint localEndPoint;
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
		const string PortCommand = "PORT";
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

		internal FtpWebRequest (Uri uri) 
		{
			this.requestUri = uri;
			this.proxy = GlobalProxySelection.Select;
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
				if (value == null)
					throw new ArgumentNullException ();

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

				Uri initial = new Uri ("ftp://dummy-host" + initial_path);
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
			SendCommand (QuitCommand);
			controlStream.Close ();
		}

		private void CloseDataConnection () {
			if(dataSocket != null)
				dataSocket.Close ();
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
			requestStream = new FtpDataStream (this, dataSocket, false);
			asyncResult.Stream = requestStream;
		}

		void DownloadData ()
		{
			State = RequestState.OpeningData;

			OpenDataConnection ();

			State = RequestState.TransferInProgress;
			ftpResponse.Stream = new FtpDataStream (this, dataSocket, true);
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

				IPEndPoint remote = new IPEndPoint (address, requestUri.Port);

				if (!ServicePoint.CallEndPointDelegate (sock, remote)) {
					sock.Close ();
					sock = null;
				} else {
					try {
						sock.Connect (remote);
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
			// ignore status for OPTS
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
		Socket SetupPassiveConnection (string statusDescription)
		{
			// Current response string
			string response = statusDescription;
			if (response.Length < 4)
				throw new WebException ("Cannot open passive data connection");
			
			// Look for first digit after code
			int i;
			for (i = 3; i < response.Length && !Char.IsDigit (response [i]); i++)
				;
			if (i >= response.Length)
				throw new WebException ("Cannot open passive data connection");

			// Get six elements
			string [] digits = response.Substring (i).Split (new char [] {','}, 6);
			if (digits.Length != 6)
				throw new WebException ("Cannot open passive data connection");

			// Clean non-digits at the end of last element
			int j;
			for (j = digits [5].Length - 1; j >= 0 && !Char.IsDigit (digits [5][j]); j--)
				;
			if (j < 0)
				throw new WebException ("Cannot open passive data connection");
			
			digits [5] = digits [5].Substring (0, j + 1);

			IPAddress ip;
			try {
				ip = IPAddress.Parse (String.Join (".", digits, 0, 4));
			} catch (FormatException) {
				throw new WebException ("Cannot open passive data connection");
			}

			// Get the port
			int p1, p2, port;
			if (!Int32.TryParse (digits [4], out p1) || !Int32.TryParse (digits [5], out p2))
				throw new WebException ("Cannot open passive data connection");

			port = (p1 << 8) + p2; // p1 * 256 + p2
			//port = p1 * 256 + p2;
			if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
				throw new WebException ("Cannot open passive data connection");

			IPEndPoint ep = new IPEndPoint (ip, port);
			Socket sock = new Socket (ep.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			try {
				sock.Connect (ep);
			} catch (SocketException) {
				sock.Close ();
				throw new WebException ("Cannot open passive data connection");
			}

			return sock;
		}

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
			
			if (usePassive) {
				status = SendCommand (PassiveCommand);
				if (status.StatusCode != FtpStatusCode.EnteringPassive) {
					throw CreateExceptionFromResponse (status);
				}
				
				return SetupPassiveConnection (status.StatusDescription);
			}

			// Open a socket to listen the server's connection
			Socket sock = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			try {
				sock.Bind (new IPEndPoint (localEndPoint.Address, 0));
				sock.Listen (1); // We only expect a connection from server

			} catch (SocketException e) {
				sock.Close ();

				throw new WebException ("Couldn't open listening socket on client", e);
			}

			IPEndPoint ep = (IPEndPoint) sock.LocalEndPoint;
			string ipString = ep.Address.ToString ().Replace ('.', ',');
			int h1 = ep.Port >> 8; // ep.Port / 256
			int h2 = ep.Port % 256;

			string portParam = ipString + "," + h1 + "," + h2;
			status = SendCommand (PortCommand, portParam);
			
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
				dataSocket = s;
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
				dataSocket = incoming;
			}

			if (EnableSsl) {
				InitiateSecureConnection (ref controlStream);
				controlReader = new StreamReader (controlStream, Encoding.ASCII);
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
			cmd = Encoding.ASCII.GetBytes (commandString);
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

		private void InitiateSecureConnection (ref NetworkStream stream) {
			FtpStatus status = SendCommand (AuthCommand, "TLS");

			if (status.StatusCode != FtpStatusCode.ServerWantsSecureSession) {
				throw CreateExceptionFromResponse (status);
			}

			ChangeToSSLSocket (ref stream);
		}

		internal static bool ChangeToSSLSocket (ref NetworkStream stream) {
#if TARGET_JVM
			stream.ChangeToSSLSocket ();
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

#endif

