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

namespace NUnit.Core
{
	using System;
	using System.Reflection;

	/// <summary>
	/// Summary description for TestCaseBuilder.
	/// </summary>
	public class TestCaseBuilder
	{
		public static TestCase Make(object fixture, MethodInfo method)
		{
			TestCase testCase = null;

			if(HasTestAttribute(method) || HasObsoleteTestName(method) && !HasAnySetUpOrTearDownAttribute(method) )
			{
				if(IsTestMethodSignatureCorrect(method))
				{
					if(!IsExpectedException(method))
						testCase = new NormalTestCase(fixture, method);
					else
					{
						NUnit.Framework.ExpectedExceptionAttribute expectedException = GetExpectedExceptions(method);
						testCase = new ExpectedExceptionTestCase(fixture, method, expectedException.ExceptionType, expectedException.ExpectedMessage);
					}

					if(HasIgnoreAttribute(method))
					{
						testCase.ShouldRun = false;
						testCase.IgnoreReason = GetIgnoreReason(method);
					}

					testCase.Description = GetDescription(method);
				}
				else
				{
					testCase = new NotRunnableTestCase(method);
				}
			}

			return testCase;
		}

		public static TestCase Make(object fixture, string methodName)
		{
			MethodInfo [] methods = fixture.GetType().GetMethods(BindingFlags.NonPublic|BindingFlags.Public|BindingFlags.Instance);
			foreach(MethodInfo method in methods)
			{
				if(method.Name.Equals(methodName))
					return Make(fixture, method);
			}

			return null;
		}

		private static bool IsExpectedException(MethodInfo method)
		{
			Type exceptionAttr = typeof(NUnit.Framework.ExpectedExceptionAttribute);
			object[] attributes = method.GetCustomAttributes(exceptionAttr, false);
			return attributes.Length == 1;
		}

		private static NUnit.Framework.ExpectedExceptionAttribute GetExpectedExceptions(MethodInfo method)
		{
			Type exceptionAttr = typeof(NUnit.Framework.ExpectedExceptionAttribute);
			object[] attributes = method.GetCustomAttributes(exceptionAttr, false);

			NUnit.Framework.ExpectedExceptionAttribute expectedAttr = null;

			if(attributes.Length == 1)
			{
				expectedAttr = (NUnit.Framework.ExpectedExceptionAttribute)attributes[0];
			}

			return expectedAttr;
		}

		private static string GetDescription(MethodInfo method)
		{
			Type testAttr = typeof(NUnit.Framework.TestAttribute);
			object[] attributes = method.GetCustomAttributes(testAttr, false);

			string description = null;

			if(attributes.Length == 1)
			{
				NUnit.Framework.TestAttribute attribute = 
					(NUnit.Framework.TestAttribute)attributes[0];
				description = attribute.Description;
			}

			return description;
		}


		public static int CountTestCases(object fixture) 
		{
			int testCases = 0;

			MethodInfo [] methods = fixture.GetType().GetMethods();
			foreach(MethodInfo method in methods)
			{
				if(IsTestMethod(method))
					testCases++;
			}

			return testCases;
		}


		public static bool IsTestMethod(MethodInfo methodToCheck) 
		{
			return
				(HasTestAttribute(methodToCheck) || HasObsoleteTestName(methodToCheck))
				&& IsTestMethodSignatureCorrect(methodToCheck);
		}

		private static bool IsTestMethodSignatureCorrect(MethodInfo methodToCheck)
		{
			return 
				!methodToCheck.IsStatic
				&& !methodToCheck.IsAbstract
				&& methodToCheck.IsPublic
				&& methodToCheck.GetParameters().Length == 0
				&& methodToCheck.ReturnType.Equals(typeof(void));
		}

		private static bool HasTestAttribute(MethodInfo methodToCheck)
		{
			return methodToCheck.IsDefined(typeof(NUnit.Framework.TestAttribute),false);
		}
		
		private static bool HasObsoleteTestName(MethodInfo methodToCheck)
		{
			return methodToCheck.Name.ToLower().StartsWith("test");
		}

		private static bool HasAnySetUpOrTearDownAttribute( MethodInfo methodToCheck )
		{
			object[] attributes = methodToCheck.GetCustomAttributes( false );
			foreach( Attribute attribute in attributes )
				if ( attribute is NUnit.Framework.SetUpAttribute ||
					 attribute is NUnit.Framework.TestFixtureSetUpAttribute ||
					 attribute is NUnit.Framework.TearDownAttribute || 
					 attribute is NUnit.Framework.TestFixtureTearDownAttribute )
					return true;

			return false;	
		}

		private static bool HasIgnoreAttribute(MethodInfo methodToCheck)
		{
			Type ignoreMethodAttribute = typeof(NUnit.Framework.IgnoreAttribute);
			object[] attributes = methodToCheck.GetCustomAttributes(ignoreMethodAttribute, false);
			return attributes.Length == 1;
		}

		private static string GetIgnoreReason(MethodInfo methodToCheck)
		{
			Type ignoreMethodAttribute = typeof(NUnit.Framework.IgnoreAttribute);
			NUnit.Framework.IgnoreAttribute[] attributes = (NUnit.Framework.IgnoreAttribute[])methodToCheck.GetCustomAttributes(ignoreMethodAttribute, false);
			string result = "no reason";
			if(attributes.Length > 0)
				result = attributes[0].Reason;

			return result;
		}
	}
}

