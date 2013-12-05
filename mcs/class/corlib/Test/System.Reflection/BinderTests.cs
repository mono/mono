//
// System.Reflection.BinderTests - Tests Type.DefaultBinder
//
// Authors:
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2004 Novell, Inc. (http://www.novell.com)
//

using NUnit.Framework;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace MonoTests.System.Reflection
{
	enum MyEnum {
		Zero,
		One,
		Two
	}

	class ParamsArrayTest
	{
		public ParamsArrayTest (params string[] strings)
		{}
	}
	
	class SampleClass {
		public static void SampleMethod (object o) { }

		public Type this[decimal i] {
			get { return i.GetType (); }
		}

		public Type this[object i] {
			get { return i.GetType (); }
		}

	}
	
	class SingleIndexer {
		public Type this [int i] {
			get { return i.GetType (); }
		}
	}
	
	class MultiIndexer
	{
		public Type this[byte i] {
			get { return i.GetType (); }
		}

		public Type this[sbyte i] {
			get { return i.GetType (); }
		}

		public Type this[short i] {
			get { return i.GetType (); }
		}

		public Type this[ushort i] {
			get { return i.GetType (); }
		}

		public Type this[int i] {
			get { return i.GetType (); }
		}

		public Type this[uint i] {
			get { return i.GetType (); }
		}

		public Type this[long i] {
			get { return i.GetType (); }
		}

		public Type this[ulong i] {
			get { return i.GetType (); }
		}

		public Type this[float i] {
			get { return i.GetType (); }
		}

		public Type this[double i] {
			get { return i.GetType (); }
		}

		public Type this[decimal i] {
			get { return i.GetType (); }
		}

		public Type this[object i] {
			get { return i.GetType (); }
		}

		public Type this[Enum i] {
			get { return i.GetType (); }
		}
	}

	class MethodInfoWrapper : MethodInfo
	{
		private readonly MethodInfo method;
		
		public MethodInfoWrapper (MethodInfo method)
		{
			this.method = method;
		}
		
		public override object[] GetCustomAttributes (bool inherit)
		{
			return method.GetCustomAttributes (inherit);
		}
		
		public override bool IsDefined (Type attributeType, bool inherit)
		{
			return method.IsDefined (attributeType, inherit);
		}
		
		public override ParameterInfo[] GetParameters ()
		{
			return method.GetParameters ();
		}
		
		public override MethodImplAttributes GetMethodImplementationFlags ()
		{
			return method.GetMethodImplementationFlags ();
		}
		
		public override object Invoke (object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
		{
			return method.Invoke (obj, invokeAttr, binder, parameters, culture);
		}
		
		public override MethodInfo GetBaseDefinition ()
		{
			return method.GetBaseDefinition ();
		}
		
		public override ICustomAttributeProvider ReturnTypeCustomAttributes {
			get { return method.ReturnTypeCustomAttributes; }
		}
		
		public override string Name {
			get { return method.Name; }
		}
		
		public override Type ReturnType {
			get { return method.ReturnType; }
		}
		
		public override Type DeclaringType {
			get { return method.DeclaringType; }
		}
		
		public override Type ReflectedType {
			get { return method.ReflectedType; }
		}
		
		public override RuntimeMethodHandle MethodHandle {
			get { return method.MethodHandle; }
		}
		
		public override MethodAttributes Attributes {
			get { return method.Attributes; }
		}
		
		public override object[] GetCustomAttributes (Type attributeType, bool inherit)
		{
			return method.GetCustomAttributes (attributeType, inherit);
		}
	}

	class DefaultValues
	{
		public int Value;
		public DefaultValues (int i = 5)
		{
			Value = i;
		}
	}

	[TestFixture]
	public class BinderTest
	{
		Binder binder = Type.DefaultBinder;

		[Test]
		public void ParamsArrayTestCast ()
		{
			string[] test_args = { "one", "two", "three" };
			var o = Activator.CreateInstance (typeof (ParamsArrayTest), new object[] { test_args });
			Assert.IsNotNull (o, "#A1");
		}

		[Test]
		public void DefaultParameter ()
		{
			var o = Activator.CreateInstance (typeof (DefaultValues),
				BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.Instance | BindingFlags.OptionalParamBinding,
				null, null, null);
			var a = o as DefaultValues;
			Assert.AreEqual (5, a.Value);
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void SelectPropertyTestNull1 ()
		{
			// The second argument is the one
			binder.SelectProperty (0, null, null, null, null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void SelectPropertyTestEmpty ()
		{
			// The second argument is the one
			binder.SelectProperty (0, new PropertyInfo [] {}, null, null, null);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ChangeTypeOnDefaultBinder ()
		{
			binder.ChangeType (null, null, null);
		}

		[Test]
		[ExpectedException (typeof (AmbiguousMatchException))]
		public void AmbiguousProperty1 () // Bug 58381
		{
			Type type = typeof (MultiIndexer);
			PropertyInfo pi = type.GetProperty ("Item");
		}

		[Test]
		public void SelectAndInvokeAllProperties1 ()
		{
			Type type = typeof (MultiIndexer);
			PropertyInfo [] props = type.GetProperties (BindingFlags.DeclaredOnly |
								    BindingFlags.Public |
								    BindingFlags.Instance);

			// These don't cause an AmbiguousMatchException
			Type [] types = { typeof (byte), typeof (short),
					  typeof (int), typeof (long),
					  typeof (MyEnum) };

			/* MS matches short for sbyte!!! */
			/* MS matches int for ushort!!! */
			/* MS matches long for uint!!! */
			/** These do weird things under MS if used together and then in separate arrays *
			Type [] types = { typeof (ulong), typeof (float), typeof (double),
					  typeof (decimal), typeof (object) };
			*/

			MultiIndexer obj = new MultiIndexer ();

			foreach (Type t in types) {
				PropertyInfo prop = null;
				try {
					prop = binder.SelectProperty (0, props, null, new Type [] {t}, null);
				} catch (Exception e) {
					throw new Exception ("Type: " + t, e);
				}
				Type gotten = (Type) prop.GetValue (obj, new object [] {Activator.CreateInstance (t)});
				Assert.AreEqual (t, gotten);
			}
		}

		[Test]
		public void SelectAndInvokeAllProperties2 ()
		{
			Type type = typeof (MultiIndexer);
			PropertyInfo [] props = type.GetProperties (BindingFlags.DeclaredOnly |
								    BindingFlags.Public |
								    BindingFlags.Instance);

			Type [] types = { typeof (ushort), typeof (char) };

			MultiIndexer obj = new MultiIndexer ();
			PropertyInfo prop1 = binder.SelectProperty (0, props, null, new Type [] {types [0]}, null);
			PropertyInfo prop2 = binder.SelectProperty (0, props, null, new Type [] {types [1]}, null);
			Assert.AreEqual (prop1, prop2);
		}

		[Test]
		public void Select1Match2 ()
		{
			Type type = typeof (SingleIndexer);
			PropertyInfo [] props = type.GetProperties (BindingFlags.DeclaredOnly |
								    BindingFlags.Public |
								    BindingFlags.Instance);
			PropertyInfo prop = binder.SelectProperty (0, props, null, new Type [0], null);
			Assert.IsNull (prop, "empty");
		}
		
		[Test]
		public void Select1Match ()
		{
			Type type = typeof (SingleIndexer);
			PropertyInfo [] props = type.GetProperties (BindingFlags.DeclaredOnly |
								    BindingFlags.Public |
								    BindingFlags.Instance);

			PropertyInfo prop;
			
			prop = binder.SelectProperty (0, props, null, new Type [] { typeof (long) }, null);
			Assert.IsNull (prop, "long");
			prop = binder.SelectProperty (0, props, null, new Type [] { typeof (int) }, null);
			Assert.IsNotNull (prop, "int");
			prop = binder.SelectProperty (0, props, null, new Type [] { typeof (short) }, null);
			Assert.IsNotNull (prop, "short");
		}

		[Test]
		[Category ("NotWorking")]
		public void SelectMethod_AmbiguousMatch ()
		{
			Type type = typeof (BinderTest);
			BindingFlags flags = BindingFlags.Static | BindingFlags.Public;
			MethodBase [] match;
			Type [] types;

			types = new Type [] { typeof (object), typeof (object []) };
			MethodInfo mi_params = type.GetMethod ("params_method1", flags, binder, types, null);
			Assert.IsNotNull (mi_params, "#A1");
			Assert.AreEqual (2, mi_params.GetParameters ().Length, "#A2");
			Assert.AreEqual (typeof (object), mi_params.GetParameters () [0].ParameterType, "#B3");
			Assert.AreEqual (typeof (object []), mi_params.GetParameters () [1].ParameterType, "#B4");
			types = new Type [] { typeof (object) };
			MethodInfo mi_single_param = type.GetMethod ("params_method1", flags, binder, types, null);
			Assert.IsNotNull (mi_single_param, "#B1");
			Assert.AreEqual (1, mi_single_param.GetParameters ().Length, "#B2");
			Assert.AreEqual (typeof (object), mi_single_param.GetParameters () [0].ParameterType, "#B3");

			match = new MethodBase [] { mi_single_param, mi_single_param };
			types = new Type [] { typeof (object) };
			try {
				binder.SelectMethod (flags, match, types, null);
				Assert.Fail ("#C1");
			} catch (AmbiguousMatchException) {
				// Ambiguous match found
			}

			match = new MethodBase [] { mi_single_param, mi_single_param };
			types = new Type [] { typeof (string) };
			try {
				binder.SelectMethod (flags, match, types, null);
				Assert.Fail ("#D1");
			} catch (AmbiguousMatchException) {
				// Ambiguous match found
			}
		}

		[Test]
		public void SelectMethod_ByRef ()
		{
			Type type = typeof (ByRefMatch);
			BindingFlags flags = BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance;
			MethodBase [] match;
			Type [] types;
			MethodBase selected;
			Type ref_int;

			MethodInfo mi_run = type.GetMethod ("Run", flags, binder,
				new Type [] { typeof (int) }, null);
			Assert.IsFalse (mi_run.GetParameters () [0].ParameterType.IsByRef, "#A1");
#if NET_2_0
			MethodInfo mi_run_ref = type.GetMethod ("Run", flags, binder,
				new Type [] { typeof (int).MakeByRefType () }, null);
#else
			MethodInfo mi_run_ref = type.GetMethod ("RunV1", flags);
#endif
			Assert.IsTrue (mi_run_ref.GetParameters () [0].ParameterType.IsByRef, "#A2");

#if NET_2_0
			ref_int = typeof (int).MakeByRefType ();
#else
			ref_int = mi_run_ref.GetParameters () [0].ParameterType;
#endif

			match = new MethodBase [] { mi_run_ref };
			types = new Type [] { typeof (int) };
			selected = binder.SelectMethod (flags, match, types, null);
			Assert.IsNull (selected, "#B1");
			types = new Type [] { ref_int };
			selected = binder.SelectMethod (flags, match, types, null);
			Assert.AreSame (mi_run_ref, selected, "#B2");

			match = new MethodBase [] { mi_run };
			types = new Type [] { typeof (int) };
			selected = binder.SelectMethod (flags, match, types, null);
			Assert.AreSame (mi_run, selected, "#C1");
			types = new Type [] { ref_int };
			selected = binder.SelectMethod (flags, match, types, null);
			Assert.IsNull (selected, "#C1");

			match = new MethodBase [] { mi_run, mi_run_ref };
			types = new Type [] { typeof (int) };
			selected = binder.SelectMethod (flags, match, types, null);
			Assert.AreSame (mi_run, selected, "#D1");
			types = new Type [] { ref_int };
			selected = binder.SelectMethod (flags, match, types, null);
			Assert.AreSame (mi_run_ref, selected, "#D2");
		}

		[Test]
		[Category ("NotWorking")]
		public void SelectMethod_Params ()
		{
			Type type = typeof (BinderTest);
			BindingFlags flags = BindingFlags.Static | BindingFlags.Public;
			MethodBase [] match;
			Type [] types;
			MethodBase selected;

			types = new Type [] { typeof (object), typeof (object) };
			MethodInfo mi_non_params = type.GetMethod ("params_method1", flags, binder, types, null);
			Assert.IsNotNull (mi_non_params, "#A1");
			Assert.AreEqual (2, mi_non_params.GetParameters ().Length, "#A2");
			Assert.AreEqual (typeof (object), mi_non_params.GetParameters () [0].ParameterType, "#A3");
			Assert.AreEqual (typeof (object), mi_non_params.GetParameters () [1].ParameterType, "#A4");
			types = new Type [] { typeof (object), typeof (object []) };
			MethodInfo mi_params = type.GetMethod ("params_method1", flags, binder, types, null);
			Assert.IsNotNull (mi_params, "#B1");
			Assert.AreEqual (2, mi_params.GetParameters ().Length, "#B2");
			Assert.AreEqual (typeof (object), mi_params.GetParameters () [0].ParameterType, "#B3");
			Assert.AreEqual (typeof (object []), mi_params.GetParameters () [1].ParameterType, "#B4");
			types = new Type [] { typeof (object) };
			MethodInfo mi_single_param = type.GetMethod ("params_method1", flags, binder, types, null);
			Assert.IsNotNull (mi_single_param, "#C1");
			Assert.AreEqual (1, mi_single_param.GetParameters ().Length, "#C2");
			Assert.AreEqual (typeof (object), mi_single_param.GetParameters () [0].ParameterType, "#C3");

			match = new MethodBase [] { mi_params };
			types = new Type [] { typeof (object) };
			selected = binder.SelectMethod (flags, match, types, null);
			Assert.IsNull (selected, "#D1");
			types = new Type [] { typeof (object), typeof (object) };
			selected = binder.SelectMethod (flags, match, types, null);
			Assert.IsNull (selected, "#D2");
			types = new Type [] { typeof (object), typeof (object []) };
			selected = binder.SelectMethod (flags, match, types, null);
			Assert.AreSame (mi_params, selected, "#D3");
			types = new Type [] { typeof (object), typeof (object), typeof (object) };
			selected = binder.SelectMethod (flags, match, types, null);
			Assert.IsNull (selected, "#D4");

			match = new MethodBase [] { mi_non_params };	
			types = new Type [] { typeof (object) };
			selected = binder.SelectMethod (flags, match, types, null);
			Assert.IsNull (selected, "#E1");
			types = new Type [] { typeof (object), typeof (object) };
			selected = binder.SelectMethod (flags, match, types, null);
			Assert.AreSame (mi_non_params, selected, "#E2");
			types = new Type [] { typeof (object), typeof (object []) };
			selected = binder.SelectMethod (flags, match, types, null);
			Assert.AreSame (mi_non_params, selected, "#E3");
			types = new Type [] { typeof (object), typeof (object), typeof (object) };
			selected = binder.SelectMethod (flags, match, types, null);
			Assert.IsNull (selected, "#E4");

			match = new MethodBase [] { mi_non_params, mi_params };
			types = new Type [] { typeof (object) };
			selected = binder.SelectMethod (flags, match, types, null);
			Assert.IsNull (selected, "#F1");
			match = new MethodBase [] { mi_non_params, mi_params };
			types = new Type [] { typeof (object), typeof (object) };
			selected = binder.SelectMethod (flags, match, types, null);
			Assert.AreSame (mi_non_params, selected, "#F2");
			match = new MethodBase [] { mi_non_params, mi_params };
			types = new Type [] { typeof (object), typeof (object []) };
			selected = binder.SelectMethod (flags, match, types, null);
			Assert.AreSame (mi_params, selected, "#F3");
			match = new MethodBase [] { mi_non_params, mi_params };
			types = new Type [] { typeof (object), typeof (object), typeof (object) };
			selected = binder.SelectMethod (flags, match, types, null);
			Assert.IsNull (selected, "#F4");

			match = new MethodBase [] { mi_params, mi_non_params };
			types = new Type [] { typeof (object) };
			selected = binder.SelectMethod (flags, match, types, null);
			Assert.IsNull (selected, "#G1");
			match = new MethodBase [] { mi_params, mi_non_params };
			types = new Type [] { typeof (object), typeof (object) };
			selected = binder.SelectMethod (flags, match, types, null);
			Assert.AreSame (mi_non_params, selected, "#G2");
			match = new MethodBase [] { mi_params, mi_non_params };
			types = new Type [] { typeof (object), typeof (object []) };
			selected = binder.SelectMethod (flags, match, types, null);
			Assert.AreSame (mi_params, selected, "#G3");
			match = new MethodBase [] { mi_params, mi_non_params };
			types = new Type [] { typeof (object), typeof (object), typeof (object) };
			selected = binder.SelectMethod (flags, match, types, null);
			Assert.IsNull (selected, "#G4");

			match = new MethodBase [] { mi_params, mi_non_params, mi_single_param };
			types = new Type [] { typeof (object) };
			selected = binder.SelectMethod (flags, match, types, null);
			Assert.AreSame (mi_single_param, selected, "#H1");
			match = new MethodBase [] { mi_params, mi_non_params, mi_single_param };
			types = new Type [] { typeof (object), typeof (object) };
			selected = binder.SelectMethod (flags, match, types, null);
			Assert.AreSame (mi_non_params, selected, "#H2");
			match = new MethodBase [] { mi_params, mi_non_params, mi_single_param };
			types = new Type [] { typeof (object), typeof (object []) };
			selected = binder.SelectMethod (flags, match, types, null);
			Assert.AreSame (mi_params, selected, "#H3");
			match = new MethodBase [] { mi_params, mi_non_params, mi_single_param };
			types = new Type [] { typeof (object), typeof (object), typeof (object) };
			selected = binder.SelectMethod (flags, match, types, null);
			Assert.IsNull (selected, "#H4");

			match = new MethodBase [] { mi_params, mi_non_params, mi_single_param };
			types = new Type [] { typeof (string) };
			selected = binder.SelectMethod (flags, match, types, null);
			Assert.AreSame (mi_single_param, selected, "#I1");
			match = new MethodBase [] { mi_params, mi_non_params, mi_single_param };
			types = new Type [] { typeof (string), typeof (string) };
			selected = binder.SelectMethod (flags, match, types, null); 
			Assert.AreSame (mi_non_params, selected, "#I2");
			match = new MethodBase [] { mi_params, mi_non_params, mi_single_param };
			types = new Type [] { typeof (string), typeof (string []) };
			selected = binder.SelectMethod (flags, match, types, null);
			Assert.AreSame (mi_params, selected, "#I3");
			match = new MethodBase [] { mi_params, mi_non_params, mi_single_param };
			types = new Type [] { typeof (string), typeof (string), typeof (long) };
			selected = binder.SelectMethod (flags, match, types, null);
			Assert.IsNull (selected, "#I4");
		}

		[Test] // bug #314809
		public void ArgNullOnMethod ()
		{
			Type type = typeof (SampleClass);
			BindingFlags flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod;
			type.InvokeMember ("SampleMethod", flags, null, null, new object[] { null });
		}

		[Test]
		public void ArgNullOnProperty ()
		{
			Type type = typeof (SampleClass);
			PropertyInfo [] props = type.GetProperties (BindingFlags.DeclaredOnly |
								    BindingFlags.Public |
								    BindingFlags.Instance);

			PropertyInfo prop = binder.SelectProperty (0, props, null, new Type [] {null}, null);
			Assert.IsNotNull (prop);
		}

		[Test]
		public void BindToMethod_ByRef ()
		{
			Type type = typeof (ByRefMatch);
			BindingFlags flags = BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance;
			MethodBase [] match;
			object [] args = new object [] { 5 };
			object state;
			MethodBase selected;
			CultureInfo culture = CultureInfo.InvariantCulture;

			MethodInfo mi_run = type.GetMethod ("Run", flags, binder,
				new Type [] { typeof (int) }, null);
			Assert.IsFalse (mi_run.GetParameters () [0].ParameterType.IsByRef, "#A1");
#if NET_2_0
			MethodInfo mi_run_ref = type.GetMethod ("Run", flags, binder,
				new Type [] { typeof (int).MakeByRefType () }, null);
#else
			MethodInfo mi_run_ref = type.GetMethod ("RunV1", flags);
#endif
			Assert.IsTrue (mi_run_ref.GetParameters () [0].ParameterType.IsByRef, "#A2");

			match = new MethodBase [] { mi_run };
			selected = binder.BindToMethod (flags, match, ref args, null, culture,
				null, out state);
			Assert.AreSame (mi_run, selected, "#B1");

			match = new MethodBase [] { mi_run_ref };
			selected = binder.BindToMethod (flags, match, ref args, null, culture,
				null, out state);
			Assert.AreSame (mi_run_ref, selected, "#B2");

			match = new MethodBase [] { mi_run, mi_run_ref };
			selected = binder.BindToMethod (flags, match, ref args, null, culture,
				null, out state);
			Assert.AreSame (mi_run, selected, "#B3");

			match = new MethodBase [] { mi_run_ref, mi_run };
			selected = binder.BindToMethod (flags, match, ref args, null, culture,
				null, out state);
			Assert.AreSame (mi_run, selected, "#B4");
		}

		[Test]
		[Category ("NotWorking")]
		public void BindToMethod_AmbiguousMatch ()
		{
			Type type = typeof (BinderTest);
			BindingFlags flags = BindingFlags.Static | BindingFlags.Public;
			MethodBase [] match;
			Type [] types;
			object state;
			object [] args;
			CultureInfo culture = CultureInfo.InvariantCulture;

			types = new Type [] { typeof (object), typeof (object []) };
			MethodInfo mi_params = type.GetMethod ("params_method1", flags, binder, types, null);
			Assert.IsNotNull (mi_params, "#A1");
			Assert.AreEqual (2, mi_params.GetParameters ().Length, "#A2");
			Assert.AreEqual (typeof (object), mi_params.GetParameters () [0].ParameterType, "#B3");
			Assert.AreEqual (typeof (object []), mi_params.GetParameters () [1].ParameterType, "#B4");
			types = new Type [] { typeof (object) };
			MethodInfo mi_single_param = type.GetMethod ("params_method1", flags, binder, types, null);
			Assert.IsNotNull (mi_single_param, "#B1");
			Assert.AreEqual (1, mi_single_param.GetParameters ().Length, "#B2");
			Assert.AreEqual (typeof (object), mi_single_param.GetParameters () [0].ParameterType, "#B3");

			match = new MethodBase [] { mi_single_param, mi_single_param };
			args = new object [] { new object () };
			try {
				binder.BindToMethod (flags, match, ref args, null, culture,
					null, out state);
				Assert.Fail ("#C1");
			} catch (AmbiguousMatchException) {
				// Ambiguous match found
			}

			match = new MethodBase [] { mi_single_param, mi_single_param };
			args = new object [] { string.Empty };
			try {
				binder.BindToMethod (flags, match, ref args, null, culture,
					null, out state);
				Assert.Fail ("#D1");
			} catch (AmbiguousMatchException) {
				// Ambiguous match found
			}

			match = new MethodBase [] { mi_params, mi_params };
			args = new object [] { new object (), new object () };
			try {
				binder.BindToMethod (flags, match, ref args, null, culture,
					null, out state);
				Assert.Fail ("#E1");
			} catch (AmbiguousMatchException) {
				// Ambiguous match found
			}

			match = new MethodBase [] { mi_params, mi_params };
			args = new object [] { string.Empty, 0L };
			try {
				binder.BindToMethod (flags, match, ref args, null, culture,
					null, out state);
				Assert.Fail ("#F1");
			} catch (AmbiguousMatchException) {
				// Ambiguous match found
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void BindToMethod_Params ()
		{
			Type type = typeof (BinderTest);
			BindingFlags flags = BindingFlags.Static | BindingFlags.Public;
			MethodBase [] match;
			Type [] types;
			MethodBase selected;
			object state;
			object [] args;
			CultureInfo culture = CultureInfo.InvariantCulture;

			types = new Type [] { typeof (object), typeof (object) };
			MethodInfo mi_non_params = type.GetMethod ("params_method1", flags, binder, types, null);
			Assert.IsNotNull (mi_non_params, "#A1");
			Assert.AreEqual (2, mi_non_params.GetParameters ().Length, "#A2");
			Assert.AreEqual (typeof (object), mi_non_params.GetParameters () [0].ParameterType, "#A3");
			Assert.AreEqual (typeof (object), mi_non_params.GetParameters () [1].ParameterType, "#A4");
			types = new Type [] { typeof (object), typeof (object []) };
			MethodInfo mi_params = type.GetMethod ("params_method1", flags, binder, types, null);
			Assert.IsNotNull (mi_params, "#B1");
			Assert.AreEqual (2, mi_params.GetParameters ().Length, "#B2");
			Assert.AreEqual (typeof (object), mi_params.GetParameters () [0].ParameterType, "#B3");
			Assert.AreEqual (typeof (object []), mi_params.GetParameters () [1].ParameterType, "#B4");
			types = new Type [] { typeof (object) };
			MethodInfo mi_single_param = type.GetMethod ("params_method1", flags, binder, types, null);
			Assert.IsNotNull (mi_single_param, "#C1");
			Assert.AreEqual (1, mi_single_param.GetParameters ().Length, "#C2");
			Assert.AreEqual (typeof (object), mi_single_param.GetParameters () [0].ParameterType, "#C3");

			match = new MethodBase [] { mi_params };
			args = new object [] { new object () };
			selected = binder.BindToMethod (flags, match, ref args, null, culture,
				null, out state);
			Assert.AreSame (mi_params, selected, "#D1");
			args = new object [] { new object (), new object () };
			selected = binder.BindToMethod (flags, match, ref args, null, culture,
				null, out state);
			Assert.AreSame (mi_params, selected, "#D2");
			args = new object [] { new object (), new object [0] };
			selected = binder.BindToMethod (flags, match, ref args, null, culture,
				null, out state);
			Assert.AreSame (mi_params, selected, "#D3");
			args = new object [] { new object (), new object (), new object () };
			selected = binder.BindToMethod (flags, match, ref args, null, culture,
				null, out state);
			Assert.AreSame (mi_params, selected, "#D4");

			match = new MethodBase [] { mi_non_params };
			args = new object [] { new object () };
			try {
				binder.BindToMethod (flags, match, ref args, null, culture,
					null, out state);
				Assert.Fail ("#E1");
			} catch (MissingMethodException) {
				// Member not found
			}
			args = new object [] { new object (), new object () };
			selected = binder.BindToMethod (flags, match, ref args, null, culture,
				null, out state);
			Assert.AreSame (mi_non_params, selected, "#E2");
			args = new object [] { new object (), new object [0] };
			selected = binder.BindToMethod (flags, match, ref args, null, culture,
				null, out state);
			Assert.AreSame (mi_non_params, selected, "#E3");
			args = new object [] { new object (), new object (), new object () };
			try {
				binder.BindToMethod (flags, match, ref args, null, culture,
					null, out state);
				Assert.Fail ("#E4");
			} catch (MissingMethodException) {
				// Member not found
			}

			match = new MethodBase [] { mi_non_params, mi_params };
			args = new object [] { new object () };
			selected = binder.BindToMethod (flags, match, ref args, null, culture,
				null, out state);
			Assert.AreSame (mi_params, selected, "#F1");
			match = new MethodBase [] { mi_non_params, mi_params };
			args = new object [] { new object (), new object () };
			selected = binder.BindToMethod (flags, match, ref args, null, culture,
				null, out state);
			Assert.AreSame (mi_non_params, selected, "#F2");
			match = new MethodBase [] { mi_non_params, mi_params };
			args = new object [] { new object (), new object [0] };
			selected = binder.BindToMethod (flags, match, ref args, null, culture,
				null, out state);
			Assert.AreSame (mi_params, selected, "#F3");
			match = new MethodBase [] { mi_non_params, mi_params };
			args = new object [] { new object (), new object (), new object () };
			selected = binder.BindToMethod (flags, match, ref args, null, culture,
				null, out state);
			Assert.AreSame (mi_params, selected, "#F4");

			match = new MethodBase [] { mi_params, mi_non_params };
			args = new object [] { new object () };
			selected = binder.BindToMethod (flags, match, ref args, null, culture,
				null, out state);
			Assert.AreSame (mi_params, selected, "#G1");
			match = new MethodBase [] { mi_params, mi_non_params };
			args = new object [] { new object (), new object () };
			selected = binder.BindToMethod (flags, match, ref args, null, culture,
				null, out state);
			Assert.AreSame (mi_non_params, selected, "#G2");
			match = new MethodBase [] { mi_params, mi_non_params };
			args = new object [] { new object (), new object [0] };
			selected = binder.BindToMethod (flags, match, ref args, null, culture,
				null, out state);
			Assert.AreSame (mi_params, selected, "#G3");
			match = new MethodBase [] { mi_params, mi_non_params };
			args = new object [] { new object (), new object (), new object () };
			selected = binder.BindToMethod (flags, match, ref args, null, culture,
				null, out state);
			Assert.AreSame (mi_params, selected, "#G4");

			match = new MethodBase [] { mi_params, mi_non_params, mi_single_param };
			args = new object [] { new object () };
			selected = binder.BindToMethod (flags, match, ref args, null, culture,
				null, out state);
			Assert.AreSame (mi_single_param, selected, "#H1");
			match = new MethodBase [] { mi_params, mi_non_params, mi_single_param };
			args = new object [] { new object (), new object () };
			selected = binder.BindToMethod (flags, match, ref args, null, culture,
				null, out state);
			Assert.AreSame (mi_non_params, selected, "#H2");
			match = new MethodBase [] { mi_params, mi_non_params, mi_single_param };
			args = new object [] { new object (), new object [0] };
			selected = binder.BindToMethod (flags, match, ref args, null, culture,
				null, out state);
			Assert.AreSame (mi_params, selected, "#H3");
			match = new MethodBase [] { mi_params, mi_non_params, mi_single_param };
			args = new object [] { new object (), new object (), new object () };
			selected = binder.BindToMethod (flags, match, ref args, null, culture,
				null, out state);
			Assert.AreSame (mi_params, selected, "#H4");

			match = new MethodBase [] { mi_params, mi_non_params, mi_single_param };
			args = new object [] { string.Empty };
			selected = binder.BindToMethod (flags, match, ref args, null, culture,
				null, out state);
			Assert.AreSame (mi_single_param, selected, "#I1");
			match = new MethodBase [] { mi_params, mi_non_params, mi_single_param };
			args = new object [] { string.Empty, string.Empty };
			selected = binder.BindToMethod (flags, match, ref args, null, culture,
				null, out state);
			Assert.AreSame (mi_non_params, selected, "#I2");
			match = new MethodBase [] { mi_params, mi_non_params, mi_single_param };
			args = new object [] { string.Empty, new string [0] };
			selected = binder.BindToMethod (flags, match, ref args, null, culture,
				null, out state);
			Assert.AreSame (mi_params, selected, "#I3");
			match = new MethodBase [] { mi_params, mi_non_params, mi_single_param };
			args = new object [] { string.Empty, string.Empty, 5L };
			selected = binder.BindToMethod (flags, match, ref args, null, culture,
				null, out state);
			Assert.AreSame (mi_params, selected, "#I4");
		}

		[Test]
		[Category ("NotDotNet")]
		[Category ("NotWorking")]
		public void BindToMethod_Params_Mono ()
		{
			Type type = typeof (BinderTest);
			BindingFlags flags = BindingFlags.Static | BindingFlags.Public;
			MethodBase [] match;
			Type [] types;
			MethodBase selected;
			object state;
			object [] args;
			CultureInfo culture = CultureInfo.InvariantCulture;

			types = new Type [] { typeof (object), typeof (object) };
			MethodInfo mi_non_params = type.GetMethod ("params_method1", flags, binder, types, null);
			Assert.IsNotNull (mi_non_params, "#A1");
			Assert.AreEqual (2, mi_non_params.GetParameters ().Length, "#A2");
			Assert.AreEqual (typeof (object), mi_non_params.GetParameters () [0].ParameterType, "#A3");
			Assert.AreEqual (typeof (object), mi_non_params.GetParameters () [1].ParameterType, "#A4");
			types = new Type [] { typeof (object), typeof (object []) };
			MethodInfo mi_params = type.GetMethod ("params_method1", flags, binder, types, null);
			Assert.IsNotNull (mi_params, "#B1");
			Assert.AreEqual (2, mi_params.GetParameters ().Length, "#B2");
			Assert.AreEqual (typeof (object), mi_params.GetParameters () [0].ParameterType, "#B3");
			Assert.AreEqual (typeof (object []), mi_params.GetParameters () [1].ParameterType, "#B4");
			types = new Type [] { typeof (object) };
			MethodInfo mi_single_param = type.GetMethod ("params_method1", flags, binder, types, null);
			Assert.IsNotNull (mi_single_param, "#C1");
			Assert.AreEqual (1, mi_single_param.GetParameters ().Length, "#C2");
			Assert.AreEqual (typeof (object), mi_single_param.GetParameters () [0].ParameterType, "#C3");

			match = new MethodBase [] { mi_non_params, mi_params };
			args = new object [] { new object () };
			selected = binder.BindToMethod (flags, match, ref args, null, culture,
				null, out state);
			Assert.AreSame (mi_params, selected, "#D1");
			args = new object [] { new object (), new object () };
			selected = binder.BindToMethod (flags, match, ref args, null, culture,
				null, out state);
			Assert.AreSame (mi_non_params, selected, "#D2");
			args = new object [] { new object (), new object [0] };
			selected = binder.BindToMethod (flags, match, ref args, null, culture,
				null, out state);
			Assert.AreSame (mi_params, selected, "#D3");
			args = new object [] { new object (), new object (), new object () };
			selected = binder.BindToMethod (flags, match, ref args, null, culture,
				null, out state);
			Assert.AreSame (mi_params, selected, "#D4");

			match = new MethodBase [] { mi_params, mi_non_params, mi_single_param };
			args = new object [] { new object () };
			selected = binder.BindToMethod (flags, match, ref args, null, culture,
				null, out state);
			Assert.AreSame (mi_single_param, selected, "#E1");
			args = new object [] { new object (), new object () };
			selected = binder.BindToMethod (flags, match, ref args, null, culture,
				null, out state);
			Assert.AreSame (mi_non_params, selected, "#E2");
		}

		[Test]
		[Category ("NotWorking")]
		public void BindToMethod_Params_MS ()
		{
			Type type = typeof (BinderTest);
			BindingFlags flags = BindingFlags.Static | BindingFlags.Public;
			MethodBase [] match;
			Type [] types;
			MethodBase selected;
			object state;
			object [] args;
			CultureInfo culture = CultureInfo.InvariantCulture;

			types = new Type [] { typeof (object), typeof (object) };
			MethodInfo mi_non_params = type.GetMethod ("params_method1", flags, binder, types, null);
			Assert.IsNotNull (mi_non_params, "#A1");
			Assert.AreEqual (2, mi_non_params.GetParameters ().Length, "#A2");
			Assert.AreEqual (typeof (object), mi_non_params.GetParameters () [0].ParameterType, "#A3");
			Assert.AreEqual (typeof (object), mi_non_params.GetParameters () [1].ParameterType, "#A4");
			types = new Type [] { typeof (object), typeof (object []) };
			MethodInfo mi_params = type.GetMethod ("params_method1", flags, binder, types, null);
			Assert.IsNotNull (mi_params, "#B1");
			Assert.AreEqual (2, mi_params.GetParameters ().Length, "#B2");
			Assert.AreEqual (typeof (object), mi_params.GetParameters () [0].ParameterType, "#B3");
			Assert.AreEqual (typeof (object []), mi_params.GetParameters () [1].ParameterType, "#B4");
			types = new Type [] { typeof (object) };
			MethodInfo mi_single_param = type.GetMethod ("params_method1", flags, binder, types, null);
			Assert.IsNotNull (mi_single_param, "#C1");
			Assert.AreEqual (1, mi_single_param.GetParameters ().Length, "#C2");
			Assert.AreEqual (typeof (object), mi_single_param.GetParameters () [0].ParameterType, "#C3");

			match = new MethodBase [] { mi_non_params, mi_params };
			args = new object [] { new object () };
			selected = binder.BindToMethod (flags, match, ref args, null, culture,
				null, out state);
			Assert.AreSame (mi_params, selected, "#D1");
			args = new object [] { new object (), new object () };
			try {
				binder.BindToMethod (flags, match, ref args, null, culture,
					null, out state);
				Assert.Fail ("#D2");
			} catch (AmbiguousMatchException) {
			}
			args = new object [] { new object (), new object [0] };
			try {
				binder.BindToMethod (flags, match, ref args, null, culture,
					null, out state);
				Assert.Fail ("#D3");
			} catch (AmbiguousMatchException) {
			}
			args = new object [] { new object (), new object (), new object () };
			try {
				binder.BindToMethod (flags, match, ref args, null, culture,
					null, out state);
				Assert.Fail ("#D4");
			} catch (IndexOutOfRangeException) {
			}

			match = new MethodBase [] { mi_params, mi_non_params, mi_single_param };
			args = new object [] { new object () };
			selected = binder.BindToMethod (flags, match, ref args, null, culture,
				null, out state);
			Assert.AreSame (mi_single_param, selected, "#E1");
			args = new object [] { new object (), new object () };
			selected = binder.BindToMethod (flags, match, ref args, null, culture,
				null, out state);
			Assert.AreSame (mi_params, selected, "#E2");
		}

		[Test] // bug #41691
		public void BindToMethodNamedArgs ()
		{
			Type t = typeof (Bug41691);

			StringWriter sw = new StringWriter ();
			sw.NewLine = "\n";

			object[] argValues = new object [] {"Hello", "World", "Extra", sw};
			string [] argNames = new string [] {"firstName", "lastName"};

			t.InvokeMember ("PrintName",
					BindingFlags.InvokeMethod,
					null,
					null,
					argValues,
					null,
					null,
					argNames);

			Assert.AreEqual ("Hello\nExtra\nWorld\n", sw.ToString ());
		}

		[Test] // #1321
		public void BindToMethodNamedArgs_2 ()
		{
			StringWriter sw = new StringWriter ();
			sw.NewLine = "\n";

			object[] argValues = new object [] {5, "AB", sw};
			string [] argNames = new string [] {"second", "first", "output"};

			typeof (BinderTest).InvokeMember ("TestMethod",
					BindingFlags.InvokeMethod,
					null,
					null,
					argValues,
					null,
					null,
					argNames);

			Assert.AreEqual ("AB\n5\n", sw.ToString ());
		}

		public static void TestMethod (string first, int second, TextWriter output) {
			output.WriteLine (first);
			output.WriteLine (second);
		}

		public class Bug41691
		{
			public static void PrintName (string lastName, string firstName, string extra, TextWriter output)
			{
				output.WriteLine (firstName);
				output.WriteLine (extra);
				output.WriteLine (lastName);
			}
		}

		[Test] // bug #42457
		public void GetMethodAmbiguity ()
		{
			object IntegerObject = 5;
			object IntArrayObject = new int[] {5, 2, 5};
			object StringArrayObject = new string [] {"One", "Two"};
			object [] IntParam = new object [] {IntegerObject};
			object [] IntArrayParam = new object [] {IntArrayObject};
			object [] StringArrayParam = new object [] {StringArrayObject};

			object be = this;
			Type betype = this.GetType ();

			string name1 = "Bug42457Method";
			string name2 = "Bug42457Method2";

			MethodInfo mi_obj = betype.GetMethod (name1, Type.GetTypeArray (IntParam));
			mi_obj.Invoke (be, IntParam);
			Assert.AreEqual (1, bug42457, "#1");
			MethodInfo mi_arr = betype.GetMethod (name1, Type.GetTypeArray (IntArrayParam));
			mi_arr.Invoke (be, IntArrayParam);
			Assert.AreEqual (2, bug42457, "#2");
			MethodInfo mi_str = betype.GetMethod (name1, Type.GetTypeArray (StringArrayParam));
			mi_str.Invoke (be, StringArrayParam);
			Assert.AreEqual (3, bug42457, "#3");

			MethodInfo m2_obj = betype.GetMethod (name2, Type.GetTypeArray (IntParam));
			m2_obj.Invoke (be, IntParam);
			Assert.AreEqual (1, bug42457_2, "#4");
			MethodInfo m2_arr = betype.GetMethod (name2, Type.GetTypeArray (IntArrayParam));
			m2_arr.Invoke (be, IntArrayParam);
			Assert.AreEqual (2, bug42457_2, "#5");
			MethodInfo m2_str = betype.GetMethod (name2, Type.GetTypeArray(StringArrayParam));
			m2_str.Invoke (be, StringArrayParam);
			Assert.AreEqual (3, bug42457_2, "#6");
		}

#if NET_2_0
		[Test]
		public void NullableArg () {
			MethodInfo method = (typeof (BinderTest)).GetMethod("SetA", new [] {typeof (Int32)});
			Assert.AreEqual (5, method.Invoke (new BinderTest (), new object [] { 5 }));
		}

		public int SetA(Int32? a) {
			return (int)a;
		}
#endif

		static void MethodWithLongParam(long param)
		{
		}

		[Test]
		public void TestParamsAttribute ()
		{
			MethodInfo mi;

			mi = typeof (BinderTest).GetMethod ("params_method1", BindingFlags.Static | BindingFlags.Public, null, new Type [] { typeof (object), typeof (object) }, null);
			Assert.IsNotNull (mi, "#A1");
			Assert.AreEqual (typeof (object), mi.GetParameters () [1].ParameterType, "#A2");

			mi = typeof (BinderTest).GetMethod ("params_method1", BindingFlags.Static | BindingFlags.Public, null, new Type [] { typeof (object), typeof (object []) }, null);
			Assert.IsNotNull (mi, "#B1");
			Assert.AreEqual (typeof (object []), mi.GetParameters () [1].ParameterType, "#B2");
		}

		[Test]
		public void TestParamsAttribute_1 ()
		{
			MethodInfo mi = typeof (BinderTest).GetMethod ("params_method1", BindingFlags.Static | BindingFlags.Public, null, new Type [] { typeof (object), typeof (object), typeof (object) }, null);
			Assert.IsNull (mi, "#1");
		}

		[Test]
		public void TestParamsAttribute_2 ()
		{
			MethodInfo mi = typeof (BinderTest).GetMethod ("params_method2", BindingFlags.Static | BindingFlags.Public, null, Type.EmptyTypes, null);
			Assert.IsNull (mi, "#1");
		}

		public static void params_method1 (object o)
		{
		}

		public static void params_method1 (object o, params object[] o2)
		{
		}

		public static void params_method1 (object o, object o2)
		{
		}

		public static void params_method2 (params string[] args)
		{
		}	

		public static double DoubleMethod (double d) {
			return d;
		}

		public static float FloatMethod (float f) {
			return f;
		}

		[Test]
		public void ChangeType ()
		{
			// Char -> Double
			Assert.AreEqual (42.0, typeof (BinderTest).GetMethod ("DoubleMethod").Invoke (null, new object[] { (char)42 }));

			// Char -> Float
			Assert.AreEqual (42.0f, typeof (BinderTest).GetMethod ("FloatMethod").Invoke (null, new object[] { (char)42 }));
		}

		[Test]
		public void TestExactBinding ()
		{
			Type[] types = new Type[] { typeof(int) };
			BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.ExactBinding;
			Assert.IsNull (typeof (BinderTest).GetMethod("MethodWithLongParam",
				flags, null, types, null));
		}

		public void Bug42457Method (object thing)
		{
			bug42457 = 1;
		}

		public void Bug42457Method (Array thing)
		{
			bug42457 = 2;
		}

		public void Bug42457Method (string [] thing)
		{
			bug42457 = 3;
		}

		public void Bug42457Method2 (object thing)
		{
			bug42457_2 = 1;
		}

		public void Bug42457Method2 (Array thing)
		{
			bug42457_2 = 2;
		}

		public void Bug42457Method2 (string [] thing)
		{
			bug42457_2 = 3;
		}

		int bug42457, bug42457_2;

		[Test] // bug #77079
		public void GetMethodAvoidAmbiguity2 ()
		{
			Type tType = this.GetType ();
			Bug77079A a = new Bug77079C ();

			tType.InvokeMember ("Bug77079",
				BindingFlags.Public | BindingFlags.InvokeMethod | 
				BindingFlags.Instance,
				null, this, new object[] {a});
			Assert.AreEqual (2, bug77079);
		}

		int bug77079;

		public void Bug77079 (Bug77079A a)
		{
			bug77079 = 1;
		}

		public void Bug77079 (Bug77079B a)
		{
			bug77079 = 2;
		}

		public class Bug77079A
		{
		}

		public class Bug77079B : Bug77079A
		{
		}

		public class Bug77079C : Bug77079B
		{
		}

		[Test] // bug #76083
		public void GetMethodAvoidAmbiguity3 ()
		{
			Type[] types = new Type[] { typeof (Bug76083ArgDerived) };
			MethodInfo m = typeof (Bug76083Derived).GetMethod ("Foo", types);
			Assert.AreEqual (typeof (Bug76083Derived), m.DeclaringType);
		}

		public class Bug76083ArgBase {}
		public class Bug76083ArgDerived : Bug76083ArgBase {}

		public class Bug76083Base
		{
			public void Foo (Bug76083ArgBase a) {}
		}

		public class Bug76083Derived : Bug76083Base
		{
			public new void Foo (Bug76083ArgBase a) {}
		}

		private const BindingFlags BUG324998_BINDING_FLAGS
			= BindingFlags.Public | BindingFlags.NonPublic
			| BindingFlags.Instance | BindingFlags.Static
			| BindingFlags.IgnoreCase;

		class Bug324998AGood { public virtual void f(int i1, int i2, bool b) {} }

		class Bug324998BGood : Bug324998AGood { public override void f(int i1, int i2, bool b) {} }

		class Bug324998ABad {
			public virtual void f(int i1, int i2) {}
			public virtual void f(int i1, int i2, bool b) {}
		}

		class Bug324998BBad : Bug324998ABad { public override void f(int i1, int i2, bool b) {} }

		[Test]
		public void Bug324998Good () {
			if (typeof(Bug324998BGood).GetMethod("f", BUG324998_BINDING_FLAGS) == null)
				throw new Exception("Bug324998Good");
		}

		[Test]
		[ExpectedException (typeof (AmbiguousMatchException))]
		public void Bug324998Bad () {
			typeof(Bug324998BBad).GetMethod("f", BUG324998_BINDING_FLAGS);
		}

		void Bug380361 (MyEnum e) { }

		[Test]
		public void TestEnumConversion ()
		{
			Type type = this.GetType ();
			MethodInfo mi = type.GetMethod ("Bug380361", BindingFlags.NonPublic | BindingFlags.Instance, binder, new Type [] { typeof (MyEnum) }, null);
			mi.Invoke (this, new object [] { (int)MyEnum.Zero });
		}

		[Test]
		public void TestEnumConversion2 ()
		{
			Type type = this.GetType ();
			MethodInfo mi = type.GetMethod ("Bug380361", BindingFlags.NonPublic | BindingFlags.Instance, binder, new Type [] { typeof (MyEnum) }, null);
			try {
				mi.Invoke (this, new object [] { (long) MyEnum.Zero });
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}

		class AssertingBinder : Binder {

			public static readonly AssertingBinder Instance = new AssertingBinder ();

			public override FieldInfo BindToField (BindingFlags bindingAttr, FieldInfo [] match, object value, CultureInfo culture)
			{
				Assert.IsNotNull (match);

				return Type.DefaultBinder.BindToField (bindingAttr, match, value, culture);
			}

			public override MethodBase BindToMethod (BindingFlags bindingAttr, MethodBase [] match, ref object [] args, ParameterModifier [] modifiers, CultureInfo culture, string [] names, out object state)
			{
				Assert.IsNotNull (match);
				Assert.IsNotNull (args);

				return Type.DefaultBinder.BindToMethod (bindingAttr, match, ref args, modifiers, culture, names, out state);
			}

			public override object ChangeType (object value, Type type, CultureInfo culture)
			{
				Assert.IsNotNull (value);
				Assert.IsNotNull (type);

				return Type.DefaultBinder.ChangeType (value, type, culture);
			}

			public override void ReorderArgumentArray (ref object [] args, object state)
			{
				Assert.IsNotNull (args);

				Type.DefaultBinder.ReorderArgumentArray (ref args, state);
			}

			public override MethodBase SelectMethod (BindingFlags bindingAttr, MethodBase [] match, Type [] types, ParameterModifier [] modifiers)
			{
				Assert.IsNotNull (match);
				Assert.IsNotNull (types);

				return Type.DefaultBinder.SelectMethod (bindingAttr, match, types, modifiers);
			}

			public override PropertyInfo SelectProperty (BindingFlags bindingAttr, PropertyInfo [] match, Type returnType, Type [] indexes, ParameterModifier [] modifiers)
			{
				Assert.IsNotNull (match);

				return Type.DefaultBinder.SelectProperty (bindingAttr, match, returnType, indexes, modifiers);
			}
		}

		class BaseFoo {
			public void Bar ()
			{
			}

			public int Add(int x, int y)
			{
				return x + y;	
			}
		}

		class Foo : BaseFoo {

			public bool Barred;

			public new void Bar ()
			{
				Barred = true;
			}
		}

		class ByRefMatch {
			public void Run (int i)
			{
			}

#if NET_2_0
			public void Run (out int i)
#else
			public void RunV1 (out int i)
#endif
			{
				i = 0;
			}
		}

		[Test] // bug  #471257
		public void TestCustomBinderNonNullArgs ()
		{
			var foo = new Foo ();

			typeof (Foo).InvokeMember (
				"Bar",
				BindingFlags.InvokeMethod,
				AssertingBinder.Instance,
				foo,
				null);

			Assert.IsTrue (foo.Barred);
		}

		class Int32Binder : AssertingBinder
		{
			public override object ChangeType(Object value, Type type, CultureInfo ci)
			{
				if (value.GetType() == type) {
					return value;
				} else if (type.IsPrimitive) {
					if (type == typeof(Int32))
						return Convert.ToInt32(value);

					throw new ArgumentException("missing support for primitive: " + type);
				}

				throw new ArgumentException("Could not ChangeType to " + type.FullName);
			}
		}

		[Test]
		[ExpectedException(typeof (TargetParameterCountException))]
		public void TestTargetParameterCountExceptionA ()
		{
			MethodInfo method = typeof (Foo).GetMethod ("Add");
			method.Invoke((new Foo ()), 0, null, null, null);
		}

		[Test]
		[ExpectedException(typeof (TargetParameterCountException))]
		public void TestTargetParameterCountExceptionB ()
		{
			MethodInfo method = typeof (Foo).GetMethod ("Add");
			method.Invoke(new Foo (), 0, null, new object [] {1}, null);
		}

		[Test]
		public void TestBindingFlagsA ()
		{
			MethodInfo method = typeof (Foo).GetMethod ("Add");
			method.Invoke((new Foo ()), 0, null, new object [] {1, 2}, null);
		}

		[Test]
		[ExpectedException(typeof (ArgumentException))]
		public void TestBindingFlagsB ()
		{
			MethodInfo method = typeof (Foo).GetMethod ("Add");
			method.Invoke((new Foo ()), 0, null, new object [] {1, "2"}, null);
		}

		[Test]
		public void TestBindingFlagsExactBindingA ()
		{
			MethodInfo method = typeof (Foo).GetMethod ("Add");
			method.Invoke((new Foo ()), BindingFlags.ExactBinding, null, new object [] {1, 2}, null);
		}

		[Test]
		[ExpectedException(typeof (ArgumentException))]
		public void TestBindingFlagsExactBindingB ()
		{
			MethodInfo method = typeof (Foo).GetMethod ("Add");
			method.Invoke((new Foo ()), BindingFlags.ExactBinding, new Int32Binder (), new object [] {1, "2"}, null);
		}

		[Test]
		public void TestBindingFlagsExactBindingC ()
		{
			MethodInfo method = typeof (Foo).GetMethod ("Add");
			method.Invoke((new Foo ()), 0, new Int32Binder (), new object [] {1, "2"}, null);
		}

		public void Bug325306<T> (int a) {}
		public void Bug325306_ (int a) {}

		[Test] //bug 325306
		[ExpectedException (typeof (AmbiguousMatchException))]
		public void SelectMethodWithExactAndAmbiguousMethods ()
		{
			var m = typeof (BinderTest).GetMethod ("Bug325306_");
	        BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;
			AssertingBinder.Instance.SelectMethod (flags, new MethodBase [] {m, m}, new Type[] { typeof (int) }, null);
	 	}

		[Test] //bug 325306
		[ExpectedException (typeof (AmbiguousMatchException))]
		public void SelectMethodWithGmdAmbiguity ()
		{
			var m0 = typeof (BinderTest).GetMethod ("Bug325306");
			var m1 = typeof (BinderTest).GetMethod ("Bug325306_");
	        BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;
	
			AssertingBinder.Instance.SelectMethod (flags, new MethodBase [] {m0, m1}, new Type[] { typeof (int) }, null);
	 	}

		public static string Bug636939 (IFormatProvider provider, string pattern, params object [] args)
		{
			return string.Format (pattern, args);
		}

		[Test] // bug #636939
		[Category ("NotWorking")]
		public void SelectMethodWithParamArrayAndNonEqualTypeArguments ()
		{
            const BindingFlags flags =
                BindingFlags.IgnoreCase | BindingFlags.Instance |
                BindingFlags.Static | BindingFlags.Public |
                BindingFlags.FlattenHierarchy | BindingFlags.InvokeMethod;

			Assert.AreEqual ("foobarbaz", typeof (BinderTest).InvokeMember (
				"bug636939",
				flags,
				null, // binder
				null, // target
				new object [] { CultureInfo.CurrentCulture, "foo{0}{1}", "bar", "baz" }));
		}

		public static void CustomMethodType_Helper ()
		{
		}

		[Test]
		public void CustomMethodType ()
		{
			var method = new MethodInfoWrapper (GetType ().GetMethod ("CustomMethodType_Helper"));

			var res = Type.DefaultBinder.SelectMethod (BindingFlags.Static | BindingFlags.Public, new[] { method }, Type.EmptyTypes, new ParameterModifier[0]);
			Assert.AreSame (method, res);
		}
	}
}
