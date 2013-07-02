// Compiler options: -unsafe

//
// Tests unsafe operators.  address-of, dereference, member access
//
using System;

unsafe struct Y {
	public int a;
	public int s;
}

unsafe class X {
	static int TestDereference ()
	{
		Y y;
		Y *z; 
		Y a;

		z = &y;
		y.a = 1;
		y.s = 2;

		a.a = z->a;
		a.s = z->s;

		if (a.a != y.a)
			return 1;
		if (a.s != y.s)
			return 2;

		return 0;
	}

	static int TestPtrAdd ()
	{
		int [] a = new int [10];
		int i;
		
		for (i = 0; i < 10; i++)
			a [i] = i;

		i = 0;
		fixed (int *b = &a [0]){ 
			int *p = b;

			for (i = 0; i < 10; i++){
				if (*p != a [i])
					return 10+i;
				p++;
			}
		}
		return 0;
	}

	static int i = 1;
	static char c = 'a';
	static long l = 123;
	static double d = 1.2;
	static float f = 1.3F;
	static short s = 4;
	
	static int TestPtrAssign ()
	{

		fixed (int *ii = &i){
			*ii = 10;
		}

		fixed (char *cc = &c){
			*cc = 'b';
		}

		fixed (long *ll = &l){
			*ll = 100;
		}

		fixed (double *dd = &d){
			*dd = 3.0;
		}

		fixed (float *ff = &f){
			*ff = 1.2F;
		}

		fixed (short *ss = &s){
			*ss = 102;
		}

		if (i != 10)
			return 100;
		if (c != 'b')
			return 101;
		if (l != 100)
			return 102;
		if (d != 3.0)
			return 103;
		if (f != 1.2F)
			return 104;
		if (s != 102)
			return 105;
		return 0;
	}

	static int TestPtrArithmetic ()
	{
		char [] array = new char [10];
		char *pb;

		array [5] = 'j';
		fixed (char *pa = array){
			pb = pa + 1;


			//
			// This one tests pointer element access
			//
			if (pa [5] != 'j')
				return 199;
			
			Console.WriteLine ("V: " + (pb - pa));
			if ((pb - pa) != 1)
				return 200;

			pb++;

			if (pb == pa)
				return 201;
			if (pb < pa)
				return 202;
			if (pa > pb)
				return 203;
			if (pa >= pb)
				return 204;
			if (pb <= pa)
				return 205;
			pb = pb - 2;
			if (pb != pa){
				Console.WriteLine ("VV: " + (pb - pa));
				return 206;
			}
		}

		return 0;
	}

	static int TestMultiple ()
	{
		char [] array = new char [10];
		int count = 0;
		
		fixed (char *pa = array, pb = array){
			count++;
		}
		if (count != 1)
			return 300;
		return 0;
	}
	
	public static int Main ()
	{
		int v;

		if ((v = TestDereference ()) != 0)
			return v;

		if ((v = TestPtrAdd ()) != 0)
			return v;

		if ((v = TestPtrAssign ()) != 0)
			return v;

		if ((v = TestPtrArithmetic ()) != 0)
			return v;

		if ((v = TestMultiple ()) != 0)
			return v;
		
		Console.WriteLine ("Ok");
		return 0;
	}
}
