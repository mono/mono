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
	using System.Diagnostics;
	using System.Reflection;

	/// <summary>
	/// Summary description for ExpectedExceptionTestCase.
	/// </summary>
	public class ExpectedExceptionTestCase : TemplateTestCase
	{
		private Type expectedException;
		private string expectedMessage;

		public ExpectedExceptionTestCase(Type fixtureType, MethodInfo method) : base(fixtureType, method)
		{
			Initialize( method );
		}

		public ExpectedExceptionTestCase(object fixture, MethodInfo method) : base(fixture, method)
		{
			Initialize( method );
		}

		private void Initialize( MethodInfo method )
		{
			NUnit.Framework.ExpectedExceptionAttribute attribute = 
				Reflect.GetExpectedExceptionAttribute( method );

			if ( attribute == null )
				throw new InvalidTestFixtureException( "ExpectedExceptionAttribute not found" );

			this.expectedException = attribute.ExceptionType;
			this.expectedMessage = attribute.ExpectedMessage;
		}

		protected override internal void ProcessException(Exception exception, TestCaseResult testResult)
		{
			if (expectedException.Equals(exception.GetType()))
			{
				if (expectedMessage != null && !expectedMessage.Equals(exception.Message))
				{
					string message = string.Format("Expected exception to have message: \"{0}\" but received message \"{1}\"", 
						expectedMessage, exception.Message);
					testResult.Failure(message, exception.StackTrace);
				} 
				else 
				{
					testResult.Success();
				}
			}
			else if (exception is Framework.AssertionException)
			{
				RecordException(exception,testResult);
			}
			else
			{
				string message = "Expected: " + expectedException.Name + " but was " + exception.GetType().Name;
				testResult.Failure(message, exception.StackTrace);
			}

			return;
		}

		protected override internal void ProcessNoException(TestCaseResult testResult)
		{
			testResult.Failure(expectedException.Name + " was expected", null);
		}
	}
}
