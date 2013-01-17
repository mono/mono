//
// System.Web.Mail.SmtpClient.cs
//
// Author(s):
//   Per Arneng <pt99par@student.bth.se>
//   Sanjay Gupta <gsanjay@novell.com>
//   (C) 2004, Novell, Inc. (http://www.novell.com)
//

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

using System.Net;
using System.IO;
using System.Text;
using System.Collections;
using System.Net.Sockets;
using System.Security.Permissions;
using System.Reflection;

namespace System.Web.Mail {

	/// represents a conntection to a smtp server
	internal class SmtpClient
	{
		string server;
		TcpClient tcpConnection;
		SmtpStream smtp;
		string username;
		string password;
		int port = 25;
		bool usessl = false;
		short authenticate = 1;  	
        
		//Initialise the variables and connect
		public SmtpClient (string server)
		{
			this.server = server;
		}
	
		// make the actual connection
		// and HELO handshaking
		void Connect ()
		{
			tcpConnection = new TcpClient (server, port);
	    
			NetworkStream stream = tcpConnection.GetStream ();
			smtp = new SmtpStream (stream);
		}
	    	    
		void ChangeToSSLSocket ()
		{
#if TARGET_JVM
			java.lang.Class c = vmw.common.TypeUtils.ToClass (smtp.Stream);
			java.lang.reflect.Method m = c.getMethod ("ChangeToSSLSocket", null);
			m.invoke (smtp.Stream, new object[]{});
#else
			// Load Mono.Security.dll
			Assembly a;
			try {
				a = Assembly.Load (Consts.AssemblyMono_Security);
			} catch (System.IO.FileNotFoundException) {
				throw new SmtpException ("Cannot load Mono.Security.dll");
			}
			Type tSslClientStream = a.GetType ("Mono.Security.Protocol.Tls.SslClientStream");
			object[] consArgs = new object[4];
			consArgs[0] = smtp.Stream;
			consArgs[1] = server;
			consArgs[2] = true;
			Type tSecurityProtocolType = a.GetType ("Mono.Security.Protocol.Tls.SecurityProtocolType");
			int nSsl3Val = (int) Enum.Parse (tSecurityProtocolType, "Ssl3");
			int nTlsVal = (int) Enum.Parse (tSecurityProtocolType, "Tls");
			consArgs[3] = Enum.ToObject (tSecurityProtocolType, nSsl3Val | nTlsVal);

			object objSslClientStream = Activator.CreateInstance (tSslClientStream, consArgs); 

			if (objSslClientStream != null)
				smtp = new SmtpStream ((Stream)objSslClientStream);
#endif
		}
		
		void ReadFields (MailMessageWrapper msg)
		{
			string tmp;
			username = msg.Fields.Data ["http://schemas.microsoft.com/cdo/configuration/sendusername"];
			password = msg.Fields.Data ["http://schemas.microsoft.com/cdo/configuration/sendpassword"]; 
			tmp = msg.Fields.Data ["http://schemas.microsoft.com/cdo/configuration/smtpauthenticate"]; 
			if (tmp != null)
				authenticate = short.Parse (tmp);
			tmp = msg.Fields.Data ["http://schemas.microsoft.com/cdo/configuration/smtpusessl"]; 	
			if (tmp != null)
				usessl = bool.Parse (tmp);
			tmp = msg.Fields.Data ["http://schemas.microsoft.com/cdo/configuration/smtpserverport"]; 
			if (tmp != null)
				port = int.Parse (tmp);
		}

		void StartSend (MailMessageWrapper msg)
		{
			ReadFields (msg);
			Connect ();

			// read the server greeting
			smtp.ReadResponse ();
			smtp.CheckForStatusCode (220);

			if (usessl || (username != null && password != null && authenticate != 1)) 
			{
				smtp.WriteEhlo (Dns.GetHostName ());

				if (usessl) {
					bool isSSL = smtp.WriteStartTLS ();
					if (isSSL)
						ChangeToSSLSocket ();
				}

				if (username != null && password != null && authenticate != 1) {
					smtp.WriteAuthLogin ();
					if (smtp.LastResponse.StatusCode == 334) {
						smtp.WriteLine (Convert.ToBase64String (Encoding.ASCII.GetBytes (username)));
						smtp.ReadResponse ();
						smtp.CheckForStatusCode (334);
						smtp.WriteLine (Convert.ToBase64String (Encoding.ASCII.GetBytes (password)));
						smtp.ReadResponse ();
						smtp.CheckForStatusCode (235);
					}
				}
			} else  {
				smtp.WriteHelo (Dns.GetHostName ());
			}
		}
	
		public void Send (MailMessageWrapper msg)
		{
			if (msg.From == null)
				throw new SmtpException ("From property must be set.");

			if (msg.To == null)
				if (msg.To.Count < 1)
					throw new SmtpException ("Atleast one recipient must be set.");
	    
			StartSend (msg);
			// start with a reset incase old data
			// is present at the server in this session
			smtp.WriteRset ();
	    
			// write the mail from command
			smtp.WriteMailFrom (msg.From.Address);
	    
			// write the rcpt to command for the To addresses
			foreach (MailAddress addr in msg.To)
				smtp.WriteRcptTo (addr.Address);

			// write the rcpt to command for the Cc addresses
			foreach (MailAddress addr in msg.Cc)
				smtp.WriteRcptTo (addr.Address);

			// write the rcpt to command for the Bcc addresses
			foreach (MailAddress addr in msg.Bcc)
				smtp.WriteRcptTo (addr.Address);
	    
			// write the data command and then
			// send the email
			smtp.WriteData ();
		
			if (msg.Attachments.Count == 0)
				SendSinglepartMail (msg);	    
			else
				SendMultipartMail (msg);

			// write the data end tag "."
			smtp.WriteDataEndTag ();
		}
	
		// sends a single part mail to the server
		void SendSinglepartMail (MailMessageWrapper msg)
		{	    	    	    
			// write the header
			smtp.WriteHeader (msg.Header);
	    
			// send the mail body
			smtp.WriteBytes (msg.BodyEncoding.GetBytes (msg.Body));
		}

		// SECURITY-FIXME: lower assertion with imperative asserts	
		[FileIOPermission (SecurityAction.Assert, Unrestricted = true)]
		// sends a multipart mail to the server
		void SendMultipartMail (MailMessageWrapper msg)
		{    
			// generate the boundary between attachments
			string boundary = MailUtil.GenerateBoundary ();
		
			// set the Content-Type header to multipart/mixed
			string bodyContentType = msg.Header.ContentType;

			msg.Header.ContentType = String.Concat ("multipart/mixed;\r\n   boundary=", boundary);
		
			// write the header
			smtp.WriteHeader (msg.Header);
		
			// write the first part text part
			// before the attachments
			smtp.WriteBoundary (boundary);
		
			MailHeader partHeader = new MailHeader ();
			partHeader.ContentType = bodyContentType;		

			// Add all the custom headers to body part as specified in 
			//Fields property of MailMessageWrapper

			//Remove fields specific for authenticating to SMTP server.
			//Need to incorporate AUTH command in SmtpStream to handle  
			//Authorization info. Its a temporary fix for Bug no 68829.
			//Will dig some more on SMTP AUTH command, and then implement
			//Authorization. - Sanjay

			if (msg.Fields.Data ["http://schemas.microsoft.com/cdo/configuration/smtpauthenticate"] != null)
				msg.Fields.Data.Remove ("http://schemas.microsoft.com/cdo/configuration/smtpauthenticate");
			if (msg.Fields.Data ["http://schemas.microsoft.com/cdo/configuration/sendusername"] != null)
				msg.Fields.Data.Remove ("http://schemas.microsoft.com/cdo/configuration/sendusername");
			if (msg.Fields.Data ["http://schemas.microsoft.com/cdo/configuration/sendpassword"] != null)
				msg.Fields.Data.Remove ("http://schemas.microsoft.com/cdo/configuration/sendpassword");
			partHeader.Data.Add (msg.Fields.Data);

			smtp.WriteHeader (partHeader);
	  
			// FIXME: probably need to use QP or Base64 on everything higher
			// then 8-bit .. like utf-16
			smtp.WriteBytes (msg.BodyEncoding.GetBytes (msg.Body) );

			smtp.WriteBoundary (boundary);

			// now start to write the attachments
	    
			for (int i=0; i< msg.Attachments.Count ; i++) {
				MailAttachment a = (MailAttachment)msg.Attachments[ i ];
				FileInfo fileInfo = new FileInfo (a.Filename);
				MailHeader aHeader = new MailHeader ();
		
				aHeader.ContentType = 
					String.Concat (MimeTypes.GetMimeType (fileInfo.Name), "; name=\"", fileInfo.Name, "\"");
		
				aHeader.ContentDisposition = String.Concat ("attachment; filename=\"", fileInfo.Name, "\"");
				aHeader.ContentTransferEncoding = a.Encoding.ToString();
				smtp.WriteHeader (aHeader);
		   
				// perform the actual writing of the file.
				// read from the file stream and write to the tcp stream
				FileStream ins = fileInfo.OpenRead ();
		
				// create an apropriate encoder
				IAttachmentEncoder encoder;
				if (a.Encoding == MailEncoding.UUEncode)
					encoder = new UUAttachmentEncoder (644, fileInfo.Name );
				else
					encoder = new Base64AttachmentEncoder ();
		
				encoder.EncodeStream (ins, smtp.Stream);
		
				ins.Close ();
				smtp.WriteLine ("");
		
				// if it is the last attachment write
				// the final boundary otherwise write
				// a normal one.
				if (i < (msg.Attachments.Count - 1))
					smtp.WriteBoundary (boundary);
				else
					smtp.WriteFinalBoundary (boundary);
			}
		}
	
		// send quit command and
		// closes the connection
		public void Close()
		{
			smtp.WriteQuit();
			tcpConnection.Close();
		}
	}
}
