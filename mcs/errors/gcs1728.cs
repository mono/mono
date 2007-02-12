// CS1728: Cannot use method `int?.GetValueOrDefault()' as delegate creation expression because it is member of Nullable type
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
