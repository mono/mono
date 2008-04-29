enum MyEnum : byte
{
	Value_1 = 1
}

public class C
{
	public static int Main ()
	{
		MyEnum me = MyEnum.Value_1;
		MyEnum b = ~me;
		
		if (b != (MyEnum)254)
			return 1;
		
		byte r = b - me;
		if (r != 253)
			return 2;
		
		b = b - 2;
		if (b != (MyEnum)252)
			return 3;
			
		me -= MyEnum.Value_1;
		
		b = (MyEnum)255;
		b &= ~MyEnum.Value_1;
		if (b != (MyEnum)254)
			return 4;

		System.Console.WriteLine ("OK");
		return 0;
	}
}
