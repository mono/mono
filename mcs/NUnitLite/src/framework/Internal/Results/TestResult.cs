// ***********************************************************************
// Copyright (c) 2010 Charlie Poole
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using System;
using NUnit.Framework.Api;

namespace NUnit.Framework.Internal
{
    /// <summary>
    /// The TestResult class represents the result of a test.
    /// </summary>
    public abstract class TestResult : ITestResult
    {
        #region Fields

        /// <summary>
        /// Indicates the result of the test
        /// </summary>
        [CLSCompliant(false)]
        protected ResultState resultState;

        /// <summary>
        /// The elapsed time for executing this test
        /// </summary>
        private TimeSpan time = TimeSpan.Zero;

        /// <summary>
        /// The test that this result pertains to
        /// </summary>
        [CLSCompliant(false)]
        protected readonly ITest test;

        /// <summary>
        /// The stacktrace at the point of failure
        /// </summary>
        private string stackTrace;

        /// <summary>
        /// Message giving the reason for failure, error or skipping the test
        /// </summary>
        [CLSCompliant(false)]
        protected string message;

        /// <summary>
        /// Number of asserts executed by this test
        /// </summary>
        [CLSCompliant(false)]
        protected int assertCount = 0;

        /// <summary>
        /// List of child results
        /// </summary>
#if CLR_2_0 || CLR_4_0
        private System.Collections.Generic.List<ITestResult> children;
#else
        private System.Collections.ArrayList children;
#endif

        #endregion

        #region Constructor

        /// <summary>
        /// Construct a test result given a Test
        /// </summary>
        /// <param name="test">The test to be used</param>
        public TestResult(ITest test)
        {
            this.test = test;
            this.resultState = ResultState.Inconclusive;
        }

        #endregion

        #region ITestResult Members

        /// <summary>
        /// Gets the test with which this result is associated.
        /// </summary>
        public ITest Test
        {
            get { return test; }
        }

        /// <summary>
        /// Gets the ResultState of the test result, which
        /// indicates the success or failure of the test.
        /// </summary>
        public ResultState ResultState
        {
            get { return resultState; }
        }

        /// <summary>
        /// Gets the name of the test result
        /// </summary>
        public virtual string Name
        {
            get { return test.Name; }
        }

        /// <summary>
        /// Gets the full name of the test result
        /// </summary>
        public virtual string FullName
        {
            get { return test.FullName; }
        }

        /// <summary>
        /// Gets or sets the elapsed time for running the test
        /// </summary>
        public TimeSpan Duration
        {
            get { return time; }
            set { time = value; }
        }

        /// <summary>
        /// Gets the message associated with a test
        /// failure or with not running the test
        /// </summary>
        public string Message
        {
            get { return message; }
        }

        /// <summary>
        /// Gets any stacktrace associated with an
        /// error or failure. Not available in
        /// the Compact Framework 1.0.
        /// </summary>
        public virtual string StackTrace
        {
            get { return stackTrace; }
        }

        /// <summary>
        /// Gets or sets the count of asserts executed
        /// when running the test.
        /// </summary>
        public int AssertCount
        {
            get { return assertCount; }
            set { assertCount = value; }
        }

        /// <summary>
        /// Gets the number of test cases that failed
        /// when running the test and all its children.
        /// </summary>
        public abstract int FailCount { get; }

        /// <summary>
        /// Gets the number of test cases that passed
        /// when running the test and all its children.
        /// </summary>
        public abstract int PassCount { get; }

        /// <summary>
        /// Gets the number of test cases that were skipped
        /// when running the test and all its children.
        /// </summary>
        public abstract int SkipCount { get; }

        /// <summary>
        /// Gets the number of test cases that were inconclusive
        /// when running the test and all its children.
        /// </summary>
        public abstract int InconclusiveCount { get; }

        /// <summary>
        /// Indicates whether this result has any child results.
        /// Test HasChildren before accessing Children to avoid
        /// the creation of an empty collection.
        /// </summary>
        public bool HasChildren
        {
            get { return children != null && children.Count > 0; }
        }

        /// <summary>
        /// Gets the collection of child results.
        /// </summary>
#if CLR_2_0 || CLR_4_0
        public System.Collections.Generic.IList<ITestResult> Children
        {
            get
            {
                if (children == null)
                    children = new System.Collections.Generic.List<ITestResult>();

                return children;
            }
        }
#else
        public System.Collections.IList Children
        {
            get 
            {
                if (children == null)
                    children = new System.Collections.ArrayList();

                return children;
            }
        }
#endif

        #endregion

        #region IXmlNodeBuilder Members

        /// <summary>
        /// Returns the Xml representation of the result.
        /// </summary>
        /// <param name="recursive">If true, descendant results are included</param>
        /// <returns>An XmlNode representing the result</returns>
        public XmlNode ToXml(bool recursive)
        {
            XmlNode topNode = XmlNode.CreateTopLevelElement("dummy");

            AddToXml(topNode, recursive);

            return topNode.FirstChild;
        }

        /// <summary>
        /// Adds the XML representation of the result as a child of the
        /// supplied parent node..
        /// </summary>
        /// <param name="parentNode">The parent node.</param>
        /// <param name="recursive">If true, descendant results are included</param>
        /// <returns></returns>
        public virtual XmlNode AddToXml(XmlNode parentNode, bool recursive)
        {
            // A result node looks like a test node with extra info added
            XmlNode thisNode = this.test.AddToXml(parentNode, false);

            thisNode.AddAttribute("result", ResultState.Status.ToString());
            if (ResultState.Label != string.Empty) // && ResultState.Label != ResultState.Status.ToString())
                thisNode.AddAttribute("label", ResultState.Label);

            thisNode.AddAttribute("time", this.Duration.ToString());

            if (this.test is TestSuite)
            {
                thisNode.AddAttribute("total", (PassCount + FailCount + SkipCount + InconclusiveCount).ToString());
                thisNode.AddAttribute("passed", PassCount.ToString());
                thisNode.AddAttribute("failed", FailCount.ToString());
                thisNode.AddAttribute("inconclusive", InconclusiveCount.ToString());
                thisNode.AddAttribute("skipped", SkipCount.ToString());                
            }            

            thisNode.AddAttribute("asserts", this.AssertCount.ToString());

            switch (ResultState.Status)
            {
                case TestStatus.Failed:
                    AddFailureElement(thisNode);
                    break;
                case TestStatus.Skipped:
                    AddReasonElement(thisNode);
                    break;
                case TestStatus.Passed:
                case TestStatus.Inconclusive:
                    if (this.Message != null)
                        AddReasonElement(thisNode);
                    break;
            }

            if (recursive && HasChildren)
                foreach (TestResult child in Children)
                    child.AddToXml(thisNode, recursive);

            return thisNode;
        }

        #endregion

        #region Other Public Methods

        /// <summary>
        /// Add a child result
        /// </summary>
        /// <param name="result">The child result to be added</param>
        public virtual void AddResult(TestResult result)
        {
            this.Children.Add(result);

            this.assertCount += result.AssertCount;

            switch (result.ResultState.Status)
            {
                case TestStatus.Passed:

                    if (this.resultState.Status == TestStatus.Inconclusive)
                        SetResult(ResultState.Success);

                    break;

                case TestStatus.Failed:

                    if (this.resultState.Status != TestStatus.Failed)
                        SetResult(ResultState.Failure, "One or more child tests had errors");

                    break;

                case TestStatus.Skipped:

                    switch (result.ResultState.Label)
                    {
                        case "Invalid":

                            if (this.ResultState != ResultState.NotRunnable && this.ResultState.Status != TestStatus.Failed)
                                SetResult(ResultState.Failure, "One or more child tests had errors");

                            break;

                        case "Ignored":

                            if (this.ResultState.Status == TestStatus.Inconclusive || this.ResultState.Status == TestStatus.Passed)
                                SetResult(ResultState.Ignored, "One or more child tests were ignored");

                            break;

                        default:

                            // Tests skipped for other reasons do not change the outcome
                            // of the containing suite when added.

                            break;
                    }

                    break;

                case TestStatus.Inconclusive:

                    // An inconclusive result does not change the outcome
                    // of the containing suite when added.

                    break;
            }
        }

        /// <summary>
        /// Set the result of the test
        /// </summary>
        /// <param name="resultState">The ResultState to use in the result</param>
        public void SetResult(ResultState resultState)
        {
            SetResult(resultState, null, null);
        }

        /// <summary>
        /// Set the result of the test
        /// </summary>
        /// <param name="resultState">The ResultState to use in the result</param>
        /// <param name="message">A message associated with the result state</param>
        public void SetResult(ResultState resultState, string message)
        {
            SetResult(resultState, message, null);
        }

        /// <summary>
        /// Set the result of the test
        /// </summary>
        /// <param name="resultState">The ResultState to use in the result</param>
        /// <param name="message">A message associated with the result state</param>
        /// <param name="stackTrace">Stack trace giving the location of the command</param>
        public void SetResult(ResultState resultState, string message, string stackTrace)
        {
            this.resultState = resultState;
            this.message = message;
            this.stackTrace = stackTrace;
        }

        /// <summary>
        /// Set the test result based on the type of exception thrown
        /// </summary>
        /// <param name="ex">The exception that was thrown</param>
        public void RecordException(Exception ex)
        {
            if (ex is NUnitException)
                ex = ex.InnerException;

            if (ex is System.Threading.ThreadAbortException)
                SetResult(ResultState.Cancelled, "Test cancelled by user", ex.StackTrace);
            else if (ex is AssertionException)
                SetResult(ResultState.Failure, ex.Message, StackFilter.Filter(ex.StackTrace));
            else if (ex is IgnoreException)
                SetResult(ResultState.Ignored, ex.Message, StackFilter.Filter(ex.StackTrace));
            else if (ex is InconclusiveException)
                SetResult(ResultState.Inconclusive, ex.Message, StackFilter.Filter(ex.StackTrace));
            else if (ex is SuccessException)
                SetResult(ResultState.Success, ex.Message, StackFilter.Filter(ex.StackTrace));
            else
                SetResult(ResultState.Error,
                    ExceptionHelper.BuildMessage(ex),
                    ExceptionHelper.BuildStackTrace(ex));
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Adds a reason element to a node and returns it.
        /// </summary>
        /// <param name="targetNode">The target node.</param>
        /// <returns>The new reason element.</returns>
        private XmlNode AddReasonElement(XmlNode targetNode)
        {
            XmlNode reasonNode = targetNode.AddElement("reason");
            reasonNode.AddElement("message").TextContent = this.Message;
            return reasonNode;
        }

        /// <summary>
        /// Adds a failure element to a node and returns it.
        /// </summary>
        /// <param name="targetNode">The target node.</param>
        /// <returns>The new failure element.</returns>
        private XmlNode AddFailureElement(XmlNode targetNode)
        {
            XmlNode failureNode = targetNode.AddElement("failure");

            if (this.Message != null)
            {
                failureNode.AddElement("message").TextContent = this.Message;
            }

            if (this.StackTrace != null)
            {
                failureNode.AddElement("stack-trace").TextContent = this.StackTrace;
            }

            return failureNode;
        }

        //private static bool IsTestCase(ITest test)
        //{
        //    return !(test is TestSuite);
        //}

        #endregion
    }
}
