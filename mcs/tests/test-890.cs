using System;

interface Iface
{
	int A { get; }

	void B ();

	event Action C;

	void D (int a, string b);

	string E { get; }
}

class X
{
	public static int Main ()
	{
		var res = typeof(Iface).GetMembers ();

		// Ensure metadata order matches source code order

		if (res [0].Name != "get_A")
			return 1;

		if (res [1].Name != "B")
			return 2;

		if (res [2].Name != "add_C")
			return 3;

		if (res [3].Name != "remove_C")
			return 4;

		if (res [4].Name != "D")
			return 5;

		if (res [5].Name != "get_E")
			return 6;

		return 0;
	}
}