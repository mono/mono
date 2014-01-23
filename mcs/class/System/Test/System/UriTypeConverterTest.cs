//
// UriTypeConverterTest.cs - Unit tests for System.UriTypeConverter
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
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

#if NET_2_0

using NUnit.Framework;

using System;
using System.IO;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;

namespace MonoTests.System {

	class UriTypeDescriptorContext : ITypeDescriptorContext {

		public IContainer Container {
			get { throw new NotImplementedException (); }
		}

		public object Instance {
			get { throw new NotImplementedException (); }
		}

		public void OnComponentChanged ()
		{
			throw new NotImplementedException ();
		}

		public bool OnComponentChanging ()
		{
			throw new NotImplementedException ();
		}

		public PropertyDescriptor PropertyDescriptor {
			get { throw new NotImplementedException (); }
		}

		public object GetService (Type serviceType)
		{
			throw new NotImplementedException ();
		}
	}

	[TestFixture]
	public class UriTypeConverterTest {

		private const string url = "http://www.mono-project.com/";
		private static Uri uri = new Uri (url);

		private UriTypeConverter converter;
		private UriTypeDescriptorContext context;

		protected bool isWin32 = false;
		
		[SetUp]
		public void SetUp ()
		{
			converter = new UriTypeConverter ();
			context = new UriTypeDescriptorContext ();
			isWin32 = (Path.DirectorySeparatorChar == '\\');
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CanConvertFrom_Null ()
		{
			converter.CanConvertFrom (null);
		}

		[Test]
		public void CanConvertFrom ()
		{
			Assert.IsTrue (converter.CanConvertFrom (typeof (string)), "string");
			Assert.IsTrue (converter.CanConvertFrom (typeof (Uri)), "Uri");
#if MOBILE
			Assert.IsFalse (converter.CanConvertFrom (typeof (InstanceDescriptor)), "InstanceDescriptor");
#else
			Assert.IsTrue (converter.CanConvertFrom (typeof (InstanceDescriptor)), "InstanceDescriptor");
#endif
			Assert.IsFalse (converter.CanConvertFrom (typeof (object)), "object");
			Assert.IsFalse (converter.CanConvertFrom (typeof (int)), "int");
			Assert.IsFalse (converter.CanConvertFrom (typeof (char)), "char");
			Assert.IsFalse (converter.CanConvertFrom (typeof (DateTime)), "datetime");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CanConvertFrom_Null_Null ()
		{
			converter.CanConvertFrom (null, null);
		}

		[Test]
		public void CanConvertFrom_Null_Type ()
		{
			Assert.IsTrue (converter.CanConvertFrom (null, typeof (string)), "string");
			Assert.IsTrue (converter.CanConvertFrom (null, typeof (Uri)), "Uri");
#if MOBILE
			Assert.IsFalse (converter.CanConvertFrom (null, typeof (InstanceDescriptor)), "InstanceDescriptor");
#else
			Assert.IsTrue (converter.CanConvertFrom (null, typeof (InstanceDescriptor)), "InstanceDescriptor");
#endif
			Assert.IsFalse (converter.CanConvertFrom (null, typeof (object)), "object");
			Assert.IsFalse (converter.CanConvertFrom (null, typeof (int)), "int");
			Assert.IsFalse (converter.CanConvertFrom (null, typeof (char)), "char");
			Assert.IsFalse (converter.CanConvertFrom (null, typeof (DateTime)), "datetime");
		}

		[Test]
		public void CanConvertFrom_TypeDescriptorContext_Type ()
		{
			Assert.IsTrue (converter.CanConvertFrom (context, typeof (string)), "string");
			Assert.IsTrue (converter.CanConvertFrom (context, typeof (Uri)), "Uri");
#if MOBILE
			Assert.IsFalse (converter.CanConvertFrom (context, typeof (InstanceDescriptor)), "InstanceDescriptor");
#else
			Assert.IsTrue (converter.CanConvertFrom (context, typeof (InstanceDescriptor)), "InstanceDescriptor");
#endif
			Assert.IsFalse (converter.CanConvertFrom (context, typeof (object)), "object");
			Assert.IsFalse (converter.CanConvertFrom (context, typeof (int)), "int");
			Assert.IsFalse (converter.CanConvertFrom (context, typeof (char)), "char");
			Assert.IsFalse (converter.CanConvertFrom (context, typeof (DateTime)), "datetime");
		}

		[Test]
		public void CanConvertTo ()
		{
			Assert.IsFalse (converter.CanConvertTo (null), "null");

			Assert.IsTrue (converter.CanConvertTo (typeof (string)), "string");
			Assert.IsTrue (converter.CanConvertTo (typeof (Uri)), "Uri");
#if MOBILE
			Assert.IsFalse (converter.CanConvertTo (typeof (InstanceDescriptor)), "InstanceDescriptor");
#else
			Assert.IsTrue (converter.CanConvertTo (typeof (InstanceDescriptor)), "InstanceDescriptor");
#endif
			Assert.IsFalse (converter.CanConvertTo (typeof (object)), "object");
			Assert.IsFalse (converter.CanConvertTo (typeof (int)), "int");
			Assert.IsFalse (converter.CanConvertTo (typeof (char)), "char");
			Assert.IsFalse (converter.CanConvertTo (typeof (DateTime)), "datetime");
		}

		[Test]
		public void CanConvertTo_Null_Type ()
		{
			Assert.IsFalse (converter.CanConvertTo (null, null), "null");

			Assert.IsTrue (converter.CanConvertTo (null, typeof (string)), "string");
			Assert.IsTrue (converter.CanConvertTo (null, typeof (Uri)), "Uri");
#if MOBILE
			Assert.IsFalse (converter.CanConvertTo (null, typeof (InstanceDescriptor)), "InstanceDescriptor");
#else
			Assert.IsTrue (converter.CanConvertTo (null, typeof (InstanceDescriptor)), "InstanceDescriptor");
#endif
			Assert.IsFalse (converter.CanConvertTo (null, typeof (object)), "object");
			Assert.IsFalse (converter.CanConvertTo (null, typeof (int)), "int");
			Assert.IsFalse (converter.CanConvertTo (null, typeof (char)), "char");
			Assert.IsFalse (converter.CanConvertTo (null, typeof (DateTime)), "datetime");
		}

		[Test]
		public void CanConvertTo_TypeDescriptorContext_Type ()
		{
			Assert.IsTrue (converter.CanConvertTo (context, typeof (string)), "string");
			Assert.IsTrue (converter.CanConvertTo (context, typeof (Uri)), "Uri");
#if MOBILE
			Assert.IsFalse (converter.CanConvertTo (context, typeof (InstanceDescriptor)), "InstanceDescriptor");
#else
			Assert.IsTrue (converter.CanConvertTo (context, typeof (InstanceDescriptor)), "InstanceDescriptor");
#endif
			Assert.IsFalse (converter.CanConvertTo (context, typeof (object)), "object");
			Assert.IsFalse (converter.CanConvertTo (context, typeof (int)), "int");
			Assert.IsFalse (converter.CanConvertTo (context, typeof (char)), "char");
			Assert.IsFalse (converter.CanConvertTo (context, typeof (DateTime)), "datetime");
		}

		[Test]
		public void ConvertFrom ()
		{
			object o = converter.ConvertFrom (url);
			Assert.AreEqual (url, (o as Uri).AbsoluteUri, "url");

			o = converter.ConvertFrom (uri);
			Assert.AreEqual (url, (o as Uri).AbsoluteUri, "uri");

#if MOBILE
			o = converter.ConvertFrom ("");
			Assert.IsNull (o, "null");
#else
			o = converter.ConvertFrom (converter.ConvertTo (uri, typeof (InstanceDescriptor)));
			Assert.AreEqual (url, (o as Uri).AbsoluteUri, "uri");

			o = converter.ConvertFrom ("");
			Assert.IsNotNull (o, "not null");
#endif
		}

		[Test]
		public void ConvertFromString ()
		{
			object o = converter.ConvertFrom ("~/SomeUri.txt");
			Assert.IsFalse ((o as Uri).IsAbsoluteUri, "CFS_01");
			Assert.AreEqual ("~/SomeUri.txt", (o as Uri).ToString (), "CFS_02");
			
			o = converter.ConvertFrom ("/SomeUri.txt");
			if (isWin32) {
				Assert.IsFalse ((o as Uri).IsAbsoluteUri, "CFS_03_WIN");
				Assert.AreEqual ("/SomeUri.txt", (o as Uri).ToString (), "CFS_04_WIN");
			} else {
				Assert.IsTrue ((o as Uri).IsAbsoluteUri, "CFS_03_UNIX");
				Assert.AreEqual ("file:///SomeUri.txt", (o as Uri).ToString (), "CFS_04_UNIX");
			}
		}
		
		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertFrom_Bad ()
		{
			converter.ConvertFrom (new object ());
		}

		[Test]
		public void ConvertFrom_TypeDescriptorContext_Type ()
		{
			object o = converter.ConvertFrom (context, CultureInfo.CurrentCulture, url);
			Assert.AreEqual (url, (o as Uri).AbsoluteUri, "url");

			o = converter.ConvertFrom (context, CultureInfo.InvariantCulture, uri);
			Assert.AreEqual (url, (o as Uri).AbsoluteUri, "uri");

#if !MOBILE
			o = converter.ConvertFrom (context, null, converter.ConvertTo (uri, typeof (InstanceDescriptor)));
			Assert.AreEqual (url, (o as Uri).AbsoluteUri, "uri");
#endif
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertFrom_TypeDescriptorContext_Bad ()
		{
			converter.ConvertFrom (context, CultureInfo.InvariantCulture, new object ());
		}

		[Test]
		public void ConvertTo ()
		{
			object o;
			o = converter.ConvertTo (uri, typeof (Uri));
			Assert.AreEqual (uri, (o as Uri), "uri-uri");

			o = converter.ConvertTo (uri, typeof (string));
			Assert.AreEqual (url, (o as String), "uri-string");

#if MOBILE
			try {
				converter.ConvertTo (url, typeof (string));
				Assert.Fail ("string-string");
			} catch (NotSupportedException) {
			}

			try {
				converter.ConvertTo (uri, typeof (InstanceDescriptor));
				Assert.Fail ("uri-InstanceDescriptor");
			} catch (NotSupportedException) {
			}
#else
			o = converter.ConvertTo (url, typeof (string));
			Assert.AreEqual (url, (o as String), "string-string");

			o = converter.ConvertTo (uri, typeof (InstanceDescriptor));
			Assert.IsTrue (o is InstanceDescriptor, "uri-InstanceDescriptor");

			InstanceDescriptor descriptor = (o as InstanceDescriptor);

			o = converter.ConvertTo (descriptor, typeof (string));
			Assert.AreEqual ("System.ComponentModel.Design.Serialization.InstanceDescriptor", (o as string), "InstanceDescriptor-string");
#endif
			// InstanceDescriptor to Uri or to InstanceDescriptor aren't supported either
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertTo_String_to_Uri ()
		{
			converter.ConvertTo (url, typeof (Uri));
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertTo_String_to_InstanceDescriptor ()
		{
			converter.ConvertTo (url, typeof (InstanceDescriptor));
		}

		[Test]
		public void ConvertTo_Bad ()
		{
#if MOBILE
			try {
				converter.ConvertTo (new object (), typeof (string));
				Assert.Fail ("object");
			} catch (NotSupportedException) {
			}
#else
			Assert.AreEqual ("System.Object", converter.ConvertTo (new object (), typeof (string)), "object");
			Assert.AreEqual ("4", converter.ConvertTo (4, typeof (string)), "int");
			Assert.AreEqual ("c", converter.ConvertTo ('c', typeof (string)), "char");
			Assert.AreEqual (String.Empty, converter.ConvertTo (null, typeof (string)), "null");
#endif
		}

		[Test]
		public void ConvertTo_TypeDescriptorContext ()
		{
			object o;

			o = converter.ConvertTo (context, null, uri, typeof (string));
			Assert.AreEqual (url, (o as String), "uri-string");

			o = converter.ConvertTo (context, CultureInfo.InvariantCulture, uri, typeof (Uri));
			Assert.AreEqual (uri, (o as Uri), "uri-uri");

#if MOBILE
			try {
				o = converter.ConvertTo (context, CultureInfo.InvariantCulture, url, typeof (string));
				Assert.AreEqual (url, (o as String), "string-string");
			} catch (NotSupportedException) {
			}
#else
			o = converter.ConvertTo (context, CultureInfo.InvariantCulture, url, typeof (string));
			Assert.AreEqual (url, (o as String), "string-string");

			o = converter.ConvertTo (context, CultureInfo.CurrentCulture, uri, typeof (InstanceDescriptor));
			Assert.IsTrue (o is InstanceDescriptor, "uri-InstanceDescriptor");

			InstanceDescriptor descriptor = (o as InstanceDescriptor);

			o = converter.ConvertTo (context, CultureInfo.InvariantCulture, descriptor, typeof (string));
			Assert.AreEqual ("System.ComponentModel.Design.Serialization.InstanceDescriptor", (o as string), "InstanceDescriptor-string");
#endif
			// InstanceDescriptor to Uri or to InstanceDescriptor aren't supported either
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertTo_TypeDescriptorContext_String_to_Uri ()
		{
			converter.ConvertTo (context, CultureInfo.InvariantCulture, url, typeof (Uri));
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertTo_TypeDescriptorContext_String_to_InstanceDescriptor ()
		{
			converter.ConvertTo (context, CultureInfo.InvariantCulture, url, typeof (InstanceDescriptor));
		}

		[Test]
		public void ConvertTo_TypeDescriptorContext_Bad ()
		{
#if MOBILE
			try {
				converter.ConvertTo (context, null, new object (), typeof (string));
				Assert.Fail ("object");
			} catch (NotSupportedException) {
			}
#else
			Assert.AreEqual ("System.Object", converter.ConvertTo (context, null, new object (), typeof (string)), "object");
			Assert.AreEqual ("4", converter.ConvertTo (context, CultureInfo.CurrentCulture, 4, typeof (string)), "int");
			Assert.AreEqual ("c", converter.ConvertTo (context, CultureInfo.InvariantCulture, 'c', typeof (string)), "char");
			Assert.AreEqual (String.Empty, converter.ConvertTo (null, null, null, typeof (string)), "null");
#endif
		}

		[Test]
		public void IsValid ()
		{
			Assert.IsFalse (converter.IsValid (null), "null");

			// LAMESPEC: all strings are accepted
			Assert.IsTrue (converter.IsValid (String.Empty), "empty");
			Assert.IsTrue (converter.IsValid ("\\"), "\\");
			Assert.IsTrue (converter.IsValid ("#%#%#"), "#%#%#");
			Assert.IsTrue (converter.IsValid (".."), "..");
			Assert.IsTrue (converter.IsValid (url), url);

			Assert.IsTrue (converter.IsValid (uri), "uri");

			Assert.IsFalse (converter.IsValid (new object ()), "object");
			Assert.IsFalse (converter.IsValid (4), "int");
			Assert.IsFalse (converter.IsValid ('c'), "char");
			Assert.IsFalse (converter.IsValid (DateTime.Now), "datetime");
		}
	}
}

#endif
