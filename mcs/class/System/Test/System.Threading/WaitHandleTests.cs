using NUnit.Framework;

using System;
using System.Security.AccessControl;
using System.Threading;

namespace MonoTests.System.Threading {
    [TestFixture]
    public class WaitHandleTests {
        SynchronizationContext OriginalContext;
        TestSynchronizationContext TestContext;

        [SetUp]
        public void SetUp ()
        {
            OriginalContext = SynchronizationContext.Current;
            TestContext = new TestSynchronizationContext();
            TestContext.SetWaitNotificationRequired();
            SynchronizationContext.SetSynchronizationContext(TestContext);
        }

        [TearDown]
        public void TearDown ()
        {
            SynchronizationContext.SetSynchronizationContext(OriginalContext);
        }

        [Test]
#if MONODROID
        [Ignore("https://github.com/mono/mono/issues/8349")]
#endif
        public void WaitHandle_WaitOne_SynchronizationContext ()
        {
            var e = new ManualResetEvent(false);
            TestContext.WaitAction = () => e.Set();
            Assert.IsTrue(e.WaitOne(0));
        }

        [Test]
        public void WaitHandle_WaitAll_SynchronizationContext ()
        {
            var e1 = new ManualResetEvent(false);
            var e2 = new ManualResetEvent(false);
            TestContext.WaitAction = () => {
                e1.Set();
                e2.Set();
            };
            Assert.IsTrue(WaitHandle.WaitAll(new[] { e1, e2 }, 0));
        }
    }

    class TestSynchronizationContext : SynchronizationContext
    {
        public Action WaitAction { get; set; }

        public new void SetWaitNotificationRequired ()
        {
            base.SetWaitNotificationRequired();
        }

        public override int Wait (IntPtr[] waitHandles, bool waitAll, int millisecondsTimeout)
        {
            WaitAction?.Invoke();
            return base.Wait(waitHandles, waitAll, millisecondsTimeout);
        }
    }
}

