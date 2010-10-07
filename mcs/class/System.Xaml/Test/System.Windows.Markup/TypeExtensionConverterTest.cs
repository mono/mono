//
// Copyright (C) 2010 Novell Inc. http://novell.com
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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Reflection;
using System.Windows.Markup;
using System.Xaml;
using System.Xaml.Schema;
using NUnit.Framework;

using Category = NUnit.Framework.CategoryAttribute;

namespace MonoTests.System.Windows.Markup
{
	[TestFixture]
	public class TypeExtensionConverterTest
	{
		class XamlTypeResolver : IXamlTypeResolver
		{
			public Type Resolve (string qualifiedTypeName)
			{
				throw new NotImplementedException ();
			}
		}
		
		class TypeDescriptorContext : ITypeDescriptorContext
		{
			public object Service { get; set; }

			public object GetService (Type serviceType)
			{
				return Service != null && serviceType.IsAssignableFrom (Service.GetType ()) ? Service : null;
			}

			public void OnComponentChanged ()
			{
			}

			public bool OnComponentChanging ()
			{
				return true;
			}
			
			public IContainer Container { get; set; }

			public object Instance { get; set; }

			public PropertyDescriptor PropertyDescriptor { get; set; }
		}
		
		[Test]
		public void CanConvertFrom ()
		{
			var tc = XamlLanguage.Type.TypeConverter.ConverterInstance;
			Assert.IsFalse (tc.CanConvertFrom (null, typeof (string)), "#1");
			Assert.IsFalse (tc.CanConvertFrom (null, typeof (Type)), "#2");
			Assert.IsFalse (tc.CanConvertFrom (null, typeof (Type)), "#3");
			Assert.IsTrue (tc.CanConvertFrom (null, typeof (InstanceDescriptor)), "#4");

			var idc = new TypeDescriptorContext () {Instance = "x:Int32", Service = new XamlTypeResolver ()}; // gives no difference ...
			Assert.IsFalse (tc.CanConvertFrom (idc, typeof (string)), "#5");
			Assert.IsFalse (tc.CanConvertFrom (idc, typeof (Type)), "#6");
			Assert.IsFalse (tc.CanConvertFrom (idc, typeof (TypeExtension)), "#7");
		}

		[Test]
		public void CanConvertTo ()
		{
			var tc = XamlLanguage.Type.TypeConverter.ConverterInstance;
			Assert.IsTrue (tc.CanConvertTo (null, typeof (string)), "#1");
			Assert.IsFalse (tc.CanConvertTo (null, typeof (Type)), "#2");
			Assert.IsFalse (tc.CanConvertTo (null, typeof (TypeExtension)), "#3");

			var idc = new TypeDescriptorContext () {Instance = "x:Int32", Service = new XamlTypeResolver ()}; // gives no differences...
			Assert.IsTrue (tc.CanConvertTo (idc, typeof (string)), "#5");
			Assert.IsFalse (tc.CanConvertTo (idc, typeof (Type)), "#6");
			Assert.IsFalse (tc.CanConvertTo (idc, typeof (TypeExtension)), "#7");
		}

		[Test]
		public void ConvertTo ()
		{
			var tc = XamlLanguage.Type.TypeConverter.ConverterInstance;
			Assert.AreEqual ("x:Int32", tc.ConvertTo (null, null, "x:Int32", typeof (string)), "#1");
			Assert.AreEqual ("System.Int32", tc.ConvertTo (null, null, typeof (int), typeof (string)), "#2");
			Assert.AreEqual ("System.Type", tc.ConvertTo (null, null, typeof (Type), typeof (string)), "#3");
		}
		
		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertToFail ()
		{
			var tc = XamlLanguage.Type.TypeConverter.ConverterInstance;
			tc.ConvertTo (null, null, typeof (int), typeof (Type));
		}
		
		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertToFail2 ()
		{
			var tc = XamlLanguage.Type.TypeConverter.ConverterInstance;
			tc.ConvertTo (null, null, "x:Int32", typeof (TypeExtension));
		}
	}
}
