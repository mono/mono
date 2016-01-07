// ***********************************************************************
// Copyright (c) 2011 Charlie Poole
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

using NUnit.Framework.Api;

namespace NUnit.Framework.Internal
{
    [TestFixture]
    public class ResultStateTests
    {
        [TestCase(TestStatus.Failed)]
        [TestCase(TestStatus.Skipped)]
        [TestCase(TestStatus.Inconclusive)]
        [TestCase(TestStatus.Passed)]
        public void Status_ConstructorWithOneArguments_ReturnsConstructorArgumentStatus(TestStatus status)
        {
            // Arrange N/A

            ResultState resultState = new ResultState(status);

            Assert.AreEqual(status, resultState.Status);
        }

        [Test]
        public void Label_ConstructorWithOneArguments_ReturnsStringEmpty()
        {
            // Arrange N/A

            ResultState resultState = new ResultState(TestStatus.Failed);

            Assert.AreEqual(string.Empty, resultState.Label);
        }

        [TestCase(TestStatus.Failed)]
        [TestCase(TestStatus.Skipped)]
        [TestCase(TestStatus.Inconclusive)]
        [TestCase(TestStatus.Passed)]
        public void Status_ConstructorWithTwoArguments_ReturnsConstructorArgumentStatus(TestStatus status)
        {
            // Arrange N/A

            ResultState resultState = new ResultState(status, string.Empty);

            Assert.AreEqual(status, resultState.Status);
        }

        [TestCase("")]
        [TestCase("label")]        
        public void Label_ConstructorWithTwoArguments_ReturnsConstructorArgumentLabel(string label)
        {
            // Arrange N/A

            ResultState resultState = new ResultState(TestStatus.Failed, label);

            Assert.AreEqual(label, resultState.Label);
        }

        [Test]
        public void Label_ConstructorWithTwoArgumentsLabelArgumentIsNull_ReturnsEmptyString()
        {
            // Arrange N/A

            ResultState resultState = new ResultState(TestStatus.Failed, null);

            Assert.AreEqual(string.Empty, resultState.Label);
        }
        
        [TestCase(TestStatus.Skipped, SpecialValue.Null, "Skipped")]
        [TestCase(TestStatus.Passed, "", "Passed")]
        [TestCase(TestStatus.Passed, "testLabel", "Passed:testLabel")]
        public void ToString_Constructor_ReturnsExepectedString(TestStatus status, string label, string expected)
        {
            // Arrange N/A

            ResultState resultState = new ResultState(status, label);

            Assert.AreEqual(expected, resultState.ToString());
        }

        #region Test Fields

        [Test]
        public void Inconclusive_NA_ReturnsResultStateWithPropertiesCorrectlySet()
        {
            ResultState resultState = ResultState.Inconclusive;

            Assert.AreEqual(TestStatus.Inconclusive, resultState.Status, "Status not correct.");
            Assert.AreEqual(string.Empty, resultState.Label, "Label not correct.");            
        }

        [Test]
        public void NotRunnable_NA_ReturnsResultStateWithPropertiesCorrectlySet()
        {
            ResultState resultState = ResultState.NotRunnable;

            Assert.AreEqual(TestStatus.Skipped, resultState.Status, "Status not correct.");
            Assert.AreEqual("Invalid", resultState.Label, "Label not correct.");
        }

        [Test]
        public void Skipped_NA_ReturnsResultStateWithPropertiesCorrectlySet()
        {
            ResultState resultState = ResultState.Skipped;

            Assert.AreEqual(TestStatus.Skipped, resultState.Status, "Status not correct.");
            Assert.AreEqual(string.Empty, resultState.Label, "Label not correct.");
        }

        [Test]
        public void Ignored_NA_ReturnsResultStateWithPropertiesCorrectlySet()
        {
            ResultState resultState = ResultState.Ignored;

            Assert.AreEqual(TestStatus.Skipped, resultState.Status, "Status not correct.");
            Assert.AreEqual("Ignored", resultState.Label, "Label not correct.");
        }
        
        [Test]
        public void Success_NA_ReturnsResultStateWithPropertiesCorrectlySet()
        {
            ResultState resultState = ResultState.Success;

            Assert.AreEqual(TestStatus.Passed, resultState.Status, "Status not correct.");
            Assert.AreEqual(string.Empty, resultState.Label, "Label not correct.");
        }

        [Test]
        public void Failure_NA_ReturnsResultStateWithPropertiesCorrectlySet()
        {
            ResultState resultState = ResultState.Failure;

            Assert.AreEqual(TestStatus.Failed, resultState.Status, "Status not correct.");
            Assert.AreEqual(string.Empty, resultState.Label, "Label not correct.");
        }

        [Test]
        public void Error_NA_ReturnsResultStateWithPropertiesCorrectlySet()
        {
            ResultState resultState = ResultState.Error;

            Assert.AreEqual(TestStatus.Failed, resultState.Status, "Status not correct.");
            Assert.AreEqual("Error", resultState.Label, "Label not correct.");
        }

        [Test]
        public void Cancelled_NA_ReturnsResultStateWithPropertiesCorrectlySet()
        {
            ResultState resultState = ResultState.Cancelled;

            Assert.AreEqual(TestStatus.Failed, resultState.Status, "Status not correct.");
            Assert.AreEqual("Cancelled", resultState.Label, "Label not correct.");
        }

        #endregion
    }
}