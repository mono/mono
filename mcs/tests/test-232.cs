using System;
using System.Reflection;

public class CtorInfoTest
{
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
			1,
			2,
			3,
			4,
			5,
			6,
			7,
		};
		
		AttributeTargets[] atarray = // enum constants
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
	}
}
