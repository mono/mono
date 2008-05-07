//
// OracleParameterCollectionTest.cs -
//      NUnit Test Cases for OracleParameterCollection
//
// Author:
//      Amit Biswas  <amit@amitbiswas.com>
//
// Copyright (C) 2007 Novell (http://www.novell.com)
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
using System.Data;
using System.Data.OracleClient;
using System.Globalization;
using System.Threading;

using NUnit.Framework;

namespace MonoTests.System.Data.OracleClient
{
	[TestFixture]
	public class OracleParameterCollectionTest
	{
		OracleParameterCollection o;
		CultureInfo oldCulture;

		[SetUp]
		public void SetUp ()
		{
			oldCulture = CultureInfo.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = new CultureInfo ("tr-TR");
			o = new OracleParameterCollection ();
		}

		[TearDown]
		public void TearDown ()
		{
			Thread.CurrentThread.CurrentCulture = oldCulture;
		}

		[Test]
		public void Clear ()
		{
			OracleParameter paramA = new OracleParameter ("ParamI", 1);
			OracleParameter paramB = new OracleParameter ("Parami", 2);

			o.Clear ();
			o.Add (paramA);
			o.Add (paramB);
			o.Clear ();

			Assert.AreEqual (0, o.Count, "#1");
			Assert.AreEqual (-1, o.IndexOf (paramA), "#2");
			Assert.AreEqual (-1, o.IndexOf (paramB), "#3");

			o.Add (paramA);
		}

		[Test]
		public void Count ()
		{
			Assert.AreEqual (0, o.Count, "#1");
			o.Add (new OracleParameter ());
			Assert.AreEqual (1, o.Count, "#2");
			o.Add (new OracleParameter ());
			Assert.AreEqual (2, o.Count, "#3");
			o.RemoveAt (0);
			Assert.AreEqual (1, o.Count, "#4");
			o.RemoveAt (0);
			Assert.AreEqual (0, o.Count, "#6");
		}

		[Test]
		public void IsFixedSize ()
		{
			Assert.IsFalse (o.IsFixedSize);
		}

		[Test]
		public void IsReadOnly ()
		{
			Assert.IsFalse (o.IsReadOnly);
		}

		[Test]
		public void IsSynchronized ()
		{
			Assert.IsFalse (o.IsSynchronized);
		}

		[Test] // Add (Object)
		public void Add1_Value_InvalidType ()
		{
			try {
				o.Add ((object) "ParamI");
				Assert.Fail ("#A1");
			} catch (InvalidCastException ex) {
				// The OracleParameterCollection only accepts
				// non-null OracleParameter type objects, not
				// String objects
				Assert.AreEqual (typeof (InvalidCastException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
#if NET_2_0
				Assert.IsTrue (ex.Message.IndexOf (o.GetType ().Name) != -1, "#A5");
				Assert.IsTrue (ex.Message.IndexOf (typeof (string).Name) != -1, "#A6");
#endif
				Assert.IsFalse (ex.Message.IndexOf (typeof (int).Name) != -1, "#A7");
				Assert.IsTrue (ex.Message.IndexOf (typeof (OracleParameter).Name) != -1, "#A8");
			}

			try {
				o.Add ((object) 5);
				Assert.Fail ("#B1");
			} catch (InvalidCastException ex) {
				// The OracleParameterCollection only accepts
				// non-null OracleParameter type objects, not
				// String objects
				Assert.AreEqual (typeof (InvalidCastException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
#if NET_2_0
				Assert.IsTrue (ex.Message.IndexOf (o.GetType ().Name) != -1, "#B5");
				Assert.IsFalse (ex.Message.IndexOf (typeof (string).Name) != -1, "#B6");
				Assert.IsTrue (ex.Message.IndexOf (typeof (int).Name) != -1, "#B7");
#endif
				Assert.IsTrue (ex.Message.IndexOf (typeof (OracleParameter).Name) != -1, "#B8");
			}
		}

		[Test] // Add (Object)
		public void Add1_Value_Null ()
		{
			try {
				o.Add ((object) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
#if NET_2_0
				// The OracleParameterCollection only accepts
				// non-null OracleParameter type objects
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf (o.GetType ().Name) != -1, "#5");
				Assert.IsTrue (ex.Message.IndexOf (typeof (OracleParameter).Name) != -1, "#6");
				Assert.AreEqual ("value", ex.ParamName, "#7");
#else
				// Value cannot be null
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsFalse (ex.Message.IndexOf (o.GetType ().Name) != -1, "#5");
				Assert.IsFalse (ex.Message.IndexOf (typeof (OracleParameter).Name) != -1, "#6");
				Assert.AreEqual ("value", ex.ParamName, "#7");
#endif
			}
		}

		[Test] // Add (Object)
		public void Add1_Value_Owned ()
		{
			OracleParameter param = new OracleParameter ("ParamI", 1);

			o.Add ((object) param);

			// attempt to add same OracleParameter to collection twice
			try {
				o.Add ((object) param);
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// The OracleParameter is already contained by
				// another OracleParameterCollection
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsTrue (ex.Message.IndexOf (o.GetType ().Name) != -1, "#A5");
				Assert.IsTrue (ex.Message.IndexOf (typeof (OracleParameter).Name) != -1, "#A6");
				Assert.IsNull (ex.ParamName, "#A7");
			}

			OracleParameterCollection o2 = new OracleParameterCollection ();

			// attempt to add OracleParameter to another collection
			try {
				o2.Add ((object) param);
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// The OracleParameter is already contained by
				// another OracleParameterCollection
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsTrue (ex.Message.IndexOf (o.GetType ().Name) != -1, "#B5");
				Assert.IsTrue (ex.Message.IndexOf (typeof (OracleParameter).Name) != -1, "#B6");
				Assert.IsNull (ex.ParamName, "#B7");
			}

			o.Remove ((object) param);
			o.Add ((object) param);
			o.Remove ((object) param);
			o2.Add ((object) param);
		}

		[Test] // Add (OracleParameter)
		public void Add2_Value_Null ()
		{
			try {
				o.Add ((OracleParameter) null);
				Assert.Fail ("#1");
#if NET_2_0
			} catch (ArgumentNullException ex) {
				// The OracleParameterCollection only accepts
				// non-null OracleParameter type objects
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf (o.GetType ().Name) != -1, "#5");
				Assert.IsTrue (ex.Message.IndexOf (typeof (OracleParameter).Name) != -1, "#6");
				Assert.AreEqual ("value", ex.ParamName, "#7");
			}
#else
			} catch (NullReferenceException ex) {
				Assert.AreEqual (typeof (NullReferenceException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
#endif
		}

		[Test] // Add (OracleParameter)
		public void Add2_Value_Owned ()
		{
			OracleParameter param = new OracleParameter ("ParamI", 1);

			o.Add (param);

			// attempt to add same OracleParameter to collection twice
			try {
				o.Add (param);
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// The OracleParameter is already contained by
				// another OracleParameterCollection
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsTrue (ex.Message.IndexOf (o.GetType ().Name) != -1, "#A5");
				Assert.IsTrue (ex.Message.IndexOf (typeof (OracleParameter).Name) != -1, "#A6");
				Assert.IsNull (ex.ParamName, "#A7");
			}

			OracleParameterCollection o2 = new OracleParameterCollection ();

			// attempt to add OracleParameter to another collection
			try {
				o2.Add (param);
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// The OracleParameter is already contained by
				// another OracleParameterCollection
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsTrue (ex.Message.IndexOf (o.GetType ().Name) != -1, "#B5");
				Assert.IsTrue (ex.Message.IndexOf (typeof (OracleParameter).Name) != -1, "#B6");
				Assert.IsNull (ex.ParamName, "#B7");
			}

			o.Remove (param);
			o.Add (param);
			o.Remove (param);
			o2.Add (param);
		}

#if NET_2_0
		[Test] // AddRange (Array)
		public void AddRange1 ()
		{
			OracleParameter paramA = new OracleParameter ("ParamI", 1);
			OracleParameter paramB = new OracleParameter ("Parami", 2);
			OracleParameter paramC = new OracleParameter ("paramI", 3);
			OracleParameter paramD = new OracleParameter ("parami", 4);

			o.Add (paramA);
			o.AddRange (new object [] { paramB, paramC, paramD });

			Assert.AreEqual (0, o.IndexOf (paramA), "#1");
			Assert.AreEqual (1, o.IndexOf (paramB), "#2");
			Assert.AreEqual (2, o.IndexOf (paramC), "#3");
			Assert.AreEqual (3, o.IndexOf (paramD), "#4");
		}

		[Test] // AddRange (Array)
		public void AddRange1_Item_InvalidType ()
		{
			OracleParameter paramA = new OracleParameter ("ParamI", 1);
			OracleParameter paramB = new OracleParameter ("Parami", 2);
			OracleParameter paramC = new OracleParameter ("paramI", 3);

			o.Add (paramA);

			object [] values = new object [] { paramB, "ParamX", paramC };
			try {
				o.AddRange (values);
				Assert.Fail ("#A1");
			} catch (InvalidCastException ex) {
				// The OracleParameterCollection only accepts
				// non-null OracleParameter type objects, not
				// String objects
				Assert.AreEqual (typeof (InvalidCastException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsTrue (ex.Message.IndexOf (o.GetType ().Name) != -1, "#A5");
				Assert.IsTrue (ex.Message.IndexOf (typeof (string).Name) != -1, "#A6");
				Assert.IsFalse (ex.Message.IndexOf (typeof (int).Name) != -1, "#A7");
				Assert.IsTrue (ex.Message.IndexOf (typeof (OracleParameter).Name) != -1, "#A8");
			}

			Assert.AreEqual (0, o.IndexOf (paramA), "#B1");
			Assert.AreEqual (-1, o.IndexOf (paramB), "#B2");
			Assert.AreEqual (-1, o.IndexOf (paramC), "#B3");
		}

		[Test] // AddRange (Array)
		public void AddRange1_Item_Null ()
		{
			OracleParameter paramA = new OracleParameter ("ParamI", 1);
			OracleParameter paramB = new OracleParameter ("Parami", 2);
			OracleParameter paramC = new OracleParameter ("paramI", 3);

			o.Add (paramA);

			object [] values = new object [] { paramB, null, paramC };
			try {
				o.AddRange (values);
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				// The OracleParameterCollection only accepts
				// non-null OracleParameter type objects
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsTrue (ex.Message.IndexOf (o.GetType ().Name) != -1, "#A5");
				Assert.IsTrue (ex.Message.IndexOf (typeof (OracleParameter).Name) != -1, "#A6");
				Assert.AreEqual ("value", ex.ParamName, "#A7");
			}

			Assert.AreEqual (0, o.IndexOf (paramA), "#B1");
			Assert.AreEqual (-1, o.IndexOf (paramB), "#B2");
			Assert.AreEqual (-1, o.IndexOf (paramC), "#B3");
		}

		[Test] // AddRange (Array)
		public void AddRange1_Item_Owned ()
		{
			OracleParameter paramA = new OracleParameter ("ParamI", 1);
			OracleParameter paramB = new OracleParameter ("Parami", 2);
			OracleParameter paramC = new OracleParameter ("paramI", 3);

			o.Add (paramA);

			object [] values = new object [] { paramB, paramA, paramC };
			try {
				o.AddRange (values);
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// The OracleParameter is already contained by
				// another OracleParameterCollection
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsTrue (ex.Message.IndexOf (o.GetType ().Name) != -1, "#A5");
				Assert.IsTrue (ex.Message.IndexOf (typeof (OracleParameter).Name) != -1, "#A6");
				Assert.IsNull (ex.ParamName, "#A7");
			}

			Assert.AreEqual (0, o.IndexOf (paramA), "#B1");
			Assert.AreEqual (1, o.IndexOf (paramB), "#B2");
			Assert.AreEqual (-1, o.IndexOf (paramC), "#B3");
		}

		[Test] // AddRange (Array)
		public void AddRange1_Values_Null ()
		{
			try {
				o.AddRange ((Array) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("values", ex.ParamName, "#5");
			}
		}

		[Test] // AddRange (OracleParameter [])
		public void AddRange2 ()
		{
			OracleParameter paramA = new OracleParameter ("ParamI", 1);
			OracleParameter paramB = new OracleParameter ("Parami", 2);
			OracleParameter paramC = new OracleParameter ("paramI", 3);
			OracleParameter paramD = new OracleParameter ("parami", 4);

			o.Add (paramA);

			OracleParameter [] values = new OracleParameter [] {
				paramB, paramC, paramD };
			o.AddRange (values);

			Assert.AreEqual (0, o.IndexOf (paramA), "#1");
			Assert.AreEqual (1, o.IndexOf (paramB), "#2");
			Assert.AreEqual (2, o.IndexOf (paramC), "#3");
			Assert.AreEqual (3, o.IndexOf (paramD), "#4");
		}

		[Test] // AddRange (OracleParameter [])
		public void AddRange2_Item_Null ()
		{
			OracleParameter paramA = new OracleParameter ("ParamI", 1);
			OracleParameter paramB = new OracleParameter ("Parami", 2);
			OracleParameter paramC = new OracleParameter ("paramI", 3);

			o.Add (paramA);

			OracleParameter [] values = new OracleParameter [] {
				paramB, null, paramC };
			try {
				o.AddRange (values);
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				// The OracleParameterCollection only accepts
				// non-null OracleParameter type objects
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsTrue (ex.Message.IndexOf (o.GetType ().Name) != -1, "#A5");
				Assert.IsTrue (ex.Message.IndexOf (typeof (OracleParameter).Name) != -1, "#A6");
				Assert.AreEqual ("value", ex.ParamName, "#A7");
			}

			Assert.AreEqual (0, o.IndexOf (paramA), "#B1");
			Assert.AreEqual (-1, o.IndexOf (paramB), "#B2");
			Assert.AreEqual (-1, o.IndexOf (paramC), "#B3");
		}

		[Test] // AddRange (OracleParameter [])
		public void AddRange2_Item_Owned ()
		{
			OracleParameter paramA = new OracleParameter ("ParamI", 1);
			OracleParameter paramB = new OracleParameter ("Parami", 2);
			OracleParameter paramC = new OracleParameter ("paramI", 3);

			o.Add (paramA);

			OracleParameter [] values = new OracleParameter [] {
				paramB, paramA, paramC };
			try {
				o.AddRange (values);
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// The OracleParameter is already contained by
				// another OracleParameterCollection
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsTrue (ex.Message.IndexOf (o.GetType ().Name) != -1, "#A5");
				Assert.IsTrue (ex.Message.IndexOf (typeof (OracleParameter).Name) != -1, "#A6");
				Assert.IsNull (ex.ParamName, "#A7");
			}

			Assert.AreEqual (0, o.IndexOf (paramA), "#B1");
			Assert.AreEqual (1, o.IndexOf (paramB), "#B2");
			Assert.AreEqual (-1, o.IndexOf (paramC), "#B3");
		}

		[Test] // AddRange (OracleParameter [])
		public void AddRange2_Values_Null ()
		{
			try {
				o.AddRange ((OracleParameter []) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("values", ex.ParamName, "#5");
			}
		}
#endif

		[Test] // Contains (Object)
		public void Contains1 ()
		{
			OracleParameter paramA = new OracleParameter ("ParamI", 1);
			OracleParameter paramB = new OracleParameter ("Parami", 2);
			OracleParameter paramC = new OracleParameter ("paramI", 3);
			OracleParameter paramD = new OracleParameter ("parami", 4);

			o.Add (paramA);
			o.Add (paramB);

			Assert.IsTrue (o.Contains ((object) paramA), "#A1");
			Assert.IsTrue (o.Contains ((object) paramB), "#A2");
			Assert.IsFalse (o.Contains ((object) paramC), "#A3");
			Assert.IsFalse (o.Contains ((object) paramD), "#A4");
			Assert.IsFalse (o.Contains ((object) null), "#A5");

			o.Add (paramC);

			Assert.IsTrue (o.Contains ((object) paramA), "#B1");
			Assert.IsTrue (o.Contains ((object) paramB), "#B2");
			Assert.IsTrue (o.Contains ((object) paramC), "#B3");
			Assert.IsFalse (o.Contains ((object) paramD), "#B4");
			Assert.IsFalse (o.Contains ((object) null), "#B5");

			o.Remove (paramA);

			Assert.IsFalse (o.Contains ((object) paramA), "#C1");
			Assert.IsTrue (o.Contains ((object) paramB), "#C2");
			Assert.IsTrue (o.Contains ((object) paramC), "#C3");
			Assert.IsFalse (o.Contains ((object) paramD), "#C4");
			Assert.IsFalse (o.Contains ((object) null), "#C5");
		}

		[Test] // Contains (Object)
		public void Contains1_Value_InvalidType ()
		{
			try {
				o.Contains ((object) "ParamI");
				Assert.Fail ("#A1");
			} catch (InvalidCastException ex) {
				// The OracleParameterCollection only accepts
				// non-null OracleParameter type objects, not
				// String objects
				Assert.AreEqual (typeof (InvalidCastException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
#if NET_2_0
				Assert.IsTrue (ex.Message.IndexOf (o.GetType ().Name) != -1, "#A5");
				Assert.IsTrue (ex.Message.IndexOf (typeof (string).Name) != -1, "#A6");
				Assert.IsFalse (ex.Message.IndexOf (typeof (int).Name) != -1, "#A7");
#endif
				Assert.IsTrue (ex.Message.IndexOf (typeof (OracleParameter).Name) != -1, "#A8");
			}

			try {
				o.Contains ((object) 5);
				Assert.Fail ("#B1");
			} catch (InvalidCastException ex) {
				// The OracleParameterCollection only accepts
				// non-null OracleParameter type objects, not
				// Int32 objects
				Assert.AreEqual (typeof (InvalidCastException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
#if NET_2_0
				Assert.IsTrue (ex.Message.IndexOf (o.GetType ().Name) != -1, "#B5");
				Assert.IsFalse (ex.Message.IndexOf (typeof (string).Name) != -1, "#B6");
				Assert.IsTrue (ex.Message.IndexOf (typeof (int).Name) != -1, "#B7");
#endif
				Assert.IsTrue (ex.Message.IndexOf (typeof (OracleParameter).Name) != -1, "#B8");
			}
		}

		[Test] // Contains (OracleParameter)
		public void Contains2 ()
		{
			OracleParameter paramA = new OracleParameter ("ParamI", 1);
			OracleParameter paramB = new OracleParameter ("Parami", 2);
			OracleParameter paramC = new OracleParameter ("paramI", 3);
			OracleParameter paramD = new OracleParameter ("parami", 4);

			o.Add (paramA);
			o.Add (paramB);

			Assert.IsTrue (o.Contains (paramA), "#A1");
			Assert.IsTrue (o.Contains (paramB), "#A2");
			Assert.IsFalse (o.Contains (paramC), "#A3");
			Assert.IsFalse (o.Contains (paramD), "#A4");
			Assert.IsFalse (o.Contains ((OracleParameter) null), "#A5");

			o.Add (paramC);

			Assert.IsTrue (o.Contains (paramA), "#B1");
			Assert.IsTrue (o.Contains (paramB), "#B2");
			Assert.IsTrue (o.Contains (paramC), "#B3");
			Assert.IsFalse (o.Contains (paramD), "#B4");
			Assert.IsFalse (o.Contains ((OracleParameter) null), "#B5");

			o.Remove (paramA);

			Assert.IsFalse (o.Contains (paramA), "#C1");
			Assert.IsTrue (o.Contains (paramB), "#C2");
			Assert.IsTrue (o.Contains (paramC), "#C3");
			Assert.IsFalse (o.Contains (paramD), "#C4");
			Assert.IsFalse (o.Contains ((OracleParameter) null), "#C5");
		}

		[Test] // Contains (String)
		public void Contains3 ()
		{
			OracleParameter paramA = new OracleParameter ("ParamI", 1);
			OracleParameter paramB = new OracleParameter ("Parami", 2);
			OracleParameter paramC = new OracleParameter ("paramI", 3);
			OracleParameter paramD = new OracleParameter ("parami", 4);

			o.Add (paramA);
			o.Add (paramB);

			Assert.IsTrue (o.Contains ("ParamI"), "#A1");
			Assert.IsTrue (o.Contains ("Parami"), "#A2");
			Assert.IsTrue (o.Contains ("paramI"), "#A3");
			Assert.IsTrue (o.Contains ("parami"), "#A4");
			Assert.IsFalse (o.Contains ("NotFound"), "#A5");
			Assert.IsFalse (o.Contains ((OracleParameter) null), "#A6");

			o.Remove (paramA);

			Assert.IsFalse (o.Contains ("ParamI"), "#B1");
			Assert.IsTrue (o.Contains ("Parami"), "#B2");
			Assert.IsFalse (o.Contains ("paramI"), "#B3");
			Assert.IsTrue (o.Contains ("parami"), "#B4");
			Assert.IsFalse (o.Contains ("NotFound"), "#B5");
			Assert.IsFalse (o.Contains ((OracleParameter) null), "#B6");
		}

		[Test] // OracleParameter this [Int32]
		public void Indexer1 ()
		{
			OracleParameter paramA = new OracleParameter ("ParamI", 1);
			OracleParameter paramB = new OracleParameter ("paramI", 2);
			OracleParameter paramC = new OracleParameter ("Parami", 3);

			o.Add (paramA);
			o.Add (paramB);

			Assert.AreSame (paramA, o [0], "#A1");
			Assert.AreSame (paramB, o [1], "#A2");
			o [0] = paramA;
			Assert.AreSame (paramA, o [0], "#B1");
			Assert.AreSame (paramB, o [1], "#B2");
			o [0] = paramC;
			Assert.AreSame (paramC, o [0], "#C1");
			Assert.AreSame (paramB, o [1], "#C2");
			o [1] = paramA;
			Assert.AreSame (paramC, o [0], "#D1");
			Assert.AreSame (paramA, o [1], "#D2");

			OracleParameterCollection o2 = new OracleParameterCollection ();
			o2.Add (paramB);
		}

		[Test] // OracleParameter this [Int32]
		public void Indexer1_Index_Invalid ()
		{
			o.Add (new OracleParameter ());

			try {
				o [1] = new OracleParameter ();
				Assert.Fail ("#A1");
			} catch (IndexOutOfRangeException ex) {
#if NET_2_0
				// Invalid index 1 for this OracleParameterCollection
				// with Count=1
				Assert.AreEqual (typeof (IndexOutOfRangeException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsTrue (ex.Message.IndexOf (o.GetType ().Name) != -1, "#A5");
#else
				// Index 1 outside the scope of the parameter array
				Assert.AreEqual (typeof (IndexOutOfRangeException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
#endif
			}

			try {
				o [-1] = new OracleParameter ();
				Assert.Fail ("#B1");
			} catch (IndexOutOfRangeException ex) {
#if NET_2_0
				// Invalid index -1 for this OracleParameterCollection
				// with Count=1
				Assert.AreEqual (typeof (IndexOutOfRangeException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsTrue (ex.Message.IndexOf (o.GetType ().Name) != -1, "#B5");
#else
				// Index -1 outside the scope of the parameter array
				Assert.AreEqual (typeof (IndexOutOfRangeException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
#endif
			}

			try {
				object value = o [1];
				Assert.Fail ("#C1:" + value);
			} catch (IndexOutOfRangeException ex) {
#if NET_2_0
				// Invalid index 1 for this OracleParameterCollection
				// with Count=1
				Assert.AreEqual (typeof (IndexOutOfRangeException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.IsTrue (ex.Message.IndexOf (o.GetType ().Name) != -1, "#C5");
#else
				// Index 1 outside the scope of the parameter array
				Assert.AreEqual (typeof (IndexOutOfRangeException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
#endif
			}

			try {
				object value = o [-1];
				Assert.Fail ("#D1");
			} catch (IndexOutOfRangeException ex) {
#if NET_2_0
				// Invalid index -1 for this OracleParameterCollection
				// with Count=1
				Assert.AreEqual (typeof (IndexOutOfRangeException), ex.GetType (), "#D2");
				Assert.IsNull (ex.InnerException, "#D3");
				Assert.IsNotNull (ex.Message, "#D4");
				Assert.IsTrue (ex.Message.IndexOf (o.GetType ().Name) != -1, "#D5");
#else
				// Index -1 outside the scope of the parameter array
				Assert.AreEqual (typeof (IndexOutOfRangeException), ex.GetType (), "#D2");
				Assert.IsNull (ex.InnerException, "#D3");
				Assert.IsNotNull (ex.Message, "#D4");
#endif
			}
		}

		[Test] // OracleParameter this [Int32]
		public void Indexer1_Value_Null ()
		{
			OracleParameter param = new OracleParameter ("ParamI", 1);
			o.Add (param);

			try {
				o [0] = null;
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
#if NET_2_0
				// The OracleParameterCollection only accepts
				// non-null OracleParameter type objects
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf (o.GetType ().Name) != -1, "#5");
				Assert.IsTrue (ex.Message.IndexOf (typeof (OracleParameter).Name) != -1, "#6");
				Assert.AreEqual ("value", ex.ParamName, "#7");
#else
				// Value cannot be null
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsFalse (ex.Message.IndexOf (o.GetType ().Name) != -1, "#5");
				Assert.IsFalse (ex.Message.IndexOf (typeof (OracleParameter).Name) != -1, "#6");
				Assert.AreEqual ("value", ex.ParamName, "#7");
#endif
			}
		}

		[Test] // OracleParameter this [Int32]
		public void Indexer1_Value_Owned ()
		{
			OracleParameter paramA = new OracleParameter ("ParamI", 1);
			OracleParameter paramB = new OracleParameter ("paramI", 2);
			OracleParameter paramC = new OracleParameter ("Parami", 3);

			o.Add (paramA);
			o.Add (paramB);

			// attempt to add same OracleParameter to collection twice
			try {
				o [1] = paramA;
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// The OracleParameter is already contained by
				// another OracleParameterCollection
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsTrue (ex.Message.IndexOf (o.GetType ().Name) != -1, "#A5");
				Assert.IsTrue (ex.Message.IndexOf (typeof (OracleParameter).Name) != -1, "#A6");
				Assert.IsNull (ex.ParamName, "#A7");
			}

			OracleParameterCollection o2 = new OracleParameterCollection ();
			o2.Add (paramC);

			// attempt to add OracleParameter to another collection
			try {
				o2 [0] = paramA;
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// The OracleParameter is already contained by
				// another OracleParameterCollection
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsTrue (ex.Message.IndexOf (o.GetType ().Name) != -1, "#B5");
				Assert.IsTrue (ex.Message.IndexOf (typeof (OracleParameter).Name) != -1, "#B6");
				Assert.IsNull (ex.ParamName, "#B7");
			}

			o.Remove (paramA);
			o [0] = paramA;
			o.Remove (paramA);
			o2 [0] = paramA;
		}

		[Test] // OracleParameter this [String]
		public void Indexer2 ()
		{
			OracleParameter paramA = new OracleParameter ("ParamI", 1);
			OracleParameter paramB = new OracleParameter ("Parami", 2);
			OracleParameter paramC = new OracleParameter ("paramI", 3);
			OracleParameter paramD = new OracleParameter ("parami", 4);
			OracleParameter paramE = new OracleParameter ("ParamI", 5);
			OracleParameter paramF = new OracleParameter ("Parami", 6);
			OracleParameter paramG = new OracleParameter ("ParamG", 7);
			OracleParameter paramH = new OracleParameter ("ParamH", 8);

			o.Add (paramA);
			o.Add (paramB);
			o.Add (paramC);
			o.Add (paramD);
			o.Add (paramE);
			o.Add (paramF);

			Assert.AreSame (paramA, o ["ParamI"], "#A1");
			Assert.AreSame (paramB, o ["Parami"], "#A2");
#if NET_2_0
			Assert.AreSame (paramC, o ["paramI"], "#A3");
			Assert.AreSame (paramD, o ["parami"], "#A4");
#else
			Assert.AreSame (paramA, o ["paramI"], "#A3");
			Assert.AreSame (paramB, o ["parami"], "#A4");
#endif
			Assert.AreEqual (0, o.IndexOf (paramA), "#A5");
			Assert.AreEqual (1, o.IndexOf (paramB), "#A6");
			Assert.AreEqual (2, o.IndexOf (paramC), "#A7");
			Assert.AreEqual (3, o.IndexOf (paramD), "#A8");
			Assert.AreEqual (4, o.IndexOf (paramE), "#A9");
			Assert.AreEqual (5, o.IndexOf (paramF), "#A10");
			Assert.AreEqual (-1, o.IndexOf (paramG), "#A11");
			Assert.AreEqual (-1, o.IndexOf (paramH), "#A12");

			o ["ParamI"] = paramG;
#if NET_2_0
			Assert.AreSame (paramE, o ["ParamI"], "#B1");
#else
			Assert.AreSame (paramC, o ["ParamI"], "#B1");
#endif
			Assert.AreSame (paramB, o ["Parami"], "#B2");
			Assert.AreSame (paramC, o ["paramI"], "#B3");
#if NET_2_0
			Assert.AreSame (paramD, o ["parami"], "#B4");
#else
			Assert.AreSame (paramB, o ["parami"], "#B4");
#endif
			Assert.AreSame (paramG, o ["paramG"], "#B5");
			Assert.AreEqual (-1, o.IndexOf (paramA), "#B6");
			Assert.AreEqual (1, o.IndexOf (paramB), "#B7");
			Assert.AreEqual (2, o.IndexOf (paramC), "#B8");
			Assert.AreEqual (3, o.IndexOf (paramD), "#B9");
			Assert.AreEqual (4, o.IndexOf (paramE), "#B10");
			Assert.AreEqual (5, o.IndexOf (paramF), "#B11");
			Assert.AreEqual (0, o.IndexOf (paramG), "#B12");
			Assert.AreEqual (-1, o.IndexOf (paramH), "#B13");

			o ["ParamI"] = paramH;
#if NET_2_0
			Assert.AreSame (paramC, o ["ParamI"], "#C1");
#else
			Assert.AreSame (paramE, o ["ParamI"], "#C1");
#endif
			Assert.AreSame (paramB, o ["Parami"], "#C2");
#if NET_2_0
			Assert.AreSame (paramC, o ["paramI"], "#C3");
			Assert.AreSame (paramD, o ["parami"], "#C4");
#else
			Assert.AreSame (paramE, o ["paramI"], "#C3");
			Assert.AreSame (paramB, o ["parami"], "#C4");
#endif
			Assert.AreSame (paramG, o ["paramG"], "#C5");
			Assert.AreSame (paramH, o ["paramH"], "#C6");
			Assert.AreEqual (-1, o.IndexOf (paramA), "#C6");
			Assert.AreEqual (1, o.IndexOf (paramB), "#C7");
#if NET_2_0
			Assert.AreEqual (2, o.IndexOf (paramC), "#C8");
#else
			Assert.AreEqual (-1, o.IndexOf (paramC), "#C8");
#endif
			Assert.AreEqual (3, o.IndexOf (paramD), "#C9");
#if NET_2_0
			Assert.AreEqual (-1, o.IndexOf (paramE), "#C10");
#else
			Assert.AreEqual (4, o.IndexOf (paramE), "#C10");
#endif
			Assert.AreEqual (5, o.IndexOf (paramF), "#C11");
			Assert.AreEqual (0, o.IndexOf (paramG), "#C12");
#if NET_2_0
			Assert.AreEqual (4, o.IndexOf (paramH), "#C13");
#else
			Assert.AreEqual (2, o.IndexOf (paramH), "#C13");
#endif

			o ["paramG"] = paramA;
			Assert.AreSame (paramA, o ["ParamI"], "#D1");
			Assert.AreSame (paramB, o ["Parami"], "#D2");
#if NET_2_0
			Assert.AreSame (paramC, o ["paramI"], "#D3");
			Assert.AreSame (paramD, o ["parami"], "#D4");
#else
			Assert.AreSame (paramA, o ["paramI"], "#D3");
			Assert.AreSame (paramB, o ["parami"], "#D4");
#endif
			Assert.AreSame (paramH, o ["paramH"], "#D5");
			Assert.AreEqual (0, o.IndexOf (paramA), "#D6");
			Assert.AreEqual (1, o.IndexOf (paramB), "#D7");
#if NET_2_0
			Assert.AreEqual (2, o.IndexOf (paramC), "#D8");
#else
			Assert.AreEqual (-1, o.IndexOf (paramC), "#D8");
#endif
			Assert.AreEqual (3, o.IndexOf (paramD), "#D9");
#if NET_2_0
			Assert.AreEqual (-1, o.IndexOf (paramE), "#D10");
#else
			Assert.AreEqual (4, o.IndexOf (paramE), "#D10");
#endif
			Assert.AreEqual (5, o.IndexOf (paramF), "#D11");
			Assert.AreEqual (-1, o.IndexOf (paramG), "#D12");
#if NET_2_0
			Assert.AreEqual (4, o.IndexOf (paramH), "#D13");
#else
			Assert.AreEqual (2, o.IndexOf (paramH), "#D13");
#endif

			o ["ParamI"] = paramA;
			Assert.AreSame (paramA, o ["ParamI"], "#E1");
			Assert.AreSame (paramB, o ["Parami"], "#E2");
#if NET_2_0
			Assert.AreSame (paramC, o ["paramI"], "#E3");
			Assert.AreSame (paramD, o ["parami"], "#E4");
#else
			Assert.AreSame (paramA, o ["paramI"], "#E3");
			Assert.AreSame (paramB, o ["parami"], "#E4");
#endif
			Assert.AreSame (paramH, o ["paramH"], "#E5");
			Assert.AreEqual (0, o.IndexOf (paramA), "#E6");
			Assert.AreEqual (1, o.IndexOf (paramB), "#E7");
#if NET_2_0
			Assert.AreEqual (2, o.IndexOf (paramC), "#E8");
#else
			Assert.AreEqual (-1, o.IndexOf (paramC), "#E8");
#endif
			Assert.AreEqual (3, o.IndexOf (paramD), "#E9");
#if NET_2_0
			Assert.AreEqual (-1, o.IndexOf (paramE), "#E10");
#else
			Assert.AreEqual (4, o.IndexOf (paramE), "#E10");
#endif
			Assert.AreEqual (5, o.IndexOf (paramF), "#E11");
			Assert.AreEqual (-1, o.IndexOf (paramG), "#E12");
#if NET_2_0
			Assert.AreEqual (4, o.IndexOf (paramH), "#E13");
#else
			Assert.AreEqual (2, o.IndexOf (paramH), "#E13");
#endif

			o ["paramI"] = paramC;
#if NET_2_0
			Assert.AreSame (paramA, o ["ParamI"], "#F1");
#else
			Assert.AreSame (paramC, o ["ParamI"], "#F1");
#endif
			Assert.AreSame (paramB, o ["Parami"], "#F2");
			Assert.AreSame (paramC, o ["paramI"], "#F3");
#if NET_2_0
			Assert.AreSame (paramD, o ["parami"], "#F4");
#else
			Assert.AreSame (paramB, o ["parami"], "#F4");
#endif
			Assert.AreSame (paramH, o ["paramH"], "#F5");
#if NET_2_0
			Assert.AreEqual (0, o.IndexOf (paramA), "#F6");
#else
			Assert.AreEqual (-1, o.IndexOf (paramA), "#F6");
#endif
			Assert.AreEqual (1, o.IndexOf (paramB), "#F7");
#if NET_2_0
			Assert.AreEqual (2, o.IndexOf (paramC), "#F8");
#else
			Assert.AreEqual (0, o.IndexOf (paramC), "#F8");
#endif
			Assert.AreEqual (3, o.IndexOf (paramD), "#F9");
#if NET_2_0
			Assert.AreEqual (-1, o.IndexOf (paramE), "#F10");
#else
			Assert.AreEqual (4, o.IndexOf (paramE), "#F10");
#endif
			Assert.AreEqual (5, o.IndexOf (paramF), "#F11");
			Assert.AreEqual (-1, o.IndexOf (paramG), "#F12");
#if NET_2_0
			Assert.AreEqual (4, o.IndexOf (paramH), "#F13");
#else
			Assert.AreEqual (2, o.IndexOf (paramH), "#F13");
#endif

			OracleParameterCollection o2 = new OracleParameterCollection ();
#if NET_2_0
			o2.Add (paramE);
#else
			o2.Add (paramA);
#endif
		}

		[Test] // OracleParameter this [String]
		public void Indexer2_ParameterName_NotFound ()
		{
			OracleParameter param = new OracleParameter ("ParamI", 1);
			o.Add (param);

			try {
				o ["NotFound"] = new OracleParameter ();
				Assert.Fail ("#A1");
			} catch (IndexOutOfRangeException ex) {
#if NET_2_0
				// Invalid index -1 for this OracleParameterCollection
				// with Count=1
				Assert.AreEqual (typeof (IndexOutOfRangeException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsTrue (ex.Message.IndexOf (o.GetType ().Name) != -1, "#A5");
				Assert.IsFalse (ex.Message.IndexOf ("'NotFound'") != -1, "#A6");
#else
				// Parameter 'NotFound' not found
				Assert.AreEqual (typeof (IndexOutOfRangeException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsFalse (ex.Message.IndexOf (o.GetType ().Name) != -1, "#A5");
				Assert.IsTrue (ex.Message.IndexOf ("'NotFound'") != -1, "#A6");
#endif
			}

			try {
				o [(string) null] = new OracleParameter ();
				Assert.Fail ("#B1");
			} catch (IndexOutOfRangeException ex) {
#if NET_2_0
				// Invalid index -1 for this OracleParameterCollection
				// with Count=1
				Assert.AreEqual (typeof (IndexOutOfRangeException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsTrue (ex.Message.IndexOf (o.GetType ().Name) != -1, "#B5");
				Assert.IsFalse (ex.Message.IndexOf ("''") != -1, "#B6");
#else
				// Parameter '' not found
				Assert.AreEqual (typeof (IndexOutOfRangeException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsFalse (ex.Message.IndexOf (o.GetType ().Name) != -1, "#B5");
				Assert.IsTrue (ex.Message.IndexOf ("''") != -1, "#B6");
#endif
			}

			try {
				object value = o ["NotFound"];
				Assert.Fail ("#C1:" + value);
			} catch (IndexOutOfRangeException ex) {
#if NET_2_0
				// Invalid index -1 for this OracleParameterCollection
				// with Count=1
				Assert.AreEqual (typeof (IndexOutOfRangeException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.IsTrue (ex.Message.IndexOf (o.GetType ().Name) != -1, "#C5");
				Assert.IsFalse (ex.Message.IndexOf ("'NotFound'") != -1, "#C6");
#else
				// Parameter 'NotFound' not found
				Assert.AreEqual (typeof (IndexOutOfRangeException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.IsFalse (ex.Message.IndexOf (o.GetType ().Name) != -1, "#C5");
				Assert.IsTrue (ex.Message.IndexOf ("'NotFound'") != -1, "#C6");
#endif
			}

			try {
				object value = o [(string) null];
				Assert.Fail ("#D1:" + value);
			} catch (IndexOutOfRangeException ex) {
#if NET_2_0
				// Invalid index -1 for this OracleParameterCollection
				// with Count=1
				Assert.AreEqual (typeof (IndexOutOfRangeException), ex.GetType (), "#D2");
				Assert.IsNull (ex.InnerException, "#D3");
				Assert.IsNotNull (ex.Message, "#D4");
				Assert.IsTrue (ex.Message.IndexOf (o.GetType ().Name) != -1, "#D5");
				Assert.IsFalse (ex.Message.IndexOf ("''") != -1, "#D6");
#else
				// Parameter 'NotFound' not found
				Assert.AreEqual (typeof (IndexOutOfRangeException), ex.GetType (), "#D2");
				Assert.IsNull (ex.InnerException, "#D3");
				Assert.IsNotNull (ex.Message, "#D4");
				Assert.IsFalse (ex.Message.IndexOf (o.GetType ().Name) != -1, "#D5");
				Assert.IsTrue (ex.Message.IndexOf ("''") != -1, "#D6");
#endif
			}
		}

		[Test] // OracleParameter this [String]
		public void Indexer2_Value_Null ()
		{
			OracleParameter param = new OracleParameter ("ParamI", 1);
			o.Add (param);

			try {
				o ["ParamI"] = null;
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
#if NET_2_0
				// The OracleParameterCollection only accepts
				// non-null OracleParameter type objects
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf (o.GetType ().Name) != -1, "#5");
				Assert.IsTrue (ex.Message.IndexOf (typeof (OracleParameter).Name) != -1, "#6");
				Assert.AreEqual ("value", ex.ParamName, "#7");
#else
				// Value cannot be null
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsFalse (ex.Message.IndexOf (o.GetType ().Name) != -1, "#5");
				Assert.IsFalse (ex.Message.IndexOf (typeof (OracleParameter).Name) != -1, "#6");
				Assert.AreEqual ("value", ex.ParamName, "#7");
#endif
			}
		}

		[Test] // OracleParameter this [String]
		public void Indexer2_Value_Owned ()
		{
			OracleParameter paramA = new OracleParameter ("ParamI", 1);
			OracleParameter paramB = new OracleParameter ("paramI", 2);
			OracleParameter paramC = new OracleParameter ("Parami", 3);

			o.Add (paramA);
			o.Add (paramB);
#if !NET_2_0
			o.Add (paramC);
			o ["paramI"] = paramA;
#endif

			// attempt to add same OracleParameter to collection twice
			try {
#if NET_2_0
				o ["paramI"] = paramA;
#else
				o ["Parami"] = paramA;
#endif
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// The OracleParameter is already contained by
				// another OracleParameterCollection
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsTrue (ex.Message.IndexOf (o.GetType ().Name) != -1, "#A5");
				Assert.IsTrue (ex.Message.IndexOf (typeof (OracleParameter).Name) != -1, "#A6");
				Assert.IsNull (ex.ParamName, "#A7");
			}

			OracleParameterCollection o2 = new OracleParameterCollection ();
			o2.Add (paramC);

			// attempt to add OracleParameter to another collection
			try {
				o2 ["Parami"] = paramA;
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// The OracleParameter is already contained by
				// another OracleParameterCollection
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsTrue (ex.Message.IndexOf (o.GetType ().Name) != -1, "#B5");
				Assert.IsTrue (ex.Message.IndexOf (typeof (OracleParameter).Name) != -1, "#B6");
				Assert.IsNull (ex.ParamName, "#B7");
			}

			o.Remove (paramA);
			o ["paramI"] = paramA;
			o.Remove (paramA);
			o2 ["Parami"] = paramA;
		}

		[Test] // IndexOf (Object)
		public void IndexOf1 ()
		{
			OracleParameter paramA = new OracleParameter ("ParamI", 1);
			OracleParameter paramB = new OracleParameter ("Parami", 2);
			OracleParameter paramC = new OracleParameter ("paramI", 3);
			OracleParameter paramD = new OracleParameter ("parami", 4);
			OracleParameter paramE = new OracleParameter ("ParamI", 1);
			OracleParameter paramF = new OracleParameter ("Parami", 2);

			o.Add (paramA);
			o.Add (paramB);

			Assert.AreEqual (1, o.IndexOf ((object) paramB), "#A1");
			Assert.AreEqual (0, o.IndexOf ((object) paramA), "#A2");
			Assert.AreEqual (-1, o.IndexOf ((object) paramD), "#A3");
			Assert.AreEqual (-1, o.IndexOf ((object) paramC), "#A4");
			Assert.AreEqual (-1, o.IndexOf ((object) paramF), "#A5");
			Assert.AreEqual (-1, o.IndexOf ((object) paramE), "#A6");
			Assert.AreEqual (-1, o.IndexOf ((object) null), "#A7");

			o.Add (paramC);
			o.Add (paramD);

			Assert.AreEqual (1, o.IndexOf ((object) paramB), "#B1");
			Assert.AreEqual (0, o.IndexOf ((object) paramA), "#B2");
			Assert.AreEqual (3, o.IndexOf ((object) paramD), "#B3");
			Assert.AreEqual (2, o.IndexOf ((object) paramC), "#B4");
			Assert.AreEqual (-1, o.IndexOf ((object) paramF), "#B5");
			Assert.AreEqual (-1, o.IndexOf ((object) paramE), "#B6");
			Assert.AreEqual (-1, o.IndexOf ((object) null), "#B7");
		}

		[Test] // IndexOf (Object)
		public void IndexOf1_Value_InvalidType ()
		{
			try {
				o.IndexOf ((object) "ParamI");
				Assert.Fail ("#A1");
			} catch (InvalidCastException ex) {
#if NET_2_0
				// The OracleParameterCollection only accepts
				// non-null OracleParameter type objects, not
				// String objects
				Assert.AreEqual (typeof (InvalidCastException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsTrue (ex.Message.IndexOf (o.GetType ().Name) != -1, "#A5");
				Assert.IsTrue (ex.Message.IndexOf (typeof (string).Name) != -1, "#A6");
				Assert.IsFalse (ex.Message.IndexOf (typeof (int).Name) != -1, "#A7");
				Assert.IsTrue (ex.Message.IndexOf (typeof (OracleParameter).Name) != -1, "#A8");
#else
				// argument value must be of type System.Data.OracleClient.OracleParameter
				Assert.AreEqual (typeof (InvalidCastException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsFalse (ex.Message.IndexOf (o.GetType ().Name) != -1, "#A5");
				Assert.IsFalse (ex.Message.IndexOf (typeof (string).Name) != -1, "#A6");
				Assert.IsFalse (ex.Message.IndexOf (typeof (int).Name) != -1, "#A7");
				Assert.IsTrue (ex.Message.IndexOf (typeof (OracleParameter).Name) != -1, "#A8");
#endif
			}

			try {
				o.IndexOf ((object) 5);
				Assert.Fail ("#B1");
			} catch (InvalidCastException ex) {
#if NET_2_0
				// The OracleParameterCollection only accepts
				// non-null OracleParameter type objects, not
				// Int32 objects
				Assert.AreEqual (typeof (InvalidCastException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsTrue (ex.Message.IndexOf (o.GetType ().Name) != -1, "#B5");
				Assert.IsFalse (ex.Message.IndexOf (typeof (string).Name) != -1, "#B6");
				Assert.IsTrue (ex.Message.IndexOf (typeof (int).Name) != -1, "#B7");
				Assert.IsTrue (ex.Message.IndexOf (typeof (OracleParameter).Name) != -1, "#B8");
#else
				// argument value must be of type System.Data.OracleClient.OracleParameter
				Assert.AreEqual (typeof (InvalidCastException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsFalse (ex.Message.IndexOf (o.GetType ().Name) != -1, "#B5");
				Assert.IsFalse (ex.Message.IndexOf (typeof (string).Name) != -1, "#B6");
				Assert.IsFalse (ex.Message.IndexOf (typeof (int).Name) != -1, "#B7");
				Assert.IsTrue (ex.Message.IndexOf (typeof (OracleParameter).Name) != -1, "#B8");
#endif
			}
		}

		[Test] // IndexOf (OracleParameter)
		public void IndexOf2 ()
		{
			OracleParameter paramA = new OracleParameter ("ParamI", 1);
			OracleParameter paramB = new OracleParameter ("Parami", 2);
			OracleParameter paramC = new OracleParameter ("paramI", 3);
			OracleParameter paramD = new OracleParameter ("parami", 4);
			OracleParameter paramE = new OracleParameter ("ParamI", 1);
			OracleParameter paramF = new OracleParameter ("Parami", 2);

			o.Add (paramA);
			o.Add (paramB);

			Assert.AreEqual (1, o.IndexOf (paramB), "#A1");
			Assert.AreEqual (0, o.IndexOf (paramA), "#A2");
			Assert.AreEqual (-1, o.IndexOf (paramD), "#A3");
			Assert.AreEqual (-1, o.IndexOf (paramC), "#A4");
			Assert.AreEqual (-1, o.IndexOf (paramF), "#A5");
			Assert.AreEqual (-1, o.IndexOf (paramE), "#A6");
			Assert.AreEqual (-1, o.IndexOf (null), "#A7");

			o.Add (paramC);
			o.Add (paramD);

			Assert.AreEqual (1, o.IndexOf (paramB), "#B1");
			Assert.AreEqual (0, o.IndexOf (paramA), "#B2");
			Assert.AreEqual (3, o.IndexOf (paramD), "#B3");
			Assert.AreEqual (2, o.IndexOf (paramC), "#B4");
			Assert.AreEqual (-1, o.IndexOf (paramF), "#B5");
			Assert.AreEqual (-1, o.IndexOf (paramE), "#B6");
			Assert.AreEqual (-1, o.IndexOf (null), "#B7");
		}

		[Test] // IndexOf (String)
		public void IndexOf3 ()
		{
			OracleParameter paramA = new OracleParameter ("ParamI", 5);
			OracleParameter paramB = new OracleParameter ("Parami", 4);

			o.Add (paramA);
			o.Add (paramB);

			Assert.AreEqual (1, o.IndexOf ("Parami"), "#1");
			Assert.AreEqual (0, o.IndexOf ("ParamI"), "#2");
			Assert.AreEqual (0, o.IndexOf ("paramI"), "#3");
			Assert.AreEqual (1, o.IndexOf ("parami"), "#4");
			Assert.AreEqual (-1, o.IndexOf ("NotFound"), "#5");
			Assert.AreEqual (-1, o.IndexOf (string.Empty), "#6");
			Assert.AreEqual (-1, o.IndexOf ((string) null), "#7");
		}

		[Test] // Insert (Int32, Object)
		public void Insert1_Value_InvalidType ()
		{
			OracleParameter param = new OracleParameter ("ParamI", 1);

			o.Insert (0, (object) param);

			try {
				o.Insert (0, (object) "ParamI");
				Assert.Fail ("#A1");
			} catch (InvalidCastException ex) {
#if NET_2_0
				// The OracleParameterCollection only accepts
				// non-null OracleParameter type objects, not
				// String objects
				Assert.AreEqual (typeof (InvalidCastException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsTrue (ex.Message.IndexOf (o.GetType ().Name) != -1, "#A5");
				Assert.IsTrue (ex.Message.IndexOf (typeof (string).Name) != -1, "#A6");
				Assert.IsFalse (ex.Message.IndexOf (typeof (int).Name) != -1, "#A7");
				Assert.IsTrue (ex.Message.IndexOf (typeof (OracleParameter).Name) != -1, "#A8");
#else
				// argument value must be of type System.Data.OracleClient.OracleParameter
				Assert.AreEqual (typeof (InvalidCastException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsFalse (ex.Message.IndexOf (o.GetType ().Name) != -1, "#A5");
				Assert.IsFalse (ex.Message.IndexOf (typeof (string).Name) != -1, "#A6");
				Assert.IsFalse (ex.Message.IndexOf (typeof (int).Name) != -1, "#A7");
				Assert.IsTrue (ex.Message.IndexOf (typeof (OracleParameter).Name) != -1, "#A8");
#endif
			}

			try {
				o.Insert (0, (object) 5);
				Assert.Fail ("#B1");
			} catch (InvalidCastException ex) {
#if NET_2_0
				// The OracleParameterCollection only accepts
				// non-null OracleParameter type objects, not
				// Int32 objects
				Assert.AreEqual (typeof (InvalidCastException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsTrue (ex.Message.IndexOf (o.GetType ().Name) != -1, "#B5");
				Assert.IsFalse (ex.Message.IndexOf (typeof (string).Name) != -1, "#B6");
				Assert.IsTrue (ex.Message.IndexOf (typeof (int).Name) != -1, "#B7");
				Assert.IsTrue (ex.Message.IndexOf (typeof (OracleParameter).Name) != -1, "#B8");
#else
				// argument value must be of type System.Data.OracleClient.OracleParameter
				Assert.AreEqual (typeof (InvalidCastException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsFalse (ex.Message.IndexOf (o.GetType ().Name) != -1, "#B5");
				Assert.IsFalse (ex.Message.IndexOf (typeof (string).Name) != -1, "#B6");
				Assert.IsFalse (ex.Message.IndexOf (typeof (int).Name) != -1, "#B7");
				Assert.IsTrue (ex.Message.IndexOf (typeof (OracleParameter).Name) != -1, "#B8");
#endif
			}
		}

		[Test] // Insert (Int32, Object)
		public void Insert1_Value_Null ()
		{
			try {
				o.Insert (0, (object) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
#if NET_2_0
				// The OracleParameterCollection only accepts
				// non-null OracleParameter type objects
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf (o.GetType ().Name) != -1, "#5");
				Assert.IsTrue (ex.Message.IndexOf (typeof (OracleParameter).Name) != -1, "#6");
				Assert.AreEqual ("value", ex.ParamName, "#7");
#else
				// Value cannot be null
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsFalse (ex.Message.IndexOf (o.GetType ().Name) != -1, "#5");
				Assert.IsFalse (ex.Message.IndexOf (typeof (OracleParameter).Name) != -1, "#6");
				Assert.AreEqual ("value", ex.ParamName, "#7");
#endif
			}
		}

		[Test] // Insert (Int32, Object)
		public void Insert1_Value_Owned ()
		{
			OracleParameter param = new OracleParameter ("ParamI", 1);

			o.Insert (0, (object) param);

			// attempt to add same OracleParameter to collection twice
			try {
				o.Insert (1, (object) param);
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// The OracleParameter is already contained by
				// another OracleParameterCollection
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsTrue (ex.Message.IndexOf (o.GetType ().Name) != -1, "#A5");
				Assert.IsTrue (ex.Message.IndexOf (typeof (OracleParameter).Name) != -1, "#A6");
				Assert.IsNull (ex.ParamName, "#A7");
			}

			OracleParameterCollection o2 = new OracleParameterCollection ();

			// attempt to add OracleParameter to another collection
			try {
				o2.Insert (0, (object) param);
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// The OracleParameter is already contained by
				// another OracleParameterCollection
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsTrue (ex.Message.IndexOf (o.GetType ().Name) != -1, "#B5");
				Assert.IsTrue (ex.Message.IndexOf (typeof (OracleParameter).Name) != -1, "#B6");
				Assert.IsNull (ex.ParamName, "#B7");
			}

			o.Remove (param);
			o.Insert (0, (object) param);
			o.Remove (param);
			o2.Insert (0, (object) param);
		}

		[Test] // Insert (Int32, OracleParameter)
		public void Insert2_Value_Null ()
		{
			try {
				o.Insert (0, (OracleParameter) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
#if NET_2_0
				// The OracleParameterCollection only accepts
				// non-null OracleParameter type objects
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf (o.GetType ().Name) != -1, "#5");
				Assert.IsTrue (ex.Message.IndexOf (typeof (OracleParameter).Name) != -1, "#6");
				Assert.AreEqual ("value", ex.ParamName, "#7");
#else
				// Value cannot be null
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsFalse (ex.Message.IndexOf (o.GetType ().Name) != -1, "#5");
				Assert.IsFalse (ex.Message.IndexOf (typeof (OracleParameter).Name) != -1, "#6");
				Assert.AreEqual ("value", ex.ParamName, "#7");
#endif
			}
		}

		[Test] // Insert (Int32, OracleParameter)
		public void Insert2_Value_Owned ()
		{
			OracleParameter param = new OracleParameter ("ParamI", 1);

			o.Insert (0, param);

			// attempt to add same OracleParameter to collection twice
			try {
				o.Insert (1, param);
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// The OracleParameter is already contained by
				// another OracleParameterCollection
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsTrue (ex.Message.IndexOf (o.GetType ().Name) != -1, "#A5");
				Assert.IsTrue (ex.Message.IndexOf (typeof (OracleParameter).Name) != -1, "#A6");
				Assert.IsNull (ex.ParamName, "#A7");
			}

			OracleParameterCollection o2 = new OracleParameterCollection ();

			// attempt to add OracleParameter to another collection
			try {
				o2.Insert (0, param);
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// The OracleParameter is already contained by
				// another OracleParameterCollection
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsTrue (ex.Message.IndexOf (o.GetType ().Name) != -1, "#B5");
				Assert.IsTrue (ex.Message.IndexOf (typeof (OracleParameter).Name) != -1, "#B6");
				Assert.IsNull (ex.ParamName, "#B7");
			}

			o.Remove (param);
			o.Insert (0, param);
			o.Remove (param);
			o2.Insert (0, param);
		}

		[Test] // Remove (Object)
		public void Remove1 ()
		{
			OracleParameter paramA = new OracleParameter ("ParamI", 1);
			OracleParameter paramB = new OracleParameter ("Parami", 2);
			OracleParameter paramC = new OracleParameter ("paramI", 3);

			o.Add (paramA);
			o.Add (paramB);
			o.Remove ((object) paramA);

			Assert.AreEqual (-1, o.IndexOf (paramA), "#A1");
			Assert.AreEqual (0, o.IndexOf (paramB), "#A2");

			o.Add (paramA);

			Assert.AreEqual (1, o.IndexOf (paramA), "#B1");
			Assert.AreEqual (0, o.IndexOf (paramB), "#B2");

			o.Remove ((object) paramB);

			Assert.AreEqual (0, o.IndexOf (paramA), "#C1");
			Assert.AreEqual (-1, o.IndexOf (paramB), "#C2");

			OracleParameterCollection o2 = new OracleParameterCollection ();
			o2.Add (paramB);
		}

		[Test] // Remove (Object)
		public void Remove1_Value_InvalidType ()
		{
			try {
				o.Remove ((object) "ParamI");
				Assert.Fail ("#A1");
			} catch (InvalidCastException ex) {
#if NET_2_0
				// The OracleParameterCollection only accepts
				// non-null OracleParameter type objects, not
				// String objects
				Assert.AreEqual (typeof (InvalidCastException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsTrue (ex.Message.IndexOf (o.GetType ().Name) != -1, "#A5");
				Assert.IsTrue (ex.Message.IndexOf (typeof (string).Name) != -1, "#A6");
				Assert.IsFalse (ex.Message.IndexOf (typeof (int).Name) != -1, "#A7");
				Assert.IsTrue (ex.Message.IndexOf (typeof (OracleParameter).Name) != -1, "#A8");
#else
				// argument value must be of type System.Data.OracleClient.OracleParameter
				Assert.AreEqual (typeof (InvalidCastException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsFalse (ex.Message.IndexOf (o.GetType ().Name) != -1, "#A5");
				Assert.IsFalse (ex.Message.IndexOf (typeof (string).Name) != -1, "#A6");
				Assert.IsFalse (ex.Message.IndexOf (typeof (int).Name) != -1, "#A7");
				Assert.IsTrue (ex.Message.IndexOf (typeof (OracleParameter).Name) != -1, "#A8");
#endif
			}

			try {
				o.Remove ((object) 5);
				Assert.Fail ("#B1");
			} catch (InvalidCastException ex) {
#if NET_2_0
				// The OracleParameterCollection only accepts
				// non-null OracleParameter type objects, not
				// Int32 objects
				Assert.AreEqual (typeof (InvalidCastException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsTrue (ex.Message.IndexOf (o.GetType ().Name) != -1, "#B5");
				Assert.IsFalse (ex.Message.IndexOf (typeof (string).Name) != -1, "#B6");
				Assert.IsTrue (ex.Message.IndexOf (typeof (int).Name) != -1, "#B7");
				Assert.IsTrue (ex.Message.IndexOf (typeof (OracleParameter).Name) != -1, "#B8");
#else
				// argument value must be of type System.Data.OracleClient.OracleParameter
				Assert.AreEqual (typeof (InvalidCastException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsFalse (ex.Message.IndexOf (o.GetType ().Name) != -1, "#B5");
				Assert.IsFalse (ex.Message.IndexOf (typeof (string).Name) != -1, "#B6");
				Assert.IsFalse (ex.Message.IndexOf (typeof (int).Name) != -1, "#B7");
				Assert.IsTrue (ex.Message.IndexOf (typeof (OracleParameter).Name) != -1, "#B8");
#endif
			}
		}

		[Test] // Remove (Object)
		public void Remove1_Value_NotOwned ()
		{
			OracleParameter paramA = new OracleParameter ("ParamI", 1);

			try {
				o.Remove ((object) paramA);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
#if NET_2_0
				// Attempted to remove an OracleParameter that
				// is not contained by this OracleParameterCollection
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsTrue (ex.Message.IndexOf (o.GetType ().Name) != -1, "#B5");
				Assert.IsTrue (ex.Message.IndexOf (typeof (OracleParameter).Name) != -1, "#B6");
				Assert.IsNull (ex.ParamName, "#B7");
#else
				// Parameter not found
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNull (ex.ParamName, "#B5");
#endif
			}
		}

		[Test] // Remove (Object)
		public void Remove1_Value_Null ()
		{
			try {
				o.Remove ((object) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
#if NET_2_0
				// The OracleParameterCollection only accepts
				// non-null OracleParameter type objects
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf (o.GetType ().Name) != -1, "#5");
				Assert.IsTrue (ex.Message.IndexOf (typeof (OracleParameter).Name) != -1, "#6");
				Assert.AreEqual ("value", ex.ParamName, "#7");
#else
				// Value cannot be null
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsFalse (ex.Message.IndexOf (o.GetType ().Name) != -1, "#5");
				Assert.IsFalse (ex.Message.IndexOf (typeof (OracleParameter).Name) != -1, "#6");
				Assert.AreEqual ("value", ex.ParamName, "#7");
#endif
			}
		}

		[Test] // Remove (OracleParameter)
		public void Remove2 ()
		{
			OracleParameter paramA = new OracleParameter ("ParamI", 1);
			OracleParameter paramB = new OracleParameter ("Parami", 2);
			OracleParameter paramC = new OracleParameter ("paramI", 3);

			o.Add (paramA);
			o.Add (paramB);
			o.Remove (paramA);

			Assert.AreEqual (-1, o.IndexOf (paramA), "#A1");
			Assert.AreEqual (0, o.IndexOf (paramB), "#A2");

			o.Add (paramA);

			Assert.AreEqual (1, o.IndexOf (paramA), "#B1");
			Assert.AreEqual (0, o.IndexOf (paramB), "#B2");

			o.Remove (paramB);

			Assert.AreEqual (0, o.IndexOf (paramA), "#C1");
			Assert.AreEqual (-1, o.IndexOf (paramB), "#C2");

			OracleParameterCollection o2 = new OracleParameterCollection ();
			o2.Add (paramB);
		}

		[Test] // Remove (OracleParameter)
		public void Remove2_Value_NotOwned ()
		{
			OracleParameter paramA = new OracleParameter ("ParamI", 1);

			try {
				o.Remove (paramA);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
#if NET_2_0
				// Attempted to remove an OracleParameter that
				// is not contained by this OracleParameterCollection
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf (o.GetType ().Name) != -1, "#5");
				Assert.IsTrue (ex.Message.IndexOf (typeof (OracleParameter).Name) != -1, "#6");
				Assert.IsNull (ex.ParamName, "#7");
#else
				// Parameter not found
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
#endif
			}
		}

		[Test] // Remove (OracleParameter)
		public void Remove2_Value_Null ()
		{
			try {
				o.Remove ((OracleParameter) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
#if NET_2_0
				// The OracleParameterCollection only accepts
				// non-null OracleParameter type objects
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf (o.GetType ().Name) != -1, "#5");
				Assert.IsTrue (ex.Message.IndexOf (typeof (OracleParameter).Name) != -1, "#6");
				Assert.AreEqual ("value", ex.ParamName, "#7");
#else
				// Value cannot be null
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsFalse (ex.Message.IndexOf (o.GetType ().Name) != -1, "#5");
				Assert.IsFalse (ex.Message.IndexOf (typeof (OracleParameter).Name) != -1, "#6");
				Assert.AreEqual ("value", ex.ParamName, "#7");
#endif
			}
		}

		[Test] // RemoveAt (Int32)
		public void RemoveAt1 ()
		{
			OracleParameter paramA = new OracleParameter ("ParamI", 1);
			OracleParameter paramB = new OracleParameter ("Parami", 2);
			OracleParameter paramC = new OracleParameter ("paramI", 3);

			o.Add (paramA);
			o.Add (paramB);
			o.Add (paramC);

			o.RemoveAt (2);

			Assert.AreEqual (0, o.IndexOf (paramA), "#A1");
			Assert.AreEqual (1, o.IndexOf (paramB), "#A2");
			Assert.AreEqual (-1, o.IndexOf (paramC), "#A3");

			o.RemoveAt (0);

			Assert.AreEqual (-1, o.IndexOf (paramA), "#B1");
			Assert.AreEqual (0, o.IndexOf (paramB), "#B2");
			Assert.AreEqual (-1, o.IndexOf (paramC), "#B3");

			o.RemoveAt (0);

			Assert.AreEqual (-1, o.IndexOf (paramA), "#C1");
			Assert.AreEqual (-1, o.IndexOf (paramB), "#C2");
			Assert.AreEqual (-1, o.IndexOf (paramC), "#C3");

			o.Add (paramA);
			o.Add (paramC);

			OracleParameterCollection o2 = new OracleParameterCollection ();
			o2.Add (paramB);
		}

		[Test] // RemoveAt (Int32)
		public void RemoveAt1_Index_Invalid ()
		{
			try {
				o.RemoveAt (0);
				Assert.Fail ("#A1");
			} catch (IndexOutOfRangeException ex) {
#if NET_2_0
				// Invalid index 0 for this OracleParameterCollection
				// with Count=0
				Assert.AreEqual (typeof (IndexOutOfRangeException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsTrue (ex.Message.IndexOf (o.GetType ().Name) != -1, "#A5");
#else
				// Index 0 outside the scope of the parameter array
				Assert.AreEqual (typeof (IndexOutOfRangeException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
#endif
			}

			try {
				o.RemoveAt (-1);
				Assert.Fail ("#B1");
			} catch (IndexOutOfRangeException ex) {
#if NET_2_0
				// Invalid index -1 for this OracleParameterCollection
				// with Count=0
				Assert.AreEqual (typeof (IndexOutOfRangeException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsTrue (ex.Message.IndexOf (o.GetType ().Name) != -1, "#B5");
#else
				// Index -1 outside the scope of the parameter array
				Assert.AreEqual (typeof (IndexOutOfRangeException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
#endif
			}
		}

		[Test] // RemoveAt (String)
		public void RemoveAt2 ()
		{
			OracleParameter paramA = new OracleParameter ("ParamI", 1);
			OracleParameter paramB = new OracleParameter ("Parami", 2);
			OracleParameter paramC = new OracleParameter ("paramI", 3);
			OracleParameter paramD = new OracleParameter ("parami", 4);
			OracleParameter paramE = new OracleParameter ("parami", 5);

			o.Add (paramA);
			o.Add (paramB);
			o.Add (paramC);
			o.Add (paramD);
			o.Add (paramE);

			o.RemoveAt ("paramI");

#if NET_2_0
			Assert.AreEqual (0, o.IndexOf (paramA), "#A1");
			Assert.AreEqual (1, o.IndexOf (paramB), "#A2");
			Assert.AreEqual (-1, o.IndexOf (paramC), "#A3");
#else
			Assert.AreEqual (-1, o.IndexOf (paramA), "#A1");
			Assert.AreEqual (0, o.IndexOf (paramB), "#A2");
			Assert.AreEqual (1, o.IndexOf (paramC), "#A3");
#endif
			Assert.AreEqual (2, o.IndexOf (paramD), "#A4");
			Assert.AreEqual (3, o.IndexOf (paramE), "#A5");

			o.RemoveAt ("parami");

#if NET_2_0
			Assert.AreEqual (0, o.IndexOf (paramA), "#B1");
			Assert.AreEqual (1, o.IndexOf (paramB), "#B2");
			Assert.AreEqual (-1, o.IndexOf (paramC), "#B3");
			Assert.AreEqual (-1, o.IndexOf (paramD), "#B4");
#else
			Assert.AreEqual (-1, o.IndexOf (paramA), "#B1");
			Assert.AreEqual (-1, o.IndexOf (paramB), "#B2");
			Assert.AreEqual (0, o.IndexOf (paramC), "#B3");
			Assert.AreEqual (1, o.IndexOf (paramD), "#B4");
#endif
			Assert.AreEqual (2, o.IndexOf (paramE), "#B5");

			o.RemoveAt ("Parami");

#if NET_2_0
			Assert.AreEqual (0, o.IndexOf (paramA), "#C1");
#else
			Assert.AreEqual (-1, o.IndexOf (paramA), "#C1");
#endif
			Assert.AreEqual (-1, o.IndexOf (paramB), "#C2");
#if NET_2_0
			Assert.AreEqual (-1, o.IndexOf (paramC), "#C3");
#else
			Assert.AreEqual (0, o.IndexOf (paramC), "#C3");
#endif
			Assert.AreEqual (-1, o.IndexOf (paramD), "#C4");
			Assert.AreEqual (1, o.IndexOf (paramE), "#C5");

			o.RemoveAt ("Parami");

#if NET_2_0
			Assert.AreEqual (0, o.IndexOf (paramA), "#D1");
#else
			Assert.AreEqual (-1, o.IndexOf (paramA), "#D1");
#endif
			Assert.AreEqual (-1, o.IndexOf (paramB), "#D2");
#if NET_2_0
			Assert.AreEqual (-1, o.IndexOf (paramC), "#D3");
#else
			Assert.AreEqual (0, o.IndexOf (paramC), "#D3");
#endif
			Assert.AreEqual (-1, o.IndexOf (paramD), "#D4");
			Assert.AreEqual (-1, o.IndexOf (paramE), "#D5");

			o.RemoveAt ("ParamI");

			Assert.AreEqual (-1, o.IndexOf (paramA), "#E1");
			Assert.AreEqual (-1, o.IndexOf (paramB), "#E2");
			Assert.AreEqual (-1, o.IndexOf (paramC), "#E3");
			Assert.AreEqual (-1, o.IndexOf (paramD), "#E4");
			Assert.AreEqual (-1, o.IndexOf (paramE), "#E5");

			o.Add (paramA);
			o.Add (paramB);
			o.Add (paramE);

			OracleParameterCollection o2 = new OracleParameterCollection ();
			o2.Add (paramC);
			o2.Add (paramD);
		}

		[Test] // RemoveAt (String)
		public void RemoveAt2_ParameterName_NotFound ()
		{
			o.Add (new OracleParameter ((string) null, 1));
			o.Add (new OracleParameter (string.Empty, 1));

			try {
				o.RemoveAt ("NotFound");
				Assert.Fail ("#A1");
			} catch (IndexOutOfRangeException ex) {
#if NET_2_0
				// An OracleParameter with ParameterName 'NotFound'
				// is not contained by this OracleParameterCollection
				Assert.AreEqual (typeof (IndexOutOfRangeException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsTrue (ex.Message.IndexOf (typeof (OracleParameter).Name) != -1, "#A5");
				Assert.IsTrue (ex.Message.IndexOf (o.GetType ().Name) != -1, "#A6");
				Assert.IsTrue (ex.Message.IndexOf ("'NotFound'") != -1, "#A7");
#else
				// Parameter 'NotFound' not found
				Assert.AreEqual (typeof (IndexOutOfRangeException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsFalse (ex.Message.IndexOf (typeof (OracleParameter).Name) != -1, "#A5");
				Assert.IsFalse (ex.Message.IndexOf (o.GetType ().Name) != -1, "#A6");
				Assert.IsTrue (ex.Message.IndexOf ("'NotFound'") != -1, "#A7");
#endif
			}

			try {
				o.RemoveAt ((string) null);
				Assert.Fail ("#B1");
			} catch (IndexOutOfRangeException ex) {
#if NET_2_0
				// An OracleParameter with ParameterName '' is
				// not contained by this OracleParameterCollection
				Assert.AreEqual (typeof (IndexOutOfRangeException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsTrue (ex.Message.IndexOf (typeof (OracleParameter).Name) != -1, "#B5");
				Assert.IsTrue (ex.Message.IndexOf (o.GetType ().Name) != -1, "#B6");
				Assert.IsTrue (ex.Message.IndexOf ("''") != -1, "#B7");
#else
				// Parameter '' not found
				Assert.AreEqual (typeof (IndexOutOfRangeException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsFalse (ex.Message.IndexOf (typeof (OracleParameter).Name) != -1, "#B5");
				Assert.IsFalse (ex.Message.IndexOf (o.GetType ().Name) != -1, "#B6");
				Assert.IsTrue (ex.Message.IndexOf ("''") != -1, "#B7");
#endif
			}
		}
	}
}
