// CS0171: Field `S.value' must be fully assigned before control leaves the constructor
// Line: 10

using System;

struct S
{
	internal string value;

	public S (int arg)
	{
		if (arg > 0) {
			return;
		}

		throw new ApplicationException ();
	}
}
