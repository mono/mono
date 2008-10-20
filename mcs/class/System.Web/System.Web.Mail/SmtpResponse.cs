//
// System.Web.Mail.SmtpResponse.cs
//
// Author(s):
//   Per Arneng <pt99par@student.bth.se>
//
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

namespace System.Web.Mail {

    /// this class represents the response from the smtp server
    internal class SmtpResponse {
	
	string rawResponse;
	int statusCode;
	string[] parts;

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
