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
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using NUnit.Framework;

namespace MonoTests.System.IO
{
	[TestFixture]
	public class DriveInfoTest
	{
		[Test]
		public void Constructor ()
		{
			if (!RuntimeInformation.IsOSPlatform (OSPlatform.Windows))
				Assert.Ignore ("The Jenkins builders don't have '/' mounted, just testing Windows for now.");

			var drive = new DriveInfo ("C:\\");
			ValidateDriveInfo (drive);
			Assert.AreEqual (DriveType.Fixed, drive.DriveType);
		}

		[Test]
		public void ConstructorThrowsOnNonExistingDrive ()
		{
			Assert.Throws<ArgumentException> (() => new DriveInfo ("monodriveinfotest"));
		}

		[Test]
		[Category ("NotWasm")] // it doesn't know about 'memfs' drive format
		public void ConstructorGetsValidDriveFromNonDriveString ()
		{
			if (!RuntimeInformation.IsOSPlatform (OSPlatform.Windows) && !RuntimeInformation.IsOSPlatform (OSPlatform.OSX))
				Assert.Ignore ("Some Linux-hosted CI builders don't have '/' mounted, just testing Windows and MacOS for now.");
			
			var tempPath = Path.GetTempPath ();
			var drive = new DriveInfo (tempPath);
			ValidateDriveInfo (drive);

			drive = new DriveInfo (tempPath.ToUpper());
			ValidateDriveInfo (drive);
		}

		[Test]
		public void GetDrivesNotEmpty ()
		{
			var drives = DriveInfo.GetDrives ();
			CollectionAssert.IsNotEmpty (drives);
		}

		[Test]
		[Category ("NotWasm")]
		public void GetDrivesValidInfo ()
		{
			var drives = DriveInfo.GetDrives ();

			foreach (var d in drives) {
				ValidateDriveInfo (d);
			}
		}

		void ValidateDriveInfo (DriveInfo d)
		{
			AssertHelper.IsNotEmpty (d.Name);
			AssertHelper.IsNotEmpty (d.VolumeLabel);
			Assert.NotNull (d.RootDirectory);

			if (d.DriveFormat != null) {
				Assert.AreNotEqual ("", d.DriveFormat);
				Assert.AreNotEqual (DriveType.Unknown, d.DriveType, "DriveFormat=" + d.DriveFormat);
			}

			if (d.DriveType == DriveType.Fixed) { // just consider fixed drives for now
				AssertHelper.GreaterOrEqual (d.AvailableFreeSpace, 0);
				AssertHelper.GreaterOrEqual (d.TotalFreeSpace, 0);
				AssertHelper.GreaterOrEqual (d.TotalSize, 0);
			}
		}
	}
}
