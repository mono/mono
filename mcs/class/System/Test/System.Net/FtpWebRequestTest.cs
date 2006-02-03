//
// FtpWebRequestTest.cs - NUnit Test Cases for System.Net.FtpWebRequest
//
// Author: Carlos Alberto Cortez <calberto.cortez@gmail.com>
//
// Copyright (c) 2006 Novell, Inc. (http://www.novell.com)
//

using NUnit.Framework;
using System;
using System.Net;

#if NET_2_0

namespace MonoTests.System.Net 
{
	[TestFixture]
	public class FtpWebRequestTest
	{
		FtpWebRequest defaultRequest;
		
		[TestFixtureSetUp]
		public void Init ()
		{
			defaultRequest = (FtpWebRequest) WebRequest.Create ("ftp://www.contoso.com");
		}
		
		[Test]
		public void ContentLength ()
		{
			try {
				long l = defaultRequest.ContentLength;
			} catch (NotSupportedException) {
				Assert.Fail ("#1"); // Not overriden
			}

			try {
				defaultRequest.ContentLength = 2;
			} catch (NotSupportedException) {
				Assert.Fail ("#2"); // Not overriden
			}
		}

		[Test]
		public void ContentType ()
		{
			try {
				string t = defaultRequest.ContentType;
				Assert.Fail ("#1");
			} catch (NotSupportedException) {
			}

			try {
				defaultRequest.ContentType = String.Empty;
				Assert.Fail ("#2");
			} catch (NotSupportedException) {
			}
		}

		[Test]
		public void ContentOffset ()
		{
			try {
				defaultRequest.ContentOffset = -2;
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException) {
			}
		}

		[Test]
		public void Credentials ()
		{
			try {
				defaultRequest.Credentials = null;
				Assert.Fail ("#1");
			} catch (ArgumentNullException) {
			}

		}

		[Test]
		public void PreAuthenticate ()
		{
			try {
				bool p = defaultRequest.PreAuthenticate;
				Assert.Fail ("#1");
			} catch (NotSupportedException) {
			}

			try {
				defaultRequest.PreAuthenticate = true;
			} catch (NotSupportedException) {
			}
		}

		[Test]
		public void ReadWriteTimeout ()
		{
			try {
				defaultRequest.ReadWriteTimeout = -2;
				Assert.Fail ("#2");
			} catch (ArgumentOutOfRangeException) {
			}
		}

		[Test]
		public void Timeout ()
		{
			try {
				defaultRequest.Timeout = -2;
				Assert.Fail ("#2");
			} catch (ArgumentOutOfRangeException) {
			}
		}
		
		[Test]
		public void DefaultValues ()
		{
			FtpWebRequest request = (FtpWebRequest) WebRequest.Create ("ftp://www.contoso.com");
			
			Assert.AreEqual (0, request.ContentOffset, "ContentOffset");
			Assert.AreEqual (false, request.EnableSsl, "EnableSsl");
			Assert.AreEqual (true, request.KeepAlive, "KeepAlive");
			Assert.AreEqual (WebRequestMethods.Ftp.DownloadFile, request.Method, "#1");
			Assert.AreEqual (300000, request.ReadWriteTimeout, "ReadWriteTimeout");
			Assert.IsNull (request.RenameTo, "RenameTo");
			Assert.AreEqual (true, request.UseBinary, "UseBinary");
			Assert.AreEqual (100000, request.Timeout, "Timeout");
			Assert.AreEqual (true, request.UsePassive, "UsePassive");
		}

		[Test]
		public void RenameTo ()
		{
			try {
				defaultRequest.RenameTo = null;
				Assert.Fail ("#1");
			} catch (ArgumentException) {
			}

			try {
				defaultRequest.RenameTo = String.Empty;
				Assert.Fail ("#2");
			} catch (ArgumentException) {
			}
		}

	}
}

#endif

