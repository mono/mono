//
// Mono.Unix.UnixPath Test Cases
//
// Authors:
//  Jonathan Pryor (jonpryor@vt.edu)
//
// (c) 2006 Jonathan Pryor
//

using NUnit.Framework;
using System.IO;
using System;
using System.Text;
using Mono.Unix;

namespace MonoTests.Mono.Unix
{

	[TestFixture, Category ("NotDotNet"), Category ("NotOnWindows")]
	public class UnixPathTest {

		private static readonly char DSC = UnixPath.DirectorySeparatorChar;

		[Test]
		public void Combine ()
		{
			string path, expected;
			string current = UnixDirectoryInfo.GetCurrentDirectory ();

			path = UnixPath.Combine ("/etc", "init.d");
			Assert.AreEqual ("/etc/init.d", path);

			path = UnixPath.Combine ("one", "");
			Assert.AreEqual ("one", path);

			path = UnixPath.Combine ("", "one");
			Assert.AreEqual ("one", path);

			path = UnixPath.Combine (current, "one");
			expected = (current == "/" ? String.Empty : current) + DSC + "one";
			Assert.AreEqual (expected, path);

			path = UnixPath.Combine ("one", current);
			Assert.AreEqual (current, path);

			path = UnixPath.Combine (current, expected);
			Assert.AreEqual (expected, path);

			path = DSC + "one";
			path = UnixPath.Combine (path, "two" + DSC);
			expected = DSC + "one" + DSC + "two" + DSC;
			Assert.AreEqual (expected, path);

			path = "one" + DSC;
			path = UnixPath.Combine (path, DSC + "two");
			expected = DSC + "two";
			Assert.AreEqual (expected, path);

			path = "one" + DSC;
			path = UnixPath.Combine (path, "two" + DSC);
			expected = "one" + DSC + "two" + DSC;
			Assert.AreEqual (expected, path);

			path = UnixPath.Combine ("/a", "b", "c", "/d", "e");
			expected = "/d/e";
			Assert.AreEqual (expected, path);

			try {
				path = UnixPath.Combine ("one", null);
				Assert.Fail ("Combine Fail #01");
			}
			catch (Exception e) {
				Assert.AreEqual (typeof (ArgumentNullException), e.GetType ());
			}

			try {
				path = UnixPath.Combine (null, "one");
				Assert.Fail ("Combine Fail #02");
			}
			catch (Exception e) {
				Assert.AreEqual (typeof (ArgumentNullException), e.GetType ());
			}
		}
	}
}

