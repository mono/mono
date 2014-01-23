// Compiler options: -r:test-319-lib.dll

using System;
using System.Runtime.CompilerServices;

public class ConstFields
{
	public const decimal ConstDecimal1 = 314159265358979323846m;
	public static readonly decimal ConstDecimal2 = -314159265358979323846m;
	public const decimal ConstDecimal3 = -3;
	public const decimal ConstDecimal4 = 0;
	public const decimal MaxValue = 79228162514264337593543950335m;
	public const decimal MaxLongValue = long.MaxValue;

	// TODO: check this value
	static readonly Decimal MaxValueDiv10 = MaxValue / 10;

	static decimal DecimalValue = -90;
	const decimal SmallConstValue = .02M;

	public static int Main ()
	{
		Type t = typeof (ConstFields);
		DecimalConstantAttribute a = (DecimalConstantAttribute) t.GetField ("ConstDecimal3").GetCustomAttributes (typeof (DecimalConstantAttribute), false)[0];
		if (a.Value != ConstDecimal3)
			return 1;

		a = (DecimalConstantAttribute) t.GetField ("ConstDecimal1").GetCustomAttributes (typeof (DecimalConstantAttribute), false)[0];
		if (a.Value != 314159265358979323846m)
			return 2;

		if (ConstDecimal1 != (-1) * ConstDecimal2)
			return 3;

		if (!(SmallConstValue < 1 && SmallConstValue > 0))
			return 4;

		decimal ld = long.MaxValue;
		Console.WriteLine (ld);

		// THIS IS TEST TOO
		Console.WriteLine (C.D);
		Console.WriteLine (Decimal.One);
		Console.WriteLine (DecimalValue);
		Console.WriteLine (Decimal.MaxValue);

		Console.WriteLine ("Success");
		return 0;
	}
}
