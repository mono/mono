//
// AssemblyInfo.cs
//
// Author:
// 	Gonzalo Paniagua (gonzalo@ximian.com)
//
// (C) 2003 Novell, Inc.  http://www.novell.com
//

using System.Reflection;
using System.Runtime.CompilerServices;

#if (NET_2_0)
	[assembly: AssemblyVersion ("2.0.3600.0")]
#elif (NET_1_1)
	[assembly: AssemblyVersion ("1.0.5000.0")]
#else
	[assembly: AssemblyVersion ("1.0.3300.0")]
#endif
[assembly: AssemblyTitle("Mono.Http.dll")]
[assembly: AssemblyDescription("Http and ASP.NET utilities")]
[assembly: AssemblyConfiguration("Development version")]
[assembly: AssemblyCompany("MONO development team")]
[assembly: AssemblyProduct("MONO CLI")]
[assembly: AssemblyCopyright("(c) 2003 Various Authors")]

[assembly: AssemblyDelaySign(true)]
[assembly: AssemblyKeyFile("../mono.pub")]
