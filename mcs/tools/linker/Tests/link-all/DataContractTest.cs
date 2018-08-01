//
// Link All Data Contract Serialization Tests
//
// Authors:
//	Sebastien Pouliot  <sebastien@xamarin.com>
//
// Copyright 2013 Xamarin Inc. All rights reserved.
//

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

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

namespace LinkAll.Serialization.DataContract {

	[TestFixture]
	// we want the tests to be available because we use the linker
	[Preserve (AllMembers = true)]
	public class DataContractTest {

		// test case from: https://bugzilla.xamarin.com/show_bug.cgi?id=11135 (public bug)

		// You also need to add custom types as '[KnownType(typeof(CustomType))]' attributes
		public static string ToXml<T> (T obj)
		{
			var sb = new StringBuilder();
			using (var x = XmlWriter.Create (sb, new XmlWriterSettings ())) {
				var s = new DataContractSerializer (typeof (T));
				s.WriteObject(x, obj);
			}
			return sb.ToString();
		}
		
		public static T FromXml<T> (string xml)
		{
			using (var r = XmlReader.Create (new StringReader (xml))) {
				var s = new DataContractSerializer (typeof (T));
				return (T) s.ReadObject (r);
			}
		}
		
		[DataContract (Namespace = "mb")]
		public class TestClass
		{
			public TestClass (SomeTypes types)
			{
				Types = types;
			}

			[DataMember]
			public SomeTypes Types { get; set; }
		}
		
		[DataContract (Namespace = "mb")][Flags] 
		public enum SomeTypes {
			[Preserve (AllMembers = true)][EnumMember] None = 0,
			[Preserve (AllMembers = true)][EnumMember] Image = 1,
			[Preserve (AllMembers = true)][EnumMember] Audio = 2,
			[Preserve (AllMembers = true)][EnumMember] Video = 4,
			[Preserve (AllMembers = true)][EnumMember] Document = 8
		}

		[Test]
		public void Flags ()
		{
			var t1 = new TestClass (SomeTypes.Audio | SomeTypes.Image);
			var st = ToXml (t1);
			var t2 = FromXml<TestClass> (st);
			Assert.AreEqual (t2.Types, t1.Types);
		}
	}
}