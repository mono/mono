//
// MessageSecurityVersionTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
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
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel
{
	[TestFixture]
	public class MessageSecurityVersionTest
	{
		[Test]
		public void Default ()
		{
			Assert.AreEqual (
				MessageSecurityVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11,
				MessageSecurityVersion.Default,
				"dude, stop using crappy lengthy name");
		}

		[Test]
		public void SecurityTokenVersion ()
		{
			ReadOnlyCollection<string> specs = 
				MessageSecurityVersion.Default.SecurityTokenVersion.GetSecuritySpecifications ();
			Assert.AreEqual (3, specs.Count, "#1");
			// Not sure why MS limits the results to them. This 
			// result rather means that it is not worthy of
			// testing strictly.
			string [] expected = new string [] {
				"http://docs.oasis-open.org/wss/oasis-wss-wssecurity-secext-1.1.xsd",
				"http://schemas.xmlsoap.org/ws/2005/02/trust",
				"http://schemas.xmlsoap.org/ws/2005/02/sc"
				};
			foreach (string spec in specs)
				if (!Array.Exists<string> (expected, delegate (string s) { return s == spec; }))
					Assert.Fail (String.Format ("Unexpected spec '{0}'", spec), "#2");

			specs = MessageSecurityVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11BasicSecurityProfile10.SecurityTokenVersion.GetSecuritySpecifications ();
			Assert.AreEqual (4, specs.Count, "#3");
			// Not sure why MS limits the results to them. This 
			// result rather means that it is not worthy of testing.
			expected = new string [] {
				"http://docs.oasis-open.org/wss/oasis-wss-wssecurity-secext-1.1.xsd",
				"http://schemas.xmlsoap.org/ws/2005/02/trust",
				"http://schemas.xmlsoap.org/ws/2005/02/sc",
				"http://ws-i.org/profiles/basic-security/core/1.0",
				};
			foreach (string spec in specs)
				if (!Array.Exists<string> (expected, delegate (string s) { return s == spec; }))
					Assert.Fail (String.Format ("Unexpected spec '{0}'", spec), "#4");

			specs = MessageSecurityVersion.WSSecurity10WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11BasicSecurityProfile10.SecurityTokenVersion.GetSecuritySpecifications ();
			Assert.AreEqual (4, specs.Count, "#5");
			// Not sure why MS limits the results to them. This 
			// result rather means that it is not worthy of testing.
			expected = new string [] {
				"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd",
				"http://schemas.xmlsoap.org/ws/2005/02/trust",
				"http://schemas.xmlsoap.org/ws/2005/02/sc",
				"http://ws-i.org/profiles/basic-security/core/1.0",
				};
			foreach (string spec in specs)
				if (!Array.Exists<string> (expected, delegate (string s) { return s == spec; }))
					Assert.Fail (String.Format ("Unexpected spec '{0}'", spec), "#6");
		}
	}
}
#endif
