//
// UnixUserTest.cs:
// 	NUnit Test Cases for Mono.Unix.UnixUser
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2004 Jonathan Pryor
// 

using NUnit.Framework;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Collections;

using Mono.Unix;

using Passwd = Mono.Unix.Native.Passwd;
using Syscall = Mono.Unix.Native.Syscall;

namespace MonoTests.Mono.Unix {

	[TestFixture, Category ("NotDotNet"), Category ("NotOnWindows")]
	public class UnixUserTest
	{
		[Test]
		[Category ("AndroidNotWorking")] // setpwent is missing from bionic
		public void ListAllUsers_ToString ()
		{
			try {
				Console.WriteLine ("Listing all users");
				foreach (UnixUserInfo user in UnixUserInfo.GetLocalUsers ()) {
					Console.WriteLine ("\t{0}", user);
				}
			}
			catch (Exception e) {
				Assert.Fail (
						string.Format ("#TLAU_TS: Exception listing local users: {0}",
							e.ToString()));
			}
		}

		[Test]
		// According to bug 72293, this may not work:
		// On systems with NIS, it is possible to have multiple users in the passwd
		// file with the same name, so the assertion above no longer holds.
		[Category ("NotWorking")]
		public void ReentrantConstructors ()
		{
			ArrayList user_ids = new ArrayList (4);
			IList users = UnixUserInfo.GetLocalUsers ();
			foreach (UnixUserInfo user in users) {
				try {
					UnixUserInfo byName = new UnixUserInfo (user.UserName);
					Assert.AreEqual (user, byName, "#TRC: construct by name");

					if (! user_ids.Contains (user.UserId))
						user_ids.Add (user.UserId);
				}
				catch (Exception e) {
					Assert.Fail (
						     string.Format ("#TRC: Exception constructing UnixUserInfo (string): {0}",
								    e.ToString()));
				}
			}

			foreach (uint uid in user_ids) {
				try {
					UnixUserInfo byId = new UnixUserInfo (uid);
					Assert.IsTrue (users.Contains (byId), "TRC: construct by uid");
				}
				catch (Exception e) {
					Assert.Fail (
						     string.Format ("#TRC: Exception constructing UnixUserInfo (uint): {0}",
								    e.ToString()));

				}
			}
		}

		[Test]
		[Category ("NotOnMac")]
		[Category ("AndroidNotWorking")] // setpwent is missing from bionic
		public void NonReentrantSyscalls ()
		{
			ArrayList user_ids = new ArrayList (4);
			IList users = UnixUserInfo.GetLocalUsers ();

			foreach (UnixUserInfo user in users) {
				try {
					Passwd byName = Syscall.getpwnam (user.UserName);
					Assert.IsNotNull (byName, "#TNRS: access by name");
					UnixUserInfo n = new UnixUserInfo (byName);
					Assert.AreEqual (user, n, "#TNRS: construct by name");

					if (! user_ids.Contains (user.UserId))
						user_ids.Add (user.UserId);
				}
				catch (Exception e) {
					Assert.Fail (
						string.Format ("#TNRS: Exception constructing UnixUserInfo (string): {0}",
							e.ToString()));
				}
			}

			foreach (long uid in user_ids) {
				try {
					Passwd byId   = Syscall.getpwuid (Convert.ToUInt32 (uid));
					Assert.IsNotNull (byId,   "#TNRS: access by uid");

					UnixUserInfo u = new UnixUserInfo (byId);
					Assert.IsTrue (users.Contains (u), "TNRS: construct by uid");
				}
				catch (Exception e) {
					Assert.Fail (
						string.Format ("#TNRS: Exception constructing UnixUserInfo (uint): {0}",
							e.ToString()));
				}
			}
		}

		[Test]
		[Category ("AndroidNotWorking")] // API 21 has getpwnam_r in the NDK headers, but bionic doesn't export it
		public void InvalidUsers_Constructor_Name ()
		{
			string[] badUsers = new string[]{"i'm bad", "so am i", "does-not-exist"};
			foreach (string u in badUsers) {
				try {
					new UnixUserInfo (u);
					Assert.Fail ("#TIUCN: exception not thrown");
				}
				catch (ArgumentException) {
					// expected
				}
				catch (Exception e) {
					Assert.Fail (string.Format ("#TIUCN: invalid exception thrown: " +
								"expected ArgumentException, got {0}: {1}",
								e.GetType().FullName, e.Message));
				}
			}
		}

		[Test]
		public void InvalidUsers_Syscall_Name ()
		{
			string[] badUsers = new string[]{"i'm bad", "so am i", "does-not-exist"};
			foreach (string u in badUsers) {
				try {
					Passwd pw = Syscall.getpwnam (u);
					Assert.IsNull (pw, "#TIUSN: invalid users should return null!");
				}
				catch (Exception e) {
					Assert.Fail (string.Format ("#TIUCN: invalid exception thrown: " +
								"expected ArgumentException, got {0}: {1}",
								e.GetType().FullName, e.Message));
				}
			}
		}

		[Test]
		public void Equality ()
		{
			Passwd orig = new Passwd ();
			Passwd mod  = new Passwd ();
			mod.pw_name   = orig.pw_name   = "some name";
			mod.pw_passwd = orig.pw_passwd = "some passwd";
			mod.pw_uid    = orig.pw_uid    = 500;
			mod.pw_gid    = orig.pw_gid    = 500;
			mod.pw_gecos  = orig.pw_gecos  = "some gecos";
			mod.pw_dir    = orig.pw_dir    = "/some/dir";
			mod.pw_shell  = orig.pw_shell  = "/some/shell";

			Assert.AreEqual (orig, mod, "#TE: copies should be equal");

			mod.pw_name = "another name";
			Assert.IsFalse (orig.Equals (mod), "#TE: changes should be reflected");
		}
	}
}

