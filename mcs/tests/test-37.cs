//
// This test excercises array access on single dimension, multi-dimension
// and jagged arrays.
//
using System;

class X {
	static void m (int [] a)
	{
		a [0] = 0xdead;
	}

	static int test_int_single_dim ()
	{
		int [] a = new int [10];
		int i;
		
		for (i = 0; i < 10; i++)
			a [i] = i;

		m (a);

		if (a [0] != 0xdead)
			return 1;

		a [0] = 0;
		for (i = 9; i >= 0; i--){
			if (a [i] != i)
				return 2;
		}
		return 0;
	}

	static int simple_test_double_dim ()
	{
		int [,] b = new int [10, 10];

		b [0, 0] = 1;
		b [4, 4] = 1;

		if (b [0, 0] != b [4, 4])
			return 20;
		if (b [1, 1] != b [5, 5])
			return 21;

		return 0;
	}
	
//	  static void dd (int [,] b)
//	  {
//		  int i, j;
//
//		  for (i = 0; i < 10; i++)
//			  for (j = 0; j < 10; j++)
//				  b [i, j] = b [i, j] + 1;
//	  }
//
//	  static int test_int_double_dim ()
//	  {
//		  int [,] b = new int [10,10];
//		  int i, j;
//		  
//		  for (i = 0; i < 10; i++)
//			  for (j = 0; j < 10; j++)
//				  b [i,j] = i * 10 + j;
//
//		  dd (b);
//		  
//		  for (i = 0; i < 10; i++)
//			  for (j = 0; j < 10; j++)
//				  if (b [i,j] != i *10 + j + 1){
//					  Console.WriteLine ("Expecting " + (i * 10 + j + 1) + "got: " + b [i,j]);
//					  return 10;
//				  }
//		  
//		  return 0;
//	  }

//	  static int test_jagged ()
//	  {
//		  int [][] a = new int [10][];
//		  int i;
//		  
//		  for (i = 0; i < 10; i++){
//			  if (a [i] != null)
//				  return 20;
//			  
//			  a [i] = new int [10];
//
//			  for (int j = 0; j < 10; j++){
//				  int q;
//				  a [i][j] = j * 10;
//				  q = a [i][j] = j * 10;
//				  
//				  a [i][j]++;
//
//				  if (a [i][j] != q + 1)
//					  return 21;
//			  }
//		  }
//
//		  return 0;
//	  }

	public static int Main ()
	{
		int v;
		
		Console.WriteLine ("hello");
		return 0;

#if FIXME
		v = test_int_single_dim ();

		if (v != 0)
			return v;

		//		v = test_int_double_dim ();
		//		if (v != 0)
		//			return v;
		//
		//		v = test_jagged ();

		v = simple_test_double_dim ();
		if (v != 0)
			return v;

		int [] a = new int [10];
		int i;
		for (i = 0; i < 10; i++){
			a [i] = i;
			//			a [i]++;
			// Console.WriteLine ("Should be: " + (i + 1) + " it is = " + a [i]);
		}
		return 0;
#endif
	}
}	
