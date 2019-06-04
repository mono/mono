using System;
using System.IO;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.UITest.Android;
using Xamarin.UITest.Queries;

namespace RuntimeTests
{
	[TestFixture]
	public class Tests
	{
		IApp app;

		[SetUp]
		public void BeforeEachTest ()
		{
			
			// TODO: If the Android app being tested is included in the solution then open
			// the Unit Tests window, right click Test Apps, select Add App Project
			// and select the app projects that should be tested.
			app = ConfigureApp
				.Android
				// TODO: Update this path to point to your Android app and uncomment the
				// code if the app is not included in the solution.
				.ApkFile ("/Users/kumpera/src/mono-sdks/android/AndroidRunner-debug.apk")
				.StartApp ();
		}

		//[Test]
		//public void Repl ()
		//{
		//	app.Repl ();
		//}

		void RunTest (string testsuite)
		{
			app.WaitForElement ("button");
			app.WaitForElement ("input");
			app.Screenshot ("app-launched");

			app.Tap ("input");
			app.EnterText (testsuite);
			app.Screenshot ("test-suite-filled");
			app.Tap ("button");
			app.Screenshot ("button-tapped");
			Thread.Sleep (500);

			int c = 0;
			string text;
			while ((text = app.Query (x => x.Id ("text")) [0].Text) == "IN-PROGRESS") {
				Console.WriteLine ("AFTER TAP STATUS: {0}, STEPS: {1}", text, c);
				Thread.Sleep (1000);
				++c;
				if (c > (10 * 60))
					break;
			}
			app.Screenshot ("loop-finished");

			Assert.AreEqual ("PASS", app.Query (x => x.Id ("text")) [0].Text, "ACTUAL RESULT");
			app.Screenshot ("after-the-last-assert");
		}

		[Test]
		public void TestMini ()
		{
			RunTest ("mini");
		}

		[Test]
		public void TestCorlib ()
		{
			RunTest ("corlib");
		}

		[Test]
		public void TestSystem ()
		{
			RunTest ("system");
		}
	}
}
