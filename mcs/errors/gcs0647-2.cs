// gcs0647-2.cs: Error during emitting `System.Runtime.CompilerServices.InternalsVisibleToAttribute' attribute. The reason is `Friend assembly `AssemblySomething, Culture=en-US' is invalid. InternalsVisibleTo cannot have version or culture specified.'
// Line: 8

using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo ("AssemblySomething, Culture=en-US")]

public class InternalsVisibleToTest 
{
	static void Main ()
	{
	}
}

