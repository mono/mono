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

namespace NUnit.Util
{
	using System;
	using NUnit.Core;

	/// <summary>
	/// Summary description for SiummaryVisitor.
	/// </summary>
	public class SummaryVisitor : ResultVisitor
	{
		private int totalCount;
		private int failureCount;
		private int testsNotRun;
		private int suitesNotRun;
		
		private double time;
		private string name;
		private bool initialized;

		public SummaryVisitor()
		{
			totalCount = 0;
			initialized = false;
		}

		public void Visit(TestCaseResult caseResult) 
		{
			SetNameandTime(caseResult.Name, caseResult.Time);

			if(caseResult.Executed)
			{
				totalCount++;
				if(caseResult.IsFailure)
					failureCount++;
			}
			else
				testsNotRun++;
		}

		public void Visit(TestSuiteResult suiteResult) 
		{
			SetNameandTime(suiteResult.Name, suiteResult.Time);

			
			
			foreach (TestResult result in suiteResult.Results)
			{
				result.Accept(this);
			}
			
			if(!suiteResult.Executed)
				suitesNotRun++;
		}

		public double Time
		{
			get { return time; }
		}

		private void SetNameandTime(string name, double time)
		{
			if(!initialized)
			{
				this.time = time;
				this.name = name;
				initialized = true;
			}
		}

		public bool Success
		{
			get { return (failureCount == 0); }
		}

		public int Count
		{
			get { return totalCount; }
		}

		public int Failures
		{
			get { return failureCount; }
		}

		public int TestsNotRun
		{
			get { return testsNotRun; }
		}

		public int SuitesNotRun
		{
			get { return suitesNotRun; }
		}

		public string Name
		{
			get { return name; }
		}
	}
}
