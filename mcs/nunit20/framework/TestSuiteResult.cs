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

	/// <summary>
	///		TestSuiteResult
	/// </summary>
	/// 
	[Serializable]
	public class TestSuiteResult : TestResult
	{
		private ArrayList results = new ArrayList();
		private string message;
		
		public TestSuiteResult(Test test, string name) : base(test, name)
		{
			Executed = false;
		}

		public void AddResult(TestResult result) 
		{
			results.Add(result);
		}

		public override bool IsSuccess
		{
			get 
			{
				bool result = true;
				foreach(TestResult testResult in results)
					result &= testResult.IsSuccess;
				return result;
			}
		}

		public override bool IsFailure
		{
			get 
			{
				bool result = false;
				foreach(TestResult testResult in results)
					result |= testResult.IsFailure;
				return result;
			}
		}

		public override void NotRun(string message)
		{
			this.Executed = false;
			this.message = message;
		}


		public override string Message
		{
			get { return message; }
		}

		public override string StackTrace
		{
			get { return null; }
		}


		public IList Results
		{
			get { return results; }
		}

		public override void Accept(ResultVisitor visitor) 
		{
			visitor.visit(this);
		}
	}
}
