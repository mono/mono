//
// ISAPIRuntimeCas.cs - CAS unit tests for System.Web.Hosting.ISAPIRuntime
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
#if false
using NUnit.Framework;

using System;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Web;
using System.Web.Hosting;

namespace MonoCasTests.System.Web.Hosting {

	[TestFixture]
	[Category ("CAS")]
	public class ISAPIRuntimeCas : AspNetHostingMinimal {

		private ISAPIRuntime isapi;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			// we're at full trust here
			isapi = new ISAPIRuntime ();
		}

		// test ctor (those tests aren't affected by a LinkDemand)

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Constructor_Deny_UnmanagedCode ()
		{
			new ISAPIRuntime ();
		}

		[Test]
		[AspNetHostingPermission (SecurityAction.Deny, Level = AspNetHostingPermissionLevel.Minimal)]
		[ExpectedException (typeof (SecurityException))]
		public void Constructor_Deny_AspNetHostingPermission ()
		{
			new ISAPIRuntime ();
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, UnmanagedCode = true)]
		[AspNetHostingPermission (SecurityAction.PermitOnly, Level = AspNetHostingPermissionLevel.Minimal)]
		public void Constructor_PermitOnly_UnmanagedCode ()
		{
			new ISAPIRuntime ();
		}

		// only StopProcessing requires some permissions (UnmanagedCode)

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Members_Deny_Unrestricted ()
		{
			try {
				isapi.DoGCCollect ();
			}
			catch (NotImplementedException) {
				// mono
			}
			try {
				isapi.ProcessRequest (IntPtr.Zero, 0);
			}
			catch (AccessViolationException) {
				// fx2.0
			}
			catch (NotImplementedException) {
				// mono
			}
			try {
				isapi.StartProcessing ();
			}
			catch (NotImplementedException) {
				// mono
			}
			try {
				isapi.InitializeLifetimeService ();
			}
			catch (NotImplementedException) {
				// mono
			}
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[ExpectedException (typeof (SecurityException))]
		public void StopProcessing_Deny_UnmanagedCode ()
		{
			try {
				isapi.StopProcessing ();
			}
			finally {
			}
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, UnmanagedCode = true)]
		public void StopProcessing_PermitOnly_UnmanagedCode ()
		{
			try {
				isapi.StopProcessing ();
			}
			catch (NotImplementedException) {
				// mono
			}
		}

		// test for LinkDemand on class

		[SecurityPermission (SecurityAction.Assert, UnmanagedCode = true)]
		public override object CreateControl (SecurityAction action, AspNetHostingPermissionLevel level)
		{
			// in this case testing the ctor isn't very conveniant
			// because it has a Demand similar to the LinkDemand.
			try {
				return base.CreateControl (action, level);
			}
			catch (TargetInvocationException tie) {
				throw tie.InnerException;
			}
		}

		public override Type Type {
			get { return typeof (ISAPIRuntime); }
		}
	}
}
#endif
