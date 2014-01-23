//
// BinaryFormatterTest.cs - Unit tests for 
//	System.Runtime.Serialization.Formatters.Binary.BinaryFormatter
//
// Author:
//      Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;

using NUnit.Framework;

namespace MonoTests.System.Runtime.Serialization.Formatters.Binary
{
	[Serializable]
	public class SerializationTest
	{
		private int integer;
		[NonSerialized]
		private bool boolean;

		public SerializationTest (bool b, int i)
		{
			boolean = b;
			integer = i;
		}

		public bool Boolean {
			get { return boolean; }
			set { boolean = value; }
		}

		public int Integer {
			get { return integer; }
			set { integer = value; }
		}
	}

	class SurrogateSelector: ISurrogateSelector
	{
		public void ChainSelector (ISurrogateSelector selector)
		{
		}

		public ISurrogateSelector GetNextSelector ()
		{
			return null;
		}

		public ISerializationSurrogate GetSurrogate (Type type, StreamingContext context, out ISurrogateSelector selector)
		{
			selector = null;
			return null;
		}
	}

	[Serializable]
	sealed class ThisObjectReference : IObjectReference
	{
		internal static int Count;

		internal ThisObjectReference()
		{
		}

		public object GetRealObject(StreamingContext context)
		{
			Count++;
			return this;
		}
	}

	[Serializable]
	sealed class NewObjectReference : IObjectReference
	{
		internal static int Count;

		internal NewObjectReference()
		{
		}

		public object GetRealObject(StreamingContext context)
		{
			Count++;
			return new NewObjectReference();
		}
	}

	[Serializable]
	class Foo
	{
		private int privateFoo;
		protected int familyFoo;
		protected internal int familyANDAssemFoo;
		public int publicFoo;
		internal int assemblyFoo;

		public int PrivateFoo {
			get { return privateFoo; }
		}

		public int FamilyFoo {
			get { return familyFoo; }
		}

		public int FamilyANDAssemFoo {
			get { return familyANDAssemFoo; }
		}

		public int PublicFoo {
			get { return publicFoo; }
		}

		public int AssemblyFoo {
			get { return assemblyFoo; }
		}

		public virtual void Init ()
		{
			privateFoo = 1;
			familyFoo = 2;
			familyANDAssemFoo = 4;
			publicFoo = 8;
			assemblyFoo = 16;
		}
	}

	[Serializable]
	class Bar : Foo
	{
		private int privateBar;
		protected int familyBar;
		protected internal int familyANDAssemBar;
		public int publicBar;
		internal int assemblyBar;

		public int PrivateBar {
			get { return privateBar; }
		}

		public int FamilyBar {
			get { return familyBar; }
		}

		public int FamilyANDAssemBar {
			get { return familyANDAssemBar; }
		}

		public int PublicBar {
			get { return publicBar; }
		}

		public int AssemblyBar {
			get { return assemblyBar; }
		}

		public override void Init ()
		{
			privateBar = 1;
			familyBar = 2;
			familyANDAssemBar = 4;
			publicBar = 8;
			assemblyBar = 16;

			base.Init ();
		}
	}

	[Serializable]
	public class Comparable
	{
		public int Foo {
			get;
			set;
		}

		public override bool Equals (object obj)
		{
			var other = obj as Comparable;
			if (other == null)
				return false;
			return other.Foo == Foo;
		}

		public override int GetHashCode ()
		{
			return Foo;
		}
	}

	[TestFixture]
	public class BinaryFormatterTest
	{
		[Test]
		public void Constructor_Default ()
		{
			BinaryFormatter bf = new BinaryFormatter ();
#if NET_2_0
			Assert.AreEqual (FormatterAssemblyStyle.Simple, bf.AssemblyFormat, "AssemblyFormat");
#else
			Assert.AreEqual (FormatterAssemblyStyle.Full, bf.AssemblyFormat, "AssemblyFormat");
#endif
			Assert.IsNull (bf.Binder, "Binder");
			Assert.AreEqual (StreamingContextStates.All, bf.Context.State, "Context");
			Assert.AreEqual (TypeFilterLevel.Full, bf.FilterLevel, "FilterLevel");
			Assert.IsNull (bf.SurrogateSelector, "SurrogateSelector");
			Assert.AreEqual (FormatterTypeStyle.TypesAlways, bf.TypeFormat, "TypeFormat");
		}

		[Test]
		public void Constructor ()
		{
			SurrogateSelector ss = new SurrogateSelector ();
			BinaryFormatter bf = new BinaryFormatter (ss, new StreamingContext (StreamingContextStates.CrossMachine));
#if NET_2_0
			Assert.AreEqual (FormatterAssemblyStyle.Simple, bf.AssemblyFormat, "AssemblyFormat");
#else
			Assert.AreEqual (FormatterAssemblyStyle.Full, bf.AssemblyFormat, "AssemblyFormat");
#endif
			Assert.IsNull (bf.Binder, "Binder");
			Assert.AreEqual (StreamingContextStates.CrossMachine, bf.Context.State, "Context");
			Assert.AreEqual (TypeFilterLevel.Full, bf.FilterLevel, "FilterLevel");
			Assert.AreSame (ss, bf.SurrogateSelector, "SurrogateSelector");
			Assert.AreEqual (FormatterTypeStyle.TypesAlways, bf.TypeFormat, "TypeFormat");
		}

		[Test]
		public void Inheritance ()
		{
			MemoryStream ms = new MemoryStream ();
			BinaryFormatter bf = new BinaryFormatter ();

			Bar bar = new Bar ();
			bar.Init ();

			bf.Serialize (ms, bar);
			ms.Position = 0;

			Bar clone = (Bar) bf.Deserialize (ms);
			Assert.AreEqual (bar.PrivateBar, clone.PrivateBar, "#1");
			Assert.AreEqual (bar.FamilyBar, clone.FamilyBar, "#2");
			Assert.AreEqual (bar.FamilyANDAssemBar, clone.FamilyANDAssemBar, "#3");
			Assert.AreEqual (bar.PublicBar, clone.PublicBar, "#4");
			Assert.AreEqual (bar.AssemblyBar, clone.AssemblyBar, "#5");
			Assert.AreEqual (bar.PrivateFoo, clone.PrivateFoo, "#6");
			Assert.AreEqual (bar.FamilyFoo, clone.FamilyFoo, "#7");
			Assert.AreEqual (bar.FamilyANDAssemFoo, clone.FamilyANDAssemFoo, "#8");
			Assert.AreEqual (bar.PublicFoo, clone.PublicFoo, "#9");
			Assert.AreEqual (bar.AssemblyFoo, clone.AssemblyFoo, "#10");
		}

		[Test]
		public void SerializationRoundtrip ()
		{
			Stream s = GetSerializedStream ();
			BinaryFormatter bf = new BinaryFormatter ();
			SerializationTest clone = (SerializationTest) bf.Deserialize (s);
			Assert.AreEqual (Int32.MinValue, clone.Integer, "Integer");
			Assert.IsFalse (clone.Boolean, "Boolean");
		}

		[Test]
		public void SerializationUnsafeRoundtrip ()
		{
			Stream s = GetSerializedStream ();
			BinaryFormatter bf = new BinaryFormatter ();
			SerializationTest clone = (SerializationTest) bf.UnsafeDeserialize (s, null);
			Assert.AreEqual (Int32.MinValue, clone.Integer, "Integer");
			Assert.IsFalse (clone.Boolean, "Boolean");
		}
		
		[Test]
		public void NestedObjectReference ()
		{
			MemoryStream ms = new MemoryStream();
			BinaryFormatter bf = new BinaryFormatter();

			bf.Serialize(ms, new ThisObjectReference());
			bf.Serialize(ms, new NewObjectReference());
			ms.Position = 0;
			Assert.AreEqual (0, ThisObjectReference.Count, "#1");

			bf.Deserialize(ms);
			Assert.AreEqual (2, ThisObjectReference.Count, "#2");
			Assert.AreEqual (0, NewObjectReference.Count, "#3");
			try {
				bf.Deserialize(ms);
			} catch (SerializationException) {
			}
			Assert.AreEqual (101, NewObjectReference.Count, "#4");
		}

		[Test]
		public void DateTimeArray ()
		{
			DateTime [] e = new DateTime [6];
			string [] names = new string [6];

			names [0] = "Today";  e [0] = DateTime.Today;
			names [1] = "Min";    e [1] = DateTime.MinValue;
			names [2] = "Max";    e [2] = DateTime.MaxValue;
			names [3] = "BiCent"; e [3] = new DateTime (1976, 07, 04);
			names [4] = "Now";    e [4] = DateTime.Now;
			names [5] = "UtcNow"; e [5] = DateTime.UtcNow;

			BinaryFormatter bf = new BinaryFormatter ();
			MemoryStream ms = new MemoryStream ();

			bf.Serialize (ms, e);

			ms.Position = 0;
			DateTime [] a = (DateTime []) bf.Deserialize (ms);

			Assert.AreEqual (e.Length, a.Length);
			for (int i = 0; i < e.Length; ++i)
				Assert.AreEqual (e [i], a [i], names [i]);
		}

		[Test]
		public void GenericArray ()
		{
			Comparable [] a = new Comparable [1];
			a [0] = new Comparable ();

			BinaryFormatter bf = new BinaryFormatter ();
			MemoryStream ms = new MemoryStream ();

			bf.Serialize (ms, a);

			ms.Position = 0;
			Comparable [] b = (Comparable []) bf.Deserialize (ms);

			Assert.AreEqual (a.Length, b.Length, "#1");
			Assert.AreEqual (a [0], b [0], "#2");
		}

		public Stream GetSerializedStream ()
		{
			SerializationTest test = new SerializationTest (true, Int32.MinValue);
			BinaryFormatter bf = new BinaryFormatter ();
			MemoryStream ms = new MemoryStream ();
			bf.Serialize (ms, test);
			ms.Position = 0;
			return ms;
		}

#if NET_4_0
		[Test]
		public void SerializationBindToName ()
		{
			BinaryFormatter bf = new BinaryFormatter ();
			bf.AssemblyFormat = FormatterAssemblyStyle.Full;
			bf.Binder = new SimpleSerializationBinder ();
			MemoryStream ms = new MemoryStream ();

			SimpleSerializableObject o = new SimpleSerializableObject ();
			o.Name = "MonoObject";
			o.Id = 666;

			bf.Serialize (ms, o);
			ms.Position = 0;

			o = (SimpleSerializableObject)bf.Deserialize (ms);
			Assert.AreEqual ("MonoObject", o.Name);
			Assert.AreEqual (666, o.Id);
		}

		class SimpleSerializationBinder : SerializationBinder
		{
			public override Type BindToType (string assemblyName, string typeName)
			{
				// We *should* be getting a SimpleSerializableObject2 instance
				// Otherwise it means we are not getting called our BindToName method.
				if (!typeName.EndsWith ("SimpleSerializableObject2"))
					Assert.Fail ("#BindToType-TypeName");

				// We are also supposed to be getting a 9.9.9.9 version here,
				// and if we get a different version, it likely means BindToName was called.
				AssemblyName aname = Assembly.GetExecutingAssembly ().GetName ();
				aname.Version = new Version (9, 9, 9, 9);
				if (aname.ToString () != assemblyName)
					Assert.Fail ("#BindToType-AssemblyName");

				// No need to call Type.GetType
				return typeof (SimpleSerializableObject);
			}

			public override void BindToName (Type serializedType, out string assemblyName, out string typeName)
			{
				AssemblyName aname = Assembly.GetExecutingAssembly ().GetName ();
				aname.Version = new Version (9, 9, 9, 9);

				// Serialize mapping to this same assembly with 9.9.9.9 version
				// and a different type name.
				assemblyName = aname.ToString ();
				typeName = serializedType.FullName.Replace ("SimpleSerializableObject", "SimpleSerializableObject2");
			}
		}

		[Serializable]
		class SimpleSerializableObject
		{
			public string Name { get; set; }
			public int Id { get; set; }
		}

		[Test]
		public void SerializationBindToName2 ()
		{
			BinaryFormatter bf = new BinaryFormatter ();
			bf.AssemblyFormat = FormatterAssemblyStyle.Full;
			bf.Binder = new SimpleSerializationBinder2 ();
			MemoryStream ms = new MemoryStream ();

			SimpleISerializableObject o = new SimpleISerializableObject ();
			o.Name = "MonoObject";
			o.Id = 666;

			bf.Serialize (ms, o);
			ms.Position = 0;

			o = (SimpleISerializableObject)bf.Deserialize (ms);
			Assert.AreEqual ("MonoObject", o.Name);
			Assert.AreEqual (666, o.Id);

			ms.Close ();
		}

		class SimpleSerializationBinder2 : SerializationBinder
		{
			public override void BindToName (Type serializedType, out string assemblyName, out string typeName)
			{
				AssemblyName aname = Assembly.GetExecutingAssembly ().GetName ();
				aname.Version = new Version (9, 9, 9, 9);

				// Serialize mapping to this same assembly with 9.9.9.9 version
				// and a different type name.
				assemblyName = aname.ToString ();
				typeName = serializedType.FullName.Replace ("SimpleISerializableObject", "SimpleISerializableObject2");
			}

			public override Type BindToType (string assemblyName, string typeName)
			{
				// We *should* be getting a SimpleISerializableObject2 instance
				if (!typeName.EndsWith ("SimpleISerializableObject2"))
					Assert.Fail ("#BindToType-TypeName");

				// We are also supposed to be getting a 9.9.9.9 version here,
				// and if we get a different version, it likely means BindToName was called.
				AssemblyName aname = Assembly.GetExecutingAssembly ().GetName ();
				aname.Version = new Version (9, 9, 9, 9);
				if (aname.ToString () != assemblyName)
					Assert.Fail ("#BindToType-AssemblyName");

				return typeof (SimpleISerializableObject);
			}
		}

		[Serializable]
		class SimpleISerializableObject : ISerializable
		{
			public string Name { get; set; }
			public int Id { get; set; }

			public SimpleISerializableObject ()
			{
			}

			protected SimpleISerializableObject (SerializationInfo info, StreamingContext context)
			{
				Name = info.GetString ("Name");
				Id = info.GetInt32 ("Id");
			}

			public void GetObjectData (SerializationInfo info, StreamingContext context)
			{
				info.AddValue ("Name", Name);
				info.AddValue ("Id", Id);
			}
		}
#endif
	}
}
