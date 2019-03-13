//
// StateRuntimeCas.cs 
//	- CAS unit tests for System.Web.SessionState.StateRuntime
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
using System.Web.SessionState;

namespace MonoCasTests.System.Web.SessionState {

	// note: the execution order (not user controlable) is very important 
	// for the tests to execute properly. However the tests are protected 
	// not to report errors if the order isn't repected.

	[TestFixture]
	[Category ("CAS")]
	public class StateRuntimeCas : AspNetHostingMinimal {

		private StateRuntime runtime;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			try {
				// ensure that the static ctor has been called
				runtime = new StateRuntime ();
			}
			catch {
			}
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Constructor_Deny_UnmanagedCode ()
		{
			try {
				new StateRuntime ();
			}
			catch (TypeInitializationException) {
				Assert.Ignore ("fails on MS");
			}
		}

		[Test]
		[AspNetHostingPermission (SecurityAction.Deny, Level = AspNetHostingPermissionLevel.Minimal)]
		[ExpectedException (typeof (SecurityException))]
		public void Constructor_Deny_Minimal ()
		{
			try {
				new StateRuntime ();
			}
			catch (TypeInitializationException) {
				Assert.Ignore ("fails on MS");
			}
		}

		[Test]
		[AspNetHostingPermission (SecurityAction.PermitOnly, Level = AspNetHostingPermissionLevel.Minimal)]
		[SecurityPermission (SecurityAction.PermitOnly, UnmanagedCode = true)]
		public void Constructor_PermitOnly_UnmanagedCodeMinimal ()
		{
			try {
				new StateRuntime ();
			}
			catch (TypeInitializationException) {
				Assert.Ignore ("fails on MS");
			}
		}

		[Test]
		[AspNetHostingPermission (SecurityAction.PermitOnly, Level = AspNetHostingPermissionLevel.Medium)]
		public void ProcessRequest9_PermitOnly_Medium ()
		{
			if (runtime == null)
				Assert.Ignore ("impossible to instantiate under MS");

			try {
				runtime.ProcessRequest (IntPtr.Zero, 0, null, 0, 0, 0, 0, 0, IntPtr.Zero);
			}
			catch (NullReferenceException) {
				// ms 1.x
			}
			catch (DllNotFoundException) {
				// ms 2.0
			}
			catch (NotImplementedException)	{
				// mono
			}
		}

		[Test]
		[AspNetHostingPermission (SecurityAction.Deny, Level = AspNetHostingPermissionLevel.Medium)]
		[ExpectedException (typeof (SecurityException))]
		public void ProcessRequest9_Deny_Medium ()
		{
			if (runtime == null)
				Assert.Ignore ("impossible to instantiate under MS");

			runtime.ProcessRequest (IntPtr.Zero, 0, null, 0, 0, 0, 0, 0, IntPtr.Zero);
		}
		[Test]
		[AspNetHostingPermission (SecurityAction.PermitOnly, Level = AspNetHostingPermissionLevel.Medium)]
		public void ProcessRequest10_PermitOnly_Medium ()
		{
			if (runtime == null)
				Assert.Ignore ("impossible to instantiate under MS");

			try {
				runtime.ProcessRequest (IntPtr.Zero, 0, null, 0, 0, 0, 0, 0, 0, IntPtr.Zero);
			}
			catch (DllNotFoundException) {
				// ms
			}
			catch (NotImplementedException)	{
				// mono
			}
		}

		[Test]
		[AspNetHostingPermission (SecurityAction.Deny, Level = AspNetHostingPermissionLevel.Medium)]
		[ExpectedException (typeof (SecurityException))]
		public void ProcessRequest10_Deny_Medium ()
		{
			if (runtime == null)
				Assert.Ignore ("impossible to instantiate under MS");

			runtime.ProcessRequest (IntPtr.Zero, 0, null, 0, 0, 0, 0, 0, 0, IntPtr.Zero);
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, UnmanagedCode = true)]
		[Category ("NotDotNet")] // freeze
		public void StopProcessing_PermitOnly_UnmanagedCode ()
		{
			if (runtime == null)
				Assert.Ignore ("impossible to instantiate under MS");

			try {
				runtime.StopProcessing ();
			}
			catch (SecurityException se) {
				Console.WriteLine (se);
			}
			catch (NotImplementedException)	{
				// mono
			}
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[ExpectedException (typeof (SecurityException))]
		public void StopProcessing_Deny_UnmanagedCode ()
		{
			if (runtime == null)
				Assert.Ignore ("may not be possible to instantiate under MS (run-order)");

			runtime.StopProcessing ();
		}

		// LinkDemand

		[SecurityPermission (SecurityAction.Assert, UnmanagedCode = true)]
		public override object CreateControl (SecurityAction action, AspNetHostingPermissionLevel level)
		{
			try {
				return base.CreateControl (action, level);
			}
			catch (TypeInitializationException) {
				// ctor can't be called more than once (else it throws TIE)
				return String.Empty;
			}
		}

		public override Type Type {
			get { return typeof (StateRuntime); }
		}
	}
}
#endif
