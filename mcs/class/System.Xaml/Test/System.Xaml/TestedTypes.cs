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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Markup;
using System.Xaml;
using System.Xaml.Schema;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

[assembly: System.Windows.Markup.XmlnsDefinition ("http://www.domain.com/path", "XamlTest")] // bug #680385
[assembly: System.Windows.Markup.XmlnsDefinition ("http://www.domain.com/path", "SecondTest")] // bug #681045, same xmlns key for different clrns.

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
	
	[TypeConverter (typeof (ComplexPositionalParameterClassConverter))]
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
	
	public class ComplexPositionalParameterClassConverter : TypeConverter
	{
		public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
		{
			return sourceType == typeof (string);
		}

		public override object ConvertFrom (ITypeDescriptorContext context, CultureInfo culture, object valueToConvert)
		{
			return new ComplexPositionalParameterClass (new ComplexPositionalParameterValue () {Foo = (string) valueToConvert});
		}

		public override bool CanConvertTo (ITypeDescriptorContext context, Type destinationType)
		{
			// conversion to string is not supported.
			return destinationType == typeof (ComplexPositionalParameterClass);
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
			items = new ArrayList ();
		}

		public MyArrayExtension (Array array)
		{
			items = new ArrayList (array);
			this.Type = array.GetType ().GetElementType ();
		}
		
		public MyArrayExtension (Type type)
			: this ()
		{
			this.Type = type;
		}

		IList items;
		public IList Items {
			get { return items; }
			private set { items = value; }
		}
		
		[ConstructorArgument ("type")]
		public Type Type { get; set; }
		
		public override object ProvideValue (IServiceProvider serviceProvider)
		{
			if (Type == null)
				throw new InvalidOperationException ("Type property must be set before calling ProvideValue method");

			Array a = Array.CreateInstance (Type, Items.Count);
			Items.CopyTo (a, 0);
			return a;
		}
	}

	// The trailing "A" gives significant difference in XML output!
	public class MyArrayExtensionA : MarkupExtension
	{
		public MyArrayExtensionA ()
		{
			items = new ArrayList ();
		}

		public MyArrayExtensionA (Array array)
		{
			items = new ArrayList (array);
			this.Type = array.GetType ().GetElementType ();
		}
		
		public MyArrayExtensionA (Type type)
			: this ()
		{
			this.Type = type;
		}

		IList items;
		public IList Items {
			get { return items; }
			private set { items = value; }
		}
		
		[ConstructorArgument ("type")]
		public Type Type { get; set; }
		
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

	// no type converter, and there are only simple-type arguments == _PositionalParameters is applicable.
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

	[ContentProperty ("Content")]
	public class ContentIncludedClass
	{
		public string Content { get; set; }
	}

	public class StaticClass1
	{
		static StaticClass1 ()
		{
			FooBar = "test";
		}

		public static string FooBar { get; set; }
	}

	public class StaticExtensionWrapper
	{
		public StaticExtensionWrapper ()
		{
		}
		
		public StaticExtension Param { get; set; }

		public static string Foo = "foo";
	}
	
	public class TypeExtensionWrapper
	{
		public TypeExtensionWrapper ()
		{
		}
		
		public TypeExtension Param { get; set; }
	}
	
	public class XDataWrapper
	{
		public XData Markup { get; set; }
	}
	
	// FIXME: test it with XamlXmlReader (needs to create xml first)
	public class EventContainer
	{
		public event Action Run;
	}
	
	public class NamedItem
	{
		public NamedItem ()
		{
			References = new List<NamedItem> ();
		}
		
		public NamedItem (string name)
			: this ()
		{
			ItemName = name;
		}
		
		public string ItemName { get; set; }
		public IList<NamedItem> References { get; private set; }
	}
	
	[RuntimeNameProperty ("ItemName")]
	public class NamedItem2
	{
		public NamedItem2 ()
		{
			References = new List<NamedItem2> ();
		}
		
		public NamedItem2 (string name)
			: this ()
		{
			ItemName = name;
		}
		
		public string ItemName { get; set; }
		public IList<NamedItem2> References { get; private set; }
	}

	[TypeConverter (typeof (TestValueConverter))]
	public class TestValueSerialized
	{
		public TestValueSerialized ()
		{
		}

		public string Foo { get; set; }
	}

	public class TestValueConverter : TypeConverter
	{
		public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
		{
			//Console.Error.WriteLine ("### {0}:{1}", sourceType, context);
			ValueSerializerContextTest.Context = (IValueSerializerContext) context;
			return true;
		}
		
		public override object ConvertFrom (ITypeDescriptorContext context, CultureInfo culture, object source)
		{
			//Console.Error.WriteLine ("##### {0}:{1}", source, context);
			ValueSerializerContextTest.Provider = (IServiceProvider) context;
			var sp = context as IServiceProvider;
			// ValueSerializerContextTest.Context = (IValueSerializerContext) context; -> causes InvalidCastException
			if ((source as string) == "v")
				return new TestValueSerialized ();
			throw new Exception ("huh");
		}

		public override bool CanConvertTo (ITypeDescriptorContext context, Type destinationType)
		{
			//Console.Error.WriteLine ("$$$ {0}:{1}", destinationType, context);
			ValueSerializerContextTest.Context = (IValueSerializerContext) context;
			return destinationType != typeof (MarkupExtension);
		}
	}

	[ContentProperty ("Value")]
	public class XmlSerializableWrapper
	{
		public XmlSerializableWrapper () // mandatory
			: this (new XmlSerializable ())
		{
		}

		public XmlSerializableWrapper (XmlSerializable val)
		{
			this.val = val;
		}

		XmlSerializable val;

		public XmlSerializable Value {
			get { return val; }
			// To make it become XData, it cannot have a setter.
		}
	}

	public class XmlSerializable : IXmlSerializable
	{
		public XmlSerializable ()
		{
		}

		public XmlSerializable (string raw)
		{
			this.raw = raw;
		}

		string raw;

		public string GetRaw ()
		{
			return raw;
		}

		public void ReadXml (XmlReader reader)
		{
			reader.MoveToContent ();
			raw = reader.ReadOuterXml ();
		}
		
		public void WriteXml (XmlWriter writer)
		{
			if (raw != null) {
				var xr = XmlReader.Create (new StringReader (raw));
				while (!xr.EOF)
					writer.WriteNode (xr, false);
			}
		}
		
		public XmlSchema GetSchema ()
		{
			return null;
		}
	}
	
	public class Attachable
	{
		public static readonly AttachableMemberIdentifier FooIdentifier = new AttachableMemberIdentifier (typeof (Attachable), "Foo");
		public static readonly AttachableMemberIdentifier ProtectedIdentifier = new AttachableMemberIdentifier (typeof (Attachable), "Protected");
		
		public static string GetFoo (object target)
		{
			string v;
			return AttachablePropertyServices.TryGetProperty (target, FooIdentifier, out v) ? v : null;
		}
		
		public static void SetFoo (object target, string value)
		{
			AttachablePropertyServices.SetProperty (target, FooIdentifier, value);
		}

		public static string GetBar (object target, object signatureMismatch)
		{
			return null;
		}
		
		public static void SetBar (object signatureMismatch)
		{
		}

		public static void GetBaz (object noReturnType)
		{
		}
		
		public static string SetBaz (object target, object extraReturnType)
		{
			return null;
		}

		protected static string GetProtected (object target)
		{
			string v;
			return AttachablePropertyServices.TryGetProperty (target, ProtectedIdentifier, out v) ? v : null;
		}
		
		protected static void SetProtected (object target, string value)
		{
			AttachablePropertyServices.SetProperty (target, ProtectedIdentifier, value);
		}

		static Dictionary<object,List<EventHandler>> handlers = new Dictionary<object,List<EventHandler>> ();

		public static void AddXHandler (object target, EventHandler handler)
		{
			List<EventHandler> l;
			if (!handlers.TryGetValue (target, out l)) {
				l = new List<EventHandler> ();
				handlers [target] = l;
			}
			l.Add (handler);
		}

		public static void RemoveXHandler (object target, EventHandler handler)
		{
			handlers [target].Remove (handler);
		}
	}
	
	public class AttachedPropertyStore : IAttachedPropertyStore
	{
		public AttachedPropertyStore ()
		{
		}
		
		Dictionary<AttachableMemberIdentifier,object> props = new Dictionary<AttachableMemberIdentifier,object> ();

		public int PropertyCount {
			get { return props.Count; }
		}
		
		public void CopyPropertiesTo (KeyValuePair<AttachableMemberIdentifier, object> [] array, int index)
		{
			((ICollection<KeyValuePair<AttachableMemberIdentifier, object>>) props).CopyTo (array, index);
		}
		
		public bool RemoveProperty (AttachableMemberIdentifier attachableMemberIdentifier)
		{
			return props.Remove (attachableMemberIdentifier);
		}
		
		public void SetProperty (AttachableMemberIdentifier attachableMemberIdentifier, object value)
		{
			props [attachableMemberIdentifier] = value;
		}
		
		public bool TryGetProperty (AttachableMemberIdentifier attachableMemberIdentifier, out object value)
		{
			return props.TryGetValue (attachableMemberIdentifier, out value);
		}
	}

	public class AttachedWrapper : AttachedPropertyStore
	{
		public AttachedWrapper ()
		{
			Value = new Attached ();
		}

		public Attached Value { get; set; }
	}

	public class AttachedWrapper2
	{
		public static readonly AttachableMemberIdentifier FooIdentifier = new AttachableMemberIdentifier (typeof (AttachedWrapper2), "Foo");

		static AttachedPropertyStore store = new AttachedPropertyStore ();

		public static string GetFoo (object target)
		{
			object v;
			return store.TryGetProperty (FooIdentifier, out v) ? (string) v : null;
		}
		
		public static void SetFoo (object target, string value)
		{
			store.SetProperty (FooIdentifier, value);
		}

		public static int PropertyCount {
			get { return store.PropertyCount; }
		}

		public AttachedWrapper2 ()
		{
			Value = new Attached ();
		}

		public Attached Value { get; set; }
	}

	public class Attached : Attachable
	{
	}

	public class EventStore
	{
		public bool Method1Invoked;

		public event EventHandler<EventArgs> Event1;
		public event Func<object> Event2;

		public object Examine ()
		{
			if (Event1 != null)
				Event1 (this, EventArgs.Empty);
			if (Event2 != null)
				return Event2 ();
			else
				return null;
		}

		public void Method1 ()
		{
			throw new Exception ();
		}

		public void Method1 (object o, EventArgs e)
		{
			Method1Invoked = true;
		}

		public object Method2 ()
		{
			return "foo";
		}
	}

	public class EventStore2<TEventArgs> where TEventArgs : EventArgs
	{
		public bool Method1Invoked;

		public event EventHandler<TEventArgs> Event1;
		public event Func<object> Event2;

		public object Examine ()
		{
			if (Event1 != null)
				Event1 (this, default (TEventArgs));
			if (Event2 != null)
				return Event2 ();
			else
				return null;
		}

		public void Method1 ()
		{
			throw new Exception ();
		}

		public void Method1 (object o, EventArgs e)
		{
			throw new Exception ();
		}

		public void Method1 (object o, TEventArgs e)
		{
			Method1Invoked = true;
		}

		public object Method2 ()
		{
			return "foo";
		}
	}

	public class AbstractContainer
	{
		public AbstractObject Value1 { get; set; }
		public AbstractObject Value2 { get; set; }
	}
	
	public abstract class AbstractObject
	{
		public abstract string Foo { get; set; }
	}

	public class DerivedObject : AbstractObject
	{
		public override string Foo { get; set; }
	}

	public class ReadOnlyPropertyContainer
	{
		string foo;
		public string Foo {
			get { return foo; }
			set { foo = Bar = value; }
		}
		public string Bar { get; private set; }
	}
}

namespace XamlTest
{
	public class Configurations : List<Configuration>
	{
		private Configuration active;
		private bool isFrozen;

		public Configuration Active {
			get { return this.active; }
			set {
				if (this.isFrozen) {
				throw new InvalidOperationException ("The 'Active' configuration can only be changed via modifying the source file (" + this.Source + ").");
				}

				this.active = value;
			}
		}

		public string Source { get; private set; }
	}

	public class Configuration
	{
		public string Version { get; set; }

		public string Path { get; set; }
	}
}
