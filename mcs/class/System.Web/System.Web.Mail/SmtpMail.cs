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
		private static string smtpServer = "localhost";
		
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
		    
		    SmtpMessage msg = new SmtpMessage ();
		    
		    try {
			
			if( message.From != null ) msg.From = MailAddress.Parse (message.From);
			if( message.To != null ) msg.To = MailAddressCollection.Parse (message.To);
			if( message.Cc != null ) msg.Cc = MailAddressCollection.Parse (message.Cc);
			if( message.Bcc != null ) msg.Bcc = MailAddressCollection.Parse (message.Bcc);
			
			msg.Headers = message.Headers;
			msg.UrlContentBase = message.UrlContentBase;
			msg.UrlContentLocation = message.UrlContentLocation;
			msg.Priority = message.Priority;
			msg.Subject = message.Subject;
			
			msg.Body = message.Body;
			msg.BodyEncoding = message.BodyEncoding; 
			msg.BodyFormat = message.BodyFormat;
			
			msg.Attachments = message.Attachments;   
						
		    } catch (FormatException ex) {
			throw new HttpException (ex.Message);
		    }
		    
		    
		    try {
			
			SmtpClient smtp = new SmtpClient (smtpServer);
			
			smtp.Send (msg);
			
			smtp.Close ();
		    
		    } catch (SmtpException ex) {
			// LAMESPEC:
			// .NET sdk throws HttpException
			// for some reason so to be compatible
			// we have to do it to :(
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
