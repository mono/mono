using System;   
   
public class Testing   
{   
        enum Fruit { Apple, Banana, Cherry };   

	static int TestEnumInit (Fruit f)
	{
		Fruit [] testedFruits = { f };

		if (f != Fruit.Apple)
			return 1;
		return 0;
	}

        public static int Main()   
        {   
                Fruit[] pieFillings = { Fruit.Apple, Fruit.Banana, Fruit.Cherry };

		if (pieFillings [0] != Fruit.Apple)
			return 1;
		if (pieFillings [1] != Fruit.Banana)
			return 2;
		if (pieFillings [2] != Fruit.Cherry)
			return 3;

		if (TestEnumInit (Fruit.Apple) != 0)
			return 4;

		return 0;
        }          
}
