//
// HttpRequestCas.cs - CAS unit tests for System.Web.HttpRequest
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

namespace MonoCasTests.System.Web {

	[TestFixture]
	[Category ("CAS")]
	public class HttpRequestCas : AspNetHostingMinimal {

		private HttpRequest request;
		private string tempfile;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			request = new HttpRequest (String.Empty, "http://localhost/", String.Empty);
			tempfile = Path.GetTempFileName ();
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Properties_Deny_Unrestricted ()
		{
			Assert.IsNull (request.AcceptTypes, "AcceptTypes");
			Assert.IsNull (request.ApplicationPath, "ApplicationPath");
			request.Browser = null;
			try {
				Assert.IsNotNull (request.Browser, "Browser");
			}
			catch (NullReferenceException) {
				// ms
			}

			request.ContentEncoding = null;
			try {
				Assert.IsNull (request.ContentEncoding, "ContentEncoding");
			}
			catch (NullReferenceException) {
				// ms
			}
			catch (HttpException) {
				// mono
			}

			Assert.AreEqual (0, request.ContentLength, "ContentLength");
			Assert.AreEqual (String.Empty, request.ContentType, "ContentType");
			request.ContentType = null;
			Assert.IsNotNull (request.Cookies, "Cookies");

			try {
				Assert.AreEqual ("/", request.CurrentExecutionFilePath, "CurrentExecutionFilePath");
			}
			catch (NullReferenceException) {
				// ms 1.x
			}

			try {
				Assert.AreEqual ("/", request.FilePath, "FilePath");
			}
			catch (NullReferenceException) {
				// ms 1.x
			}

			Assert.IsNotNull (request.Files, "Files");

			Assert.IsNotNull (request.Filter, "Filter");
			request.Filter = null;

			Assert.IsNotNull (request.Form, "Form");
			Assert.IsNotNull (request.Headers, "Headers");
			Assert.AreEqual ("GET", request.HttpMethod, "HttpMethod");
			Assert.IsNotNull (request.InputStream, "InputStream");
			Assert.IsFalse (request.IsSecureConnection, "IsSecureConnection");
			Assert.IsNotNull (request.Path, "Path");

			try {
				Assert.IsNotNull (request.PathInfo, "PathInfo");
			}
			catch (NullReferenceException) {
				// ms 1.x
			}

			Assert.IsNotNull (request.QueryString, "QueryString");
			Assert.IsNotNull (request.RawUrl, "RawUrl");
			Assert.AreEqual ("GET", request.RequestType, "RequestType");
			request.RequestType = null;
			Assert.AreEqual (0, request.TotalBytes, "TotalBytes");
			Assert.IsNotNull (request.Url, "Url");
			Assert.IsNull (request.UrlReferrer, "UrlReferrer");
			Assert.IsNull (request.UserAgent, "UserAgent");
			Assert.IsNull (request.UserHostAddress, "UserHostAddress");
			Assert.IsNull (request.UserHostName, "UserHostName");
			Assert.IsNull (request.UserLanguages, "UserLanguages");
#if NET_2_0
			Assert.IsFalse (request.IsLocal, "IsLocal");
#endif
		}

#if NET_2_0
		[Test]
		[AspNetHostingPermission (SecurityAction.Deny, Level = AspNetHostingPermissionLevel.Low)]
		[ExpectedException (typeof (SecurityException))]
		public void ClientCertificate_Deny_Low ()
		{
			Assert.IsNotNull (request.ClientCertificate, "ClientCertificate");
		}

		[Test]
		[AspNetHostingPermission (SecurityAction.PermitOnly, Level = AspNetHostingPermissionLevel.Low)]
		[ExpectedException (typeof (NullReferenceException))]
		public void ClientCertificate_PermitOnly_Low ()
		{
			Assert.IsNotNull (request.ClientCertificate, "ClientCertificate");
		}
#else
		// ClientCertificate fails before hitting the SecurityException
#endif

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (NullReferenceException))]
		public void IsAuthenticated_Deny_Unrestricted ()
		{
			Assert.IsNull (request.IsAuthenticated, "IsAuthenticated");
		}

		[Test]
		[AspNetHostingPermission (SecurityAction.Deny, Level = AspNetHostingPermissionLevel.Low)]
		[ExpectedException (typeof (SecurityException))]
		public void Params_Deny_Low ()
		{
			Assert.IsNotNull (request.Params, "Params");
		}

		[Test]
		[AspNetHostingPermission (SecurityAction.PermitOnly, Level = AspNetHostingPermissionLevel.Low)]
		public void Params_PermitOnly_Low ()
		{
			Assert.IsNotNull (request.Params, "Params");
		}

		[Test]
		[AspNetHostingPermission (SecurityAction.Deny, Level = AspNetHostingPermissionLevel.Low)]
		[ExpectedException (typeof (SecurityException))]
		public void ServerVariables_Deny_Low ()
		{
			Assert.IsNotNull (request.ServerVariables, "ServerVariables");
		}

		[Test]
		[AspNetHostingPermission (SecurityAction.PermitOnly, Level = AspNetHostingPermissionLevel.Low)]
		public void ServerVariables_PermitOnly_Low ()
		{
			Assert.IsNotNull (request.ServerVariables, "ServerVariables");
			Assert.IsNull (request["mono"], "this[string]");
		}

		[Test]
		[AspNetHostingPermission (SecurityAction.Deny, Level = AspNetHostingPermissionLevel.Low)]
		[ExpectedException (typeof (SecurityException))]
		public void This_Deny_Low ()
		{
			Assert.IsNull (request["mono"], "this[string]");
		}

		[Test]
		[FileIOPermission (SecurityAction.Deny, Unrestricted = true)]
		// default path is null [ExpectedException (typeof (SecurityException))]
		public void PhysicalApplicationPath_Deny_FileIOPermission ()
		{
			try {
				Assert.IsNull (request.PhysicalApplicationPath, "PhysicalApplicationPath");
			}
			catch (ArgumentNullException) {
				// ms 2.0
			}
			catch (TypeInitializationException) {
				// ms 1.x
			}
		}

		[Test]
		[FileIOPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		// default path is null and mess up the security check
		public void PhysicalApplicationPath_PermitOnly_FileIOPermission ()
		{
			try {
				Assert.IsNull (request.PhysicalApplicationPath, "PhysicalApplicationPath");
			}
			catch (ArgumentNullException) {
				// ms 2.0
			}
			catch (TypeInitializationException) {
				// ms 1.x
			}
		}

		[Test]
		[FileIOPermission (SecurityAction.Deny, Unrestricted = true)]
		//[ExpectedException (typeof (ArgumentException))]
		public void PhysicalPath_Deny_FileIOPermission ()
		{
			// strange - must be a special case not to check (and fail) a 
			// FileIOPermission check (strangeest part being that this isn't
			// done for PhysicalApplicationPath)
			Assert.AreEqual (String.Empty, request.PhysicalPath, "PhysicalPath");
		}

		[Test]
		[FileIOPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		public void PhysicalPath_PermitOnly_FileIOPermission ()
		{
			Assert.IsNotNull (request.PhysicalPath, "PhysicalPath");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Methods_Deny_Unrestricted ()
		{
			Assert.IsNotNull (request.BinaryRead (0), "BinaryRead");
			Assert.IsNull (request.MapImageCoordinates ("mono"), "MapImageCoordinates");

			try {
				Assert.IsNull (request.MapPath ("/mono"), "MapPath");
			}
			catch (NullReferenceException) {
				// ms 1.x fails
			}

			try {
				request.MapPath ("/mono", "/", true);
			}
			catch (HttpException) {
				// ms 2.0
			}
			catch (TypeInitializationException) {
				// ms 1.0
			}

			request.ValidateInput ();
		}

		[Test]
		[FileIOPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void SaveAs_Deny_Write ()
		{
			request.SaveAs (tempfile, true);
		}

		[Test]
		[FileIOPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		public void SaveAs_PermitOnly_Write ()
		{
			request.SaveAs (tempfile, true);
		}

		// LinkDemand

		public override object CreateControl (SecurityAction action, AspNetHostingPermissionLevel level)
		{
			ConstructorInfo ci = this.Type.GetConstructor (new Type[3] { typeof (string), typeof (string), typeof (string) });
			Assert.IsNotNull (ci, ".ctor(string,string,string)");
			return ci.Invoke (new object[3] { String.Empty, "http://localhost/", String.Empty });
		}

		public override Type Type {
			get { return typeof (HttpRequest); }
		}
	}
}
