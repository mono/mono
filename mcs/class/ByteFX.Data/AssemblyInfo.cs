using System.Reflection;
using System.Runtime.CompilerServices;

//
// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
//
[assembly: AssemblyTitle("ByteFX.Data.dll")]
[assembly: AssemblyDescription("ADO.Net drivers for MySql & PostgreSQL")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("ByteFX, Inc.")]
[assembly: AssemblyProduct("")]
[assembly: AssemblyCopyright("Copyright 2002-2003, ByteFX, Inc.")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]		

//
// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:

#if (NET_2_0)
	[assembly: AssemblyVersion ("2.0.3600.0")]
#elif (NET_1_1)
	[assembly: AssemblyVersion ("1.0.5000.0")]
#else
	[assembly: AssemblyVersion ("1.0.3300.0")]
#endif

[assembly: AssemblyDelaySign (true)]
[assembly: AssemblyKeyFile ("../mono.pub")]
