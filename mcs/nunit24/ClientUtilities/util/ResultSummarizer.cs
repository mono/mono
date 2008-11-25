// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

namespace NUnit.Util
{
	using System;
	using NUnit.Core;

	/// <summary>
	/// Summary description for ResultSummarizer.
	/// </summary>
	public class ResultSummarizer
	{
		private SummaryVisitor visitor = new SummaryVisitor();

		public ResultSummarizer(TestResult result)
		{
			result.Accept(visitor);
		}

		public ResultSummarizer(TestResult[] results)
		{
			foreach( TestResult result in results )
				result.Accept( visitor );
		}

		public string Name
		{
			get { return visitor.Name; }
		}

		public bool Success
		{
			get { return visitor.Success; }
		}

		public int ResultCount
		{
			get { return visitor.ResultCount; }
		}

//		public int Errors
//		{
//			get { return visitor.Errors; }
//		}

		public int FailureCount 
		{
			get { return visitor.FailureCount; }
		}

		public int SkipCount
		{
			get { return visitor.SkipCount; }
		}

		public int IgnoreCount
		{
			get { return visitor.IgnoreCount; }
		}

		public double Time
		{
			get { return visitor.Time; }
		}

		public int TestsNotRun
		{
			get { return visitor.TestsNotRun; }
		}

		public int SuitesNotRun
		{
			get { return visitor.SuitesNotRun; }
		}
	}
}
