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
using System.Linq;
using System.Reflection;
using System.Windows.Markup;
using System.Xaml;
using System.Xaml.Schema;
using NUnit.Framework;

using CategoryAttribute = NUnit.Framework.CategoryAttribute;

namespace MonoTests.System.Xaml
{
	public class ArgumentAttributed
	{
		public ArgumentAttributed (string s1, string s2)
		{
			Arg1 = s1;
			Arg2 = s2;
		}

		[ConstructorArgument ("s1")]
		public string Arg1 { get; set; }

		[ConstructorArgument ("s2")]
		public string Arg2 { get; set; }
	}

	public class ComplexPositionalParameterWrapper
	{
		public ComplexPositionalParameterWrapper ()
		{
		}
		
		public ComplexPositionalParameterClass Param { get; set; }
	}
	
	public class ComplexPositionalParameterClass : MarkupExtension
	{
		public ComplexPositionalParameterClass (ComplexPositionalParameterValue value)
		{
			this.Value = value;
		}

		[ConstructorArgument ("value")]
		public ComplexPositionalParameterValue Value { get; private set; }
		
		public override object ProvideValue (IServiceProvider sp)
		{
			return Value.Foo;
		}
	}
	
	public class ComplexPositionalParameterValue
	{
		public string Foo { get; set; }
	}
	
	//[MarkupExtensionReturnType (typeof (Array))]
	//[ContentProperty ("Items")]  ... so, these attributes do not affect XamlObjectReader.
	public class MyArrayExtension : MarkupExtension
	{
		public MyArrayExtension ()
		{
			Items = new ArrayList ();
		}

		public MyArrayExtension (Array array)
		{
			this.Items = array;
			this.Type = array.GetType ().GetElementType ();
		}
		
		public MyArrayExtension (Type type)
			: this ()
		{
			this.Type = type;
		}
		
		[ConstructorArgument ("type")]
		public Type Type { get; set; }

		public IList Items { get; private set; }
		
		public override object ProvideValue (IServiceProvider serviceProvider)
		{
			if (Type == null)
				throw new InvalidOperationException ("Type property must be set before calling ProvideValue method");

			Array a = Array.CreateInstance (Type, Items.Count);
			Items.CopyTo (a, 0);
			return a;
		}
	}

	class TestClass1
	{
	}

	public class TestClass3
	{
		public TestClass3 Nested { get; set; }
	}

	public class TestClass4
	{
		public string Foo { get; set; }
		public string Bar { get; set; }
	}
	
	public class TestClass5
	{
		public static string Foo { get; set; }
		public string Bar { get; set; }
		public string Baz { internal get; set; }
		public string ReadOnly {
			get { return Foo; }
		}
	}

	public class MyExtension : MarkupExtension
	{
		public MyExtension ()
		{
		}

		public MyExtension (Type arg1, string arg2, string arg3)
		{
			Foo = arg1;
			Bar = arg2;
			Baz = arg3;
		}

		[ConstructorArgument ("arg1")]
		public Type Foo { get; set; }
		
		[ConstructorArgument ("arg2")]
		public string Bar { get; set; }
		
		[ConstructorArgument ("arg3")]
		public string Baz { get; set; }

		public override object ProvideValue (IServiceProvider provider)
		{
			return "provided_value";
		}
	}

	[TypeConverter (typeof (StringConverter))] // This attribute is the markable difference between MyExtension and this type.
	public class MyExtension2 : MarkupExtension
	{
		public MyExtension2 ()
		{
		}

		public MyExtension2 (Type arg1, string arg2)
		{
			Foo = arg1;
			Bar = arg2;
		}

		[ConstructorArgument ("arg1")]
		public Type Foo { get; set; }
		
		[ConstructorArgument ("arg2")]
		public string Bar { get; set; }

		public override object ProvideValue (IServiceProvider provider)
		{
			return "provided_value";
		}
	}

	[TypeConverter (typeof (StringConverter))] // same as MyExtension2 except that it is *not* MarkupExtension.
	public class MyExtension3
	{
		public MyExtension3 ()
		{
		}

		// cf. According to [MS-XAML-2009] 3.2.1.11, constructors are invalid unless the type is derived from TypeExtension. So, it is likely *ignored*.
		public MyExtension3 (Type arg1, string arg2)
		{
			Foo = arg1;
			Bar = arg2;
		}

		[ConstructorArgument ("arg1")]
		public Type Foo { get; set; }
		
		[ConstructorArgument ("arg2")]
		public string Bar { get; set; }
	}

	[TypeConverter (typeof (DateTimeConverter))] // same as MyExtension3 except for the type converter.
	public class MyExtension4
	{
		public MyExtension4 ()
		{
		}

		// cf. According to [MS-XAML-2009] 3.2.1.11, constructors are invalid unless the type is derived from TypeExtension. So, it is likely *ignored*.
		public MyExtension4 (Type arg1, string arg2)
		{
			Foo = arg1;
			Bar = arg2;
		}

		[ConstructorArgument ("arg1")]
		public Type Foo { get; set; }
		
		[ConstructorArgument ("arg2")]
		public string Bar { get; set; }
	}

	// no type converter, and there are only simple-type arguments == _PositionalParameters is applicable. (Unlike MyExtension7)
	public class MyExtension5 : MarkupExtension
	{
		public MyExtension5 (string arg1, string arg2)
		{
			Foo = arg1;
			Bar = arg2;
		}

		[ConstructorArgument ("arg1")]
		public string Foo { get; set; }
		
		[ConstructorArgument ("arg2")]
		public string Bar { get; set; }
		
		public override object ProvideValue (IServiceProvider sp)
		{
			return Foo;
		}
	}

	// Almost the same as MyExtension5, BUT there is default constructor which XamlObjectReader prefers.
	public class MyExtension6 : MarkupExtension
	{
		public MyExtension6 ()
		{
		}

		public MyExtension6 (string arg1)
		{
			Foo = arg1;
		}

		[ConstructorArgument ("arg1")]
		public string Foo { get; set; }
		
		public override object ProvideValue (IServiceProvider sp)
		{
			return Foo;
		}
	}

	public class PositionalParametersClass1 : MarkupExtension
	{
		public PositionalParametersClass1 (string foo)
			: this (foo, -1)
		{
		}
		
		public PositionalParametersClass1 (string foo, int bar)
		{
			Foo = foo;
			Bar = bar;
		}
		
		[ConstructorArgument ("foo")]
		public string Foo { get; set; }

		[ConstructorArgument ("bar")]
		public int Bar { get; set; }

		public override object ProvideValue (IServiceProvider sp)
		{
			return Foo;
		}
	}

	public class PositionalParametersWrapper
	{
		public PositionalParametersClass1 Body { get; set; }
		
		public PositionalParametersWrapper ()
		{
		}
		
		public PositionalParametersWrapper (string foo, int bar)
		{
			Body = new PositionalParametersClass1 (foo, bar);
		}
	}
	
	public class ListWrapper
	{
		public ListWrapper ()
		{
			Items = new List<int> ();
		}

		public ListWrapper (List<int> items)
		{
			Items = items;
		}

		public List<int> Items { get; private set; }
	}
	
	public class ListWrapper2
	{
		public ListWrapper2 ()
		{
			Items = new List<int> ();
		}

		public ListWrapper2 (List<int> items)
		{
			Items = items;
		}

		public List<int> Items { get; set; } // it is settable, which makes difference.
	}
}
