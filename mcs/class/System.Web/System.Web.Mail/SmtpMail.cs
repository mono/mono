//
// System.Web.Mail.SmtpMail.cs
//
// Author:
//    Lawrence Pit (loz@cable.a2000.nl)
//
//

using System;
using System.Net;
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
		
		[MonoTODO]
		public static void Send (MailMessage message) 
		{
			// delegate work to loosly coupled component Mono.Mail
			
			// Mono.Mail.Smtp.SmtpSender.Send (smtpServer, message);	
			
			// NOTE: Mono.Mail is work in progress, and could be replaced by 
			// another component. For now:
			
			throw new NotImplementedException("Mono.Mail component is work in progress");

		/*
			try {
				// TODO: possibly cache ctor info object..
				Type stype = Type.GetType ("Mono.Mail.Smtp.SmtpSender");
				if (stype == null) {
					throw new Exception ("You must have Mono.Mail installed to send mail.");
				}
				Type[] types = new Type[2];
				types[0] = typeof (string);
				types[1] = message.GetType ();
				ConstructorInfo cinfo = 
					stype.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null,
						CallingConventions.HasThis, types, null);
				cinfo.Invoke (new object[] {smtpServer, message});
			} catch (Exception) {
				throw new Exception ("Unable to call Mono.Mail.Smtp.SmtpSender");
			}
		*/
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
