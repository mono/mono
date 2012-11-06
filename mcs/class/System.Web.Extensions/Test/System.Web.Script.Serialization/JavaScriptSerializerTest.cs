//
// JavaScriptSerializer.cs
//
// Author:
//   Konstantin Triger <kostat@mainsoft.com>
//
// (C) 2007 Mainsoft, Inc.  http://www.mainsoft.com
//
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
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.Web.Script.Serialization;
using System.Reflection;
using System.Collections;
using System.Drawing;
using ComponentModel = System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Text.RegularExpressions;
using System.ComponentModel;
using CategoryAttribute = NUnit.Framework.CategoryAttribute;
using System.Web.UI.WebControls;
using System.Collections.ObjectModel;


namespace MonoTests.System.Web.Script.Serialization
{
	[TestFixture]
	public class JavaScriptSerializerTest
	{
		enum MyEnum
		{
			AAA,
			BBB,
			CCC
		}
#pragma warning disable 659
		class bug
		{
			//public DateTime dt;
			//public DateTime dt1;
			//public DateTime dt2;
			public bool bb;
			//Hashtable hash;

			public void Init() {
				//dt = DateTime.MaxValue;
				//dt1 = DateTime.MinValue;
				//dt2 = new DateTime ((DateTime.Now.Ticks / 10000) * 10000);
				bb = true;
				//hash = new Hashtable ();
				//hash.Add ("mykey", 1);
			}

			public override bool Equals (object obj) {
				if (!(obj is bug))
					return false;
				JavaScriptSerializerTest.FieldsEqual (this, obj);
				return true;
			}
		}
		class X
		{
			int x = 5;
			//int y;
			ulong _bb;
			Y[] _yy;
			Y [] _yyy = new Y [] { new Y (), new Y () };
			public int z;
			public char ch;
			public char ch_null;
			public string str;
			public byte b;
			public sbyte sb;
			public short sh;
			public ushort ush;
			public int i;
			public uint ui;
			public long l;
			public ulong ul;
			
			public float f;
			public float f1;
			public float f2;
			public float f3;
			public float f4;

			public double d;
			public double d1;
			public double d2;
			public double d3;
			public double d4;

			public decimal de;
			public decimal de1;
			public decimal de2;
			public decimal de3;
			public decimal de4;

			

			public Guid g;
			
			public Nullable<bool> nb;
			public DBNull dbn;
			IEnumerable<int> enum_int;
			IEnumerable enum_int1;
			public Uri uri;
			public Dictionary<string, Y> hash;
			public Point point;

			public void Init () {
				//y = 6;
				_bb = ulong.MaxValue - 5;
				_yy = new Y [] { new Y (), new Y () };
				z = 8;
				ch = (char) 0xFF56;
				ch_null = '\0';
				str = "\uFF56\uFF57\uF58FF59g";
				b = 253;
				sb = -48;
				sh = short.MinValue + 28;
				ush = ushort.MaxValue - 24;
				i = -234235453;
				ui = uint.MaxValue - 234234;
				l = long.MinValue + 28;
				ul = ulong.MaxValue - 3;

				f = float.NaN;
				f1 = float.NegativeInfinity;
				f2 = float.PositiveInfinity;
				f3 = float.MinValue;
				f4 = float.MaxValue;

				d = double.NaN;
				d1 = double.NegativeInfinity;
				d2 = double.PositiveInfinity;
				d3 = double.MinValue;
				d4 = double.MaxValue;

				de = decimal.MinusOne;
				de1 = decimal.Zero;
				de2 = decimal.One;
				de3 = decimal.MinValue;
				de4 = decimal.MaxValue;

				g = new Guid (234, 2, 354, new byte [] { 1, 2, 3, 4, 5, 6, 7, 8 });
				
				nb = null;
				dbn = null;

				enum_int = new List<int> (MyEnum);
				enum_int1 = new ArrayList ();
				foreach (object obj in MyEnum1)
					((ArrayList) enum_int1).Add (obj);
				uri = new Uri ("http://kostat@mainsoft/adfasdf/asdfasdf.aspx/asda/ads?a=b&c=d", UriKind.RelativeOrAbsolute);

				hash = new Dictionary<string, Y> ();
				Y y = new Y ();
				hash ["mykey"] = y;
				point = new Point (150, 150);
			}

			public IEnumerable<int> MyEnum {
				get {
					yield return 1;
					yield return 10;
					yield return 345;
				}

				set {
					enum_int = value;
				}
			}

			public IEnumerable MyEnum1 {
				get {
					yield return 1;
					yield return 10;
					yield return 345;
				}

				set {
					enum_int1 = value;
				}
			}

			public int AA {
				get { return x; }
			}

			public Y[] AA1 {
				get { return _yyy; }
			}

			public ulong BB {
				get { return _bb; }
				set { _bb = value; }
			}

			public Y[] YY {
				get { return _yy; }
				set { _yy = value; }
			}

			public override bool Equals (object obj) {
				if (!(obj is X))
					return false;
				JavaScriptSerializerTest.FieldsEqual (this, obj);
				return true;
			}
		}

		class Y
		{

			long _bb = 10;

			public long BB {
				get { return _bb; }
				set { _bb = value; }
			}

			public override bool Equals (object obj) {
				if (!(obj is Y))
					return false;
				JavaScriptSerializerTest.FieldsEqual(this, obj);
				return true;
			}
		}

		class YY
		{
			public YY () 
			{
				Y1 = new Y ();
				Y2 = new Y ();
			}

			public Y Y1;
			public Y Y2;
		}

		[TypeConverter (typeof (MyUriConverter))]
		class MyUri : Uri
		{
			public MyUri (string uriString, UriKind uriKind)
				: base (uriString, uriKind) {
			}

			public MyUri (Uri value)
				: base (value.AbsoluteUri) {
			}
		}

		class MyUriConverter : UriTypeConverter
		{
			public override object ConvertTo (ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) 
			{
				return base.ConvertTo (context, culture, value, destinationType);
			}

			public override object ConvertFrom (ITypeDescriptorContext context, CultureInfo culture, object value) 
			{
				Uri convertedUri = (Uri)base.ConvertFrom (context, culture, value);
				return new MyUri (convertedUri);
			}
		}

		[TypeConverter(typeof(MyPointConverter))]
		class MyPointContainer
		{
			public MyPointContainer () 
			{
			}

			public MyPointContainer (Point v) 
			{
				p = v;
			}

			internal Point p;
		}

		class MyPointConverter : TypeConverter
		{
			public override bool CanConvertTo (ITypeDescriptorContext context, Type destinationType) 
			{
				if (destinationType == typeof (string)) {
					return true;
				}
				return base.CanConvertTo (context, destinationType);
			}

			public override object ConvertTo (ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) 
			{
				if (destinationType == typeof (string)) {
					MyPointContainer pc = (MyPointContainer) value;
					return pc.p.X + "," + pc.p.Y;
				}
				return base.ConvertTo (context, culture, value, destinationType);
			}

			public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType) 
			{
				if (sourceType == typeof (string)) {
					return true;
				}
				return base.CanConvertFrom (context, sourceType);
			}

			public override object ConvertFrom (ITypeDescriptorContext context, CultureInfo culture, object value) 
			{
				if (value is string) {
					string [] v = ((string) value).Split (new char [] { ',' });
					return new MyPointContainer(new Point (int.Parse (v [0]), int.Parse (v [1])));
				}
				return base.ConvertFrom (context, culture, value);
			}
		}

#pragma warning restore 659

		[Test]
		[Category ("NotDotNet")]
		public void TestDefaults () {
			JavaScriptSerializer ser = new JavaScriptSerializer ();
#if NET_3_5
			Assert.AreEqual (2097152, ser.MaxJsonLength);
#else
			Assert.AreEqual (102400, ser.MaxJsonLength);
#endif
			Assert.AreEqual (100, ser.RecursionLimit);
			//List<JavaScriptConverter> l = new List<JavaScriptConverter> ();
			//l.Add (new MyJavaScriptConverter ());
			//ser.RegisterConverters (l);
			//string x = ser.Serialize (new X [] { new X (), new X () });
			//string s = ser.Serialize (new X());
			//"{\"BB\":10,\"__type\":\"Tests.System.Web.Script.Serialization.JavaScriptSerializerTest+Y, Tests, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null\"}"
			//X x = ser.Deserialize<X> (s);
			//object ddd = typeof (Y).GetMember ("BB");
			//object x1 = ser.Deserialize<X []> (null);
			//object x2 = ser.Deserialize<X []> ("");
			//object d = ser.Deserialize<X[]> (x);
		}

		[Test]
		public void TestDeserializeUnquotedKeys ()
		{
			JavaScriptSerializer ser = new JavaScriptSerializer ();
			IDictionary dict = ser.Deserialize <Dictionary <string, object>> ("{itemOne:\"1\",itemTwo:\"2\"}");

			Assert.AreEqual ("1", dict ["itemOne"], "#A1");
			Assert.AreEqual ("2", dict ["itemTwo"], "#A2");

			dict = ser.Deserialize <Dictionary <string, object>> ("{itemOne:1,itemTwo:2}");
			Assert.AreEqual (1, dict ["itemOne"], "#B1");
			Assert.AreEqual (2, dict ["itemTwo"], "#B2");
		}

		[Test]
		public void TestDeserializeUnquotedKeysWithSpaces ()
		{
			JavaScriptSerializer ser = new JavaScriptSerializer ();
			IDictionary dict = ser.Deserialize <Dictionary <string, object>> ("{ itemOne :\"1\",itemTwo:\"2\"}");

			Assert.AreEqual ("1", dict ["itemOne"], "#A1");
			Assert.AreEqual ("2", dict ["itemTwo"], "#A2");

			dict = ser.Deserialize <Dictionary <string, object>> ("{   itemOne   :1,   itemTwo   :2}");
			Assert.AreEqual (1, dict ["itemOne"], "#B1");
			Assert.AreEqual (2, dict ["itemTwo"], "#B2");
		}
		
		[Test]
		public void TestDeserialize () {
			JavaScriptSerializer ser = new JavaScriptSerializer ();
			Assert.IsNull (ser.Deserialize<X> (""));

			X s = new X ();
			s.Init ();
			string x = ser.Serialize (s);

			Assert.AreEqual ("{\"z\":8,\"ch\":\"ｖ\",\"ch_null\":null,\"str\":\"ｖｗF59g\",\"b\":253,\"sb\":-48,\"sh\":-32740,\"ush\":65511,\"i\":-234235453,\"ui\":4294733061,\"l\":-9223372036854775780,\"ul\":18446744073709551612,\"f\":NaN,\"f1\":-Infinity,\"f2\":Infinity,\"f3\":-3.40282347E+38,\"f4\":3.40282347E+38,\"d\":NaN,\"d1\":-Infinity,\"d2\":Infinity,\"d3\":-1.7976931348623157E+308,\"d4\":1.7976931348623157E+308,\"de\":-1,\"de1\":0,\"de2\":1,\"de3\":-79228162514264337593543950335,\"de4\":79228162514264337593543950335,\"g\":\"000000ea-0002-0162-0102-030405060708\",\"nb\":null,\"dbn\":null,\"uri\":\"http://kostat@mainsoft/adfasdf/asdfasdf.aspx/asda/ads?a=b&c=d\",\"hash\":{\"mykey\":{\"BB\":10}},\"point\":{\"IsEmpty\":false,\"X\":150,\"Y\":150},\"MyEnum\":[1,10,345],\"MyEnum1\":[1,10,345],\"AA\":5,\"AA1\":[{\"BB\":10},{\"BB\":10}],\"BB\":18446744073709551610,\"YY\":[{\"BB\":10},{\"BB\":10}]}", x, "#A1");
			
			X n = ser.Deserialize<X> (x);
			Assert.AreEqual (s, n, "#A2");

			//string json = "\\uFF56";
			//string result = ser.Deserialize<string> (json);
			//Assert.AreEqual ("\uFF56", result);

			//object oo = ser.DeserializeObject ("{value:'Purple\\r \\n monkey\\'s:\\tdishwasher'}");
		}

		[Test]
		public void TestDeserializeTypeResolver () 
		{
#if NET_4_5
			string expected = "{\"__type\":\"MonoTests.System.Web.Script.Serialization.JavaScriptSerializerTest+X, System.Web.Extensions_test_net_4_5, Version=1.3.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\",\"z\":8,\"ch\":\"ｖ\",\"ch_null\":null,\"str\":\"ｖｗF59g\",\"b\":253,\"sb\":-48,\"sh\":-32740,\"ush\":65511,\"i\":-234235453,\"ui\":4294733061,\"l\":-9223372036854775780,\"ul\":18446744073709551612,\"f\":NaN,\"f1\":-Infinity,\"f2\":Infinity,\"f3\":-3.40282347E+38,\"f4\":3.40282347E+38,\"d\":NaN,\"d1\":-Infinity,\"d2\":Infinity,\"d3\":-1.7976931348623157E+308,\"d4\":1.7976931348623157E+308,\"de\":-1,\"de1\":0,\"de2\":1,\"de3\":-79228162514264337593543950335,\"de4\":79228162514264337593543950335,\"g\":\"000000ea-0002-0162-0102-030405060708\",\"nb\":null,\"dbn\":null,\"uri\":\"http://kostat@mainsoft/adfasdf/asdfasdf.aspx/asda/ads?a=b&c=d\",\"hash\":{\"mykey\":{\"__type\":\"MonoTests.System.Web.Script.Serialization.JavaScriptSerializerTest+Y, System.Web.Extensions_test_net_4_5, Version=1.3.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\",\"BB\":10}},\"point\":{\"__type\":\"System.Drawing.Point, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a\",\"IsEmpty\":false,\"X\":150,\"Y\":150},\"MyEnum\":[1,10,345],\"MyEnum1\":[1,10,345],\"AA\":5,\"AA1\":[{\"__type\":\"MonoTests.System.Web.Script.Serialization.JavaScriptSerializerTest+Y, System.Web.Extensions_test_net_4_5, Version=1.3.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\",\"BB\":10},{\"__type\":\"MonoTests.System.Web.Script.Serialization.JavaScriptSerializerTest+Y, System.Web.Extensions_test_net_4_5, Version=1.3.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\",\"BB\":10}],\"BB\":18446744073709551610,\"YY\":[{\"__type\":\"MonoTests.System.Web.Script.Serialization.JavaScriptSerializerTest+Y, System.Web.Extensions_test_net_4_5, Version=1.3.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\",\"BB\":10},{\"__type\":\"MonoTests.System.Web.Script.Serialization.JavaScriptSerializerTest+Y, System.Web.Extensions_test_net_4_5, Version=1.3.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\",\"BB\":10}]}";
#elif NET_4_0
			string expected = "{\"__type\":\"MonoTests.System.Web.Script.Serialization.JavaScriptSerializerTest+X, System.Web.Extensions_test_net_4_0, Version=1.3.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\",\"z\":8,\"ch\":\"ｖ\",\"ch_null\":null,\"str\":\"ｖｗF59g\",\"b\":253,\"sb\":-48,\"sh\":-32740,\"ush\":65511,\"i\":-234235453,\"ui\":4294733061,\"l\":-9223372036854775780,\"ul\":18446744073709551612,\"f\":NaN,\"f1\":-Infinity,\"f2\":Infinity,\"f3\":-3.40282347E+38,\"f4\":3.40282347E+38,\"d\":NaN,\"d1\":-Infinity,\"d2\":Infinity,\"d3\":-1.7976931348623157E+308,\"d4\":1.7976931348623157E+308,\"de\":-1,\"de1\":0,\"de2\":1,\"de3\":-79228162514264337593543950335,\"de4\":79228162514264337593543950335,\"g\":\"000000ea-0002-0162-0102-030405060708\",\"nb\":null,\"dbn\":null,\"uri\":\"http://kostat@mainsoft/adfasdf/asdfasdf.aspx/asda/ads?a=b&c=d\",\"hash\":{\"mykey\":{\"__type\":\"MonoTests.System.Web.Script.Serialization.JavaScriptSerializerTest+Y, System.Web.Extensions_test_net_4_0, Version=1.3.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\",\"BB\":10}},\"point\":{\"__type\":\"System.Drawing.Point, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a\",\"IsEmpty\":false,\"X\":150,\"Y\":150},\"MyEnum\":[1,10,345],\"MyEnum1\":[1,10,345],\"AA\":5,\"AA1\":[{\"__type\":\"MonoTests.System.Web.Script.Serialization.JavaScriptSerializerTest+Y, System.Web.Extensions_test_net_4_0, Version=1.3.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\",\"BB\":10},{\"__type\":\"MonoTests.System.Web.Script.Serialization.JavaScriptSerializerTest+Y, System.Web.Extensions_test_net_4_0, Version=1.3.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\",\"BB\":10}],\"BB\":18446744073709551610,\"YY\":[{\"__type\":\"MonoTests.System.Web.Script.Serialization.JavaScriptSerializerTest+Y, System.Web.Extensions_test_net_4_0, Version=1.3.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\",\"BB\":10},{\"__type\":\"MonoTests.System.Web.Script.Serialization.JavaScriptSerializerTest+Y, System.Web.Extensions_test_net_4_0, Version=1.3.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\",\"BB\":10}]}";
#else
			string expected = "{\"__type\":\"MonoTests.System.Web.Script.Serialization.JavaScriptSerializerTest+X, System.Web.Extensions_test_net_2_0, Version=1.3.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\",\"z\":8,\"ch\":\"ｖ\",\"ch_null\":null,\"str\":\"ｖｗF59g\",\"b\":253,\"sb\":-48,\"sh\":-32740,\"ush\":65511,\"i\":-234235453,\"ui\":4294733061,\"l\":-9223372036854775780,\"ul\":18446744073709551612,\"f\":NaN,\"f1\":-Infinity,\"f2\":Infinity,\"f3\":-3.40282347E+38,\"f4\":3.40282347E+38,\"d\":NaN,\"d1\":-Infinity,\"d2\":Infinity,\"d3\":-1.7976931348623157E+308,\"d4\":1.7976931348623157E+308,\"de\":-1,\"de1\":0,\"de2\":1,\"de3\":-79228162514264337593543950335,\"de4\":79228162514264337593543950335,\"g\":\"000000ea-0002-0162-0102-030405060708\",\"nb\":null,\"dbn\":null,\"uri\":\"http://kostat@mainsoft/adfasdf/asdfasdf.aspx/asda/ads?a=b&c=d\",\"hash\":{\"mykey\":{\"__type\":\"MonoTests.System.Web.Script.Serialization.JavaScriptSerializerTest+Y, System.Web.Extensions_test_net_2_0, Version=1.3.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\",\"BB\":10}},\"point\":{\"__type\":\"System.Drawing.Point, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a\",\"IsEmpty\":false,\"X\":150,\"Y\":150},\"MyEnum\":[1,10,345],\"MyEnum1\":[1,10,345],\"AA\":5,\"AA1\":[{\"__type\":\"MonoTests.System.Web.Script.Serialization.JavaScriptSerializerTest+Y, System.Web.Extensions_test_net_2_0, Version=1.3.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\",\"BB\":10},{\"__type\":\"MonoTests.System.Web.Script.Serialization.JavaScriptSerializerTest+Y, System.Web.Extensions_test_net_2_0, Version=1.3.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\",\"BB\":10}],\"BB\":18446744073709551610,\"YY\":[{\"__type\":\"MonoTests.System.Web.Script.Serialization.JavaScriptSerializerTest+Y, System.Web.Extensions_test_net_2_0, Version=1.3.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\",\"BB\":10},{\"__type\":\"MonoTests.System.Web.Script.Serialization.JavaScriptSerializerTest+Y, System.Web.Extensions_test_net_2_0, Version=1.3.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\",\"BB\":10}]}";
#endif
			JavaScriptSerializer ser = new JavaScriptSerializer (new SimpleTypeResolver ());
			X x = new X ();
			x.Init ();

			string s = ser.Serialize (x);
			Assert.AreEqual (expected, s, "#A1");
			
			X x2 = ser.Deserialize<X> (s);
			Assert.AreEqual (x, x2, "#A2");
		}

		[Test]
		public void TestDeserializeBugs () {
			JavaScriptSerializer ser = new JavaScriptSerializer ();

			bug s = new bug ();
			s.Init ();
			string x = ser.Serialize (s);
			bug n = ser.Deserialize<bug> (x);
			Assert.AreEqual (s, n);

			// Should check correctness with .Net GA:
			//js = ser.Serialize (Color.Red);
			//Color ccc = ser.Deserialize<Color> (js);
			//string xml = @"<root><node attr=""xxx""/></root>";

			//XmlDocument doc = new XmlDocument ();
			//doc.LoadXml (xml);
			//string js = ser.Serialize (doc);
			//DataTable table = new DataTable();
			//table.Columns.Add ("col1", typeof (int));
			//table.Columns.Add ("col2", typeof (float));
			//table.Rows.Add (1, 1f);
			//table.Rows.Add (234234, 2.4f);

			//string js = ser.Serialize (table);
		}

		static void FieldsEqual (object expected, object actual) {
			Assert.AreEqual (expected.GetType (), actual.GetType ());
			FieldInfo [] infos = expected.GetType ().GetFields (BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
			foreach (FieldInfo info in infos) {
				object value1 = info.GetValue (expected);
				object value2 = info.GetValue (actual);
				if (value1 is IEnumerable) {
					IEnumerator yenum = ((IEnumerable) value2).GetEnumerator ();
					int index = -1;
					foreach (object x in (IEnumerable) value1) {
						if (!yenum.MoveNext ())
							Assert.Fail (info.Name + " index:" + index);
						index++;
						if (x is DictionaryEntry) {
							DictionaryEntry entry = (DictionaryEntry)x;
							IDictionary dict = (IDictionary) value2;
							Assert.AreEqual (entry.Value, dict [entry.Key], info.Name + ", key:" + entry.Key);
						}
						else
							Assert.AreEqual (x, yenum.Current, info.Name + ", index:" + index);
					}
					Assert.IsFalse (yenum.MoveNext (), info.Name);
					continue;
				}
				Assert.AreEqual (value1, value2, info.Name);
			}

		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestDeserialize1 () {
			JavaScriptSerializer ser = new JavaScriptSerializer ();
			ser.Deserialize<string> (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestDeserializeNullConverter () {
			JavaScriptSerializer ser = new JavaScriptSerializer ();
			ser.RegisterConverters (null);
		}

		[Test]
		[SetCulture ("en-US")]
		public void TestDeserializeConverter () {
			JavaScriptSerializer ser = new JavaScriptSerializer ();
			List<JavaScriptConverter> list = new List<JavaScriptConverter> ();
			list.Add (new MyJavaScriptConverter ());
			list.Add (new CultureInfoConverter ());
			ser.RegisterConverters (list);
			string result = ser.Serialize (new X [] { new X (), new X () });
			Assert.AreEqual ("{\"0\":1,\"1\":2}", result);
			result = ser.Serialize (Thread.CurrentThread.CurrentCulture);
		}

		[Test]
		public void TestDeserializeConverter1 () {
			JavaScriptSerializer serializer = new JavaScriptSerializer ();

			serializer.RegisterConverters (new JavaScriptConverter [] {new ListItemCollectionConverter()});

			ListBox ListBox1 = new ListBox ();
			ListBox1.Items.Add ("a1");
			ListBox1.Items.Add ("a2");
			ListBox1.Items.Add ("a3");

			string x = serializer.Serialize (ListBox1.Items);
			ListItemCollection recoveredList = serializer.Deserialize<ListItemCollection> (x);
			Assert.AreEqual (3, recoveredList.Count);
		}

		[Test]
		public void TestSerialize1 () {
			JavaScriptSerializer ser = new JavaScriptSerializer ();
			Assert.AreEqual("null", ser.Serialize(null));

			string js = ser.Serialize (1234);
			Assert.AreEqual ("1234", js);
			Assert.AreEqual (1234, ser.Deserialize<int> (js));
			js = ser.Serialize (1.1);
			Assert.AreEqual ("1.1", js);
			Assert.AreEqual (1.1f, ser.Deserialize<float> (js));
			char [] chars = "faskjhfasd0981234".ToCharArray ();
			js = ser.Serialize (chars);
			char[] actual = ser.Deserialize<char[]> (js);
			Assert.AreEqual (chars.Length, actual.Length);
			for (int i = 0; i < chars.Length; i++)
				Assert.AreEqual (chars[i], actual[i]);

			string expected = @"""\u0000\u0001\u0002\u0003\u0004\u0005\u0006\u0007\b\t\n\u000b\f\r\u000e\u000f\u0010\u0011\u0012\u0013\u0014\u0015\u0016\u0017\u0018\u0019\u001a\u001b\u001c\u001d\u001e\u001f !\""#$%&\u0027()*+,-./0123456789:;\u003c=\u003e?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~""";
			string data = "\u0000\u0001\u0002\u0003\u0004\u0005\u0006\u0007\b\t\n\u000b\f\r\u000e\u000f\u0010\u0011\u0012\u0013\u0014\u0015\u0016\u0017\u0018\u0019\u001a\u001b\u001c\u001d\u001e\u001f !\"#$%&\u0027()*+,-./0123456789:;\u003c=\u003e?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";

			string serRes = ser.Serialize (data);
			Assert.AreEqual (expected, serRes);
			string deserRes = ser.Deserialize<string> (serRes);
			Assert.AreEqual (data, deserRes);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		[Category ("NotDotNet")]
		public void TestSerialize2 () {
			JavaScriptSerializer ser = new JavaScriptSerializer ();
			ser.Serialize ("aaa", (StringBuilder)null);
		}

		static readonly long InitialJavaScriptDateTicks = new DateTime (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;

		[Test]
		public void TestSerializeDate () {
			JavaScriptSerializer ser = new JavaScriptSerializer ();
			DateTime now = new DateTime (633213894056010000L);

			string actual = ser.Serialize (now);
			DateTime dateTime = now.ToUniversalTime ();
			long javaScriptTicks = (dateTime.Ticks - InitialJavaScriptDateTicks) / (long) 10000;

			object dd = ser.DeserializeObject (@"""\/Datte(" + javaScriptTicks + @")\/""");
			Assert.AreEqual (@"""\/Date(" + javaScriptTicks + @")\/""", actual);
			Assert.AreEqual (now.ToUniversalTime(), ser.DeserializeObject (actual));
		}

		[Test]
		public void TestSerializeEnum () {
			JavaScriptSerializer ser = new JavaScriptSerializer ();
			string result = ser.Serialize (MyEnum.BBB);
			Assert.AreEqual ("1", result);
			Assert.AreEqual (MyEnum.BBB, ser.Deserialize<MyEnum> (result));
		}

		class MyJavaScriptConverter : JavaScriptConverter
		{
			public override object Deserialize (IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer) {
				throw new Exception ("The method or operation is not implemented.");
			}

			public override IDictionary<string, object> Serialize (object obj, JavaScriptSerializer serializer) {
				Array a = (Array) obj;
				Dictionary<string, object> d = new Dictionary<string, object> ();
				d.Add ("0", 1);
				d.Add ("1", 2);
				return d;
				//throw new Exception ("The method or operation is not implemented.");
			}

			public override IEnumerable<Type> SupportedTypes {
				get {
					yield return typeof (X[]);
				}
			}
		}

		sealed class CultureInfoConverter : JavaScriptConverter
		{
			static readonly Type typeofCultureInfo = typeof (CultureInfo);
			public override IEnumerable<Type> SupportedTypes {
				get { yield return typeofCultureInfo; }
			}

			public override object Deserialize (IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer) {
				throw new NotSupportedException ();
			}

			public override IDictionary<string, object> Serialize (object obj, JavaScriptSerializer serializer) {
				CultureInfo ci = (CultureInfo)obj;
				if (ci == null)
					return null;
				Dictionary<string, object> d = new Dictionary<string, object> ();
				d.Add ("name", ci.Name);
				d.Add ("numberFormat", ci.NumberFormat);
				d.Add ("dateTimeFormat", ci.DateTimeFormat);
				return d;
			}
		}

		public class ListItemCollectionConverter : JavaScriptConverter
		{
			public override IEnumerable<Type> SupportedTypes {
				//Define the ListItemCollection as a supported type.
				get { return new ReadOnlyCollection<Type> (new Type [] { typeof (ListItemCollection) }); }
			}

			public override IDictionary<string, object> Serialize (object obj, JavaScriptSerializer serializer) {
				ListItemCollection listType = obj as ListItemCollection;

				if (listType != null) {
					// Create the representation.
					Dictionary<string, object> result = new Dictionary<string, object> ();
					ArrayList itemsList = new ArrayList ();
					foreach (ListItem item in listType) {
						//Add each entry to the dictionary.
						Dictionary<string, object> listDict = new Dictionary<string, object> ();
						listDict.Add ("Value", item.Value);
						listDict.Add ("Text", item.Text);
						itemsList.Add (listDict);
					}
					result ["List"] = itemsList;

					return result;
				}
				return new Dictionary<string, object> ();
			}

			public override object Deserialize (IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer) {
				if (dictionary == null)
					throw new ArgumentNullException ("dictionary");

				if (type == typeof (ListItemCollection)) {
					// Create the instance to deserialize into.
					ListItemCollection list = new ListItemCollection ();

					// Deserialize the ListItemCollection's items.
					ArrayList itemsList = (ArrayList) dictionary ["List"];
					for (int i = 0; i < itemsList.Count; i++)
						list.Add (serializer.ConvertToType<ListItem> (itemsList [i]));

					return list;
				}
				return null;
			}
		}

		[Test]
		public void DeserializeObject () {
			object o = new JavaScriptSerializer ().DeserializeObject ("{\"Numeric\":0,\"Array\":[true,false,0]}");
			Assert.IsNotNull (o as Dictionary<string, object>, "type");
			Dictionary<string, object> dictionary = (Dictionary<string, object>) o;
			Assert.AreEqual (0, (int) dictionary ["Numeric"], "Numeric");
			Assert.IsNotNull (dictionary ["Array"] as object [], "Array type");
			object [] array = (object []) dictionary ["Array"];
			Assert.AreEqual (true, (bool) array [0], "array [0]");
			Assert.AreEqual (false, (bool) array [1], "array [1]");
			Assert.AreEqual (0, (int) array [2], "array [2]");
		}

		[Test]
		public void DeserializeObject2 () 
		{
			JavaScriptSerializer ser = new JavaScriptSerializer ();
			Y y = new Y ();
			string s = ser.Serialize (y);
			object y2 = ser.DeserializeObject (s);
			Assert.AreEqual (typeof (Dictionary<string, object>), y2.GetType (), "DeserializeObject to Dictionary");
		}

		[Test]
		public void DeserializeObject3 () 
		{
			JavaScriptSerializer ser = new JavaScriptSerializer (new SimpleTypeResolver());
			Y y = new Y ();
			string s = ser.Serialize (y);
			object y2 = ser.DeserializeObject (s);
			Assert.AreEqual (typeof (Y), y2.GetType (), "DeserializeObject to Dictionary");
		}

		[Test]
		public void DeserializeObject4 () 
		{
			JavaScriptSerializer ser = new JavaScriptSerializer (new CustomResolver());
			Y y = new Y ();
			string s = ser.Serialize (y);
			object y2 = ser.DeserializeObject (s);
			Assert.AreEqual (typeof (Y), y2.GetType (), "DeserializeObject to Dictionary");
			Assert.AreEqual (1, CustomResolver.ResolvedIds.Count, "ResolvedIds Count");
			Assert.AreEqual ("Y", CustomResolver.ResolvedIds [0], "ResolvedIds.Y");
			Assert.AreEqual (1, CustomResolver.ResolvedTypes.Count, "ResolvedTypes Count");
			Assert.AreEqual ("Y", CustomResolver.ResolvedTypes [0], "ResolvedTypes.Y");
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void SerializeWithResolverDeserializeWithout () 
		{
			JavaScriptSerializer ser = new JavaScriptSerializer (new SimpleTypeResolver ());
			Y y = new Y ();
			string s = ser.Serialize (y);
			ser = new JavaScriptSerializer ();
			object y2 = ser.DeserializeObject (s);
		}

		[Test]
		public void SerializeWithoutResolverDeserializeWith ()
		{
			JavaScriptSerializer ser = new JavaScriptSerializer ();
			Y y = new Y ();
			string s = ser.Serialize (y);
			ser = new JavaScriptSerializer (new SimpleTypeResolver ());
			object y2 = ser.DeserializeObject (s);
			Assert.AreEqual (typeof (Dictionary<string, object>), y2.GetType (), "DeserializeObject to Dictionary");
		}

		class B
		{
			public int v1 = 15;
			public string s1 = "s1";
		}

		class D : B
		{
			public int v2 = 16;
			public string s2 = "s2";
		}

		class C
		{
			public B b1 = new B ();
			public B b2 = new D ();
		}

		[Test]
		public void SerializeDerivedType () 
		{
			JavaScriptSerializer ser = new JavaScriptSerializer (new SimpleTypeResolver ());
			B b = new D ();
			string s = ser.Serialize (b);
			B b2 = ser.Deserialize<B> (s);
			Assert.AreEqual (typeof (D), b2.GetType (), "Deserialize Derived Type");
		}

		[Test]
		public void SerializeDerivedType2 () 
		{
			JavaScriptSerializer ser = new JavaScriptSerializer (new SimpleTypeResolver ());
			B b = new D ();
			string s = ser.Serialize (b);
			B b2 = (B)ser.DeserializeObject (s);
			Assert.AreEqual (typeof (D), b2.GetType (), "Deserialize Derived Type");
		}

		[Test]
		public void SerializeContainedDerivedType () 
		{
			JavaScriptSerializer ser = new JavaScriptSerializer (new SimpleTypeResolver ());
			C c = new C ();
			string s = ser.Serialize (c);
			C c2 = ser.Deserialize<C> (s);
			Assert.AreEqual (typeof (C), c2.GetType (), "Deserialize Derived Type");
			Assert.AreEqual (typeof (D), c2.b2.GetType (), "Deserialize Derived Type");
		}

		[Test]
		public void SerializeContainedDerivedType2 () 
		{
			JavaScriptSerializer ser = new JavaScriptSerializer (new SimpleTypeResolver ());
			C c = new C ();
			string s = ser.Serialize (c);
			C c2 = (C)ser.DeserializeObject (s);
			Assert.AreEqual (typeof (C), c2.GetType (), "Deserialize Derived Type");
			Assert.AreEqual (typeof (D), c2.b2.GetType (), "Deserialize Derived Type");
		}

		[Test]
		public void SerializeWithTypeConverter () 
		{
			JavaScriptSerializer ser = new JavaScriptSerializer ();
			MyUri uri = new MyUri ("http://kostat@mainsoft/adfasdf/asdfasdf.aspx/asda/ads?a=b&c=d", UriKind.RelativeOrAbsolute);
			string s = ser.Serialize (uri);
			MyUri uri2 = ser.Deserialize<MyUri> (s);
			Assert.AreEqual (uri, uri2);
		}

		[Test]
		public void SerializeWithTypeConverter2 () 
		{
			JavaScriptSerializer ser = new JavaScriptSerializer ();
			MyPointContainer pc = new MyPointContainer(new Point(15, 16));
			string s = ser.Serialize(pc);
			MyPointContainer pc2 = ser.Deserialize<MyPointContainer>(s);
		}
		
		[Test]
		public void MaxJsonLengthDeserializeObject () 
		{
			JavaScriptSerializer ser = new JavaScriptSerializer ();
			ser.MaxJsonLength = 16;
			object o = ser.DeserializeObject ("{s:'1234567890'}");
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void MaxJsonLengthDeserializeObjectToLong () 
		{
			JavaScriptSerializer ser = new JavaScriptSerializer ();
			ser.MaxJsonLength = 15;
			object o = ser.DeserializeObject ("{s:'1234567890'}");
		}

		[Test]
		public void MaxJsonLengthSerialize () 
		{
			JavaScriptSerializer ser = new JavaScriptSerializer ();
			ser.MaxJsonLength = 9;
			Y y = new Y ();
			string s = ser.Serialize (y);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void MaxJsonLengthSerializeToLong () 
		{
			JavaScriptSerializer ser = new JavaScriptSerializer ();
			ser.MaxJsonLength = 8;
			Y y = new Y ();
			string s = ser.Serialize (y);
		}

		[Test]
		public void RecursionLimitDeserialize1 () 
		{
			JavaScriptSerializer ser = new JavaScriptSerializer ();
			ser.RecursionLimit = 3;
			YY yy = ser.Deserialize<YY> ("{\"Y1\":{\"BB\":10},\"Y2\":{\"BB\":10}}");
		}

		[Test]
		public void RecursionLimitDeserialize2 () 
		{
			JavaScriptSerializer ser = new JavaScriptSerializer ();
			ser.RecursionLimit = 2;
			YY yy = ser.Deserialize<YY> ("{\"Y1\":{},\"Y2\":{}}");
		}

		[Test]
		public void RecursionLimitDeserialize3 () 
		{
			JavaScriptSerializer ser = new JavaScriptSerializer ();
			ser.RecursionLimit = 1;
			object o = ser.DeserializeObject ("\"xxx\"");
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void RecursionLimitDeserializeToDeep () 
		{
			JavaScriptSerializer ser = new JavaScriptSerializer ();
			ser.RecursionLimit = 2;
			YY yy = ser.Deserialize<YY> ("{\"Y1\":{\"BB\":10},\"Y2\":{\"BB\":10}}");
		}

		[Test]
		public void RecursionLimitSerialize () 
		{
			JavaScriptSerializer ser = new JavaScriptSerializer ();
			ser.RecursionLimit = 3;
			YY yy = new YY();
			string s = ser.Serialize (yy);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void RecursionLimitSerializeToDeep () 
		{
			JavaScriptSerializer ser = new JavaScriptSerializer ();
			ser.RecursionLimit = 2;
			YY yy = new YY ();
			string s = ser.Serialize (yy);
		}

		[Test]
		public void RecursionLimitSerialize2 () 
		{
			JavaScriptSerializer ser = new JavaScriptSerializer ();
			ser.RecursionLimit = 2;
			YY yy = new YY ();
			StringBuilder b = new StringBuilder ();
			bool caughtException = false;
			try {
				ser.Serialize (yy, b);
			}
			catch {
				caughtException = true;
			}
			Assert.IsTrue (caughtException, "RecursionLimitSerialize2 Expected an exception!");
			Assert.AreEqual ("{\"Y1\":{\"BB\":", b.ToString (), "RecursionLimitSerialize2");
		}

		[Test]
		public void SimpleTypeResolver () 
		{
			JavaScriptSerializer ser = new JavaScriptSerializer (new SimpleTypeResolver ());
			YY yy = new YY ();
			string s = ser.Serialize (yy);
			string expected = String.Format("\"__type\":\"{0}\"", yy.GetType().AssemblyQualifiedName);

			Assert.IsTrue (s.Contains (expected), "YY: expected {0} to contain {1}", s, expected);
			
			expected = String.Format ("\"__type\":\"{0}\"", yy.Y1.GetType ().AssemblyQualifiedName);
			Assert.IsTrue (s.Contains (expected), "Y: expected {0} to contain {1}", s, expected);
		}

		public class CustomResolver : JavaScriptTypeResolver
		{
			public CustomResolver () 
			{
				Reset ();
			}

			public override Type ResolveType (string id) 
			{
				ResolvedIds.Add (id);

				switch (id) {
				case "YY":
					return typeof(YY);

				case "Y":
					return typeof (Y);

				case "X":
					return typeof (X);

				case "int":
					return typeof (int);

				case "long":
					return typeof (long);

				case "string":
					return typeof (string);

				case "point":
					return typeof(Point);
				}
				return null;
			}

			public override string ResolveTypeId (Type type) 
			{
				if (type == null) {
					throw new ArgumentNullException ("type");
				}

				ResolvedTypes.Add (type.Name);

				if (type == typeof (YY))
					return "YY";

				if (type == typeof (Y))
					return "Y";

				if (type == typeof (X))
					return "X";

				if (type == typeof (int))
					return "int";

				if (type == typeof (long))
					return "long";

				if (type == typeof (string))
					return "string";

				if (type == typeof(Point))
					return "point";

				return null;
			}

			public static List<string> ResolvedTypes {
				get {
					if (resolvedTypes == null) {
						resolvedTypes = new List<string> ();
					}
					return resolvedTypes;
				}
			}

			public static List<string> ResolvedIds {
				get {
					if (resolvedIds == null) {
						resolvedIds = new List<string> ();
					}
					return resolvedIds;
				}
			}

			public static void Reset () 
			{
				resolvedIds = null;
				resolvedTypes = null;
			}

			private static List<string> resolvedTypes;
			private static List<string> resolvedIds;
		}

		[Test]
		[NUnit.Framework.Category ("NotWorking")]
		public void CustomTypeResolver ()
		{
			JavaScriptSerializer ser = new JavaScriptSerializer (new CustomResolver ());
			X x = new X ();
			x.Init ();

			string s = ser.Serialize (x);

			CustomResolver.Reset ();
			X x1 = (X) ser.DeserializeObject (s);
			Assert.IsTrue (x.Equals (x1), "x != x1");

			CustomResolver.Reset ();
			X x2 = ser.Deserialize<X> (s);
			Assert.IsTrue (x.Equals (x2), "x != x2");
		}

		[Test]
		public void InfinityAndNaN ()
		{
			JavaScriptSerializer ser = new JavaScriptSerializer ();

			double nan = Double.NaN;
			string s = ser.Serialize (nan);
			Assert.AreEqual (s, "NaN", "#A1");
			nan = (double)ser.DeserializeObject (s);
			Assert.AreEqual (Double.NaN, nan, "#A2");
			nan = (double)ser.Deserialize <double> (s);
			Assert.AreEqual (Double.NaN, nan, "#A3");
			
			double infinity = Double.PositiveInfinity;
			s = ser.Serialize (infinity);
			Assert.AreEqual (s, "Infinity", "#B1");
			infinity = (double)ser.DeserializeObject (s);
			Assert.AreEqual (Double.PositiveInfinity, infinity, "#B2");
			infinity = ser.Deserialize <double> (s);
			Assert.AreEqual (Double.PositiveInfinity, infinity, "#B3");
			
			infinity = Double.NegativeInfinity;
			s = ser.Serialize (infinity);
			Assert.AreEqual (s, "-Infinity", "#C1");
			infinity = (double)ser.DeserializeObject (s);
			Assert.AreEqual (Double.NegativeInfinity, infinity, "#C2");
			infinity = ser.Deserialize <double> (s);
			Assert.AreEqual (Double.NegativeInfinity, infinity, "#C3");

			var dict = new Dictionary <string, object> ();
			dict.Add ("A", Double.NaN);
			dict.Add ("B", Double.PositiveInfinity);
			dict.Add ("C", Double.NegativeInfinity);
			s = ser.Serialize (dict);
			Assert.AreEqual ("{\"A\":NaN,\"B\":Infinity,\"C\":-Infinity}", s, "#D1");
			
			dict = (Dictionary <string, object>)ser.DeserializeObject (s);
			Assert.AreEqual (Double.NaN, dict ["A"], "#D2");
			Assert.AreEqual (Double.PositiveInfinity, dict ["B"], "#D3");
			Assert.AreEqual (Double.NegativeInfinity, dict ["C"], "#D4");

			dict = (Dictionary <string, object>)ser.Deserialize <Dictionary <string, object>> (s);
			Assert.AreEqual (Double.NaN, dict ["A"], "#D5");
			Assert.AreEqual (Double.PositiveInfinity, dict ["B"], "#D6");
			Assert.AreEqual (Double.NegativeInfinity, dict ["C"], "#D7");

			var arr = new ArrayList () {
					Double.NaN,
					Double.PositiveInfinity,
					Double.NegativeInfinity};
			s = ser.Serialize (arr);
			Assert.AreEqual ("[NaN,Infinity,-Infinity]", s, "#E1");

			object[] arr2 = (object[])ser.DeserializeObject (s);
			Assert.AreEqual (3, arr2.Length, "#E2");
			Assert.AreEqual (Double.NaN, arr2 [0], "#E3");
			Assert.AreEqual (Double.PositiveInfinity, arr2 [1], "#E4");
			Assert.AreEqual (Double.NegativeInfinity, arr2 [2], "#E5");

			arr = ser.Deserialize <ArrayList> (s);
			Assert.AreEqual (3, arr.Count, "#E6");
			Assert.AreEqual (Double.NaN, arr [0], "#E7");
			Assert.AreEqual (Double.PositiveInfinity, arr [1], "#E8");
			Assert.AreEqual (Double.NegativeInfinity, arr [2], "#E9");
		}

		[Test]
		public void StandalonePrimitives ()
		{
			JavaScriptSerializer ser = new JavaScriptSerializer ();

			object o;
			int i;
			
			o = ser.DeserializeObject ("1");
			Assert.AreEqual (typeof (global::System.Int32), o.GetType (), "#A1");
			i = (int)o;
			Assert.AreEqual (1, i, "#A2");
			o =ser.DeserializeObject ("-1");
			Assert.AreEqual (typeof (global::System.Int32), o.GetType (), "#A3");
			i = (int)o;
			Assert.AreEqual (-1, i, "#A4");
			
			o = ser.DeserializeObject ("2147483649");
			Assert.AreEqual (typeof (global::System.Int64), o.GetType (), "#B1");
			long l = (long)o;
			Assert.AreEqual (2147483649, l, "#B2");
			o = ser.DeserializeObject ("-2147483649");
			Assert.AreEqual (typeof (global::System.Int64), o.GetType (), "#B3");
			l = (long)o;
			Assert.AreEqual (-2147483649, l, "#B4");

			o = ser.DeserializeObject ("9223372036854775808");
			Assert.AreEqual (typeof (global::System.Decimal), o.GetType (), "#C1");
			decimal d = (decimal)o;
			Assert.AreEqual (9223372036854775808m, d, "#C2");
			o = ser.DeserializeObject ("-9223372036854775809");
			Assert.AreEqual (typeof (global::System.Decimal), o.GetType (), "#C3");
			d = (decimal)o;
			Assert.AreEqual (-9223372036854775809m, d, "#C4");

			o = ser.DeserializeObject ("79228162514264337593543950336");
			Assert.AreEqual (typeof (global::System.Double), o.GetType (), "#D1");
			double db = (double)o;
			Assert.AreEqual (79228162514264337593543950336.0, db, "#D2");
			o = ser.DeserializeObject ("-79228162514264337593543950336");
			Assert.AreEqual (typeof (global::System.Double), o.GetType (), "#D3");
			db = (double)o;
			Assert.AreEqual (-79228162514264337593543950336.0, db, "#D4");
			
			o = ser.DeserializeObject ("\"test string\"");
			Assert.AreEqual (typeof (global::System.String), o.GetType (), "#E1");
			string s = (string)o;
			Assert.AreEqual ("test string", s, "#E2");

			o = ser.DeserializeObject ("true");
			Assert.AreEqual (typeof (global::System.Boolean), o.GetType (), "#F1");
			bool b = (bool)o;
			Assert.AreEqual (true, b, "#F2");

			o = ser.DeserializeObject ("false");
			Assert.AreEqual (typeof (global::System.Boolean), o.GetType (), "#F3");
			b = (bool)o;
			Assert.AreEqual (false, b, "#F4");

			o = ser.DeserializeObject ("-1.7976931348623157E+308");
			Assert.AreEqual (typeof (global::System.Double), o.GetType (), "#G1");
			db = (double)o;
			Assert.AreEqual (Double.MinValue, db, "#G2");

			o = ser.DeserializeObject ("1.7976931348623157E+308");
			Assert.AreEqual (typeof (global::System.Double), o.GetType (), "#G3");
			db = (double)o;
			Assert.AreEqual (Double.MaxValue, db, "#G4");
		}

		class SomeDict : IDictionary<string, object>
		{
			void IDictionary<string, object>.Add (string key, object value) {
				throw new NotSupportedException ();
			}

			bool IDictionary<string, object>.ContainsKey (string key) {
				throw new NotSupportedException ();
			}

			ICollection<string> IDictionary<string, object>.Keys {
				get { throw new NotSupportedException (); }
			}

			bool IDictionary<string, object>.Remove (string key) {
				throw new NotSupportedException ();
			}

			bool IDictionary<string, object>.TryGetValue (string key, out object value) {
				throw new NotSupportedException ();
			}

			ICollection<object> IDictionary<string, object>.Values {
				get { throw new NotSupportedException (); }
			}

			object IDictionary<string, object>.this [string key] {
				get { throw new NotSupportedException (); }
				set { throw new NotSupportedException (); }
			}

			void ICollection<KeyValuePair<string, object>>.Add (KeyValuePair<string, object> item) {
				throw new NotSupportedException ();
			}

			void ICollection<KeyValuePair<string, object>>.Clear () {
				throw new NotSupportedException ();
			}

			bool ICollection<KeyValuePair<string, object>>.Contains (KeyValuePair<string, object> item) {
				throw new NotSupportedException ();
			}

			void ICollection<KeyValuePair<string, object>>.CopyTo (KeyValuePair<string, object> [] array, int arrayIndex) {
				throw new NotSupportedException ();
			}

			int ICollection<KeyValuePair<string, object>>.Count {
				get { throw new NotSupportedException (); }
			}

			bool ICollection<KeyValuePair<string, object>>.IsReadOnly {
				get { throw new NotSupportedException (); }
			}

			bool ICollection<KeyValuePair<string, object>>.Remove (KeyValuePair<string, object> item) {
				throw new NotSupportedException ();
			}

			IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator () {
				return GetEnumerator ();
			}

			IEnumerator IEnumerable.GetEnumerator () {
				return ((IEnumerable<KeyValuePair<string, object>>) this).GetEnumerator ();
			}

			protected IEnumerator<KeyValuePair<string, object>> GetEnumerator () {
				yield return new KeyValuePair<string, object> ("hello", "world");
			}
		}

		[Test] //bug #424704
		public void NonGenericClassImplementingClosedGenericIDictionary ()
		{
			JavaScriptSerializer ser = new JavaScriptSerializer ();

			SomeDict dictIn = new SomeDict ();

			string s = ser.Serialize (dictIn);

			Dictionary<string, object> dictOut = ser.Deserialize<Dictionary<string, object>> (s);
			Assert.AreEqual (dictOut.Count, 1, "#1");
			Assert.AreEqual (dictOut["hello"], "world", "#2");
		}

		[Test]
		public void ConvertToIDictionary ()
		{
			JavaScriptSerializer jss = new JavaScriptSerializer ();
			string json = "{\"node\":{\"Text\":\"Root Node\",\"Value\":null,\"ExpandMode\":3,\"NavigateUrl\":null,\"PostBack\":true,\"DisabledCssClass\":null,\"SelectedCssClass\":null,\"HoveredCssClass\":null,\"ImageUrl\":null,\"HoveredImageUrl\":null,\"DisabledImageUrl\":null,\"ExpandedImageUrl\":null,\"ContextMenuID\":\"\"},\"context\":{\"NumberOfNodes\":1000}}";
			object input = jss.Deserialize<IDictionary>(json);
			IDictionary o = jss.ConvertToType<IDictionary>(input);

			Assert.IsTrue (o != null, "#A1");
			Assert.AreEqual (typeof (Dictionary <string, object>), o.GetType (), "#A2");
		}

		[Test]
		public void ConvertToGenericIDictionary ()
		{
			JavaScriptSerializer jss = new JavaScriptSerializer ();
			string json = "{\"node\":{\"Text\":\"Root Node\",\"Value\":null,\"ExpandMode\":3,\"NavigateUrl\":null,\"PostBack\":true,\"DisabledCssClass\":null,\"SelectedCssClass\":null,\"HoveredCssClass\":null,\"ImageUrl\":null,\"HoveredImageUrl\":null,\"DisabledImageUrl\":null,\"ExpandedImageUrl\":null,\"ContextMenuID\":\"\"},\"context\":{\"NumberOfNodes\":1000}}";

			object input = jss.Deserialize<IDictionary>(json);
			
			IDictionary <string, object> o = jss.ConvertToType<IDictionary <string, object>>(input);
			Assert.IsTrue (o != null, "#A1");
			Assert.AreEqual (typeof (Dictionary <string, object>), o.GetType (), "#A2");

			IDictionary <object, object> o1 = jss.ConvertToType<IDictionary <object, object>>(input);
			Assert.IsTrue (o1 != null, "#B1");
			Assert.AreEqual (typeof (Dictionary <object, object>), o1.GetType (), "#B2");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ConvertToGenericIDictionary_InvalidDefinition ()
		{
			JavaScriptSerializer jss = new JavaScriptSerializer ();
			string json = "{\"node\":{\"Text\":\"Root Node\",\"Value\":null,\"ExpandMode\":3,\"NavigateUrl\":null,\"PostBack\":true,\"DisabledCssClass\":null,\"SelectedCssClass\":null,\"HoveredCssClass\":null,\"ImageUrl\":null,\"HoveredImageUrl\":null,\"DisabledImageUrl\":null,\"ExpandedImageUrl\":null,\"ContextMenuID\":\"\"},\"context\":{\"NumberOfNodes\":1000}}";

			object input = jss.Deserialize<IDictionary>(json);
			IDictionary <int, object> o = jss.ConvertToType<IDictionary <int, object>>(input);
		}

		[Test (Description="Bug #655474")]
		public void TestRelativeUri()
		{
			JavaScriptSerializer ser = new JavaScriptSerializer();
			Uri testUri = new Uri("/lala/123", UriKind.Relative);
			StringBuilder sb = new StringBuilder();

			ser.Serialize(testUri, sb);
			Assert.AreEqual ("\"/lala/123\"", sb.ToString ());
		}
		
		[Test]
		public void DeserializeDictionaryOfArrayList ()
		{
			var ser = new JavaScriptSerializer ();
			string test1 = "{\"key\":{\"subkey\":\"subval\"}}";
			string test2 = "{\"key\":[{\"subkey\":\"subval\"}]}";

			var ret1 = ser.Deserialize<Dictionary<string, object>>(test1);
			Assert.AreEqual (typeof (Dictionary<string, object>), ret1.GetType (), "#1.1");
			var ret1v = ret1 ["key"];
			Assert.AreEqual (typeof (Dictionary<string, object>), ret1v.GetType (), "#1.2");
			var ret1vd = (IDictionary<string,object>) ret1v;
			Assert.AreEqual ("subval", ret1vd ["subkey"], "#1.3");

			var ret2 = ser.Deserialize<Dictionary<string, object>>(test2);
			Assert.AreEqual (typeof (Dictionary<string, object>), ret2.GetType (), "#2.1");
			var ret2v = ret2 ["key"];
			Assert.AreEqual (typeof (ArrayList), ret2v.GetType (), "#2.2");
			var ret2va = (ArrayList) ret2v;
			Assert.AreEqual (typeof (Dictionary<string, object>), ret2va [0].GetType (), "#2.3");
			var ret2vad = (IDictionary<string,object>) ret2va [0];
			Assert.AreEqual ("subval", ret2vad ["subkey"], "#2.4");
		}
	}
}
