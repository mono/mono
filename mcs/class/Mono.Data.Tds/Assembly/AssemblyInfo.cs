//
// AssemblyInfo.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Ximian, Inc.  http://www.ximian.com
// (C) 2004 Novell (http://www.novell.com)
//

using System;
using System.Reflection;
using System.Runtime.InteropServices;

#if (NET_1_0)
	[assembly: AssemblyVersion ("1.0.3300.0")]
#elif (NET_2_0)
	[assembly: AssemblyVersion ("2.0.3600.0")]
#elif (NET_1_1)
	[assembly: AssemblyVersion ("1.0.5000.0")]        
#endif

/* TODO COMPLETE INFORMATION

[assembly: AssemblyTitle ("")]
[assembly: AssemblyDescription ("")]

[assembly: CLSCompliant (true)]
[assembly: AssemblyFileVersion ("0.0.0.1")]

[assembly: ComVisible (false)]

*/

[assembly: AssemblyDelaySign (true)]
[assembly: AssemblyKeyFile ("../mono.pub")]

