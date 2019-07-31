//
// SmtpMailCas.cs - CAS unit tests for System.Web.Mail.SmtpMail
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using NUnit.Framework;

using System;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Web;
using System.Web.Mail;

namespace MonoCasTests.System.Web.Mail {

	[TestFixture]
	[Category ("CAS")]
	public class SmtpMailCas : AspNetHostingMinimal {

		private MailMessage msg;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string fname = Path.GetTempFileName ();
			using (FileStream fs = File.OpenWrite (fname)) {
				fs.WriteByte (0);
				fs.Close ();
			}
			msg = new MailMessage ();
			msg.Attachments.Add (new MailAttachment (fname));
			msg.Bcc = "bcc@localhost.com";
			msg.Body = "Hola!";
			msg.BodyEncoding = Encoding.ASCII;
			msg.Cc = "cc@localhost.com";
			msg.Fields["mono"] = "monkey";
			msg.From = "from@localhost.com";
			msg.Headers["monkey"] = "mono";
			msg.Priority = MailPriority.High;
			msg.Subject = "Monkey business";
			msg.To = "to@localhost.com";
			msg.UrlContentBase = "http://www.example.org";
			msg.UrlContentLocation = "http://www.example.com";
			// ensure the static ctor is called at full trust
			Assert.IsNotNull (SmtpMail.SmtpServer);
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void SmtpServer_Deny_Unrestricted ()
		{
			// get/set
			SmtpMail.SmtpServer = "localhost";
			Assert.AreEqual ("localhost", SmtpMail.SmtpServer, "SmtpServer");
		}

		[Test]
		[AspNetHostingPermission (SecurityAction.Deny, Level = AspNetHostingPermissionLevel.Medium)]
		[ExpectedException (typeof (SecurityException))]
		public void SendMessage_Deny_Medium ()
		{
			SmtpMail.Send (msg);
		}

		[Test]
		[AspNetHostingPermission (SecurityAction.PermitOnly, Level = AspNetHostingPermissionLevel.Medium)]
		public void SendMessage_PermitOnly_Medium ()
		{
			try {
				SmtpMail.Send (msg);
			}
			catch (Exception e) {
				// HttpException: can't sent mail
				// TypeInitializationException: missing stuff in machine.config
				// ConfigurationException: missing stuff in machine.config
				if (e is SecurityException)
					throw e; // that's the one we're looking for
			}
		}

		[Test]
		[AspNetHostingPermission (SecurityAction.Deny, Level = AspNetHostingPermissionLevel.Medium)]
		[ExpectedException (typeof (SecurityException))]
		public void Send_Deny_Medium ()
		{
			SmtpMail.Send (msg.From, msg.To, msg.Subject, msg.Body);
		}

		[Test]
		[AspNetHostingPermission (SecurityAction.PermitOnly, Level = AspNetHostingPermissionLevel.Medium)]
		public void Send_PermitOnly_Medium ()
		{
			try {
				SmtpMail.Send (msg.From, msg.To, msg.Subject, msg.Body);
			}
			catch (Exception e) {
				// HttpException: can't sent mail
				// TypeInitializationException: missing stuff in machine.config
				// ConfigurationException: missing stuff in machine.config
				if (e is SecurityException)
					throw e; // that's the one we're looking for
			}
		}

		// LinkDemand tests

		public override object CreateControl (SecurityAction action, AspNetHostingPermissionLevel level)
		{
			// in this case testing with a (private) ctor isn't very conveniant
			// and the LinkDemand promotion (to Demand) will still occurs on other stuff
			// and (finally) we know that the SmtpServer properties isn't protected ifself
			MethodInfo mi = this.Type.GetProperty ("SmtpServer").GetGetMethod ();
			Assert.IsNotNull (mi, "get");
			return mi.Invoke (null, null);
		}

		public override Type Type {
			get { return typeof (SmtpMail); }
		}
	}
}
