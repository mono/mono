//
// System.ComponentModel.TypeDescriptorTests test cases
//
// Authors:
// 	Lluis Sanchez Gual (lluis@ximian.com)
//
// (c) 2004 Novell, Inc. (http://www.ximian.com)
//
using NUnit.Framework;
using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Globalization;

namespace MonoTests.System.ComponentModel
{
	class MyDesigner: IDesigner
	{
		public MyDesigner()
		{
		}

		public IComponent Component {get{return null; }}

		public DesignerVerbCollection Verbs {get{return null; }}

		public void DoDefaultAction () { }

		public void Initialize (IComponent component) { }

		public void Dispose () { }
	}

	class MySite: ISite
	{ 
		public IComponent Component { get {  return null; } }

		public IContainer Container { get {  return null; } }

		public bool DesignMode { get {  return true; } }

		public string Name { get { return "TestName"; } set { } }	

		public object GetService (Type t)
		{
			if (t == typeof(ITypeDescriptorFilterService)) return new MyFilter ();
			return null;
		}
	}
	
	class MyFilter: ITypeDescriptorFilterService
	{
		public bool FilterAttributes (IComponent component,IDictionary attributes)
		{
			Attribute ea = new DefaultEventAttribute ("AnEvent");
			attributes [ea.TypeId] = ea;
			ea = new DefaultPropertyAttribute ("TestProperty");
			attributes [ea.TypeId] = ea;
			ea = new EditorAttribute ();
			attributes [ea.TypeId] = ea;
			return true;
		}
		
		public bool FilterEvents (IComponent component, IDictionary events)
		{
			events.Remove ("AnEvent");
			return true;
		}
		
		public bool FilterProperties (IComponent component, IDictionary properties)
		{
			properties.Remove ("TestProperty");
			return true;
		}
	}

	[DescriptionAttribute ("my test component")]
	[DesignerAttribute (typeof(MyDesigner), typeof(int))]
	public class MyComponent: Component
	{
		string prop;
		
		[DescriptionAttribute ("test")]
		public event EventHandler AnEvent;
		
		public event EventHandler AnotherEvent;
		
		public MyComponent  ()
		{
		}
		
		public MyComponent (ISite site)
		{
			Site = site;
		}
		
		[DescriptionAttribute ("test")]
		public string TestProperty
		{
			get { return prop; }
			set { prop = value; }
		}
		
		public string AnotherProperty
		{
			get { return prop; }
			set { prop = value; }
		}
	}
	
	[TestFixture]
	public class TypeDescriptorTests: Assertion
	{
		MyComponent com = new MyComponent ();
		MyComponent sitedcom = new MyComponent (new MySite ());
		
		[Test]
		public void TestCreateDesigner ()
		{
			IDesigner des = TypeDescriptor.CreateDesigner (com, typeof(int));
			Assert ("t1", des is MyDesigner);
			
			des = TypeDescriptor.CreateDesigner (com, typeof(string));
			AssertNull ("t2", des);
		}
		
		[Test]
		public void TestCreateEvent ()
		{
			EventDescriptor ed = TypeDescriptor.CreateEvent (typeof(MyComponent), "AnEvent", typeof(EventHandler), null);
			AssertEquals ("t1", typeof(MyComponent), ed.ComponentType);
			AssertEquals ("t2", typeof(EventHandler), ed.EventType);
			AssertEquals ("t3", true, ed.IsMulticast);
			AssertEquals ("t4", "AnEvent", ed.Name);
		}
		
		[Test]
		public void TestCreateProperty ()
		{
			PropertyDescriptor pd = TypeDescriptor.CreateProperty (typeof(MyComponent), "TestProperty", typeof(string), null);
			AssertEquals ("t1", typeof(MyComponent), pd.ComponentType);
			AssertEquals ("t2", "TestProperty", pd.Name);
			AssertEquals ("t3", typeof(string), pd.PropertyType);
			AssertEquals ("t4", false, pd.IsReadOnly);
			
			pd.SetValue (com, "hi");
			AssertEquals ("t5", "hi", pd.GetValue(com));
		}
		
		[Test]
		public void TestGetAttributes ()
		{
			AttributeCollection col = TypeDescriptor.GetAttributes (typeof(MyComponent));
			Assert ("t2", col[typeof(DescriptionAttribute)] != null);
			Assert ("t3", col[typeof(DesignerAttribute)] != null);
			Assert ("t4", col[typeof(EditorAttribute)] == null);
			
			col = TypeDescriptor.GetAttributes (com);
			Assert ("t6", col[typeof(DescriptionAttribute)] != null);
			Assert ("t7", col[typeof(DesignerAttribute)] != null);
			Assert ("t8", col[typeof(EditorAttribute)] == null);
			
			col = TypeDescriptor.GetAttributes (sitedcom);
			Assert ("t10", col[typeof(DescriptionAttribute)] != null);
			Assert ("t11", col[typeof(DesignerAttribute)] != null);
			Assert ("t12", col[typeof(EditorAttribute)] != null);
		}
		
		[Test]
		public void TestGetClassName ()
		{
			AssertEquals ("t1", typeof(MyComponent).FullName, TypeDescriptor.GetClassName (com));
		}
		
		[Test]
		public void TestGetComponentName ()
		{
			AssertNotNull ("t1", TypeDescriptor.GetComponentName (com));
			AssertEquals ("t2", "MyComponent", TypeDescriptor.GetComponentName (com));
			AssertEquals ("t3", "TestName", TypeDescriptor.GetComponentName (sitedcom));
		}
		
		[Test]
		public void TestGetConverter ()
		{
			AssertEquals (typeof(BooleanConverter), TypeDescriptor.GetConverter (typeof (bool)).GetType());
			AssertEquals (typeof(ByteConverter), TypeDescriptor.GetConverter (typeof (byte)).GetType());
			AssertEquals (typeof(SByteConverter), TypeDescriptor.GetConverter (typeof (sbyte)).GetType());
			AssertEquals (typeof(StringConverter), TypeDescriptor.GetConverter (typeof (string)).GetType());
			AssertEquals (typeof(CharConverter), TypeDescriptor.GetConverter (typeof (char)).GetType());
			AssertEquals (typeof(Int16Converter), TypeDescriptor.GetConverter (typeof (short)).GetType());
			AssertEquals (typeof(Int32Converter), TypeDescriptor.GetConverter (typeof (int)).GetType());
			AssertEquals (typeof(Int64Converter), TypeDescriptor.GetConverter (typeof (long)).GetType());
			AssertEquals (typeof(UInt16Converter), TypeDescriptor.GetConverter (typeof (ushort)).GetType());
			AssertEquals (typeof(UInt32Converter), TypeDescriptor.GetConverter (typeof (uint)).GetType());
			AssertEquals (typeof(UInt64Converter), TypeDescriptor.GetConverter (typeof (ulong)).GetType());
			AssertEquals (typeof(SingleConverter), TypeDescriptor.GetConverter (typeof (float)).GetType());
			AssertEquals (typeof(DoubleConverter), TypeDescriptor.GetConverter (typeof (double)).GetType());
			AssertEquals (typeof(DecimalConverter), TypeDescriptor.GetConverter (typeof (decimal)).GetType());
			AssertEquals (typeof(ArrayConverter), TypeDescriptor.GetConverter (typeof (Array)).GetType());
			AssertEquals (typeof(CultureInfoConverter), TypeDescriptor.GetConverter (typeof (CultureInfo)).GetType());
			AssertEquals (typeof(DateTimeConverter), TypeDescriptor.GetConverter (typeof (DateTime)).GetType());
			AssertEquals (typeof(GuidConverter), TypeDescriptor.GetConverter (typeof (Guid)).GetType());
			AssertEquals (typeof(TimeSpanConverter), TypeDescriptor.GetConverter (typeof (TimeSpan)).GetType());
			AssertEquals (typeof(CollectionConverter), TypeDescriptor.GetConverter (typeof (ICollection)).GetType());
		}
		
		[Test]
		public void TestGetDefaultEvent ()
		{
			EventDescriptor des = TypeDescriptor.GetDefaultEvent (typeof(MyComponent));
			AssertNull ("t1", des);
			
			des = TypeDescriptor.GetDefaultEvent (com);
			AssertNull ("t2", des);
			
			des = TypeDescriptor.GetDefaultEvent (sitedcom);
			AssertNotNull ("t3", des);
			AssertEquals ("t4", "AnotherEvent", des.Name);
		}
		
		[Test]
		public void TestGetDefaultProperty ()
		{
			PropertyDescriptor des = TypeDescriptor.GetDefaultProperty (typeof(MyComponent));
			AssertNull ("t1", des);
			
			des = TypeDescriptor.GetDefaultProperty (com);
			AssertNull ("t2", des);
			
		}
		
		[Test]
		[Ignore("Fails on .NET")]
		public void TestGetDefaultProperty2 ()
		{
			PropertyDescriptor des = TypeDescriptor.GetDefaultProperty (sitedcom);
			AssertNotNull ("t3", des);
			AssertEquals ("t4", "TestProperty", des.Name);
		}
		
		[Test]
		public void TestGetEvents ()
		{
			EventDescriptorCollection col = TypeDescriptor.GetEvents (typeof(MyComponent));
				
			AssertEquals ("t1.1", 3, col.Count);
			Assert ("t1.2", col.Find ("AnEvent", true) != null);
			Assert ("t1.3", col.Find ("AnotherEvent", true) != null);
			Assert ("t1.4", col.Find ("Disposed", true) != null);
			
			col = TypeDescriptor.GetEvents (com);
			AssertEquals ("t2.1", 3, col.Count);
			Assert ("t2.2", col.Find ("AnEvent", true) != null);
			Assert ("t2.3", col.Find ("AnotherEvent", true) != null);
			Assert ("t2.4", col.Find ("Disposed", true) != null);
			
			col = TypeDescriptor.GetEvents (sitedcom);
			AssertEquals ("t3.1", 2, col.Count);
			Assert ("t3.2", col.Find ("AnotherEvent", true) != null);
			Assert ("t3.3", col.Find ("Disposed", true) != null);
			
			Attribute[] filter = new Attribute[] { new DescriptionAttribute ("test") };
			
			col = TypeDescriptor.GetEvents (typeof(MyComponent), filter);
			AssertEquals ("t4.1", 1, col.Count);
			Assert ("t4.2", col.Find ("AnEvent", true) != null);
			
			col = TypeDescriptor.GetEvents (com, filter);
			AssertEquals ("t5.1", 1, col.Count);
			Assert ("t5.2", col.Find ("AnEvent", true) != null);
			
			col = TypeDescriptor.GetEvents (sitedcom, filter);
			AssertEquals ("t6", 0, col.Count);
		}
		
		[Test]
		public void TestGetProperties ()
		{
			PropertyDescriptorCollection col = TypeDescriptor.GetProperties (typeof(MyComponent));
			Assert ("t1.1", col.Find ("TestProperty", true) != null);
			Assert ("t1.2", col.Find ("AnotherProperty", true) != null);
			
			col = TypeDescriptor.GetProperties (com);
			Assert ("t2.1", col.Find ("TestProperty", true) != null);
			Assert ("t2.2", col.Find ("AnotherProperty", true) != null);
			
			Attribute[] filter = new Attribute[] { new DescriptionAttribute ("test") };
			
			col = TypeDescriptor.GetProperties (typeof(MyComponent), filter);
			Assert ("t4.1", col.Find ("TestProperty", true) != null);
			Assert ("t4.2", col.Find ("AnotherProperty", true) == null);
			
			col = TypeDescriptor.GetProperties (com, filter);
			Assert ("t5.1", col.Find ("TestProperty", true) != null);
			Assert ("t5.2", col.Find ("AnotherProperty", true) == null);
			
		}

		[Test]
		[Ignore("Fails on .NET")]
		public void TestGetProperties2 ()
		{
			PropertyDescriptorCollection col = TypeDescriptor.GetProperties (sitedcom);
			Assert ("t3.1", col.Find ("TestProperty", true) == null);
			Assert ("t3.2", col.Find ("AnotherProperty", true) != null);

			Attribute[] filter = new Attribute[] { new DescriptionAttribute ("test") };
			col = TypeDescriptor.GetProperties (sitedcom, filter);
			Assert ("t6.1", col.Find ("TestProperty", true) == null);
			Assert ("t6.2", col.Find ("AnotherProperty", true) == null);
		}

	}
}

