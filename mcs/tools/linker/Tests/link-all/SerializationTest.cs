//
// Link All [Regression] Tests for Serialization
//
// Authors:
//	Sebastien Pouliot  <sebastien@xamarin.com>
//
// Copyright 2016 Xamarin Inc. All rights reserved.
//

using System;
using System.Runtime.Serialization;
#if XAMCORE_2_0
using Foundation;
#else
#if MONOTOUCH
using MonoTouch.Foundation;
#endif
#endif
using NUnit.Framework;

namespace LinkAll
{
	static class Helper
	{
		public static Type GetType (string name)
		{
			return Type.GetType (name);
		}

		public static Type GetType (string typeName, bool throwOnError)
		{
			return Type.GetType (typeName, throwOnError);
		}
	}
}

namespace LinkAll.Serialization {

	[Serializable]
	public class Unused {
		
		[OnDeserializing]
		void Deserializing ()
		{
		}

		[OnDeserialized]
		void Deserialized ()
		{
		}

		[OnSerializing]
		void Serializing ()
		{
		}

		[OnSerialized]
		void Serialized ()
		{
		}
	}
	
	[Serializable]
	public class Used {

		[OnDeserializing]
		void Deserializing ()
		{
		}

		[OnDeserialized]
		void Deserialized ()
		{
		}
	
		[OnSerializing]
		void Serializing ()
		{
		}

		[OnSerialized]
		void Serialized ()
		{
		}
	}
	
	[TestFixture]
	// we want the tests to be available because we use the linker
	[Preserve (AllMembers = true)]
	public class SerializationAttributeTests {
	
		[Test]
		public void UnusedType ()
		{
			// the serialization attributes only keeps the method(s) if the type was used
			var t = Helper.GetType ("LinkAll.Serialization.Unused");
			// since it's not used in the app then it's removed by the linker
			Assert.Null (t, "type");
		}

		[Test]
		public void UsedType ()
		{
			// the serialization attributes only keeps the method(s) if the type was used
			var t = Helper.GetType ("LinkAll.Serialization.Used");
			// since it's used here...
			Assert.NotNull (new Used (), "reference");
			// it's not removed by the linker
			Assert.NotNull (t, "type");
			// and since it's not the 4 decorated methods are also kept (even if uncalled)
			Assert.That (t.GetMethods ().Length, Is.EqualTo (4), "4");	  
		}
	}
}
