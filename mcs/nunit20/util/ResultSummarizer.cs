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
			get { return visitor.Count; }
		}

//		public int Errors
//		{
//			get { return visitor.Errors; }
//		}

		public int Failures 
		{
			get { return visitor.Failures; }
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
