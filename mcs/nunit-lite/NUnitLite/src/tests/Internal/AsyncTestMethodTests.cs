#if NET_4_5
using System.Collections;
using System.Reflection;
using System.Threading;
using NUnit.Framework.Api;
using NUnit.Framework.Builders;
using NUnit.TestData;
using NUnit.TestUtilities;

namespace NUnit.Framework.Internal
{
	[TestFixture]
	public class NUnitAsyncTestMethodTests
	{
		private NUnitTestCaseBuilder _builder;
        private object _testObject;

		[SetUp]
		public void Setup()
		{
			_builder = new NUnitTestCaseBuilder();
            _testObject = new AsyncRealFixture();
		}

		public IEnumerable TestCases
		{
			get
			{
                yield return new object[] { Method("AsyncVoidSuccess"), ResultState.Success, 1 };
                yield return new object[] { Method("AsyncVoidFailure"), ResultState.Failure, 1 };
                yield return new object[] { Method("AsyncVoidError"), ResultState.Error, 0 };

                yield return new object[] { Method("AsyncTaskSuccess"), ResultState.Success, 1 };
                yield return new object[] { Method("AsyncTaskFailure"), ResultState.Failure, 1 };
                yield return new object[] { Method("AsyncTaskError"), ResultState.Error, 0 };

                yield return new object[] { Method("AsyncTaskResultSuccess"), ResultState.NotRunnable, 0 };
                yield return new object[] { Method("AsyncTaskResultFailure"), ResultState.NotRunnable, 0 };
                yield return new object[] { Method("AsyncTaskResultError"), ResultState.NotRunnable, 0 };

                yield return new object[] { Method("AsyncTaskResultCheckSuccess"), ResultState.Success, 1 };
                yield return new object[] { Method("AsyncVoidTestCaseWithParametersSuccess"), ResultState.Success, 1 };
                yield return new object[] { Method("AsyncTaskResultCheckSuccessReturningNull"), ResultState.Success, 1 };
                yield return new object[] { Method("AsyncTaskResultCheckFailure"), ResultState.Failure, 1 };
                yield return new object[] { Method("AsyncTaskResultCheckError"), ResultState.Failure, 0 };

                yield return new object[] { Method("AsyncVoidExpectedException"), ResultState.Success, 0 };
                yield return new object[] { Method("AsyncTaskExpectedException"), ResultState.Success, 0 };
                yield return new object[] { Method("AsyncTaskResultExpectedException"), ResultState.NotRunnable, 0 };

                yield return new object[] { Method("NestedAsyncVoidSuccess"), ResultState.Success, 1 };
                yield return new object[] { Method("NestedAsyncVoidFailure"), ResultState.Failure, 1 };
                yield return new object[] { Method("NestedAsyncVoidError"), ResultState.Error, 0 };

                yield return new object[] { Method("NestedAsyncTaskSuccess"), ResultState.Success, 1 };
                yield return new object[] { Method("NestedAsyncTaskFailure"), ResultState.Failure, 1 };
                yield return new object[] { Method("NestedAsyncTaskError"), ResultState.Error, 0 };

                yield return new object[] { Method("AsyncVoidMultipleSuccess"), ResultState.Success, 1 };
                yield return new object[] { Method("AsyncVoidMultipleFailure"), ResultState.Failure, 1 };
                yield return new object[] { Method("AsyncVoidMultipleError"), ResultState.Error, 0 };

                yield return new object[] { Method("AsyncTaskMultipleSuccess"), ResultState.Success, 1 };
                yield return new object[] { Method("AsyncTaskMultipleFailure"), ResultState.Failure, 1 };
                yield return new object[] { Method("AsyncTaskMultipleError"), ResultState.Error, 0 };

                yield return new object[] { Method("VoidCheckTestContextAcrossTasks"), ResultState.Success, 2 };
                yield return new object[] { Method("VoidCheckTestContextWithinTestBody"), ResultState.Success, 2 };
                yield return new object[] { Method("TaskCheckTestContextAcrossTasks"), ResultState.Success, 2 };
                yield return new object[] { Method("TaskCheckTestContextWithinTestBody"), ResultState.Success, 2 };

                yield return new object[] { Method("VoidAsyncVoidChildCompletingEarlierThanTest"), ResultState.Success, 0 };
                yield return new object[] { Method("VoidAsyncVoidChildThrowingImmediately"), ResultState.Success, 0 };
            }
		}

		[Test]
		[TestCaseSource("TestCases")]
		public void RunTests(MethodInfo method, ResultState resultState, int assertionCount)
		{
			var test = _builder.BuildFrom(method);
			var result = TestBuilder.RunTest(test, _testObject);

			Assert.That(result.ResultState, Is.EqualTo(resultState), "Wrong result state");
            Assert.That(result.AssertCount, Is.EqualTo(assertionCount), "Wrong assertion count");
		}

        [Test]
        public void SynchronizationContextSwitching()
        {
            var context = new CustomSynchronizationContext();

            SynchronizationContext.SetSynchronizationContext(context);

            var test = _builder.BuildFrom(Method("AsyncVoidAssertSynchronizationContext"));
            var result = TestBuilder.RunTest(test, _testObject);

            Assert.AreSame(context, SynchronizationContext.Current);
            Assert.That(result.ResultState, Is.EqualTo(ResultState.Success), "Wrong result state");
            Assert.That(result.AssertCount, Is.EqualTo(0), "Wrong assertion count");
        }

		private static MethodInfo Method(string name)
		{
			return typeof (AsyncRealFixture).GetMethod(name);
		}

		public class CustomSynchronizationContext : SynchronizationContext
		{
		}
	}
}
#endif