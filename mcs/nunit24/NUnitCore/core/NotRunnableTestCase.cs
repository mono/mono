// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

using System;
using System.Reflection;

namespace NUnit.Core
{
	/// <summary>
	/// Summary description for NotRunnableTestCase.
	/// </summary>
	public class NotRunnableTestCase : TestCase
	{
		public NotRunnableTestCase(MethodInfo method, string reason) : base(method.DeclaringType.FullName, method.Name)
		{
			RunState = RunState.NotRunnable;
			IgnoreReason = reason;
		}

		public NotRunnableTestCase(MethodInfo method) : base(method.DeclaringType.FullName, method.Name)
		{
			string reason;

			if (method.IsAbstract)
				reason = "it must not be abstract";
			else if (method.IsStatic)
				reason = "it must be an instance method";
			else if (!method.IsPublic)
				reason = "it must be a public method";
			else if (method.GetParameters().Length != 0)
				reason = "it must not have parameters";
			else if (!method.ReturnType.Equals(typeof(void)))
				reason = "it must return void";
			else
				reason = "reason not known";

			RunState = RunState.NotRunnable;
			IgnoreReason = String.Format("Method {0}'s signature is not correct: {1}.", method.Name, reason);
		}

		public override void Run(TestCaseResult result)
		{
			result.Ignore(base.IgnoreReason);
		}
	}
}

