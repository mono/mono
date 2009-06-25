//
// System.Runtime.Serialization.FormatterServicesTests: NUnit test
//
// Authors: 
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2002 Ximian Inc. (http://www.ximian.com)
//

using NUnit.Framework;
using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace MonoTests.System.Runtime.Serialization
{
	public class FormatterServicesTests
	{
		public void TestClass1 ()
		{
			DerivedClass1 derived = new DerivedClass1 ();
			derived.anotherInt = 69;
			MemberInfo [] members = FormatterServices.GetSerializableMembers (derived.GetType ());
			Assert.IsTrue (members != null, "#01");
			Assert.AreEqual (3, members.Length, "#02");

			object [] data = FormatterServices.GetObjectData (derived, members);
			Assert.IsTrue (data != null, "#03");
			Assert.AreEqual (3, data.Length, "#04");

			DerivedClass1 o = (DerivedClass1) FormatterServices.GetUninitializedObject (derived.GetType ());
			Assert.IsTrue (o != null, "#05");

			o = (DerivedClass1) FormatterServices.PopulateObjectMembers (o, members, data);
			Assert.IsTrue (o != null, "#06");
			Assert.AreEqual ("hola", o.Hello, "#07");
			Assert.AreEqual (21, o.IntBase, "#08");
			Assert.AreEqual (1, o.IntDerived, "#09");
			Assert.AreEqual (69, o.anotherInt, "#10");
			Assert.AreEqual ("hey", DerivedClass1.hey, "#11");
		}
	}

	[Serializable]
	class BaseClass1
	{
		public string hello = "hola";
		static int intBase = 21;

		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}

		public override bool Equals (object o)
		{
			BaseClass1 bc = o  as BaseClass1;
			if (o == null)
				return false;

			if (hello != "hola")
				return false;

			return (hello == bc.hello);
		}

		public string Hello
		{
			get {
				return hello;
			}
		}

		public int IntBase
		{
			get {
				return intBase;
			}
		}
	}
	
	[Serializable]
	class DerivedClass1 : BaseClass1
	{
		private int intDerived = 1;
		[NonSerialized] public int publicint = 2;
		public int anotherInt = 22;
		public static string hey = "hey";

		public string Name 
		{
			get {
				return "Ahem";
			}
		}

		public void SomeMethod ()
		{
			/* Does nothing */
		}

		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}

		public override bool Equals (object o)
		{
			DerivedClass1 dc = o  as DerivedClass1;
			if (o == null)
				return false;

			if (anotherInt != 22 || hey != "hey")
				return false;
			
			return (anotherInt == dc.anotherInt);
		}

		public int IntDerived
		{
			get {
				return intDerived;
			}
		}
	}
}

