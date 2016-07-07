using NUnit.Framework;

using System;
using Mono.Tasklets;

namespace MonoTests.System
{
    [TestFixture]
	public class ContinuationsTest {

		private Continuation _contA = new Continuation();

		private int total = 0;

		[Test]
		public void TestContinuationsLoop() {
			_contA.Mark();
			int value = 0;
			int ret = _contA.Store(0);
			for(int i = ret; i < 10; i++) {
				value += i;
			}

			if(value > 0) {
				total += value;
				_contA.Restore(ret + 1);
			}

			Assert.AreEqual(total,330);
		}

		[Test]
		public void Yielding() {
			Continuation baseCont = new Continuation();
			Continuation taskCont = new Continuation();
			int yields = 0;
			baseCont.Mark();
			taskCont.Mark();
			
			// Store the base continuation to start the task
			if (baseCont.Store(0) == 0) {
				bool done = false;
				int count = 0;

				while (!done) {
					// Do stuff for the task.
					++count;
					
					// This task is counting to 100.
					if (count == 100) {
						done = true;
					}

					// Yield every 10 loops
					else if (count % 10 == 0) {

						// To yield, store the task continuation then restore
						// the base continuation.
						if (taskCont.Store(0) == 0) {
							baseCont.Restore(1);
						}
					}
				}
			}
			// When restored, 'Store' will return what was passed to Restore, in this case 1 so fall here.
			else {
				// Count the yields, then go back to the task.
				++yields;
				taskCont.Restore(1);
			}

			Assert.AreEqual(yields,9);
		}
			
		
	}
}
// vim: noexpandtab
// Local Variables:
// tab-width: 4
// c-basic-offset: 4
// indent-tabs-mode: t
// End:
