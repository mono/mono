//
// WebScriptEnablingBehaviorTest.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2009 Novell, Inc (http://www.novell.com)
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
#if !MOBILE && !XAMMAC_4_5
using System;
using System.Net;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;
using System.Text;
using NUnit.Framework;

using MonoTests.Helpers;

namespace MonoTests.System.ServiceModel.Description
{
	public class MyHostFactory : WebScriptServiceHostFactory
	{
		// Calling CreateServiceHost(string,Uri[]) is not valid outside ASP.NET
		// so we have to use custom factory...
		public ServiceHost CreateServiceHost (Type type)
		{
			return CreateServiceHost (type, new Uri [0]);
		}
	}

	[TestFixture]
	public class WebScriptEnablingBehaviorTest
	{
		/*
		ServiceEndpoint CreateEndpoint ()
		{
			return new ServiceEndpoint (ContractDescription.GetContract (typeof (IMyService)), new WebHttpBinding (),
						    new EndpointAddress ("http://localhost:37564"));
		}
		*/

		[Test]
		public void ScriptGenerator ()
		{
			var url = "http://localhost:" + NetworkHelpers.FindFreePort ();
			var host = new MyHostFactory ().CreateServiceHost (typeof (HogeService));
			var binding = new WebHttpBinding ();
			host.AddServiceEndpoint (typeof (IHogeService), binding, url);
			host.Open ();
			try {
				var wc = new WebClient ();
				var s = wc.DownloadString (url + "/js");
				Assert.IsTrue (s.IndexOf ("IHogeService") > 0, "#1");
				Assert.IsTrue (s.IndexOf ("Join") > 0, "#2");
				s = wc.DownloadString (url + "/jsdebug");
				Assert.IsTrue (s.IndexOf ("IHogeService") > 0, "#3");
				Assert.IsTrue (s.IndexOf ("Join") > 0, "#4");
				s = wc.DownloadString (url + "/Join?s1=foo&s2=bar");
				Assert.AreEqual ("{\"d\":\"foobar\"}", s, "#5");
			} finally {
				host.Close ();
			}
		}

		[ServiceContract]
		public interface IHogeService
		{
			[WebGet]
			[OperationContract]
			string Echo (string s);

			[WebGet]
			// error -> [WebGet (BodyStyle = WebMessageBodyStyle.Wrapped)]
			[OperationContract]
			string Join (string s1, string s2);
		}

		public class HogeService : IHogeService
		{
			public string Echo (string s)
			{
				return "heh, I don't";
			}

			public string Join (string s1, string s2)
			{
				Console.WriteLine ("{0} + {1}", s1, s2);
				return s1 + s2;
			}
		}
	}
}
#endif