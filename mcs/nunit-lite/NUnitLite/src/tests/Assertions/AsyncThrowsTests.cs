// ***********************************************************************
// Copyright (c) 2012 Charlie Poole
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

#if NET_4_5
using System;
using System.Threading.Tasks;
using NUnit.Framework.Constraints;

namespace NUnit.Framework.Assertions
{
	[TestFixture]
	public class AsyncThrowsTests
	{
		private readonly TestDelegate _noThrowsVoid = new TestDelegate(async () => await Task.Yield());
		private readonly ActualValueDelegate<Task> _noThrowsAsyncTask = async () => await Task.Yield();
		private readonly ActualValueDelegate<Task<int>> _noThrowsAsyncGenericTask = async () => await ReturnOne();
		private readonly TestDelegate _throwsAsyncVoid = new TestDelegate(async () => await ThrowAsyncTask());
		private readonly TestDelegate _throwsSyncVoid = new TestDelegate(async () => { throw new InvalidOperationException(); });
		private readonly ActualValueDelegate<Task> _throwsAsyncTask = async () => await ThrowAsyncTask();
		private readonly ActualValueDelegate<Task<int>> _throwsAsyncGenericTask = async () => await ThrowAsyncGenericTask();

		private static ThrowsConstraint ThrowsInvalidOperationExceptionConstraint
		{
			get { return new ThrowsConstraint(new ExactTypeConstraint(typeof(InvalidOperationException))); }
		}

		[Test]
		public void ThrowsConstraintVoid()
		{
			Assert.IsTrue(ThrowsInvalidOperationExceptionConstraint.Matches(_throwsAsyncVoid));
		}

		[Test]
		public void ThrowsConstraintVoidRunSynchronously()
		{
			Assert.IsTrue(ThrowsInvalidOperationExceptionConstraint.Matches(_throwsSyncVoid));
		}

		[Test]
		public void ThrowsConstraintAsyncTask()
		{
			Assert.IsTrue(ThrowsInvalidOperationExceptionConstraint.Matches(_throwsAsyncTask));
		}

		[Test]
		public void ThrowsConstraintAsyncGenericTask()
		{
			Assert.IsTrue(ThrowsInvalidOperationExceptionConstraint.Matches(_throwsAsyncGenericTask));
		}

		[Test]
		public void ThrowsNothingConstraintVoidSuccess()
		{
			Assert.IsTrue(new ThrowsNothingConstraint().Matches(_noThrowsVoid));
		}

		[Test]
		public void ThrowsNothingConstraintVoidFailure()
		{
			Assert.IsFalse(new ThrowsNothingConstraint().Matches(_throwsAsyncVoid));
		}

		[Test]
		public void ThrowsNothingConstraintTaskVoidSuccess()
		{
			Assert.IsTrue(new ThrowsNothingConstraint().Matches(_noThrowsAsyncTask));
		}

		[Test]
		public void ThrowsNothingConstraintTaskFailure()
		{
			Assert.IsFalse(new ThrowsNothingConstraint().Matches(_throwsAsyncTask));
		}

		[Test]
		public void AssertThrowsVoid()
		{
			Assert.Throws(typeof(InvalidOperationException), _throwsAsyncVoid);
		}

		[Test]
		public void AssertThatThrowsVoid()
		{
			Assert.That(_throwsAsyncVoid, Throws.TypeOf<InvalidOperationException>());
		}
		
		[Test]
		public void AssertThatThrowsTask()
		{
			Assert.That(_throwsAsyncTask, Throws.TypeOf<InvalidOperationException>());
		}

		[Test]
		public void AssertThatThrowsGenericTask()
		{
			Assert.That(_throwsAsyncGenericTask, Throws.TypeOf<InvalidOperationException>());
		}

		[Test]
		public void AssertThatThrowsNothingVoidSuccess()
		{
			Assert.That(_noThrowsVoid, Throws.Nothing);
		}

		[Test]
		public void AssertThatThrowsNothingTaskSuccess()
		{
			Assert.That(_noThrowsAsyncTask, Throws.Nothing);
		}

		[Test]
		public void AssertThatThrowsNothingGenericTaskSuccess()
		{
			Assert.That(_noThrowsAsyncGenericTask, Throws.Nothing);
		}

		[Test]
		public void AssertThatThrowsNothingVoidFailure()
		{
			Assert.Throws<AssertionException>(() => Assert.That(_throwsAsyncVoid, Throws.Nothing));
		}

		[Test]
		public void AssertThatThrowsNothingTaskFailure()
		{
			Assert.Throws<AssertionException>(() => Assert.That(_throwsAsyncTask, Throws.Nothing));
		}

		[Test]
		public void AssertThatThrowsNothingGenericTaskFailure()
		{
			Assert.Throws<AssertionException>(() => Assert.That(_throwsAsyncGenericTask, Throws.Nothing));
		}

		[Test]
		public void AssertThrowsAsync()
		{
			Assert.Throws<InvalidOperationException>(_throwsAsyncVoid);
		}

		[Test]
		public void AssertThrowsSync()
		{
			Assert.Throws<InvalidOperationException>(_throwsSyncVoid);
		}

		private static async Task ThrowAsyncTask()
		{
			await ReturnOne();
			throw new InvalidOperationException();
		}

		private static async Task<int> ThrowAsyncGenericTask()
		{
			await ThrowAsyncTask();
			return await ReturnOne();
		}

		private static Task<int> ReturnOne()
		{
			return Task.Run(() => 1);
		}
	}
}
#endif