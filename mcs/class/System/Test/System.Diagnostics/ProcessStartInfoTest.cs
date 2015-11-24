//
// ProcessStartInfoTest.cs - NUnit Test Cases for System.Diagnostics.ProcessStartInfo
//
// Authors:
//   Ankit Jain <jankit@novell.com>
//   Atsushi Enomoto <atsushi@ximian.com>
//
// (c) 2007 Novell, Inc. (http://www.novell.com)
// 

#if MONO_FEATURE_PROCESS_START

using System;
using System.Diagnostics;
using System.Text;

using NUnit.Framework;

namespace MonoTests.System.Diagnostics
{
	[TestFixture]
	public class ProcessStartInfoTest
	{
		[Test]
		public void NotNullCommonProperties ()
		{
			// Force FileName and Arguments to null. The others are null by default.
			ProcessStartInfo info = new ProcessStartInfo (null, null);

			Assert.AreEqual (info.Arguments, String.Empty, "#1");
			Assert.AreEqual (info.Domain, String.Empty, "#2");
			Assert.AreEqual (info.FileName, String.Empty, "#3");
			Assert.AreEqual (info.UserName, String.Empty, "#4");
			Assert.AreEqual (info.Verb, String.Empty, "#5");
			Assert.AreEqual (info.WorkingDirectory, String.Empty, "#6");
		}

		[Test]
		public void StandardErrorOutputEncoding ()
		{
			ProcessStartInfo info = new ProcessStartInfo ();
			Assert.IsNull (info.StandardErrorEncoding, "#1");
			Assert.IsNull (info.StandardOutputEncoding, "#2");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void StandardErrorEncodingWithoutRedirect ()
		{
			ProcessStartInfo info = new ProcessStartInfo ();
			info.FileName = "mono";
			info.StandardErrorEncoding = Encoding.UTF8;
			Process.Start (info);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void StandardOutputEncodingWithoutRedirect ()
		{
			ProcessStartInfo info = new ProcessStartInfo ();
			info.FileName = "mono";
			info.StandardOutputEncoding = Encoding.UTF8;
			Process.Start (info);
		}
	}
}

#endif // MONO_FEATURE_PROCESS_START
