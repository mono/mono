// PipeSecurityTest.cs - NUnit Test Cases for PipeSecurity
//
// Authors:
//	James Bellinger  <jfb@zer7.com>
//
// Copyright (C) 2012 James Bellinger

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using NUnit.Framework;

namespace MonoTests.System.IO.Pipes
{
	[TestFixture]
	public class PipeSecurityTest
	{
		[Test]
		public void NamedPipeDefaultPermissionsWork ()
		{
			if (PlatformID.Win32NT != Environment.OSVersion.Platform) {
				Assert.Ignore (); return;
			}

			string name = @"Local\MonoTestPipeNPNPW";
			using (NamedPipeServerStream server = CreateNamedServer (false, name, null, 0)) {
				PipeSecurity security = server.GetAccessControl ();

				AuthorizationRuleCollection rules = security.GetAccessRules (true, false,
				                                                             typeof (SecurityIdentifier));
				Assert.AreNotEqual (0, rules.Count);
			}
		}

		[Test]
		public void NamedPipeSetAccessControlFailsWithoutChangePermissionRight ()
		{
			if (PlatformID.Win32NT != Environment.OSVersion.Platform) {
				Assert.Ignore (); return;
			}

			string name = @"Local\MonoTestPipeNPSACFWCPR";
			using (NamedPipeServerStream server = CreateNamedServer (false, name, null, 0)) {
				bool unauthorized = false;
				try {
					AddDenyEveryone (server);
				} catch (UnauthorizedAccessException) {
					unauthorized = true;
				}

				Assert.IsTrue (unauthorized, "PipeAccessRights.ChangePermissions was not required");
			}
		}

		[Test]
		public void NamedPipePermissionsActuallyWorkSyncAllow ()
		{
			NamedPipePermissionsActuallyWorkSync (@"Local\MonoTestPipeNPPAWSA", false);
		}

		[Test]
		public void NamedPipePermissionsActuallyWorkSyncDeny ()
		{
			NamedPipePermissionsActuallyWorkSync (@"Local\MonoTestPipeNPPAWSD", true);
		}

		void NamedPipePermissionsActuallyWorkSync (string name, bool addDenyEveryone)
		{
			if (PlatformID.Win32NT != Environment.OSVersion.Platform) {
				Assert.Ignore (); return;
			}

			PipeSecurity security = new PipeSecurity ();
			SecurityIdentifier worldSid = new SecurityIdentifier ("WD");
			PipeAccessRule rule = new PipeAccessRule (worldSid,
			                                          PipeAccessRights.FullControl,
			                                          AccessControlType.Allow);
			security.AddAccessRule (rule);

			using (NamedPipeServerStream server = CreateNamedServer (false, name, security,
										 PipeAccessRights.ChangePermissions)) {
				security = server.GetAccessControl ();

				AuthorizationRuleCollection rules;
				rules = security.GetAccessRules (true, true, typeof (SecurityIdentifier));
				Assert.AreEqual (1, rules.Count);

				rule = (PipeAccessRule)rules [0];
				Assert.AreEqual (AccessControlType.Allow, rule.AccessControlType);
				Assert.AreEqual (worldSid, rule.IdentityReference);
				Assert.AreEqual (PipeAccessRights.FullControl, rule.PipeAccessRights);

				if (addDenyEveryone)
					AddDenyEveryone (server);

				bool unauthorized = false;
				using (NamedPipeClientStream client = CreateNamedClient (false, name)) {
					try {
						client.Connect (1000);
					} catch (UnauthorizedAccessException) {
						unauthorized = true;
					}
				}

				Assert.AreEqual (addDenyEveryone, unauthorized);
			}
		}


		[Test]
		[Category ("NotWorking")] // Async is completely broken on Mono Win32 pipes.
		public void NamedPipePermissionsActuallyWorkAsync ()
		{
			if (PlatformID.Win32NT != Environment.OSVersion.Platform) {
				Assert.Ignore (); return;
			}

			IAsyncResult waitForConnection;
			string name = @"Local\MonoTestPipeNPPAWA";

			using (NamedPipeServerStream server = CreateNamedServer (true, name, null,
										 PipeAccessRights.ChangePermissions)) {
				// Test connecting to make sure our later test throwing is due to permissions.
				waitForConnection = server.BeginWaitForConnection (null, null);

				using (NamedPipeClientStream client = CreateNamedClient (true, name)) {
					client.Connect (1000);

					if (!waitForConnection.AsyncWaitHandle.WaitOne (1000)) {
						Assert.Fail ("No connection request received."); return;
					}
					server.EndWaitForConnection (waitForConnection);
					server.Disconnect ();
				}

				// Let's add a Deny for Everyone.
				AddDenyEveryone (server);

				// This Connect call should fail.
				waitForConnection = server.BeginWaitForConnection (null, null);

				bool unauthorized = false;
				using (NamedPipeClientStream client = CreateNamedClient (true, name)) {
					try {
						client.Connect (1000);
					} catch (UnauthorizedAccessException) {
						unauthorized = true;
					}
				}

				Assert.IsTrue (unauthorized, "Client was allowed to connect despite Deny ACE.");
			}
		}

		static void AddDenyEveryone (PipeStream stream)
		{
			PipeAccessRule rule; PipeSecurity security;
			AuthorizationRuleCollection inRules, outRules;

			// Let's add a Deny for Everyone.
			security = stream.GetAccessControl ();

			inRules = security.GetAccessRules (true, false, typeof (SecurityIdentifier));
			Assert.AreNotEqual (0, inRules.Count);

			rule = new PipeAccessRule (new SecurityIdentifier ("WD"),
			                           PipeAccessRights.FullControl,
			                           AccessControlType.Deny);
			security.AddAccessRule (rule);
			stream.SetAccessControl (security);

			security = stream.GetAccessControl ();
			outRules = security.GetAccessRules (true, false, typeof (SecurityIdentifier));
			Assert.AreEqual (inRules.Count + 1, outRules.Count);
		}

		static NamedPipeClientStream CreateNamedClient (bool @async, string name)
		{
			return new NamedPipeClientStream (".", name,
			                                  PipeDirection.InOut,
			                                  @async ? PipeOptions.Asynchronous : PipeOptions.None);
		}

		static NamedPipeServerStream CreateNamedServer (bool @async, string name,
		                                           PipeSecurity security,
		                                           PipeAccessRights additionalRights)
		{
			return new NamedPipeServerStream (name,
			                                  PipeDirection.InOut, 1,
			                                  PipeTransmissionMode.Byte,
			                                  @async ? PipeOptions.Asynchronous : PipeOptions.None,
			                                  512, 512, security,
			                                  HandleInheritability.None,
			                                  additionalRights);
		}
	}
}

