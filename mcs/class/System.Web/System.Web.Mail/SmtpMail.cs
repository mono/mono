//
// System.Web.Mail.SmtpMail.cs
//
// Author:
//    Lawrence Pit (loz@cable.a2000.nl)
//    Per Arneng (pt99par@student.bth.se) (SmtpMail.Send)
//

using System;
using System.Net;
using System.IO;
using System.Reflection;

namespace System.Web.Mail
{
	/// <remarks>
	/// </remarks>
	public class SmtpMail
	{
		private static string smtpServer;
		
		// Constructor		
		private SmtpMail ()
		{
			/* empty */
		}		

		// Properties
		public static string SmtpServer {
			get { return smtpServer; } 
			set { smtpServer = value; }
		}
		
		
		public static void Send (MailMessage message) 
		{
		 
		    try {
			
			SmtpClient smtp = new SmtpClient (smtpServer);
			
			smtp.Send (message);
			
			smtp.Close ();
		    
		    } catch (SmtpException ex) {
			// MS implementation throws HttpException
			// so it is just to follow
			throw new HttpException (ex.Message);
		    
		    } catch (IOException ex) {
			
			throw new HttpException (ex.Message);
			
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
	
} //namespace System.Web.Mail
