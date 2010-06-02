//
// System.Web.Mail.SmtpMail.cs
//
// Author:
//    Lawrence Pit (loz@cable.a2000.nl)
//    Per Arneng (pt99par@student.bth.se) (SmtpMail.Send)
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

using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Reflection;
using System.Security.Permissions;

namespace System.Web.Mail
{
	// CAS
	[Obsolete ("The recommended alternative is System.Net.Mail.SmtpClient. http://go.microsoft.com/fwlink/?linkid=14202")]
#if !NET_4_0
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
    	public class SmtpMail
	{
		static string smtpServer = "localhost";
		
		// Constructor
		SmtpMail ()
		{
			/* empty */
		}

		// Properties
		public static string SmtpServer {
			get { return smtpServer; } 
			set { smtpServer = value; }
		}
		
		// Medium (not Minimal) here
		// http://msdn.microsoft.com/library/en-us/dnpag2/html/paght000017.asp
		[AspNetHostingPermission (SecurityAction.Demand, Level = AspNetHostingPermissionLevel.Medium)]
		public static void Send (MailMessage message) 
		{
		   		   		    
		    try {
			
			// wrap the MailMessage in a MailMessage wrapper for easier
			// access to properties and to add some functionality
			MailMessageWrapper messageWrapper = new MailMessageWrapper( message );
			
			SmtpClient smtp = new SmtpClient (smtpServer);
			
			smtp.Send (messageWrapper);
		       
			smtp.Close ();
		    
		    } catch (SmtpException ex) {
			// LAMESPEC:
			// .NET sdk throws HttpException
			// for some reason so to be compatible
			// we have to do it to :(
			throw new HttpException (ex.Message, ex);
		    
		    } catch (IOException ex) {
			
			throw new HttpException (ex.Message, ex);
			
		    } catch (FormatException ex) {
			
			throw new HttpException (ex.Message, ex);
		    
		    } catch (SocketException ex) {
			
			throw new HttpException (ex.Message, ex);
			
		    }
		    
		}
		
		public static void Send (string from, string to, string subject, string messageText) 
		{
			MailMessage message = new MailMessage ();
			message.From = from;
			message.To = to;
			message.Subject = subject;
			message.Body = messageText;
			Send (message);
		}
	}
}
