// SmtpStream.cs
// author: Per Arneng <pt99par@student.bth.se>
using System;
using System.IO;
using System.Collections;
using System.Text;
using System.Security.Cryptography;

namespace System.Web.Mail {

    internal class SmtpStream {
	
	protected Stream stream;
	protected Encoding encoding;
	protected SmtpResponse lastResponse;
	protected string command = "";

	public SmtpStream( Stream stream ) {
	    this.stream = stream;
	    encoding = new ASCIIEncoding();
	}
	
	public SmtpResponse LastResponse {
	    get { return lastResponse; }
	}
	
	public void WriteRset() {
	    command = "RSET";
	    WriteLine( command );
	    ReadResponse();
	    CheckForStatusCode( 250 );
	
	}
	
	public void WriteHelo( string hostName ) { 
	    command = "HELO " + hostName;
	    WriteLine( command );
	    ReadResponse();
	    CheckForStatusCode( 250 );
	    
	}
	
	public void WriteMailFrom( string from ) {
	    command = "MAIL FROM: " + from;
	    WriteLine( command );
	    ReadResponse();
	    CheckForStatusCode( 250 );
	    
	}
	
	public void WriteRcptTo( string to ) {
	    command = "RCPT TO: " + to;  
	    WriteLine( command );
	    ReadResponse();
	    CheckForStatusCode( 250 );
	    	    
	}
	

	public void WriteData() {
	    command = "DATA";
	    WriteLine( command );
	    ReadResponse();
	    CheckForStatusCode( 354 );
	
	}
	
	public void WriteQuit() {
	    command = "QUIT";
	    WriteLine( command );
	    ReadResponse();
	    CheckForStatusCode( 221 );
	
	}
		
	public void WriteBoundary( string boundary ) {
	
	    WriteLine( "--{0}" , boundary );
	
	}
	
	public void WriteFinalBoundary( string boundary ) {
	
	    WriteLine( "--{0}--" , boundary );
	
	}
	
	public void WriteDataEndTag() {
	    command = ".";
	    WriteLine( command );
	    ReadResponse();
	    CheckForStatusCode( 250 );
	
	}
	
	
	public void WriteHeaders( IDictionary headers ) {
	    // write the headers
	    foreach( string key in headers.Keys )
		WriteLine( "{0}: {1}" , key , (string)headers[ key ] );
	    
	    // write the header end tag
	    WriteLine( "" );
	}
	
	public void CheckForStatusCode( int statusCode ) {
	    
	    if( LastResponse.StatusCode != statusCode ) {
		
		string msg = "" + 
		    "Server reponse: '" + lastResponse.RawResponse + "';" +
		    "Status code: '" +  lastResponse.StatusCode + "';" + 
		    "Expected status code: '" + statusCode + "';" + 
		    "Last command: '" + command + "'";
		
		throw new SmtpException( msg ); 
					
	    }
	}
	
	// writes a formatted line to the server
	public void WriteLine( string format ,  params object[] args ) {
	    WriteLine( String.Format( format , args ) );
	}
	
	// writes a line to the server
	public void WriteLine( string line ) {
	    byte[] buffer = encoding.GetBytes( line + "\r\n" );
	    
	    stream.Write( buffer , 0 , buffer.Length );
	
	    #if DEBUG 
	      DebugPrint( line );
            #endif
	}
	
	// read a line from the server
	public void ReadResponse( ) {
	    string line = null;
	    
	    byte[] buffer = new byte[ 4096 ];
	    
	    int readLength = stream.Read( buffer , 0 , buffer.Length );
	    
	    if( readLength > 0 ) { 
	    
		line = encoding.GetString( buffer , 0 , readLength );
		
		line = line.TrimEnd( new Char[] { '\r' , '\n' , ' ' } );
			
	    }
	   
	    // parse the line to the lastResponse object
	    lastResponse = SmtpResponse.Parse( line );
	   
	    #if DEBUG
	      DebugPrint( line );
	    #endif
	}


	// reads bytes from a stream and writes the encoded
	// as base64 encoded characters. ( 60 chars on each row)
	public void WriteBase64( Stream inStream ) {
	
	    ICryptoTransform base64 = new ToBase64Transform();
	    ASCIIEncoding encoding = new ASCIIEncoding();
	
	    // the buffers
	    byte[] plainText = new byte[ base64.InputBlockSize ];
	    byte[] cipherText = new byte[ base64.OutputBlockSize ];

	    int readLength = 0;
	    int trLength = 0;
	
	    StringBuilder row = new StringBuilder( 60 );
	
	    // read through the stream until there 
	    // are no more bytes left
	    while( true ) {
		readLength = inStream.Read( plainText , 0 , plainText.Length );
	    
		// break when there is no more data
		if( readLength < 1 ) break;
	    
		// transfrom and write the blocks. If the block size
		// is less than the InputBlockSize then write the final block
		if( readLength == plainText.Length ) {
		
		    trLength = base64.TransformBlock( plainText , 0 , 
						      plainText.Length ,
						      cipherText , 0 );
		
		    // trLength must be the same size as the cipherText
		    // length otherwise something is wrong
		    if( trLength != cipherText.Length )
			throw new Exception( "All of the plaintext bytes where not converted" );
		
		    // convert the bytes to a string and then add it to the
		    // current row
		    string cipherString = encoding.GetString( cipherText , 0 , trLength ); 
		    row.Append( cipherString );
		
		    // when a row is full write it and begin
		    // on the next row
		    if( row.Length == 60 ) {
			WriteLine( row.ToString() );
			row.Remove( 0 , 60 );
		    }
		
		} else {
		    // convert the final blocks of bytes
		    cipherText = base64.TransformFinalBlock( plainText , 0 , readLength );
		
		    // convert the bytes to a string and then write it
		    string cipherString = encoding.GetString( cipherText , 0 , 
							      cipherText.Length );
		    row.Append( cipherString );
		    WriteLine( row.ToString() );
		
		}
	    
	    } 
    
	}
	
	/// debug printing 
	private void DebugPrint( string line ) {
	    Console.WriteLine( "smtp: {0}" , line );
	}

    }


}
