// CS1728: Cannot create delegate from method `int?.GetValueOrDefault()' because it is a member of System.Nullable<T> type
// Line: 14

using System;

class C
{
	delegate int Test ();
	event Test MyEvent;

	void Error ()
	{
		int? i = 0;
		MyEvent += new Test (i.GetValueOrDefault);
	}
}
