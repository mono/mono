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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Markup;
using System.Xaml;
using System.Xaml.Schema;
using System.Xml;
using NUnit.Framework;
using MonoTests.System.Xaml;

using CategoryAttribute = NUnit.Framework.CategoryAttribute;

namespace MonoTests.System.Xaml
{
	[TestFixture]
	public class ValueSerializerContextTest
	{
		public static IServiceProvider Provider;
		public static IValueSerializerContext Context {
			get { return (IValueSerializerContext) Provider; }
			set { Provider = value; }
		}

		[SetUp]
		public void Setup ()
		{
		}

		[Test]
		[Category ("NotWorking")]
		public void GetService ()
		{
			var obj = new TestValueSerialized ();
			var xr = new XamlObjectReader (obj);
			while (!xr.IsEof)
				xr.Read ();
			Assert.IsNotNull (Context, "premise#1");
			GetServiceCoreReader ();

			Context = null;
			var ctx = new XamlSchemaContext ();
			var xw = new XamlObjectWriter (ctx);
			var xt = ctx.GetXamlType (obj.GetType ());
			xw.WriteStartObject (xt);
			xw.WriteStartMember (XamlLanguage.Initialization);
			xw.WriteValue ("v");
			xw.WriteEndMember ();
			xw.Close ();
			Assert.IsNotNull (Context, "premise#2");
			GetServiceCoreWriter ();
		}
		
		void GetServiceCoreReader ()
		{
			//Assert.IsNull (Provider.GetService (typeof (IXamlNameResolver)), "#1");
			Assert.IsNotNull (Provider.GetService (typeof (IXamlNameProvider)), "#2");
			//Assert.IsNull (Provider.GetService (typeof (IXamlNamespaceResolver)), "#3");
			Assert.IsNotNull (Provider.GetService (typeof (INamespacePrefixLookup)), "#4");
			//Assert.IsNull (Provider.GetService (typeof (IXamlTypeResolver)), "#5");
			Assert.IsNotNull (Provider.GetService (typeof (IXamlSchemaContextProvider)), "#6");
			//Assert.IsNull (Provider.GetService (typeof (IAmbientProvider)), "#7");
			//Assert.IsNull (Provider.GetService (typeof (IAttachedPropertyStore)), "#8");
			//Assert.IsNull (Provider.GetService (typeof (IDestinationTypeProvider)), "#9");
			//Assert.IsNull (Provider.GetService (typeof (IXamlObjectWriterFactory)), "#10");
		}
		
		void GetServiceCoreWriter ()
		{
			Assert.IsNotNull (Provider.GetService (typeof (IXamlNameResolver)), "#1");
			//Assert.IsNull (Provider.GetService (typeof (IXamlNameProvider)), "#2");
			Assert.IsNotNull (Provider.GetService (typeof (IXamlNamespaceResolver)), "#3");
			//Assert.IsNull (Provider.GetService (typeof (INamespacePrefixLookup)), "#4");
			Assert.IsNotNull (Provider.GetService (typeof (IXamlTypeResolver)), "#5");
			Assert.IsNotNull (Provider.GetService (typeof (IXamlSchemaContextProvider)), "#6");
			Assert.IsNotNull (Provider.GetService (typeof (IAmbientProvider)), "#7");
			//Assert.IsNull (Provider.GetService (typeof (IAttachedPropertyStore)), "#8");
			Assert.IsNotNull (Provider.GetService (typeof (IDestinationTypeProvider)), "#9");
			//Assert.IsNull (Provider.GetService (typeof (IXamlObjectWriterFactory)), "#10"); -> call to this method causes some internal exception. Smells like a .NET bug.
		}

		[Test]
		[Category ("NotWorking")]
		public void NameResolver ()
		{
			var nr = (IXamlNameResolver) Provider.GetService (typeof (IXamlNameResolver));
			Assert.IsNull (nr.Resolve ("random"), "nr#1");
			//var ft = nr.GetFixupToken (new string [] {"random"}); -> causes internal error.
			//var ft = nr.GetFixupToken (new string [] {"random"}, true); -> causes internal error
			//var ft = nr.GetFixupToken (new string [0], false);
			//Assert.IsNotNull (ft, "nr#2");
		}
	}
}
