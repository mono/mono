
//
// TypeBuilderTest.cs - NUnit Test Cases for the TypeBuilder class
//
// Zoltan Varga (vargaz@freemail.hu)
//
// (C) Ximian, Inc.  http://www.ximian.com
//
// TODO:
//  - implement a mechnanism for easier testing of null argument exceptions
//  - with overloaded methods like DefineNestedType (), check the defaults
//    on the shorter versions.
//  - ToString on enums with the flags attribute set should print all
//    values which match, e.g. 0 == AutoLayou,AnsiClass,NotPublic
//

using System;
using System.Threading;
using System.Reflection;
using System.Reflection.Emit;
using System.Security;
using System.Security.Permissions;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace MonoTests.System.Reflection.Emit
{

[TestFixture]
public class TypeBuilderTest : Assertion
{	
	private interface AnInterface {
	}

	private AssemblyBuilder assembly;

	private ModuleBuilder module;

	static string ASSEMBLY_NAME = "MonoTests.System.Reflection.Emit.TypeBuilderTest";

	[SetUp]
	protected void SetUp () {
		AssemblyName assemblyName = new AssemblyName();
		assemblyName.Name = ASSEMBLY_NAME;

		assembly = 
			Thread.GetDomain().DefineDynamicAssembly(
				assemblyName, AssemblyBuilderAccess.RunAndSave, "c:\\");

		module = assembly.DefineDynamicModule("module1");
	}

	static int typeIndexer = 0;

	// Return a unique type name
	private string genTypeName () {
		return "t" + (typeIndexer ++);
	}

	private string nullName () {
		return String.Format ("{0}", (char)0);
	}

	[Test]
	public void TestAssembly () {
		TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
		AssertEquals ("Assembly works",
					  tb.Assembly, assembly);
	}

	[Test]
	public void TestAssemblyQualifiedName () {
		TypeBuilder tb = module.DefineType ("A.B.C.D", TypeAttributes.Public);

		AssertEquals ("AssemblyQualifiedName works",
					  tb.AssemblyQualifiedName, "A.B.C.D, " + assembly.GetName ().FullName);
	}

	[Test]
	public void TestAttributes () {
		TypeAttributes attrs = TypeAttributes.Public | TypeAttributes.BeforeFieldInit;
		TypeBuilder tb = module.DefineType (genTypeName (), attrs);

		AssertEquals ("Attributes works",
					  tb.Attributes, attrs);
	}

	[Test]
	public void TestBaseTypeClass () {
		TypeAttributes attrs = TypeAttributes.Public;
		TypeBuilder tb = module.DefineType (genTypeName (), attrs);
		AssertEquals ("BaseType defaults to Object",
					  tb.BaseType, typeof (object));

		TypeBuilder tb2 = module.DefineType (genTypeName (), attrs, tb);
		AssertEquals ("BaseType works",
					  tb2.BaseType, tb);
	}

	[Test]
	public void TestBaseTypeInterface ()
	{
		TypeBuilder tb3 = module.DefineType (genTypeName (), TypeAttributes.Interface | TypeAttributes.Abstract);
		AssertEquals ("Interfaces should default to no base type", null, tb3.BaseType);
	}

	[Test]
	public void TestDeclaringType () {
		TypeAttributes attrs = 0;
		TypeBuilder tb = module.DefineType (genTypeName (), attrs);

		AssertEquals ("Has no declaring type",
					  null, tb.DeclaringType);

		attrs = TypeAttributes.NestedPublic;
		TypeBuilder tb2 = tb.DefineNestedType (genTypeName (), attrs);
		TypeBuilder tb3 = tb2.DefineNestedType (genTypeName (), attrs);
		AssertEquals ("DeclaringType works",
					  tb, tb3.DeclaringType.DeclaringType);
	}

	[Test]
	public void TestFullName () {
		string name = genTypeName ();
		TypeAttributes attrs = 0;
		TypeBuilder tb = module.DefineType (name, attrs);
		AssertEquals ("FullName works",
					  name, tb.FullName);

		string name2 = genTypeName ();
		attrs = TypeAttributes.NestedPublic;
		TypeBuilder tb2 = tb.DefineNestedType (name2, attrs);

		string name3 = genTypeName ();
		attrs = TypeAttributes.NestedPublic;
		TypeBuilder tb3 = tb2.DefineNestedType (name3, attrs);

		AssertEquals ("FullName works on nested types",
					  name + "+" + name2 + "+" + name3, tb3.FullName);
	}

	[Test]
	[ExpectedException (typeof(NotSupportedException))]
	public void TestGUIDIncomplete () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		Guid g = tb.GUID;
	}

	[Test]
	public void TestGUIDComplete ()
	{
		TypeBuilder tb = module.DefineType (genTypeName ());
		tb.CreateType ();
		Assert(tb.GUID != Guid.Empty);
	}

	[Test]
	public void TestFixedGUIDComplete ()
	{
		TypeBuilder tb = module.DefineType (genTypeName ());

		Guid guid = Guid.NewGuid ();

		ConstructorInfo guidCtor = typeof(GuidAttribute).GetConstructor(
			new Type[] {typeof(string)});

		CustomAttributeBuilder caBuilder = new CustomAttributeBuilder (guidCtor,
			new object[] { guid.ToString("D") }, new FieldInfo[0], new object[0]);

		tb.SetCustomAttribute (caBuilder);
		tb.CreateType ();
		AssertEquals (guid, tb.GUID);
	}

	[Test]
	[ExpectedException (typeof(NotSupportedException))]
	public void TestHasElementType () {
		// According to the MSDN docs, this member works, but in reality, it
		// returns a NotSupportedException
		TypeBuilder tb = module.DefineType (genTypeName ());
		bool b = tb.HasElementType;
	}

	[Test]
	public void TestIsAbstract () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		AssertEquals ("",
					  false, tb.IsAbstract);

		TypeBuilder tb2 = module.DefineType (genTypeName (), TypeAttributes.Abstract);
		AssertEquals ("",
					  true, tb2.IsAbstract);
	}

	[Test]
	public void TestIsAnsiClass () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		AssertEquals ("",
					  true, tb.IsAnsiClass);

		TypeBuilder tb2 = module.DefineType (genTypeName (), TypeAttributes.UnicodeClass);
		AssertEquals ("",
					  false, tb2.IsAnsiClass);
	}

	[Test]
	public void TestIsArray () {
		// How can a TypeBuilder be an array ?
		string name = genTypeName ();
		TypeBuilder tb = module.DefineType (name);
		AssertEquals ("IsArray works",
					  false, tb.IsArray);
	}

	[Test]
	public void TestIsAutoClass () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		AssertEquals ("",
					  false, tb.IsAutoClass);

		TypeBuilder tb2 = module.DefineType (genTypeName (), TypeAttributes.AutoClass);
		AssertEquals ("",
					  true, tb2.IsAutoClass);
	}

	[Test]
	public void TestIsAutoLayout () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		AssertEquals ("AutoLayout defaults to true",
					  true, tb.IsAutoLayout);

		TypeBuilder tb2 = module.DefineType (genTypeName (), TypeAttributes.ExplicitLayout);
		AssertEquals ("",
					  false, tb2.IsAutoLayout);
	}

	[Test]
	public void TestIsByRef () {
		// How can a TypeBuilder be ByRef ?
		TypeBuilder tb = module.DefineType (genTypeName ());
		AssertEquals ("IsByRef works",
					  false, tb.IsByRef);
	}

	[Test]
	public void TestIsClass () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		AssertEquals ("Most types are classes",
					  true, tb.IsClass);

		TypeBuilder tb2 = module.DefineType (genTypeName (), TypeAttributes.Interface | TypeAttributes.Abstract);
		AssertEquals ("Interfaces are not classes",
					  false, tb2.IsClass);

		TypeBuilder tb3 = module.DefineType (genTypeName (), 0, typeof (ValueType));
		AssertEquals ("value types are not classes",
					  false, tb3.IsClass);

		TypeBuilder tb4 = module.DefineType (genTypeName (), 0, typeof (Enum));
		AssertEquals ("enums are not classes",
					  false, tb4.IsClass);
	}

	[Test]
	public void TestIsCOMObject () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		AssertEquals ("Probably not", false, tb.IsCOMObject);

		tb = module.DefineType (genTypeName (), TypeAttributes.Import);
		AssertEquals ("type with Import attribute is COM object",
			true, tb.IsCOMObject);
	}

	[Test]
	public void TestIsContextful () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		AssertEquals (false, tb.IsContextful);

		TypeBuilder tb2 = module.DefineType (genTypeName (), 0, typeof (ContextBoundObject));
		AssertEquals (true, tb2.IsContextful);
	}

	[Test]
	public void TestIsEnum () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		AssertEquals (false, tb.IsEnum);

		// This returns true under both mono and MS .NET ???
		TypeBuilder tb2 = module.DefineType (genTypeName (), 0, typeof (ValueType));
		AssertEquals ("value types are not necessary enums",
			false, tb2.IsEnum);

		TypeBuilder tb3 = module.DefineType (genTypeName (), 0, typeof (Enum));
		AssertEquals ("enums are enums", true, tb3.IsEnum);
	}

	[Test]
	public void TestIsExplicitLayout () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		AssertEquals ("ExplicitLayout defaults to false",
			false, tb.IsExplicitLayout);

		TypeBuilder tb2 = module.DefineType (genTypeName (), TypeAttributes.ExplicitLayout);
		AssertEquals (true, tb2.IsExplicitLayout);
	}

	[Test]
	public void TestIsImport () {
		// How can this be true ?
		TypeBuilder tb = module.DefineType (genTypeName ());
		AssertEquals (false, tb.IsImport);
	}

	[Test]
	public void TestIsInterface () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		AssertEquals ("Most types are not interfaces",
			false, tb.IsInterface);

		TypeBuilder tb2 = module.DefineType (genTypeName (), TypeAttributes.Interface | TypeAttributes.Abstract);
		AssertEquals ("Interfaces are interfaces",
			true, tb2.IsInterface);

		TypeBuilder tb3 = module.DefineType (genTypeName (), 0, typeof (ValueType));
		AssertEquals ("value types are not interfaces",
			false, tb3.IsInterface);
	}

	[Test]
	public void TestIsLayoutSequential () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		AssertEquals ("SequentialLayout defaults to false",
			false, tb.IsLayoutSequential);

		TypeBuilder tb2 = module.DefineType (genTypeName (), TypeAttributes.SequentialLayout);
		AssertEquals (true, tb2.IsLayoutSequential);
	}

	[Test]
	public void TestIsMarshalByRef () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		AssertEquals (false, tb.IsMarshalByRef);

		TypeBuilder tb2 = module.DefineType (genTypeName (), 0, typeof (MarshalByRefObject));
		AssertEquals (true, tb2.IsMarshalByRef);

		TypeBuilder tb3 = module.DefineType (genTypeName (), 0, typeof (ContextBoundObject));
		AssertEquals (true, tb3.IsMarshalByRef);
	}

	// TODO: Visibility properties

	[Test]
	public void TestIsPointer () {
		// How can this be true?
		TypeBuilder tb = module.DefineType (genTypeName ());
		AssertEquals (false, tb.IsPointer);
	}

	[Test]
	public void TestIsPrimitive () {
		TypeBuilder tb = module.DefineType ("int");
		AssertEquals (false, tb.IsPrimitive);
	}

	[Test]
	public void IsSealed () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		AssertEquals ("Sealed defaults to false",
			false, tb.IsSealed);

		TypeBuilder tb2 = module.DefineType (genTypeName (), TypeAttributes.Sealed);
		AssertEquals ("IsSealed works", true, tb2.IsSealed);
	}

	[Test]
	public void IsSerializable () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		AssertEquals (false, tb.IsSerializable);

		ConstructorInfo[] ctors = typeof (SerializableAttribute).GetConstructors (BindingFlags.Instance | BindingFlags.Public);
		Assert ("SerializableAttribute should have more than 0 public instance ctors", 
			ctors.Length > 0);

		tb.SetCustomAttribute (new CustomAttributeBuilder (ctors[0], new object[0]));
		Type createdType = tb.CreateType ();

		assembly.Save ("TestAssembly.dll");
		AssertEquals (true, createdType.IsSerializable);
	}

	[Test]
	public void TestIsSpecialName () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		AssertEquals ("SpecialName defaults to false",
			false, tb.IsSpecialName);

		TypeBuilder tb2 = module.DefineType (genTypeName (), TypeAttributes.SpecialName);
		AssertEquals ("IsSpecialName works",
			true, tb2.IsSpecialName);
	}

	[Test]
	public void TestIsUnicodeClass () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		AssertEquals (false, tb.IsUnicodeClass);

		TypeBuilder tb2 = module.DefineType (genTypeName (), TypeAttributes.UnicodeClass);
		AssertEquals (true, tb2.IsUnicodeClass);
	}

	[Test]
	public void TestIsValueType () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		AssertEquals ("Most types are not value types",
			false, tb.IsValueType);

		TypeBuilder tb2 = module.DefineType (genTypeName (), TypeAttributes.Interface | TypeAttributes.Abstract);
		AssertEquals ("Interfaces are not value types",
			false, tb2.IsValueType);

		TypeBuilder tb3 = module.DefineType (genTypeName (), 0, typeof (ValueType));
		AssertEquals ("value types are value types",
			true, tb3.IsValueType);

		TypeBuilder tb4 = module.DefineType (genTypeName (), 0, typeof (Enum));
		AssertEquals ("enums are value types",
			true, tb4.IsValueType);
	}

	[Test]
	public void TestMemberType () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		AssertEquals ("A type is a type",
			MemberTypes.TypeInfo, tb.MemberType);
	}

	[Test]
	public void TestModule () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		AssertEquals ("Module works", module, tb.Module);
	}

	[Test]
	public void TestName () {
		TypeBuilder tb = module.DefineType ("A");
		AssertEquals ("A", tb.Name);

		TypeBuilder tb2 = module.DefineType ("A.B.C.D.E");
		AssertEquals ("E", tb2.Name);

		TypeBuilder tb3 = tb2.DefineNestedType ("A");
		AssertEquals ("A", tb3.Name);

		/* Is .E a valid name ?
		TypeBuilder tb4 = module.DefineType (".E");
		AssertEquals ("",
					  "E", tb4.Name);
		*/
	}

	[Test]
	public void TestNamespace () {
		TypeBuilder tb = module.DefineType ("A");
		AssertEquals ("", tb.Namespace);

		TypeBuilder tb2 = module.DefineType ("A.B.C.D.E");
		AssertEquals ("A.B.C.D", tb2.Namespace);

		TypeBuilder tb3 = tb2.DefineNestedType ("A");
		AssertEquals ("", tb3.Namespace);

		/* Is .E a valid name ?
		TypeBuilder tb4 = module.DefineType (".E");
		AssertEquals ("",
					  "E", tb4.Name);
		*/		
	}

	[Test]
	public void TestPackingSize () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		AssertEquals (PackingSize.Unspecified, tb.PackingSize);

		TypeBuilder tb2 = module.DefineType (genTypeName (), 0, typeof (object),
			PackingSize.Size16, 16);
		AssertEquals (PackingSize.Size16, tb2.PackingSize);
	}

	[Test]
	public void TestReflectedType () {
		// It is the same as DeclaringType, but why?
		TypeBuilder tb = module.DefineType (genTypeName ());
		AssertEquals (null, tb.ReflectedType);

		TypeBuilder tb2 = tb.DefineNestedType (genTypeName ());
		AssertEquals (tb, tb2.ReflectedType);
	}

	[Test]
	[ExpectedException (typeof(ArgumentNullException))]
	public void TestSetParentNull ()
	{
		TypeBuilder tb = module.DefineType (genTypeName ());
		tb.SetParent (null);
	}

	[Test]
	public void TestSetParentIncomplete ()
	{
		TypeBuilder tb = module.DefineType (genTypeName ());
		tb.SetParent (typeof(Attribute));
		AssertEquals (typeof(Attribute), tb.BaseType);
	}

	[Test]
	[ExpectedException (typeof(InvalidOperationException))]
	public void TestSetParentComplete ()
	{
		TypeBuilder tb = module.DefineType (genTypeName ());
		tb.CreateType ();
		tb.SetParent (typeof(Attribute));
	}

	[Test]
	public void TestSize () {
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			AssertEquals (0, tb.Size);
			tb.CreateType ();
			AssertEquals (0, tb.Size);
		}

		{
			TypeBuilder tb = module.DefineType (genTypeName (), 0, typeof (object),
				PackingSize.Size16, 32);
			AssertEquals (32, tb.Size);
		}
	}

	[Test]
	[ExpectedException (typeof(NotSupportedException))]
	public void TestTypeHandle () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		RuntimeTypeHandle handle = tb.TypeHandle;
	}

	[Test]
	[ExpectedException (typeof(NotSupportedException))]
	public void TestTypeInitializerIncomplete ()
	{
		TypeBuilder tb = module.DefineType (genTypeName ());
		ConstructorInfo cb = tb.TypeInitializer;
	}

	[Test]
	public void TestTypeInitializerComplete ()
	{
		TypeBuilder tb = module.DefineType (genTypeName ());
		tb.CreateType ();
		ConstructorInfo cb = tb.TypeInitializer;
	}

	[Test]
	public void TestTypeToken () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		TypeToken token = tb.TypeToken;
	}

	[Test]
	public void TestUnderlyingSystemType () {
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			AssertEquals ("For non-enums this equals itself",
						  tb, tb.UnderlyingSystemType);
		}
		{
			TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Interface | TypeAttributes.Abstract);
			AssertEquals (tb, tb.UnderlyingSystemType);
		}
		{
			TypeBuilder tb = module.DefineType (genTypeName (), 0, typeof (ValueType));
			AssertEquals (tb, tb.UnderlyingSystemType);
		}

		{
			TypeBuilder tb = module.DefineType (genTypeName (), 0, typeof (Enum));
			try {
				Type t = tb.UnderlyingSystemType;
				Fail ();
			}
			catch (InvalidOperationException) {
			}

			tb.DefineField ("val", typeof (int), 0);
			AssertEquals (typeof (int), tb.UnderlyingSystemType);
		}
	}

	[Test]
	public void TestAddInterfaceImplementation () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		try {
			tb.AddInterfaceImplementation (null);
			Fail ();
		}
		catch (ArgumentNullException) {
		}

		tb.AddInterfaceImplementation (typeof (AnInterface));
		tb.AddInterfaceImplementation (typeof (AnInterface));

		Type t = tb.CreateType ();
		AssertEquals ("Should merge identical interfaces",
					  tb.GetInterfaces ().Length, 1);

		// Can not be called on a created type
		try {
			tb.AddInterfaceImplementation (typeof (AnInterface));
			Fail ();
		}
		catch (InvalidOperationException) {
		}
	}

	[Test]
	public void TestCreateType () {
		// TODO: LOTS OF TEST SHOULD GO THERE
		TypeBuilder tb = module.DefineType (genTypeName ());
		tb.CreateType ();

		// Can not be called on a created type
		try {
			tb.CreateType ();
			Fail ();
		}
		catch (InvalidOperationException) {
		}
	}

	[Test]
	public void TestDefineConstructor () {
		TypeBuilder tb = module.DefineType (genTypeName ());

		ConstructorBuilder cb = tb.DefineConstructor (0, 0, null);
		cb.GetILGenerator ().Emit (OpCodes.Ret);
		tb.CreateType ();

		// Can not be called on a created type
		try {
			tb.DefineConstructor (0, 0, null);
			Fail ();
		}
		catch (InvalidOperationException) {
		}
	}

	[Test]
	public void TestDefineDefaultConstructor () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		tb.DefineDefaultConstructor (0);
		tb.CreateType ();

		// Can not be called on a created type, altough the MSDN docs does not mention this
		try {
			tb.DefineDefaultConstructor (0);
			Fail ();
		}
		catch (InvalidOperationException) {
		}
	}

	[Test]
	[ExpectedException (typeof(NotSupportedException))]
	public void TestDefineDefaultConstructorParent () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		tb.DefineConstructor (MethodAttributes.Public,
			CallingConventions.Standard, 
			new Type[] { typeof(string) });
		Type type = tb.CreateType ();

		// create TypeBuilder for type that derived from the 
		// previously created type (which has no default ctor)
		tb = module.DefineType (genTypeName (), TypeAttributes.Class
			| TypeAttributes.Public, type);

		// you cannot create a type with a default ctor that
		// derives from a type without a default ctor
		tb.CreateType ();
	}

	[Test]
	public void TestDefineEvent () {
		TypeBuilder tb = module.DefineType (genTypeName ());

		// Test invalid arguments
		try {
			tb.DefineEvent (null, 0, typeof (int));
			Fail ();
		}
		catch (ArgumentNullException) {
		}

		try {
			tb.DefineEvent ("FOO", 0, null);
			Fail ();
		}
		catch (ArgumentNullException) {
		}

		try {
			tb.DefineEvent ("", 0, typeof (int));
			Fail ();
		}
		catch (ArgumentException) {
		}

		tb.CreateType ();
		// Can not be called on a created type
		try {
			tb.DefineEvent ("BAR", 0, typeof (int));
			Fail ();
		}
		catch (InvalidOperationException) {
		}
	}

	[Test]
	public void TestDefineField () {
		TypeBuilder tb = module.DefineType (genTypeName ());

		// Check invalid arguments
		try {
			tb.DefineField (null, typeof (int), 0);
			Fail ();
		}
		catch (ArgumentNullException) {
		}

		try {
			tb.DefineField ("", typeof (int), 0);
			Fail ();
		}
		catch (ArgumentException) {
		}

		try {
			// Strangely, 'A<NULL>' is accepted...
			string name = String.Format ("{0}", (char)0);
			tb.DefineField (name, typeof (int), 0);
			Fail ("Names with embedded nulls should be rejected");
		}
		catch (ArgumentException) {
		}

		try {
			tb.DefineField ("A", typeof (void), 0);
			Fail ();
		}
		catch (ArgumentException) {
		}

		tb.CreateType ();
		// Can not be called on a created type
		try {
			tb.DefineField ("B", typeof (int), 0);
			Fail ();
		}
		catch (InvalidOperationException) {
		}
	}

	[Test]
	public void TestDefineInitializedData () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		
		// Check invalid arguments
		try {
			tb.DefineInitializedData (null, new byte[1], 0);
			Fail ();
		}
		catch (ArgumentNullException) {
		}

		try {
			tb.DefineInitializedData ("FOO", null, 0);
			Fail ();
		}
		catch (ArgumentNullException) {
		}

		try {
			tb.DefineInitializedData ("", new byte[1], 0);
			Fail ();
		}
		catch (ArgumentException) {
		}

		// The size of the data is less than or equal to zero ???
		try {
			tb.DefineInitializedData ("BAR", new byte[0], 0);
			Fail ();
		}
		catch (ArgumentException) {
		}

		try {
			string name = String.Format ("{0}", (char)0);
			tb.DefineInitializedData (name, new byte[1], 0);
			Fail ("Names with embedded nulls should be rejected");
		}
		catch (ArgumentException) {
		}

		tb.CreateType ();

		// Can not be called on a created type, altough the MSDN docs does not mention this
		try {
			tb.DefineInitializedData ("BAR2", new byte[1], 0);
			Fail ();
		}
		catch (InvalidOperationException) {
		}
	}

	[Test]
	public void DefineUninitializedDataInvalidArgs () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		
		try {
			tb.DefineUninitializedData (null, 1, 0);
			Fail ();
		}
		catch (ArgumentNullException) {
		}

		try {
			tb.DefineUninitializedData ("", 1, 0);
			Fail ();
		}
		catch (ArgumentException) {
		}

		// The size of the data is less than or equal to zero ???
		try {
			tb.DefineUninitializedData ("BAR", 0, 0);
			Fail ();
		}
		catch (ArgumentException) {
		}

		try {
			string name = String.Format ("{0}", (char)0);
			tb.DefineUninitializedData (name, 1, 0);
			Fail ("Names with embedded nulls should be rejected");
		}
		catch (ArgumentException) {
		}
	}

	[Test]
	[ExpectedException (typeof (InvalidOperationException))]
	public void DefineUninitializedDataAlreadyCreated () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		tb.CreateType ();

		tb.DefineUninitializedData ("BAR2", 1, 0);
	}

	[Test]
	public void DefineUninitializedData () {
		TypeBuilder tb = module.DefineType (genTypeName ());

		tb.DefineUninitializedData ("foo", 4, FieldAttributes.Public);

		Type t = tb.CreateType ();

		object o = Activator.CreateInstance (t);

		FieldInfo fi = t.GetField ("foo");

		object fieldVal = fi.GetValue (o);

		IntPtr ptr = Marshal.AllocHGlobal (4);
		Marshal.StructureToPtr (fieldVal, ptr, true);
		Marshal.FreeHGlobal (ptr);
	}

	[Test]
	public void TestDefineMethod () {
		TypeBuilder tb = module.DefineType (genTypeName ());

		// Check invalid arguments
		try {
			tb.DefineMethod (null, 0, null, null);
			Fail ();
		}
		catch (ArgumentNullException) {
		}

		try {
			tb.DefineMethod ("", 0, null, null);
			Fail ();
		}
		catch (ArgumentException) {
		}

		// Check non-virtual methods on an interface
		TypeBuilder tb2 = module.DefineType (genTypeName (), TypeAttributes.Interface | TypeAttributes.Abstract);
		try {
			tb2.DefineMethod ("FOO", MethodAttributes.Abstract, null, null);
			Fail ();
		}
		catch (ArgumentException) {
		}

		tb.CreateType ();
		// Can not be called on a created type
		try {
			tb.DefineMethod ("bar", 0, null, null);
			Fail ();
		}
		catch (InvalidOperationException) {
		}
	}

	// TODO: DefineMethodOverride

	[Test]
	public void TestDefineNestedType () {
		TypeBuilder tb = module.DefineType (genTypeName ());

		// Check invalid arguments
		try {
			tb.DefineNestedType (null);
			Fail ("Should reject null name");
		}
		catch (ArgumentNullException) {
		}

		try {
			tb.DefineNestedType ("");
			Fail ("Should reject empty name");
		}
		catch (ArgumentException) {
		}

		try {
			tb.DefineNestedType (nullName ());
			Fail ("Should reject name with embedded 0s");
		}
		catch (ArgumentException) {
		}

		// If I fix the code so this works then mcs breaks -> how can mcs
		// works under MS .NET in the first place ???
		/*
		try {
			tb.DefineNestedType ("AA", TypeAttributes.Public, null, null);
			Fail ("Nested visibility must be specified.");
		}
		catch (ArgumentException) {
		}
		*/

		try {
			tb.DefineNestedType ("BB", TypeAttributes.NestedPublic, null,
								 new Type[1]);
			Fail ("Should reject empty interface");
		}
		catch (ArgumentException) {
		}

		// I think this should reject non-interfaces, but it does not
		tb.DefineNestedType ("BB", TypeAttributes.NestedPublic, null,
							 new Type[1] { typeof (object) });

		// Normal invocation
		tb.DefineNestedType ("Nest");

		tb.CreateType ();

		// According to the MSDN docs, this cannnot be called after the type
		// is created, but it works.
		tb.DefineNestedType ("Nest2");

		// According to the MSDN docs, a Sealed class can't contain nested 
		// types, but this is not true
		TypeBuilder tb2 = module.DefineType (genTypeName (), TypeAttributes.Sealed);
		tb2.DefineNestedType ("AA");

		// According to the MSDN docs, interfaces can only contain interfaces,
		// but this is not true
		TypeBuilder tb3 = module.DefineType (genTypeName (), TypeAttributes.Interface | TypeAttributes.Abstract);

		tb3.DefineNestedType ("AA");

		// Check shorter versions
		{
			TypeBuilder nested = tb.DefineNestedType ("N1");

			AssertEquals (nested.Name, "N1");
			AssertEquals (nested.BaseType, typeof (object));
			AssertEquals (nested.Attributes, TypeAttributes.NestedPrivate);
			AssertEquals (nested.GetInterfaces ().Length, 0);
		}

		// TODO:
	}

	[Test]
	public void TestDefinePInvokeMethod () {
		TypeBuilder tb = module.DefineType (genTypeName ());

		tb.DefinePInvokeMethod ("A", "B", "C", 0, 0, null, null, 0, 0);

		// Try invalid parameters
		try {
			tb.DefinePInvokeMethod (null, "B", "C", 0, 0, null, null, 0, 0);
			Fail ();
		}
		catch (ArgumentNullException) {
		}
		// etc...

		// Try invalid attributes
		try {
			tb.DefinePInvokeMethod ("A2", "B", "C", MethodAttributes.Abstract, 0, null, null, 0, 0);
		}
		catch (ArgumentException) {
		}

		// Try an interface parent
		TypeBuilder tb2 = module.DefineType (genTypeName (), TypeAttributes.Interface | TypeAttributes.Abstract);

		try {
			tb2.DefinePInvokeMethod ("A", "B", "C", 0, 0, null, null, 0, 0);
		}
		catch (ArgumentException) {
		}
	}

	[Test]
	public void TestDefineProperty () {
		TypeBuilder tb = module.DefineType (genTypeName ());

		// Check null parameter types
		try {
			tb.DefineProperty ("A", 0, null, new Type[1]);
		}
		catch (ArgumentNullException) {
		}
	}

	[Test]
	[ExpectedException (typeof(NotSupportedException))]
	public void TestIsDefinedIncomplete () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		tb.IsDefined (typeof (int), true);
	}

	[Test]
	public void TestIsDefinedComplete () {
		TypeBuilder tb = module.DefineType (genTypeName ());

		ConstructorInfo obsoleteCtor = typeof(ObsoleteAttribute).GetConstructor(
			new Type[] {typeof(string)});

		CustomAttributeBuilder caBuilder = new CustomAttributeBuilder (obsoleteCtor,
			new object[] { "obsolete message" }, new FieldInfo[0], new object[0]);

		tb.SetCustomAttribute (caBuilder);
		tb.CreateType ();
		AssertEquals (true, tb.IsDefined (typeof(ObsoleteAttribute), false));
	}

	[Test]
	[ExpectedException (typeof(NotSupportedException))]
	public void TestGetCustomAttributesIncomplete ()
	{
		TypeBuilder tb = module.DefineType (genTypeName ());
		tb.GetCustomAttributes (false);
	}

	[Test]
	public void TestGetCustomAttributesComplete ()
	{
		TypeBuilder tb = module.DefineType (genTypeName ());

		ConstructorInfo guidCtor = typeof(GuidAttribute).GetConstructor (
			new Type[] { typeof(string) });

		CustomAttributeBuilder caBuilder = new CustomAttributeBuilder (guidCtor,
			new object[] { Guid.NewGuid ().ToString ("D") }, new FieldInfo[0], new object[0]);

		tb.SetCustomAttribute (caBuilder);
		tb.CreateType ();

		AssertEquals (1, tb.GetCustomAttributes (false).Length);
	}

	[Test]
	[ExpectedException (typeof(NotSupportedException))]
	public void TestGetCustomAttributesOfTypeIncomplete ()
	{
		TypeBuilder tb = module.DefineType (genTypeName ());
		tb.GetCustomAttributes (typeof(ObsoleteAttribute), false);
	}

	[Test]
	public void TestGetCustomAttributesOfTypeComplete ()
	{
		TypeBuilder tb = module.DefineType (genTypeName ());

		ConstructorInfo guidCtor = typeof(GuidAttribute).GetConstructor (
			new Type[] { typeof(string) });

		CustomAttributeBuilder caBuilder = new CustomAttributeBuilder (guidCtor,
			new object[] { Guid.NewGuid ().ToString ("D") }, new FieldInfo[0], new object[0]);

		tb.SetCustomAttribute (caBuilder);
		tb.CreateType ();

		AssertEquals (1, tb.GetCustomAttributes (typeof(GuidAttribute), false).Length);
		AssertEquals (0, tb.GetCustomAttributes (typeof(ObsoleteAttribute), false).Length);
	}

	[Test]
	[ExpectedException (typeof(ArgumentNullException))]
	public void TestGetCustomAttributesOfNullTypeComplete ()
	{
		TypeBuilder tb = module.DefineType (genTypeName ());
		tb.CreateType ();
		tb.GetCustomAttributes (null, false);
	}

	[Test]
	[ExpectedException (typeof(NotSupportedException))]
	[Ignore("mcs depends on this")]
	public void TestGetEventsIncomplete () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		tb.GetEvents ();
	}

	[Test]
	public void TestGetEventsComplete () {
		TypeBuilder tb = module.DefineType (genTypeName ());

		MethodBuilder onclickMethod = tb.DefineMethod ("OnChange", MethodAttributes.Public, 
			typeof(void), new Type[] { typeof(Object) });
		onclickMethod.GetILGenerator ().Emit (OpCodes.Ret);

		// create public event
		EventBuilder eventbuilder = tb.DefineEvent ("Change", EventAttributes.None,
			typeof(ResolveEventHandler));
		eventbuilder.SetRaiseMethod (onclickMethod);

		Type emittedType = tb.CreateType ();

		AssertEquals (1, tb.GetEvents ().Length);
		AssertEquals (tb.GetEvents ().Length, emittedType.GetEvents ().Length);
	}


	[Test]
	[ExpectedException (typeof(NotSupportedException))]
	[Ignore("mcs depends on this")]
	public void TestGetEventsFlagsIncomplete () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		tb.GetEvents (BindingFlags.Public);
	}

	[Test]
	public void TestGetEventsFlagsComplete () {
		TypeBuilder tb = module.DefineType (genTypeName ());

		MethodBuilder onchangeMethod = tb.DefineMethod ("OnChange", MethodAttributes.Public,
			typeof(void), new Type[] { typeof(Object) });
		onchangeMethod.GetILGenerator ().Emit (OpCodes.Ret);

		// create public event
		EventBuilder changeEvent = tb.DefineEvent ("Change", EventAttributes.None,
			typeof(ResolveEventHandler));
		changeEvent.SetRaiseMethod (onchangeMethod);

		// create non-public event
		EventBuilder redoChangeEvent = tb.DefineEvent ("RedoChange", EventAttributes.None,
			typeof(ResolveEventHandler));

		Type emittedType = tb.CreateType ();

		AssertEquals (1, tb.GetEvents (BindingFlags.Instance | BindingFlags.Public).Length);
		AssertEquals (1, tb.GetEvents (BindingFlags.Instance | BindingFlags.NonPublic).Length);
		AssertEquals (2, tb.GetEvents (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Length);
		AssertEquals (tb.GetEvents (BindingFlags.Instance | BindingFlags.Public).Length,
			emittedType.GetEvents (BindingFlags.Instance | BindingFlags.Public).Length);
		AssertEquals (tb.GetEvents (BindingFlags.Instance | BindingFlags.NonPublic).Length,
			emittedType.GetEvents (BindingFlags.Instance | BindingFlags.NonPublic).Length);
		AssertEquals (tb.GetEvents (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Length,
			emittedType.GetEvents (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Length);
	}

	[Test]
	[ExpectedException (typeof(NotSupportedException))]
	[Ignore("mcs depends on this")]
	public void TestGetEventIncomplete () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		tb.GetEvent ("FOO");
	}

	[Test]
	public void TestGetEventComplete () {
		TypeBuilder tb = module.DefineType (genTypeName ());

		MethodBuilder onclickMethod = tb.DefineMethod ("OnChange", MethodAttributes.Public,
			typeof(void), new Type[] { typeof(Object) });
		onclickMethod.GetILGenerator ().Emit (OpCodes.Ret);

		EventBuilder eventbuilder = tb.DefineEvent ("Change", EventAttributes.None,
			typeof(ResolveEventHandler));
		eventbuilder.SetRaiseMethod (onclickMethod);

		Type emittedType = tb.CreateType ();

		AssertNotNull (tb.GetEvent ("Change"));
		AssertEquals (tb.GetEvent ("Change"), emittedType.GetEvent ("Change"));
		AssertNull (tb.GetEvent ("NotChange"));
		AssertEquals (tb.GetEvent ("NotChange"), emittedType.GetEvent ("NotChange"));
	}

	[Test]
	[ExpectedException (typeof(NotSupportedException))]
	[Ignore("mcs depends on this")]
	public void TestGetEventFlagsIncomplete () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		tb.GetEvent ("FOO", BindingFlags.Public);
	}

	[Test]
	public void TestGetEventFlagsComplete () {
		TypeBuilder tb = module.DefineType (genTypeName ());

		MethodBuilder onclickMethod = tb.DefineMethod ("OnChange", MethodAttributes.Public,
			typeof(void), new Type[] { typeof(Object) });
		onclickMethod.GetILGenerator ().Emit (OpCodes.Ret);

		EventBuilder eventbuilder = tb.DefineEvent ("Change", EventAttributes.None,
			typeof(ResolveEventHandler));
		eventbuilder.SetRaiseMethod (onclickMethod);

		Type emittedType = tb.CreateType ();

		AssertNotNull (tb.GetEvent ("Change", BindingFlags.Instance | BindingFlags.Public));
		AssertEquals (tb.GetEvent ("Change", BindingFlags.Instance | BindingFlags.Public),
			emittedType.GetEvent ("Change", BindingFlags.Instance | BindingFlags.Public));
		AssertNull (tb.GetEvent ("Change", BindingFlags.Instance | BindingFlags.NonPublic));
		AssertEquals (tb.GetEvent ("Change", BindingFlags.Instance | BindingFlags.NonPublic),
			emittedType.GetEvent ("Change", BindingFlags.Instance | BindingFlags.NonPublic));
	}

	[Test]
	[ExpectedException (typeof(NotSupportedException))]
	[Ignore("mcs depends on this")]
	public void TestGetFieldsIncomplete () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		tb.GetFields ();
	}

	[Test]
	public void TestGetFieldsComplete () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		tb.DefineField ("TestField", typeof(int), FieldAttributes.Public);

		Type emittedType = tb.CreateType ();

		AssertEquals (1, tb.GetFields ().Length);
		AssertEquals (tb.GetFields ().Length, emittedType.GetFields().Length);
	}

	[Test]
	[ExpectedException (typeof(NotSupportedException))]
	[Ignore("mcs depends on this")]
	public void TestGetFieldsFlagsIncomplete () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		tb.GetFields (BindingFlags.Instance | BindingFlags.Public);
	}

	[Test]
	public void TestGetFieldsFlagsComplete () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		tb.DefineField ("TestField", typeof(int), FieldAttributes.Public);

		Type emittedType = tb.CreateType ();

		AssertEquals (1, tb.GetFields (BindingFlags.Instance | BindingFlags.Public).Length);
		AssertEquals (tb.GetFields (BindingFlags.Instance | BindingFlags.Public).Length, 
			emittedType.GetFields (BindingFlags.Instance | BindingFlags.Public).Length);
		AssertEquals (0, tb.GetFields (BindingFlags.Instance | BindingFlags.NonPublic).Length);
		AssertEquals (tb.GetFields (BindingFlags.Instance | BindingFlags.NonPublic).Length,
			emittedType.GetFields (BindingFlags.Instance | BindingFlags.NonPublic).Length);
	}

	[Test]
	[ExpectedException (typeof(NotSupportedException))]
	[Ignore("mcs depends on this")]
	public void TestGetFieldIncomplete () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		tb.GetField ("test");
	}

	[Test]
	public void TestGetFieldComplete () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		tb.DefineField ("TestField", typeof(int), FieldAttributes.Public);

		Type emittedType = tb.CreateType ();

		AssertNotNull (tb.GetField ("TestField"));
		AssertEquals (tb.GetField ("TestField").Name, emittedType.GetField ("TestField").Name);
		AssertNull (tb.GetField ("TestOtherField"));
		AssertEquals (tb.GetField ("TestOtherField").Name, 
			emittedType.GetField ("TestOtherField").Name);
	}

	[Test]
	[ExpectedException (typeof(NotSupportedException))]
	[Ignore("mcs depends on this")]
	public void TestGetFieldFlagsIncomplete () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		tb.GetField ("test", BindingFlags.Public);
	}

	[Test]
	public void TestGetFieldFlagsComplete () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		tb.DefineField ("TestField", typeof(int), FieldAttributes.Public);

		Type emittedType = tb.CreateType ();

		AssertNotNull (tb.GetField ("TestField", BindingFlags.Instance | BindingFlags.Public));
		AssertEquals (tb.GetField ("TestField", BindingFlags.Instance | BindingFlags.Public).Name,
			emittedType.GetField ("TestField", BindingFlags.Instance | BindingFlags.Public).Name);
		AssertNull (tb.GetField ("TestField", BindingFlags.Instance | BindingFlags.NonPublic));
		AssertEquals (tb.GetField ("TestField", BindingFlags.Instance | BindingFlags.NonPublic),
			emittedType.GetField ("TestField", BindingFlags.Instance | BindingFlags.NonPublic));
	}

	[Test]
	[ExpectedException (typeof(NotSupportedException))]
	[Ignore("mcs depends on this")]
	public void TestGetPropertiesIncomplete () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		tb.GetProperties ();
	}

	[Test]
	public void TestGetPropertiesComplete () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		DefineStringProperty (tb, "CustomerName", "customerName", MethodAttributes.Public);

		Type emittedType = tb.CreateType ();

		AssertEquals (1, tb.GetProperties ().Length);
		AssertEquals (tb.GetProperties ().Length, emittedType.GetProperties ().Length);
	}

	[Test]
	[ExpectedException (typeof(NotSupportedException))]
	[Ignore("mcs depends on this")]
	public void TestGetPropertiesFlagsIncomplete () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		tb.GetProperties (BindingFlags.Public);
	}

	[Test]
	public void TestGetPropertiesFlagsComplete () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		DefineStringProperty (tb, "CustomerName", "customerName", MethodAttributes.Public);

		Type emittedType = tb.CreateType ();

		AssertEquals (1, tb.GetProperties (BindingFlags.Instance | BindingFlags.Public).Length);
		AssertEquals (tb.GetProperties (BindingFlags.Instance | BindingFlags.Public).Length,
			emittedType.GetProperties (BindingFlags.Instance | BindingFlags.Public).Length);
		AssertEquals (0, tb.GetProperties (BindingFlags.Instance | BindingFlags.NonPublic).Length);
		AssertEquals (tb.GetProperties (BindingFlags.Instance | BindingFlags.NonPublic).Length,
			emittedType.GetProperties (BindingFlags.Instance | BindingFlags.NonPublic).Length);
	}

	[Test]
	[ExpectedException (typeof(NotSupportedException))]
	public void TestGetPropertyIncomplete () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		tb.GetProperty ("test");
	}

	[Test]
	public void TestGetPropertyComplete () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		DefineStringProperty (tb, "CustomerName", "customerName", MethodAttributes.Public);

		Type emittedType = tb.CreateType ();

		AssertNotNull (emittedType.GetProperty ("CustomerName"));
		AssertNull (emittedType.GetProperty ("OtherCustomerName"));

		try {
			tb.GetProperty ("CustomerName");
			Fail ();
		} catch (NotSupportedException) {}
	}

	[Test]
	[ExpectedException (typeof(NotSupportedException))]
	public void TestGetPropertyFlagsIncomplete () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		tb.GetProperty ("test", BindingFlags.Public);
	}

	[Test]
	public void TestGetPropertyFlagsComplete () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		DefineStringProperty (tb, "CustomerName", "customerName", MethodAttributes.Public);

		Type emittedType = tb.CreateType ();

		AssertNotNull (emittedType.GetProperty ("CustomerName", BindingFlags.Instance | 
			BindingFlags.Public));
		AssertNull (emittedType.GetProperty ("CustomerName", BindingFlags.Instance |
			BindingFlags.NonPublic));

		try {
			tb.GetProperty ("CustomerName", BindingFlags.Instance | BindingFlags.Public);
			Fail ();
		}
		catch (NotSupportedException) { }
	}

	[Test]
	[ExpectedException (typeof(NotSupportedException))]
	[Ignore("mcs depends on this")]
	public void TestGetMethodsIncomplete () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		tb.GetMethods ();
	}

	[Test]
	public void TestGetMethodsComplete () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		MethodBuilder helloMethod = tb.DefineMethod ("HelloMethod", 
			MethodAttributes.Public, typeof(string), new Type[0]);
		ILGenerator helloMethodIL = helloMethod.GetILGenerator ();
		helloMethodIL.Emit (OpCodes.Ldstr, "Hi! ");
		helloMethodIL.Emit (OpCodes.Ldarg_1);
		MethodInfo infoMethod = typeof(string).GetMethod ("Concat", 
			new Type[] { typeof(string), typeof(string) });
		helloMethodIL.Emit (OpCodes.Call, infoMethod);
		helloMethodIL.Emit (OpCodes.Ret);

		Type emittedType = tb.CreateType ();

		AssertEquals (typeof(object).GetMethods (BindingFlags.Public | BindingFlags.Instance).Length + 1, 
			tb.GetMethods ().Length);
		AssertEquals (tb.GetMethods ().Length, emittedType.GetMethods ().Length);
	}

	[Test]
	[ExpectedException (typeof(NotSupportedException))]
	[Ignore("mcs depends on this")]
	public void TestGetMethodsFlagsIncomplete () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		tb.GetMethods (BindingFlags.Public);
	}

	[Test]
	public void TestGetMethodsFlagsComplete () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		MethodBuilder helloMethod = tb.DefineMethod ("HelloMethod",
			MethodAttributes.Public, typeof(string), new Type[0]);
		ILGenerator helloMethodIL = helloMethod.GetILGenerator ();
		helloMethodIL.Emit (OpCodes.Ldstr, "Hi! ");
		helloMethodIL.Emit (OpCodes.Ldarg_1);
		MethodInfo infoMethod = typeof(string).GetMethod ("Concat", 
			new Type[] { typeof(string), typeof(string) });
		helloMethodIL.Emit (OpCodes.Call, infoMethod);
		helloMethodIL.Emit (OpCodes.Ret);

		Type emittedType = tb.CreateType ();

		AssertEquals (1, tb.GetMethods (BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly).Length);
		AssertEquals (tb.GetMethods (BindingFlags.Instance | BindingFlags.Public).Length,
			emittedType.GetMethods (BindingFlags.Instance | BindingFlags.Public).Length);
		AssertEquals (0, tb.GetMethods (BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).Length);
		AssertEquals (tb.GetMethods (BindingFlags.Instance | BindingFlags.NonPublic).Length,
			emittedType.GetMethods (BindingFlags.Instance | BindingFlags.NonPublic).Length);
	}

	[Test]
	[ExpectedException (typeof(NotSupportedException))]
	public void TestGetMemberIncomplete () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		tb.GetMember ("FOO", MemberTypes.All, BindingFlags.Public);
	}

	[Test]
	public void TestGetMemberComplete () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		tb.DefineField ("FOO", typeof(int), FieldAttributes.Private);

		Type emittedType = tb.CreateType ();

		AssertEquals (1, tb.GetMember ("FOO", MemberTypes.Field, BindingFlags.Instance | BindingFlags.NonPublic).Length);
		AssertEquals (0, tb.GetMember ("FOO", MemberTypes.Field, BindingFlags.Instance | BindingFlags.Public).Length);
	}

	[Test]
	[ExpectedException (typeof(NotSupportedException))]
	public void TestGetMembersIncomplete () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		tb.GetMembers ();
	}

	[Test]
	public void TestGetMembersComplete () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		Type emittedType = tb.CreateType ();

		AssertEquals (tb.GetMembers ().Length, emittedType.GetMembers ().Length);
	}

	[Test]
	[ExpectedException (typeof(NotSupportedException))]
	public void TestGetMembersFlagsIncomplete () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		tb.GetMembers (BindingFlags.Public);
	}

	[Test]
	public void TestGetMembersFlagsComplete () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		tb.DefineField ("FOO", typeof(int), FieldAttributes.Public);

		Type emittedType = tb.CreateType ();

		Assert (tb.GetMembers (BindingFlags.Instance | BindingFlags.Public).Length != 0);
		AssertEquals (tb.GetMembers (BindingFlags.Instance | BindingFlags.Public).Length,
			emittedType.GetMembers (BindingFlags.Instance | BindingFlags.Public).Length);
		AssertEquals (tb.GetMembers (BindingFlags.Instance | BindingFlags.NonPublic).Length,
			emittedType.GetMembers (BindingFlags.Instance | BindingFlags.NonPublic).Length);
	}

	[Test]
	[ExpectedException (typeof(NotSupportedException))]
	public void TestGetInterfaceIncomplete () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		tb.GetInterface ("FOO", true);
	}

	[Test]
	public void TestGetInterfaces () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		Type[] interfaces = tb.GetInterfaces ();
		AssertEquals (0, interfaces.Length);

		TypeBuilder tbInterface = module.DefineType (genTypeName (), TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract);
		Type emittedInterface = tbInterface.CreateType ();

		tb = module.DefineType (genTypeName (), TypeAttributes.Public, typeof(object), new Type[] { emittedInterface });
		interfaces = tb.GetInterfaces ();
		AssertEquals (1, interfaces.Length);
	}

	[Test]
	[ExpectedException (typeof (InvalidOperationException))]
	public void TestAddDeclarativeSecurityAlreadyCreated () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		tb.CreateType ();

		PermissionSet set = new PermissionSet (PermissionState.Unrestricted);
		tb.AddDeclarativeSecurity (SecurityAction.Demand, set);
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void TestAddDeclarativeSecurityNullPermissionSet () {
		TypeBuilder tb = module.DefineType (genTypeName ());

		tb.AddDeclarativeSecurity (SecurityAction.Demand, null);
	}

	[Test]
	public void TestAddDeclarativeSecurityInvalidAction () {
		TypeBuilder tb = module.DefineType (genTypeName ());

		SecurityAction[] actions = new SecurityAction [] { 
			SecurityAction.RequestMinimum,
			SecurityAction.RequestOptional,
			SecurityAction.RequestRefuse };
		PermissionSet set = new PermissionSet (PermissionState.Unrestricted);

		foreach (SecurityAction action in actions) {
			try {
				tb.AddDeclarativeSecurity (action, set);
				Fail ();
			}
			catch (ArgumentException) {
			}
		}
	}

	[Test]
	[ExpectedException (typeof (InvalidOperationException))]
	public void TestAddDeclarativeSecurityDuplicateAction () {
		TypeBuilder tb = module.DefineType (genTypeName ());

		PermissionSet set = new PermissionSet (PermissionState.Unrestricted);
		tb.AddDeclarativeSecurity (SecurityAction.Demand, set);
		tb.AddDeclarativeSecurity (SecurityAction.Demand, set);
	}

	[Test]
	public void TestEnums () {
		TypeAttributes typeAttrs = TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed;            
		TypeBuilder enumToCreate = module.DefineType(genTypeName (), typeAttrs, 
													 typeof(Enum));
		enumToCreate.SetCustomAttribute (new CustomAttributeBuilder (typeof (FlagsAttribute).GetConstructors ()[0], new Type [0]));
		// add value__ field, see DefineEnum method of ModuleBuilder
		enumToCreate.DefineField("value__", typeof(Int32), 
			FieldAttributes.Public | FieldAttributes.SpecialName | FieldAttributes.RTSpecialName);

		// add enum entries
		FieldBuilder fb = enumToCreate.DefineField("A", enumToCreate, 
			FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.Literal);
		fb.SetConstant((Int32) 0);

		fb = enumToCreate.DefineField("B", enumToCreate, 
			FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.Literal);
		fb.SetConstant((Int32) 1);

		fb = enumToCreate.DefineField("C", enumToCreate, 
			FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.Literal);
		fb.SetConstant((Int32) 2);

		Type enumType = enumToCreate.CreateType();

		object enumVal = Enum.ToObject(enumType, (Int32) 3);

		AssertEquals ("B, C", enumVal.ToString ());
		AssertEquals (3, (Int32)enumVal);
	}

	[Test]
	public void DefineEnum () {
		TypeBuilder typeBuilder = module.DefineType (genTypeName (),
													 TypeAttributes.Public);
		EnumBuilder enumBuilder = module.DefineEnum (genTypeName (),
													 TypeAttributes.Public, typeof(int));
		typeBuilder.DefineField ("myField", enumBuilder, FieldAttributes.Private);
		enumBuilder.CreateType();
		typeBuilder.CreateType();
	}

	[Test]
	[ExpectedException(typeof(TypeLoadException))]
	public void DefineEnumThrowIfTypeBuilderCalledBeforeEnumBuilder () {
		TypeBuilder typeBuilder = module.DefineType (genTypeName (),
													 TypeAttributes.Public);
		EnumBuilder enumBuilder = module.DefineEnum (genTypeName (),
													 TypeAttributes.Public, typeof(int));
		typeBuilder.DefineField ("myField", enumBuilder, FieldAttributes.Private);
		typeBuilder.CreateType();
		enumBuilder.CreateType();
	}
	private void DefineStringProperty (TypeBuilder tb, string propertyName, string fieldName, MethodAttributes methodAttribs) {
		// define the field holding the property value
		FieldBuilder fieldBuilder = tb.DefineField (fieldName,
			typeof(string), FieldAttributes.Private);

		PropertyBuilder propertyBuilder = tb.DefineProperty (
			propertyName, PropertyAttributes.HasDefault, typeof(string),
			new Type[] { typeof(string) });

		// First, we'll define the behavior of the "get" property for CustomerName as a method.
		MethodBuilder getMethodBuilder = tb.DefineMethod ("Get" + propertyName,
								methodAttribs,
								typeof(string),
								new Type[] { });

		ILGenerator getIL = getMethodBuilder.GetILGenerator ();

		getIL.Emit (OpCodes.Ldarg_0);
		getIL.Emit (OpCodes.Ldfld, fieldBuilder);
		getIL.Emit (OpCodes.Ret);

		// Now, we'll define the behavior of the "set" property for CustomerName.
		MethodBuilder setMethodBuilder = tb.DefineMethod ("Set" + propertyName,
								methodAttribs,
								null,
								new Type[] { typeof(string) });

		ILGenerator setIL = setMethodBuilder.GetILGenerator ();

		setIL.Emit (OpCodes.Ldarg_0);
		setIL.Emit (OpCodes.Ldarg_1);
		setIL.Emit (OpCodes.Stfld, fieldBuilder);
		setIL.Emit (OpCodes.Ret);

		// Last, we must map the two methods created above to our PropertyBuilder to 
		// their corresponding behaviors, "get" and "set" respectively. 
		propertyBuilder.SetGetMethod (getMethodBuilder);
		propertyBuilder.SetSetMethod (setMethodBuilder);
	}

	static int handler_called = 0;

	[Test]
	public void TestTypeResolve () {
		string typeName = genTypeName ();

		ResolveEventHandler handler = new ResolveEventHandler (TypeResolve);
        AppDomain.CurrentDomain.TypeResolve += handler;
		handler_called = 0;
		Type t = Type.GetType (typeName);
		AssertEquals (typeName, t.Name);
		AssertEquals (1, handler_called);
        AppDomain.CurrentDomain.TypeResolve -= handler;
	}
    
    Assembly TypeResolve (object sender, ResolveEventArgs args) {
		TypeBuilder tb = module.DefineType (args.Name, TypeAttributes.Public);
		tb.CreateType ();
		handler_called ++;
		return tb.Assembly;
	}
}
}
