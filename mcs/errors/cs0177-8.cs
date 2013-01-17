// CS0177: The out parameter `parameterModifiers' must be assigned to before control leaves the current method
// Line: 13

using System;
using System.Reflection;

class Program
{
	bool GetArgsForCall (object [] originalArgs, out ParameterModifier parameterModifiers)
	{
		int countOfArgs = originalArgs.Length;
		if (countOfArgs == 0)
			return false;

		parameterModifiers = new ParameterModifier (countOfArgs);
		return true;
	}
}
