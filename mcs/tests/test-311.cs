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

	[SecurityPermission (SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlEvidence, UnmanagedCode=true)]
	[SecurityPermission (SecurityAction.Demand, Flags = SecurityPermissionFlag.AllFlags, UnmanagedCode=true)]
	static public int Main (string[] args)
	{
		Type program = typeof (Program);
		if (program.GetCustomAttributes (true).Length != 0)
			return 1;

		if (program.GetConstructor (System.Type.EmptyTypes).GetCustomAttributes (true).Length != 0)
			return 2;

		if (program.GetProperty ("Message").GetSetMethod ().GetCustomAttributes (true).Length != 0)
			return 3;

		if (program.GetMethod ("Main").GetCustomAttributes (true).Length != 0)
			return 4;

		Console.WriteLine (Message);
		return 0;
	}
}
