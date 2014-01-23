using System;

struct Result {
	public int res;
	// big enough that it won't be returned in registers
	double duh;
	long bah;

	public Result (int val) {
		res = val;
		bah = val;
		duh = val;
	}
}

class Class1
{
	static int AddABunchOfInts (__arglist)
	{
		int result = 0;

		System.ArgIterator iter = new System.ArgIterator (__arglist);
		int argCount = iter.GetRemainingCount();

		for (int i = 0; i < argCount; i++) {
			System.TypedReference typedRef = iter.GetNextArg();
			result += (int)TypedReference.ToObject( typedRef );
		}
		
		return result;
	}

	static int AddASecondBunchOfInts (int a, __arglist)
	{
		int result = 0;

		System.ArgIterator iter = new System.ArgIterator (__arglist);
		int argCount = iter.GetRemainingCount();

		for (int i = 0; i < argCount; i++) {
			System.TypedReference typedRef = iter.GetNextArg();
			result += (int)TypedReference.ToObject( typedRef );
		}
		
		return result;
	}

	static Result VtAddABunchOfInts (__arglist)
	{
		int result = 0;

		System.ArgIterator iter = new System.ArgIterator (__arglist);
		int argCount = iter.GetRemainingCount();

		for (int i = 0; i < argCount; i++) {
			System.TypedReference typedRef = iter.GetNextArg();
			result += (int)TypedReference.ToObject( typedRef );
		}
		
		return new Result (result);
	}

	static Result VtAddASecondBunchOfInts (int a, __arglist)
	{
		int result = 0;

		System.ArgIterator iter = new System.ArgIterator (__arglist);
		int argCount = iter.GetRemainingCount();

		for (int i = 0; i < argCount; i++) {
			System.TypedReference typedRef = iter.GetNextArg();
			result += (int)TypedReference.ToObject( typedRef );
		}
		
		return new Result (result);
	}

	int InstAddABunchOfInts (__arglist)
	{
		int result = 0;

		System.ArgIterator iter = new System.ArgIterator (__arglist);
		int argCount = iter.GetRemainingCount();

		for (int i = 0; i < argCount; i++) {
			System.TypedReference typedRef = iter.GetNextArg();
			result += (int)TypedReference.ToObject( typedRef );
		}
		
		return result;
	}

	int InstAddASecondBunchOfInts (int a, __arglist)
	{
		int result = 0;

		System.ArgIterator iter = new System.ArgIterator (__arglist);
		int argCount = iter.GetRemainingCount();

		for (int i = 0; i < argCount; i++) {
			System.TypedReference typedRef = iter.GetNextArg();
			result += (int)TypedReference.ToObject( typedRef );
		}
		
		return result;
	}

	Result InstVtAddABunchOfInts (__arglist)
	{
		int result = 0;

		System.ArgIterator iter = new System.ArgIterator (__arglist);
		int argCount = iter.GetRemainingCount();

		for (int i = 0; i < argCount; i++) {
			System.TypedReference typedRef = iter.GetNextArg();
			result += (int)TypedReference.ToObject( typedRef );
		}
		
		return new Result (result);
	}

	Result InstVtAddASecondBunchOfInts (int a, __arglist)
	{
		int result = 0;

		System.ArgIterator iter = new System.ArgIterator (__arglist);
		int argCount = iter.GetRemainingCount();

		for (int i = 0; i < argCount; i++) {
			System.TypedReference typedRef = iter.GetNextArg();
			result += (int)TypedReference.ToObject( typedRef );
		}
		
		return new Result (result);
	}

	public static int Main (string[] args)
	{
		int result = AddABunchOfInts (__arglist ( 2, 3, 4 ));
		Console.WriteLine ("Answer: {0}", result);

		if (result != 9)
			return 1;

		result = AddASecondBunchOfInts (16, __arglist ( 2, 3, 4 ));
		Console.WriteLine ("Answer: {0}", result);

		if (result != 9)
			return 2;

		Class1 s = new Class1 ();

		result = s.InstAddABunchOfInts (__arglist ( 2, 3, 4, 5 ));
		Console.WriteLine ("Answer: {0}", result);

		if (result != 14)
			return 3;

		result = s.InstAddASecondBunchOfInts (16, __arglist ( 2, 3, 4, 5, 6 ));
		Console.WriteLine ("Answer: {0}", result);

		if (result != 20)
			return 4;

		result = s.InstVtAddABunchOfInts (__arglist ( 2, 3, 4, 5 )).res;
		Console.WriteLine ("Answer: {0}", result);

		if (result != 14)
			return 5;

		result = s.InstVtAddASecondBunchOfInts (16, __arglist ( 2, 3, 4, 5, 6 )).res;
		Console.WriteLine ("Answer: {0}", result);

		if (result != 20)
			return 6;

		result = VtAddABunchOfInts (__arglist ( 2, 3, 4, 5, 1 )).res;
		Console.WriteLine ("Answer: {0}", result);

		if (result != 15)
			return 7;

		result = VtAddASecondBunchOfInts (16, __arglist ( 2, 3, 4, 5, 6, 1 )).res;
		Console.WriteLine ("Answer: {0}", result);

		if (result != 21)
			return 8;

		result = s.InstAddABunchOfInts (__arglist ( ));
		if (result != 0)
			return 9;
		
		return 0;
	}
}
