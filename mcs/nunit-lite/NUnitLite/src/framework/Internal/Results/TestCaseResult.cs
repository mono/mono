using System;
using NUnit.Framework.Api;

namespace NUnit.Framework.Internal
{
    /// <summary>
    /// Represents the result of running a single test case.
    /// </summary>
    public class TestCaseResult : TestResult
    {
        /// <summary>
        /// Construct a TestCaseResult based on a TestMethod
        /// </summary>
        /// <param name="test">A TestMethod to which the result applies.</param>
        public TestCaseResult(TestMethod test) : base(test) { }

        /// <summary>
        /// Gets the number of test cases that failed
        /// when running the test and all its children.
        /// </summary>
        public override int FailCount
        {
            get { return ResultState.Status == TestStatus.Failed ? 1 : 0; }
        }

        /// <summary>
        /// Gets the number of test cases that passed
        /// when running the test and all its children.
        /// </summary>
        public override int PassCount
        {
            get { return ResultState.Status == TestStatus.Passed ? 1 : 0; }
        }

        /// <summary>
        /// Gets the number of test cases that were skipped
        /// when running the test and all its children.
        /// </summary>
        public override int SkipCount
        {
            get { return ResultState.Status == TestStatus.Skipped ? 1 : 0; }
        }

        /// <summary>
        /// Gets the number of test cases that were inconclusive
        /// when running the test and all its children.
        /// </summary>
        public override int InconclusiveCount
        {
            get { return ResultState.Status == TestStatus.Inconclusive ? 1 : 0; }
        }

        //public override XmlNode AddToXml(XmlNode parentNode, bool recursive)
        //{
        //    XmlNode thisNode = this.test.AddToXml(parentNode, recursive);
        //    //thisNode.AddAttribute("seed", this.test.Seed.ToString());
        //    return thisNode;
        //}
    }
}
