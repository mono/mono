// Compiler options: -r:gtest-232-lib.dll
using System;

class M
{
	public static int Main ()
	{
		A<int>.B<bool>.C<string> v_1 = Factory.Create_1 ();		
		v_1.T = 5;
		v_1.U = true;
		v_1 = new A<int>.B<bool>.C<string> ();
		
		A<int>.B2.C<string> v_2 = Factory.Create_2 ();
		v_2.T2 = -5;
		v_2 = new A<int>.B2.C<string> ();

		A<int>.B2 v_3 = Factory.Create_3 ();
		v_3.T = 99;
		v_3 = new A<int>.B2 ();
		
		return 0;
	}
}