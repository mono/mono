//
// System.Web.Mail.MailMessageWrapper.cs
//
// Author(s):
//   Per Arneng <pt99par@student.bth.se>
//   Sanjay Gupta <gsanjay@novell.com>
//
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
using System.Collections;
using System.Text;

namespace System.Web.Mail {

    // wraps a MailMessage to make an easier
    // interface to work with collections of
    // addresses instead of a single string
    internal class MailMessageWrapper {
		
	MailAddressCollection bcc = new MailAddressCollection();
	MailAddressCollection cc = new MailAddressCollection();
	MailAddress from;
	MailAddressCollection to = new MailAddressCollection();
	MailHeader header = new MailHeader();
	MailMessage message;
	string body;
		
	// Constructor		
	public MailMessageWrapper( MailMessage message )
	{
	    this.message = message;
	    
	    if( message.From != null ) {
			from = MailAddress.Parse( message.From );
			header.From = from.ToString();
	    }
	    
	    if( message.To != null ) {
		to = MailAddressCollection.Parse( message.To );
		header.To = to.ToString();
	    }
	    
	    if( message.Cc != null ) {
		cc = MailAddressCollection.Parse( message.Cc );
		header.Cc = cc.ToString();
	    }
		
	    if( message.Bcc != null ) {
		bcc = MailAddressCollection.Parse( message.Bcc );
		header.Bcc = bcc.ToString();
	    }
   
	    // set the subject
	    if( message.Subject != null ) {
		
		// encode the subject if it needs encoding
		if( MailUtil.NeedEncoding( message.Subject ) ) {
		    		
		    byte[] subjectBytes = message.BodyEncoding.GetBytes( message.Subject );
		    // encode the subject with Base64
		    header.Subject = "=?" + message.BodyEncoding.BodyName + "?B?" + Convert.ToBase64String (subjectBytes) + "?=";
		} else {
		    
		    header.Subject = message.Subject;
		
		}
	    }

	    // convert single '.' on a line with ".." to not
	    // confuse the smtp server since the DATA command
	    // is terminated with a '.' on a single line.
	    // this is also according to the smtp specs.
	    if( message.Body != null ) {
		body = message.Body.Replace( "\n.\n" , "\n..\n" );
		body = body.Replace( "\r\n.\r\n" , "\r\n..\r\n" );
	    }
	    
	    
	    // set the Contet-Base header
	    if( message.UrlContentBase != null ) 
		header.ContentBase = message.UrlContentBase;
	    
	    // set the Contet-Location header
	    if( message.UrlContentLocation != null ) 
		header.ContentLocation = message.UrlContentLocation;

	    	    
	    // set the content type
	    switch( message.BodyFormat ) {		
		    case MailFormat.Html: 
			    header.ContentType = String.Concat ( "text/html; charset=\"", message.BodyEncoding.BodyName, "\""); 
			    break;
	    
		    case MailFormat.Text: 
			    header.ContentType = String.Concat ( "text/plain; charset=\"", message.BodyEncoding.BodyName, "\"");
			    break;
	    
		    default: 
			    header.ContentType = String.Concat ( "text/html; charset=\"", message.BodyEncoding.BodyName, "\"");
			    break;
	    }
	    
	    	    
	    // set the priority as in the same way as .NET sdk does
	    switch( message.Priority ) {
		
	    case MailPriority.High: 
		header.Importance = "high";
		break;
	    
	    case MailPriority.Low: 
		header.Importance = "low";
		break;
		
	    case MailPriority.Normal: 
		header.Importance = "normal";
		break;
		
	    default: 
		header.Importance = "normal";
		break;

	    }

	    // .NET sdk allways sets this to normal
	    header.Priority = "normal";
	    
	    
	    // Set the mime version
	    header.MimeVersion = "1.0";
	    
	    // Set the transfer encoding
	    if( message.BodyEncoding is ASCIIEncoding ) {
		header.ContentTransferEncoding = "7bit";
	    } else {
		header.ContentTransferEncoding = "8bit";
	    }

	    // Add Date header, we were missing earlier 27/08/04
	    // RFC822 requires date to be in format Fri, 27 Aug 2004 20:13:20 +0530
	    //DateTime.Now gives in format 8/27/2004 8:13:00 PM
	    // Need to explore further dateTime formats available or do we need
	    // to write a function to convert.
		//header.Data.Add ("Date", DateTime.Now.ToString()); 

	    // Add the custom headers
	    foreach( string key in message.Headers.Keys )
		header.Data[ key ] = (string)this.message.Headers[ key ];
	}		
	
	// Properties
	public IList Attachments {
	    get { return message.Attachments; }
	}		
		
	public MailAddressCollection Bcc {
	    get { return bcc; } 
	}
	
	public string Body {
	    get { return body; } 
	    set { body = value; } 
	}

	public Encoding BodyEncoding {
	    get { return message.BodyEncoding; } 
	    set { message.BodyEncoding = value; }
	}

	public MailFormat BodyFormat {
	    get { return message.BodyFormat; } 
	    set { message.BodyFormat = value; }
	}		

	public MailAddressCollection Cc {
	    get { return cc; } 
	}

	public MailAddress From {
	    get { return from; } 
	}

	public MailHeader Header {
	    get { return header; }
	}
		
	public MailPriority Priority {
	    get { return message.Priority; } 
	    set { message.Priority = value; }
	}
		
	public string Subject {
	    get { return message.Subject; } 
	    set { message.Subject = value; }
	}

	public MailAddressCollection To {
	    get { return to; }   
	}

	public string UrlContentBase {
	    get { return message.UrlContentBase; } 
	    
	}

	public string UrlContentLocation {
	    get { return message.UrlContentLocation; } 
	}

		public MailHeader Fields {
			get {
					MailHeader bodyHeaders = new MailHeader();
					// Add Fields to MailHeader Object
					foreach( string key in message.Fields.Keys )
						bodyHeaders.Data[ key ] = this.message.Fields[ key ].ToString();

					return bodyHeaders;
			}
			
		}
    }	 
}
