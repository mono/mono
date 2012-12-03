using System;
public class Test {
    public static int Main () {
        object val1  = compare_gte(0, 0);
        object val2  = compare_gte(1, 0);
        object val3  = compare_gte(0, 1);
        object val4  = compare_lte(0, 0);
        object val5  = compare_lte(1, 0);
        object val6  = compare_lte(0, 1);
        bool b;
       
	b = (true == (bool) val1);
	if (b == false)
	{
	   return 1;
	}
        
	b = (true == (bool) val2);
	if (b == false)
	{
	   return 2;
	}

	b = (true == (bool) val3);
	if (b == true)
	{
	   return 3;
	}

	b = (true == (bool) val4);
	if (b == false)
	{
	   return 4;
	}

	b = (true == (bool) val5);
	if (b == true)
	{
	   return 5;
	}

	b = (true == (bool) val6);
	if (b == false)
	{
	   return 6;
	}

	return 0;
   }

   static object compare_gte(int a, int b)
   {
       return a >= b;
   }
   static object compare_lte(int a, int b)
   {
       return a <= b;
   }
}

