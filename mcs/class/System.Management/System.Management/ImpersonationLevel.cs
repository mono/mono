//
// System.Management.ImpersonationLevel
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//
namespace System.Management
{
	public enum ImpersonationLevel
	{
		Default = 0,
		Anonymous = 1,
		Identify = 2,
		Impersonate = 3,
		Delegate = 4
	}
}

