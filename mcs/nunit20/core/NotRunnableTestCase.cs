#region Copyright (c) 2002-2003, James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole, Philip A. Craig
/************************************************************************************
'
' Copyright © 2002-2003 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole
' Copyright © 2000-2003 Philip A. Craig
'
' This software is provided 'as-is', without any express or implied warranty. In no 
' event will the authors be held liable for any damages arising from the use of this 
' software.
' 
' Permission is granted to anyone to use this software for any purpose, including 
' commercial applications, and to alter it and redistribute it freely, subject to the 
' following restrictions:
'
' 1. The origin of this software must not be misrepresented; you must not claim that 
' you wrote the original software. If you use this software in a product, an 
' acknowledgment (see the following) in the product documentation is required.
'
' Portions Copyright © 2003 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole
' or Copyright © 2000-2003 Philip A. Craig
'
' 2. Altered source versions must be plainly marked as such, and must not be 
' misrepresented as being the original software.
'
' 3. This notice may not be removed or altered from any source distribution.
'
'***********************************************************************************/
#endregion

using System;
using System.Reflection;
using System.Diagnostics;

namespace NUnit.Core
{
	/// <summary>
	/// Summary description for NotRunnableTestCase.
	/// </summary>
	public class NotRunnableTestCase : TestCase
	{
		public NotRunnableTestCase(MethodInfo method, string reason) : base(method.DeclaringType.FullName, method.Name)
		{
			ShouldRun = false;
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

			ShouldRun = false;
			IgnoreReason = String.Format("Method {0}'s signature is not correct: {1}.", method.Name, reason);
		}

		public override void Run(TestCaseResult result)
		{
			result.NotRun(base.IgnoreReason);
		}
	}
}

