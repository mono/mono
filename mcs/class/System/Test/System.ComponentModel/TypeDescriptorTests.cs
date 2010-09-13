//
// System.ComponentModel.TypeDescriptorTests test cases
//
// Authors:
// 	Lluis Sanchez Gual (lluis@ximian.com)
//
// (c) 2004 Novell, Inc. (http://www.ximian.com)
//

using System;
using System.Collections;
using System.ComponentModel;
using DescriptionAttribute = System.ComponentModel.DescriptionAttribute;
using System.ComponentModel.Design;
using System.Globalization;

using System.Collections.Generic;

using NUnit.Framework;

namespace MonoTests.System.ComponentModel
{
	interface IFoo
	{
		event EventHandler Fired;
		event EventHandler Closed;

		bool HasFired {
			get;
		}
	}

	interface IBar : IFoo
	{
		event EventHandler Destroyed;

		bool IsDestroyed {
			get;
		}
	}

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

	class MyOtherDesigner: IDesigner
	{
		public MyOtherDesigner()
		{
		}

		public IComponent Component {get {return null; } }
		public DesignerVerbCollection Verbs { get {return null; } }
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

	class AnotherSite: ISite
	{ 
		public IComponent Component { get {  return null; } }

		public IContainer Container { get {  return null; } }

		public bool DesignMode { get {  return true; } }

		public string Name { get { return "TestName"; } set { } }

		public object GetService (Type t)
		{
			if (t == typeof(ITypeDescriptorFilterService)) {
				return new AnotherFilter ();
			}
			return null;
		}
	}

	class NoFilterSite : ISite
	{
		public NoFilterSite () : this (null)
		{
		}

		public NoFilterSite (IContainer container)
		{
			_container = container;
		}

		public IComponent Component {
			get { return null; }
		}

		public IContainer Container {
			get { return _container; }
		}

		public bool DesignMode { get { return true; } }

		public string Name { get { return "TestName"; } set { } }

		public object GetService (Type t)
		{
			return null;
		}

		public IContainer _container;
	}

	class MyContainer : IContainer
	{
		public MyContainer ()
		{
			_components = new ComponentCollection (new IComponent [0]);
		}

		public ComponentCollection Components {
			get { return _components; }
		}

		public void Add (IComponent component)
		{
		}

		public void Add (IComponent component, string name)
		{
		}

		public void Dispose ()
		{
		}

		public void Remove (IComponent component)
		{
		}

		private ComponentCollection _components;
	}

	class AnotherFilter: ITypeDescriptorFilterService
	{
		public bool FilterAttributes (IComponent component,IDictionary attributes) {
			Attribute ea = new DefaultEventAttribute ("AnEvent");
			attributes [ea.TypeId] = ea;
			ea = new DefaultPropertyAttribute ("TestProperty");
			attributes [ea.TypeId] = ea;
			ea = new EditorAttribute ();
			attributes [ea.TypeId] = ea;
			return true;
		}

		public bool FilterEvents (IComponent component, IDictionary events) {
			return true;
		}

		public bool FilterProperties (IComponent component, IDictionary properties) {
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

		public MyComponent (IContainer container)
		{
			container.Add (this);
		}

		[DescriptionAttribute ("test")]
		public virtual string TestProperty
		{
			get { return prop; }
			set { prop = value; }
		}
		
		public string AnotherProperty
		{
			get { return prop; }
			set { prop = value; }
		}

		[Browsable (false)]
		public string YetAnotherProperty
		{
			get { return null; }
		}

		public string Name {
			get { return null; }
		}

		public string Address {
			get { return null; }
		}

		public string Country {
			get { return null; }
		}

		private string HairColor {
			get { return null; }
		}

		protected int Weight {
			get { return 5; }
		}

		internal int Height {
			get { return 0; }
		}

		public string WriteOnlyProperty {
			set { prop = value; }
		}
	}

	[DescriptionAttribute ("my test derived component")]
	[DesignerAttribute (typeof(MyOtherDesigner))]
	public class MyDerivedComponent: MyComponent
	{
		string prop;
		
		public MyDerivedComponent  ()
		{
		}
		
		public MyDerivedComponent (ISite site) : base (site)
		{
		}
		
		[DescriptionAttribute ("test derived")]
		public override string TestProperty
		{
			get { return prop; }
			set { prop = value; }
		}


		[DescriptionAttribute ("test derived")]
		public new string AnotherProperty
		{
			get { return base.AnotherProperty; }
			set { base.AnotherProperty = value; }
		}

		public new object YetAnotherProperty
		{
			get { return null; }
		}
	}
	

	[DefaultProperty("AnotherProperty")]
	[DefaultEvent("AnotherEvent")]
	[DescriptionAttribute ("my test component")]
	[DesignerAttribute (typeof(MyDesigner), typeof(int))]
	public class AnotherComponent: Component {
		string prop;
		
		[DescriptionAttribute ("test")]
		public event EventHandler AnEvent;
		
		public event EventHandler AnotherEvent;
		
		public AnotherComponent () {
		}
		
		public AnotherComponent (ISite site) {
			Site = site;
		}
		
		[DescriptionAttribute ("test")]
		public string TestProperty {
			get { return prop; }
			set { prop = value; }
		}
		
		public string AnotherProperty {
			get { return prop; }
			set { prop = value; }
		}
	}

	[Browsable (false)]
	public interface ITestInterface
	{
		void TestFunction ();
	}

	[DesignerAttribute (typeof(MyDesigner), typeof(int))]
	public class TestClass
	{
		public TestClass()
		{}

		void TestFunction ()
		{}
	}

	[DescriptionAttribute ("bla")]
	public class TestDerivedClass : TestClass, ITestInterface
	{
		public void TestFunction ()
		{}
	}

	public struct TestStruct
	{
		public int TestVal;
	}

	public class TestCustomTypeDescriptor : ICustomTypeDescriptor
	{
		public string methods_called = "";

		public void ResetMethodsCalled ()
		{
			methods_called = "";
		}

		public TypeConverter GetConverter()
		{
			return new StringConverter ();
		}

		public EventDescriptorCollection GetEvents(Attribute[] attributes)
		{
			methods_called += "1";
			return null;
		}

		public EventDescriptorCollection GetEvents()
		{
			methods_called += "2";
			return null;
		}

		public string GetComponentName()
		{
			return "MyComponentnName";
		}

		public object GetPropertyOwner(PropertyDescriptor pd)
		{
			return this;
		}

		public AttributeCollection GetAttributes()
		{
			methods_called += "3";
			return null;
		}

		public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
		{
			methods_called += "4";
			return new PropertyDescriptorCollection(new PropertyDescriptor[0]);
		}

		public PropertyDescriptorCollection GetProperties()
		{
			methods_called += "5";
			return new PropertyDescriptorCollection(new PropertyDescriptor[0]);
		}

		public object GetEditor(Type editorBaseType)
		{
			return null;
		}

		public PropertyDescriptor GetDefaultProperty()
		{
			methods_called += "6";
			return null;
		}

		public EventDescriptor GetDefaultEvent()
		{
			methods_called += "7";
			return null;
		}

		public string GetClassName()
		{
			return this.GetType ().Name;
		}
	}

	class MyCustomTypeDescriptor : CustomTypeDescriptor
	{
		public MyTypeDescriptionProvider Provider { get; private set; }

		public MyCustomTypeDescriptor (MyTypeDescriptionProvider provider)
		{
			Provider = provider;
		}

		public override string GetClassName ()
		{
			return Provider.Id;
		}
	}

	class MyTypeDescriptionProvider : TypeDescriptionProvider
	{
		public string Id { get; private set; }
		public bool CreateInstanceCalled { get; private set; }

		public MyTypeDescriptionProvider ()
			: this (null)
		{
		}

		public MyTypeDescriptionProvider (string id)
		{
			Id = id;
		}

		public override ICustomTypeDescriptor GetTypeDescriptor (Type objectType, object instance)
		{
			return new MyCustomTypeDescriptor (this);
		}

		public override object CreateInstance (IServiceProvider provider, Type objectType, Type[] argTypes, object[] args)
		{
			CreateInstanceCalled = true;
			return base.CreateInstance (provider, objectType, argTypes, args);
		}
	}

	[TestFixture]
	public class TypeDescriptorTests
	{
		MyComponent com = new MyComponent ();
		MyComponent sitedcom = new MyComponent (new MySite ());
		MyComponent nfscom = new MyComponent (new NoFilterSite (new MyContainer ()));
		AnotherComponent anothercom = new AnotherComponent ();
		
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestAddAttributes_Type_Attributes_1 ()
		{
			TypeDescriptionProvider provider = TypeDescriptor.AddAttributes ((Type) null, null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestAddAttributes_Type_Attributes_2 ()
		{
			TypeDescriptionProvider provider = TypeDescriptor.AddAttributes (typeof (string), null);
		}

		[Test]
		public void TestAddAttributes_Type_Attributes_3 ()
		{
			Attribute[] new_attributes = new Attribute[] {
				new ReadOnlyAttribute (true),
				new BindableAttribute (true)
			};

			TypeDescriptionProvider provider = null;
			ICustomTypeDescriptor descriptor;
			AttributeCollection attributes;

			try {
				provider = TypeDescriptor.AddAttributes (typeof (string), new Attribute[] { });
				Assert.IsNotNull (provider, "#A1");

				descriptor = provider.GetTypeDescriptor (typeof (string));
				Assert.IsNotNull (descriptor, "#A1-1");

				attributes = descriptor.GetAttributes ();
				Assert.IsNotNull (attributes, "#A1-2");
			} finally {
				if (provider != null)
					TypeDescriptor.RemoveProvider (provider, typeof (string));
			}

			provider = null;
			try {
				provider = TypeDescriptor.AddAttributes (typeof (string), new_attributes);
				Assert.IsNotNull (provider, "#B1");

				descriptor = provider.GetTypeDescriptor (typeof (string));
				Assert.IsNotNull (descriptor, "#B1-1");

				attributes = descriptor.GetAttributes ();
				Assert.IsNotNull (attributes, "#B1-2");
				Assert.AreNotEqual (0, attributes.Count, "#B1-3");
				Assert.IsTrue (attributes.Contains (new_attributes));
			} finally {
				if (provider != null)
					TypeDescriptor.RemoveProvider (provider, typeof (string));
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestAddAttributes_Instance_Attributes_1 ()
		{
			TypeDescriptionProvider provider = TypeDescriptor.AddAttributes ((object) null, null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestAddAttributes_Instance_Attributes_2 ()
		{
			string s = "test";
			TypeDescriptionProvider provider = TypeDescriptor.AddAttributes (s, null);
		}

		[Test]
		public void TestAddAttributes_Instance_Attributes_3 ()
		{
			Attribute[] new_attributes = new Attribute[] {
				new ReadOnlyAttribute (true),
				new BindableAttribute (true)
			};

			TypeDescriptionProvider provider = null;
			ICustomTypeDescriptor descriptor;
			AttributeCollection attributes;
			string s = "test";

			try {
				provider = TypeDescriptor.AddAttributes (s, new Attribute[] { });
				Assert.IsNotNull (provider, "#A1");

				descriptor = provider.GetTypeDescriptor (s);
				Assert.IsNotNull (descriptor, "#A1-1");

				attributes = descriptor.GetAttributes ();
				Assert.IsNotNull (attributes, "#A1-2");
			} finally {
				if (provider != null)
					TypeDescriptor.RemoveProvider (provider, s);
			}

			provider = null;
			try {
				provider = TypeDescriptor.AddAttributes (s, new_attributes);
				Assert.IsNotNull (provider, "#B1");

				descriptor = provider.GetTypeDescriptor (s);
				Assert.IsNotNull (descriptor, "#B1-1");

				attributes = descriptor.GetAttributes ();
				Assert.IsNotNull (attributes, "#B1-2");
				Assert.AreNotEqual (0, attributes.Count, "#B1-3");
				Assert.IsTrue (attributes.Contains (new_attributes));
			} finally {
				if (provider != null)
					TypeDescriptor.RemoveProvider (provider, s);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestAddProvider_Provider_Instance_1 ()
		{
			TypeDescriptor.AddProvider (null, (object)null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestAddProvider_Provider_Instance_2 ()
		{
			var provider = new MyTypeDescriptionProvider ();
			TypeDescriptor.AddProvider (provider, (object) null);
		}

		[Test]
		public void TestAddProvider_Provider_Instance_3 ()
		{
			var instance = new MyComponent ();
			var providers = new MyTypeDescriptionProvider[] {
				new MyTypeDescriptionProvider ("One"),
				new MyTypeDescriptionProvider ("Two"),
				new MyTypeDescriptionProvider ("Three"),
				new MyTypeDescriptionProvider ("Four")
			};

			try {
				TypeDescriptionProvider provider;
				ICustomTypeDescriptor descriptor;

				TypeDescriptor.AddProvider (providers[0], instance);
				provider = TypeDescriptor.GetProvider (instance);
				Assert.IsNotNull (provider, "#A1");
				descriptor = provider.GetTypeDescriptor (instance.GetType (), instance);
				Assert.IsNotNull (descriptor, "#A1-1");
				Assert.AreEqual ("One", descriptor.GetClassName (), "#A1-2");
				Assert.AreEqual (false, providers[0].CreateInstanceCalled, "#A1-3");

				descriptor.GetProperties ();

				TypeDescriptor.AddProvider (providers[1], instance);
				provider = TypeDescriptor.GetProvider (instance);
				Assert.IsNotNull (provider, "#B1");
				descriptor = provider.GetTypeDescriptor (instance.GetType (), instance);
				Assert.IsNotNull (descriptor, "#B1-1");
				Assert.AreEqual ("Two", descriptor.GetClassName (), "#B1-2");

				// Providers are stored in a stack according to docs, but it's in reality
				// a FIFO linked list
				TypeDescriptor.AddProvider (providers[2], instance);
				TypeDescriptor.AddProvider (providers[3], instance);
				provider = TypeDescriptor.GetProvider (instance);
				Assert.IsNotNull (provider, "#C1");
				descriptor = provider.GetTypeDescriptor (instance.GetType (), instance);
				Assert.IsNotNull (descriptor, "#C1-1");
				Assert.AreEqual ("Four", descriptor.GetClassName (), "#C1-2");

				TypeDescriptor.RemoveProvider (providers[2], instance);
				provider = TypeDescriptor.GetProvider (instance);
				Assert.IsNotNull (provider, "#D1");
				descriptor = provider.GetTypeDescriptor (instance.GetType (), instance);
				Assert.IsNotNull (descriptor, "#D1-1");
				Assert.AreEqual ("Four", descriptor.GetClassName (), "#D1-2");

				TypeDescriptor.RemoveProvider (providers[3], instance);
				provider = TypeDescriptor.GetProvider (instance);
				Assert.IsNotNull (provider, "#E1");
				descriptor = provider.GetTypeDescriptor (instance.GetType (), instance);
				Assert.IsNotNull (descriptor, "#E1-1");
				Assert.AreEqual ("Two", descriptor.GetClassName (), "#E1-2");

			} finally {
				TypeDescriptor.RemoveProvider (providers[0], instance);
				TypeDescriptor.RemoveProvider (providers[1], instance);
				TypeDescriptor.RemoveProvider (providers[2], instance);
				TypeDescriptor.RemoveProvider (providers[3], instance);
			}
		}

		[Test]
		public void TestAddProvider_Provider_Instance_4 ()
		{
			var instance = new MyComponent ();
			var providers = new MyTypeDescriptionProvider[] {
				new MyTypeDescriptionProvider ("One"),
				new MyTypeDescriptionProvider ("Two"),
				new MyTypeDescriptionProvider ("Three"),
				new MyTypeDescriptionProvider ("Four")
			};

			try {
				TypeDescriptionProvider provider;
				ICustomTypeDescriptor descriptor;

				TypeDescriptor.AddProvider (providers[0], instance);
				provider = TypeDescriptor.GetProvider (instance);
				Assert.IsNotNull (provider, "#A1");
				descriptor = provider.GetTypeDescriptor (instance.GetType (), instance);
				Assert.IsNotNull (descriptor, "#A1-1");
				Assert.AreEqual ("One", descriptor.GetClassName (), "#A1-2");
				Assert.AreEqual (false, providers[0].CreateInstanceCalled, "#A1-3");

				descriptor.GetProperties ();

				TypeDescriptor.AddProvider (providers[1], instance);
				provider = TypeDescriptor.GetProvider (instance);
				Assert.IsNotNull (provider, "#B1");
				descriptor = provider.GetTypeDescriptor (instance.GetType (), instance);
				Assert.IsNotNull (descriptor, "#B1-1");
				Assert.AreEqual ("Two", descriptor.GetClassName (), "#B1-2");

				// Providers are stored in a stack according to docs, but it's in reality
				// a FIFO linked list
				TypeDescriptor.AddProvider (providers[0], instance);
				TypeDescriptor.AddProvider (providers[0], instance);
				provider = TypeDescriptor.GetProvider (instance);
				Assert.IsNotNull (provider, "#C1");
				descriptor = provider.GetTypeDescriptor (instance.GetType (), instance);
				Assert.IsNotNull (descriptor, "#C1-1");
				Assert.AreEqual ("One", descriptor.GetClassName (), "#C1-2");

				TypeDescriptor.RemoveProvider (providers[0], instance);
				provider = TypeDescriptor.GetProvider (instance);
				Assert.IsNotNull (provider, "#D1");
				descriptor = provider.GetTypeDescriptor (instance.GetType (), instance);
				Assert.IsNotNull (descriptor, "#D1-1");
				Assert.AreEqual ("One", descriptor.GetClassName (), "#D1-2");

				TypeDescriptor.RemoveProvider (providers[0], instance);
				provider = TypeDescriptor.GetProvider (instance);
				Assert.IsNotNull (provider, "#E1");
				descriptor = provider.GetTypeDescriptor (instance.GetType (), instance);
				Assert.IsNotNull (descriptor, "#E1-1");
				Assert.AreEqual ("Two", descriptor.GetClassName (), "#E1-2");

			} finally {
				TypeDescriptor.RemoveProvider (providers[0], instance);
				TypeDescriptor.RemoveProvider (providers[1], instance);
				TypeDescriptor.RemoveProvider (providers[2], instance);
				TypeDescriptor.RemoveProvider (providers[3], instance);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestAddProvider_Provider_Type_1 ()
		{
			TypeDescriptor.AddProvider (null, (Type) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestAddProvider_Provider_Type_2 ()
		{
			var provider = new MyTypeDescriptionProvider ();
			TypeDescriptor.AddProvider (provider, (Type) null);
		}

		[Test]
		public void TestAddProvider_Provider_Type_3 ()
		{
			var type = typeof (MyComponent);
			var providers = new MyTypeDescriptionProvider[] {
				new MyTypeDescriptionProvider ("One"),
				new MyTypeDescriptionProvider ("Two"),
				new MyTypeDescriptionProvider ("Three"),
				new MyTypeDescriptionProvider ("Four")
			};

			try {
				TypeDescriptionProvider provider;
				ICustomTypeDescriptor descriptor;

				TypeDescriptor.AddProvider (providers[0], type);
				provider = TypeDescriptor.GetProvider (type);
				Assert.IsNotNull (provider, "#A1");
				descriptor = provider.GetTypeDescriptor (type);
				Assert.IsNotNull (descriptor, "#A1-1");
				Assert.AreEqual ("One", descriptor.GetClassName (), "#A1-2");
				Assert.AreEqual (false, providers[0].CreateInstanceCalled, "#A1-3");

				TypeDescriptor.AddProvider (providers[1], type);
				provider = TypeDescriptor.GetProvider (type);
				Assert.IsNotNull (provider, "#B1");
				descriptor = provider.GetTypeDescriptor (type.GetType (), type);
				Assert.IsNotNull (descriptor, "#B1-1");
				Assert.AreEqual ("Two", descriptor.GetClassName (), "#B1-2");

				// Providers are stored in a stack according to docs, but it's in reality
				// a FIFO linked list
				TypeDescriptor.AddProvider (providers[2], type);
				TypeDescriptor.AddProvider (providers[3], type);
				provider = TypeDescriptor.GetProvider (type);
				Assert.IsNotNull (provider, "#C1");
				descriptor = provider.GetTypeDescriptor (type.GetType (), type);
				Assert.IsNotNull (descriptor, "#C1-1");
				Assert.AreEqual ("Four", descriptor.GetClassName (), "#C1-2");

				TypeDescriptor.RemoveProvider (providers[2], type);
				provider = TypeDescriptor.GetProvider (type);
				Assert.IsNotNull (provider, "#D1");
				descriptor = provider.GetTypeDescriptor (type.GetType (), type);
				Assert.IsNotNull (descriptor, "#D1-1");
				Assert.AreEqual ("Four", descriptor.GetClassName (), "#D1-2");

				TypeDescriptor.RemoveProvider (providers[3], type);
				provider = TypeDescriptor.GetProvider (type);
				Assert.IsNotNull (provider, "#E1");
				descriptor = provider.GetTypeDescriptor (type.GetType (), type);
				Assert.IsNotNull (descriptor, "#E1-1");
				Assert.AreEqual ("Two", descriptor.GetClassName (), "#E1-2");

			} finally {
				TypeDescriptor.RemoveProvider (providers[0], type);
				TypeDescriptor.RemoveProvider (providers[1], type);
				TypeDescriptor.RemoveProvider (providers[2], type);
				TypeDescriptor.RemoveProvider (providers[3], type);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestGetProvider_Type_1 ()
		{
			TypeDescriptor.GetProvider ((Type)null);
		}

		[Test]
		public void TestGetProvider_Type_2 ()
		{
			TypeDescriptionProvider provider = TypeDescriptor.GetProvider (typeof (string));
			Assert.IsNotNull (provider, "#A1");
			provider = new MyTypeDescriptionProvider ("One");

			try {
				TypeDescriptor.AddProvider (provider, typeof (string));
				ICustomTypeDescriptor descriptor = provider.GetTypeDescriptor (typeof (string));
				Assert.IsNotNull (descriptor, "#B1");
				Assert.AreEqual ("One", descriptor.GetClassName (), "#B1-1");
			} finally {
				TypeDescriptor.RemoveProvider (provider, typeof (string));
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestGetProvider_Instance_1 ()
		{
			TypeDescriptor.GetProvider ((object) null);
		}

		[Test]
		public void TestGetProvider_Instance_2 ()
		{
			var instance = new MyComponent ();
			TypeDescriptionProvider provider = TypeDescriptor.GetProvider (instance);
			Assert.IsNotNull (provider, "#A1");
			provider = new MyTypeDescriptionProvider ("One");

			try {
				TypeDescriptor.AddProvider (provider, instance);
				ICustomTypeDescriptor descriptor = provider.GetTypeDescriptor (instance);
				Assert.IsNotNull (descriptor, "#B1");
				Assert.AreEqual ("One", descriptor.GetClassName (), "#B1-1");
			} finally {
				TypeDescriptor.RemoveProvider (provider, instance);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestRemoveProvider_Provider_Type_1 ()
		{
			TypeDescriptor.RemoveProvider (null, (Type)null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestRemoveProvider_Provider_Type_2 ()
		{
			var provider = new MyTypeDescriptionProvider ();
			TypeDescriptor.RemoveProvider (provider, null);
		}

		[Test]
		public void TestRemoveProvider_Provider_Type_3 ()
		{
			var provider = new MyTypeDescriptionProvider ();
			bool refreshedCalled = false;
			bool refreshedCorrectComponentChanged = false;
			bool refreshedCorrectTypeChanged = false;

			RefreshEventHandler handler = (RefreshEventArgs args) => {
				refreshedCalled = true;
				refreshedCorrectComponentChanged = args.ComponentChanged == null;
				refreshedCorrectTypeChanged = args.TypeChanged == typeof (string);
			};

			try {
				TypeDescriptor.Refreshed += handler;

				TypeDescriptor.RemoveProvider (provider, typeof (string));
				Assert.AreEqual (true, refreshedCalled, "#A1");
				Assert.AreEqual (true, refreshedCorrectComponentChanged, "#A2");
				Assert.AreEqual (true, refreshedCorrectTypeChanged, "#A3");
			} finally {
				TypeDescriptor.Refreshed -= handler;
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestRemoveProvider_Provider_Instance_1 ()
		{
			TypeDescriptor.RemoveProvider (null, (object)null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestRemoveProvider_Provider_Instance_2 ()
		{
			var provider = new MyTypeDescriptionProvider ();
			TypeDescriptor.RemoveProvider (provider, (object)null);
		}

		[Test]
		public void TestRemoveProvider_Provider_Instance_3 ()
		{
			var instance = new MyComponent ();
			var provider = new MyTypeDescriptionProvider ();
			bool refreshedCalled = false;
			bool refreshedCorrectComponentChanged = false;
			bool refreshedCorrectTypeChanged = false;

			RefreshEventHandler handler = (RefreshEventArgs args) => {
				refreshedCalled = true;
				refreshedCorrectComponentChanged = args.ComponentChanged == instance;
				refreshedCorrectTypeChanged = args.TypeChanged == typeof (MyComponent);
			};

			try {
				TypeDescriptor.Refreshed += handler;

				TypeDescriptor.RemoveProvider (provider, instance);
				Assert.AreEqual (true, refreshedCalled, "#A1");
				Assert.AreEqual (true, refreshedCorrectComponentChanged, "#A2");
				Assert.AreEqual (true, refreshedCorrectTypeChanged, "#A3");
			} finally {
				TypeDescriptor.Refreshed -= handler;
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestGetReflectionType_Type_1 ()
		{
			TypeDescriptor.GetReflectionType ((Type) null);
		}

		[Test]
		public void TestGetReflectionType_Type_2 ()
		{
			Type type = TypeDescriptor.GetReflectionType (typeof (string));
			Assert.IsNotNull (type, "#A1");
			Assert.AreEqual (typeof (string), type, "#A1-1");

			type = TypeDescriptor.GetReflectionType (typeof (MyComponent));
			Assert.IsNotNull (type, "#B1");
			Assert.AreEqual (typeof (MyComponent), type, "#B1-1");

			type = TypeDescriptor.GetReflectionType (typeof (List<string>));
			Assert.IsNotNull (type, "#C1");
			Assert.AreEqual (typeof (List <string>), type, "#C1-1");

			type = TypeDescriptor.GetReflectionType (typeof (IList<>));
			Assert.IsNotNull (type, "#D1");
			Assert.AreEqual (typeof (IList<>), type, "#D1-1");

			type = TypeDescriptor.GetReflectionType (typeof (IDictionary<,>));
			Assert.IsNotNull (type, "#E1");
			Assert.AreEqual (typeof (IDictionary<,>), type, "#E1-1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestGetReflectionType_Instance_1 ()
		{
			TypeDescriptor.GetReflectionType ((object) null);
		}

		[Test]
		public void TestGetReflectionType_Instance_2 ()
		{
			string s = "string";
			Type type = TypeDescriptor.GetReflectionType (s);
			Assert.IsNotNull (type, "#A1");
			Assert.AreEqual (typeof (string), type, "#A1-1");

			var mc = new MyComponent ();
			type = TypeDescriptor.GetReflectionType (mc);
			Assert.IsNotNull (type, "#B1");
			Assert.AreEqual (typeof (MyComponent), type, "#B1-1");

			var l = new List<string> ();
			type = TypeDescriptor.GetReflectionType (l);
			Assert.IsNotNull (type, "#C1");
			Assert.AreEqual (typeof (List<string>), type, "#C1-1");

			IList il = new List<string> ();
			type = TypeDescriptor.GetReflectionType (il);
			Assert.IsNotNull (type, "#D1");
			Assert.AreEqual (typeof (List<string>), type, "#D1-1");

			IDictionary id = new Dictionary<string, object> ();
			type = TypeDescriptor.GetReflectionType (id);
			Assert.IsNotNull (type, "#E1");
			Assert.AreEqual (typeof (Dictionary<string,object>), type, "#E1-1");

			object o = 1;
			type = TypeDescriptor.GetReflectionType (o);
			Assert.IsNotNull (type, "#F1");
			Assert.AreEqual (typeof (int), type, "#F1-1");
		}

		[Test]
		public void TestICustomTypeDescriptor ()
		{
			TestCustomTypeDescriptor test = new TestCustomTypeDescriptor ();

			PropertyDescriptorCollection props;
			PropertyDescriptor prop;
			EventDescriptorCollection events;

			test.ResetMethodsCalled ();
			props = TypeDescriptor.GetProperties (test);
			Assert.AreEqual ("5", test.methods_called, "#1");

			test.ResetMethodsCalled ();
			props = TypeDescriptor.GetProperties (test, new Attribute[0]);
			Assert.AreEqual ("4", test.methods_called, "#2");

			test.ResetMethodsCalled ();
			props = TypeDescriptor.GetProperties (test, new Attribute[0], false);
			Assert.AreEqual ("4", test.methods_called, "#3");

			test.ResetMethodsCalled ();
			props = TypeDescriptor.GetProperties (test, false);
			Assert.AreEqual ("5", test.methods_called, "#4");

			test.ResetMethodsCalled ();
			prop = TypeDescriptor.GetDefaultProperty (test);
			Assert.AreEqual ("6", test.methods_called, "#5");

			test.ResetMethodsCalled ();
			events = TypeDescriptor.GetEvents (test);
			Assert.AreEqual ("2", test.methods_called, "#6");

			test.ResetMethodsCalled ();
			events = TypeDescriptor.GetEvents (test, new Attribute[0]);
			Assert.AreEqual ("1", test.methods_called, "#7");

			test.ResetMethodsCalled ();
			events = TypeDescriptor.GetEvents (test, false);
			Assert.AreEqual ("2", test.methods_called, "#8");
		}

		[Test]
		public void TestCreateDesigner ()
		{
			IDesigner des = TypeDescriptor.CreateDesigner (com, typeof(int));
			Assert.IsTrue (des is MyDesigner, "#1");
			
			des = TypeDescriptor.CreateDesigner (com, typeof(string));
			Assert.IsNull (des, "#2");
		}
		
		[Test]
		public void TestCreateEvent ()
		{
			EventDescriptor ed = TypeDescriptor.CreateEvent (typeof(MyComponent), "AnEvent", typeof(EventHandler), null);
			Assert.AreEqual (typeof (MyComponent), ed.ComponentType, "#1");
			Assert.AreEqual (typeof (EventHandler), ed.EventType, "#2");
			Assert.IsTrue (ed.IsMulticast, "#3");
			Assert.AreEqual ("AnEvent", ed.Name, "#4");
		}
		
		[Test]
		public void TestCreateProperty ()
		{
			PropertyDescriptor pd = TypeDescriptor.CreateProperty (typeof(MyComponent), "TestProperty", typeof(string), null);
			Assert.AreEqual (typeof (MyComponent), pd.ComponentType, "#1");
			Assert.AreEqual ("TestProperty", pd.Name, "#2");
			Assert.AreEqual (typeof (string), pd.PropertyType, "#3");
			Assert.IsFalse (pd.IsReadOnly, "#4");
			
			pd.SetValue (com, "hi");
			Assert.AreEqual ("hi", pd.GetValue (com), "#5");
		}
		
		[Test]
		public void TestGetAttributes ()
		{
			AttributeCollection col = TypeDescriptor.GetAttributes (typeof(MyComponent));
			Assert.IsNotNull (col [typeof (DescriptionAttribute)], "#A1");
			Assert.IsNotNull (col [typeof (DesignerAttribute)], "#A2");
			Assert.IsNull (col [typeof (EditorAttribute)], "#A3");
			
			col = TypeDescriptor.GetAttributes (com);
			Assert.IsNotNull (col [typeof (DescriptionAttribute)], "#B1");
			Assert.IsNotNull (col [typeof (DesignerAttribute)], "#B2");
			Assert.IsNull (col [typeof (EditorAttribute)], "#B3");
			
			col = TypeDescriptor.GetAttributes (sitedcom);
			Assert.IsNotNull (col [typeof (DescriptionAttribute)], "#C1");
			Assert.IsNotNull (col [typeof (DesignerAttribute)], "#C2");
			Assert.IsNotNull (col [typeof (EditorAttribute)], "#C3");

			col = TypeDescriptor.GetAttributes (nfscom);
			Assert.IsNotNull (col [typeof (DescriptionAttribute)], "#D1");
			Assert.IsNotNull (col [typeof (DesignerAttribute)], "#D2");
			Assert.IsNull (col [typeof (EditorAttribute)], "#D3");

			col = TypeDescriptor.GetAttributes (typeof (MyDerivedComponent));
			Assert.IsNotNull (col [typeof (DesignerAttribute)], "#E1");
			Assert.IsNotNull (col [typeof (DescriptionAttribute)], "#E2");
			DesignerAttribute attribute = col[typeof(DesignerAttribute)] as DesignerAttribute;
			Assert.IsNotNull (attribute, "#E3");
			// there are multiple DesignerAttribute present and their order in the collection isn't deterministic
			bool found = false;
			for (int i = 0; i < col.Count; i++) {
				attribute = (col [i] as DesignerAttribute);
				if (attribute != null) {
					found = typeof(MyOtherDesigner).AssemblyQualifiedName == attribute.DesignerTypeName;
					if (found)
						break;
				}
			}
			Assert.IsTrue (found, "#E4");

			// Shows that attributes are retrieved from the current type, the base types 
			// and the implemented by the type interfaces.
			Assert.AreEqual (3, TypeDescriptor.GetAttributes (typeof (TestDerivedClass)).Count, "#F1");
		}
		
		[Test]
		public void TestGetClassName ()
		{
			Assert.AreEqual (typeof(MyComponent).FullName, TypeDescriptor.GetClassName (com));
		}
		
		[Test]
		public void TestGetComponentName ()
		{
			// in MS.NET 2.0, GetComponentName no longer returns
			// the type name if there's no custom typedescriptor
			// and no site
			Assert.IsNull (TypeDescriptor.GetComponentName (com), "#1");
			Assert.IsNull (TypeDescriptor.GetComponentName (com, false), "#2");
			Assert.IsNull (TypeDescriptor.GetComponentName (new Exception ()), "#3");
			Assert.IsNull (TypeDescriptor.GetComponentName (new Exception (), false), "#4");
			Assert.IsNull (TypeDescriptor.GetComponentName (typeof (Exception)), "#4");
			Assert.IsNull (TypeDescriptor.GetComponentName (typeof (Exception), false), "#6");
			Assert.AreEqual ("TestName", TypeDescriptor.GetComponentName (sitedcom), "#7");
			Assert.AreEqual ("TestName", TypeDescriptor.GetComponentName (sitedcom), "#8");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestGetConverterNullParam ()
		{
			TypeDescriptor.GetConverter (null);
		}
		
		[Test]
		public void TestGetConverter ()
		{
			Assert.AreEqual (typeof (BooleanConverter), TypeDescriptor.GetConverter (typeof (bool)).GetType (), "#1");
			Assert.AreEqual (typeof (ByteConverter), TypeDescriptor.GetConverter (typeof (byte)).GetType (), "#2");
			Assert.AreEqual (typeof (SByteConverter), TypeDescriptor.GetConverter (typeof (sbyte)).GetType (), "#3");
			Assert.AreEqual (typeof (StringConverter), TypeDescriptor.GetConverter (typeof (string)).GetType (), "#4");
			Assert.AreEqual (typeof (CharConverter), TypeDescriptor.GetConverter (typeof (char)).GetType (), "#5");
			Assert.AreEqual (typeof (Int16Converter), TypeDescriptor.GetConverter (typeof (short)).GetType (), "#6");
			Assert.AreEqual (typeof (Int32Converter), TypeDescriptor.GetConverter (typeof (int)).GetType (), "#7");
			Assert.AreEqual (typeof (Int64Converter), TypeDescriptor.GetConverter (typeof (long)).GetType (), "#8");
			Assert.AreEqual (typeof (UInt16Converter), TypeDescriptor.GetConverter (typeof (ushort)).GetType (), "#9");
			Assert.AreEqual (typeof (UInt32Converter), TypeDescriptor.GetConverter (typeof (uint)).GetType (), "#10");
			Assert.AreEqual (typeof (UInt64Converter), TypeDescriptor.GetConverter (typeof (ulong)).GetType (), "#11");
			Assert.AreEqual (typeof (SingleConverter), TypeDescriptor.GetConverter (typeof (float)).GetType (), "#12");
			Assert.AreEqual (typeof (DoubleConverter), TypeDescriptor.GetConverter (typeof (double)).GetType (), "#13");
			Assert.AreEqual (typeof (DecimalConverter), TypeDescriptor.GetConverter (typeof (decimal)).GetType (), "#14");
			Assert.AreEqual (typeof (ArrayConverter), TypeDescriptor.GetConverter (typeof (Array)).GetType (), "#15");
			Assert.AreEqual (typeof (CultureInfoConverter), TypeDescriptor.GetConverter (typeof (CultureInfo)).GetType (), "#16");
			Assert.AreEqual (typeof (DateTimeConverter), TypeDescriptor.GetConverter (typeof (DateTime)).GetType (), "#17");
			Assert.AreEqual (typeof (GuidConverter), TypeDescriptor.GetConverter (typeof (Guid)).GetType (), "#18");
			Assert.AreEqual (typeof (TimeSpanConverter), TypeDescriptor.GetConverter (typeof (TimeSpan)).GetType (), "#19");
			Assert.AreEqual (typeof (CollectionConverter), TypeDescriptor.GetConverter (typeof (ICollection)).GetType (), "#20");

			// Tests from bug #71444
			Assert.AreEqual (typeof (CollectionConverter), TypeDescriptor.GetConverter (typeof (IDictionary)).GetType (), "#21");
			Assert.AreEqual (typeof (ReferenceConverter), TypeDescriptor.GetConverter (typeof (ITestInterface)).GetType (), "#22");
			Assert.AreEqual (typeof (TypeConverter), TypeDescriptor.GetConverter (typeof (TestClass)).GetType (), "#23");
			Assert.AreEqual (typeof (TypeConverter), TypeDescriptor.GetConverter (typeof (TestStruct)).GetType (), "#24");

			Assert.AreEqual (typeof (TypeConverter), TypeDescriptor.GetConverter (new TestClass ()).GetType (), "#25");
			Assert.AreEqual (typeof (TypeConverter), TypeDescriptor.GetConverter (new TestStruct ()).GetType (), "#26");
			Assert.AreEqual (typeof (CollectionConverter), TypeDescriptor.GetConverter (new Hashtable ()).GetType (), "#27");

			// Test from bug #76686
			Assert.AreEqual  (typeof (Int32Converter), TypeDescriptor.GetConverter ((int?) 1).GetType (), "#28");
			Assert.IsTrue (TypeDescriptor.GetConverter (typeof (Component)) is ComponentConverter, "#29");
			Assert.IsTrue (TypeDescriptor.GetConverter (new Component()) is ComponentConverter, "#30");

			Assert.AreEqual (typeof (NullableConverter), TypeDescriptor.GetConverter (typeof (int?)).GetType (), "#31");
		}
		
		[Test]
		public void TestGetDefaultEvent ()
		{
			EventDescriptor des = TypeDescriptor.GetDefaultEvent (typeof(MyComponent));
			Assert.IsNull ( des, "#A");
			
			des = TypeDescriptor.GetDefaultEvent (com);
			Assert.IsNull (des, "#B");

			des = TypeDescriptor.GetDefaultEvent (typeof(AnotherComponent));
			Assert.IsNotNull (des, "#C1");
			Assert.AreEqual ("AnotherEvent", des.Name, "#C2");

			des = TypeDescriptor.GetDefaultEvent (anothercom);
			Assert.IsNotNull (des, "#D1");
			Assert.AreEqual ("AnotherEvent", des.Name, "#D2");

			des = TypeDescriptor.GetDefaultEvent (sitedcom);
			Assert.IsNull (des, "#E1");

			des = TypeDescriptor.GetDefaultEvent (new MyComponent(new AnotherSite ()));
			Assert.IsNotNull (des, "#F1");
			Assert.AreEqual ("AnEvent", des.Name, "#F2");

			des = TypeDescriptor.GetDefaultEvent (new AnotherComponent(new AnotherSite ()));
			Assert.IsNotNull (des, "#G1");
			Assert.AreEqual ("AnEvent", des.Name, "#G2");
		}
		
		[Test]
		public void TestGetDefaultProperty ()
		{
			PropertyDescriptor des = TypeDescriptor.GetDefaultProperty (typeof(MyComponent));
			Assert.IsNull (des, "#A");
			
			des = TypeDescriptor.GetDefaultProperty (com);
			Assert.IsNull (des, "#B");

			des = TypeDescriptor.GetDefaultProperty (typeof(AnotherComponent));
			Assert.IsNotNull (des, "#C1");
			Assert.AreEqual ("AnotherProperty", des.Name, "#C2");

			des = TypeDescriptor.GetDefaultProperty (anothercom);
			Assert.IsNotNull (des, "#D1");
			Assert.AreEqual ("AnotherProperty", des.Name, "#D2");
		}
		
		[Test]
		public void TestGetDefaultProperty2 ()
		{
			PropertyDescriptor des = TypeDescriptor.GetDefaultProperty (sitedcom);
			Assert.IsNull (des, "#A");

			des = TypeDescriptor.GetDefaultProperty (new MyComponent (new AnotherSite ()));
			Assert.IsNotNull (des, "#B1");
			Assert.AreEqual ("TestProperty", des.Name, "#B2");

			des = TypeDescriptor.GetDefaultProperty (new AnotherComponent (new AnotherSite ()));
			Assert.IsNotNull (des, "#C1");
			Assert.AreEqual ("TestProperty", des.Name, "#C2");

			des = TypeDescriptor.GetDefaultProperty (new AnotherComponent (new MySite ()));
			Assert.IsNull (des, "#D");
		}

		[Test]
		public void TestGetEvents ()
		{
			EventDescriptorCollection col = TypeDescriptor.GetEvents (typeof(MyComponent));
			Assert.AreEqual (3, col.Count, "#A1");
			Assert.IsNotNull (col.Find ("AnEvent", true), "#A2");
			Assert.IsNotNull (col.Find ("AnotherEvent", true), "#A3");
			Assert.IsNotNull (col.Find ("Disposed", true), "#A4");
			
			col = TypeDescriptor.GetEvents (com);
			Assert.AreEqual (3, col.Count, "#B1");
			Assert.IsNotNull (col.Find ("AnEvent", true), "#B2");
			Assert.IsNotNull (col.Find ("AnotherEvent", true), "#B3");
			Assert.IsNotNull (col.Find ("Disposed", true), "#B4");
			
			col = TypeDescriptor.GetEvents (sitedcom);
			Assert.AreEqual (2, col.Count, "#C1");
			Assert.IsNotNull (col.Find ("AnotherEvent", true), "#C2");
			Assert.IsNotNull (col.Find ("Disposed", true), "#C3");

			col = TypeDescriptor.GetEvents (nfscom);
			Assert.AreEqual (3, col.Count, "#D1");
			Assert.IsNotNull (col.Find ("AnEvent", true), "#D2");
			Assert.IsNotNull ( col.Find ("AnotherEvent", true), "#D3");
			Assert.IsNotNull (col.Find ("Disposed", true), "#D4");

			Attribute[] filter = new Attribute[] { new DescriptionAttribute ("test") };
			
			col = TypeDescriptor.GetEvents (typeof(MyComponent), filter);
			Assert.AreEqual (1, col.Count, "#E1");
			Assert.IsNotNull (col.Find ("AnEvent", true), "#E2");
			
			col = TypeDescriptor.GetEvents (com, filter);
			Assert.AreEqual (1, col.Count, "#F1");
			Assert.IsNotNull (col.Find ("AnEvent", true), "#F2");
			
			col = TypeDescriptor.GetEvents (sitedcom, filter);
			Assert.AreEqual (0, col.Count, "#G");

			col = TypeDescriptor.GetEvents (nfscom, filter);
			Assert.AreEqual (1, col.Count, "#H1");
			Assert.IsNotNull (col.Find ("AnEvent", true), "#H2");

			col = TypeDescriptor.GetEvents (typeof (IFoo));
			Assert.AreEqual (2, col.Count, "#I1");
			Assert.IsNotNull (col.Find ("Fired", true), "#I2");
			Assert.IsNotNull (col.Find ("Fired", false), "#I3");
			Assert.IsNotNull (col.Find ("fired", true), "#I4");
			Assert.IsNull (col.Find ("fired", false), "#I5");
			Assert.IsNotNull (col.Find ("Closed", true), "#I6");

			col = TypeDescriptor.GetEvents (typeof (IBar));
			Assert.AreEqual (1, col.Count, "#J1");
			Assert.IsNull (col.Find ("Fired", true), "#J2");
			Assert.IsNull (col.Find ("Closed", true), "#J3");
			Assert.IsNotNull (col.Find ("Destroyed", true), "#J4");
		}
		
		[Test]
		public void TestGetProperties ()
		{
			PropertyDescriptorCollection col = TypeDescriptor.GetProperties (typeof(MyComponent));
			Assert.IsNotNull (col.Find ("TestProperty", true), "#A1");
			Assert.IsNotNull ( col.Find ("AnotherProperty", true), "#A2");
			
			col = TypeDescriptor.GetProperties (com);
			Assert.IsNotNull (col.Find ("TestProperty", true), "#B1");
			Assert.IsNotNull (col.Find ("AnotherProperty", true), "#B2");

			col = TypeDescriptor.GetProperties (nfscom);
			Assert.IsNotNull (col.Find ("TestProperty", true), "#C1");
			Assert.IsNotNull (col.Find ("AnotherProperty", true), "#C2");

			Attribute[] filter = new Attribute[] { new DescriptionAttribute ("test") };
			
			col = TypeDescriptor.GetProperties (typeof(MyComponent), filter);
			Assert.IsNotNull (col.Find ("TestProperty", true), "#D1");
			Assert.IsNull (col.Find ("AnotherProperty", true), "#D2");
			
			col = TypeDescriptor.GetProperties (com, filter);
			Assert.IsNotNull (col.Find ("TestProperty", true), "#E1");
			Assert.IsNull (col.Find ("AnotherProperty", true), "#E2");

			col = TypeDescriptor.GetProperties (nfscom, filter);
			Assert.IsNotNull (col.Find ("TestProperty", true), "#F1");
			Assert.IsNull (col.Find ("AnotherProperty", true), "#F2");


			// GetProperties should return only the last type's implementation of a
			// property with a matching name in the base types. E.g in the case where 
			// the "new" keyword is used.
			//
			PropertyDescriptorCollection derivedCol = TypeDescriptor.GetProperties (typeof(MyDerivedComponent));
			Assert.IsNotNull (derivedCol["AnotherProperty"].Attributes[typeof (DescriptionAttribute)], "#G1");
			int anotherPropsFound = 0;
			int yetAnotherPropsFound = 0;
			foreach (PropertyDescriptor props in derivedCol) {
				if (props.Name == "AnotherProperty")
					anotherPropsFound++;
				else if (props.Name == "YetAnotherProperty")
					yetAnotherPropsFound++;
			}

			// GetProperties does not return the base type property in the case 
			// where both the "new" keyword is used and also the Property type is different 
			// (Type.GetProperties does return both properties)
			//
			Assert.AreEqual (1, anotherPropsFound, "#G2");
			Assert.IsNotNull (derivedCol["AnotherProperty"].Attributes[typeof (DescriptionAttribute)], "#G3");
			Assert.AreEqual (1, yetAnotherPropsFound, "#G4");

			// Verify that we return the derived "new" property when 
			// filters are applied that match the parent property
			// 
			PropertyDescriptorCollection filteredCol = TypeDescriptor.GetProperties (typeof(MyDerivedComponent), 
												 new Attribute[] { BrowsableAttribute.Yes });
			Assert.IsNotNull (filteredCol["YetAnotherProperty"], "#G5");

			// GetProperties does not return write-only properties (ones without a getter)
			// 
			Assert.IsNull (col.Find ("WriteOnlyProperty", true), "#H1");

			col = TypeDescriptor.GetProperties (typeof (IFoo));
			Assert.AreEqual (1, col.Count, "#I1");
			Assert.IsNotNull (col.Find ("HasFired", true), "#I1");
			Assert.IsNotNull (col.Find ("HasFired", false), "#I2");
			Assert.IsNotNull (col.Find ("hasFired", true), "#I3");
			Assert.IsNull (col.Find ("hasFired", false), "#I4");

			col = TypeDescriptor.GetProperties (typeof (IBar));
			Assert.AreEqual (1, col.Count, "#J1");
			Assert.IsNull (col.Find ("HasFired", true), "#J2");
			Assert.IsNotNull (col.Find ("IsDestroyed", true), "#J3");
			Assert.IsNotNull (col.Find ("IsDestroyed", false), "#J4");
			Assert.IsNotNull (col.Find ("isDestroyed", true), "#J5");
			Assert.IsNull (col.Find ("isDestroyed", false), "#J6");
		}

		[Test]
		public void TestGetProperties2 ()
		{
			PropertyDescriptorCollection col = TypeDescriptor.GetProperties (sitedcom);
			Assert.IsNull (col.Find ("TestProperty", true), "#A1");
			Assert.IsNotNull (col.Find ("AnotherProperty", true), "#A2");

			Attribute[] filter = new Attribute[] { new DescriptionAttribute ("test") };
			col = TypeDescriptor.GetProperties (sitedcom, filter);
			Assert.IsNull (col.Find ("TestProperty", true), "#B1");
			Assert.IsNull (col.Find ("AnotherProperty", true), "#B2");
		}

		[Test]
		public void GetProperties_Order ()
		{
			MyComponent com = new MyComponent (new MyContainer ());

			PropertyDescriptorCollection col = TypeDescriptor.GetProperties (com);
			Assert.AreEqual (8, col.Count, "#1");
			Assert.AreEqual ("TestProperty", col [0].Name, "#2");
			Assert.AreEqual ("AnotherProperty", col [1].Name, "#3");
			Assert.AreEqual ("YetAnotherProperty", col [2].Name, "#4");
			Assert.AreEqual ("Name", col [3].Name, "#5");
			Assert.AreEqual ("Address", col [4].Name, "#6");
			Assert.AreEqual ("Country", col [5].Name, "#7");
			Assert.AreEqual ("Site", col [6].Name, "#8");
			Assert.AreEqual ("Container", col [7].Name, "#9");
		}

		[TypeConverter (typeof (TestConverter))]
		class TestConverterClass {
		}

		class TestConverter : TypeConverter {
			public Type Type;

			public TestConverter (Type type)
			{
				this.Type = type;
			}
		}

		[Test]
		public void TestConverterCtorWithArgument ()
		{
			TypeConverter t = TypeDescriptor.GetConverter (typeof (TestConverterClass));
			Assert.IsNotNull (t.GetType (), "#A1");
			Assert.AreEqual (typeof (TestConverter), t.GetType (), "#A2");
			TestConverter converter = (TestConverter) t;
			Assert.AreEqual (typeof (TestConverterClass), converter.Type, "#B");
		}

		[Test]
		public void GetPropertiesIgnoreIndexers ()
		{
			PropertyDescriptorCollection pc =
				TypeDescriptor.GetProperties (typeof (string));
			// There are two string properties: Length and Chars.
			// Chars is an indexer.
			//
			// Future version of CLI might contain some additional
			// properties. In that case simply increase the
			// number. (Also, it is fine to just remove #2.)
			Assert.AreEqual (1, pc.Count, "#1");
			Assert.AreEqual ("Length", pc [0].Name, "#2");
		}
	}
}
