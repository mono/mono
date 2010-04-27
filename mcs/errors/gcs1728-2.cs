// CS1728: Cannot create delegate from method `long?.GetValueOrDefault()' because it is a member of System.Nullable<T> type
// Line: 10

using System;

class C
{
	public static void Main ()
	{
		Func<long> a = new long?().GetValueOrDefault;
	}
}
