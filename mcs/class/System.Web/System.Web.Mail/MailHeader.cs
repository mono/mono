//
// System.Web.Mail.MailHeader.cs
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
using System.Collections;
using System.Collections.Specialized;

namespace System.Web.Mail {
    
    // This class represents the header of a mail with
    // all the header fields.
    internal class MailHeader {
	
	protected NameValueCollection data = new NameValueCollection();
	
	public string To {
	    get { return data[ "To" ]; }
	    set { data[ "To" ] = value; }
	}

	public string From {
	    get { return data[ "From" ]; }
	    set { data[ "From" ] = value; }
	}

	public string Cc {
	    get { return data[ "Cc" ]; }
	    set { data[ "Cc" ] = value; }
	}
	
	public string Bcc {
	    get { return data[ "Bcc" ]; }
	    set { data[ "Bcc" ] = value; }
	}
	
	public string Subject {
	    get { return data[ "Subject" ]; }
	    set { data[ "Subject" ] = value; }
	}

	public string Importance {
	    get { return data[ "Importance" ]; }
	    set { data[ "Importance" ] = value; }
	}
	
	public string Priority {
	    get { return data[ "Priority" ]; }
	    set { data[ "Priority" ] = value; }
	}
	
	public string MimeVersion {
	    get { return data[ "Mime-Version" ]; }
	    set { data[ "Mime-Version" ] = value; }
	}

	public string ContentType {
	    get { return data[ "Content-Type" ]; }
	    set { data[ "Content-Type" ] = value; }
	} 
	
	public string ContentTransferEncoding{
	    get { return data[ "Content-Transfer-Encoding" ]; }
	    set { data[ "Content-Transfer-Encoding" ] = value; }
	} 

	public string ContentDisposition {
	    get { return data[ "Content-Disposition" ]; }
	    set { data[ "Content-Disposition" ] = value; }
	} 

	public string ContentBase {
	    get { return data[ "Content-Base" ]; }
	    set { data[ "Content-Base" ] = value; }
	}
	
	public string ContentLocation {
	    get { return data[ "Content-Location" ]; }
	    set { data[ "Content-Location" ] = value; }
	}	
	
	
	public NameValueCollection Data {
	   get { return data; } 
	}
	
    }

}
