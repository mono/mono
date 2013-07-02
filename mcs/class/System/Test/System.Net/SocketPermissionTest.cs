//
// SocketPermissionTest.cs - NUnit Test Cases for System.Net.SocketPermission
//
// Authors:
//   Lawrence Pit (loz@cable.a2000.nl)
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

#if !MOBILE

using NUnit.Framework;
using System;
using System.Net;
using System.Collections;
using System.Security;
using System.Security.Permissions;

namespace MonoTests.System.Net
{

[TestFixture]
public class SocketPermissionTest
{
	SocketPermission s1;
	SocketPermission s2;
	
	[SetUp]
        public void GetReady () 
        {
		s1 = new SocketPermission(NetworkAccess.Connect, TransportType.Tcp, "12.13.14.15", 80);
		//s1.AddPermission(NetworkAccess.Accept, TransportType.All, "localhost", 8080);
		//s1.AddPermission(NetworkAccess.Accept, TransportType.All, "123", SocketPermission.AllPorts);
		//s1.AddPermission(NetworkAccess.Accept, TransportType.All, "www.ximian.com", SocketPermission.AllPorts);
		//s1.AddPermission(NetworkAccess.Accept, TransportType.All, "120.4.3.2", SocketPermission.AllPorts);
		//s1.AddPermission(NetworkAccess.Accept, TransportType.Tcp, "www.google.com", 80);
		//s1.AddPermission(NetworkAccess.Accept, TransportType.All, "1.*.10.*.99", SocketPermission.AllPorts);
		//s1.AddPermission(NetworkAccess.Accept, TransportType.All, "128.0.0.1", SocketPermission.AllPorts);
		//s1.AddPermission(NetworkAccess.Accept, TransportType.All, "0.0.0.0", SocketPermission.AllPorts);
		s1.AddPermission(NetworkAccess.Accept, TransportType.All, "10.11.4.*", SocketPermission.AllPorts);

		s2 = new SocketPermission(NetworkAccess.Connect, TransportType.All, "12.13.14.15", 80);
		//s2.AddPermission(NetworkAccess.Accept, TransportType.All, "localhost", 8080);
		//s2.AddPermission(NetworkAccess.Accept, TransportType.All, "123", 8080);
		//s2.AddPermission(NetworkAccess.Accept, TransportType.Tcp, "www.google.com", SocketPermission.AllPorts);
		s2.AddPermission(NetworkAccess.Accept, TransportType.All, "213.*.*.*", SocketPermission.AllPorts);
		//s2.AddPermission(NetworkAccess.Accept, TransportType.All, "128.0.0.1", 9090);
		s2.AddPermission(NetworkAccess.Accept, TransportType.Tcp, "216.239.*.*", SocketPermission.AllPorts);
		//s2.AddPermission(NetworkAccess.Accept, TransportType.All, "128.0.0.1", SocketPermission.AllPorts);
		//s2.AddPermission(NetworkAccess.Accept, TransportType.Tcp, "120.4.3.2", 80);
		//s2.AddPermission(NetworkAccess.Accept, TransportType.All, "196.*.*.*", SocketPermission.AllPorts);
		//s2.AddPermission(NetworkAccess.Accept, TransportType.All, "1.*.*.*.99", SocketPermission.AllPorts);
		s2.AddPermission(NetworkAccess.Accept, TransportType.All, "10.11.*.*", 9090);
		//s2.AddPermission(NetworkAccess.Accept, TransportType.All, "10.11.4.7", SocketPermission.AllPorts);		
	}

        [Test]
        public void IsSubsetOf ()
        {
		s1 = new SocketPermission(NetworkAccess.Connect, TransportType.Tcp, "12.13.14.15", 80);
		s1.AddPermission(NetworkAccess.Accept, TransportType.All, "10.11.4.*", SocketPermission.AllPorts);
		s2 = new SocketPermission(NetworkAccess.Connect, TransportType.All, "12.13.14.15", 80);
		s2.AddPermission(NetworkAccess.Accept, TransportType.All, "10.11.*.*", 9090);
		
		Assert.IsFalse (s1.IsSubsetOf (s2), "#1");
		Assert.IsFalse (s2.IsSubsetOf (s1), "#2");

		s1 = new SocketPermission(NetworkAccess.Connect, TransportType.Tcp, "12.13.14.15", 80);
		s1.AddPermission(NetworkAccess.Accept, TransportType.All, "10.11.4.*", 9090);
		s2 = new SocketPermission(NetworkAccess.Connect, TransportType.All, "12.13.14.15", 80);
		s2.AddPermission(NetworkAccess.Accept, TransportType.All, "10.11.*.*", 9090);
		
		Assert.IsFalse (s2.IsSubsetOf (s1), "#4");
		
		s1 = new SocketPermission(NetworkAccess.Connect, TransportType.Tcp, "12.13.*.*", 80);
		s2 = new SocketPermission(NetworkAccess.Connect, TransportType.All, "12.13.14.*", 80);
		Assert.IsTrue (s1.IsSubsetOf (s2), "#5");
		Assert.IsFalse (s2.IsSubsetOf (s1), "#6");

		s1 = new SocketPermission(NetworkAccess.Connect, TransportType.Tcp, "12.13.14.15", 80);
		s1.AddPermission(NetworkAccess.Accept, TransportType.Tcp, "10.11.*.*", 9090);
		s2 = new SocketPermission(NetworkAccess.Connect, TransportType.Tcp, "12.13.14.15", 80);
		s2.AddPermission(NetworkAccess.Accept, TransportType.All, "10.11.4.*", SocketPermission.AllPorts);
		Assert.IsTrue (s1.IsSubsetOf (s2), "#7");
		Assert.IsFalse (s2.IsSubsetOf (s1), "#8");
	}
	
	[Test]
	[Category("NotDotNet")]
	public void IsSubsetOf2 ()
	{
		s1 = new SocketPermission(NetworkAccess.Connect, TransportType.Tcp, "12.13.14.15", 80);
		s1.AddPermission(NetworkAccess.Accept, TransportType.All, "10.11.4.*", 9090);
		s2 = new SocketPermission(NetworkAccess.Connect, TransportType.All, "12.13.14.15", 80);
		s2.AddPermission(NetworkAccess.Accept, TransportType.All, "10.11.*.*", 9090);
		
		Assert.IsTrue (s1.IsSubsetOf (s2), "#3: bug in MS.Net");
	}

	[Test]
	public void Intersect ()
	{
	}
	
    [Test]
	public void Union ()
	{
	}
	
    [Test]
	public void Xml ()
	{
		SecurityElement elem = s2.ToXml ();
		s1.FromXml (elem);
		Assert.IsTrue (s2.IsSubsetOf (s1) && s1.IsSubsetOf (s2), "#1");
	}
}

}

#endif
