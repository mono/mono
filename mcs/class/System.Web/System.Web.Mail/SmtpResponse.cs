//
// System.Web.Mail.SmtpResponse.cs
//
// Author(s):
//   Per Arneng <pt99par@student.bth.se>
//
//
using System;

namespace System.Web.Mail {

    /// this class represents the response from the smtp server
    internal class SmtpResponse {
	
	private string rawResponse;
	private int statusCode;
	private string[] parts;

	/// use the Parse method to create instances
	protected SmtpResponse() {}

	/// the smtp status code FIXME: change to Enumeration?
	public int StatusCode {
	    get { return statusCode; }
	    set { statusCode = value; }
	}
	
	/// the response as it was recieved
	public string RawResponse {
	    get { return rawResponse; }
	    set { rawResponse = value; }
	}

	/// the response as parts where ; was used as delimiter
	public string[] Parts {
	    get { return parts; }
	    set { parts = value; }
	}

	/// parses a new response object from a response string
	public static SmtpResponse Parse( string line ) {
	    SmtpResponse response = new SmtpResponse();
	    
	    if( line.Length < 4 ) 
		throw new SmtpException( "Response is to short " + 
					   line.Length + ".");
	    
	    if( ( line[ 3 ] != ' ' ) && ( line[ 3 ] != '-' ) )
		throw new SmtpException( "Response format is wrong.(" + 
					 line + ")" );
	    
	    // parse the response code
	    response.StatusCode = Int32.Parse( line.Substring( 0 , 3 ) );
	    
	    // set the rawsponse
	    response.RawResponse = line;

	    // set the response parts
	    response.Parts = line.Substring( 0 , 3 ).Split( ';' );

	    return response;
	}
    }

}
