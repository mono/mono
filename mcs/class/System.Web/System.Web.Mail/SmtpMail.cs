//
// System.Web.Mail.SmtpMail.cs
//
// Author:
//    Lawrence Pit (loz@cable.a2000.nl)
//    Per Arneng (pt99par@student.bth.se) (SmtpMail.Send)
//

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
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
			throw new HttpException (ex.Message);
		    
		    } catch (IOException ex) {
			
			throw new HttpException (ex.Message);
			
		    } catch (FormatException ex) {
			
			throw new HttpException (ex.Message);
		    
		    } catch (SocketException ex) {
			
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
