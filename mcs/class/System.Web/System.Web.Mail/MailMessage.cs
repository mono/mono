//
// System.Web.Mail.MailMessage.cs
//
// Author:
//    Lawrence Pit (loz@cable.a2000.nl)
//    Per Arneng (pt99par@student.bth.se)
//	Sanjay Gupta (gsanjay@novell.com)
//
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

using System.Collections;
using System.Collections.Specialized;
using System.Security.Permissions;
using System.Text;

namespace System.Web.Mail
{
#if !NET_4_0
	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
	[Obsolete ("The recommended alternative is System.Net.Mail.MailMessage. http://go.microsoft.com/fwlink/?linkid=14202")]
	public class MailMessage
	{
		ArrayList attachments;
		string bcc;
		string body = String.Empty;
		Encoding bodyEncoding;
		MailFormat bodyFormat;
		string cc;		
		string from;
		ListDictionary headers;
		MailPriority priority;
		string subject = String.Empty;
		string to;
		string urlContentBase;
		string urlContentLocation;
		
		// Constructor		
		public MailMessage ()
		{
			attachments = new ArrayList (8);
			headers = new ListDictionary ();
			bodyEncoding = Encoding.Default;
#if NET_1_1
			fields = new Hashtable ();
#endif
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

#if NET_1_1
		Hashtable fields;
		
		public IDictionary Fields {
			get {
				return (IDictionary) fields;
			}
		}
#endif
	}
}
