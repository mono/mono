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
using System.Xml;

using Mono.Unix;

namespace MonoTests.Mono.Unix {

	[TestFixture]
	public class UnixUserTest
	{
		[Test]
		public void ListAllUsers_ToString ()
		{
			try {
				Console.WriteLine ("Listing all users");
				foreach (UnixUserInfo user in UnixUser.GetLocalUsers ()) {
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
		public void ReentrantConstructors ()
		{
			foreach (UnixUserInfo user in UnixUser.GetLocalUsers ()) {
				try {
					UnixUserInfo byName = new UnixUserInfo (user.UserName);
					UnixUserInfo byId   = new UnixUserInfo (user.UserId);

					Assert.AreEqual (user, byName, "#TRC: construct by name");
					Assert.AreEqual (user, byId,   "#TRC: construct by uid");
					Assert.AreEqual (byName, byId, "#TRC: name == uid?");
				}
				catch (Exception e) {
					Assert.Fail (
						string.Format ("#TRC: Exception constructing UnixUserInfo: {0}",
							e.ToString()));
				}
			}
		}

		[Test]
		public void NonReentrantSyscalls ()
		{
			foreach (UnixUserInfo user in UnixUser.GetLocalUsers ()) {
				try {
					Passwd byName = Syscall.getpwnam (user.UserName);
					Passwd byId   = Syscall.getpwuid (user.UserId);

					Assert.IsNotNull (byName, "#TNRS: access by name");
					Assert.IsNotNull (byId,   "#TNRS: access by uid");

					UnixUserInfo n = new UnixUserInfo (byName);
					UnixUserInfo u = new UnixUserInfo (byId);

					Assert.AreEqual (user, n, "#TNRS: construct by name");
					Assert.AreEqual (user, u, "#TNRS: construct by uid");
					Assert.AreEqual (n, u,    "#TNRS: name == uid?");
				}
				catch (Exception e) {
					Assert.Fail (
						string.Format ("#TRC: Exception constructing UnixUserInfo: {0}",
							e.ToString()));
				}
			}
		}

		[Test]
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

