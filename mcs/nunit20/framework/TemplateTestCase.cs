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
	using System.Reflection;

	/// <summary>
	/// Summary description for TestCase.
	/// </summary>
	public abstract class TemplateTestCase : TestCase
	{
		private object fixture;
		private MethodInfo  method;

		public TemplateTestCase(object fixture, MethodInfo method) : base(fixture.GetType().FullName, method.Name)
		{
			this.fixture = fixture;
			this.method = method;
		}

		public override void Run(TestCaseResult testResult)
		{
			if(ShouldRun)
			{
				DateTime start = DateTime.Now;
#if NUNIT_LEAKAGE_TEST
				long before = System.GC.GetTotalMemory( true );
#endif

				try 
				{
					InvokeSetUp();
					InvokeTestCase();
					ProcessNoException(testResult);
				}
				catch(NunitException exception)
				{
					ProcessException(exception.InnerException, testResult); 
				}
				catch(Exception exp)
				{
					ProcessException(exp, testResult);
				}
				finally 
				{
					try
					{
						InvokeTearDown();
					}
					catch(NunitException exception)
					{
						ProcessException(exception.InnerException, testResult); 
					}
					catch(Exception exp)
					{
						ProcessException(exp, testResult);
					}
					
					DateTime stop = DateTime.Now;
					TimeSpan span = stop.Subtract(start);
					testResult.Time = (double)span.Ticks / (double)TimeSpan.TicksPerSecond;

#if NUNIT_LEAKAGE_TEST
					long after = System.GC.GetTotalMemory( true );
					testResult.Leakage = after - before;
#endif
				}
			}
			else
			{
				testResult.NotRun(this.IgnoreReason);
			}

			return;
		}

		private void InvokeTearDown()
		{
			MethodInfo method = FindTearDownMethod(fixture);
			if(method != null)
			{
				InvokeMethod(method, fixture);
			}
		}

		private MethodInfo FindTearDownMethod(object fixture)
		{			
			return FindMethodByAttribute(fixture, typeof(NUnit.Framework.TearDownAttribute));
		}

		private void InvokeSetUp()
		{
			MethodInfo method = FindSetUpMethod(fixture);
			if(method != null)
			{
				InvokeMethod(method, fixture);
			}
		}

		private MethodInfo FindSetUpMethod(object fixture)
		{
			return FindMethodByAttribute(fixture, typeof(NUnit.Framework.SetUpAttribute));
		}

		private void InvokeTestCase() 
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

		protected internal abstract void ProcessNoException(TestCaseResult testResult);
		
		protected internal abstract void ProcessException(Exception exception, TestCaseResult testResult);
	}
}
