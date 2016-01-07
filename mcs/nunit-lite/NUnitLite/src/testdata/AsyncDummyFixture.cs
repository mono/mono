#if NET_4_5
using System.Threading.Tasks;
using NUnit.Framework;
using System;

namespace NUnit.TestData
{
	public class AsyncDummyFixture
	{
		[Test]
		public async void AsyncVoid()
		{
            await Task.Delay(0); // To avoid warning message
		}

		[Test]
		public async Task AsyncTask()
		{
			await Task.Yield();
		}

		[Test]
		public async Task<int> AsyncGenericTask()
		{
			return await Task.FromResult(1);
		}

        [Test]
        public Task NonAsyncTask()
        {
            return Task.Delay(0);
        }

        [Test]
		public Task<int> NonAsyncGenericTask()
		{
			return Task.FromResult(1);
		}

        [TestCase(4)]
        public async void AsyncVoidTestCase(int x)
        {
            await Task.Delay(0);
        }
        
        [TestCase(ExpectedResult = 1)]
        public async void AsyncVoidTestCaseWithExpectedResult()
        {
            await Task.Run(() => 1);
        }

        [TestCase(4)]
        public async Task AsyncTaskTestCase(int x)
        {
            await Task.Delay(0);
        }

        [TestCase(ExpectedResult = 1)]
        public async Task AsyncTaskTestCaseWithExpectedResult()
        {
            await Task.Run(() => 1);
        }

        [TestCase(4)]
        public async Task<int> AsyncGenericTaskTestCase()
        {
            return await Task.Run(() => 1);
        }

        [TestCase(ExpectedResult = 1)]
        public async Task<int> AsyncGenericTaskTestCaseWithExpectedResult()
        {
            return await Task.Run(() => 1);
        }

        [TestCase(ExpectedException = typeof(Exception))]
        public async Task<int> AsyncGenericTaskTestCaseWithExpectedException()
        {
            return await Throw();
        }

        private async Task<int> Throw()
        {
            return await Task.Run(() =>
            {
                throw new InvalidOperationException();
                return 1;
            });
        }
    }
}
#endif