// MMRUThreadTest.cs - NUnit Test Cases for the Mono.Tasklets.MMRUThread class
//
// Authors:
//   Mike Rieker <mrieker@nii.net>
//

using System;
using System.IO;
using System.Text;

using Mono.Tasklets;
using NUnit.Framework;

namespace MonoTests.Mono.Tasklets
{
	[TestFixture]
	public class MMRUThreadTest
	{
		[SetUp]
		public void SetUp ()
		{
		}

		[TearDown]
		public void TearDown ()
		{
		}

		[Test]
		public void ExceptionPassingMadness ()
		{
			Console.WriteLine ("testing exception passing");
			ThreadExcept thex = new ThreadExcept ();
			Exception ex = thex.StartEx ();
			Assert.IsNotNull (ex, "#1");
			Assert.AreEqual (ex.Message, "suspend to start", "#2");
			ex = thex.ResumeEx (new Exception ("resume to suspend"));
			Assert.IsNotNull (ex, "#3");
			Assert.AreEqual (ex.Message, "exit to resume", "#4");
			thex.Dispose ();
			Assert.IsTrue (thex.success, "#5");
			Console.WriteLine ("exception passing works!");
		}

		[Test]
		public void StackScanning ()
		{
			Console.WriteLine ("testing stack scanned by GC");

			ThreadOne thread1 = null;
			ThreadTwo thread2 = null;

			/*
			 * Pound out some thread switching with garbage collection.
			 */
			for (int i = 0; i < 100; i ++) {

				/*
				 * Create a couple MMRUThread derived objects.
				 * Use different classes so we switch RIPs and such.
				 */
				thread1 = new ThreadOne ();
				thread2 = new ThreadTwo ();

				GarbageCollect ();
				thread1.Start ();
				GarbageCollect ();
				thread2.Start ();
				GarbageCollect ();

				/*
				 * This is our 'scheduler' loop.  Run threads until all are
				 * inactive.
				 */
				while ((thread1.Active () < 0) || (thread2.Active () < 0)) {
					if (thread1.Active () < 0) {
						GarbageCollect ();
						thread1.Resume ();
						GarbageCollect ();
					}
					if (thread2.Active () < 0) {
						GarbageCollect ();
						thread2.Resume ();
						GarbageCollect ();
					}
				}

				Assert.AreEqual (thread1.Active (), 0, "#6");
				Assert.AreEqual (thread2.Active (), 0, "#7");
			}

			Console.WriteLine ("stack is scanned by GC!");
		}

		/*
		 * Run the garbage collector.
		 * Sense that it actually ran.
		 * Make sure ThreadOne and ThreadTwo's gc objects still alive afterward.
		 */
		public static void GarbageCollect ()
		{
			new GCSense (0);                                   // create an object that we can sense

			// repeatedly create garbazhe until gcSense(0) is garbage collected
			do {
				new GCSense ();                            // an unindexed object
				GC.Collect ();                             // hopefully force it to die
				GC.WaitForPendingFinalizers ();            // hopefully wait for it to die
			} while (!GCSense.gcDestroyed[0]);

			// hopefully the microthread's gcSense1 and gcSense2 stack vars are visible to GC
			// ... so they didn't get garbage collected
			if (GCSense.gcAllocated[1]) {
				Assert.IsFalse (GCSense.gcDestroyed[1], "#17");
			}
			if (GCSense.gcAllocated[2]) {
				Assert.IsFalse (GCSense.gcDestroyed[2], "#18");
			}
		}
	}

	public class ThreadExcept : MMRUThread {
		public bool success = false;
		public override Exception MainEx ()
		{
			Exception ex;

			ex = SuspendEx (new Exception ("suspend to start"));
			if (ex.Message != "resume to suspend") throw ex;
			success = true;
			ex = ExitEx (new Exception ("exit to resume"));
			success = false;
			throw ex;
		}
	}

	public class ThreadOne : MMRUThread {

		public ThreadOne ()
		{
			///Console.WriteLine ("ThreadOne {0}", this.GetHashCode());
		}

		~ThreadOne ()
		{
			///Console.WriteLine ("ThreadOne destroyed");
		}

		public override void Main ()
		{
			GCSense gcSense1 = new GCSense (1);
			GCSense.gcAllocated[1] = true;
			MMRUThreadTest.GarbageCollect ();
			Suspend ();
			MMRUThreadTest.GarbageCollect ();
			Suspend ();
			MMRUThreadTest.GarbageCollect ();
			GCSense.gcAllocated[1] = false;
			GC.KeepAlive (gcSense1);
			gcSense1 = null;
		}
	}

	public class ThreadTwo : MMRUThread {

		public ThreadTwo ()
		{
			///Console.WriteLine ("ThreadTwo {0}", this.GetHashCode());
		}

		~ThreadTwo ()
		{
			///Console.WriteLine ("ThreadTwo destroyed");
		}

		public override void Main ()
		{
			GCSense gcSense2 = new GCSense (2);
			GCSense.gcAllocated[2] = true;
			MMRUThreadTest.GarbageCollect ();
			Suspend ();
			MMRUThreadTest.GarbageCollect ();
			Suspend ();
			MMRUThreadTest.GarbageCollect ();
			Suspend ();
			MMRUThreadTest.GarbageCollect ();
			GCSense.gcAllocated[2] = false;
			GC.KeepAlive (gcSense2);
			gcSense2 = null;
			Assert.IsNull (gcSense2, "#19");
			Exit ();
			Assert.IsTrue (false, "#20");
		}
	}

	/*
	 * Sense when the garbage collector has run.
	 */
	public class GCSense {
		public static volatile int gcUnindexed = 0;
		public static volatile bool[] gcAllocated = new bool[3];
		public static volatile bool[] gcDestroyed = new bool[3];

		private int index = -1;

		public GCSense ()
		{
			System.Threading.Interlocked.Increment (ref gcUnindexed);
		}

		public GCSense (int i)
		{
			index = i;
			gcDestroyed[index] = false;
		}
		~GCSense ()
		{
			if (index < 0) {
				System.Threading.Interlocked.Decrement (ref gcUnindexed);
			} else {
				gcDestroyed[index] = true;
			}
		}
	}
}
