//
// BadImageFormatExceptionCas.cs -
//	CAS unit tests for System.BadImageFormatException
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
using System.Security;
using System.Security.Permissions;

namespace MonoCasTests.System {

	[TestFixture]
	[Category ("CAS")]
	public class BadImageFormatExceptionCas {

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}


		[Test]
		public void NoRestriction ()
		{
			BadImageFormatException fle = new BadImageFormatException ("message", "filename",
				new BadImageFormatException ("inner message", "inner filename"));

			Assert.AreEqual ("message", fle.Message, "Message");
			Assert.AreEqual ("filename", fle.FileName, "FileName");
			Assert.IsNull (fle.FusionLog, "FusionLog");
			Assert.IsNotNull (fle.ToString (), "ToString");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void FullRestriction ()
		{
			BadImageFormatException fle = new BadImageFormatException ("message", "filename",
				new BadImageFormatException ("inner message", "inner filename"));

			Assert.AreEqual ("message", fle.Message, "Message");
			Assert.AreEqual ("filename", fle.FileName, "FileName");
			Assert.IsNull (fle.FusionLog, "FusionLog");
			// ToString doesn't work in this case and strangely throws a BadImageFormatException
			Assert.IsNotNull (fle.ToString (), "ToString");
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, ControlEvidence = true, ControlPolicy = true)]
		public void GetFusionLog_Pass ()
		{
			BadImageFormatException fle = new BadImageFormatException ("message", "filename");
			Assert.AreEqual ("message", fle.Message, "Message");
			Assert.AreEqual ("filename", fle.FileName, "FileName");
			Assert.IsNull (fle.FusionLog, "FusionLog");
			// note: ToString doesn't work in this case
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlEvidence = true)]
		[ExpectedException (typeof (SecurityException))]
		public void GetFusionLog_Fail_ControlEvidence ()
		{
			BadImageFormatException fle = new BadImageFormatException ();
			Assert.IsNull (fle.FusionLog, "FusionLog");
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlPolicy = true)]
		[ExpectedException (typeof (SecurityException))]
		public void GetFusionLog_Fail_ControlPolicy ()
		{
			BadImageFormatException fle = new BadImageFormatException ();
			Assert.IsNull (fle.FusionLog, "FusionLog");
			// we don't have to throw the exception to have FusionLog
			// informations restricted (even if there could be no
			// data in this state).
		}


		[Test]
		public void Throw_NoRestriction ()
		{
			try {
				throw new BadImageFormatException ("message", "filename",
					new BadImageFormatException ("inner message", "inner filename"));
			}
			catch (BadImageFormatException fle) {
				Assert.AreEqual ("message", fle.Message, "Message");
				Assert.AreEqual ("filename", fle.FileName, "FileName");
				Assert.IsNull (fle.FusionLog, "FusionLog");
				Assert.IsNotNull (fle.ToString (), "ToString");
			}
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Throw_FullRestriction ()
		{
			try {
				throw new BadImageFormatException ("message", "filename",
					new BadImageFormatException ("inner message", "inner filename"));
			}
			catch (BadImageFormatException fle) {
				Assert.AreEqual ("message", fle.Message, "Message");
				Assert.AreEqual ("filename", fle.FileName, "FileName");
				// both FusionLog and ToString doesn't work in this case
				// but strangely we get a BadImageFormatException
				Assert.IsNull (fle.FusionLog, "FusionLog");
				Assert.IsNotNull (fle.ToString (), "ToString");
			}
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlEvidence = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Throw_GetFusionLog_Fail_ControlEvidence ()
		{
			try {
				throw new BadImageFormatException ("message", "filename",
					new BadImageFormatException ("inner message", "inner filename"));
			}
			catch (BadImageFormatException fle) {
				Assert.IsNull (fle.FusionLog, "FusionLog");
			}
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlPolicy = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Throw_GetFusionLog_Fail_ControlPolicy ()
		{
			try {
				throw new BadImageFormatException ("message", "filename",
					new BadImageFormatException ("inner message", "inner filename"));
			}
			catch (BadImageFormatException fle) {
				Assert.IsNull (fle.FusionLog, "FusionLog");
			}
		}
	}
}
