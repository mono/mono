using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Configuration.Assemblies;
using NUnit.Framework;

namespace MonoTests.System.Reflection.Emit
{

[TestFixture]
public class SaveTest
{
	// strongname generated using "sn -k unit.snk"
	static byte[] strongName = { 
		0x07, 0x02, 0x00, 0x00, 0x00, 0x24, 0x00, 0x00, 0x52, 0x53, 0x41, 0x32, 
		0x00, 0x04, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x7F, 0x7C, 0xEA, 0x4A, 
		0x28, 0x33, 0xD8, 0x3C, 0x86, 0x90, 0x86, 0x91, 0x11, 0xBB, 0x30, 0x0D, 
		0x3D, 0x69, 0x04, 0x4C, 0x48, 0xF5, 0x4F, 0xE7, 0x64, 0xA5, 0x82, 0x72, 
		0x5A, 0x92, 0xC4, 0x3D, 0xC5, 0x90, 0x93, 0x41, 0xC9, 0x1D, 0x34, 0x16, 
		0x72, 0x2B, 0x85, 0xC1, 0xF3, 0x99, 0x62, 0x07, 0x32, 0x98, 0xB7, 0xE4, 
		0xFA, 0x75, 0x81, 0x8D, 0x08, 0xB9, 0xFD, 0xDB, 0x00, 0x25, 0x30, 0xC4, 
		0x89, 0x13, 0xB6, 0x43, 0xE8, 0xCC, 0xBE, 0x03, 0x2E, 0x1A, 0x6A, 0x4D, 
		0x36, 0xB1, 0xEB, 0x49, 0x26, 0x6C, 0xAB, 0xC4, 0x29, 0xD7, 0x8F, 0x25, 
		0x11, 0xA4, 0x7C, 0x81, 0x61, 0x97, 0xCB, 0x44, 0x2D, 0x80, 0x49, 0x93, 
		0x48, 0xA7, 0xC9, 0xAB, 0xDB, 0xCF, 0xA3, 0x34, 0xCB, 0x6B, 0x86, 0xE0, 
		0x4D, 0x27, 0xFC, 0xA7, 0x4F, 0x36, 0xCA, 0x13, 0x42, 0xD3, 0x83, 0xC4, 
		0x06, 0x6E, 0x12, 0xE0, 0xA1, 0x3D, 0x9F, 0xA9, 0xEC, 0xD1, 0xC6, 0x08, 
		0x1B, 0x3D, 0xF5, 0xDB, 0x4C, 0xD4, 0xF0, 0x2C, 0xAA, 0xFC, 0xBA, 0x18, 
		0x6F, 0x48, 0x7E, 0xB9, 0x47, 0x68, 0x2E, 0xF6, 0x1E, 0x67, 0x1C, 0x7E, 
		0x0A, 0xCE, 0x10, 0x07, 0xC0, 0x0C, 0xAD, 0x5E, 0xC1, 0x53, 0x70, 0xD5, 
		0xE7, 0x25, 0xCA, 0x37, 0x5E, 0x49, 0x59, 0xD0, 0x67, 0x2A, 0xBE, 0x92, 
		0x36, 0x86, 0x8A, 0xBF, 0x3E, 0x17, 0x04, 0xFB, 0x1F, 0x46, 0xC8, 0x10, 
		0x5C, 0x93, 0x02, 0x43, 0x14, 0x96, 0x6A, 0xD9, 0x87, 0x17, 0x62, 0x7D, 
		0x3A, 0x45, 0xBE, 0x35, 0xDE, 0x75, 0x0B, 0x2A, 0xCE, 0x7D, 0xF3, 0x19, 
		0x85, 0x4B, 0x0D, 0x6F, 0x8D, 0x15, 0xA3, 0x60, 0x61, 0x28, 0x55, 0x46, 
		0xCE, 0x78, 0x31, 0x04, 0x18, 0x3C, 0x56, 0x4A, 0x3F, 0xA4, 0xC9, 0xB1, 
		0x41, 0xED, 0x22, 0x80, 0xA1, 0xB3, 0xE2, 0xC7, 0x1B, 0x62, 0x85, 0xE4, 
		0x81, 0x39, 0xCB, 0x1F, 0x95, 0xCC, 0x61, 0x61, 0xDF, 0xDE, 0xF3, 0x05, 
		0x68, 0xB9, 0x7D, 0x4F, 0xFF, 0xF3, 0xC0, 0x0A, 0x25, 0x62, 0xD9, 0x8A, 
		0x8A, 0x9E, 0x99, 0x0B, 0xFB, 0x85, 0x27, 0x8D, 0xF6, 0xD4, 0xE1, 0xB9, 
		0xDE, 0xB4, 0x16, 0xBD, 0xDF, 0x6A, 0x25, 0x9C, 0xAC, 0xCD, 0x91, 0xF7, 
		0xCB, 0xC1, 0x81, 0x22, 0x0D, 0xF4, 0x7E, 0xEC, 0x0C, 0x84, 0x13, 0x5A, 
		0x74, 0x59, 0x3F, 0x3E, 0x61, 0x00, 0xD6, 0xB5, 0x4A, 0xA1, 0x04, 0xB5, 
		0xA7, 0x1C, 0x29, 0xD0, 0xE1, 0x11, 0x19, 0xD7, 0x80, 0x5C, 0xEE, 0x08, 
		0x15, 0xEB, 0xC9, 0xA8, 0x98, 0xF5, 0xA0, 0xF0, 0x92, 0x2A, 0xB0, 0xD3, 
		0xC7, 0x8C, 0x8D, 0xBB, 0x88, 0x96, 0x4F, 0x18, 0xF0, 0x8A, 0xF9, 0x31, 
		0x9E, 0x44, 0x94, 0x75, 0x6F, 0x78, 0x04, 0x10, 0xEC, 0xF3, 0xB0, 0xCE, 
		0xA0, 0xBE, 0x7B, 0x25, 0xE1, 0xF7, 0x8A, 0xA8, 0xD4, 0x63, 0xC2, 0x65, 
		0x47, 0xCC, 0x5C, 0xED, 0x7D, 0x8B, 0x07, 0x4D, 0x76, 0x29, 0x53, 0xAC, 
		0x27, 0x8F, 0x5D, 0x78, 0x56, 0xFA, 0x99, 0x45, 0xA2, 0xCC, 0x65, 0xC4, 
		0x54, 0x13, 0x9F, 0x38, 0x41, 0x7A, 0x61, 0x0E, 0x0D, 0x34, 0xBC, 0x11, 
		0xAF, 0xE2, 0xF1, 0x8B, 0xFA, 0x2B, 0x54, 0x6C, 0xA3, 0x6C, 0x09, 0x1F, 
		0x0B, 0x43, 0x9B, 0x07, 0x95, 0x83, 0x3F, 0x97, 0x99, 0x89, 0xF5, 0x51, 
		0x41, 0xF6, 0x8E, 0x5D, 0xEF, 0x6D, 0x24, 0x71, 0x41, 0x7A, 0xAF, 0xBE, 
		0x81, 0x71, 0xAB, 0x76, 0x2F, 0x1A, 0x5A, 0xBA, 0xF3, 0xA6, 0x65, 0x7A, 
		0x80, 0x50, 0xCE, 0x23, 0xC3, 0xC7, 0x53, 0xB0, 0x7C, 0x97, 0x77, 0x27, 
		0x70, 0x98, 0xAE, 0xB5, 0x24, 0x66, 0xE1, 0x60, 0x39, 0x41, 0xDA, 0x54, 
		0x01, 0x64, 0xFB, 0x10, 0x33, 0xCE, 0x8B, 0xBE, 0x27, 0xD4, 0x21, 0x57, 
		0xCC, 0x0F, 0x1A, 0xC1, 0x3D, 0xF3, 0xCC, 0x39, 0xF0, 0x2F, 0xAE, 0xF1, 
		0xC0, 0xCD, 0x3B, 0x23, 0x87, 0x49, 0x7E, 0x40, 0x32, 0x6A, 0xD3, 0x96, 
		0x4A, 0xE5, 0x5E, 0x6E, 0x26, 0xFD, 0x8A, 0xCF, 0x7E, 0xFC, 0x37, 0xDE, 
		0x39, 0x0C, 0x53, 0x81, 0x75, 0x08, 0xAF, 0x6B, 0x39, 0x6C, 0xFB, 0xC9, 
		0x79, 0xC0, 0x9B, 0x5F, 0x34, 0x86, 0xB2, 0xDE, 0xC4, 0x19, 0x84, 0x5F, 
		0x0E, 0xED, 0x9B, 0xB8, 0xD3, 0x17, 0xDA, 0x78 };

	string tempDir = Path.Combine (Path.GetTempPath (), typeof (SaveTest).FullName);

	[SetUp]
	protected void SetUp ()
	{
		var rand = new Random ();
		string basePath = tempDir;
		while (Directory.Exists (tempDir))
			tempDir = Path.Combine (basePath, rand.Next ().ToString ());
		Directory.CreateDirectory (tempDir);
	}

	[TearDown]
	protected void TearDown ()
	{
		try {
			// This throws an exception under MS.NET, since the directory contains loaded
			// assemblies.
			Directory.Delete (tempDir, true);
		} catch (Exception) {
		}
	}

	[Test]
	public void Save () {

		//
		// Create a test assembly, write it to disk, then read it back
		//
		AssemblyName aname = new AssemblyName ("h");
		// AssemblyName properties
		aname.ProcessorArchitecture = ProcessorArchitecture.X86;
		aname.Version = new Version (1, 2, 3, 4);
		aname.CultureInfo = new CultureInfo ("en");
		aname.Flags = AssemblyNameFlags.Retargetable;
		aname.HashAlgorithm = AssemblyHashAlgorithm.SHA256;
		var ab = AppDomain.CurrentDomain.DefineDynamicAssembly (aname, AssemblyBuilderAccess.RunAndSave, tempDir);

		string strongfile = Path.Combine (tempDir, "strongname.snk");
		using (FileStream fs = File.OpenWrite (strongfile)) {
			fs.Write (strongName, 0, strongName.Length);
			fs.Close ();
		}
		ab.SetCustomAttribute (new CustomAttributeBuilder (typeof (AssemblyKeyFileAttribute).GetConstructor (new Type [] { typeof (string) }), new object [] { strongfile }));
		ab.SetCustomAttribute (new CustomAttributeBuilder (typeof (AssemblyDelaySignAttribute).GetConstructor (new Type [] { typeof (bool) }), new object [] { true }));

		var cattrb = new CustomAttributeBuilder (typeof (AttributeUsageAttribute).GetConstructor (new Type [] { typeof (AttributeTargets) }), new object [] { AttributeTargets.Class },
												 new PropertyInfo[] { typeof (AttributeUsageAttribute).GetProperty ("AllowMultiple") },
												 new object[] { true },
												 new FieldInfo [0], new object [0]);
		ab.SetCustomAttribute (cattrb);

		var moduleb = ab.DefineDynamicModule ("h.exe", "h.exe");
		moduleb.SetCustomAttribute (cattrb);

		TypeBuilder iface1 = moduleb.DefineType ("iface1", TypeAttributes.Public|TypeAttributes.Interface, typeof (object));
		iface1.CreateType ();

		// Interfaces, attributes, class size, packing size
		TypeBuilder tb1 = moduleb.DefineType ("type1", TypeAttributes.Public|TypeAttributes.SequentialLayout, typeof (object), PackingSize.Size2, 16);
		tb1.AddInterfaceImplementation (iface1);
		tb1.AddInterfaceImplementation (typeof (IComparable));
		tb1.SetCustomAttribute (cattrb);
		tb1.CreateType ();

		// Nested type
		TypeBuilder tb_nested = tb1.DefineNestedType ("type_nested", TypeAttributes.NestedPublic, typeof (object));
		tb_nested.CreateType ();

		// Generics
		TypeBuilder tbg = moduleb.DefineType ("gtype1", TypeAttributes.Public, typeof (object));
		var gparams = tbg.DefineGenericParameters ("K", "T");
		// Constraints
		gparams [0].SetBaseTypeConstraint (null);
		gparams [0].SetInterfaceConstraints (new Type [] { typeof (IComparable) });
		gparams [0].SetCustomAttribute (cattrb);
		gparams [1].SetBaseTypeConstraint (tbg);
		// Type param
		tbg.DefineField ("field_gparam", tbg.GetGenericArguments () [0], FieldAttributes.Public|FieldAttributes.Static);
		// Open type
		tbg.DefineField ("field_open", typeof (List<>).MakeGenericType (new Type [] { tbg.GetGenericArguments () [1] }), FieldAttributes.Public|FieldAttributes.Static);
		tbg.CreateType ();

		TypeBuilder tbg2 = moduleb.DefineType ("gtype2", TypeAttributes.Public, typeof (object));
		tbg2.DefineGenericParameters ("K", "T");
		tbg2.CreateType ();

		TypeBuilder tb3 = moduleb.DefineType ("type3", TypeAttributes.Public, typeof (object));
		// Nested type
		tb3.DefineField ("field_nested", tb_nested, FieldAttributes.Public|FieldAttributes.Static);
		// Nested type ref
		tb3.DefineField ("field_nested_ref", typeof (TimeZoneInfo.AdjustmentRule), FieldAttributes.Public|FieldAttributes.Static);
		// Primitive types
		tb3.DefineField ("field_int", typeof (int), FieldAttributes.Public|FieldAttributes.Static);
		// Typeref array
		tb3.DefineField ("field_array_typeref", typeof (object[]), FieldAttributes.Public|FieldAttributes.Static);
		// Type szarray
		tb3.DefineField ("field_szarray", tb1.MakeArrayType (), FieldAttributes.Public|FieldAttributes.Static);
		// Multi-dim non szarray
		tb3.DefineField ("field_non_szarray", Array.CreateInstance (typeof (int), new int [] { 10 }, new int [] { 1 }).GetType (), FieldAttributes.Public|FieldAttributes.Static);
		// Multi-dim array
		tb3.DefineField ("field_multi_dim_array", Array.CreateInstance (typeof (int), new int [] { 10, 10 }, new int [] { 1, 1 }).GetType (), FieldAttributes.Public|FieldAttributes.Static);
		// Type pointer
		tb3.DefineField ("field_pointer", tb1.MakePointerType (), FieldAttributes.Public|FieldAttributes.Static);
		// Generic instance
		tb3.DefineField ("field_ginst", typeof (List<int>), FieldAttributes.Public|FieldAttributes.Static);
		// Generic instance of tbuilder
		tb3.DefineField ("field_ginst_tbuilder", tbg2.MakeGenericType (new Type [] { typeof (int), typeof (string) }), FieldAttributes.Public|FieldAttributes.Static);
		tb3.CreateType ();

		// Fields
		TypeBuilder tb_fields = moduleb.DefineType ("type4", TypeAttributes.Public, typeof (object));
		// Field with a constant
		tb_fields.DefineField ("field_int", typeof (int), FieldAttributes.Public|FieldAttributes.Static|FieldAttributes.HasDefault).SetConstant (42);
		// Field with an offset
		tb_fields.DefineField ("field_offset", typeof (int), FieldAttributes.Public|FieldAttributes.Static).SetOffset (64);
		// Modreq/modopt
		tb_fields.DefineField ("field_modopt", typeof (int), new Type [] { typeof (int) }, new Type [] { typeof (uint) }, FieldAttributes.Public|FieldAttributes.Static);
		// Marshal
		var fb = tb_fields.DefineField ("field_marshal1", typeof (int), FieldAttributes.Public);
		fb.SetCustomAttribute (new CustomAttributeBuilder (typeof (MarshalAsAttribute).GetConstructor (new Type [] { typeof (UnmanagedType) }), new object [] { UnmanagedType.U4 }));
		fb = tb_fields.DefineField ("field_marshal_byval_array", typeof (int), FieldAttributes.Public);
		fb.SetCustomAttribute (new CustomAttributeBuilder (typeof (MarshalAsAttribute).GetConstructor (new Type [] { typeof (UnmanagedType) }), new object [] { UnmanagedType.ByValArray },
														   new FieldInfo[] { typeof (MarshalAsAttribute).GetField ("SizeConst") },
														   new object[] { 16 }));
		fb = tb_fields.DefineField ("field_marshal_byval_tstr", typeof (int), FieldAttributes.Public);
		fb.SetCustomAttribute (new CustomAttributeBuilder (typeof (MarshalAsAttribute).GetConstructor (new Type [] { typeof (UnmanagedType) }), new object [] { UnmanagedType.ByValTStr },
														   new FieldInfo[] { typeof (MarshalAsAttribute).GetField ("SizeConst") },
														   new object[] { 16 }));
#if false
		fb = tb_fields.DefineField ("field_marshal_custom", typeof (int), FieldAttributes.Public);
		fb.SetCustomAttribute (new CustomAttributeBuilder (typeof (MarshalAsAttribute).GetConstructor (new Type [] { typeof (UnmanagedType) }), new object [] { UnmanagedType.CustomMarshaler },
														   new FieldInfo[] { typeof (MarshalAsAttribute).GetField ("MarshalTypeRef"),
																			 typeof (MarshalAsAttribute).GetField ("MarshalCookie") },
														   new object [] { typeof (object), "Cookie" }));
#endif
		// Cattr
		fb = tb_fields.DefineField ("field_cattr", typeof (int), FieldAttributes.Public|FieldAttributes.Static);
		fb.SetCustomAttribute (cattrb);
		tb_fields.CreateType ();

		// Data
		moduleb.DefineUninitializedData ("data1", 16, FieldAttributes.Public);
		moduleb.DefineInitializedData ("data2", new byte[] { 1, 2, 3, 4, 5, 6 }, FieldAttributes.Public);

		// Methods and signatures
		TypeBuilder tb5 = moduleb.DefineType ("type_methods", TypeAttributes.Public, typeof (object));
		// .ctor
		var cmods_req_1 = new Type [] { typeof (object) };
		var cmods_opt_1 = new Type [] { typeof (int) };
		var ctorb = tb5.DefineConstructor (MethodAttributes.Public, CallingConventions.VarArgs, new Type [] { typeof (int), typeof (object) }, new Type[][] { cmods_req_1, null }, new Type [][] { cmods_opt_1, null });
		ctorb.SetImplementationFlags (MethodImplAttributes.NoInlining);
		ctorb.GetILGenerator ().Emit (OpCodes.Ret);
		// Parameters
		var paramb = ctorb.DefineParameter (1, ParameterAttributes.None, "param1");
		paramb.SetConstant (16);
		paramb.SetCustomAttribute (cattrb);
		paramb = ctorb.DefineParameter (2, ParameterAttributes.Out, "param2");
		//paramb.SetCustomAttribute (new CustomAttributeBuilder (typeof (MarshalAsAttribute).GetConstructor (new Type [] { typeof (UnmanagedType) }), new object [] { UnmanagedType.U4 }));
		// .cctor
		var ctorb2 = tb5.DefineConstructor (MethodAttributes.Public|MethodAttributes.Static, CallingConventions.Standard, new Type [] { typeof (int), typeof (object) });
		ctorb2.GetILGenerator ().Emit (OpCodes.Ret);
		// method
		var mb = tb5.DefineMethod ("method1", MethodAttributes.Public, CallingConventions.Standard, typeof (int), cmods_req_1, cmods_opt_1, new Type [] { typeof (int), typeof (object) }, new Type [][] { cmods_req_1, null }, new Type [][] { cmods_opt_1, null });
		mb.SetImplementationFlags (MethodImplAttributes.NoInlining);
		mb.GetILGenerator ().Emit (OpCodes.Ret);
		gparams = mb.DefineGenericParameters ("K", "T");
		// Constraints
		gparams [0].SetBaseTypeConstraint (null);
		gparams [0].SetInterfaceConstraints (new Type [] { typeof (IComparable) });
		paramb = mb.DefineParameter (1, ParameterAttributes.None, "param1");
		paramb.SetConstant (16);
		paramb = mb.DefineParameter (2, ParameterAttributes.Out, "param2");
		//paramb.SetCustomAttribute (new CustomAttributeBuilder (typeof (MarshalAsAttribute).GetConstructor (new Type [] { typeof (UnmanagedType) }), new object [] { UnmanagedType.U4 }));
		// return value
		paramb = mb.DefineParameter (0, ParameterAttributes.None, "ret");
		//paramb.SetCustomAttribute (new CustomAttributeBuilder (typeof (MarshalAsAttribute).GetConstructor (new Type [] { typeof (UnmanagedType) }), new object [] { UnmanagedType.U4 }));
		paramb.SetCustomAttribute (cattrb);
		// override method
		tb5.AddInterfaceImplementation (typeof (IComparable));
		mb = tb5.DefineMethod ("method_override", MethodAttributes.Public|MethodAttributes.Virtual, CallingConventions.Standard|CallingConventions.HasThis, typeof (int), new Type [] { typeof (object) });
		mb.GetILGenerator ().Emit (OpCodes.Ret);
		tb5.DefineMethodOverride (mb, typeof (IComparable).GetMethod ("CompareTo"));
		tb5.CreateType ();

		// Properties
		TypeBuilder tb_properties = moduleb.DefineType ("type_properties", TypeAttributes.Public, typeof (object));
		var mb_get = tb_properties.DefineMethod ("get_method1", MethodAttributes.Public, CallingConventions.Standard, typeof (int), new Type [] { });
		mb_get.GetILGenerator ().Emit (OpCodes.Ret);
		var mb_set = tb_properties.DefineMethod ("set_method1", MethodAttributes.Public, CallingConventions.Standard, typeof (int), new Type [] { });
		mb_set.GetILGenerator ().Emit (OpCodes.Ret);
		var mb_other = tb_properties.DefineMethod ("other_method1", MethodAttributes.Public, CallingConventions.Standard, typeof (int), new Type [] { });
		mb_other.GetILGenerator ().Emit (OpCodes.Ret);
		var propertyb = tb_properties.DefineProperty ("AProperty", PropertyAttributes.HasDefault, typeof (int), new Type[] { typeof (object) });
		propertyb.SetCustomAttribute (cattrb);
		propertyb.SetConstant (1);
		propertyb.SetGetMethod (mb_get);
		propertyb.SetSetMethod (mb_set);
		propertyb.AddOtherMethod (mb_other);
		tb_properties.CreateType ();

		// Events
		TypeBuilder tb_events = moduleb.DefineType ("type_events", TypeAttributes.Public, typeof (object));
		var mb_add = tb_events.DefineMethod ("add_method1", MethodAttributes.Public, CallingConventions.Standard, typeof (int), new Type [] { });
		mb_add.GetILGenerator ().Emit (OpCodes.Ret);
		var mb_raise = tb_events.DefineMethod ("raise_method1", MethodAttributes.Public, CallingConventions.Standard, typeof (int), new Type [] { });
		mb_raise.GetILGenerator ().Emit (OpCodes.Ret);
		var mb_remove = tb_events.DefineMethod ("remove_method1", MethodAttributes.Public, CallingConventions.Standard, typeof (int), new Type [] { });
		mb_remove.GetILGenerator ().Emit (OpCodes.Ret);
		var eventb = tb_events.DefineEvent ("Event1", EventAttributes.SpecialName, typeof (int));
		eventb.SetCustomAttribute (cattrb);
		eventb.SetAddOnMethod (mb_add);
		eventb.SetRaiseMethod (mb_raise);
		eventb.SetRemoveOnMethod (mb_remove);
		tb_events.CreateType ();

		ab.Save ("h.exe");

		// Read the assembly and check data
		Assembly a = Assembly.LoadFrom (Path.Combine (tempDir, "h.exe"));
		Assert.IsTrue (a != ab);
		CheckAssembly (a);
	}

	void CheckCattr (ICustomAttributeProvider obj) {
		var cattrs = obj.GetCustomAttributes (typeof (AttributeUsageAttribute), false);
		Assert.AreEqual (1, cattrs.Length);
		var cattr = (AttributeUsageAttribute)cattrs [0];
		Assert.AreEqual (AttributeTargets.Class, cattr.ValidOn);
		Assert.IsTrue (cattr.AllowMultiple);
	}

	void CheckAssembly (Assembly a) {
		// AssemblyName properties
		var aname = a.GetName (false);
		Assert.AreEqual (new Version (1, 2, 3, 4), aname.Version);
		Assert.AreEqual ("en", aname.CultureInfo.Name);
		Assert.IsTrue ((aname.Flags & AssemblyNameFlags.Retargetable) > 0);
		//Assert.AreEqual (AssemblyHashAlgorithm.SHA256, aname.HashAlgorithm);
		CheckCattr (a);

		var iface1 = a.GetType ("iface1");
		var gtype2 = a.GetType ("gtype2");

		var type1 = a.GetType ("type1");
		Assert.IsNotNull (type1);

		// Type attributes
		Assert.AreEqual (TypeAttributes.Public|TypeAttributes.SequentialLayout, type1.Attributes);
		// Interfaces
		var ifaces = type1.GetInterfaces ();
		Assert.AreEqual (2, ifaces.Length);
		Assert.IsTrue (iface1 == ifaces [0] || iface1 == ifaces [1]);
		Assert.IsTrue (typeof (IComparable) == ifaces [0] || typeof (IComparable) == ifaces [1]);
		CheckCattr (type1);
		// FIXME: Class size/packing size

		// Nested types
		var type_nested = a.GetType ("type1/type_nested");
		Assert.IsNotNull (type_nested);

		// Generics
		var gtype1 = a.GetType ("gtype1");
		Assert.IsTrue (gtype1.IsGenericTypeDefinition);
		// Generic parameters
		var gparams = gtype1.GetGenericArguments ();
		Assert.AreEqual (2, gparams.Length);
		Assert.AreEqual ("K", gparams [0].Name);
		Assert.AreEqual ("T", gparams [1].Name);
		var constraints = gparams [0].GetGenericParameterConstraints ();
		Assert.AreEqual (2, constraints.Length);
		Assert.AreEqual (typeof (object), constraints [0]);
		Assert.AreEqual (typeof (IComparable), constraints [1]);
		CheckCattr (gparams [0]);
		constraints = gparams [1].GetGenericParameterConstraints ();
		Assert.AreEqual (1, constraints.Length);
		Assert.AreEqual (gtype1, constraints [0]);
		// Type param encoding
		var field = gtype1.GetField ("field_gparam");
		Assert.AreEqual (gparams [0], field.FieldType);
		field = gtype1.GetField ("field_open");
		Assert.AreEqual (typeof (List<>).MakeGenericType (new Type [] { gparams [1] }), field.FieldType);

		// Type encoding
		var t = a.GetType ("type3");
		Assert.AreEqual (type_nested, t.GetField ("field_nested").FieldType);
		Assert.AreEqual (typeof (TimeZoneInfo.AdjustmentRule), t.GetField ("field_nested_ref").FieldType);
		Assert.AreEqual (typeof (int), t.GetField ("field_int").FieldType);
		Assert.AreEqual (typeof (object[]), t.GetField ("field_array_typeref").FieldType);
		Assert.AreEqual (type1.MakeArrayType (), t.GetField ("field_szarray").FieldType);
		var arraytype1 = Array.CreateInstance (typeof (int), new int [] { 10 }, new int [] { 1 }).GetType ();
		// FIXME:
		//Assert.AreEqual (arraytype1, t.GetField ("field_non_szarray").FieldType);
		arraytype1 = Array.CreateInstance (typeof (int), new int [] { 10, 10 }, new int [] { 1, 1 }).GetType ();
		Assert.AreEqual (arraytype1, t.GetField ("field_multi_dim_array").FieldType);
		Assert.AreEqual (type1.MakePointerType (), t.GetField ("field_pointer").FieldType);
		Assert.AreEqual (typeof (List<int>), t.GetField ("field_ginst").FieldType);
		var ginsttype = gtype2.MakeGenericType (new Type [] { typeof (int), typeof (string) });
		Assert.AreEqual (ginsttype, t.GetField ("field_ginst_tbuilder").FieldType);

		// Field properties
		var type4 = a.GetType ("type4");
		// FIXME: constant
		field = type4.GetField ("field_int");
		// FIXME: field offset
		field = type4.GetField ("field_offset");
		//var attrs = field.GetCustomAttributes (typeof (FieldOffsetAttribute), true);
		field = type4.GetField ("field_modopt");
		var cmods = field.GetRequiredCustomModifiers ();
		Assert.AreEqual (1, cmods.Length);
		Assert.AreEqual (typeof (int), cmods [0]);
		cmods = field.GetOptionalCustomModifiers ();
		Assert.AreEqual (1, cmods.Length);
		Assert.AreEqual (typeof (uint), cmods [0]);
		// FIXME: marshal
		// Simple marshal
		field = type4.GetField ("field_marshal1");
		var attrs = field.GetCustomAttributes (typeof (MarshalAsAttribute), true);
		Assert.AreEqual (1, attrs.Length);
		var marshal = attrs [0] as MarshalAsAttribute;
		Assert.AreEqual (UnmanagedType.U4, marshal.Value);
		// ByValArray
		field = type4.GetField ("field_marshal_byval_array");
		attrs = field.GetCustomAttributes (typeof (MarshalAsAttribute), true);
		Assert.AreEqual (1, attrs.Length);
		marshal = attrs [0] as MarshalAsAttribute;
		Assert.AreEqual (UnmanagedType.ByValArray, marshal.Value);
		Assert.AreEqual (16, marshal.SizeConst);
		// ByValTStr
		field = type4.GetField ("field_marshal_byval_tstr");
		attrs = field.GetCustomAttributes (typeof (MarshalAsAttribute), true);
		Assert.AreEqual (1, attrs.Length);
		marshal = attrs [0] as MarshalAsAttribute;
		Assert.AreEqual (UnmanagedType.ByValTStr, marshal.Value);
		Assert.AreEqual (16, marshal.SizeConst);
#if false
		// Custom marshaler
		field = type4.GetField ("field_marshal_custom");
		attrs = field.GetCustomAttributes (typeof (MarshalAsAttribute), true);
		Assert.AreEqual (1, attrs.Length);
		marshal = attrs [0] as MarshalAsAttribute;
		Assert.AreEqual (UnmanagedType.CustomMarshaler, marshal.Value);
		Assert.AreEqual (typeof (object), marshal.MarshalTypeRef);
		Assert.AreEqual ("Cookie", marshal.MarshalCookie);
#endif
		field = type4.GetField ("field_cattr");
		CheckCattr (field);

		// Global fields
		field = a.ManifestModule.GetField ("data1");
		Assert.IsNotNull (field);
		field = a.ManifestModule.GetField ("data2");
		Assert.IsNotNull (field);

		// Methods and signatures
		var type_methods = a.GetType ("type_methods");
		var ctors = type_methods.GetConstructors (BindingFlags.Public|BindingFlags.Static|BindingFlags.Instance);
		Assert.AreEqual (2, ctors.Length);
		// .ctor
		var ctor = type_methods.GetConstructor (new Type[] { typeof (int), typeof (object) });
		Assert.IsNotNull (ctor);
		Assert.AreEqual (MethodImplAttributes.NoInlining|MethodImplAttributes.IL, ctor.GetMethodImplementationFlags ());
		//Assert.AreEqual (CallingConventions.VarArgs, ctor.CallingConvention);
		// .cctor
		ctors = type_methods.GetConstructors (BindingFlags.Public|BindingFlags.Static);
		Assert.AreEqual (1, ctors.Length);
		// parameters
		ctor = type_methods.GetConstructor (new Type[] { typeof (int), typeof (object) });
		Assert.IsNotNull (ctor);
		var parameters = ctor.GetParameters ();
		Assert.AreEqual (2, parameters.Length);
		Assert.AreEqual ("param1", parameters [0].Name);
		Assert.AreEqual (typeof (int), parameters [0].ParameterType);
		Assert.AreEqual (ParameterAttributes.HasDefault, parameters [0].Attributes);
		Assert.AreEqual (16, parameters [0].RawDefaultValue);
		CheckCattr (parameters [0]);
		cmods = parameters [0].GetRequiredCustomModifiers ();
		Assert.AreEqual (1, cmods.Length);
		Assert.AreEqual (typeof (object), cmods [0]);
		cmods = parameters [0].GetOptionalCustomModifiers ();
		Assert.AreEqual (1, cmods.Length);
		Assert.AreEqual (typeof (int), cmods [0]);
		Assert.AreEqual ("param2", parameters [1].Name);
#if false
		Assert.AreEqual (ParameterAttributes.Out|ParameterAttributes.HasFieldMarshal, parameters [1].Attributes);
		Assert.AreEqual (typeof (object), parameters [1].ParameterType);
		attrs = parameters [1].GetCustomAttributes (typeof (MarshalAsAttribute), true);
		Assert.AreEqual (1, attrs.Length);
		marshal = attrs [0] as MarshalAsAttribute;
		Assert.AreEqual (UnmanagedType.U4, marshal.Value);
#endif
		// methods
		var method = type_methods.GetMethod ("method1");
		Assert.IsNotNull (method);
		Assert.AreEqual (typeof (int), method.ReturnType);
		Assert.AreEqual (MethodImplAttributes.NoInlining|MethodImplAttributes.IL, method.GetMethodImplementationFlags ());
		gparams = gtype1.GetGenericArguments ();
		Assert.AreEqual (2, gparams.Length);
		Assert.AreEqual ("K", gparams [0].Name);
		Assert.AreEqual ("T", gparams [1].Name);
		constraints = gparams [0].GetGenericParameterConstraints ();
		Assert.AreEqual (2, constraints.Length);
		Assert.AreEqual (typeof (object), constraints [0]);
		Assert.AreEqual (typeof (IComparable), constraints [1]);
		parameters = method.GetParameters ();
		// method parameters
		Assert.AreEqual (2, parameters.Length);
		Assert.AreEqual ("param1", parameters [0].Name);
		Assert.AreEqual (typeof (int), parameters [0].ParameterType);
		Assert.AreEqual (ParameterAttributes.HasDefault, parameters [0].Attributes);
		Assert.AreEqual (16, parameters [0].RawDefaultValue);
		cmods = parameters [0].GetRequiredCustomModifiers ();
		Assert.AreEqual (1, cmods.Length);
		Assert.AreEqual (typeof (object), cmods [0]);
		cmods = parameters [0].GetOptionalCustomModifiers ();
		Assert.AreEqual (1, cmods.Length);
		Assert.AreEqual (typeof (int), cmods [0]);
		Assert.AreEqual ("param2", parameters [1].Name);
#if false
		Assert.AreEqual (ParameterAttributes.Out|ParameterAttributes.HasFieldMarshal, parameters [1].Attributes);
		Assert.AreEqual (typeof (object), parameters [1].ParameterType);
		attrs = parameters [1].GetCustomAttributes (typeof (MarshalAsAttribute), true);
		Assert.AreEqual (1, attrs.Length);
		marshal = attrs [0] as MarshalAsAttribute;
		Assert.AreEqual (UnmanagedType.U4, marshal.Value);
#endif
		// return type
		var rparam = method.ReturnParameter;
		cmods = rparam.GetRequiredCustomModifiers ();
		Assert.AreEqual (1, cmods.Length);
		Assert.AreEqual (typeof (object), cmods [0]);
		cmods = rparam.GetOptionalCustomModifiers ();
		Assert.AreEqual (1, cmods.Length);
		Assert.AreEqual (typeof (int), cmods [0]);
#if false
		attrs = rparam.GetCustomAttributes (typeof (MarshalAsAttribute), true);
		Assert.AreEqual (1, attrs.Length);
		marshal = attrs [0] as MarshalAsAttribute;
		Assert.AreEqual (UnmanagedType.U4, marshal.Value);
#endif
		CheckCattr (rparam);

		// Properties
		var type_props = a.GetType ("type_properties");
		var prop = type_props.GetProperty ("AProperty");
		Assert.IsNotNull (prop);
		Assert.AreEqual (PropertyAttributes.HasDefault, prop.Attributes);
		var getter = prop.GetGetMethod ();
		Assert.IsNotNull (getter);
		Assert.AreEqual ("get_method1", getter.Name);
		var setter = prop.GetSetMethod ();
		Assert.IsNotNull (setter);
		Assert.AreEqual ("set_method1", setter.Name);
		CheckCattr (prop);

		// Events
		var type_events = a.GetType ("type_events");
		var ev = type_events.GetEvent ("Event1");
		Assert.IsNotNull (ev);
		var m = ev.AddMethod;
		Assert.IsNotNull (m);
		Assert.AreEqual ("add_method1", m.Name);
		m = ev.RemoveMethod;
		Assert.IsNotNull (m);
		Assert.AreEqual ("remove_method1", m.Name);
		m = ev.RaiseMethod;
		Assert.IsNotNull (m);
		Assert.AreEqual ("raise_method1", m.Name);
		Assert.AreEqual (EventAttributes.SpecialName, ev.Attributes);
		CheckCattr (ev);
	}
}
}
