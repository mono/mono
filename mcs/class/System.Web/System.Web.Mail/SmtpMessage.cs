using System;
using System.Collections;
using System.Text;

namespace System.Web.Mail {

    internal class SmtpMessage {
	
	private IList attachments;
	private MailAddressCollection bcc;
	private string body;
	private Encoding bodyEncoding;
	private MailFormat bodyFormat;
	private MailAddressCollection cc;		
	private MailAddress from;
	private IDictionary headers;
	private MailPriority priority;
	private string subject;
	private MailAddressCollection to;
	private string urlContentBase;
	private string urlContentLocation;
		
	// Constructor		
	public SmtpMessage ()
	{
	    attachments = new ArrayList (8);
	    headers = new Hashtable ();
	    to = new MailAddressCollection();
	    cc = new MailAddressCollection();
	    bcc = new MailAddressCollection();
	    
	}		
	
	// Properties
	public IList Attachments {
	    get { return attachments; }
	    set { attachments = value; }
	}		
		
	public MailAddressCollection Bcc {
	    get { return bcc; } 
	    set { bcc = value; }
	}
	
	public string Body {
	    get { return body; } 
	    set { body = value; }
	}

	public Encoding BodyEncoding {
	    get { return bodyEncoding; } 
	    set { bodyEncoding = value; }
	}

	public MailFormat BodyFormat {
	    get { return bodyFormat; } 
	    set { bodyFormat = value; }
	}		

	public MailAddressCollection Cc {
	    get { return cc; } 
	    set { cc = value; }
	}

	public MailAddress From {
	    get { return from; } 
	    set { from = value; }
	}

	public IDictionary Headers {
	    get { return headers; }
	    set { headers = value; }
	}
		
	public MailPriority Priority {
	    get { return priority; } 
	    set { priority = value; }
	}
		
	public string Subject {
	    get { return subject; } 
	    set { subject = value; }
	}

	public MailAddressCollection To {
	    get { return to; }   
	    set { to = value; }
	}

	public string UrlContentBase {
	    get { return urlContentBase; } 
	    set { urlContentBase = value; }
	}

	public string UrlContentLocation {
	    get { return urlContentLocation; } 
	    set { urlContentLocation = value; }
	}
    }

}
