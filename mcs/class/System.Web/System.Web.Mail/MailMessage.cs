//
// System.Web.Mail.MailMessage.cs
//
// Author:
//    Lawrence Pit (loz@cable.a2000.nl)
//    Per Arneng (pt99par@student.bth.se)
//

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;

namespace System.Web.Mail
{
	/// <remarks>
	/// </remarks>
	public class MailMessage
	{
		private ArrayList attachments;
		private string bcc;
		private string body;
		private Encoding bodyEncoding;
		private MailFormat bodyFormat;
		private string cc;		
		private string from;
		private ListDictionary headers;
		private MailPriority priority;
		private string subject;
		private string to;
		private string urlContentBase;
		private string urlContentLocation;
		
		// Constructor		
		public MailMessage ()
		{
			attachments = new ArrayList (8);
			headers = new ListDictionary ();
			bodyEncoding = Encoding.Default;
		}		
	
		// Properties
		public IList Attachments {
			get { return (IList) attachments; }
		}		
		
		public string Bcc {
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

		public string Cc {
			get { return cc; } 
			set { cc = value; }
		}

		public string From {
			get { return from; } 
			set { from = value; }
		}

		public IDictionary Headers {
			get { return (IDictionary) headers; }
		}
		
		public MailPriority Priority {
			get { return priority; } 
			set { priority = value; }
		}
		
		public string Subject {
			get { return subject; } 
			set { subject = value; }
		}

		public string To {
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
	
} //namespace System.Web.Mail
