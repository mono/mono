//
// HttpResponseCas.cs - CAS unit tests for System.Web.HttpResponse
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

	[TestFixture]
	[Category ("CAS")]
	public class HttpResponseCas : AspNetHostingMinimal {

		private StringWriter writer;
		private String fname;
		private FileStream fs;
		private IntPtr handle;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			// running at full-trust
			writer = new StringWriter ();
		}

		[SetUp]
		public override void SetUp ()
		{
			// running at full-trust too
			base.SetUp ();

			fname = Path.GetTempFileName ();
			fs = new FileStream (fname, FileMode.Open, FileAccess.Read);
			handle = fs.Handle;
		}

		[TearDown]
		public void TearDown ()
		{
			try {
				if (fs != null)
					fs.Close ();
				handle = IntPtr.Zero;
				if (File.Exists (fname))
					File.Delete (fname);
			}
			catch {
			}
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Properties_Deny_Unrestricted ()
		{
			HttpResponse response = new HttpResponse (writer);

			response.Buffer = false;			
			Assert.IsFalse (response.Buffer, "Buffer");

			response.BufferOutput = false;
			Assert.IsFalse (response.BufferOutput, "BufferOutput");

			Assert.IsNotNull (response.Cache, "Cache");

			response.CacheControl = "public";
			Assert.AreEqual ("public", response.CacheControl, "CacheControl");

			response.ContentEncoding = Encoding.UTF8;
			Assert.AreEqual (Encoding.UTF8, response.ContentEncoding, "ContentEncoding");

			response.ContentType = String.Empty;
			Assert.AreEqual (String.Empty, response.ContentType, "ContentType");

			response.Charset = Encoding.UTF8.WebName;
			Assert.AreEqual (Encoding.UTF8.WebName, response.Charset, "Charset");

			Assert.IsNotNull (response.Cookies, "Cookies");

			try {
				response.Expires = 2;
			}
			catch (NullReferenceException) {
				// ms
			}
			Assert.IsTrue (response.Expires > 0, "Expires");

			response.ExpiresAbsolute = DateTime.MinValue;
			Assert.AreEqual (DateTime.MinValue, response.ExpiresAbsolute, "ExpiresAbsolute");

			Assert.IsTrue (response.IsClientConnected, "IsClientConnected");
			Assert.IsNotNull (response.Output, "Ouput");

			response.RedirectLocation = String.Empty;
			Assert.AreEqual (String.Empty, response.RedirectLocation, "RedirectLocation");

			response.Status = "501 Not Ok";
			Assert.AreEqual ("501 Not Ok", response.Status, "Status");

			response.StatusCode = 501;
			Assert.AreEqual (501, response.StatusCode, "StatusCode");

			response.StatusDescription = "Not Ok";
			Assert.AreEqual ("Not Ok", response.StatusDescription, "StatusDescription");

			response.SuppressContent = false;
			Assert.IsFalse (response.SuppressContent, "SuppressContent");
#if NET_2_0
			response.HeaderEncoding = Encoding.UTF8;
			Assert.AreEqual (Encoding.UTF8, response.HeaderEncoding, "HeaderEncoding");

			Assert.IsFalse (response.IsRequestBeingRedirected, "IsRequestBeingRedirected");
#endif
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
#if ONLY_1_1
		[Category ("NotDotNet")] // triggers a TypeInitializationException in HttpRuntime
#endif
		public void Filter_Deny_Unrestricted ()
		{
			HttpResponse response = new HttpResponse (writer);
			try {
				response.Filter = new MemoryStream ();
			}
			catch (HttpException) {
				// ms
			}

			Assert.IsNull (response.Filter, "Filter");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
#if ONLY_1_1
		[Category ("NotDotNet")] // triggers a TypeInitializationException in HttpRuntime
#endif
		public void OutputStream_Deny_Unrestricted ()
		{
			HttpResponse response = new HttpResponse (writer);
			try {
				Assert.IsNotNull (response.OutputStream, "OutputStream");
			}
			catch (HttpException) {
				// ms 2.0
			}
		}

		private string Callback (HttpContext context)
		{
			return string.Empty;
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Methods_Deny_Unrestricted ()
		{
			HttpResponse response = new HttpResponse (writer);
			response.AddCacheItemDependencies (new ArrayList ());
			response.AddCacheItemDependency (String.Empty);
			response.AddFileDependencies (new ArrayList ());
			response.AddFileDependency (fname);
#if NET_2_0
			response.AddCacheDependency (new CacheDependency[0]);
			response.AddCacheItemDependencies (new string [0]);
			response.AddFileDependencies (new string [0]);
#endif

			try {
				response.AppendCookie (new HttpCookie ("mono"));
			}
			catch (NullReferenceException) {
				// ms 
			}

			try {
				Assert.IsNull (response.ApplyAppPathModifier (null), "ApplyAppPathModifier");
			}
			catch (NullReferenceException) {
				// ms 
			}

			try {
				response.Clear ();
			}
			catch (NullReferenceException) {
				// ms 
			}
		
			try {
				response.ClearContent ();
			}
			catch (NullReferenceException) {
				// ms 
			}
		
			try {
				response.ClearHeaders ();
			}
			catch (NullReferenceException) {
				// ms 
			}

			try {
				response.Redirect ("http://www.mono-project.com");
			}
			catch (NullReferenceException) {
				// ms 
			}
			try {
				response.Redirect ("http://www.mono-project.com", false);
			}
			catch (NullReferenceException) {
				// ms 
			}

			try {
				response.SetCookie (new HttpCookie ("mono"));
			}
			catch (NullReferenceException) {
				// ms 
			}

			response.Write (String.Empty);
			response.Write (Char.MinValue);
			response.Write (new char[0], 0, 0);
			response.Write (this);
#if NET_2_0
			response.WriteSubstitution (new HttpResponseSubstitutionCallback (Callback));
#endif

			response.Flush ();

			response.Close ();

			try {
				response.End ();
			}
			catch (NullReferenceException) {
				// ms 
			}
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
#if ONLY_1_1
		[Category ("NotDotNet")] // triggers a TypeInitializationException in HttpRuntime
#endif
		public void AppendHeader_Deny_Unrestricted ()
		{
			HttpResponse response = new HttpResponse (writer);
			response.AppendHeader ("monkey", "mono");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
#if ONLY_1_1
		[Category ("NotDotNet")] // triggers a TypeInitializationException in HttpRuntime
#endif
		public void AddHeader_Deny_Unrestricted ()
		{
			HttpResponse response = new HttpResponse (writer);
			try {
				response.AddHeader (String.Empty, String.Empty);
			}
			catch (HttpException) {
				// ms 2.0
			}
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
#if ONLY_1_1
		[Category ("NotDotNet")] // triggers a TypeInitializationException in HttpRuntime
#endif
		public void BinaryWrite_Deny_Unrestricted ()
		{
			HttpResponse response = new HttpResponse (writer);
			try {
				response.BinaryWrite (new byte[0]);
			}
			catch (HttpException) {
				// ms 
			}
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
#if ONLY_1_1
		[Category ("NotDotNet")] // triggers a TypeInitializationException in HttpRuntime
#endif
		public void Pics_Deny_Unrestricted ()
		{
			HttpResponse response = new HttpResponse (writer);
			try {
				response.Pics (String.Empty);
			}
			catch (HttpException) {
				// ms 
			}
		}

		[Test]
		[AspNetHostingPermission (SecurityAction.Deny, Level = AspNetHostingPermissionLevel.Medium)]
		[ExpectedException (typeof (SecurityException))]
		public void AppendToLog_Deny_Medium ()
		{
			HttpResponse response = new HttpResponse (writer);
			response.AppendToLog ("mono");
		}

		[Test]
		[AspNetHostingPermission (SecurityAction.PermitOnly, Level = AspNetHostingPermissionLevel.Medium)]
		public void AppendToLog_PermitOnly_Medium ()
		{
			HttpResponse response = new HttpResponse (writer);
			response.AppendToLog ("mono");
		}

		[Test]
		[FileIOPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void TransmitFile_Deny_FileIOPermission ()
		{
			HttpResponse response = new HttpResponse (writer);
			response.TransmitFile (fname);
		}

		[Test]
		[FileIOPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		public void TransmitFile_PermitOnly_FileIOPermission ()
		{
			HttpResponse response = new HttpResponse (writer);
			response.TransmitFile (fname);
		}

		[Test]
		[FileIOPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void WriteFile_String_Deny_FileIOPermission ()
		{
			HttpResponse response = new HttpResponse (writer);
			response.WriteFile (fname);
		}

		[Test]
		[FileIOPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void WriteFile_StringBool_Deny_FileIOPermission ()
		{
			HttpResponse response = new HttpResponse (writer);
			response.WriteFile (fname, false);
		}

		[Test]
		[FileIOPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void WriteFile_StringIntInt_Deny_FileIOPermission ()
		{
			HttpResponse response = new HttpResponse (writer);
			response.WriteFile (fname, 0, 1);
		}

		[Test]
		[FileIOPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		public void WriteFile_PermitOnly_FileIOPermission ()
		{
			HttpResponse response = new HttpResponse (writer);
			response.WriteFile (fname);
			response.WriteFile (fname, false);
			response.WriteFile (fname, 0, 0);
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[ExpectedException (typeof (SecurityException))]
		public void WriteFile_Deny_UnmanagedCode ()
		{
			HttpResponse response = new HttpResponse (writer);
			response.WriteFile (handle, 0, 1);
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, UnmanagedCode = true)]
		public void WriteFile_PermitOnly_UnmanagedCode ()
		{
			HttpResponse response = new HttpResponse (writer);
			response.WriteFile (handle, 0, 1);
		}

		// LinkDemand

		public override object CreateControl (SecurityAction action, AspNetHostingPermissionLevel level)
		{
			ConstructorInfo ci = this.Type.GetConstructor (new Type[1] { typeof (TextWriter) });
			Assert.IsNotNull (ci, ".ctor(TextWriter)");
			return ci.Invoke (new object[1] { writer });
		}

		public override Type Type {
			get { return typeof (HttpResponse); }
		}
	}
}
