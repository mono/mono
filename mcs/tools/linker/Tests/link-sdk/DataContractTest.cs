using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
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

namespace LinkSdk {

	[DataContract(Namespace="XamarinBugExample")]
	public enum MyEnum : short
	{
		[EnumMember]
		Foo = 0,
		[EnumMember]
		Bar = 1,
		[EnumMember]
		Baz = 2
	}

	[DataContract(Namespace="XamarinBugExample")]
	public class MyClass
	{
		public MyClass() { }

		[DataMember]
		List<MyEnum> MyList { get; set; }
	}

	[TestFixture]
	// we want the test to be availble if we use the linker
	[Preserve (AllMembers = true)]
	public class CrashExample {

		[Test]
		// http://forums.xamarin.com/discussion/7380/type-cast-error-in-xamarin-6-4-3-on-device-only#latest
		// could not be reproduced with 6.4.4
		public void DoCrash()
		{
			System.Collections.IList lst = new List<MyEnum>();
			object value = Enum.Parse (typeof(MyEnum), "1");
			lst.Add (value); // Exception here.  List.cs throws ArgumentException.
		}
	}
}