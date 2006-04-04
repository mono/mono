using System;
using System.Reflection;

public class CtorInfoTest
{
	enum E
	{
		A = 0,
		B = 1
	}
	
	public static void Main(string[] args)
	{

		// uses static initialization
		int[] iarray = // int array, int constants
		{
			0,
			1,
			2,
			3,
			4,
			5,
			6,
		};
		
		object[] oarray = // int array, int constants
		{
			0,
			E.A,
			null,
			"A",
			new int (),
			1.1,
			-2m,
		};
		
		object[] ooarray =
		{
			null,
			new int[] { 0, 0 },
			0,
			new object[0],
		};
		
		// mcs used to throw with 7 or more elements in the array initializer
		ConstructorInfo[] ciarray = // ref array, null constants
		{
			null,
			null,
			null,
			null,
			null,
			null,
			null,
		};

		string[] scarray = // string array, string constants
		{
			"a",
			"b",
			"c",
			"d",
			"e",
			"f",
			"g",
		};

		string[] snarray = // string array, null constants
		{
			null,
			null,
			null,
			null,
			null,
			null,
			null,
		};

		decimal[] darray = // decimal constants
		{
			0M,
			1M,
			2M,
			3M,
			4M,
			5M,
			6M,
			7M,
		};

		IConvertible[] lcarray = // boxed integer constants
		{
			0,
			1,
			2,
			3,
			4,
			5,
			6,
			7,
		};
		

		System.Enum[] eatarray = // boxed enum constants
		{
			AttributeTargets.Assembly,
			AttributeTargets.Module,
			AttributeTargets.Class,
			AttributeTargets.Struct,
			AttributeTargets.Enum,
			AttributeTargets.Constructor,
			AttributeTargets.Method,
			AttributeTargets.Property,
			AttributeTargets.Field,
			AttributeTargets.Event,
			AttributeTargets.Interface,
			AttributeTargets.Parameter,
			AttributeTargets.Delegate,
			AttributeTargets.ReturnValue,
			AttributeTargets.All,
		};

		E[] atarray = // enum constants
		{
			E.A,
			E.B
		};

		
		string[] smarray = // string array, mixture
		{
			null,
			"a"
		};

		for (int i = 0; i < iarray.Length; ++i)
			Assert (i, iarray [i]);
		
		for (int i = 0; i < ciarray.Length; ++i)
			Assert (null, ciarray [i]);
		
		Assert ("a", scarray [0]);

		for (int i = 0; i < snarray.Length; ++i)
			Assert (null, snarray [i]);

		for (decimal i = 0; i < darray.Length; ++i)
			Assert (i, darray [(int)i]);

		for (int i = 0; i < lcarray.Length; ++i)
			Assert (i, lcarray [i]);

		Assert (E.A, atarray [0]);
		Assert (E.B, atarray [1]);
		
		Assert (AttributeTargets.Assembly, eatarray [0]);
		Assert (AttributeTargets.Class, eatarray [2]);
		
		Assert (null, smarray [0]);
		Assert ("a", smarray [1]);
		
	}
	
	static void Assert (object expected, object value)
	{
		if (expected == null && value == null)
			return;
		
		if (!expected.Equals (value))
			Console.WriteLine ("ERROR {0} != {1}", expected, value);
	}
}
