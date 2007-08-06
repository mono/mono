//
// ProcessStartInfoTest.cs - NUnit Test Cases for System.Diagnostics.ProcessStartInfo
//
// Authors:
//   Ankit Jain <jankit@novell.com>
//   Atsushi Enomoto <atsushi@ximian.com>
//
// (c) 2007 Novell, Inc. (http://www.novell.com)
// 

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
		public void NullWorkingDirectory ()
		{
			ProcessStartInfo info = new ProcessStartInfo ();
			info.WorkingDirectory = null;
			Assert.AreEqual (info.WorkingDirectory, String.Empty, "#1");
		}

#if NET_2_0
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
#endif
	}
}
