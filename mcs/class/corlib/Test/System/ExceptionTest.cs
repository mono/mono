//
// ExceptionTest.cs - NUnit Test Cases for the System.Exception class
// 
// Linus Upson (linus@linus.com)
//

using System;
using NUnit.Framework;

namespace MonoTests.System
{
	public class ExceptionTest : TestCase
	{
		public ExceptionTest() {}
		
		// This test makes sure that exceptions thrown on block boundaries are
		// handled in the correct block. The meaning of the 'caught' variable is
		// a little confusing since there are two catchers: the method being
		// tested the the method calling the test. There is probably a better
		// name, but I can't think of it right now.
		
		public void TestThrowOnBlockBoundaries()
		{
			bool caught;
			
			try {
				caught = false;
				ThrowBeforeTry();
			} catch {
				caught = true;
			}
			Assert("Exceptions thrown before try blocks should not be caught", caught);
			
			try {
				caught = false;
				ThrowAtBeginOfTry();
			} catch {
				caught = true;
			}
			Assert("Exceptions thrown at begin of try blocks should be caught", !caught);

			try {
				caught = false;
				ThrowAtEndOfTry();
			} catch {
				caught = true;
			}
			Assert("Exceptions thrown at end of try blocks should be caught", !caught);

			try {
				caught = false;
				ThrowAtBeginOfCatch();
			} catch {
				caught = true;
			}
			Assert("Exceptions thrown at begin of catch blocks should not be caught", caught);

			try {
				caught = false;
				ThrowAtEndOfCatch();
			} catch {
				caught = true;
			}
			Assert("Exceptions thrown at end of catch blocks should not be caught", caught);

			try {
				caught = false;
				ThrowAtBeginOfFinally();
			} catch {
				caught = true;
			}
			Assert("Exceptions thrown at begin of finally blocks should not be caught", caught);

			try {
				caught = false;
				ThrowAtEndOfFinally();
			} catch {
				caught = true;
			}
			Assert("Exceptions thrown at end of finally blocks should not be caught", caught);

			try {
				caught = false;
				ThrowAfterFinally();
			} catch {
				caught = true;
			}
			Assert("Exceptions thrown after finally blocks should not be caught", caught);
		}
		
		private static void DoNothing()
		{
		}

		private static void ThrowException()
		{
			throw new Exception();
		}
		
		private static void ThrowBeforeTry()
		{
			ThrowException();
			try {
				DoNothing();
			} catch (Exception) {
				DoNothing();
			}
		}

		private static void ThrowAtBeginOfTry()
		{
			DoNothing();
			try {
				ThrowException();
				DoNothing();
			} catch (Exception) {
				DoNothing();
			}
		}

		private static void ThrowAtEndOfTry()
		{
			DoNothing();
			try {
				DoNothing();
				ThrowException();
			} catch (Exception) {
				DoNothing();
			}
		}

		private static void ThrowAtBeginOfCatch()
		{
			DoNothing();
			try {
				DoNothing();
				ThrowException();
			} catch (Exception) {
				throw;
			}
		}

		private static void ThrowAtEndOfCatch()
		{
			DoNothing();
			try {
				DoNothing();
				ThrowException();
			} catch (Exception) {
				DoNothing();
				throw;
			}
		}

		private static void ThrowAtBeginOfFinally()
		{
			DoNothing();
			try {
				DoNothing();
				ThrowException();
			} catch (Exception) {
				DoNothing();
			} finally {
				ThrowException();
				DoNothing();
			}
		}

		private static void ThrowAtEndOfFinally()
		{
			DoNothing();
			try {
				DoNothing();
				ThrowException();
			} catch (Exception) {
				DoNothing();
			} finally {
				DoNothing();
				ThrowException();
			}
		}

		private static void ThrowAfterFinally()
		{
			DoNothing();
			try {
				DoNothing();
				ThrowException();
			} catch (Exception) {
				DoNothing();
			} finally {
				DoNothing();
			}
			ThrowException();
		}
	}
}
