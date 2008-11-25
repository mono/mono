// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

using System;

namespace NUnit.Util
{
	using NUnit.Core;

	/// <summary>
	/// Summary description for TestResultItem.
	/// </summary>
	public class TestResultItem
	{
		private string testName;
		private string message;
		private string stackTrace;

		public TestResultItem(TestResult result )
		{
			testName = result.Test.TestName.FullName;
			message = result.Message;
			stackTrace = result.StackTrace;

			if ( result.Test.IsSuite && result.FailureSite == FailureSite.SetUp )
				testName += " (TestFixtureSetUp)";
		}

		public TestResultItem( string testName, string message, string stackTrace )
		{
			this.testName = testName;
			this.message = message;
			this.stackTrace = stackTrace;
		}

		public override string ToString()
		{
			if ( message.Length > 64000 )
				return string.Format( "{0}:{1}{2}", testName, Environment.NewLine, message.Substring( 0, 64000 ) );

			return GetMessage();
		}

		public string GetMessage()
		{
			return String.Format("{0}:{1}{2}", testName, Environment.NewLine, message);
		}

        public string GetToolTipMessage()   //NRG 05/28/03 - Substitute spaces for tab characters
        {
            return (ReplaceTabs(GetMessage(), 8)); // Change each tab to 8 space characters
        }

        public string ReplaceTabs(string strOriginal, int nSpaces)  //NRG 05/28/03
        {
            string strSpaces = string.Empty;
            strSpaces = strSpaces.PadRight(nSpaces, ' ');
            return(strOriginal.Replace("\t", strSpaces));
        }

		public string StackTrace
		{
			get 
			{
				string trace = "No stack trace is available";
				if(stackTrace != null)
					trace = StackTraceFilter.Filter(stackTrace);

				return trace;
			}
		}
	}
}
