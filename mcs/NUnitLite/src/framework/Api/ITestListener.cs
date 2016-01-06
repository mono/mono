// ***********************************************************************
// Copyright (c) 2009 Charlie Poole
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

namespace NUnit.Framework.Api
{
	/// <summary>
	/// The ITestListener interface is used internally to receive 
	/// notifications of significant events while a test is being 
    /// run. The events are propogated to clients by means of an
    /// AsyncCallback. NUnit extensions may also monitor these events.
	/// </summary>
	public interface ITestListener
	{
		/// <summary>
		/// Called when a test has just started
		/// </summary>
		/// <param name="test">The test that is starting</param>
		void TestStarted(ITest test);
			
		/// <summary>
		/// Called when a test has finished
		/// </summary>
		/// <param name="result">The result of the test</param>
		void TestFinished(ITestResult result);

		/// <summary>
		/// Called when the test creates text output.
		/// </summary>
		/// <param name="testOutput">A console message</param>
		void TestOutput(TestOutput testOutput);
	}
}
