//
// System.Runtime.Serialization.SerializationTest.cs
//
// Author: Lluis Sanchez Gual  (lluis@ximian.com)
//
// (C) Ximian, Inc.
//

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System.Runtime.Remoting;
#if !DISABLE_REMOTING
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Proxies;
using System.Runtime.Remoting.Messaging;
#endif
using System.Collections;
using NUnit.Framework;
using System.Text;

namespace MonoTests.System.Runtime.Serialization
{
	[TestFixture]
	public class SerializationTest
	{
		MemoryStream ms;
		string uri;

#if FEATURE_REMOTING
		[Test]
		public void TestSerialization ()
		{
			MethodTester mt = new MethodTester();
			RemotingServices.Marshal (mt);
			uri = RemotingServices.GetObjectUri (mt);

			WriteData();
			ReadData();

			RemotingServices.Disconnect (mt);
		}
#endif

#if !MONOTOUCH && !FULL_AOT_RUNTIME
		[Test]
		public void DelegateSerializationTest ()
		{
			var a = new DelegateSerialization ();
			a.E += HandleE1;

			var d2 = Delegate.CreateDelegate (typeof(Func<StringBuilder, int>), "val", typeof(SerializationTest).GetMethod ("HandleE2"));
			a.E += (Func<StringBuilder, int>) d2;

			using (var ms = new MemoryStream ()) {
				var fmt = new BinaryFormatter ();
				fmt.Serialize (ms, a);
				ms.Flush ();

				ms.Seek (0, SeekOrigin.Begin);
				var a2 = (DelegateSerialization) fmt.Deserialize (ms);
				a2.Test ();
			}
		}
#endif

		static int HandleE1 (StringBuilder arg)
		{
			arg.Append ("E1");
			return 1;
		}

		public static int HandleE2 (object o, StringBuilder arg)
		{
			arg.Append ("E2|");
			arg.Append (o);
			return 2;
		}

#if FEATURE_REMOTING
		void WriteData ()
		{
			StreamingContext context = new StreamingContext (StreamingContextStates.Other);
			SurrogateSelector sel = new SurrogateSelector();
			sel.AddSurrogate (typeof (Point), context, new PointSurrogate());
			sel.AddSurrogate (typeof (FalseISerializable), context, new FalseISerializableSurrogate());

			List list = CreateTestData();
			BinderTester_A bta = CreateBinderTestData();

			ms = new MemoryStream();
			BinaryFormatter f = new BinaryFormatter (sel, new StreamingContext(StreamingContextStates.Other));
			f.Serialize (ms, list);
			ProcessMessages (ms, null);
			f.Serialize (ms, bta);
			ms.Flush ();
			ms.Position = 0;
		}

		void ReadData()
		{
			StreamingContext context = new StreamingContext (StreamingContextStates.Other);
			SurrogateSelector sel = new SurrogateSelector();
			sel.AddSurrogate (typeof (Point), context, new PointSurrogate());
			sel.AddSurrogate (typeof (FalseISerializable), context, new FalseISerializableSurrogate());

			BinaryFormatter f = new BinaryFormatter (sel, context);

			object list = f.Deserialize (ms);

			object[][] originalMsgData = null;
			IMessage[] calls = null;
			IMessage[] resps = null;

			originalMsgData = ProcessMessages (null, null);

			calls = new IMessage[originalMsgData.Length];
			resps = new IMessage[originalMsgData.Length];


			for (int n=0; n<originalMsgData.Length; n++)
			{
				calls[n] = (IMessage) f.Deserialize (ms);
				resps[n] = (IMessage) f.DeserializeMethodResponse (ms, null, (IMethodCallMessage)calls[n]);
			}

			f.Binder = new TestBinder ();
			object btbob = f.Deserialize (ms);

			ms.Close();

			List expected = CreateTestData ();
			List actual = (List) list;
			expected.CheckEquals (actual, "List");

			for (int i = 0; i < actual.children.Length - 1; ++i)
				if (actual.children [i].next != actual.children [i+1])
					Assert.Fail ("Deserialization did not restore pointer graph");

			BinderTester_A bta = CreateBinderTestData();
			Assert.AreEqual (btbob.GetType(), typeof (BinderTester_B), "BinderTest.class");
			BinderTester_B btb = btbob as BinderTester_B;
			if (btb != null)
			{
				Assert.AreEqual (btb.x, bta.x, "BinderTest.x");
				Assert.AreEqual (btb.y, bta.y, "BinderTest.y");
			}
			
			CheckMessages ("MethodCall", originalMsgData, ProcessMessages (null, calls));
			CheckMessages ("MethodResponse", originalMsgData, ProcessMessages (null, resps));
		}
#endif
		BinderTester_A CreateBinderTestData ()
		{
			BinderTester_A bta = new BinderTester_A();
			bta.x = 11;
			bta.y = "binder tester";
			return bta;
		}

		List CreateTestData()
		{
			List list = new List();
			list.name = "my list";
			list.values = new SomeValues();
			list.values.Init();

			ListItem item1 = new ListItem();
			ListItem item2 = new ListItem();
			ListItem item3 = new ListItem();

			item1.label = "value label 1";
			item1.next = item2;
			item1.value.color = 111;
			item1.value.point = new Point();
			item1.value.point.x = 11;
			item1.value.point.y = 22;

			item2.label = "value label 2";
			item2.next = item3;
			item2.value.color = 222;

			item2.value.point = new Point();
			item2.value.point.x = 33;
			item2.value.point.y = 44;

			item3.label = "value label 3";
			item3.value.color = 333;
			item3.value.point = new Point();
			item3.value.point.x = 55;
			item3.value.point.y = 66;

			list.children = new ListItem[3];

			list.children[0] = item1;
			list.children[1] = item2;
			list.children[2] = item3;

			return list;
		}

#if !DISABLE_REMOTING
		object[][] ProcessMessages (Stream stream, IMessage[] messages)
		{
			object[][] results = new object[9][];

			AuxProxy prx = new AuxProxy (stream, uri);
			MethodTester mt = (MethodTester)prx.GetTransparentProxy();
			object res;

			if (messages != null) prx.SetTestMessage (messages[0]);
			res = mt.OverloadedMethod();
			results[0] = new object[] {res};

			if (messages != null) prx.SetTestMessage (messages[1]);
			res = mt.OverloadedMethod(22);
			results[1] = new object[] {res};

			if (messages != null) prx.SetTestMessage (messages[2]);
			int[] par1 = new int[] {1,2,3};
			res = mt.OverloadedMethod(par1);
			results[2] = new object[] { res, par1 };

			if (messages != null) prx.SetTestMessage (messages[3]);
			mt.NoReturn();

			if (messages != null) prx.SetTestMessage (messages[4]);
			res = mt.Simple ("hello",44);
			results[4] = new object[] { res };

			if (messages != null) prx.SetTestMessage (messages[5]);
			res = mt.Simple2 ('F');
			results[5] = new object[] { res };

			if (messages != null) prx.SetTestMessage (messages[6]);
			char[] par2 = new char[] { 'G' };
			res = mt.Simple3 (par2);
			results[6] = new object[] { res, par2 };

			if (messages != null) prx.SetTestMessage (messages[7]);
			res = mt.Simple3 (null);
			results[7] = new object[] { res };

			if (messages != null) prx.SetTestMessage (messages[8]);

			SimpleClass b = new SimpleClass ('H');
			res = mt.SomeMethod (123456, b);
			results[8] = new object[] { res, b };

			return results;
		}
#endif

		void CheckMessages (string label, object[][] original, object[][] serialized)
		{
			for (int n=0; n<original.Length; n++)
				EqualsArray (label + " " + n, original[n], serialized[n]);
		}

		public static void AssertEquals(string message, Object expected, Object actual)
		{
			if (expected != null && expected.GetType().IsArray)
				EqualsArray (message, (Array)expected, (Array)actual);
			else
				Assert.AreEqual (expected, actual, message);
		}

		public static void EqualsArray (string message, object oar1, object oar2)
		{
			if (oar1 == null || oar2 == null || !(oar1 is Array) || !(oar2 is Array))
			{
				Assert.AreEqual (oar1, oar2, message);
				return;
			}

			Array ar1 = (Array) oar1;
			Array ar2 = (Array) oar2;

			Assert.AreEqual (ar1.Length, ar2.Length, message + ".Length");

			for (int n=0; n<ar1.Length; n++)
			{
				object av1 = ar1.GetValue(n);
				object av2 = ar2.GetValue(n);
				SerializationTest.AssertEquals (message + "[" + n + "]", av1, av2);
			}
		}
	}



	class PointSurrogate: ISerializationSurrogate
	{
		public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			Point p = (Point) obj;
			info.AddValue ("xv",p.x);
			info.AddValue ("yv",p.y);
		}

		public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			typeof (Point).GetField ("x").SetValue (obj, info.GetInt32 ("xv"));
			typeof (Point).GetField ("y").SetValue (obj, info.GetInt32 ("yv"));
			return obj;
		}
	}

	[Serializable]
	public class List
	{
		public string name = null;
		public ListItem[] children = null; 
		public SomeValues values;

		public void CheckEquals (List val, string context)
		{
			Assert.AreEqual (name, val.name, context + ".name");
			values.CheckEquals (val.values, context + ".values");

			Assert.AreEqual (children.Length, val.children.Length, context + ".children.Length");

			for (int n=0; n<children.Length; n++)
				children[n].CheckEquals (val.children[n], context + ".children[" + n + "]");
		}
	}

	[Serializable]
	public class ListItem: ISerializable
	{
		public ListItem()
		{
		}

		ListItem (SerializationInfo info, StreamingContext ctx)
		{
			next = (ListItem)info.GetValue ("next", typeof (ListItem));
			value = (ListValue)info.GetValue ("value", typeof (ListValue));
			label = info.GetString ("label");
		}

		public void GetObjectData (SerializationInfo info, StreamingContext ctx)
		{
			info.AddValue ("next", next);
			info.AddValue ("value", value);
			info.AddValue ("label", label);
		}

		public void CheckEquals (ListItem val, string context)
		{
			Assert.AreEqual (label, val.label, context + ".label");
			value.CheckEquals (val.value, context + ".value");

			if (next == null) {
				Assert.IsNull (val.next, context + ".next == null");
			} else {
				Assert.IsNotNull (val.next, context + ".next != null");
				next.CheckEquals (val.next, context + ".next");
			}
		}
		
		public override bool Equals(object obj)
		{
			ListItem val = (ListItem)obj;
			if ((next == null || val.next == null) && (next != val.next)) return false;
			if (next == null) return true;
			if (!next.Equals(val.next)) return false;
			return value.Equals (val.value) && label == val.label;
		}

		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}

		public ListItem next;
		public ListValue value;
		public string label;
	}

	[Serializable]
	public struct ListValue
	{
		public int color;
		public Point point;
		
		public override bool Equals(object obj)
		{
			ListValue val = (ListValue)obj;
			return (color == val.color && point.Equals(val.point));
		}

		public void CheckEquals (ListValue val, string context)
		{
			Assert.AreEqual (color, val.color, context + ".color");
			point.CheckEquals (val.point, context + ".point");
		}

		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}
	}

	public struct Point
	{
		public int x;
		public int y;

		public override bool Equals(object obj)
		{
			Point p = (Point)obj;
			return (x == p.x && y == p.y);
		}

		public void CheckEquals (Point p, string context)
		{
			Assert.AreEqual (x, p.x, context + ".x");
			Assert.AreEqual (y, p.y, context + ".y");
		}

		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}
	}

	[Serializable]
	public class FalseISerializable : ISerializable
	{
		public int field;
		
		public FalseISerializable (int n)
		{
			field = n;
		}
		
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			throw new InvalidOperationException ("Serialize:We should not pass here.");
		}
		
		public FalseISerializable (SerializationInfo info, StreamingContext context)
		{
			throw new InvalidOperationException ("Deserialize:We should not pass here.");
		}
	}
	
	public class FalseISerializableSurrogate : ISerializationSurrogate
	{
		public void GetObjectData (object obj, SerializationInfo info, StreamingContext context)
		{
			info.AddValue("field", Convert.ToString (((FalseISerializable)obj).field));
		}
		
		public object SetObjectData (object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			((FalseISerializable)obj).field = Convert.ToInt32 (info.GetValue("field", typeof(string)));
			return obj;
		}
	}

	[Serializable]
	public class SimpleClass
	{
		public SimpleClass (char v) { val = v; }

		public override bool Equals(object obj)
		{
			if (obj == null) return false;
			return val == ((SimpleClass)obj).val;
		}

		public override int GetHashCode()
		{
			return val.GetHashCode();
		}

		public int SampleCall (string str, SomeValues sv, ref int acum)
		{
			acum += (int)val;
			return (int)val;
		}

		public char val;
	}

	enum IntEnum { aaa, bbb, ccc }
	enum ByteEnum: byte { aaa=221, bbb=3, ccc=44 }

	delegate int SampleDelegate (string str, SomeValues sv, ref int acum);

	[Serializable]
	public class SomeValues
	{
		Type _type;
		Type _type2;
		DBNull _dbnull;
		Assembly _assembly;
		IntEnum _intEnum;
		ByteEnum _byteEnum;

		bool _bool;
		bool _bool2;
		byte _byte;
		char _char;
		DateTime _dateTime;
		decimal _decimal;
		double _double;
		short _short;
		int _int;
		long _long;
		sbyte _sbyte;
		float _float;
		ushort _ushort;
		uint _uint;
		ulong _ulong;

		object[] _objects;
		string[] _strings;
		int[] _ints;
		public int[,,] _intsMulti;
		int[][] _intsJagged;
		SimpleClass[] _simples;
		SimpleClass[,] _simplesMulti;
		SimpleClass[][] _simplesJagged;
		double[] _doubles;
		object[] _almostEmpty;

		object[] _emptyObjectArray;
		Type[] _emptyTypeArray;
		SimpleClass[] _emptySimpleArray;
		int[] _emptyIntArray;
		string[] _emptyStringArray;
		Point[] _emptyPointArray;


		SampleDelegate _sampleDelegate;
		SampleDelegate _sampleDelegate2;
		SampleDelegate _sampleDelegate3;
		SampleDelegate _sampleDelegateStatic;
		SampleDelegate _sampleDelegateCombined;

		SimpleClass _shared1;
		SimpleClass _shared2;
		SimpleClass _shared3;
		
		FalseISerializable _falseSerializable;

		public void Init()
		{
			_type = typeof (string);
			_type2 = typeof (SomeValues);
			_dbnull = DBNull.Value;
			_assembly = typeof (SomeValues).Assembly;
			_intEnum = IntEnum.bbb;
			_byteEnum = ByteEnum.ccc;
			_bool = true;
			_bool2 = false;
			_byte = 254;
			_char = 'A';
			_dateTime = new DateTime (1972,7,13,1,20,59);
			_decimal = (decimal)101010.10101;
			_double = 123456.6789;
			_short = -19191;
			_int = -28282828;
			_long = 37373737373;
			_sbyte = -123;
			_float = (float)654321.321;
			_ushort = 61616;
			_uint = 464646464;
			_ulong = 55555555;

			Point p = new Point();
			p.x = 56; p.y = 67;
			object boxedPoint = p;

			long i = 22;
			object boxedLong = i;

			_objects = new object[] { "string", (int)1234, null , /*boxedPoint, boxedPoint,*/ boxedLong, boxedLong};
			_strings = new string[] { "an", "array", "of", "strings","I","repeat","an", "array", "of", "strings" };
			_ints = new int[] { 4,5,6,7,8 };
			_intsMulti = new int[2,3,4] { { {1,2,3,4},{5,6,7,8},{9,10,11,12}}, { {13,14,15,16},{17,18,19,20},{21,22,23,24} } };
			_intsJagged = new int[2][] { new int[3] {1,2,3}, new int[2] {4,5} };
			_simples = new SimpleClass[] { new SimpleClass('a'),new SimpleClass('b'),new SimpleClass('c') };
			_simplesMulti = new SimpleClass[2,3] {{new SimpleClass('d'),new SimpleClass('e'),new SimpleClass('f')}, {new SimpleClass('g'),new SimpleClass('j'),new SimpleClass('h')}};
			_simplesJagged = new SimpleClass[2][] { new SimpleClass[1] { new SimpleClass('i') }, new SimpleClass[2] {null, new SimpleClass('k')}};
			_almostEmpty = new object[2000];
			_almostEmpty[1000] = 4;

			_emptyObjectArray = new object[0];
			_emptyTypeArray = new Type[0];
			_emptySimpleArray = new SimpleClass[0];
			_emptyIntArray = new int[0];
			_emptyStringArray = new string[0];
			_emptyPointArray = new Point[0];

			_doubles = new double[] { 1010101.101010, 292929.29292, 3838383.38383, 4747474.474, 56565.5656565, 0, Double.NaN, Double.MaxValue, Double.MinValue, Double.NegativeInfinity, Double.PositiveInfinity };

			_sampleDelegate = new SampleDelegate(SampleCall);
			_sampleDelegate2 = new SampleDelegate(_simples[0].SampleCall);
			_sampleDelegate3 = new SampleDelegate(new SimpleClass('x').SampleCall);
			_sampleDelegateStatic = new SampleDelegate(SampleStaticCall);
			_sampleDelegateCombined = (SampleDelegate)Delegate.Combine (new Delegate[] {_sampleDelegate, _sampleDelegate2, _sampleDelegate3, _sampleDelegateStatic });

			// This is to test that references are correctly solved
			_shared1 = new SimpleClass('A');
			_shared2 = new SimpleClass('A');
			_shared3 = _shared1;
			
			_falseSerializable = new FalseISerializable (2);
		}

		public int SampleCall (string str, SomeValues sv, ref int acum)
		{
			acum += _int;
			return _int;
		}

		public static int SampleStaticCall (string str, SomeValues sv, ref int acum)
		{
			acum += 99;
			return 99;
		}

		public void CheckEquals (SomeValues obj, string context)
		{
			Assert.AreEqual (_type, obj._type, context + "._type");
			Assert.AreEqual (_type2, obj._type2, context + "._type2");
			Assert.AreEqual (_dbnull, obj._dbnull, context + "._dbnull");
			Assert.AreEqual (_assembly, obj._assembly, context + "._assembly");

			Assert.AreEqual (_intEnum, obj._intEnum, context + "._intEnum");
			Assert.AreEqual (_byteEnum, obj._byteEnum, context + "._byteEnum");
			Assert.AreEqual (_bool, obj._bool, context + "._bool");
			Assert.AreEqual (_bool2, obj._bool2, context + "._bool2");
			Assert.AreEqual (_byte, obj._byte, context + "._byte");
			Assert.AreEqual (_char, obj._char, context + "._char");
			Assert.AreEqual (_dateTime, obj._dateTime, context + "._dateTime");
			Assert.AreEqual (_decimal, obj._decimal, context + "._decimal");
			Assert.AreEqual (_double, obj._double, context + "._double");
			Assert.AreEqual (_short, obj._short, context = "._short");
			Assert.AreEqual (_int, obj._int, context + "._int");
			Assert.AreEqual (_long, obj._long, context + "._long");
			Assert.AreEqual (_sbyte, obj._sbyte, context + "._sbyte");
			Assert.AreEqual (_float, obj._float, context + "._float");
			Assert.AreEqual (_ushort, obj._ushort, context + "._ushort");
			Assert.AreEqual (_uint, obj._uint, context + "._uint");
			Assert.AreEqual (_ulong, obj._ulong, context + "._ulong");

			SerializationTest.EqualsArray (context + "._objects", _objects, obj._objects);
			SerializationTest.EqualsArray (context + "._strings", _strings, obj._strings);
			SerializationTest.EqualsArray (context + "._doubles", _doubles, obj._doubles);
			SerializationTest.EqualsArray (context + "._ints", _ints, obj._ints);
			SerializationTest.EqualsArray (context + "._simples", _simples, obj._simples);
			SerializationTest.EqualsArray (context + "._almostEmpty", _almostEmpty, obj._almostEmpty);

			SerializationTest.EqualsArray (context + "._emptyObjectArray", _emptyObjectArray, obj._emptyObjectArray);
			SerializationTest.EqualsArray (context + "._emptyTypeArray", _emptyTypeArray, obj._emptyTypeArray);
			SerializationTest.EqualsArray (context + "._emptySimpleArray", _emptySimpleArray, obj._emptySimpleArray);
			SerializationTest.EqualsArray (context + "._emptyIntArray", _emptyIntArray, obj._emptyIntArray);
			SerializationTest.EqualsArray (context + "._emptyStringArray", _emptyStringArray, obj._emptyStringArray);
			SerializationTest.EqualsArray (context + "._emptyPointArray", _emptyPointArray, obj._emptyPointArray);

			for (int i=0; i<2; i++)
				for (int j=0; j<3; j++)
					for (int k=0; k<4; k++)
						SerializationTest.AssertEquals("SomeValues._intsMulti[" + i + "," + j + "," + k + "]", _intsMulti[i,j,k], obj._intsMulti[i,j,k]);

			for (int i=0; i<_intsJagged.Length; i++)
				for (int j=0; j<_intsJagged[i].Length; j++)
					SerializationTest.AssertEquals ("SomeValues._intsJagged[" + i + "][" + j + "]", _intsJagged[i][j], obj._intsJagged[i][j]);

			for (int i=0; i<2; i++)
				for (int j=0; j<3; j++)
					SerializationTest.AssertEquals ("SomeValues._simplesMulti[" + i + "," + j + "]", _simplesMulti[i,j], obj._simplesMulti[i,j]);

			for (int i=0; i<_simplesJagged.Length; i++)
				SerializationTest.EqualsArray ("SomeValues._simplesJagged", _simplesJagged[i], obj._simplesJagged[i]);

			int acum = 0;
			SerializationTest.AssertEquals ("SomeValues._sampleDelegate", _sampleDelegate ("hi", this, ref acum), _int);
			SerializationTest.AssertEquals ("SomeValues._sampleDelegate_bis", _sampleDelegate ("hi", this, ref acum), obj._sampleDelegate ("hi", this, ref acum));

			SerializationTest.AssertEquals ("SomeValues._sampleDelegate2", _sampleDelegate2 ("hi", this, ref acum), (int)_simples[0].val);
			SerializationTest.AssertEquals ("SomeValues._sampleDelegate2_bis", _sampleDelegate2 ("hi", this, ref acum), obj._sampleDelegate2 ("hi", this, ref acum));

			SerializationTest.AssertEquals ("SomeValues._sampleDelegate3", _sampleDelegate3 ("hi", this, ref acum), (int)'x');
			SerializationTest.AssertEquals ("SomeValues._sampleDelegate3_bis", _sampleDelegate3 ("hi", this, ref acum), obj._sampleDelegate3 ("hi", this, ref acum));

			SerializationTest.AssertEquals ("SomeValues._sampleDelegateStatic", _sampleDelegateStatic ("hi", this, ref acum), 99);
			SerializationTest.AssertEquals ("SomeValues._sampleDelegateStatic_bis", _sampleDelegateStatic ("hi", this, ref acum), obj._sampleDelegateStatic ("hi", this, ref acum));

			int acum1 = 0;
			int acum2 = 0;
			_sampleDelegateCombined ("hi", this, ref acum1);
			obj._sampleDelegateCombined ("hi", this, ref acum2);

			SerializationTest.AssertEquals ("_sampleDelegateCombined", acum1, _int + (int)_simples[0].val + (int)'x' + 99);
			SerializationTest.AssertEquals ("_sampleDelegateCombined_bis", acum1, acum2);

			SerializationTest.AssertEquals ("SomeValues._shared1", _shared1, _shared2);
			SerializationTest.AssertEquals ("SomeValues._shared1_bis", _shared1, _shared3);

			_shared1.val = 'B';
			SerializationTest.AssertEquals ("SomeValues._shared2", _shared2.val, 'A');
			SerializationTest.AssertEquals ("SomeValues._shared3", _shared3.val, 'B');
			
			SerializationTest.AssertEquals ("SomeValues._falseSerializable", _falseSerializable.field, 2);
		}
	}

	class MethodTester : MarshalByRefObject
	{
		public int OverloadedMethod ()
		{
			return 123456789;
		}

		public int OverloadedMethod (int a)
		{
			return a+2;
		}

		public int OverloadedMethod (int[] a)
		{
			return a.Length;
		}

		public void NoReturn ()
		{}

		public string Simple (string a, int b)
		{
			return a + b;
		}

		public SimpleClass Simple2 (char c)
		{
			return new SimpleClass(c);
		}

		public SimpleClass Simple3 (char[] c)
		{
			if (c != null) return new SimpleClass(c[0]);
			else return null;
		}

		public int SomeMethod (int a, SimpleClass b)
		{
			object[] d;
			string c = "hi";
			int r = a + c.Length;
			c = "bye";
			d = new object[3];
			d[1] = b;
			return r;
		}
	}

#if !DISABLE_REMOTING
	class AuxProxy: RealProxy
	{
		public static bool useHeaders = false;
		Stream _stream;
		string _uri;
		IMethodMessage _testMsg;

		public AuxProxy(Stream stream, string uri): base(typeof(MethodTester))
		{
			_stream = stream;
			_uri = uri;
		}

		public void SetTestMessage (IMessage msg)
		{
			_testMsg = (IMethodMessage)msg;
			_testMsg.Properties["__Uri"] = _uri;
		}

		public override IMessage Invoke(IMessage msg)
		{
			IMethodCallMessage call = (IMethodCallMessage)msg;
			if (call.MethodName.StartsWith ("Initialize")) return new ReturnMessage(null,null,0,null,(IMethodCallMessage)msg);

			call.Properties["__Uri"] = _uri;

			if (_stream != null)
			{
				SerializeCall (call);
				IMessage response = ChannelServices.SyncDispatchMessage (call);
				SerializeResponse (response);
				return response;
			}
			else if (_testMsg != null)
			{
				if (_testMsg is IMethodCallMessage)
					return ChannelServices.SyncDispatchMessage (_testMsg);
				else
					return _testMsg;
			}
			else
				return ChannelServices.SyncDispatchMessage (call);
		}

		void SerializeCall (IMessage call)
		{
			RemotingSurrogateSelector rss = new RemotingSurrogateSelector();
			var fmt = new BinaryFormatter (rss, new StreamingContext(StreamingContextStates.Remoting));
			fmt.Serialize (_stream, call, GetHeaders());
		}

		void SerializeResponse (IMessage resp)
		{
			RemotingSurrogateSelector rss = new RemotingSurrogateSelector();
			var fmt = new BinaryFormatter (rss, new StreamingContext(StreamingContextStates.Remoting));
			fmt.Serialize (_stream, resp, GetHeaders());
		}

		Header[] GetHeaders()
		{
			Header[] hs = null;
			if (useHeaders)
			{
				hs = new Header[1];
				hs[0] = new Header("unom",new SimpleClass('R'));
			}
			return hs;
		}
	}
#endif

	public class TestBinder : SerializationBinder
	{
		public override Type BindToType (string assemblyName, string typeName)
		{
			if (typeName.IndexOf("BinderTester_A") != -1)
				typeName = typeName.Replace ("BinderTester_A", "BinderTester_B");

			return Assembly.Load (assemblyName).GetType (typeName);
		}
	}

	[Serializable]
	public class BinderTester_A
	{
		public int x;
		public string y;
	}

	[Serializable]
	public class BinderTester_B
	{
		public string y;
		public int x;
	}

	[Serializable]
	class DelegateSerialization
	{
		public event Func<StringBuilder, int> E;

		public void Test ()
		{
			var sb = new StringBuilder ();
			Assert.AreEqual (2, E (sb), "#1");
			Assert.AreEqual ("E1E2|val", sb.ToString (), "#2");
		}
	}

}
