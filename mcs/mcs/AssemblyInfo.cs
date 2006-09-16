using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyVersion(Consts.MonoVersion)]
[assembly: AssemblyTitle ("Mono C# Compiler")]
#if GMCS_SOURCE
[assembly: AssemblyDescription ("Mono C# Compiler with Generics")]
#else
[assembly: AssemblyDescription ("Mono C# Compiler")]
#endif
[assembly: AssemblyCopyright ("2001, 2002, 2003 Ximian, Inc.")]
[assembly: AssemblyCompany ("Ximian, Inc.")]
