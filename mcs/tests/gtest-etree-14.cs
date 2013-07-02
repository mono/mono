using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

class Person
{
	public int Age { get; set; }
}

class Repro
{
	public static int Main ()
	{
		var persons = GetPersons (new [] { new Person { Age = 25 }, new Person { Age = 21 } }, 25);
		return persons.Count () - 1;
	}

	static IEnumerable<T> GetPersons<T> (IEnumerable<T> persons, int age) where T : Person
	{
		foreach (var person in persons)
			if (Test (person, p => p.Age == age))
				yield return person;
	}

	static bool Test<T> (T t, Expression<Func<T, bool>> predicate)
	{
		return predicate.Compile () (t);
	}
}
