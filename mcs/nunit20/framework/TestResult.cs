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
	/// Summary description for TestResult.
	/// </summary>
	/// 
	[Serializable]
	public abstract class TestResult
	{
		private bool executed;
		private bool isFailure; 
		private double time;
		private string name;
		private ITest test;
		private string stackTrace;
		private string description;

#if NUNIT_LEAKAGE_TEST
		private long leakage = 0;
#endif
		
		protected TestResult(ITest test, string name)
		{
			this.name = name;
			this.test = test;
			if(test != null)
				this.description = test.Description;
		}

		public bool Executed 
		{
			get { return executed; }
			set { executed = value; }
		}

		public virtual bool AllTestsExecuted
		{
			get { return executed; }
		}

		public virtual string Name
		{
			get{ return name;}
		}

		public ITest Test
		{
			get{ return test;}
		}

		public virtual bool IsSuccess
		{
			get { return !(isFailure); }
		}
		
		public virtual bool IsFailure
		{
			get { return isFailure; }
			set { isFailure = value; }
		}

		public virtual string Description
		{
			get { return description; }
			set { description = value; }
		}

		public double Time 
		{
			get{ return time; }
			set{ time = value; }
		}

#if NUNIT_LEAKAGE_TEST
		public long Leakage
		{
			get{ return leakage; }
			set{ leakage = value; }
		}
#endif

		public abstract string Message
		{
			get;
		}

		public virtual string StackTrace
		{
			get 
			{ 
				return stackTrace;
			}
			set 
			{
				stackTrace = value;
			}
		}

		public abstract void NotRun(string message);

		public abstract void Accept(ResultVisitor visitor);
	}
}
