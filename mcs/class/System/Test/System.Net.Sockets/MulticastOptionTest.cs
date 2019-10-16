// System.Net.Sockets.MulticastOptionTest.cs
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
	public class MulticastOptionTest
	{
		[Test] // .ctor (IPAddress)
		public void Constructor1 ()
		{
			MulticastOption option;
			IPAddress group;

			group = IPAddress.Parse ("239.255.255.250");
			option = new MulticastOption (group);
			Assert.AreSame (group, option.Group, "#A:Group");
			Assert.AreEqual (0, option.InterfaceIndex, "#A:InterfaceIndex");
			Assert.AreEqual (IPAddress.Any, option.LocalAddress, "#A:LocalAddress");

			group = IPAddress.Parse ("ff02::1");
			option = new MulticastOption (group);
			Assert.AreSame (group, option.Group, "#B:Group");
			Assert.AreEqual (0, option.InterfaceIndex, "#B:InterfaceIndex");
			Assert.AreEqual (IPAddress.Any, option.LocalAddress, "#B:LocalAddress");
		}

		[Test] // .ctor (IPAddress)
		public void Constructor1_Group_Null ()
		{
			try {
				new MulticastOption ((IPAddress) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("group", ex.ParamName, "#5");
			}
		}

		[Test] // .ctor (IPAddress, IPAddress)
		public void Constructor2 ()
		{
			MulticastOption option;
			IPAddress group;
			IPAddress mcint;

			group = IPAddress.Parse ("239.255.255.250");
			mcint = IPAddress.Any;
			option = new MulticastOption (group, mcint);
			Assert.AreSame (group, option.Group, "#A:Group");
			Assert.AreEqual (0, option.InterfaceIndex, "#A:InterfaceIndex");
			Assert.AreEqual (mcint, option.LocalAddress, "#A:LocalAddress");

			group = IPAddress.Parse ("ff02::1");
			mcint = IPAddress.IPv6Any;
			option = new MulticastOption (group, mcint);
			Assert.AreSame (group, option.Group, "#B:Group");
			Assert.AreEqual (0, option.InterfaceIndex, "#B:InterfaceIndex");
			Assert.AreEqual (mcint, option.LocalAddress, "#B:LocalAddress");
		}

		[Test] // .ctor (IPAddress, IPAddress)
		public void Constructor2_Group_Null ()
		{
			IPAddress group = null;
			IPAddress mcint = IPAddress.Any;

			try {
				new MulticastOption (group, mcint);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("group", ex.ParamName, "#5");
			}
		}

		[Test] // .ctor (IPAddress, IPAddress)
		public void Constructor2_Mcint_Null ()
		{
			IPAddress group = IPAddress.Parse ("239.255.255.250");
			IPAddress mcint = null;

			try {
				new MulticastOption (group, mcint);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("mcint", ex.ParamName, "#5");
			}
		}

		[Test] // .ctor (IPAddress, Int32)
		public void Constructor3 ()
		{
			MulticastOption option;
			IPAddress group;
			int interfaceIndex;

			group = IPAddress.Parse ("239.255.255.250");
			interfaceIndex = 0;
			option = new MulticastOption (group, interfaceIndex);
			Assert.AreSame (group, option.Group, "#A:Group");
			Assert.AreEqual (interfaceIndex, option.InterfaceIndex, "#A:InterfaceIndex");
			Assert.AreEqual (null, option.LocalAddress, "#A:LocalAddress");

			group = IPAddress.Parse ("ff02::1");
			interfaceIndex = 0;
			option = new MulticastOption (group, interfaceIndex);
			Assert.AreSame (group, option.Group, "#B:Group");
			Assert.AreEqual (interfaceIndex, option.InterfaceIndex, "#B:InterfaceIndex");
			Assert.AreEqual (null, option.LocalAddress, "#B:LocalAddress");

			group = IPAddress.Parse ("239.255.255.250");
			interfaceIndex = 124;
			option = new MulticastOption (group, interfaceIndex);
			Assert.AreSame (group, option.Group, "#C:Group");
			Assert.AreEqual (interfaceIndex, option.InterfaceIndex, "#C:InterfaceIndex");
			Assert.AreEqual (null, option.LocalAddress, "#C:LocalAddress");

			group = IPAddress.Parse ("ff02::1");
			interfaceIndex = 124;
			option = new MulticastOption (group, interfaceIndex);
			Assert.AreSame (group, option.Group, "#D:Group");
			Assert.AreEqual (interfaceIndex, option.InterfaceIndex, "#D:InterfaceIndex");
			Assert.AreEqual (null, option.LocalAddress, "#D:LocalAddress");

			group = IPAddress.Parse ("239.255.255.250");
			interfaceIndex = 0xFFFFFF;
			option = new MulticastOption (group, interfaceIndex);
			Assert.AreSame (group, option.Group, "#E:Group");
			Assert.AreEqual (interfaceIndex, option.InterfaceIndex, "#E:InterfaceIndex");
			Assert.AreEqual (null, option.LocalAddress, "#E:LocalAddress");

			group = IPAddress.Parse ("ff02::1");
			interfaceIndex = 0xFFFFFF;
			option = new MulticastOption (group, interfaceIndex);
			Assert.AreSame (group, option.Group, "#F:Group");
			Assert.AreEqual (interfaceIndex, option.InterfaceIndex, "#F:InterfaceIndex");
			Assert.AreEqual (null, option.LocalAddress, "#F:LocalAddress");
		}

		[Test] // .ctor (IPAddress, Int32)
		public void Constructor3_Group_Null ()
		{
			IPAddress group = null;

			try {
				new MulticastOption (group, int.MaxValue);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("group", ex.ParamName, "#5");
			}
		}

		[Test] // .ctor (IPAddress, Int32)
		public void Constructor3_InterfaceIndex_OutOfRange ()
		{
			IPAddress group = IPAddress.Parse ("239.255.255.250");

			try {
				new MulticastOption (group, -1);
				Assert.Fail ("#A1");
			} catch (ArgumentOutOfRangeException ex) {
				// Specified argument was out of the range of valid values
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.AreEqual ("interfaceIndex", ex.ParamName, "#A5");
			}

			try {
				new MulticastOption (group, 0x1000000);
				Assert.Fail ("#B1");
			} catch (ArgumentOutOfRangeException ex) {
				// Specified argument was out of the range of valid values
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.AreEqual ("interfaceIndex", ex.ParamName, "#B5");
			}
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Group ()
		{
			IPAddress group;
			IPAddress local;
			MulticastOption option;

			local = Dns.GetHostEntry ("localhost").AddressList [0];
			group = IPAddress.Parse ("239.255.255.250");
			option = new MulticastOption (group, local);
			group = IPAddress.Parse ("224.0.0.23");
			option.Group = group;
			Assert.AreSame (group, option.Group, "#A1");
			Assert.AreSame (local, option.LocalAddress, "#A2");
			group = IPAddress.Parse ("239.255.255.250");
			option.Group = group;
			Assert.AreSame (group, option.Group, "#B1");
			Assert.AreSame (local, option.LocalAddress, "#B2");
			group = IPAddress.Parse ("ff02::1");
			option.Group = group;
			Assert.AreSame (group, option.Group, "#C1");
			Assert.AreSame (local, option.LocalAddress, "#C2");
			option.Group = null;
			Assert.IsNull (option.Group, "#D1");
			Assert.AreSame (local, option.LocalAddress, "#D2");
			option = new MulticastOption (group, 5);
			group = IPAddress.Parse ("224.0.0.23");
			option.Group = group;
			Assert.AreSame (group, option.Group, "#E1");
			Assert.AreEqual (5, option.InterfaceIndex, "#E2");
			Assert.IsNull (option.LocalAddress, "#E3");
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void InterfaceIndex ()
		{
			IPAddress group;
			IPAddress local;
			MulticastOption option;
		
			group = IPAddress.Parse ("239.255.255.250");
			option = new MulticastOption (group, 10);
			option.InterfaceIndex = 0;
			Assert.AreSame (group, option.Group, "#A1");
			Assert.AreEqual (0, option.InterfaceIndex, "#A2");
			Assert.IsNull (option.LocalAddress, "#A3");
			option.InterfaceIndex = 124;
			Assert.AreSame (group, option.Group, "#B1");
			Assert.AreEqual (124, option.InterfaceIndex, "#B2");
			Assert.IsNull (option.LocalAddress, "#B3");
			option.InterfaceIndex = 0xFFFFFF;
			Assert.AreSame (group, option.Group, "#C1");
			Assert.AreEqual (0xFFFFFF, option.InterfaceIndex, "#C2");
			Assert.IsNull (option.LocalAddress, "#C3");

			local = Dns.GetHostEntry ("localhost").AddressList [0];
			option = new MulticastOption (group, local);
			option.InterfaceIndex = 10;
			Assert.AreSame (group, option.Group, "#D1");
			Assert.AreEqual (10, option.InterfaceIndex, "#D2");
			Assert.IsNull (option.LocalAddress, "#D3");
		}

		[Test]
		public void InterfaceIndex_Value_OutOfRange ()
		{
			IPAddress group = IPAddress.Parse ("239.255.255.250");
			MulticastOption option = new MulticastOption (group, 10);

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
				option.InterfaceIndex = 0x1000000;
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

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void LocalAddress ()
		{
			IPAddress group;
			IPAddress local;
			MulticastOption option;

			local = Dns.GetHostEntry ("localhost").AddressList [0];
			group = IPAddress.Parse ("239.255.255.250");
			option = new MulticastOption (group, local);
			local = IPAddress.Loopback;
			option.LocalAddress = local;
			Assert.AreSame (group, option.Group, "#A1");
			Assert.AreEqual (0, option.InterfaceIndex, "#A2");
			Assert.AreSame (local, option.LocalAddress, "#A3");
			local = Dns.GetHostEntry ("localhost").AddressList [0];
			option.LocalAddress = local;
			Assert.AreSame (group, option.Group, "#B1");
			Assert.AreEqual (0, option.InterfaceIndex, "#B2");
			Assert.AreSame (local, option.LocalAddress, "#B3");
			option.LocalAddress = null;
			Assert.AreSame (group, option.Group, "#C1");
			Assert.AreEqual (0, option.InterfaceIndex, "#C2");
			Assert.IsNull (option.LocalAddress, "#C3");
			option = new MulticastOption (group, 5);
			local = IPAddress.Loopback;
			option.LocalAddress = local;
			Assert.AreSame (group, option.Group, "#D1");
			Assert.AreEqual (0, option.InterfaceIndex, "#D2");
			Assert.AreSame (local, option.LocalAddress, "#D3");
		}
	}
}
