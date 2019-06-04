using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel
{
	[TestFixture]
	public class SynchronizedCollectionTest
	{
		[Test] // from https://bugzilla.xamarin.com/show_bug.cgi?id=43447
		public void TestConcurrentAddRemove ()
		{
			var c = new SynchronizedCollection<int> ();
			for (int i = 0; i < 10000; i++)
			{
				c.Add(i);
			}

			var wait = new CountdownEvent (2);
			ThreadStart add = () =>
			{
				wait.Signal ();
				wait.Wait ();
				for (int i = 10000; i < 20000; i++)
				{
					c.Add (i);
				}
			};

			ThreadStart remove = () =>
			{
				wait.Signal ();
				wait.Wait ();
				for(int i = 9999; i >= 0; i--)
				{
					c.Remove (i);
				}
			};

			var t1 = new Thread (add);
			var t2 = new Thread (remove);
			t1.Start ();
			t2.Start ();

			t1.Join ();
			t2.Join ();
		}
	}
}
