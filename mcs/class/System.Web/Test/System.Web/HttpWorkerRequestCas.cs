//
// HttpWorkerRequestCas.cs - CAS unit tests for System.Web.HttpWorkerRequest
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
using System.Collections;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Web;
using System.Web.Caching;

namespace MonoCasTests.System.Web {

	class CasHttpWorkerRequest : HttpWorkerRequest {

		public override void EndOfRequest ()
		{
		}

		public override void FlushResponse (bool finalFlush)
		{
		}

		public override string GetHttpVerbName ()
		{
			return null;
		}

		public override string GetHttpVersion ()
		{
			return null;
		}

		public override string GetLocalAddress ()
		{
			return null;
		}

		public override int GetLocalPort ()
		{
			return 0;
		}

		public override string GetQueryString ()
		{
			return null;
		}

		public override string GetRawUrl ()
		{
			return null;
		}

		public override string GetRemoteAddress ()
		{
			return null;
		}

		public override int GetRemotePort ()
		{
			return 0;
		}

		public override string GetUriPath ()
		{
			return null;
		}

		public override void SendKnownResponseHeader (int index, string value)
		{
		}

		public override void SendResponseFromFile (IntPtr handle, long offset, long length)
		{
		}

		public override void SendResponseFromFile (string filename, long offset, long length)
		{
		}

		public override void SendResponseFromMemory (byte[] data, int length)
		{
		}

		public override void SendStatus (int statusCode, string statusDescription)
		{
		}

		public override void SendUnknownResponseHeader (string name, string value)
		{
		}
	}

	[TestFixture]
	[Category ("CAS")]
	public class HttpWorkerRequestCas {

		[SetUp]
		public virtual void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Properties_Deny_Unrestricted ()
		{
			CasHttpWorkerRequest hwr = new CasHttpWorkerRequest ();
			Assert.IsNull (hwr.MachineConfigPath, "MachineConfigPath");
			Assert.IsNull (hwr.MachineInstallDirectory, "MachineInstallDirectory");
#if NET_2_0
			Assert.IsNotNull (hwr.RequestTraceIdentifier, "RequestTraceIdentifier");
			Assert.IsNull (hwr.RootWebConfigPath, "RootWebConfigPath");
#endif
		}

		private void Callback (HttpWorkerRequest wr, object extraData)
		{
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Methods_Deny_Unrestricted ()
		{
			CasHttpWorkerRequest hwr = new CasHttpWorkerRequest ();
			hwr.CloseConnection ();
			Assert.IsNull (hwr.GetAppPath (), "GetAppPath");
			Assert.IsNull (hwr.GetAppPathTranslated (), "GetAppPathTranslated");
			Assert.IsNull (hwr.GetAppPoolID (), "GetAppPoolID");
			Assert.AreEqual (0, hwr.GetBytesRead (), "GetBytesRead");
			Assert.IsNull (hwr.GetFilePath (), "GetFilePath");
			Assert.IsNull (hwr.GetFilePathTranslated (), "GetGetFilePathTranslated");
			Assert.IsNull (hwr.GetKnownRequestHeader (0), "GetKnownRequestHeader");
			Assert.AreEqual (String.Empty, hwr.GetPathInfo (), "GetPathInfo");
			Assert.IsNull (hwr.GetPreloadedEntityBody (), "GetPreloadedEntityBody");
			Assert.AreEqual ("http", hwr.GetProtocol (), "GetProtocol");
			Assert.IsNull (hwr.GetQueryStringRawBytes (), "GetQueryStringRawBytes");
			Assert.AreEqual (0, hwr.GetRequestReason (), "GetRequestReason");
			Assert.IsNull (hwr.GetServerVariable (null), "GetServerVariable");
			Assert.IsNull (hwr.GetUnknownRequestHeader (null), "GetUnknownRequestHeader");
			Assert.IsNull (hwr.GetUnknownRequestHeaders (), "GetUnknownRequestHeaders");
			Assert.AreEqual (IntPtr.Zero, hwr.GetUserToken (), "GetUserToken");
			Assert.IsFalse (hwr.HasEntityBody (), "HasEntityBody");
			Assert.IsTrue (hwr.HeadersSent (), "HeadersSent");
			Assert.IsTrue (hwr.IsClientConnected (), "IsClientConnected");
			Assert.IsFalse (hwr.IsEntireEntityBodyIsPreloaded (), "IsEntireEntityBodyIsPreloaded");
			Assert.IsFalse (hwr.IsSecure (), "IsSecure");
			Assert.IsNull (hwr.MapPath (null), "MapPath");

			try {
				Assert.AreEqual (0, hwr.ReadEntityBody (new byte[1], 1), "ReadEntityBody(byte[],int)");
			}
			catch (NotImplementedException) {
				// mono
			}

			try {
				hwr.SendCalculatedContentLength (0);
			}
			catch (NotImplementedException) {
				// mono
			}

			hwr.SendResponseFromMemory (IntPtr.Zero, 0);
			hwr.SetEndOfSendNotification (new HttpWorkerRequest.EndOfSendNotification (Callback), null);

			Assert.IsNotNull (hwr.GetClientCertificate (), "GetClientCertificate");
			Assert.IsNotNull (hwr.GetClientCertificateBinaryIssuer (), "GetClientCertificateBinaryIssuer");
			Assert.AreEqual (0, hwr.GetClientCertificateEncoding (), "GetClientCertificateEncoding");
			Assert.IsNotNull (hwr.GetClientCertificatePublicKey (), "GetClientCertificatePublicKey");
			DateTime dt = DateTime.Now.AddMinutes (1);
			Assert.IsTrue (hwr.GetClientCertificateValidFrom () < dt, "GetClientCertificateValidFrom");
			Assert.IsTrue (hwr.GetClientCertificateValidUntil () < dt, "GetClientCertificateValidUntil");
			Assert.AreEqual (0, hwr.GetConnectionID (), "GetConnectionID");
			Assert.AreEqual (0, hwr.GetUrlContextID (), "GetUrlContextID");
			Assert.AreEqual (IntPtr.Zero, hwr.GetVirtualPathToken (), "GetVirtualPathToken");
#if NET_2_0
			Assert.AreEqual (0, hwr.GetPreloadedEntityBody (new byte[0], 0), "GetPreloadedEntityBody(byte[],int)");
			Assert.AreEqual (0, hwr.GetPreloadedEntityBodyLength (), "GetPreloadedEntityBodyLength");
			Assert.AreEqual (0, hwr.GetTotalEntityBodyLength (), "GetTotalEntityBodyLength");
			Assert.AreEqual (0, hwr.ReadEntityBody (new byte[1], 0, 1), "ReadEntityBody(byte[],int,int)");
#endif
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void StaticMethods_Deny_Unrestricted ()
		{
			Assert.AreEqual (-1, HttpWorkerRequest.GetKnownRequestHeaderIndex ("mono"), "GetKnownRequestHeaderIndex");
			Assert.AreEqual ("Cache-Control", HttpWorkerRequest.GetKnownRequestHeaderName (0), "GetKnownRequestHeaderName");
			Assert.AreEqual (-1, HttpWorkerRequest.GetKnownResponseHeaderIndex ("mono"), "GetKnownResponseHeaderIndex");
			Assert.AreEqual ("Cache-Control", HttpWorkerRequest.GetKnownResponseHeaderName (0), "GetKnownResponseHeaderName");
			Assert.AreEqual ("OK", HttpWorkerRequest.GetStatusDescription (200), "GetStatusDescription");
		}
	}
}
