//
// ProxyGeneratorTest.cs
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
using System;
using System.Net;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Web.Script.Services;
using System.Text;
using NUnit.Framework;

namespace MonoTests.System.Web.Script.Services
{
	[TestFixture]
	public class ProxyGeneratorTest
	{
		[Test]
		public void ScriptGenerator ()
		{
			var s = ProxyGenerator.GetClientProxyScript (typeof (IHogeService), "/js", false);
			Assert.IsTrue (s.IndexOf ("IHogeService") > 0, "#1");
			Assert.IsTrue (s.IndexOf ("Join") > 0, "#2");
			s = ProxyGenerator.GetClientProxyScript (typeof (IHogeService), "/jsdebug", true);
			Assert.IsTrue (s.IndexOf ("IHogeService") > 0, "#3");
			Assert.IsTrue (s.IndexOf ("Join") > 0, "#4");
		}

		[ServiceContract]
		public interface IHogeService
		{
//			[WebGet]
			[OperationContract]
			string Echo (string s);

//			[WebGet]
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
