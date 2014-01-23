using System;
using System.Security;
using System.Security.Permissions;

[assembly: SecurityPermission (SecurityAction.RequestMinimum, Execution=true)]
[assembly: SecurityPermission (SecurityAction.RequestOptional, Unrestricted=true)]
[assembly: SecurityPermission (SecurityAction.RequestRefuse, SkipVerification=true)]

[SecurityPermission (SecurityAction.LinkDemand, ControlPrincipal=true)]
struct LinkDemandStruct {
	internal string Info;
}

[SecurityPermission (SecurityAction.Demand, ControlAppDomain=true)]
public class Program {

	private static string _message = "Hello Mono!";
	private LinkDemandStruct info;

	[SecurityPermission (SecurityAction.InheritanceDemand, ControlAppDomain=true)]
	public Program () {
		info = new LinkDemandStruct ();
		info.Info = ":-)";
	}

	public static string Message {
		[SecurityPermission (SecurityAction.PermitOnly, ControlEvidence=true)]
		get { return _message; }
		[SecurityPermission (SecurityAction.Assert, ControlThread=true)]
		set { _message = value; }
	}

	[SecurityPermission (SecurityAction.Deny, UnmanagedCode=true)]
	private bool DenyMethod () {
		return false;
	}
	
	[SiteIdentityPermission (SecurityAction.PermitOnly)]
	[PermissionSet (SecurityAction.PermitOnly, Unrestricted=true)]
	[PermissionSet (SecurityAction.PermitOnly, Unrestricted=false)]
	public void Test2 ()
	{
	}

	[PermissionSet (SecurityAction.PermitOnly, Unrestricted=true)]
	[PermissionSet (SecurityAction.PermitOnly, Unrestricted=false)]
	public void Test3 ()
	{
	}
	
	[EnvironmentPermission (SecurityAction.Demand, Unrestricted=true)]
	public void Test4 ()
	{
	}
	
	[SecurityPermission (SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlEvidence, UnmanagedCode=true)]
	[SecurityPermission (SecurityAction.Demand, Flags = SecurityPermissionFlag.AllFlags, UnmanagedCode=true)]
	public static int Main (string[] args)
	{
		// TODO: this will not be working for .NET 2.0 as attributes are decoded back
		Type program = typeof (Program);
		
		if (program.GetCustomAttributes (true).Length != 0)
			return 1;
		
		if (program.GetConstructor (System.Type.EmptyTypes).GetCustomAttributes (true).Length != 0)
			return 2;

		if (program.GetProperty ("Message").GetSetMethod ().GetCustomAttributes (true).Length != 0)
			return 3;

		if (program.GetMethod ("Main").GetCustomAttributes (true).Length != 0)
			return 4;

		if (program.GetMethod ("Test2").GetCustomAttributes (true).Length != 0)
			return 5;		
		
		Type test2 = typeof (Test2);
		if (test2.GetCustomAttributes (true).Length != 0)
			return 6;
		
		Console.WriteLine ("OK");
		return 0;
	}
}

[SecurityPermission (SecurityAction.Demand, ControlAppDomain=true)]
public partial class Test2 {}

[SecurityPermission (SecurityAction.Demand, ControlAppDomain=true)]
public partial class Test2 {}
