// SmtpClient.cs
// author: Per Arneng <pt99par@student.bth.se>
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
	    
	    // if no encoding is set then set the system
	    // default encoding
	    if( msg.BodyEncoding == null ) 
		msg.BodyEncoding = Encoding.Default;
	    
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
	    
	    // write the data command and then
	    // send the email
	    smtp.WriteData();
	   

	    if( msg.Attachments.Count == 0 ) {
		
		SendSinglepartMail( msg );
	    
	    } else {
		
		SendMultipartMail( msg );
	    
	    }

	    // write the data end tag "."
	    smtp.WriteDataEndTag();

	}
	
	// sends a single part mail to the server
	private void SendSinglepartMail( MailMessageWrapper msg ) {
	    	    	    
	    // create the headers
	    IDictionary headers = CreateHeaders( msg );
	
	    smtp.WriteHeaders( headers );
	    
	    // send the mail body FIXME
	    smtp.WriteBytes( msg.BodyEncoding.GetBytes( msg.Body ) );

	}
	
	// sends a multipart mail to the server
	private void SendMultipartMail( MailMessageWrapper msg ) {
	    	    	    
	    // create the headers
	    IDictionary headers = CreateHeaders( msg );

	    // set the part boundary FIXME: THIS SHOULD NOT BE HARDCODED
	    // look att  Gaurav Vaish implementation
	    string boundary = "NextPart_000_1113_1962_1fe8";
		
	    // set the Content-Type header to multipart/mixed
	    headers[ "Content-Type" ] = 
		String.Format( "multipart/mixed;\r\n   boundary={0}" , boundary );
		
	    // write the headers
	    // and start writing the multipart body
	    smtp.WriteHeaders( headers );
		
	    // write the first part text part
	    // before the attachments
	    smtp.WriteBoundary( boundary );
		
	    Hashtable partHeaders = new Hashtable();
	    partHeaders[ "Content-Type" ] = "text/plain";
		
	    smtp.WriteHeaders( partHeaders );
	    	
	  
	    // FIXME: probably need to use QP or Base64 on everything higher
	    // then 8-bit .. like utf-16
	    smtp.WriteBytes( msg.BodyEncoding.GetBytes( msg.Body )  );

	    smtp.WriteBoundary( boundary );
	    
	    // now start to write the attachments
	    
	    for( int i=0; i< msg.Attachments.Count ; i++ ) {
		MailAttachment a = (MailAttachment)msg.Attachments[ i ];
			
		FileInfo fileInfo = new FileInfo( a.Filename );

		Hashtable aHeaders = new Hashtable();
		
		aHeaders[ "Content-Type" ] = 
		    String.Format( "application/octet-stream; name=\"{0}\"", 
				   fileInfo.Name  );
		
		aHeaders[ "Content-Disposition" ] = 
		    String.Format( "attachment; filename=\"{0}\"" , fileInfo.Name );
		
		aHeaders[ "Content-Transfer-Encoding" ] = 
		    (a.Encoding == MailEncoding.UUEncode ? "UUEncode" : "Base64" );
		
		smtp.WriteHeaders( aHeaders );
		   
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
	
	// send the standard headers
	// and the custom in MailMessage
	private IDictionary CreateHeaders( MailMessageWrapper msg ) {
	    Hashtable headers = new Hashtable(); 
	    
	    headers[ "From" ] = msg.From.ToString();
	    headers[ "To" ] = msg.To.ToString();
	    	    
	    if( msg.Cc.Count > 0 ) headers[ "Cc" ] = msg.Cc.ToString();
			    
	    if( msg.Bcc.Count > 0 ) headers[ "Bcc" ] = msg.Bcc.ToString();
	    
	    if( HasData( msg.Subject ) ) {
		
		// if the BodyEncoding is not 7bit us-ascii then
		// convert using base64 
		if( msg.BodyEncoding is ASCIIEncoding ) {
		
		    headers[ "Subject" ] = msg.Subject;
		
		} else {
		
		    byte[] subjectBytes = msg.BodyEncoding.GetBytes( msg.Subject );
		    // encode the subject with Base64
		    headers[ "Subject" ] = 
			String.Format( "=?{0}?{1}?{2}?=" , 
				       msg.BodyEncoding.BodyName , "B",
				       Convert.ToBase64String( subjectBytes ) );
		}

	    }
	    
	    if( HasData( msg.UrlContentBase ) ) 
		headers[ "Content-Base" ] = msg.UrlContentBase;
	    
	    if( HasData( msg.UrlContentLocation ) ) 
		headers[ "Content-Location" ] = msg.UrlContentLocation;
	   
	    
	    string charset = String.Format( "charset=\"{0}\"" , msg.BodyEncoding.BodyName );

	    // set body the content type
	    switch( msg.BodyFormat ) {
		
	    case MailFormat.Html: 
		headers[ "Content-Type" ] = "text/html; " + charset; 
		break;
	    
	    case MailFormat.Text: 
		headers[ "Content-Type" ] = "text/plain; " + charset; 
		break;
	    
	    default: 
		headers[ "Content-Type" ] = "text/plain; " + charset; 
		break;

	    }
	    
	    // set the priority as in the same way as .NET sdk does
	    switch( msg.Priority ) {
		
	    case MailPriority.High: 
		headers[ "Importance" ] = "high";
		break;
	    
	    case MailPriority.Low: 
		headers[ "Importance" ] = "low";
		break;
		
	    case MailPriority.Normal: 
		headers[ "Importance" ] = "normal";
		break;
		
	    default: 
		headers[ "Importance" ] = "normal";
		break;

	    }
	    
	    // .NET sdk allways sets this to normal
	    headers[ "Priority" ] = "normal";
	    

	    // add mime version
	    headers[ "Mime-Version" ] = "1.0";
	    
	    // set the mailer -- should probably be changed
	    headers[ "X-Mailer" ] = "Mono (System.Web.Mail.SmtpMail.Send)";
	    
	    // Set the transfer encoding.. it seems like only sends 7bit
	    // if it is ASCII
	    if( msg.BodyEncoding is ASCIIEncoding ) {
		headers[ "Content-Transfer-Encoding" ] = "7bit";
	    } else {
		headers[ "Content-Transfer-Encoding" ] = "8bit";
	    }
	    
	    
	    // add the custom headers they will overwrite
	    // the earlier ones if they are the same
	    foreach( string key in msg.Headers.Keys )
		headers[ key ] = (string)msg.Headers[ key ];
		
	    

	    return headers;
	}
	
	// returns true if str is not null and not
	// empty
	private bool HasData( string str ) {
	    bool hasData = false;
	    if( str != null ) {
		if( str.Length > 0 ) {
		    hasData = true;
		}
	    }
	    return hasData;
	}
	
	
	// send quit command and
	// closes the connection
	public void Close() {
	    
	    smtp.WriteQuit();
	    tcpConnection.Close();
	
	}
	
		
    }

}
