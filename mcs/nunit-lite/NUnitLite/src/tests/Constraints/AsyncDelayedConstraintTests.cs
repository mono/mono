#if NET_4_5
using System;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace NUnit.Framework.Constraints.Tests
{
	[TestFixture]
	public class AsyncDelayedConstraintTests
	{
		[Test]
		public void ConstraintSuccess()
		{
			Assert.IsTrue(new DelayedConstraint(new EqualConstraint(1), 100)
				.Matches(async () => await One()));
		}

		[Test]
		public void ConstraintFailure()
		{
			Assert.IsFalse(new DelayedConstraint(new EqualConstraint(2), 100)
				.Matches(async () => await One()));
		}

		[Test]
		public void ConstraintError()
		{
			Assert.Throws<InvalidOperationException>(() => 
				new DelayedConstraint(new EqualConstraint(1), 100).Matches(async () => await Throw()));
		}

		[Test]
		public void ConstraintVoidDelegateFailureAsDelegateIsNotCalled()
		{
			Assert.IsFalse(new DelayedConstraint(new EqualConstraint(1), 100)
				.Matches(new TestDelegate(async () => { await One(); })));
		}

		[Test]
		public void ConstraintVoidDelegateExceptionIsFailureAsDelegateIsNotCalled()
		{
			Assert.IsFalse(new DelayedConstraint(new EqualConstraint(1), 100)
				.Matches(new TestDelegate(async () => { await Throw(); })));
		}

		[Test]
		public void SyntaxSuccess()
		{
			Assert.That(async () => await One(), Is.EqualTo(1).After(100));
		}


		[Test]
		public void SyntaxFailure()
		{
			Assert.Throws<AssertionException>(() =>
				Assert.That(async () => await One(), Is.EqualTo(2).After(100)));
		}

		[Test]
		public void SyntaxError()
		{
			Assert.Throws<InvalidOperationException>(() =>
				Assert.That(async () => await Throw(), Is.EqualTo(1).After(100)));
		}

		[Test]
		public void SyntaxVoidDelegateExceptionIsFailureAsCodeIsNotCalled()
		{
			Assert.Throws<AssertionException>(() =>
				Assert.That(new TestDelegate(async () => await Throw()), Is.EqualTo(1).After(100)));
		}

		private static async Task<int> One()
		{
			return await Task.Run(() => 1);
		}

		private static async Task Throw()
		{
			await One();
			throw new InvalidOperationException();
		}
	}
}
#endif