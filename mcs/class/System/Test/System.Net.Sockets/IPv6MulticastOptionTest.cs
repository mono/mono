// System.Net.Sockets.IPv6MulticastOptionTest.cs
//
// Authors:
//	Gert Driesen (drieseng@users.sourceforge.net)
//
// Copyright (c) 2009 Gert Driesen
//

using System;
using System.Net;
using System.Net.Sockets;

using NUnit.Framework;

namespace MonoTests.System.Net.Sockets
{
	[TestFixture]
	[Category("NotWasm")]
	public class IPv6MulticastOptionTest
	{
		[Test] // .ctor (IPAddress)
		public void Constructor1 ()
		{
			IPv6MulticastOption option;
			IPAddress group;

			group = IPAddress.Parse ("ff02::1");
			option = new IPv6MulticastOption (group);
			Assert.AreSame (group, option.Group, "#A:Group");
			Assert.AreEqual (0, option.InterfaceIndex, "#A:InterfaceIndex");

			group = IPAddress.Parse ("224.0.0.23");
			option = new IPv6MulticastOption (group);
			Assert.AreSame (group, option.Group, "#B:Group");
			Assert.AreEqual (0, option.InterfaceIndex, "#B:InterfaceIndex");
		}

		[Test] // .ctor (IPAddress)
		public void Constructor1_Group_Null ()
		{
			try {
				new IPv6MulticastOption ((IPAddress) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("group", ex.ParamName, "#5");
			}
		}

		[Test] // .ctor (IPAddress, Int64)
		public void Constructor2 ()
		{
			IPv6MulticastOption option;
			IPAddress group;
			long interfaceIndex;

			group = IPAddress.Parse ("239.255.255.250");
			interfaceIndex = 0;
			option = new IPv6MulticastOption (group, interfaceIndex);
			Assert.AreSame (group, option.Group, "#A:Group");
			Assert.AreEqual (interfaceIndex, option.InterfaceIndex, "#A:InterfaceIndex");

			group = IPAddress.Parse ("ff02::1");
			interfaceIndex = 0;
			option = new IPv6MulticastOption (group, interfaceIndex);
			Assert.AreSame (group, option.Group, "#B:Group");
			Assert.AreEqual (interfaceIndex, option.InterfaceIndex, "#B:InterfaceIndex");

			group = IPAddress.Parse ("239.255.255.250");
			interfaceIndex = 124;
			option = new IPv6MulticastOption (group, interfaceIndex);
			Assert.AreSame (group, option.Group, "#C:Group");
			Assert.AreEqual (interfaceIndex, option.InterfaceIndex, "#C:InterfaceIndex");

			group = IPAddress.Parse ("ff02::1");
			interfaceIndex = 124;
			option = new IPv6MulticastOption (group, interfaceIndex);
			Assert.AreSame (group, option.Group, "#D:Group");
			Assert.AreEqual (interfaceIndex, option.InterfaceIndex, "#D:InterfaceIndex");

			group = IPAddress.Parse ("239.255.255.250");
			interfaceIndex = 0xFFFFFFFF;
			option = new IPv6MulticastOption (group, interfaceIndex);
			Assert.AreSame (group, option.Group, "#E:Group");
			Assert.AreEqual (interfaceIndex, option.InterfaceIndex, "#E:InterfaceIndex");

			group = IPAddress.Parse ("ff02::1");
			interfaceIndex = 0xFFFFFFFF;
			option = new IPv6MulticastOption (group, interfaceIndex);
			Assert.AreSame (group, option.Group, "#F:Group");
			Assert.AreEqual (interfaceIndex, option.InterfaceIndex, "#F:InterfaceIndex");
		}

		[Test] // .ctor (IPAddress, Int64)
		public void Constructor2_Group_Null ()
		{
			IPAddress group = null;

			try {
				new IPv6MulticastOption (group, 0);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("group", ex.ParamName, "#5");
			}
		}

		[Test] // .ctor (IPAddress, Int64)
		public void Constructor2_Ifindex_OutOfRange ()
		{
			IPAddress group = IPAddress.Parse ("ff02::1");

			try {
				new IPv6MulticastOption (group, -1);
				Assert.Fail ("#A1");
			} catch (ArgumentOutOfRangeException ex) {
				// Specified argument was out of the range of valid values
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.AreEqual ("ifindex", ex.ParamName, "#A5");
			}

			try {
				new IPv6MulticastOption (group, 0x100000000);
				Assert.Fail ("#B1");
			} catch (ArgumentOutOfRangeException ex) {
				// Specified argument was out of the range of valid values
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.AreEqual ("ifindex", ex.ParamName, "#B5");
			}
		}

		[Test]
		public void Group ()
		{
			IPAddress group;
			IPv6MulticastOption option;

			group = IPAddress.Parse ("239.255.255.250");
			option = new IPv6MulticastOption (group, 5L);
			group = IPAddress.Parse ("224.0.0.23");
			option.Group = group;
			Assert.AreSame (group, option.Group, "#A1");
			Assert.AreEqual (5L, option.InterfaceIndex, "#A2");
			group = IPAddress.Parse ("239.255.255.250");
			option.Group = group;
			Assert.AreSame (group, option.Group, "#B1");
			Assert.AreEqual (5L, option.InterfaceIndex, "#B2");
			group = IPAddress.Parse ("ff02::1");
			option.Group = group;
			Assert.AreSame (group, option.Group, "#C1");
			Assert.AreEqual (5L, option.InterfaceIndex, "#C2");
		}

		[Test]
		public void Group_Value_Null ()
		{
			IPAddress group = IPAddress.Parse ("239.255.255.250");
			IPv6MulticastOption option = new IPv6MulticastOption (group, 10);

			try {
				option.Group = null;
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("value", ex.ParamName, "#5");
			}
		}

		[Test]
		public void InterfaceIndex ()
		{
			IPAddress group;
			IPv6MulticastOption option;

			group = IPAddress.Parse ("239.255.255.250");
			option = new IPv6MulticastOption (group, 10);
			option.InterfaceIndex = 0;
			Assert.AreSame (group, option.Group, "#A1");
			Assert.AreEqual (0, option.InterfaceIndex, "#A2");
			option.InterfaceIndex = 124;
			Assert.AreSame (group, option.Group, "#B1");
			Assert.AreEqual (124, option.InterfaceIndex, "#B2");
			option.InterfaceIndex = 0xFFFFFFFF;
			Assert.AreSame (group, option.Group, "#C1");
			Assert.AreEqual (0xFFFFFFFF, option.InterfaceIndex, "#C3");
		}

		[Test]
		public void InterfaceIndex_Value_OutOfRange ()
		{
			IPAddress group = IPAddress.Parse ("239.255.255.250");
			IPv6MulticastOption option = new IPv6MulticastOption (group, 10);

			try {
				option.InterfaceIndex = -1;
				Assert.Fail ("#A1");
			} catch (ArgumentOutOfRangeException ex) {
				// Specified argument was out of the range of valid values
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.AreEqual ("value", ex.ParamName, "#A5");
			}

			try {
				option.InterfaceIndex = 0x100000000;
				Assert.Fail ("#B1");
			} catch (ArgumentOutOfRangeException ex) {
				// Specified argument was out of the range of valid values
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.AreEqual ("value", ex.ParamName, "#B5");
			}

			// ensure original value was retained
			Assert.AreEqual (10, option.InterfaceIndex, "#C");
		}
	}
}
