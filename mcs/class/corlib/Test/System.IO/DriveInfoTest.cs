//
// DriveInfoTest.cs - NUnit Test Cases for System.IO.DriveInfo class
//
// Authors
//	Alexander Köplinger <alkpli@microsoft.com>
// 
// Copyright (c) 2017 Xamarin, Inc.
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using NUnit.Framework;

namespace MonoTests.System.IO
{
	[TestFixture]
	public class DriveInfoTest
	{
		[Test]
		[Category ("MobileNotWorking")]
		public void Constructor ()
		{
			var drive = new DriveInfo (Environment.OSVersion.Platform == PlatformID.Unix ? "/" : "C:\\");
			ValidateDriveInfo (drive);
			Assert.AreEqual (DriveType.Fixed, drive.DriveType);
		}

		[Test]
		public void GetDrivesNotEmpty ()
		{
			var drives = DriveInfo.GetDrives ();
			Assert.That (drives, Is.Not.Empty);
		}

		[Test]
		[Category ("MobileNotWorking")]
		public void GetDrivesValidInfo ()
		{
			var drives = DriveInfo.GetDrives ();

			foreach (var d in drives) {
				ValidateDriveInfo (d);
			}
		}

		void ValidateDriveInfo (DriveInfo d)
		{
			Assert.That (d.Name, Is.Not.Empty);
			Assert.That (d.VolumeLabel, Is.Not.Empty);
			Assert.NotNull (d.RootDirectory);

			AssertHelper.GreaterOrEqual (d.AvailableFreeSpace, 0);
			AssertHelper.GreaterOrEqual (d.TotalFreeSpace, 0);
			AssertHelper.GreaterOrEqual (d.TotalSize, 0);

			Assert.That (d.DriveFormat, Is.Not.Empty);
			if (d.DriveType == DriveType.Fixed)
				Assert.True (d.IsReady);
		}
	}
}
