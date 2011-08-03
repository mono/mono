// CS0110: The evaluation of the constant value for `A.B.C.X' involves a circular definition
// Line: 9

class A {
	int a;
	
	class B {
		int b;

		class C {
			int c;

			void m ()
			{
				c = 1;
			}

			enum F {
			    A, 
			    B,
			    C,
			    D = X,
			    E
			}

			const int X = Y + 1;
			const int Y = 1 + (int) F.E;
		}
	}

	static int Main (string [] args)
	{
		return 0;
	}

}
