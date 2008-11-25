// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

namespace NUnit.Core
{
    /// <summary>
    /// The ResultVisitor interface implements the
    /// Visitor pattern over TestResults
    /// </summary>
	public interface ResultVisitor
	{
        /// <summary>
        /// Visit a TestCaseResult
        /// </summary>
        /// <param name="caseResult">The result to visit</param>
		void Visit(TestCaseResult caseResult);

        /// <summary>
        /// Visit a TestSuiteResult
        /// </summary>
        /// <param name="suiteResult">The result to visit</param>
		void Visit(TestSuiteResult suiteResult);
	}
}
