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
using System;
using System.Net;
using System.IO;
using System.Text;
using System.Collections;
using System.Net.Sockets;

namespace System.Web.Mail {

    /// represents a conntection to a smtp server
    internal class SmtpClient {
	
	private string server;
	private TcpClient tcpConnection;
	private SmtpStream smtp;
	private Encoding encoding;
	
	//Initialise the variables and connect
	public SmtpClient( string server ) {
	    
	    this.server = server;
	    encoding = new ASCIIEncoding( );

	    Connect();
	}
	
	// make the actual connection
	// and HELO handshaking
	private void Connect() {
	    tcpConnection = new TcpClient( server , 25 );
	    
	    Stream stream = tcpConnection.GetStream();
	    smtp = new SmtpStream( stream );
	    
	    // read the server greeting
	    smtp.ReadResponse();
	    smtp.CheckForStatusCode( 220 );
	   
	    // write the HELO command to the server
	    smtp.WriteHelo( Dns.GetHostName() );
	    	    
	}
	
	public void Send( MailMessageWrapper msg ) {
	    
	    if( msg.From == null ) {
		throw new SmtpException( "From property must be set." );
	    }

	    if( msg.To == null ) {
		if( msg.To.Count < 1 ) throw new SmtpException( "Atleast one recipient must be set." );
	    }
	    
	    	    
	    // start with a reset incase old data
	    // is present at the server in this session
	    smtp.WriteRset();
	    
	    // write the mail from command
	    smtp.WriteMailFrom( msg.From.Address );
	    
	    // write the rcpt to command for the To addresses
	    foreach( MailAddress addr in msg.To ) {
		smtp.WriteRcptTo( addr.Address );
	    }

	    // write the rcpt to command for the Cc addresses
	    foreach( MailAddress addr in msg.Cc ) {
		smtp.WriteRcptTo( addr.Address );
	    }

	    // write the rcpt to command for the Bcc addresses
	    foreach( MailAddress addr in msg.Bcc ) {
		smtp.WriteRcptTo( addr.Address );
	    }
	    
	    // write the data command and then
	    // send the email
	    smtp.WriteData();
		
	    if( msg.Attachments.Count == 0 ) {
#if NET_2_0
		//The message might be multipart, if RelatedBodyParts are present
		if (msg.RelatedBodyParts.Count != 0)
			SendMultipartMail (msg);
		else
#endif		
			SendSinglepartMail( msg );	    
	    } else {
		
		SendMultipartMail( msg );
	    
	    }

	    // write the data end tag "."
	    smtp.WriteDataEndTag();

	}
	
	// sends a single part mail to the server
	private void SendSinglepartMail( MailMessageWrapper msg ) {
	    	    	    
	    // write the header
	    smtp.WriteHeader( msg.Header );
	    
	    // send the mail body
	    smtp.WriteBytes( msg.BodyEncoding.GetBytes( msg.Body ) );

	}
	
	// sends a multipart mail to the server
	private void SendMultipartMail( MailMessageWrapper msg ) {
	    	    
	    // generate the boundary between attachments
	    string boundary = MailUtil.GenerateBoundary();
		
	    // set the Content-Type header to multipart/mixed
	    string bodyContentType = msg.Header.ContentType;

#if NET_2_0
		if (msg.RelatedBodyParts.Count != 0)
			msg.Header.ContentType = String.Format( "multipart/related;\r\n   boundary={0}" , boundary );
		else
#endif
	    msg.Header.ContentType = 
		String.Format( "multipart/mixed;\r\n   boundary={0}" , boundary );
		
	    // write the header
	    smtp.WriteHeader( msg.Header );
		
	    // write the first part text part
	    // before the attachments
	    smtp.WriteBoundary( boundary );
		
	    MailHeader partHeader = new MailHeader();
	    partHeader.ContentType = bodyContentType;		

#if NET_1_1
		// Add all the custom headers to body part as specified in 
		//Fields property of MailMessageWrapper
		partHeader.Data.Add(msg.Fields.Data);
#endif

	    smtp.WriteHeader( partHeader );
	  
	    // FIXME: probably need to use QP or Base64 on everything higher
	    // then 8-bit .. like utf-16
	    smtp.WriteBytes( msg.BodyEncoding.GetBytes( msg.Body )  );

	    smtp.WriteBoundary( boundary );

#if NET_2_0
		for (int i = 0; i < msg.RelatedBodyParts.Count; i++) {
			RelatedBodyPart rbp = (RelatedBodyPart) msg.RelatedBodyParts [i];
			FileInfo file = new FileInfo (rbp.Path);
			MailHeader header = new MailHeader ();
			header.ContentLocation = rbp.Path;
			header.ContentType = String.Format ("application/octet-stream");
			if (rbp.Name != null)
				header.Data.Add ("Content-ID", rbp.Name);
			
			header.ContentTransferEncoding = "BASE64";
			smtp.WriteHeader (header);
			FileStream rbpStream = new FileStream (file.FullName, FileMode.Open);
			IAttachmentEncoder rbpEncoder = new Base64AttachmentEncoder ();
			rbpEncoder.EncodeStream (rbpStream, smtp.Stream);
			rbpStream.Close();
			smtp.WriteLine( "" );
			
			if (i < (msg.RelatedBodyParts.Count - 1)) {
				smtp.WriteBoundary (boundary);
			} else {
				if (msg.Attachments.Count == 0)
			   		 smtp.WriteFinalBoundary (boundary);
				else
			    		smtp.WriteBoundary (boundary);
					
			}						
		}
#endif	    
	    // now start to write the attachments
	    
	    for( int i=0; i< msg.Attachments.Count ; i++ ) {
		MailAttachment a = (MailAttachment)msg.Attachments[ i ];
			
		FileInfo fileInfo = new FileInfo( a.Filename );

		MailHeader aHeader = new MailHeader();
		
		aHeader.ContentType = 
		    String.Format( "application/octet-stream; name=\"{0}\"", 
				   fileInfo.Name  );
		
		aHeader.ContentDisposition = 
		    String.Format( "attachment; filename=\"{0}\"" , fileInfo.Name );
		
		aHeader.ContentTransferEncoding = a.Encoding.ToString();
		    		
		smtp.WriteHeader( aHeader );
		   
		// perform the actual writing of the file.
		// read from the file stream and write to the tcp stream
		FileStream ins = new FileStream( fileInfo.FullName  , FileMode.Open );
		
		// create an apropriate encoder
		IAttachmentEncoder encoder;
		if( a.Encoding == MailEncoding.UUEncode ) {
		    encoder = new UUAttachmentEncoder( 644 , fileInfo.Name  );
		} else {
		    encoder = new Base64AttachmentEncoder();
		}
		
		encoder.EncodeStream( ins , smtp.Stream );
		
		ins.Close();
		
		    
		smtp.WriteLine( "" );
		
		// if it is the last attachment write
		// the final boundary otherwise write
		// a normal one.
		if( i < (msg.Attachments.Count - 1) ) { 
		    smtp.WriteBoundary( boundary );
		} else {
		    smtp.WriteFinalBoundary( boundary );
		}
		    
	    }
	       
	}
	
	// send quit command and
	// closes the connection
	public void Close() {
	    
	    smtp.WriteQuit();
	    tcpConnection.Close();
	
	}
	
		
    }

}
