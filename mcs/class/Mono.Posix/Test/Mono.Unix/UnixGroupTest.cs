//
// UnixGroupTest.cs:
// 	NUnit Test Cases for Mono.Unix.UnixGroup
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2004 Jonathan Pryor
// 

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;

using Mono.Unix;

using Group = Mono.Unix.Native.Group;
using Syscall = Mono.Unix.Native.Syscall;

namespace MonoTests.Mono.Unix {

	[TestFixture, Category ("NotDotNet")]
	public class UnixGroupTest
	{
		[Test]
		public void ListAllGroups_ToString ()
		{
			try {
				Console.WriteLine ("Listing all groups");
				foreach (UnixGroupInfo group in UnixGroupInfo.GetLocalGroups ()) {
					Console.WriteLine ("\t{0}", group);
				}
			}
			catch (Exception e) {
				Assert.Fail (
						string.Format ("#TLAU_TS: Exception listing local groups: {0}",
							e.ToString()));
			}
		}

		[Test]
		public void ReentrantConstructors ()
		{
			var seen = new Dictionary<string, object> ();
			foreach (UnixGroupInfo group in UnixGroupInfo.GetLocalGroups ()) {
				if (seen.ContainsKey (group.GroupName))
					continue;
				seen.Add (group.GroupName, null);
				try {
					UnixGroupInfo byName = new UnixGroupInfo (group.GroupName);
					UnixGroupInfo byId   = new UnixGroupInfo (group.GroupId);

					Assert.AreEqual (group, byName, "#TRC: construct by name");
					Assert.AreEqual (group, byId,   "#TRC: construct by gid");
					Assert.AreEqual (byName, byId,  "#TRC: name == gid?");
				}
				catch (Exception e) {
					Assert.Fail (
						string.Format ("#TRC: Exception constructing UnixGroupInfo: {0}",
							e.ToString()));
				}
			}
		}

		[Test]
		public void NonReentrantSyscalls ()
		{
			var seen = new Dictionary<string, object> ();
			foreach (UnixGroupInfo group in UnixGroupInfo.GetLocalGroups ()) {
				if (seen.ContainsKey (group.GroupName))
					continue;
				seen.Add (group.GroupName, null);
				try {
					Group byName = Syscall.getgrnam (group.GroupName);
					Group byId   = Syscall.getgrgid ((uint) group.GroupId);

					Assert.IsNotNull (byName, "#TNRS: access by name");
					Assert.IsNotNull (byId,   "#TNRS: access by gid");

					UnixGroupInfo n = new UnixGroupInfo (byName);
					UnixGroupInfo u = new UnixGroupInfo (byId);

					Assert.AreEqual (group, n, "#TNRS: construct by name");
					Assert.AreEqual (group, u, "#TNRS: construct by gid");
					Assert.AreEqual (n, u,     "#TNRS: name == gid?");
				}
				catch (Exception e) {
					Assert.Fail (
						string.Format ("#TRC: Exception constructing UnixGroupInfo: {0}",
							e.ToString()));
				}
			}
		}

		[Test]
		public void InvalidGroups_Constructor_Name ()
		{
			string[] badGroups = new string[]{"i'm bad", "so am i", "does-not-exist"};
			foreach (string u in badGroups) {
				try {
					new UnixGroupInfo (u);
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
		public void InvalidGroups_Syscall_Name ()
		{
			string[] badGroups = new string[]{"i'm bad", "so am i", "does-not-exist"};
			foreach (string u in badGroups) {
				try {
					Group pw = Syscall.getgrnam (u);
					Assert.IsNull (pw, "#TIUSN: invalid groups should return null!");
				}
				catch (Exception e) {
					Assert.Fail (string.Format ("#TIUCN: invalid exception thrown: " +
								"expected null return, got {0}: {1}",
								e.GetType().FullName, e.Message));
				}
			}
		}

		[Test]
		public void Equality ()
		{
			Group orig = new Group ();
			Group mod  = new Group ();
			mod.gr_name   = orig.gr_name   = "some name";
			mod.gr_passwd = orig.gr_passwd = "some passwd";
			mod.gr_gid    = orig.gr_gid    = 500;
			mod.gr_mem    = orig.gr_mem    = new string[]{"foo", "bar"};

			Assert.AreEqual (orig, mod, "#TE: copies should be equal");

			mod.gr_name = "another name";
			Assert.IsFalse (orig.Equals (mod), "#TE: changes should be reflected");
		}
	}
}

