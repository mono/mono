//
// SocketPermissionTest.cs - NUnit Test Cases for System.Net.SocketPermission
//
// Author:
//   Lawrence Pit (loz@cable.a2000.nl)
//

using NUnit.Framework;
using System;
using System.Net;
using System.Collections;
using System.Security;
using System.Security.Permissions;

namespace MonoTests.System.Net
{

public class SocketPermissionTest : TestCase
{
	SocketPermission s1;
	SocketPermission s2;
	
        public SocketPermissionTest () :
                base ("[MonoTests.System.Net.SocketPermissionTest]") {}

        public SocketPermissionTest (string name) : base (name) {}

        protected override void SetUp () 
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

        protected override void TearDown () {}

        public static ITest Suite
        {
                get {
                        return new TestSuite (typeof (SocketPermissionTest));
                }
        }
        
        public void TestIsSubsetOf ()
        {
		s1 = new SocketPermission(NetworkAccess.Connect, TransportType.Tcp, "12.13.14.15", 80);
		s1.AddPermission(NetworkAccess.Accept, TransportType.All, "10.11.4.*", SocketPermission.AllPorts);
		s2 = new SocketPermission(NetworkAccess.Connect, TransportType.All, "12.13.14.15", 80);
		s2.AddPermission(NetworkAccess.Accept, TransportType.All, "10.11.*.*", 9090);
		
		Assert ("#1", !s1.IsSubsetOf (s2));
		Assert ("#2", !s2.IsSubsetOf (s1));

		s1 = new SocketPermission(NetworkAccess.Connect, TransportType.Tcp, "12.13.14.15", 80);
		s1.AddPermission(NetworkAccess.Accept, TransportType.All, "10.11.4.*", 9090);
		s2 = new SocketPermission(NetworkAccess.Connect, TransportType.All, "12.13.14.15", 80);
		s2.AddPermission(NetworkAccess.Accept, TransportType.All, "10.11.*.*", 9090);
		
		Assert ("#3: bug in MS.Net", s1.IsSubsetOf (s2));
		Assert ("#4", !s2.IsSubsetOf (s1));
		
		s1 = new SocketPermission(NetworkAccess.Connect, TransportType.Tcp, "12.13.*.*", 80);
		s2 = new SocketPermission(NetworkAccess.Connect, TransportType.All, "12.13.14.*", 80);
		Assert ("#5", !s1.IsSubsetOf (s2));
		Assert ("#6", !s2.IsSubsetOf (s1));

		s1 = new SocketPermission(NetworkAccess.Connect, TransportType.Tcp, "12.13.14.15", 80);
		s1.AddPermission(NetworkAccess.Accept, TransportType.Tcp, "10.11.*.*", 9090);
		s2 = new SocketPermission(NetworkAccess.Connect, TransportType.Tcp, "12.13.14.15", 80);
		s2.AddPermission(NetworkAccess.Accept, TransportType.All, "10.11.4.*", SocketPermission.AllPorts);
		Assert ("#7", !s1.IsSubsetOf (s2));
		Assert ("#8", s2.IsSubsetOf (s1));
	}
	
	public void TestIntersect ()
	{
	}
	
	public void TestUnion ()
	{
	}
	
	public void TestXml ()
	{
		SecurityElement elem = s2.ToXml ();
		s1.FromXml (elem);
		Assert ("#1", s2.IsSubsetOf (s1) && s1.IsSubsetOf (s2));
	}
}

}

