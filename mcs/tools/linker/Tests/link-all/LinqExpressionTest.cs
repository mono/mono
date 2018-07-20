using System;
using System.Linq.Expressions;
#if XAMCORE_2_0
using Foundation;
#else
#if __MONODROID__
using Android.Runtime;
#else
#if MONOTOUCH
using MonoTouch.Foundation;
#endif
#endif
#endif
using NUnit.Framework;

namespace LinkAll.Linq {

	[TestFixture]
	// we want the tests to be available because we use the linker
	[Preserve (AllMembers = true)]
	public class LinqExpressionTest {

		// Normally this test would have been included in linksdk.app - but there's already
		// some code that (indirectly) includes the code we want to test (so it can't fail)

		delegate object Bug14863Delegate ();

		[Test]
		public void Expression_T_Ctor ()
		{
			var ctor = typeof (LinqExpressionTest).GetConstructor (Type.EmptyTypes);
			var expr = Expression.New (ctor, new Expression [0]);
			Assert.NotNull (Expression.Lambda (typeof(Bug14863Delegate), expr, null), "Lambda");
			// note: reflection is used to create an instance of Expression<TDelegate> using an internal ctor
			// it can be indirectly "preserved" by other code (in Expression) but it can fail in other cases
			// ref: https://bugzilla.xamarin.com/show_bug.cgi?id=14863
		}
	}
}