//
// Testcases.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2012 Xamarin Inc. (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Net;
using System.Xml;
using System.Text;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using NUnit.Framework.SyntaxHelpers;

using WS = System.Web.Services.Description;

namespace MonoTests.System.ServiceModel.MetadataTests {

	[TestFixture]
	[Category ("MetadataTests")]
	public partial class ImportTests {

		[Test]
		public void BasicHttp ()
		{
			var doc = TestContext.GetMetadata ("BasicHttp");
			var label = new TestLabel ("BasicHttp");

			BindingTestAssertions.BasicHttpBinding (doc, BasicHttpSecurityMode.None, label);
		}
		
		[Test]
		public void BasicHttp_TransportSecurity ()
		{
			var doc = TestContext.GetMetadata ("BasicHttp_TransportSecurity");
			var label = new TestLabel ("BasicHttp_TransportSecurity");

			BindingTestAssertions.BasicHttpBinding (doc, BasicHttpSecurityMode.Transport, label);
		}
		
		[Test]
		[Category ("NotWorking")]
		public void BasicHttp_MessageSecurity ()
		{
			var doc = TestContext.GetMetadata ("BasicHttp_MessageSecurity");
			var label = new TestLabel ("BasicHttp_MessageSecurity");

			BindingTestAssertions.BasicHttpBinding (doc, BasicHttpSecurityMode.Message, label);
		}
		
		[Test]
		[Category ("NotWorking")]
		public void BasicHttp_TransportWithMessageCredential ()
		{
			var doc = TestContext.GetMetadata ("BasicHttp_TransportWithMessageCredential");
			var label = new TestLabel ("BasicHttp_TransportWithMessageCredential");

			BindingTestAssertions.BasicHttpBinding (
				doc, BasicHttpSecurityMode.TransportWithMessageCredential, label);
		}
		
		[Test]
		public void BasicHttp_Mtom ()
		{
			var doc = TestContext.GetMetadata ("BasicHttp_Mtom");
			var label = new TestLabel ("BasicHttp_Mtom");

			BindingTestAssertions.BasicHttpBinding (
				doc, WSMessageEncoding.Mtom, label);
		}

		[Test]
		public void BasicHttp_NtlmAuth ()
		{
			var doc = TestContext.GetMetadata ("BasicHttp_NtlmAuth");
			var label = new TestLabel ("BasicHttp_NtlmAuth");
			
			BindingTestAssertions.BasicHttpBinding (
				doc, BasicHttpSecurityMode.TransportCredentialOnly, WSMessageEncoding.Text,
				HttpClientCredentialType.Ntlm, AuthenticationSchemes.Ntlm,
				label);
		}

		[Test]
		public void BasicHttps ()
		{
			var doc = TestContext.GetMetadata ("BasicHttps");
			var label = new TestLabel ("BasicHttps");

			BindingTestAssertions.BasicHttpsBinding (
				doc, BasicHttpSecurityMode.Transport, WSMessageEncoding.Text,
				HttpClientCredentialType.None, AuthenticationSchemes.Anonymous,
				label);
		}
		
		[Test]
		public void BasicHttps_NtlmAuth ()
		{
			var doc = TestContext.GetMetadata ("BasicHttps_NtlmAuth");
			var label = new TestLabel ("BasicHttps_NtlmAuth");

			BindingTestAssertions.BasicHttpsBinding (
				doc, BasicHttpSecurityMode.Transport, WSMessageEncoding.Text,
				HttpClientCredentialType.Ntlm, AuthenticationSchemes.Ntlm,
				label);
		}
		
		[Test]
		[Category ("NotWorking")]
		public void BasicHttps_Certificate ()
		{
			var doc = TestContext.GetMetadata ("BasicHttps_Certificate");
			var label = new TestLabel ("BasicHttps_Certificate");

			BindingTestAssertions.BasicHttpsBinding (
				doc, BasicHttpSecurityMode.Transport, WSMessageEncoding.Text,
				HttpClientCredentialType.Certificate, AuthenticationSchemes.Anonymous,
				label);
		}
		
		[Test]
		[Category ("NotWorking")]
		public void BasicHttps_TransportWithMessageCredential ()
		{
			var doc = TestContext.GetMetadata ("BasicHttps_TransportWithMessageCredential");
			var label = new TestLabel ("BasicHttps_TransportWithMessageCredential");

			BindingTestAssertions.BasicHttpsBinding (
				doc, BasicHttpSecurityMode.TransportWithMessageCredential,
				WSMessageEncoding.Text, HttpClientCredentialType.None,
				AuthenticationSchemes.Anonymous, label);
		}
		
		[Test]
		public void NetTcp ()
		{
			var doc = TestContext.GetMetadata ("NetTcp");
			var label = new TestLabel ("NetTcp");
			BindingTestAssertions.NetTcpBinding (
				doc, SecurityMode.None, false, TransferMode.Buffered, label);
		}

		[Test]
		public void NetTcp_TransferMode ()
		{
			var doc = TestContext.GetMetadata ("NetTcp_TransferMode");

			var label = new TestLabel ("NetTcp_TransferMode");
			BindingTestAssertions.NetTcpBinding (
				doc, SecurityMode.None, false,
				TransferMode.Streamed, label);
		}

		[Test]
		public void NetTcp_TransportSecurity ()
		{
			var doc = TestContext.GetMetadata ("NetTcp_TransportSecurity");
			var label = new TestLabel ("NetTcp_TransportSecurity");
			BindingTestAssertions.NetTcpBinding (
				doc, SecurityMode.Transport, false,
				TransferMode.Buffered, label);
		}
		
		[Test]
		[Category ("NotWorking")]
		public void NetTcp_MessageSecurity ()
		{
			var doc = TestContext.GetMetadata ("NetTcp_MessageSecurity");
			var label = new TestLabel ("NetTcp_MessageSecurity");
			BindingTestAssertions.NetTcpBinding (
				doc, SecurityMode.Message, false,
				TransferMode.Buffered, label);
		}
		
		[Test]
		[Category ("NotWorking")]
		public void NetTcp_TransportWithMessageCredential ()
		{
			var doc = TestContext.GetMetadata ("NetTcp_TransportWithMessageCredential");
			var label = new TestLabel ("NetTcp_TransportWithMessageCredential");

			BindingTestAssertions.NetTcpBinding (
				doc, SecurityMode.TransportWithMessageCredential, false,
				TransferMode.Buffered, label);
		}

		[Test]
		public void NetTcp_Binding ()
		{
			var label = new TestLabel ("NetTcp_Binding");

			label.EnterScope ("None");
			BindingTestAssertions.CheckNetTcpBinding (
				new NetTcpBinding (SecurityMode.None), SecurityMode.None,
				false, TransferMode.Buffered, label);
			label.LeaveScope ();

			label.EnterScope ("Transport");
			BindingTestAssertions.CheckNetTcpBinding (
				new NetTcpBinding (SecurityMode.Transport), SecurityMode.Transport,
				false, TransferMode.Buffered, label);
			label.LeaveScope ();
		}

		[Test]
		[Category ("NotWorking")]
		public void NetTcp_Binding2 ()
		{
			var label = new TestLabel ("NetTcp_Binding2");

			label.EnterScope ("TransportWithMessageCredential");
			BindingTestAssertions.CheckNetTcpBinding (
				new NetTcpBinding (SecurityMode.TransportWithMessageCredential),
				SecurityMode.TransportWithMessageCredential, false,
				TransferMode.Buffered, label);
			label.LeaveScope ();
		}
		
		[Test]
		[Category ("NotWorking")]
		public void NetTcp_ReliableSession ()
		{
			var doc = TestContext.GetMetadata ("NetTcp_ReliableSession");
			var label = new TestLabel ("NetTcp_ReliableSession");
			BindingTestAssertions.NetTcpBinding (
				doc, SecurityMode.None, true,
				TransferMode.Buffered, label);
		}
	}

}

