// MailMessageWrapper.cs
// author: Per Arneng <pt99par@student.bth.se>
using System;
using System.Collections;
using System.Text;

namespace System.Web.Mail {

    internal class MailMessageWrapper {
		
	private MailAddressCollection bcc = new MailAddressCollection();
	private MailAddressCollection cc = new MailAddressCollection();		
	private MailAddress from;
	private MailAddressCollection to = new MailAddressCollection();
	private MailMessage message;
		
	// Constructor		
	public MailMessageWrapper( MailMessage message )
	{
	    this.message = message;
	    
	    if(message.From != null ) from = MailAddress.Parse( message.From );
	    if(message.To != null ) to = MailAddressCollection.Parse( message.To );
	    if(message.Cc != null )cc = MailAddressCollection.Parse( message.Cc );
	    if(message.Bcc != null )bcc = MailAddressCollection.Parse( message.Bcc );
	}		
	
	// Properties
	public IList Attachments {
	    get { return message.Attachments; }
	}		
		
	public MailAddressCollection Bcc {
	    get { return bcc; } 
	}
	
	public string Body {
	    get { return message.Body; } 
	    set { message.Body = value; } 
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

	public IDictionary Headers {
	    get { return message.Headers; }
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
    }

}
