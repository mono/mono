#region Copyright (c) 2002, James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Philip A. Craig
/************************************************************************************
'
' Copyright © 2002 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov
' Copyright © 2000-2002 Philip A. Craig
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
' Portions Copyright © 2002 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov 
' or Copyright © 2000-2002 Philip A. Craig
'
' 2. Altered source versions must be plainly marked as such, and must not be 
' misrepresented as being the original software.
'
' 3. This notice may not be removed or altered from any source distribution.
'
'***********************************************************************************/
#endregion

namespace NUnit.Core
{
	using System;
	using System.Collections;
	using System.Reflection;

	/// <summary>
	///		Test Class.
	/// </summary>
	public abstract class Test : LongLivingMarshalByRefObject, TestInfo
	{
		private string fullName;
		private string testName;
		private bool shouldRun;
		private string ignoreReason;

		protected Test(string pathName, string testName) 
		{ 
			fullName = pathName + "." + testName;
			this.testName = testName;
			shouldRun = true;
		}

		public string IgnoreReason
		{
			get { return ignoreReason; }
			set { ignoreReason = value; }
		}

		public bool ShouldRun
		{
			get { return shouldRun; }
			set { shouldRun = value; }
		}

		public Test(string name)
		{
			fullName = testName = name;
		}

		public string FullName 
		{
			get { return fullName; }
		}

		public string Name
		{
			get { return testName; }
		}

		public abstract int CountTestCases { get; }
		public abstract bool IsSuite { get; }
		public abstract ArrayList Tests { get; }

		public abstract TestResult Run(EventListener listener);

		protected MethodInfo FindMethodByAttribute(object fixture, Type type)
		{
			foreach(MethodInfo method in fixture.GetType().GetMethods(BindingFlags.Public|BindingFlags.Instance|BindingFlags.NonPublic))
			{
				if(method.IsDefined(type,true)) 
				{
					return method;
				}
			}
			return null;
		}

		protected void InvokeMethod(MethodInfo method, object fixture) 
		{
			if(method != null)
			{
				try
				{
					method.Invoke(fixture, null);
				}
				catch(TargetInvocationException e)
				{
					Exception inner = e.InnerException;
					throw new NunitException("Rethrown",inner);
				}
			}
		}
	}
}
