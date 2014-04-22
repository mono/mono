// Compiler options: -warnaserror

using System;
using System.Collections.Generic;
using System.Linq;

public enum SomeEnum
{
	A = 1,
	B = 2,
}

public class EnumSwitch
{
	public object SomeFunction<T> (SomeEnum endRole, object parent, IQueryable<T> input)
	{
		switch (endRole) {
		case SomeEnum.A:
			return input.Where (i => i != null);
		default:
			throw new NotImplementedException ();
		}
	}

	public static void Main ()
	{
	}
}