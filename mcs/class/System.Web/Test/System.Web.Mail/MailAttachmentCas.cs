//
// MailAttachmentCas.cs - CAS unit tests for System.Web.Mail.MailAttachment
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
using System.Web;
using System.Web.Mail;

namespace MonoCasTests.System.Web.Mail {

	[TestFixture]
	[Category ("CAS")]
	public class MailAttachmentCas : AspNetHostingMinimal {

		private string fname;
		private MailAttachment attachment;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			fname = Path.GetTempFileName ();
			using (FileStream fs = File.OpenWrite (fname)) {
				fs.WriteByte (0);
				fs.Close ();
			}
			attachment = new MailAttachment (fname);
		}

		[Test]
		[FileIOPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Contructor_String_Deny_FileIOPermission ()
		{
			try {
				new MailAttachment (fname);
			}
			catch (TypeInitializationException e) {
				// MS BUG - the original security exception gets wrapped
				// inside a TypeInitializationException.
				Assert.IsNotNull (e.InnerException, "InnerException");
				throw e.InnerException;
			}
			catch (HttpException e) {
				// MS BUG - the original security exception gets replaced
				// by an HttpException. Even worst the SecurityException 
				// is not in the InnerException!
				Assert.IsNull (e.InnerException, "InnerException");
				Assert.Ignore ("2.0 hides the SecurityException with a HttpException");
			}
		}

		[Test]
		[FileIOPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Contructor_StringMailEncoding_Deny_FileIOPermission ()
		{
			try {
				new MailAttachment (fname, MailEncoding.UUEncode);
			}
			catch (TypeInitializationException e) {
				// MS BUG - the original security exception gets wrapped
				// inside a TypeInitializationException.
				Assert.IsNotNull (e.InnerException, "InnerException");
				throw e.InnerException;
			}
			catch (HttpException e) {
				// MS BUG - the original security exception gets replaced
				// by an HttpException. Even worst the SecurityException 
				// is not in the InnerException!
				Assert.IsNull (e.InnerException, "InnerException");
				Assert.Ignore ("2.0 hides the SecurityException with a HttpException");
			}
		}

		[Test]
		[FileIOPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		public void Contructor_PermitOnly_FileIOPermission ()
		{
			new MailAttachment (fname);
			new MailAttachment (fname, MailEncoding.UUEncode);
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Properties ()
		{
			// once created we can get the filename even with no permissions
			Assert.AreEqual (fname, attachment.Filename, "Filename");
			// LAMESPEC: default isn't UUEncode
			Assert.AreEqual (MailEncoding.Base64, attachment.Encoding, "Encoding");
		}

		// LinkDemand tests

		// overriden because
		// (a) there's no empty .ctor in MailAttachment
		// (b) the filename parameter implies some file i/o
		[FileIOPermission (SecurityAction.Assert, Unrestricted = true)]
		public override object CreateControl (SecurityAction action, AspNetHostingPermissionLevel level)
		{
			ConstructorInfo ci = this.Type.GetConstructor (new Type [1] { typeof (string) });
			Assert.IsNotNull (ci, ".ctor(string)");
			return ci.Invoke (new object [1] { fname });
		}
		
		public override Type Type {
			get { return typeof (MailAttachment); }
		}
	}
}
