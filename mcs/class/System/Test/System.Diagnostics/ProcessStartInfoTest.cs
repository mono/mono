//
// ProcessStartInfoTest.cs - NUnit Test Cases for System.Diagnostics.ProcessStartInfo
//
// Authors:
//   Ankit Jain <jankit@novell.com>
//
// (c) 2007 Novell, Inc. (http://www.novell.com)
// 

using System;
using System.Diagnostics;

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
	}
}
