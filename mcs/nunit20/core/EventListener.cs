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

	/// <summary>
	/// The EventListener interface is used to receive notifications of
	/// significant events while a test is being run.
	/// </summary>
	public interface EventListener
	{
		/// <summary>
		/// Run is starting
		/// </summary>
		/// <param name="tests">Array of tests to be run</param>
		void RunStarted( Test[] tests );

		/// <summary>
		/// Run finished successfully
		/// </summary>
		/// <param name="results">Array of test results</param>
		void RunFinished( TestResult[] results );

		/// <summary>
		/// Run was terminated due to an exception
		/// </summary>
		/// <param name="exception">Exception that was thrown</param>
		void RunFinished( Exception exception );

		/// <summary>
		/// A single test case is starting
		/// </summary>
		/// <param name="testCase">The test case</param>
		void TestStarted(TestCase testCase);
			
		/// <summary>
		/// A test case finished
		/// </summary>
		/// <param name="result">Result of the test case</param>
		void TestFinished(TestCaseResult result);

		/// <summary>
		/// A suite is starting
		/// </summary>
		/// <param name="suite">The suite that is starting</param>
		void SuiteStarted(TestSuite suite);

		/// <summary>
		/// A suite finished
		/// </summary>
		/// <param name="result">Result of the suite</param>
		void SuiteFinished(TestSuiteResult result);

		/// <summary>
		/// An unhandled exception occured while running a test,
		/// but the test was not terminated.
		/// </summary>
		/// <param name="exception"></param>
		void UnhandledException( Exception exception );
	}
}
