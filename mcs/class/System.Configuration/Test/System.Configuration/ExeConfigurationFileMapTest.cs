//
// System.Configuration.ExeConfigurationFileMapTest.cs - Unit tests
// for System.Configuration.ExeConfigurationFileMap.
//
// Author:
//	Chris Toshok  <toshok@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System;
using System.Configuration;
using NUnit.Framework;

namespace MonoTests.System.Configuration {
	using Util;

	[TestFixture]
	public class ExeConfigurationFileMapTest
	{
		[Test]
		public void Properties ()
		{
			ExeConfigurationFileMap map = new ExeConfigurationFileMap ();

			/* defaults */
			Assert.AreEqual ("", map.ExeConfigFilename, "A1");
			Assert.AreEqual ("", map.LocalUserConfigFilename, "A2");
			Assert.AreEqual ("", map.RoamingUserConfigFilename, "A2");

			/* setter */
			map.ExeConfigFilename = "foo";
			Assert.AreEqual ("foo", map.ExeConfigFilename, "A3");
			map.LocalUserConfigFilename = "bar";
			Assert.AreEqual ("bar", map.LocalUserConfigFilename, "A4");
			map.RoamingUserConfigFilename = "baz";
			Assert.AreEqual ("baz", map.RoamingUserConfigFilename, "A5");

			/* null setter */
			map.ExeConfigFilename = null;
			Assert.IsNull (map.ExeConfigFilename, "A6");
			map.LocalUserConfigFilename = null;
			Assert.IsNull (map.LocalUserConfigFilename, "A7");
			map.RoamingUserConfigFilename = null;
			Assert.IsNull (map.RoamingUserConfigFilename, "A8");
		}

		[Test]
		public void MissingRoamingFilename ()
		{
			TestUtil.RunWithTempFile (filename => {
				var map = new ExeConfigurationFileMap ();
				map.ExeConfigFilename = filename;
				
				try {
					ConfigurationManager.OpenMappedExeConfiguration (
						map, ConfigurationUserLevel.PerUserRoaming);
					Assert.Fail ("#1");
				} catch (ArgumentException) {
					;
				}
			});
		}
		
		[Test]
		public void MissingRoamingFilename2 ()
		{
			TestUtil.RunWithTempFile (filename => {
				var map = new ExeConfigurationFileMap ();
				map.LocalUserConfigFilename = filename;
				
				try {
					ConfigurationManager.OpenMappedExeConfiguration (
						map, ConfigurationUserLevel.PerUserRoamingAndLocal);
					Assert.Fail ("#1");
				} catch (ArgumentException) {
					;
				}
			});
		}
		
		[Test]
		public void MissingLocalFilename ()
		{
			TestUtil.RunWithTempFile (filename => {
				var map = new ExeConfigurationFileMap ();
				map.ExeConfigFilename = filename;
				map.RoamingUserConfigFilename = filename;
				
				try {
					ConfigurationManager.OpenMappedExeConfiguration (
						map, ConfigurationUserLevel.PerUserRoamingAndLocal);
					Assert.Fail ("#1");
				} catch (ArgumentException) {
					;
				}
			});
		}
		
		[Test]
		public void MissingExeFilename ()
		{
			TestUtil.RunWithTempFiles ((roaming,local) => {
				var map = new ExeConfigurationFileMap ();
				map.RoamingUserConfigFilename = roaming;
				map.LocalUserConfigFilename = local;
				
				try {
					ConfigurationManager.OpenMappedExeConfiguration (
						map, ConfigurationUserLevel.PerUserRoamingAndLocal);
					Assert.Fail ("#1");
				} catch (ArgumentException) {
					;
				}
			});
		}

		[Test]
		public void MissingExeFilename2 ()
		{
			TestUtil.RunWithTempFile ((machine) => {
				var map = new ExeConfigurationFileMap ();
				map.MachineConfigFilename = machine;

				try {
					ConfigurationManager.OpenMappedExeConfiguration (
						map, ConfigurationUserLevel.None);
					Assert.Fail ("#1");
				} catch (ArgumentException) {
					;
				}
			});
		}
	}
}

#endif
