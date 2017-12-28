using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;

namespace MonoTests.System.Threading.Tasks
{
    [TestFixture]
    public class ThreadPrincipalTests
    {   
        [Test]
        public void PrincipalFlowsToAsyncTask ()
        {
            var t = _PrincipalFlowsToAsyncTask();
            t.GetAwaiter().GetResult();
        }

        public async Task _PrincipalFlowsToAsyncTask ()
        {    
            var mockIdentity = new MockIdentity {
                AuthenticationType = "authtype",
                IsAuthenticated = true,
                Name = "name"
            };
            var mockPrincipal = new MockPrincipal {
                Identity = mockIdentity
            };          
            var oldPrincipal = Thread.CurrentPrincipal;
            Thread.CurrentPrincipal = mockPrincipal;          

            try {
                await Task.Factory.StartNew(async () =>
                {
                    var newThreadId = Thread.CurrentThread.ManagedThreadId; // on different thread.
                    Assert.IsTrue(Thread.CurrentPrincipal.Identity.IsAuthenticated);
                    Assert.AreEqual(mockPrincipal, Thread.CurrentPrincipal);
                   
                    await Task.Factory.StartNew(() =>
                    {
                        // still works even when nesting..
                        newThreadId = Thread.CurrentThread.ManagedThreadId;
                        Assert.IsTrue(Thread.CurrentPrincipal.Identity.IsAuthenticated);
                        Assert.AreEqual(mockPrincipal, Thread.CurrentPrincipal);

                    }, TaskCreationOptions.LongRunning);
                }, TaskCreationOptions.LongRunning);

                await Task.Run(() =>
                {
                    // Following works on NET4.7 and fails under Xamarin.Android.
                    var newThreadId = Thread.CurrentThread.ManagedThreadId;
                    Assert.IsTrue(Thread.CurrentPrincipal.Identity.IsAuthenticated);
                    Assert.AreEqual(mockPrincipal, Thread.CurrentPrincipal);
                });
            } finally {
                Thread.CurrentPrincipal = oldPrincipal;
            }
        }
    }

    public class MockPrincipal : IPrincipal {
        public IIdentity Identity { get; set; }
        public bool IsInRole (string role) {
            return true;
        }
    }

    public class MockIdentity : IIdentity {
        public string AuthenticationType { get; set; }
        public bool IsAuthenticated { get; set; }
        public string Name { get; set; }
    }
}