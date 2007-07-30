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


namespace Tests.System.Web.Script.Serialization
{
	[TestFixture]
	public class JavaScriptSerializerTest
	{
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

#pragma warning restore 659

		[Test]
		public void TestDefaults () {
			JavaScriptSerializer ser = new JavaScriptSerializer ();
			Assert.AreEqual (102400, ser.MaxJsonLength);
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
		public void TestDeserialize () {
			JavaScriptSerializer ser = new JavaScriptSerializer ();
			Assert.IsNull (ser.Deserialize<X> (""));

			X s = new X ();
			s.Init ();
			string x = ser.Serialize (s);
			X n = ser.Deserialize<X> (x);
			Assert.AreEqual (s, n);

			//string json = "\\uFF56";
			//string result = ser.Deserialize<string> (json);
			//Assert.AreEqual ("\uFF56", result);

			//object oo = ser.DeserializeObject ("{value:'Purple\\r \\n monkey\\'s:\\tdishwasher'}");
		}

		[Test]
		[Category("NotWorking")]
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
		public void TestSerialize2 () {
			JavaScriptSerializer ser = new JavaScriptSerializer ();
			ser.Serialize ("aaa", null);
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
		[ExpectedException (typeof (InvalidOperationException), "Maximum length exceeded.")]
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
		public void RecursionLimitDeserializeToDeap () 
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
		public void RecursionLimitSerializeToDeap () 
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
	}
}
