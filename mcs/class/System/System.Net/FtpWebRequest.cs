//
// System.Net.FtpWebRequest.cs
//
// Authors:
//	Carlos Alberto Cortez (calberto.cortez@gmail.com)
//
// (c) Copyright 2006 Novell, Inc. (http://www.novell.com)
//

using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
#if NET_2_0
using System.Net.Cache;
#endif

#if NET_2_0

namespace System.Net
{
	[Serializable]
	public class FtpWebRequest : WebRequest
	{
		Uri requestUri;
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
		long offset;
		bool binary = true;
		bool enableSsl;
		bool requestInProgress;
		bool usePassive = true;
		bool keepAlive = true;
		bool aborted;
		bool transferCompleted;
		bool gotRequestStream;
		string method = WebRequestMethods.Ftp.DownloadFile;
		string renameTo;
		object locker = new object ();

		FtpStatusCode statusCode;
		string statusDescription = String.Empty;

		FtpAsyncResult asyncRead;
		FtpAsyncResult asyncWrite;

		FtpWebResponse ftpResponse;
		Stream requestStream = Stream.Null;

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
		const string EOL = "\r\n"; // Special end of line

		// sorted commands
		static readonly string [] supportedCommands = new string [] {
			WebRequestMethods.Ftp.AppendFile, // APPE
			WebRequestMethods.Ftp.DeleteFile, // DELE
			WebRequestMethods.Ftp.ListDirectoryDetails, // LIST
			WebRequestMethods.Ftp.GetDateTimestamps, // MDTM
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

		public bool EnableSsl {
			get {
				return enableSsl;
			}
			set {
				CheckRequestStarted ();
				enableSsl = value;
			}
		}

		public bool KeepAlive {
			get {
				return keepAlive;
			}
			set {
				CheckRequestStarted ();
				keepAlive = value;
			}
		}

		public override string Method {
			get {
				return method;
			}
			set {
				CheckRequestStarted ();
				if (value == null)
					throw new ArgumentNullException ("method");

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

		ServicePoint GetServicePoint ()
		{
			if (servicePoint == null)
				servicePoint = ServicePointManager.FindServicePoint (requestUri, proxy);

			return servicePoint;
		}

		// Probably move some code of command connection here
		bool ResolveHost ()
		{
			hostEntry = GetServicePoint ().HostEntry;
			if (hostEntry == null)
				return false;
			
			return true;
		}

		public override void Abort ()
		{
			FtpStatusCode status = SendCommand (AbortCommand);
			if (status != FtpStatusCode.ClosingData)
				throw CreateExceptionFromResponse (0); // Probably ignore it by now

			aborted = true;
			if (asyncRead != null) {
				FtpAsyncResult r = asyncRead;
				WebException wexc = new WebException ("Request aborted", WebExceptionStatus.RequestCanceled);
				r.SetCompleted (false, wexc);
				r.DoCallback ();
				asyncRead = null;
			}
			if (asyncWrite != null) {
				FtpAsyncResult r = asyncWrite;
				WebException wexc = new WebException ("Request aborted", WebExceptionStatus.RequestCanceled);
				r.SetCompleted (false, wexc);
				r.DoCallback ();
				asyncWrite = null;
			}
		}

		void ProcessRequest ()
		{
			ftpResponse = new FtpWebResponse (requestUri, method, keepAlive);

			if (!ResolveHost ()) {
				SetResponseError (new WebException ("The remote server name could not be resolved: " + requestUri,
						null, WebExceptionStatus.NameResolutionFailure, ftpResponse));
				return;
			}
			
			if (!OpenControlConnection ())
				return;

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
				case WebRequestMethods.Ftp.GetDateTimestamps:
					GetInfoFromControl ();
					break;
				case WebRequestMethods.Ftp.Rename:
					RenameFile ();
					break;
				case WebRequestMethods.Ftp.MakeDirectory:
					ProcessSimpleRequest ();
					break;
				default: // What to do here?
					throw new Exception ("Support for command not implemented yet");
			}
		}

		// Currently I use this only for MKD 
		// (Commands that don't need any parsing in command connection
		// for open data connection)
		void ProcessSimpleRequest ()
		{
			if (SendCommand (method, requestUri.LocalPath) != FtpStatusCode.PathnameCreated) {
				asyncRead.SetCompleted (true, CreateExceptionFromResponse (0));
				return;
			}

			asyncRead.SetCompleted (true, ftpResponse);
		}

		// It would be good to have a SetCompleted method for
		// settting asyncRead as completed (some code is here and there, repeated)
		void GetInfoFromControl ()
		{
			FtpStatusCode status = SendCommand (method, requestUri.LocalPath);
			if (status != FtpStatusCode.FileStatus) {
				asyncRead.SetCompleted (true, CreateExceptionFromResponse (0));
				return;
			}

			string desc = statusDescription;
			Console.WriteLine ("Desc = " + desc);
			if (method == WebRequestMethods.Ftp.GetFileSize) {
				int i, len;
				long size;
				for (i = 4, len = 0; i < desc.Length && Char.IsDigit (desc [i]); i++, len++)
					;

				if (len == 0) {
					asyncRead.SetCompleted (true, new WebException ("Bad format for server response in " + method));
					return;
				}

				if (!Int64.TryParse (desc.Substring (4, len), out size)) {
					asyncRead.SetCompleted (true, new WebException ("Bad format for server response in " + method));
					return;
				}

				ftpResponse.contentLength = size;
				asyncRead.SetCompleted (true, ftpResponse);
				return;
			}
			
			if (method == WebRequestMethods.Ftp.GetDateTimestamps) {
				// Here parse the format the date time (different formats)
				asyncRead.SetCompleted (true, ftpResponse);
				return;
			}

			throw new Exception ("You shouldn't reach this point");
		}

		void RenameFile ()
		{
		}

		void UploadData ()
		{
			if (gotRequestStream) {
				if (GetResponseCode () != FtpStatusCode.ClosingData)
					asyncRead.SetCompleted (true, CreateExceptionFromResponse (0));
				
				return;
			}
			
			if (!OpenDataConnection ())
				return;

			gotRequestStream = true;
			requestStream = new FtpDataStream (this, dataSocket, false);
			asyncWrite.SetCompleted (true, requestStream);
		}

		void DownloadData ()
		{
			FtpStatusCode status;

			// Handle content offset
			if (offset > 0) {
				status = SendCommand (RestCommand, offset.ToString ());
				if (status != FtpStatusCode.FileCommandPending) {
					asyncRead.SetCompleted (true, CreateExceptionFromResponse (0));
					return;
				}
			}

			if (!OpenDataConnection ())
				return;

			ftpResponse.Stream = new FtpDataStream (this, dataSocket, true);
			ftpResponse.StatusDescription = statusDescription;
			ftpResponse.StatusCode = statusCode;
			asyncRead.SetCompleted (true, ftpResponse);
		}

		public override IAsyncResult BeginGetResponse (AsyncCallback callback, object state)
		{
			if (aborted)
				throw new WebException ("Request was previously aborted.");
			
			Monitor.Enter (this);
			if (asyncRead != null) {
				Monitor.Exit (this);
				throw new InvalidOperationException ();
			}

			requestInProgress = true;
			asyncRead = new FtpAsyncResult (callback, state);
			Thread thread = new Thread (ProcessRequest);
			thread.Start ();

			Monitor.Exit (this);
			return asyncRead;
		}

		public override WebResponse EndGetResponse (IAsyncResult asyncResult)
		{
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");

			if (!(asyncResult is FtpAsyncResult) || asyncResult != asyncRead)
				throw new ArgumentException ("asyncResult");

			FtpAsyncResult asyncFtpResult = (FtpAsyncResult) asyncResult;
			if (!asyncFtpResult.WaitUntilComplete (timeout, false)) {
				Abort ();
				throw new WebException ("Transfer timed out.", WebExceptionStatus.Timeout);
			}

			if (asyncFtpResult.GotException)
				throw asyncFtpResult.Exception;

			return asyncFtpResult.Response;
		}

		public override WebResponse GetResponse ()
		{
			IAsyncResult asyncResult = BeginGetResponse (null, null);
			return EndGetResponse (asyncResult);
		}

		public override IAsyncResult BeginGetRequestStream (AsyncCallback callback, object state)
		{
			if (aborted)
				throw new WebException ("Request was previously aborted.");
			
			if (method != WebRequestMethods.Ftp.UploadFile && method != WebRequestMethods.Ftp.UploadFileWithUniqueName &&
					method != WebRequestMethods.Ftp.AppendFile)
				throw new ProtocolViolationException ();

			lock (locker) {
				if (asyncWrite != null || asyncRead != null)
					throw new InvalidOperationException ();
				
				requestInProgress = true;
				asyncWrite = new FtpAsyncResult (callback, state);
				Thread thread = new Thread (ProcessRequest);
				thread.Start ();

				return asyncWrite;
			}
		}

		public override Stream EndGetRequestStream (IAsyncResult asyncResult)
		{
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");

			if (!(asyncResult is FtpAsyncResult))
				throw new ArgumentException ("asyncResult");

			FtpAsyncResult res = (FtpAsyncResult) asyncResult;
			if (!res.WaitUntilComplete (timeout, false)) {
				Abort ();
				throw new WebException ("Request timeod out");
			}

			if (res.GotException)
				throw res.Exception;

			return res.Stream;
		}

		public override Stream GetRequestStream ()
		{
			IAsyncResult asyncResult = BeginGetRequestStream (null, null);
			return EndGetRequestStream (asyncResult);
		}

		void CheckRequestStarted ()
		{
			if (requestInProgress)
				throw new InvalidOperationException ("request in progress");
		}

		bool OpenControlConnection ()
		{
			Socket sock = null;
			foreach (IPAddress address in hostEntry.AddressList) {
				sock = new Socket (address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
				try {
					sock.Connect (new IPEndPoint (address, requestUri.Port));
					localEndPoint = (IPEndPoint) sock.LocalEndPoint;
					break;
				} catch (SocketException e) {
					sock.Close ();
					sock = null;
				}
			}

			// Couldn't connect to any address
			if (sock == null) {
				SetResponseError (new WebException ("Unable to connect to remote server", null, 
						WebExceptionStatus.UnknownError, ftpResponse));
				return false;
			}

			controlStream = new NetworkStream (sock);
			controlReader = new StreamReader (controlStream, Encoding.ASCII);

			if (!Authenticate ()) {
				SetResponseError (CreateExceptionFromResponse (0));
				return false;
			}

			return true;
		}

		// Probably we could do better having here a regex
		Socket SetupPassiveConnection ()
		{
			// Current response string
			string response = statusDescription;
			if (response.Length < 4)
				return null;
			
			// Look for first digit after code
			int i;
			for (i = 3; i < response.Length && !Char.IsDigit (response [i]); i++)
				;
			if (i >= response.Length)
				return null;

			// Get six elements
			string [] digits = response.Substring (i).Split (new char [] {','}, 6);
			if (digits.Length != 6)
				return null;

			// Clean non-digits at the end of last element
			int j;
			for (j = digits [5].Length - 1; j >= 0 && !Char.IsDigit (digits [5][j]); j--)
				;
			if (j < 0)
				return null;
			
			digits [5] = digits [5].Substring (0, j + 1);

			IPAddress ip;
			try {
				ip = IPAddress.Parse (String.Join (".", digits, 0, 4));
			} catch (FormatException) {
				return null;
			}

			// Get the port
			int p1, p2, port;
			if (!Int32.TryParse (digits [4], out p1) || !Int32.TryParse (digits [5], out p2))
				return null;

			port = (p1 << 8) + p2; // p1 * 256 + p2
			//port = p1 * 256 + p2;
			if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
				return null;

			IPEndPoint ep = new IPEndPoint (ip, port);
			Socket sock = new Socket (ep.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			try {
				sock.Connect (ep);
			} catch (SocketException exc) {
				sock.Close ();
				return null;
			}

			return sock;
		}

		Exception CreateExceptionFromResponse (WebExceptionStatus status)
		{
			WebException exc = new WebException ("Server returned an error: " + statusDescription, null, status,
					ftpResponse);
			return exc;
		}
		
		// Here we could also get a server error, so be cautious
		internal void SetTransferCompleted ()
		{
			if (transferCompleted)
				return;
			
			transferCompleted = true;
			
			FtpStatusCode status = GetResponseCode ();
			ftpResponse.StatusCode = status;
			ftpResponse.StatusDescription = statusDescription;
		}

		internal void SetResponseError (Exception exc)
		{
			FtpAsyncResult ar = asyncRead;
			if (ar == null)
				ar = asyncWrite;

			ar.SetCompleted (true, exc);
			ar.DoCallback ();
		}

		Socket InitDataConnection ()
		{
			FtpStatusCode status;
			
			if (usePassive) {
				status = SendCommand (PassiveCommand);
				if (status != FtpStatusCode.EnteringPassive) {
					SetResponseError (CreateExceptionFromResponse (0));
					return null;
				}
				
				Socket retval = SetupPassiveConnection ();
				if (retval == null)
					SetResponseError (new WebException ("Couldn't setup passive connection"));
					
				return retval;
			}

			// Open a socket to listen the server's connection
			Socket sock = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			try {
				sock.Bind (new IPEndPoint (localEndPoint.Address, 0));
				sock.Listen (1); // We only expect a connection from server

			} catch (SocketException e) {
				sock.Close ();

				SetResponseError (new WebException ("Couldn't open listening socket on client", e));
				return null;
			}

			IPEndPoint ep = (IPEndPoint) sock.LocalEndPoint;
			string ipString = ep.Address.ToString ().Replace (".", ",");
			int h1 = ep.Port >> 8; // ep.Port / 256
			int h2 = ep.Port % 256;

			string portParam = ipString + "," + h1 + "," + h2;
			status = SendCommand (PortCommand, portParam);
			if (status != FtpStatusCode.CommandOK) {
				sock.Close ();
				
				SetResponseError (CreateExceptionFromResponse (0));
				return null;
			}

			return sock;
		}

		bool OpenDataConnection ()
		{
			FtpStatusCode status;
			Socket s = InitDataConnection ();
			if (s == null)
				return false;

			// TODO - Check that this command is only used for data connection based commands
			if (method != WebRequestMethods.Ftp.ListDirectory && method != WebRequestMethods.Ftp.ListDirectoryDetails) {
				status = SendCommand (TypeCommand, DataType);
				
				if (status != FtpStatusCode.CommandOK) {
					SetResponseError (CreateExceptionFromResponse (0));
					return false;
				}
			}

			status = SendCommand (method, requestUri.LocalPath);
			if (status != FtpStatusCode.OpeningData) {
				SetResponseError (CreateExceptionFromResponse (0));
				return false;
			}
			
			if (usePassive) {
				dataSocket = s;
				return true;
			}

			// Active connection (use Socket.Blocking to true)
			Socket incoming = null;
			try {
				incoming = s.Accept ();
			} catch (SocketException e) {
				s.Close ();
				if (incoming != null)
					incoming.Close ();
				
				SetResponseError (new ProtocolViolationException ("Server commited a protocol violation."));
				return false;
			} 

			s.Close ();
			dataSocket = incoming;
			return true;
		}

		// Take in count 'account' case
		bool Authenticate ()
		{
			string username = null;
			string password = null;
			
			if (credentials != null) {
				username = credentials.UserName;
				password = credentials.Password;
				// account = credentials.Domain;
			}

			if (username == null)
				username = "anonymous";
			if (password == null)
				password = "@anonymous";

			// Connect to server and get banner message
			FtpStatusCode status = GetResponseCode ();
			ftpResponse.BannerMessage = statusDescription;
			if (status != FtpStatusCode.SendUserCommand)
				return false;

			status = SendCommand (UserCommand, username);
			if (status == FtpStatusCode.LoggedInProceed) {
				ftpResponse.WelcomeMessage = statusDescription;
				return true;
			}
			if (status == FtpStatusCode.SendPasswordCommand) {
				status = SendCommand (PasswordCommand, password);
				if (status == FtpStatusCode.LoggedInProceed) {
					ftpResponse.WelcomeMessage = statusDescription;
					return true;
				}

				return false;
			}

			return false;
		}

		FtpStatusCode SendCommand (string command, params string [] parameters)
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
				return FtpStatusCode.ServiceNotAvalaible;
			}

			return GetResponseCode ();
		}

		internal FtpStatusCode GetResponseCode ()
		{
			string responseString = null;
			try {
				responseString = controlReader.ReadLine ();
			} catch (IOException exc) {
				// controlReader.Close ();
			}

			if (responseString == null || responseString.Length < 3)
				return FtpStatusCode.ServiceNotAvalaible;

			string codeString = responseString.Substring (0, 3);
			int code;
			if (!Int32.TryParse (codeString, out code))
				return FtpStatusCode.ServiceNotAvalaible;

			statusDescription = responseString;
			return statusCode = (FtpStatusCode) code;
		}

	}
}

#endif

