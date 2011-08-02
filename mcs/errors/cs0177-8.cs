// CS0177: The out parameter `parameterModifiers' must be assigned to before control leaves the current method
// Line: 17

using System;
using System.Reflection;

/// <summary>
/// MS does not report CS0177 for structs:
/// https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=304489
/// </summary>
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
