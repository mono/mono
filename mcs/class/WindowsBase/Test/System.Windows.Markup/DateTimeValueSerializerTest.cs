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
// Copyright (c) 2007 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Converters;
using System.Windows.Markup;
using NUnit.Framework;

namespace MonoTests.System.Windows.Markup {

	[TestFixture]
	public class DateTimeValueSerializerTest
	{
		class Context : IValueSerializerContext {
			public ValueSerializer GetValueSerializerFor (PropertyDescriptor descriptor)
			{
				return null;
			}

			public ValueSerializer GetValueSerializerFor (Type type)
			{
				return null;
			}

			public void OnComponentChanged ()
			{
			}

			public bool OnComponentChanging ()
			{
				return false;
			}

			public IContainer Container {
				get { return null; }
			}

			public object Instance {
				get { return null; }
			}

			public PropertyDescriptor PropertyDescriptor {
				get { return null; }
			}

			public object GetService (Type serviceType)
			{
				return null;
			}
		}

		[Test]
		public void CanConvertFromStringNullContext ()
		{
			ValueSerializer serializer = (ValueSerializer)ValueSerializer.GetSerializerFor (typeof (DateTime));
			Assert.IsTrue (serializer.CanConvertFromString (null, null));
		}

		[Test]
		public void CanConvertToStringNullContext ()
		{
			ValueSerializer serializer = (ValueSerializer)ValueSerializer.GetSerializerFor (typeof (DateTime));
			Assert.IsTrue (serializer.CanConvertToString (DateTime.Now, null));
		}

		[Test]
#if NET_4_0
		[NUnit.Framework.CategoryAttribute ("NotWorking")]
		// Since ValueSerializer has moved to System.Xaml.dll while the type
		// this test expects is in WindowsBase, there should be some additional
		// support code in this assembly. Until someone does that job, this
		// test won't pass.
#endif
		public void CanConvertFromString ()
		{
			Context context = new Context ();
			ValueSerializer serializer = (ValueSerializer)ValueSerializer.GetSerializerFor (typeof (DateTime), context);

			Assert.AreEqual ("DateTimeValueSerializer", serializer.GetType().Name, "1");

			Assert.IsTrue (serializer.CanConvertFromString (null, context), "2");
			Assert.IsTrue (serializer.CanConvertFromString ("2008-01-18T10:39:27.106426-08:00", context), "3");
			Assert.IsTrue (serializer.CanConvertFromString ("2008-01-.106426-08:00", context), "4");
		}

#if NET_4_0
		[NUnit.Framework.CategoryAttribute ("NotWorking")]
		// Since ValueSerializer has moved to System.Xaml.dll while the type
		// this test expects is in WindowsBase, there should be some additional
		// support code in this assembly. Until someone does that job, this
		// test won't pass.
#endif
		[Test]
		[ExpectedException (typeof (ArgumentException))] // Expected object of type 'DateTime'.
		public void CanConvertToString1 ()
		{
			ValueSerializer serializer = (ValueSerializer)ValueSerializer.GetSerializerFor (typeof (DateTime));
			Context context = new Context ();

			serializer.CanConvertToString (null, context);
		}
		[Test]
		public void CanConvertToString2 ()
		{
			ValueSerializer serializer = (ValueSerializer)ValueSerializer.GetSerializerFor (typeof (DateTime));
			Context context = new Context ();

			DateTime dt = DateTime.Parse ("2008-01-18T10:39:27.106426-08:00");

			Assert.IsTrue (serializer.CanConvertToString (dt, context), "6");
		}

		[Test]
		public void ConvertFromString1 ()
		{
			ValueSerializer serializer = (ValueSerializer)ValueSerializer.GetSerializerFor (typeof (DateTime));
			Context context = new Context ();

			DateTime dt = DateTime.Parse ("2008-01-18T10:39:27.106426-08:00");

			Assert.AreEqual (dt, serializer.ConvertFromString ("2008-01-18T10:39:27.106426-08:00", context), "2");
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))] // 'DateTimeValueSerializer' ValueSerializer cannot convert from '(null)'
		public void ConvertFromString2 ()
		{
			ValueSerializer serializer = (ValueSerializer)ValueSerializer.GetSerializerFor (typeof (DateTime));
			Context context = new Context ();

			serializer.ConvertFromString (null, context);
		}

		[Test]
		[ExpectedException (typeof (FormatException))] // String was not recognized as a valid DateTime.
		public void ConvertFromString3 ()
		{
			ValueSerializer serializer = (ValueSerializer)ValueSerializer.GetSerializerFor (typeof (DateTime));
			Context context = new Context ();

			serializer.ConvertFromString ("2008-01-.106426-08:00", context);
		}

		[Test]
		[NUnit.Framework.CategoryAttribute ("NotWorking")]
		public void ConvertToString1 ()
		{
			ValueSerializer serializer = (ValueSerializer)ValueSerializer.GetSerializerFor (typeof (DateTime));
			Context context = new Context ();

			DateTime dt = DateTime.Parse ("2008-01-18T10:39:27.106426-08:00");

			Assert.AreEqual ("2008-01-18T10:39:27.106426-08:00", serializer.ConvertToString (dt, context), "1");
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertToString2 ()
		{
			ValueSerializer serializer = (ValueSerializer)ValueSerializer.GetSerializerFor (typeof (DateTime));
			Context context = new Context ();

			serializer.ConvertToString (null, context);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertToString3 ()
		{
			ValueSerializer serializer = (ValueSerializer)ValueSerializer.GetSerializerFor (typeof (DateTime));
			Context context = new Context ();

			serializer.ConvertToString (1, context);
		}
	}

}
