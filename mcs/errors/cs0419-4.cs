// CS0419: Ambiguous reference in cref attribute `DateTime.ToString'. Assuming `System.DateTime.ToString()' but other overloads including `System.DateTime.ToString(System.IFormatProvider)' have also matched
// Line: 10
// Compiler options: -doc:dummy.xml -warnaserror -warn:4
// 
// NOTE: this error message is dependent on the order of members, so feel free to modify the message if is going not to match.

using System;

/// <summary>
/// <see cref="DateTime.ToString" />
/// </summary>
public class EntryPoint
{
	static void Main () {
	}
}

