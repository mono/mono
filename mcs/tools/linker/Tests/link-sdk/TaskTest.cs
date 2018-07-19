using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
#if XAMCORE_2_0
using Foundation;
using ObjCRuntime;
#else
#if __MONODROID__
using Android.Runtime;
#else
#if MONOTOUCH
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
#endif
#endif
#endif
using NUnit.Framework;

namespace LinkSdk {

	[TestFixture]
	[Preserve (AllMembers = true)]
	public class TaskBugsTest {
		
		[Test]
		public void ContinueWithDifferentOptionsAreCanceledTest ()
		{
			TaskScheduler.UnobservedTaskException += (obj, evt) => evt.SetObserved ();
			var mre = new ManualResetEventSlim ();
			var task = Task.Factory.StartNew (() => mre.Wait (200));
			var contFailed = task.ContinueWith (t => {}, TaskContinuationOptions.OnlyOnFaulted);
			var contCanceled = task.ContinueWith (t => {}, TaskContinuationOptions.OnlyOnCanceled);
			var contSuccess = task.ContinueWith (t => {}, TaskContinuationOptions.OnlyOnRanToCompletion);

			mre.Set ();
			contSuccess.Wait (100);

			Assert.True (contSuccess.IsCompleted, "contSuccess.IsCompleted");
			Assert.True (contFailed.IsCompleted, "contFailed.IsCompleted");
			Assert.True (contCanceled.IsCompleted, "contCanceled.IsCompleted");
			Assert.False (contSuccess.IsCanceled, "contSuccess.IsCanceled");
			Assert.True (contFailed.IsCanceled, "contFailed.IsCanceled");
			Assert.True (contCanceled.IsCanceled, "contCanceled.IsCanceled");
		}

		[Test]
		public void ContinueWhenAll_WithMixedCompletionState ()
		{
			TaskScheduler.UnobservedTaskException += (obj, evt) => evt.SetObserved ();
			var mre = new ManualResetEventSlim ();
			var task = Task.Factory.StartNew (() => mre.Wait (200));
			var contFailed = task.ContinueWith (t => {}, TaskContinuationOptions.OnlyOnFaulted);
			var contCanceled = task.ContinueWith (t => {}, TaskContinuationOptions.OnlyOnCanceled);
			var contSuccess = task.ContinueWith (t => {}, TaskContinuationOptions.OnlyOnRanToCompletion);
			bool ran = false;

			var cont = Task.Factory.ContinueWhenAll (new Task[] { contFailed, contCanceled, contSuccess }, _ => ran = true);

			mre.Set ();
			cont.Wait (200);

			Assert.True (ran, "ran");
			Assert.That (cont.Status, Is.EqualTo (TaskStatus.RanToCompletion), "Status");
		}
	}
}