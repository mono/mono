class MainClass
{
	static class TypeConverter<TIn, TOut>
		where TIn : class
		where TOut : struct
	{
		public static bool Convert(TIn input)
		{
			if (input is TOut)
			{
				return true;
			}

			return false;
		}
	}

	public static int Main()
	{
		object x = 3;
		if (TypeConverter<object, double>.Convert(x))
			return 1;

		if (!TypeConverter<I, S>.Convert(new S()))
			return 2;

		return 0;
	}
}

interface I
{
}

struct S : I
{
}
