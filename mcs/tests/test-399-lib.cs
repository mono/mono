// Compiler options: -t:library

using System;

public struct Result {
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

public class Vararg
{
	public static int AddABunchOfInts (__arglist)
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

	public static int AddASecondBunchOfInts (int a, __arglist)
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

	public static Result VtAddABunchOfInts (__arglist)
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

	public static Result VtAddASecondBunchOfInts (int a, __arglist)
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

	public int InstAddABunchOfInts (__arglist)
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

	public int InstAddASecondBunchOfInts (int a, __arglist)
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

	public Result InstVtAddABunchOfInts (__arglist)
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

	public Result InstVtAddASecondBunchOfInts (int a, __arglist)
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
}
