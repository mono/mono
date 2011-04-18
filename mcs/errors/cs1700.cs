// CS1700: Assembly reference `MyAssemblyName, Version=' is invalid and cannot be resolved
// Line: 8
// Compiler options: -warnaserror -warn:3

using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo ("MyAssemblyName, Version=")]

public class InternalsVisibleToTest 
{
	static void Main ()
	{
	}

}

