//
// System.Web.Util.ICalls
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//

using System.Runtime.CompilerServices;
namespace System.Web.Util
{
	class ICalls
	{
		private ICalls () {}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static public string GetMachineConfigPath ();

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static public string GetMachineInstallDirectory ();
	}
}

