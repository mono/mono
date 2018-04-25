// Compiler options: -warnaserror

using System;

public class Person
{
	public Car MyCar { get; set; }
}

public class Car
{
	public int Year { get; set; }
}

enum EE
{
	K
}

public class MainClass
{
	class Nested
	{
	}

	public static Person MyPerson1 { get; } = new Person();
	public static Person MyPerson2 = new Person();
	public const Person MyPerson3 = null;
	public static event Action Act = null;
	public static dynamic BBB = null;

	void ParameterTest (Person ParPerson)
	{
		Console.WriteLine (nameof (ParPerson.MyCar.Year));
	}

	public static int Main ()
	{
		string name;

		name = nameof (MyPerson1.MyCar.Year);
		if (name != "Year")
			return 1;

		name = nameof (MyPerson2.MyCar.Year);
		if (name != "Year")
			return 2;

		name = nameof (MyPerson3.MyCar.Year);
		if (name != "Year")
			return 3;

		name = nameof (Act.Method.MemberType);
		if (name != "MemberType")
			return 4;

		name = nameof (BBB.A.B.C);
		if (name != "C")
			return 5;

		name = nameof (EE.K.ToString);
		if (name != "ToString")
			return 6;

		name = nameof (int.ToString);
		if (name != "ToString")
			return 7;

		Person LocPerson = null;
		name = nameof (LocPerson.MyCar.Year);
		if (name != "Year")
			return 8;

		return 0;
	}
}
