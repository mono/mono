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
	
	public void Send( MailMessage msg ) {

	    if( ( ! HasData( msg.From )  ) || ( ! HasData( msg.To ) ) )
		throw new SmtpException( "From & To properties must be set." );
	    
	    // if no encoding is set then set the system
	    // default encoding
	    if( msg.BodyEncoding == null ) 
		msg.BodyEncoding = Encoding.Default;
	    
	    // start with a reset incase old data
	    // is present at the server in this session
	    smtp.WriteRset();
	    
	    // write the mail from command
	    smtp.WriteMailFrom( msg.From );
	    	    
	    // write the rcpt to command
	    smtp.WriteRcptTo( msg.To );
	    
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
	private void SendSinglepartMail( MailMessage msg ) {
	    	    	    
	    // create the headers
	    IDictionary headers = CreateHeaders( msg );
	
	    smtp.WriteHeaders( headers );
	    
	    // send the mail body
	    smtp.WriteLine( msg.Body );

	}
	
	// sends a multipart mail to the server
	private void SendMultipartMail( MailMessage msg ) {
	    	    	    
	    // create the headers
	    IDictionary headers = CreateHeaders( msg );

	    // set the part boundary
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
	    		
	    smtp.WriteLine( msg.Body );

	    smtp.WriteBoundary( boundary );
	    
	    // now start to write the attachments

	    for( int i=0; i< msg.Attachments.Count ; i++ ) {
		MailAttachment a = (MailAttachment)msg.Attachments[ i ];
		FileStream file = 
		    new FileStream( a.Filename , FileMode.Open );
		    		    
		Hashtable aHeaders = new Hashtable();
		
		aHeaders[ "Content-Type" ] = 
		    String.Format( "unknown/unknown; name=\"{0}\"", a.Filename );
		
		aHeaders[ "Content-Disposition" ] = 
		    String.Format( "attachment; filename=\"{0}\"" , a.Filename );
		
		aHeaders[ "Content-Transfer-Encoding" ] = "base64";
			
		smtp.WriteHeaders( aHeaders );
		    
		smtp.WriteBase64( file );
		    
		smtp.WriteLine( "" );
		
		// if it is the last attachment write
		// the final boundary otherwise write
		// a normal one.
		if( i < (msg.Attachments.Count - 1) ) { 
		    smtp.WriteBoundary( boundary );
		} else {
		    smtp.WriteFinalBoundary( boundary );
		}
		    
		file.Close();
	    }
	       
	}
	
	// send the standard headers
	// and the custom in MailMessage
	// FIXME: more headers needs to be added so
	// that all properties from MailMessage are sent..
	// missing: Priority , UrlContentBase,UrlContentLocation
	private IDictionary CreateHeaders( MailMessage msg ) {
	    Hashtable headers = new Hashtable(); 
	    
	    headers[ "From" ] = msg.From;
	    headers[ "To" ] = msg.To;
	    	    
	    if( HasData( msg.Cc ) ) headers[ "Cc" ] = msg.Cc;
			    
	    if( HasData( msg.Bcc ) ) headers[ "Bcc" ] = msg.Bcc;
	    
	    if( HasData( msg.Subject ) ) headers[ "Subject" ] = msg.Subject;
	    
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
