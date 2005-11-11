// gcs0647.cs: Error during emitting `System.Runtime.CompilerServices.InternalsVisibleToAttribute' attribute. The reason is `Friend assembly `MyAssemblyName, Version=0.0.0.0' is invalid. InternalsVisibleTo cannot have version or culture specified.'
// Line: 8

using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo ("MyAssemblyName, Version=0.0.0.0")]

public class InternalsVisibleToTest 
{
	static void Main ()
	{
	}

}

