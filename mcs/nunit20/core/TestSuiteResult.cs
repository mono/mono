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
	using System.Collections;

	/// <summary>
	///		TestSuiteResult
	/// </summary>
	/// 
	[Serializable]
	public class TestSuiteResult : TestResult
	{
		private ArrayList results = new ArrayList();
		
		public TestSuiteResult(ITest test, string name) : base(test, name)
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

		/// <summary>
		/// A suite is considered as failing if it is marked as a failure - usually
		/// because TestFixtureSetUp or TestFixtureTearDown failed - or if one of the
		/// tests it contains failed. 
		/// </summary>
		public override bool IsFailure
		{
			get 
			{
				if ( base.IsFailure )
					return true;

				foreach(TestResult testResult in results)
					if ( testResult.IsFailure )
						return true;

				return false;
			}
		}

		public override bool AllTestsExecuted
		{
			get
			{
				if (!this.Executed)
					return false;

				foreach( TestResult testResult in results )
				{
					if ( !testResult.AllTestsExecuted )
						return false;
				}
				return true;
			}
		}

		public IList Results
		{
			get { return results; }
		}

		public override void Accept(ResultVisitor visitor) 
		{
			visitor.Visit(this);
		}
	}
}
