//
// Consts.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Andreas Nahr
//
// NOTE:
//	Ensure that every constant is defined for every version symbol!
//

// This class contains constants that are dependent on the defined symbols
// Use it to shorten and make code more maintainable in situations like:
//
//#if (NET_1_0)
//	[Designer ("System.Diagnostics.Design.ProcessDesigner, System.Design, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof (IDesigner))]
//#endif
//#if (NET_1_1)
//    	[Designer ("System.Diagnostics.Design.ProcessDesigner, System.Design, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof (IDesigner))]
//#endif
//
// by changing them into:
//
// [Designer ("System.Diagnostics.Design.ProcessDesigner, " + Consts.AssemblySystem_Design, typeof (IDesigner))]
//

internal sealed class Consts
{
	
	private Consts ()
	{
	}

#if (NET_1_0)

	public const string AssemblySystem_Windows_Forms = "System.Windows.Forms, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";

#elif (NET_2_0)

	public const string AssemblySystem_Windows_Forms = "System.Windows.Forms, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
	
#else
	// NET_1_1 is seen as default if somebody 'forgets' to specify any of the symbols
	// to ensure we are not breaking the build in this case

	public const string AssemblySystem_Windows_Forms = "System.Windows.Forms, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";

#endif

}
