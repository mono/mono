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

	/*
	 * This class is abstract to allow it to be run multiple times with
	 * different TestContexts.
	 */
	[Category ("MetadataTests")]
	public abstract class ImportTests {

		public abstract TestContext Context {
			get;
		}

		protected MetadataSet GetMetadata (string name, out TestLabel label)
		{
			label = new TestLabel (name);
			return Context.GetMetadata (name);
		}

		protected MetadataSet GetMetadataAndConfig (
			string name, out XmlDocument config, out TestLabel label)
		{
			var metadata = GetMetadata (name, out label);
			config = Context.GetConfiguration (name);
			return metadata;
		}

		[Test]
		public virtual void BasicHttp ()
		{
			TestLabel label;
			var doc = GetMetadata ("BasicHttp", out label);

			BindingTestAssertions.BasicHttpBinding (
				Context, doc, BasicHttpSecurityMode.None, label);
		}
		
		[Test]
		public virtual void BasicHttp_TransportSecurity ()
		{
			TestLabel label;
			var doc = GetMetadata ("BasicHttp_TransportSecurity", out label);

			BindingTestAssertions.BasicHttpBinding (
				Context, doc, BasicHttpSecurityMode.Transport, label);
		}
		
		[Test]
		[Category ("NotWorking")]
		public virtual void BasicHttp_MessageSecurity ()
		{
			TestLabel label;
			var doc = GetMetadata ("BasicHttp_MessageSecurity", out label);

			BindingTestAssertions.BasicHttpBinding (
				Context, doc, BasicHttpSecurityMode.Message, label);
		}
		
		[Test]
		[Category ("NotWorking")]
		public virtual void BasicHttp_TransportWithMessageCredential ()
		{
			TestLabel label;
			var doc = GetMetadata ("BasicHttp_TransportWithMessageCredential", out label);

			BindingTestAssertions.BasicHttpBinding (
				Context, doc, BasicHttpSecurityMode.TransportWithMessageCredential, label);
		}
		
		[Test]
		public virtual void BasicHttp_Mtom ()
		{
			TestLabel label;
			var doc = GetMetadata ("BasicHttp_Mtom", out label);

			BindingTestAssertions.BasicHttpBinding (
				Context, doc, WSMessageEncoding.Mtom, label);
		}

		[Test]
		public virtual void BasicHttp_NtlmAuth ()
		{
			TestLabel label;
			var doc = GetMetadata ("BasicHttp_NtlmAuth", out label);

			BindingTestAssertions.BasicHttpBinding (
				Context, doc, BasicHttpSecurityMode.TransportCredentialOnly,
				WSMessageEncoding.Text, HttpClientCredentialType.Ntlm,
				AuthenticationSchemes.Ntlm, label);
		}

#if NET_4_5
		[Test]
		public virtual void BasicHttps ()
		{
			TestLabel label;
			var doc = GetMetadata ("BasicHttps", out label);

			BindingTestAssertions.BasicHttpsBinding (
				Context, doc, BasicHttpSecurityMode.Transport, WSMessageEncoding.Text,
				HttpClientCredentialType.None, AuthenticationSchemes.Anonymous,
				label);
		}
		
		[Test]
		public virtual void BasicHttps_NtlmAuth ()
		{
			TestLabel label;
			var doc = GetMetadata ("BasicHttps_NtlmAuth", out label);

			BindingTestAssertions.BasicHttpsBinding (
				Context, doc, BasicHttpSecurityMode.Transport, WSMessageEncoding.Text,
				HttpClientCredentialType.Ntlm, AuthenticationSchemes.Ntlm,
				label);
		}
		
		[Test]
		[Category ("NotWorking")]
		public virtual void BasicHttps_Certificate ()
		{
			TestLabel label;
			var doc = GetMetadata ("BasicHttps_Certificate", out label);

			BindingTestAssertions.BasicHttpsBinding (
				Context, doc, BasicHttpSecurityMode.Transport, WSMessageEncoding.Text,
				HttpClientCredentialType.Certificate, AuthenticationSchemes.Anonymous,
				label);
		}
		
		[Test]
		[Category ("NotWorking")]
		public virtual void BasicHttps_TransportWithMessageCredential ()
		{
			TestLabel label;
			var doc = GetMetadata ("BasicHttps_TransportWithMessageCredential", out label);

			BindingTestAssertions.BasicHttpsBinding (
				Context, doc, BasicHttpSecurityMode.TransportWithMessageCredential,
				WSMessageEncoding.Text, HttpClientCredentialType.None,
				AuthenticationSchemes.Anonymous, label);
		}
#endif
		
		[Test]
		public virtual void NetTcp ()
		{
			TestLabel label;
			var doc = GetMetadata ("NetTcp", out label);

			BindingTestAssertions.NetTcpBinding (
				Context, doc, SecurityMode.None, false, TransferMode.Buffered, label);
		}

		[Test]
		public virtual void NetTcp_TransferMode ()
		{
			TestLabel label;
			var doc = GetMetadata ("NetTcp_TransferMode", out label);

			BindingTestAssertions.NetTcpBinding (
				Context, doc, SecurityMode.None, false,
				TransferMode.Streamed, label);
		}

		[Test]
		public virtual void NetTcp_TransportSecurity ()
		{
			TestLabel label;
			var doc = GetMetadata ("NetTcp_TransportSecurity", out label);

			BindingTestAssertions.NetTcpBinding (
				Context, doc, SecurityMode.Transport, false,
				TransferMode.Buffered, label);
		}
		
		[Test]
		[Category ("NotWorking")]
		public virtual void NetTcp_MessageSecurity ()
		{
			TestLabel label;
			var doc = GetMetadata ("NetTcp_MessageSecurity", out label);

			BindingTestAssertions.NetTcpBinding (
				Context, doc, SecurityMode.Message, false,
				TransferMode.Buffered, label);
		}
		
		[Test]
		[Category ("NotWorking")]
		public virtual void NetTcp_TransportWithMessageCredential ()
		{
			TestLabel label;
			var doc = GetMetadata ("NetTcp_TransportWithMessageCredential", out label);

			BindingTestAssertions.NetTcpBinding (
				Context, doc, SecurityMode.TransportWithMessageCredential, false,
				TransferMode.Buffered, label);
		}

		[Test]
		public virtual void NetTcp_Binding ()
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
		public virtual void NetTcp_Binding2 ()
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
		public virtual void NetTcp_ReliableSession ()
		{
			TestLabel label;
			var doc = GetMetadata ("NetTcp_ReliableSession", out label);

			BindingTestAssertions.NetTcpBinding (
				Context, doc, SecurityMode.None, true,
				TransferMode.Buffered, label);
		}

		[Test]
		public virtual void BasicHttp_Operation ()
		{
			TestLabel label;
			var doc = GetMetadata ("BasicHttp_Operation", out label);

			BindingTestAssertions.TestOperation (doc, false, label);
		}

		[Test]
		public virtual void NetTcp_Operation ()
		{
			TestLabel label;
			var doc = GetMetadata ("NetTcp_Operation", out label);

			BindingTestAssertions.TestOperation (doc, true, label);
		}

		[Test]
		public virtual void BasicHttp_Config ()
		{
			TestLabel label;
			XmlDocument config;
			var metadata = GetMetadataAndConfig (
				"BasicHttp_Config", out config, out label);

			BindingTestAssertions.AssertConfig (metadata, config, label);
		}

		[Test]
		public virtual void BasicHttp_Config2 ()
		{
			TestLabel label;
			XmlDocument config;
			var metadata = GetMetadataAndConfig (
				"BasicHttp_Config2", out config, out label);
			
			BindingTestAssertions.AssertConfig (metadata, config, label);
		}
	}

}

