using NUnit.Framework;

using System;
using Mono.Tasklets;

namespace MonoTests.System
{
    [TestFixture]
    public class ContinuationsTest
    {
        [TestFixtureSetUp]
        public void FixtureSetUp ()
        {
            try {
                var temp = new Continuation ();
            } catch (NotImplementedException) {
                Assert.Ignore ("This platform doesn't support Tasklets.");
            }
        }

        int total = 0;

        [Test]
        public void TestContinuationsLoop()
        {
            Continuation _contA = new Continuation();

            _contA.Mark();
            int value = 0;
            int ret = _contA.Store(0);
            for (int i = ret; i < 10; i++) {
                value += i;
            }

            if (value > 0) {
                total += value;
                _contA.Restore(ret + 1);
            }

            Assert.AreEqual(total, 330);
        }

        private int yields = 0;

        [Test]
        public void Yielding()
        {
            Continuation baseCont = new Continuation();
            Continuation taskCont = new Continuation();

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

            Assert.AreEqual(9, yields);
        }


        public class MicroThread
        {

            public void Yield()
            {
                if (MyThread.Store(0) == 0) {
                    MainThread.Restore(1);
                }
            }

            public void Resume()
            {
                if (MainThread.Store(0) == 0) {
                    MyThread.Restore(1);
                }
            }

            public void DoWork(Action action)
            {
                if (MainThread.Store(0) == 0) {
                    action();
                    Done = true;
                    MainThread.Restore(1);
                }
            }

            public bool Done = false;
            public Continuation MainThread = new Continuation();
            public Continuation MyThread = new Continuation();
        }

        public class MicroBJob
        {
            private int _Count = 0;
            public int Count
            {
                get { return _Count; }
                set { _Count = value; }
            }

            public MicroThread MicroThread;
            public void Work()
            {
                while (Count < 100) {
                    ++Count;
                    if (Count % 10 == 0) {
                        MicroThread.Yield();
                    }
                }
            }
        }

        [Test]
        public void MicroThreadTest()
        {
            MicroThread microA = new MicroThread();
            MicroThread microB = new MicroThread();

            microA.MainThread.Mark();
            microA.MyThread.Mark();
            microB.MainThread.Mark();
            microB.MyThread.Mark();

            Assert.AreEqual(false, microA.Done);
            Assert.AreEqual(false, microB.Done);

            microA.DoWork(() =>
            {
                int count = 0;
                while (count < 100) {
                    ++count;
                    if (count % 10 == 0) {
                        microA.Yield();
                    }
                }
            });

            MicroBJob jobB = new MicroBJob();
            jobB.MicroThread = microB;

            microB.DoWork(jobB.Work);

            Assert.AreEqual(false, microA.Done);
            Assert.AreEqual(false, microB.Done);

            int yields = 0;
            while (yields < 20) {
                if (!microA.Done) microA.Resume();
                if (!microB.Done) microB.Resume();
                if (microA.Done && microB.Done) break;
                ++yields;
            }

            Assert.AreEqual(true, microA.Done);
            Assert.AreEqual(true, microB.Done);
            Assert.AreEqual(100, jobB.Count);
            Assert.AreEqual(9, yields);
        }
    }
}
