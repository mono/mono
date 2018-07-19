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
using System.Runtime.Serialization.Formatters.Soap;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Proxies;
using System.Runtime.Remoting.Messaging;
using System.Collections;
using NUnit.Framework;

namespace MonoTests.System.Runtime.Serialization.Formatters.Soap
{
	[TestFixture]
	public class SerializationTest
	{
		MemoryStream ms;

		[Test]
		public void TestSerialization ()
		{
			MethodTester mt = new MethodTester();
			RemotingServices.Marshal (mt, "myuri");

			WriteData();
			ReadData();

			RemotingServices.Disconnect (mt);
		}

		public static void Main() 
		{
			SerializationTest test = new SerializationTest();
			test.TestSerialization();
		}

		void WriteData ()
		{
			StreamingContext context = new StreamingContext (StreamingContextStates.Other);
			SurrogateSelector sel = new SurrogateSelector();
			sel.AddSurrogate (typeof (Point), context, new PointSurrogate());

			List list = CreateTestData();
			BinderTester_A bta = CreateBinderTestData();

			ms = new MemoryStream();
			SoapFormatter f = new SoapFormatter (sel, new StreamingContext(StreamingContextStates.Other));
			f.Serialize (ms, list);
//			ProcessMessages (ms, null);
//			f.Serialize (ms, bta);
			ms.Flush ();
			ms.Position = 0;
			StreamReader reader = new StreamReader(ms);
			Console.WriteLine(reader.ReadToEnd());
			ms.Position = 0;
		}

		void ReadData()
		{
			StreamingContext context = new StreamingContext (StreamingContextStates.Other);
			SurrogateSelector sel = new SurrogateSelector();
			sel.AddSurrogate (typeof (Point), context, new PointSurrogate());

			SoapFormatter f = new SoapFormatter (sel, context);

			object list = f.Deserialize (ms);

			object[][] originalMsgData = null;
			IMessage[] calls = null;
			IMessage[] resps = null;

//			originalMsgData = ProcessMessages (null, null);

//			calls = new IMessage[originalMsgData.Length];
//			resps = new IMessage[originalMsgData.Length];


//			for (int n=0; n<originalMsgData.Length; n++)
//			{
//				calls[n] = (IMessage) f.Deserialize (ms);
//				resps[n] = (IMessage) f.DeserializeMethodResponse (ms, null, (IMethodCallMessage)calls[n]);
//			}
//
//			f.Binder = new TestBinder ();
//			object btbob = f.Deserialize (ms);

			ms.Close();

			((List)list).CheckEquals(CreateTestData());
//
//			BinderTester_A bta = CreateBinderTestData();
//			Assertion.AssertEquals ("BinderTest.class", btbob.GetType(), typeof (BinderTester_B));
//			BinderTester_B btb = btbob as BinderTester_B;
//			if (btb != null)
//			{
//				Assertion.AssertEquals ("BinderTest.x", btb.x, bta.x);
//				Assertion.AssertEquals ("BinderTest.y", btb.y, bta.y);
//			}
//			
//			CheckMessages ("MethodCall", originalMsgData, ProcessMessages (null, calls));
//			CheckMessages ("MethodResponse", originalMsgData, ProcessMessages (null, resps));
		}

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


		object[][] ProcessMessages (Stream stream, IMessage[] messages)
		{
			object[][] results = new object[9][];

			AuxProxy prx = new AuxProxy (stream, "myuri");
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
				SerializationTest.AssertEquals (message, oar1, oar2);
				return;
			}

			Array ar1 = (Array) oar1;
			Array ar2 = (Array) oar2;

			SerializationTest.AssertEquals(message + ".Length", ar1.Length,ar2.Length);

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
			Point p = (Point)obj;
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

		public void CheckEquals(List val)
		{
			SerializationTest.AssertEquals ("List.children.Length", children.Length, val.children.Length);

			for (int n=0; n<children.Length; n++)
				children[n].CheckEquals (val.children[n]);

			SerializationTest.AssertEquals ("List.name", name, val.name);
			values.CheckEquals (val.values);
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

		public void CheckEquals(ListItem val)
		{
			SerializationTest.AssertEquals ("ListItem.next", next, val.next);
			SerializationTest.AssertEquals ("ListItem.label", label, val.label);
			value.CheckEquals (val.value);
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

		public void CheckEquals(ListValue val)
		{
			SerializationTest.AssertEquals ("ListValue.color", color, val.color);
			point.CheckEquals (val.point);
		}

		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}
	}

//	[Serializable]
	public struct Point
	{
		public int x;
		public int y;

		public override bool Equals(object obj)
		{
			Point p = (Point)obj;
			return (x == p.x && y == p.y);
		}

		public void CheckEquals(Point p)
		{
			SerializationTest.AssertEquals ("Point.x", x, p.x);
			SerializationTest.AssertEquals ("Point.y", y, p.y);
		}

		public override int GetHashCode ()
		{
			return base.GetHashCode ();
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
//		DateTime _dateTime;
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


		SampleDelegate _sampleDelegate;
		SampleDelegate _sampleDelegate2;
		SampleDelegate _sampleDelegate3;
		SampleDelegate _sampleDelegateStatic;
		SampleDelegate _sampleDelegateCombined;

		SimpleClass _shared1;
		SimpleClass _shared2;
		SimpleClass _shared3;

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
//			_dateTime = new DateTime (1972,7,13,1,20,59);
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

			// FIXME: Once double.ToString("G17") is implemented
			// we'll be able to serialize double.MaxValue and double.MinValue.
			// Currently, it throws a System.OverflowException.
			//_doubles = new double[] { 1010101.101010, 292929.29292, 3838383.38383, 4747474.474, 56565.5656565, 0, Double.NaN, Double.MaxValue, Double.MinValue, Double.NegativeInfinity, Double.PositiveInfinity };
			_doubles = new double[] { 1010101.101010, 292929.29292, 3838383.38383, 4747474.474, 56565.5656565, 0, Double.NaN, Double.NegativeInfinity, Double.PositiveInfinity };

			_sampleDelegate = new SampleDelegate(SampleCall);
			_sampleDelegate2 = new SampleDelegate(_simples[0].SampleCall);
			_sampleDelegate3 = new SampleDelegate(new SimpleClass('x').SampleCall);
			_sampleDelegateStatic = new SampleDelegate(SampleStaticCall);
			_sampleDelegateCombined = (SampleDelegate)Delegate.Combine (new Delegate[] {_sampleDelegate, _sampleDelegate2, _sampleDelegate3, _sampleDelegateStatic });

			// This is to test that references are correctly solved
			_shared1 = new SimpleClass('A');
			_shared2 = new SimpleClass('A');
			_shared3 = _shared1;
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

		public void CheckEquals(SomeValues obj)
		{
			SerializationTest.AssertEquals ("SomeValues._type", _type, obj._type);
			SerializationTest.AssertEquals ("SomeValues._type2", _type2, obj._type2);
			SerializationTest.AssertEquals ("SomeValues._dbnull", _dbnull, obj._dbnull);
			SerializationTest.AssertEquals ("SomeValues._assembly", _assembly, obj._assembly);

			SerializationTest.AssertEquals ("SomeValues._intEnum", _intEnum, obj._intEnum);
			SerializationTest.AssertEquals ("SomeValues._byteEnum", _byteEnum, obj._byteEnum);
			SerializationTest.AssertEquals ("SomeValues._bool", _bool, obj._bool);
			SerializationTest.AssertEquals ("SomeValues._bool2", _bool2, obj._bool2);
			SerializationTest.AssertEquals ("SomeValues._byte", _byte, obj._byte);
			SerializationTest.AssertEquals ("SomeValues._char", _char, obj._char);
//			SerializationTest.AssertEquals ("SomeValues._dateTime", _dateTime, obj._dateTime);
			SerializationTest.AssertEquals ("SomeValues._decimal", _decimal, obj._decimal);
			SerializationTest.AssertEquals ("SomeValues._int", _int, obj._int);
			SerializationTest.AssertEquals ("SomeValues._long", _long, obj._long);
			SerializationTest.AssertEquals ("SomeValues._sbyte", _sbyte, obj._sbyte);
			SerializationTest.AssertEquals ("SomeValues._float", _float, obj._float);
			SerializationTest.AssertEquals ("SomeValues._ushort", _ushort, obj._ushort);
			SerializationTest.AssertEquals ("SomeValues._uint", _uint, obj._uint);
			SerializationTest.AssertEquals ("SomeValues._ulong", _ulong, obj._ulong);

			SerializationTest.EqualsArray ("SomeValues._objects", _objects, obj._objects);
			SerializationTest.EqualsArray ("SomeValues._strings", _strings, obj._strings);
			SerializationTest.EqualsArray ("SomeValues._doubles", _doubles, obj._doubles);
			SerializationTest.EqualsArray ("SomeValues._ints", _ints, obj._ints);
			SerializationTest.EqualsArray ("SomeValues._simples", _simples, obj._simples);
			SerializationTest.EqualsArray ("SomeValues._almostEmpty", _almostEmpty, obj._almostEmpty);

			SerializationTest.EqualsArray ("SomeValues._emptyObjectArray", _emptyObjectArray, obj._emptyObjectArray);
			SerializationTest.EqualsArray ("SomeValues._emptyTypeArray", _emptyTypeArray, obj._emptyTypeArray);
			SerializationTest.EqualsArray ("SomeValues._emptySimpleArray", _emptySimpleArray, obj._emptySimpleArray);
			SerializationTest.EqualsArray ("SomeValues._emptyIntArray", _emptyIntArray, obj._emptyIntArray);
			SerializationTest.EqualsArray ("SomeValues._emptyStringArray", _emptyStringArray, obj._emptyStringArray);

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
			IRemotingFormatter fmt = new SoapFormatter (rss, new StreamingContext(StreamingContextStates.Remoting));
			fmt.Serialize (_stream, call, GetHeaders());
		}

		void SerializeResponse (IMessage resp)
		{
			RemotingSurrogateSelector rss = new RemotingSurrogateSelector();
			IRemotingFormatter fmt = new SoapFormatter (rss, new StreamingContext(StreamingContextStates.Remoting));
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


}
